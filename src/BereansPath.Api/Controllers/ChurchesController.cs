using System.Text.Json;
using BereansPath.Api.Data;
using BereansPath.Api.Dtos;
using BereansPath.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BereansPath.Api.Controllers;

[ApiController]
[Route("api/churches")]
public class ChurchesController(AppDbContext db, IHttpClientFactory httpClientFactory, ILogger<ChurchesController> logger)
    : ControllerBase
{
    private const string OverpassUrl = "https://overpass-api.de/api/interpreter";

    /// <summary>
    /// Finds nearby Christian places of worship via Overpass (OpenStreetMap).
    /// </summary>
    [HttpGet("nearby")]
    public async Task<IActionResult> FindNearby(
        [FromQuery] double lat,
        [FromQuery] double lon,
        [FromQuery] int radiusMeters = 50000,
        CancellationToken cancellationToken = default)
    {
        if (lat is < -90 or > 90 || lon is < -180 or > 180)
        {
            return BadRequest(new { error = "Invalid latitude/longitude." });
        }

        radiusMeters = Math.Clamp(radiusMeters, 1000, 50000);

        var query = $"""
            [out:json][timeout:25];
            (
              node["amenity"="place_of_worship"]["religion"="christian"](around:{radiusMeters},{lat},{lon});
              way["amenity"="place_of_worship"]["religion"="christian"](around:{radiusMeters},{lat},{lon});
              relation["amenity"="place_of_worship"]["religion"="christian"](around:{radiusMeters},{lat},{lon});
            );
            out center;
            """;

        var client = httpClientFactory.CreateClient();
        using var content = new FormUrlEncodedContent(new Dictionary<string, string> { ["data"] = query });

        try
        {
            using var response = await client.PostAsync(OverpassUrl, content, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Overpass returned {Status}: {Body}", response.StatusCode, body);
                return StatusCode(StatusCodes.Status502BadGateway, new { error = "Church search provider failed." });
            }

            using var doc = JsonDocument.Parse(body);
            var results = new List<object>();

            if (doc.RootElement.TryGetProperty("elements", out var elements))
            {
                foreach (var element in elements.EnumerateArray())
                {
                    double? churchLat = null;
                    double? churchLon = null;

                    if (element.TryGetProperty("lat", out var latProp) && element.TryGetProperty("lon", out var lonProp))
                    {
                        churchLat = latProp.GetDouble();
                        churchLon = lonProp.GetDouble();
                    }
                    else if (element.TryGetProperty("center", out var center))
                    {
                        churchLat = center.GetProperty("lat").GetDouble();
                        churchLon = center.GetProperty("lon").GetDouble();
                    }

                    if (churchLat is null || churchLon is null) continue;

                    var name = "Church";
                    if (element.TryGetProperty("tags", out var tags) &&
                        tags.TryGetProperty("name", out var nameProp))
                    {
                        name = nameProp.GetString() ?? name;
                    }

                    var type = element.GetProperty("type").GetString();
                    var id = element.GetProperty("id").GetInt64();

                    results.Add(new
                    {
                        name,
                        latitude = churchLat,
                        longitude = churchLon,
                        osmId = $"{type}/{id}"
                    });
                }
            }

            return Ok(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Nearby church search failed");
            return StatusCode(StatusCodes.Status502BadGateway, new { error = "Failed to search for churches." });
        }
    }

    [HttpGet("saved")]
    public async Task<IActionResult> GetSaved(CancellationToken cancellationToken)
    {
        var items = await db.SavedChurches
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new { c.Id, c.Name, c.Latitude, c.Longitude, c.OsmId, c.CreatedAt })
            .ToListAsync(cancellationToken);
        return Ok(items);
    }

    [HttpPost("saved")]
    public async Task<IActionResult> Save([FromBody] CreateSavedChurchRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var church = new SavedChurch
        {
            Name = request.Name.Trim(),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            OsmId = request.OsmId,
            CreatedAt = DateTime.UtcNow
        };

        db.SavedChurches.Add(church);
        await db.SaveChangesAsync(cancellationToken);

        return Created($"/api/churches/saved/{church.Id}", new
        {
            church.Id,
            church.Name,
            church.Latitude,
            church.Longitude,
            church.OsmId,
            church.CreatedAt
        });
    }

    [HttpDelete("saved/{id:int}")]
    public async Task<IActionResult> DeleteSaved(int id, CancellationToken cancellationToken)
    {
        var church = await db.SavedChurches.FindAsync([id], cancellationToken);
        if (church is null) return NotFound();

        db.SavedChurches.Remove(church);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}

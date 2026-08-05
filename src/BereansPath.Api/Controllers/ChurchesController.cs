using BereansPath.Api.Data;
using BereansPath.Api.Dtos;
using BereansPath.Api.Models;
using BereansPath.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BereansPath.Api.Controllers;

[ApiController]
[Route("api/churches")]
public class ChurchesController(
    AppDbContext db,
    OverpassChurchSearch overpass,
    ILogger<ChurchesController> logger) : ControllerBase
{
    /// <summary>
    /// Proxies Overpass church search (POST + identifying User-Agent + mirror fallbacks).
    /// Direct browser calls to overpass-api.de often receive HTTP 406.
    /// </summary>
    [HttpGet("nearby")]
    public async Task<IActionResult> Nearby(
        [FromQuery] double lat,
        [FromQuery] double lon,
        [FromQuery] int radius = 16000,
        CancellationToken cancellationToken = default)
    {
        if (lat is < -90 or > 90 || lon is < -180 or > 180)
            return BadRequest(new { error = "Invalid coordinates." });

        try
        {
            var results = await overpass.SearchAsync(lat, lon, radius, cancellationToken);
            return Ok(results.Select(c => new
            {
                name = c.Name,
                latitude = c.Latitude,
                longitude = c.Longitude,
                osmId = c.OsmId,
                denomination = c.Denomination,
                tags = c.Tags,
            }));
        }
        catch (OverpassUnavailableException ex)
        {
            logger.LogError(ex, "All Overpass endpoints failed");
            return StatusCode(StatusCodes.Status502BadGateway, new
            {
                error = ex.Message,
            });
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

    [HttpGet("suggestions")]
    public async Task<IActionResult> ListSuggestions(CancellationToken cancellationToken)
    {
        var items = await db.ChurchSuggestions
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new
            {
                s.Id,
                s.Name,
                s.City,
                s.Website,
                s.Denomination,
                s.Reason,
                s.CreatedAt,
            })
            .ToListAsync(cancellationToken);
        return Ok(items);
    }

    [HttpPost("suggestions")]
    public async Task<IActionResult> Suggest([FromBody] CreateChurchSuggestionRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var suggestion = new ChurchSuggestion
        {
            Name = request.Name.Trim(),
            City = string.IsNullOrWhiteSpace(request.City) ? null : request.City.Trim(),
            Website = string.IsNullOrWhiteSpace(request.Website) ? null : request.Website.Trim(),
            Denomination = string.IsNullOrWhiteSpace(request.Denomination) ? null : request.Denomination.Trim(),
            Reason = request.Reason.Trim(),
            ContactEmail = string.IsNullOrWhiteSpace(request.ContactEmail) ? null : request.ContactEmail.Trim(),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            CreatedAt = DateTime.UtcNow,
        };

        db.ChurchSuggestions.Add(suggestion);
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Church suggestion received: {Name} ({City})", suggestion.Name, suggestion.City);

        return Created($"/api/churches/suggestions/{suggestion.Id}", new
        {
            suggestion.Id,
            suggestion.Name,
            suggestion.City,
            suggestion.Website,
            suggestion.Denomination,
            suggestion.Reason,
            suggestion.CreatedAt,
        });
    }
}

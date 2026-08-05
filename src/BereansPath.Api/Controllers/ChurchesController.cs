using BereansPath.Api.Data;
using BereansPath.Api.Dtos;
using BereansPath.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BereansPath.Api.Controllers;

[ApiController]
[Route("api/churches")]
public class ChurchesController(AppDbContext db) : ControllerBase
{
    // Nearby search uses public Overpass from the browser (same as the Flask app).
    // This controller only persists saved churches.

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

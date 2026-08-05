using BereansPath.Api.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BereansPath.Api.Controllers;

[ApiController]
[Route("api/diagnostics")]
public class DiagnosticsController(AppLogStore logStore, IConfiguration configuration, IHostEnvironment env) : ControllerBase
{
    [HttpGet("health")]
    public IActionResult Health()
    {
        var hasEsvKey = !string.IsNullOrWhiteSpace(configuration["ESV_API_KEY"]);
        return Ok(new
        {
            status = "ok",
            environment = env.EnvironmentName,
            esvApiKeyConfigured = hasEsvKey,
            databaseProvider = configuration["DatabaseProvider"] ?? "Sqlite",
            logFile = logStore.LogFilePath,
            timeUtc = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Recent application log lines (in-memory buffer + same content written to logs/bereans-api.log).
    /// </summary>
    [HttpGet("logs")]
    public IActionResult GetLogs([FromQuery] int take = 200)
    {
        var lines = logStore.GetRecent(take);
        return Ok(new
        {
            count = lines.Count,
            logFile = logStore.LogFilePath,
            lines
        });
    }

    [HttpDelete("logs")]
    public IActionResult ClearLogs()
    {
        if (!env.IsDevelopment())
        {
            return NotFound();
        }

        logStore.ClearBuffer();
        return NoContent();
    }
}

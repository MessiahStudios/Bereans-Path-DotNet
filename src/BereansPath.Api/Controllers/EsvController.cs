using Microsoft.AspNetCore.Mvc;

namespace BereansPath.Api.Controllers;

[ApiController]
[Route("api/esv")]
public class EsvController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<EsvController> logger)
    : ControllerBase
{
    private const string EsvApiUrl = "https://api.esv.org/v3/passage/text/";

    /// <summary>
    /// Proxies the ESV Bible API so the key never reaches the browser.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPassage([FromQuery] string q, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest(new { error = "Query parameter 'q' is required (e.g. John 3:16)." });
        }

        var apiKey = configuration["ESV_API_KEY"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            logger.LogError("ESV request for {Query} failed: ESV_API_KEY is not configured", q);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = "ESV_API_KEY is not configured. Set it via user secrets or environment variables."
            });
        }

        logger.LogInformation("Proxying ESV passage request for {Query}", q);

        var client = httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, EsvApiUrl);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Token", apiKey);

        var query = HttpContext.Request.Query
            .Where(kv => !string.IsNullOrEmpty(kv.Key))
            .SelectMany(kv => kv.Value.Select(v => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(v ?? string.Empty)}"));
        request.RequestUri = new Uri($"{EsvApiUrl}?{string.Join("&", query)}");

        try
        {
            using var response = await client.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            return new ContentResult
            {
                Content = body,
                ContentType = "application/json",
                StatusCode = (int)response.StatusCode
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ESV proxy failed for query {Query}", q);
            return StatusCode(StatusCodes.Status502BadGateway, new { error = "Failed to reach ESV API." });
        }
    }
}

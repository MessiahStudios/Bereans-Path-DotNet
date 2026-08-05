using System.Net;
using System.Text.Json;

namespace BereansPath.Api.Services;

public class OverpassChurchSearch(IHttpClientFactory httpClientFactory, ILogger<OverpassChurchSearch> logger)
{
    // Prefer mirrors that often stay up when the FOSSGIS pair is overloaded.
    private static readonly string[] Endpoints =
    [
        "https://overpass.kumi.systems/api/interpreter",
        "https://overpass.private.coffee/api/interpreter",
        "https://z.overpass-api.de/api/interpreter",
        "https://overpass-api.de/api/interpreter",
        "https://lz4.overpass-api.de/api/interpreter",
    ];

    public async Task<IReadOnlyList<OverpassChurch>> SearchAsync(
        double lat,
        double lon,
        int radiusMeters,
        CancellationToken cancellationToken)
    {
        var clamped = Math.Clamp(radiusMeters, 1000, 50000);
        // Keep the query light: nodes + ways only (relations are rare for local churches).
        var query = $"""
            [out:json][timeout:60];
            (
              node["amenity"="place_of_worship"]["religion"="christian"](around:{clamped},{lat},{lon});
              way["amenity"="place_of_worship"]["religion"="christian"](around:{clamped},{lat},{lon});
            );
            out center tags;
            """;

        var client = httpClientFactory.CreateClient("Overpass");
        Exception? lastError = null;

        foreach (var endpoint in Endpoints)
        {
            for (var attempt = 1; attempt <= 2; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    using var content = new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        ["data"] = query,
                    });

                    using var response = await client.PostAsync(endpoint, content, cancellationToken);
                    var body = await response.Content.ReadAsStringAsync(cancellationToken);

                    if (response.StatusCode is HttpStatusCode.GatewayTimeout
                        or HttpStatusCode.ServiceUnavailable
                        or HttpStatusCode.TooManyRequests
                        or (HttpStatusCode)504
                        or (HttpStatusCode)429)
                    {
                        logger.LogWarning(
                            "Overpass {Endpoint} attempt {Attempt} returned {Status}",
                            endpoint,
                            attempt,
                            (int)response.StatusCode);
                        await Task.Delay(TimeSpan.FromSeconds(attempt), cancellationToken);
                        continue;
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        logger.LogWarning(
                            "Overpass {Endpoint} returned {Status}: {Snippet}",
                            endpoint,
                            (int)response.StatusCode,
                            body.Length > 180 ? body[..180] : body);
                        break; // try next endpoint
                    }

                    var parsed = ParseChurches(body);
                    logger.LogInformation(
                        "Overpass {Endpoint} returned {Count} churches",
                        endpoint,
                        parsed.Count);
                    return parsed;
                }
                catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
                {
                    lastError = ex;
                    logger.LogWarning("Overpass {Endpoint} timed out (attempt {Attempt})", endpoint, attempt);
                    await Task.Delay(TimeSpan.FromSeconds(attempt), cancellationToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    lastError = ex;
                    logger.LogWarning(ex, "Overpass {Endpoint} failed (attempt {Attempt})", endpoint, attempt);
                    await Task.Delay(TimeSpan.FromSeconds(attempt), cancellationToken);
                }
            }
        }

        throw new OverpassUnavailableException("All Overpass endpoints failed.", lastError);
    }

    private static List<OverpassChurch> ParseChurches(string body)
    {
        using var doc = JsonDocument.Parse(body);
        var results = new List<OverpassChurch>();
        if (!doc.RootElement.TryGetProperty("elements", out var elements))
            return results;

        foreach (var element in elements.EnumerateArray())
        {
            double? latitude = null;
            double? longitude = null;

            if (element.TryGetProperty("lat", out var latEl) && element.TryGetProperty("lon", out var lonEl))
            {
                latitude = latEl.GetDouble();
                longitude = lonEl.GetDouble();
            }
            else if (element.TryGetProperty("center", out var center)
                     && center.TryGetProperty("lat", out var cLat)
                     && center.TryGetProperty("lon", out var cLon))
            {
                latitude = cLat.GetDouble();
                longitude = cLon.GetDouble();
            }

            if (latitude is null || longitude is null) continue;

            var type = element.GetProperty("type").GetString() ?? "node";
            var id = element.GetProperty("id").GetInt64();

            string name = "Church";
            string denomination = "";
            var tagMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (element.TryGetProperty("tags", out var tags) && tags.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in tags.EnumerateObject())
                    tagMap[prop.Name] = prop.Value.GetString() ?? "";

                if (tagMap.TryGetValue("name", out var n) && !string.IsNullOrWhiteSpace(n))
                    name = n;
                if (tagMap.TryGetValue("denomination", out var d))
                    denomination = d;
            }

            results.Add(new OverpassChurch(
                name,
                latitude.Value,
                longitude.Value,
                $"{type}/{id}",
                denomination,
                tagMap));
        }

        return results;
    }
}

public record OverpassChurch(
    string Name,
    double Latitude,
    double Longitude,
    string OsmId,
    string Denomination,
    Dictionary<string, string> Tags);

public class OverpassUnavailableException(string message, Exception? inner = null)
    : Exception(message, inner);

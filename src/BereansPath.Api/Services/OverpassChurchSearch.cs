using System.Net;
using System.Text.Json;

namespace BereansPath.Api.Services;

public class OverpassChurchSearch(IHttpClientFactory httpClientFactory, ILogger<OverpassChurchSearch> logger)
{
    // Prefer FOSSGIS first; community mirrors often hang for a full HttpClient timeout.
    private static readonly string[] Endpoints =
    [
        "https://z.overpass-api.de/api/interpreter",
        "https://lz4.overpass-api.de/api/interpreter",
        "https://overpass-api.de/api/interpreter",
        "https://overpass.kumi.systems/api/interpreter",
        "https://overpass.private.coffee/api/interpreter",
    ];

    private static string? _lastGoodEndpoint;
    private static readonly object LastGoodGate = new();

    public async Task<IReadOnlyList<OverpassChurch>> SearchAsync(
        double lat,
        double lon,
        int radiusMeters,
        CancellationToken cancellationToken)
    {
        var clamped = Math.Clamp(radiusMeters, 1000, 50000);
        var qlTimeoutSec = clamped <= 10000 ? 15 : clamped <= 20000 ? 18 : 22;
        var perAttempt = TimeSpan.FromSeconds(qlTimeoutSec + 4);

        var query = $"""
            [out:json][timeout:{qlTimeoutSec}];
            (
              node["amenity"="place_of_worship"]["religion"="christian"](around:{clamped},{lat},{lon});
              way["amenity"="place_of_worship"]["religion"="christian"](around:{clamped},{lat},{lon});
            );
            out center tags;
            """;

        // Two full passes — Overpass is bursty; a second pass often succeeds immediately.
        Exception? lastError = null;
        for (var pass = 1; pass <= 2; pass++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var budgetCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            budgetCts.CancelAfter(TimeSpan.FromSeconds(35));

            try
            {
                var result = await SearchOnceAsync(
                    query,
                    perAttempt,
                    budgetCts.Token,
                    cancellationToken);
                if (result is not null)
                    return result;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning("Overpass search pass {Pass} hit the time budget", pass);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                lastError = ex;
                logger.LogWarning(ex, "Overpass search pass {Pass} failed", pass);
            }

            if (pass < 2)
                await Task.Delay(800, cancellationToken);
        }

        throw new OverpassUnavailableException(
            "Church map servers are busy right now. Tap Find near me again in a moment.",
            lastError);
    }

    private async Task<IReadOnlyList<OverpassChurch>?> SearchOnceAsync(
        string query,
        TimeSpan perAttempt,
        CancellationToken budgetToken,
        CancellationToken requestToken)
    {
        var client = httpClientFactory.CreateClient("Overpass");
        foreach (var endpoint in OrderedEndpoints())
        {
            requestToken.ThrowIfCancellationRequested();
            if (budgetToken.IsCancellationRequested)
                return null;

            var parsed = await QueryEndpointAsync(client, endpoint, query, perAttempt, budgetToken);
            if (parsed is null)
                continue;

            lock (LastGoodGate)
                _lastGoodEndpoint = endpoint;
            return parsed;
        }

        return null;
    }

    private static IEnumerable<string> OrderedEndpoints()
    {
        string? lastGood;
        lock (LastGoodGate)
            lastGood = _lastGoodEndpoint;

        if (!string.IsNullOrEmpty(lastGood))
            yield return lastGood;

        foreach (var endpoint in Endpoints)
        {
            if (!string.Equals(endpoint, lastGood, StringComparison.Ordinal))
                yield return endpoint;
        }
    }

    private async Task<IReadOnlyList<OverpassChurch>?> QueryEndpointAsync(
        HttpClient client,
        string endpoint,
        string query,
        TimeSpan perAttempt,
        CancellationToken outerToken)
    {
        using var attemptCts = CancellationTokenSource.CreateLinkedTokenSource(outerToken);
        attemptCts.CancelAfter(perAttempt);
        var token = attemptCts.Token;

        try
        {
            using var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["data"] = query,
            });

            using var response = await client.PostAsync(endpoint, content, token);
            var body = await response.Content.ReadAsStringAsync(token);

            if (response.StatusCode is HttpStatusCode.GatewayTimeout
                or HttpStatusCode.ServiceUnavailable
                or HttpStatusCode.TooManyRequests
                or (HttpStatusCode)504
                or (HttpStatusCode)429)
            {
                logger.LogWarning(
                    "Overpass {Endpoint} returned {Status}",
                    endpoint,
                    (int)response.StatusCode);
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Overpass {Endpoint} returned {Status}: {Snippet}",
                    endpoint,
                    (int)response.StatusCode,
                    body.Length > 180 ? body[..180] : body);
                return null;
            }

            var parsed = ParseChurches(body);
            logger.LogInformation(
                "Overpass {Endpoint} returned {Count} churches",
                endpoint,
                parsed.Count);
            return parsed;
        }
        catch (OperationCanceledException) when (!outerToken.IsCancellationRequested)
        {
            logger.LogWarning("Overpass {Endpoint} timed out after {Seconds}s", endpoint, perAttempt.TotalSeconds);
            return null;
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Overpass {Endpoint} request failed", endpoint);
            return null;
        }
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
                else if (tagMap.TryGetValue("official_name", out var official) && !string.IsNullOrWhiteSpace(official))
                    name = official;
                else if (tagMap.TryGetValue("alt_name", out var alt) && !string.IsNullOrWhiteSpace(alt))
                    name = alt;
                else
                    name = "Church"; // OSM often omits name — UI explains this.

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

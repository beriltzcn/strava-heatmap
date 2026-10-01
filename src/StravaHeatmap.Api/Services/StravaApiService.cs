using System.Net.Http.Headers;
using System.Net.Http.Json;
using StravaHeatmap.Api.Dtos;

namespace StravaHeatmap.Api.Services;

// Strava's data endpoints (not the token endpoints).
public class StravaApiService
{
    private readonly HttpClient _httpClient;

    public StravaApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // Fetches the athlete's activities, one page at a time.
    // Strava returns at most 200 records per request.
    public async Task<List<StravaActivityDto>> GetActivitiesAsync(
        string accessToken, int page, int perPage, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/v3/athlete/activities?page={page}&per_page={perPage}");

        // Send the token in the Authorization header: "Bearer <token>"
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        try
        {
            using var response = await _httpClient.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                throw new StravaAuthException(
                    $"Strava activities request failed ({(int)response.StatusCode}): {body}");
            }

            var activities = await response.Content.ReadFromJsonAsync<List<StravaActivityDto>>(ct);

            return activities ?? new List<StravaActivityDto>();
        }
        catch (HttpRequestException ex)
        {
            // Network failure: no internet, DNS failure, certificate problem...
            throw new StravaAuthException($"Could not reach Strava: {ex.Message}");
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            // The request timed out (the app itself is not shutting down).
            throw new StravaAuthException("The Strava request timed out.");
        }
    }
}

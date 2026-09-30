using System.Net.Http.Headers;
using System.Net.Http.Json;
using StravaHeatmap.Api.Dtos;

namespace StravaHeatmap.Api.Services;

// Strava'nın veri uçları (token uçları değil).
public class StravaApiService
{
    private readonly HttpClient _httpClient;

    public StravaApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // Kullanıcının aktivitelerini sayfa sayfa çeker.
    // Strava tek istekte en fazla 200 kayıt verir.
    public async Task<List<StravaActivityDto>> GetActivitiesAsync(
        string accessToken, int page, int perPage, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/v3/athlete/activities?page={page}&per_page={perPage}");

        // Token'ı başlıkta gönderiyoruz: "Bearer <token>"
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            throw new StravaAuthException(
                $"Strava aktivite isteği başarısız ({(int)response.StatusCode}): {body}");
        }

        var activities = await response.Content.ReadFromJsonAsync<List<StravaActivityDto>>(ct);

        return activities ?? new List<StravaActivityDto>();
    }
}
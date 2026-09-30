using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using StravaHeatmap.Api.Options;

namespace StravaHeatmap.Api.Services;

// Strava'nın OAuth uçlarıyla konuşan servis.
public class StravaAuthService
{
    private readonly HttpClient _httpClient;
    private readonly StravaOptions _options;

    public StravaAuthService(HttpClient httpClient, IOptions<StravaOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    // Kullanıcıyı göndereceğimiz Strava adresini üretir.
    // state: CSRF koruması için bizim ürettiğimiz rastgele değer.
    public string BuildAuthorizationUrl(string state)
    {
        return "https://www.strava.com/oauth/authorize"
             + $"?client_id={_options.ClientId}"
             + $"&redirect_uri={Uri.EscapeDataString(_options.RedirectUri)}"
             + "&response_type=code"
             + "&approval_prompt=force"
             + "&scope=read,activity:read_all"
             + $"&state={state}";
    }

    // Tarayıcıdan gelen tek kullanımlık kodu kalıcı token'a çevirir.
    public async Task<StravaTokenResponse> ExchangeCodeAsync(string code, CancellationToken ct)
    {
        var form = new Dictionary<string, string>
        {
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["code"] = code,
            ["grant_type"] = "authorization_code"
        };

        return await PostTokenRequestAsync(form, ct);
    }

    // Erişim token'ının süresi dolduğunda yenisini alır.
    public async Task<StravaTokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken ct)
    {
        var form = new Dictionary<string, string>
        {
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["refresh_token"] = refreshToken,
            ["grant_type"] = "refresh_token"
        };

        return await PostTokenRequestAsync(form, ct);
    }

    // İki istek de aynı yere gittiği için ortak kısım burada.
    private async Task<StravaTokenResponse> PostTokenRequestAsync(
        Dictionary<string, string> form, CancellationToken ct)
    {
        try
        {
            using var response = await _httpClient.PostAsync(
                "/oauth/token", new FormUrlEncodedContent(form), ct);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                throw new StravaAuthException(
                    $"Strava token isteği başarısız ({(int)response.StatusCode}): {body}");
            }

            var token = await response.Content.ReadFromJsonAsync<StravaTokenResponse>(ct);

            return token ?? throw new StravaAuthException("Strava boş cevap döndü.");
        }
        catch (HttpRequestException ex)
        {
            // Ağ hatası: internet yok, DNS çözülemedi, sertifika sorunu...
            throw new StravaAuthException($"Strava'ya bağlanılamadı: {ex.Message}");
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            // İstek zaman aşımına uğradı (uygulama kapanıyor değil).
            throw new StravaAuthException("Strava isteği zaman aşımına uğradı.");
        }
    }
}

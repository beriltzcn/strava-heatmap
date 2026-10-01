using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using StravaHeatmap.Api.Options;

namespace StravaHeatmap.Api.Services;

// Talks to Strava's OAuth endpoints.
public class StravaAuthService
{
    private readonly HttpClient _httpClient;
    private readonly StravaOptions _options;

    public StravaAuthService(HttpClient httpClient, IOptions<StravaOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    // Builds the Strava URL the user is sent to.
    // state: a random value we generate for CSRF protection.
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

    // Exchanges the one-time code from the browser for a long-lived token.
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

    // Gets a new access token once the current one has expired.
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

    // Both requests go to the same endpoint, so the shared part lives here.
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
                    $"Strava token request failed ({(int)response.StatusCode}): {body}");
            }

            var token = await response.Content.ReadFromJsonAsync<StravaTokenResponse>(ct);

            return token ?? throw new StravaAuthException("Strava returned an empty response.");
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

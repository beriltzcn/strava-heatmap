using System.Text.Json.Serialization;

namespace StravaHeatmap.Api.Services;

// Strava'nın /oauth/token cevabının C# karşılığı.
public class StravaTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = "";

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = "";

    // Strava bunu Unix zaman damgası olarak gönderir (1 Ocak 1970'ten beri geçen saniye).
    [JsonPropertyName("expires_at")]
    public long ExpiresAtUnix { get; set; }

    [JsonPropertyName("athlete")]
    public StravaAthlete? Athlete { get; set; }

    // Hesaplanmış hâli: okunabilir tarihe çevirir.
    public DateTimeOffset ExpiresAt => DateTimeOffset.FromUnixTimeSeconds(ExpiresAtUnix);
}

public class StravaAthlete
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("firstname")]
    public string? FirstName { get; set; }

    [JsonPropertyName("lastname")]
    public string? LastName { get; set; }
}
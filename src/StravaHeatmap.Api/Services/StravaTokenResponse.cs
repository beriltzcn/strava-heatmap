using System.Text.Json.Serialization;

namespace StravaHeatmap.Api.Services;

// C# shape of Strava's /oauth/token response.
public class StravaTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = "";

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = "";

    // Strava sends this as a Unix timestamp (seconds since 1 January 1970).
    [JsonPropertyName("expires_at")]
    public long ExpiresAtUnix { get; set; }

    [JsonPropertyName("athlete")]
    public StravaAthlete? Athlete { get; set; }

    // Computed property: turns the Unix timestamp into a readable date.
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

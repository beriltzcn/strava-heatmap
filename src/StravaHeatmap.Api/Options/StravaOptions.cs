namespace StravaHeatmap.Api.Options;

// C# shape of the "Strava" section in appsettings.json.
public class StravaOptions
{
    public const string SectionName = "Strava";

    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
    public string RedirectUri { get; set; } = "";
}

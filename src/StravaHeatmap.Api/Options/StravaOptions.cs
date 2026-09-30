namespace StravaHeatmap.Api.Options;

// appsettings.json'daki "Strava" bölümünün C# karşılığı.
public class StravaOptions
{
    public const string SectionName = "Strava";

    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
    public string RedirectUri { get; set; } = "";
}
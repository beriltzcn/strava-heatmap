namespace StravaHeatmap.Api.Models;

// An activity fetched from Strava and stored locally.
public class Activity
{
    public int Id { get; set; }

    // Strava's own activity id. Unique, so the same activity is never stored twice.
    public long StravaActivityId { get; set; }

    // Which connection this activity belongs to.
    public int StravaConnectionId { get; set; }
    public StravaConnection? Connection { get; set; }

    public string? Name { get; set; }
    public string? SportType { get; set; }
    public DateTimeOffset StartDate { get; set; }

    // Raw values: meters and seconds. Converted to km/minutes when displayed.
    public double DistanceMeters { get; set; }
    public int MovingTimeSeconds { get; set; }
    public int ElapsedTimeSeconds { get; set; }
    public double TotalElevationGain { get; set; }
    public double AverageSpeed { get; set; }

    // The encoded route. Empty for indoor activities without GPS data.
    public string? SummaryPolyline { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
}

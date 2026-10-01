namespace StravaHeatmap.Api.Dtos;

public class ActivitySummaryDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? SportType { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public double DistanceKm { get; set; }
    public int MovingTimeMinutes { get; set; }
    public double ElevationGainMeters { get; set; }
    public bool HasRoute { get; set; }
    public string? Polyline { get; set; }
}

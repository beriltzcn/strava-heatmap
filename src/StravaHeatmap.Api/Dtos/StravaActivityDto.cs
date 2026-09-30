using System.Text.Json.Serialization;

namespace StravaHeatmap.Api.Dtos;

public class StravaActivityDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("sport_type")]
    public  string? SportType { get; set; }

    [JsonPropertyName("start_date")]
    public DateTimeOffset StartDate { get; set; }

    [JsonPropertyName("distance")]
    public double Distance { get; set; }

    [JsonPropertyName("moving_time")]
    public int MovingTime { get; set; }

    [JsonPropertyName("elapsed_time")]
    public int ElapsedTime { get; set; }

    [JsonPropertyName("total_elevation_gain")]
    public double TotalElevationGain { get; set; }

    [JsonPropertyName("average_speed")]
    public double AverageSpeed { get; set; }

    [JsonPropertyName("map")]
    public StravaActivityMap? Map { get; set; }


    public class StravaActivityMap
    {
        [JsonPropertyName("summary_polyline")]
        public string? SummaryPolyline { get; set; }
    }


}

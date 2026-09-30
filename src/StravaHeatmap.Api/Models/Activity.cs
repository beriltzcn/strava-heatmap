namespace StravaHeatmap.Api.Models;

// Strava'dan cekip kaydettigimiz aktivite.
public class Activity
{
    public int Id { get; set; }

    // Strava'daki aktivite numarasi. Tekil olacak: ayni aktivite iki kez kaydedilmesin.
    public long StravaActivityId { get; set; }

    // Hangi baglantiya ait oldugu.
    public int StravaConnectionId { get; set; }
    public StravaConnection? Connection { get; set; }

    public string? Name { get; set; }
    public string? SportType { get; set; }
    public DateTimeOffset StartDate { get; set; }

    // Ham degerler: metre ve saniye. Gosterim aninda km/dakikaya cevrilecek.
    public double DistanceMeters { get; set; }
    public int MovingTimeSeconds { get; set; }
    public int ElapsedTimeSeconds { get; set; }
    public double TotalElevationGain { get; set; }
    public double AverageSpeed { get; set; }

    // Sifrelenmis rota. Kapali mekan aktivitelerinde bos olur.
    public string? SummaryPolyline { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
}

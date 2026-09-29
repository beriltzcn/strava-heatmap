namespace StravaHeatmap.Api.Models;

public class StravaConnection
{
    public int Id { get; set; }
    public long AthletId { get; set; }
    
    public string? AthleteName { get; set; }

    public string AccessToken { get; set; } = "";
    public string RefreshToken { get; set; } = "";

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
}

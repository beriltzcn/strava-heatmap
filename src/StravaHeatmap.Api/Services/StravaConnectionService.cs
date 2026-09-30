using Microsoft.EntityFrameworkCore;
using StravaHeatmap.Api.Data;
using StravaHeatmap.Api.Models;

namespace StravaHeatmap.Api.Services;

public class StravaConnectionService
{

    private static readonly TimeSpan RefreshBuffer = TimeSpan.FromMinutes(5);

    private readonly AppDbContext _db;
    private readonly StravaAuthService _auth;
    private readonly ILogger<StravaConnectionService> _logger;

    public StravaConnectionService(
        AppDbContext db,
        StravaAuthService auth,
        ILogger<StravaConnectionService> logger)
        {
            _db = db;
            _auth = auth;
            _logger = logger;
        }
        
        public async Task<StravaConnection> GetConnectionWithValidTokenAsync(CancellationToken ct)
    {
        var connection = await _db.StravaConnections.FirstOrDefaultAsync(ct)
            ?? throw new StravaAuthException(
                "Strava bağlantısı bulunamadı. Önce /api/strava/connect adresine gidin.");

        if (connection.ExpiresAt <= DateTimeOffset.UtcNow.Add(RefreshBuffer))
        {
            _logger.LogInformation("Access token süresi doluyor, yenileniyor.");

            var refreshed = await _auth.RefreshTokenAsync(connection.RefreshToken, ct);

            connection.AccessToken = refreshed.AccessToken;
            connection.ExpiresAt = refreshed.ExpiresAt;

            if (!string.IsNullOrEmpty(refreshed.RefreshToken))
            {
                connection.RefreshToken = refreshed.RefreshToken;
            }

            connection.UpdatedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync(ct);
        }
        return connection;
    }  
}

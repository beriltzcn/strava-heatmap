using Microsoft.EntityFrameworkCore;
using StravaHeatmap.Api.Data;
using StravaHeatmap.Api.Models;

namespace StravaHeatmap.Api.Services;

// Loads the stored Strava connection and keeps its access token fresh.
public class StravaConnectionService
{

    // Refresh the token this long before it actually expires, so a request
    // never races against the expiry time.
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

    // Returns the connection with a usable access token, refreshing it if needed.
    public async Task<StravaConnection> GetConnectionWithValidTokenAsync(CancellationToken ct)
    {
        var connection = await _db.StravaConnections.FirstOrDefaultAsync(ct)
            ?? throw new StravaAuthException(
                "No Strava connection found. Open /api/strava/connect first.");

        if (connection.ExpiresAt <= DateTimeOffset.UtcNow.Add(RefreshBuffer))
        {
            _logger.LogInformation("Access token is expiring, refreshing it.");

            var refreshed = await _auth.RefreshTokenAsync(connection.RefreshToken, ct);

            connection.AccessToken = refreshed.AccessToken;
            connection.ExpiresAt = refreshed.ExpiresAt;

            // Some providers return a new refresh token, some do not.
            // Only overwrite ours when a value actually came back.
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

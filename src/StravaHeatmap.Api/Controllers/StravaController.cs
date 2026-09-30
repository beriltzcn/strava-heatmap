using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StravaHeatmap.Api.Data;
using StravaHeatmap.Api.Models;
using StravaHeatmap.Api.Services;

namespace StravaHeatmap.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StravaController : ControllerBase
{
    private const string StateSessionKey = "strava_oauth_state";

    private readonly AppDbContext _db;
    private readonly StravaAuthService _stravaAuth;
    private readonly StravaConnectionService _connections;
    private readonly StravaApiService _stravaApi;
    private readonly StravaSyncService _sync;

    public StravaController(
        AppDbContext db,
        StravaAuthService stravaAuth,
        StravaConnectionService connections,
        StravaApiService stravaApi,
        StravaSyncService sync)
    {
        _db = db;
        _stravaAuth = stravaAuth;
        _connections = connections;
        _stravaApi = stravaApi;
        _sync = sync;
    }

    // GET /api/strava/connect
    // Kullaniciyi Strava'nin izin ekranina gonderir.
    [HttpGet("connect")]
    public IActionResult Connect()
    {
        // CSRF korumasi: rastgele bir deger uretip oturumda sakliyoruz.
        var state = Guid.NewGuid().ToString("N");
        HttpContext.Session.SetString(StateSessionKey, state);

        var url = _stravaAuth.BuildAuthorizationUrl(state);
        return Redirect(url);
    }

    // GET /api/strava/callback?code=...&state=...&scope=...
    // Strava, kullanici izin verdikten sonra buraya geri gonderir.
    [HttpGet("callback")]
    public async Task<IActionResult> Callback(
        [FromQuery] string? code,
        [FromQuery] string? state,
        [FromQuery] string? error,
        CancellationToken ct)
    {
        // Kullanici izin ekraninda "Cancel" derse Strava error ile doner.
        if (!string.IsNullOrEmpty(error))
        {
            return BadRequest(new { message = $"Strava yetkilendirmeyi reddetti: {error}" });
        }

        if (string.IsNullOrEmpty(code))
        {
            return BadRequest(new { message = "code parametresi eksik." });
        }

        // State kontrolu: bu istek gercekten bizim baslattigimiz istek mi?
        var expectedState = HttpContext.Session.GetString(StateSessionKey);
        HttpContext.Session.Remove(StateSessionKey);

        if (string.IsNullOrEmpty(expectedState) || expectedState != state)
        {
            return BadRequest(new
            {
                message = "state dogrulanamadi. Lutfen baglantiyi bastan baslat."
            });
        }

        try
        {
            // Kodu token'a cevir.
            var token = await _stravaAuth.ExchangeCodeAsync(code, ct);

            if (token.Athlete is null)
            {
                return BadRequest(new { message = "Strava cevabinda atlet bilgisi yok." });
            }

            var athleteName = $"{token.Athlete.FirstName} {token.Athlete.LastName}".Trim();

            // Ayni atlet daha once baglandiysa guncelle, yoksa yeni kayit ac.
            var connection = await _db.StravaConnections
                .FirstOrDefaultAsync(c => c.AthleteId == token.Athlete.Id, ct);

            if (connection is null)
            {
                connection = new StravaConnection { AthleteId = token.Athlete.Id };
                _db.StravaConnections.Add(connection);
            }

            connection.AthleteName = string.IsNullOrWhiteSpace(athleteName) ? null : athleteName;
            connection.AccessToken = token.AccessToken;
            connection.RefreshToken = token.RefreshToken;
            connection.ExpiresAt = token.ExpiresAt;
            connection.UpdatedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync(ct);

            return Ok(new
            {
                status = "connected",
                athleteId = connection.AthleteId,
                athleteName = connection.AthleteName,
                expiresAt = connection.ExpiresAt
            });
        }
        catch (StravaAuthException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET /api/strava/activities?page=1&perPage=30
    // Token gerekirse otomatik yenilenir, sonra Strava'dan aktiviteler çekilir.
    [HttpGet("activities")]
    public async Task<IActionResult> GetActivities(
        [FromQuery] int page = 1,
        [FromQuery] int perPage = 30,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (perPage < 1 || perPage > 200) perPage = 30;

        try
        {
            var connection = await _connections.GetConnectionWithValidTokenAsync(ct);
            var activities = await _stravaApi.GetActivitiesAsync(
                connection.AccessToken, page, perPage, ct);

            return Ok(new
            {
                page,
                perPage,
                count = activities.Count,
                activities
            });
        }
        catch (StravaAuthException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // POST /api/strava/sync?maxPages=3
    // Strava'dan aktiviteleri ceker ve veritabanina kaydeder.
    [HttpPost("sync")]
    public async Task<IActionResult> Sync(
        [FromQuery] int maxPages = 3,
        CancellationToken ct = default)
    {
        if (maxPages < 1 || maxPages > 50) maxPages = 3;

        try
        {
            var result = await _sync.SyncAsync(maxPages, ct);

            return Ok(new
            {
                status = "synced",
                added = result.Added,
                updated = result.Updated,
                pagesFetched = result.PagesFetched
            });
        }
        catch (StravaAuthException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

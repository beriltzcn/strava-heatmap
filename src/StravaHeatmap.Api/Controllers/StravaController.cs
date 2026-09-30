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

    public StravaController(AppDbContext db, StravaAuthService stravaAuth)
    {
        _db = db;
        _stravaAuth = stravaAuth;
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
}

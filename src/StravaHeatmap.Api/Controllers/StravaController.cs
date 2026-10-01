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
    // Sends the user to Strava's authorization screen.
    [HttpGet("connect")]
    public IActionResult Connect()
    {
        // CSRF protection: generate a random value and keep it in the session.
        var state = Guid.NewGuid().ToString("N");
        HttpContext.Session.SetString(StateSessionKey, state);

        var url = _stravaAuth.BuildAuthorizationUrl(state);
        return Redirect(url);
    }

    // GET /api/strava/callback?code=...&state=...&scope=...
    // Strava sends the user back here after they approve.
    [HttpGet("callback")]
    public async Task<IActionResult> Callback(
        [FromQuery] string? code,
        [FromQuery] string? state,
        [FromQuery] string? error,
        CancellationToken ct)
    {
        // If the user cancels on the consent screen, Strava returns an error.
        if (!string.IsNullOrEmpty(error))
        {
            return BadRequest(new { message = $"Strava authorization was denied: {error}" });
        }

        if (string.IsNullOrEmpty(code))
        {
            return BadRequest(new { message = "The code parameter is missing." });
        }

        // State check: is this really the request we started?
        var expectedState = HttpContext.Session.GetString(StateSessionKey);
        HttpContext.Session.Remove(StateSessionKey);

        if (string.IsNullOrEmpty(expectedState) || expectedState != state)
        {
            return BadRequest(new
            {
                message = "State could not be verified. Please start the connection again."
            });
        }

        try
        {
            // Exchange the one-time code for tokens.
            var token = await _stravaAuth.ExchangeCodeAsync(code, ct);

            if (token.Athlete is null)
            {
                return BadRequest(new { message = "Strava response did not include athlete information." });
            }

            var athleteName = $"{token.Athlete.FirstName} {token.Athlete.LastName}".Trim();

            // Update the existing row for this athlete, or create a new one.
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
    // Refreshes the token when needed, then fetches activities straight from Strava.
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
    // Fetches activities from Strava and stores them in the database.
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

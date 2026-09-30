using Microsoft.EntityFrameworkCore;
using StravaHeatmap.Api.Data;
using StravaHeatmap.Api.Dtos;
using StravaHeatmap.Api.Models;

namespace StravaHeatmap.Api.Services;

// Senkronizasyon sonucunun ozeti.
public record SyncResult(int Added, int Updated, int PagesFetched);

public class StravaSyncService
{
    private const int PerPage = 100;

    private readonly AppDbContext _db;
    private readonly StravaConnectionService _connections;
    private readonly StravaApiService _stravaApi;
    private readonly ILogger<StravaSyncService> _logger;

    public StravaSyncService(
        AppDbContext db,
        StravaConnectionService connections,
        StravaApiService stravaApi,
        ILogger<StravaSyncService> logger)
    {
        _db = db;
        _connections = connections;
        _stravaApi = stravaApi;
        _logger = logger;
    }

    public async Task<SyncResult> SyncAsync(int maxPages, CancellationToken ct)
    {
        var connection = await _connections.GetConnectionWithValidTokenAsync(ct);

        var added = 0;
        var updated = 0;
        var page = 1;

        while (page <= maxPages)
        {
            var batch = await _stravaApi.GetActivitiesAsync(
                connection.AccessToken, page, PerPage, ct);

            // Strava veri bitince bos liste doner.
            if (batch.Count == 0)
            {
                break;
            }

            // Bu sayfadaki aktivitelerden hangileri zaten kayitli?
            // Hepsini tek sorguda bulup sozluge koyuyoruz (N+1 sorgusundan kacinma).
            var stravaIds = batch.Select(a => a.Id).ToList();

            var existing = await _db.Activities
                .Where(a => stravaIds.Contains(a.StravaActivityId))
                .ToDictionaryAsync(a => a.StravaActivityId, ct);

            foreach (var dto in batch)
            {
                if (existing.TryGetValue(dto.Id, out var activity))
                {
                    Apply(dto, activity);
                    activity.UpdatedAt = DateTimeOffset.UtcNow;
                    updated++;
                }
                else
                {
                    var created = new Activity { StravaConnectionId = connection.Id };
                    Apply(dto, created);
                    _db.Activities.Add(created);
                    added++;
                }
            }

            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Sayfa {Page}: {Count} aktivite islendi.", page, batch.Count);

            // Son sayfaya geldiysek devam etmenin anlami yok.
            if (batch.Count < PerPage)
            {
                break;
            }

            page++;
        }

        return new SyncResult(added, updated, page);
    }

    // Strava'dan gelen veriyi bizim tabloya aktarir.
    private static void Apply(StravaActivityDto dto, Activity activity)
    {
        activity.StravaActivityId = dto.Id;
        activity.Name = dto.Name;
        activity.SportType = dto.SportType;
        activity.StartDate = dto.StartDate;
        activity.DistanceMeters = dto.Distance;
        activity.MovingTimeSeconds = dto.MovingTime;
        activity.ElapsedTimeSeconds = dto.ElapsedTime;
        activity.TotalElevationGain = dto.TotalElevationGain;
        activity.AverageSpeed = dto.AverageSpeed;
        activity.SummaryPolyline = dto.Map?.SummaryPolyline;
    }
}

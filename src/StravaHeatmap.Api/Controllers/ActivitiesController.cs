using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StravaHeatmap.Api.Data;
using StravaHeatmap.Api.Dtos;

namespace StravaHeatmap.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ActivitiesController : ControllerBase
{
    private readonly AppDbContext _db;
    
    public ActivitiesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int limit = 100,
        [FromQuery] bool onlyWithRoute = false,
        CancellationToken ct = default)
    {
        if (limit < 1 || limit > 500) limit = 100;

        var query = _db.Activities.AsNoTracking();

        if (onlyWithRoute)
        {
            query = query.Where(a => a.SummaryPolyline != null && a.SummaryPolyline != "");
        }

        // NOTE: SQLite cannot translate ORDER BY on DateTimeOffset columns.
        // So we sort and limit in memory instead of in the database.
        // For personal use (thousands of activities) the cost is negligible;
        // if the dataset grows a lot we can change the column type (UTC DateTime
        // or a long) and move this back into SQL.
        var rows = await query
            .Select(a => new
            {
                a.Id,
                a.Name,
                a.SportType,
                a.StartDate,
                a.DistanceMeters,
                a.MovingTimeSeconds,
                a.TotalElevationGain,
                a.SummaryPolyline
            })
            .ToListAsync(ct);

        var activities = rows
            .OrderByDescending(a => a.StartDate)
            .Take(limit)
            .Select(a => new ActivitySummaryDto
        {
            Id = a.Id,
            Name = a.Name,
            SportType = a.SportType,
            StartDate = a.StartDate,
            DistanceKm = Math.Round(a.DistanceMeters / 1000.0, 2),
            MovingTimeMinutes = (int)Math.Round(a.MovingTimeSeconds / 60.0),
            ElevationGainMeters = a.TotalElevationGain,
            HasRoute = !string.IsNullOrEmpty(a.SummaryPolyline),
            Polyline = a.SummaryPolyline
        }).ToList();

        return Ok(new
        {
            count = activities.Count,
            activities
        });

    }
}

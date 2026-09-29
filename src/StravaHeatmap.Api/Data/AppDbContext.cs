using Microsoft.EntityFrameworkCore;

namespace StravaHeatmap.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
}

using Microsoft.EntityFrameworkCore;
using StravaHeatmap.Api.Models;

namespace StravaHeatmap.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    public DbSet<StravaConnection> StravaConnections => Set<StravaConnection>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<StravaConnection>()
            .HasIndex(c => c.AthleteId)
            .IsUnique();
    }

}

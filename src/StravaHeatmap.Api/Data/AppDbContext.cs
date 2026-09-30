using Microsoft.EntityFrameworkCore;
using StravaHeatmap.Api.Models;

namespace StravaHeatmap.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    public DbSet<StravaConnection> StravaConnections => Set<StravaConnection>();
    public DbSet<Activity> Activities => Set<Activity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<StravaConnection>()
            .HasIndex(c => c.AthleteId)
            .IsUnique();

        // Ayni Strava aktivitesi iki kez kaydedilmesin.
        modelBuilder.Entity<Activity>()
            .HasIndex(a => a.StravaActivityId)
            .IsUnique();

        // Bir baglanti silinirse aktiviteleri de silinsin.
        modelBuilder.Entity<Activity>()
            .HasOne(a => a.Connection)
            .WithMany()
            .HasForeignKey(a => a.StravaConnectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }

}

using Microsoft.EntityFrameworkCore;
using PuckStats.Shared;

namespace PuckStats.Api.Data;

/// <summary>
/// PostgreSQL database context for PuckStats.
/// Stores players, matches, replays, events, ratings, and analytics.
/// </summary>
public class PuckStatsDbContext : DbContext
{
    public PuckStatsDbContext(DbContextOptions<PuckStatsDbContext> options) : base(options) { }

    // Core entities
    public DbSet<PlayerEntity> Players => Set<PlayerEntity>();
    public DbSet<MatchEntity> Matches => Set<MatchEntity>();
    public DbSet<MatchPlayerEntity> MatchPlayers => Set<MatchPlayerEntity>();
    public DbSet<ReplayEntity> Replays => Set<ReplayEntity>();
    public DbSet<MatchEventEntity> MatchEvents => Set<MatchEventEntity>();

    // Analytics
    public DbSet<PlayerRatingEntity> PlayerRatings => Set<PlayerRatingEntity>();
    public DbSet<RatingHistoryEntity> RatingHistory => Set<RatingHistoryEntity>();
    public DbSet<HeatmapEntity> Heatmaps => Set<HeatmapEntity>();
    public DbSet<ShiftEntity> Shifts => Set<ShiftEntity>();

    // Telemetry
    public DbSet<TelemetryTickEntity> TelemetryTicks => Set<TelemetryTickEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Players
        modelBuilder.Entity<PlayerEntity>(entity =>
        {
            entity.HasKey(e => e.SteamId);
            entity.HasIndex(e => e.Username);
            entity.HasIndex(e => e.OverallRating);
            entity.HasIndex(e => e.Archetype);
            entity.Property(e => e.SteamId).HasMaxLength(32);
            entity.Property(e => e.Username).HasMaxLength(128);
        });

        // Matches
        modelBuilder.Entity<MatchEntity>(entity =>
        {
            entity.HasKey(e => e.MatchId);
            entity.HasIndex(e => e.StartTime);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.ServerName);
            entity.Property(e => e.MatchId).HasMaxLength(32);
        });

        // Match players (join table)
        modelBuilder.Entity<MatchPlayerEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.MatchId);
            entity.HasIndex(e => e.SteamId);
            entity.HasIndex(e => new { e.MatchId, e.SteamId }).IsUnique();
            entity.HasOne<PlayerEntity>().WithMany().HasForeignKey(e => e.SteamId);
            entity.HasOne<MatchEntity>().WithMany(m => m.Players).HasForeignKey(e => e.MatchId);
            entity.Property(e => e.MatchId).HasMaxLength(32);
            entity.Property(e => e.SteamId).HasMaxLength(32);
        });

        // Replays
        modelBuilder.Entity<ReplayEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.MatchId);
            entity.HasIndex(e => e.UploadedBySteamId);
            entity.HasIndex(e => e.UploadedAt);
            entity.Property(e => e.MatchId).HasMaxLength(32);
            entity.Property(e => e.UploadedBySteamId).HasMaxLength(32);
        });

        // Match events (goals, saves, faceoffs, hits, etc.)
        modelBuilder.Entity<MatchEventEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.MatchId);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.PlayerSteamId);
            entity.Property(e => e.MatchId).HasMaxLength(32);
            entity.Property(e => e.PlayerSteamId).HasMaxLength(32);
        });

        // Player ratings (latest)
        modelBuilder.Entity<PlayerRatingEntity>(entity =>
        {
            entity.HasKey(e => e.SteamId);
            entity.Property(e => e.SteamId).HasMaxLength(32);
        });

        // Rating history (time series)
        modelBuilder.Entity<RatingHistoryEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.SteamId);
            entity.HasIndex(e => new { e.SteamId, e.RecordedAt });
            entity.Property(e => e.SteamId).HasMaxLength(32);
        });

        // Heatmaps
        modelBuilder.Entity<HeatmapEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.MatchId);
            entity.HasIndex(e => new { e.MatchId, e.Type });
            entity.Property(e => e.MatchId).HasMaxLength(32);
            entity.Property(e => e.Data).HasColumnType("jsonb");
        });

        // Shifts
        modelBuilder.Entity<ShiftEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.MatchId);
            entity.HasIndex(e => e.SteamId);
            entity.Property(e => e.MatchId).HasMaxLength(32);
            entity.Property(e => e.SteamId).HasMaxLength(32);
        });

        // Telemetry (partitioned by match)
        modelBuilder.Entity<TelemetryTickEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.MatchId, e.SteamId, e.TickNumber });
            entity.Property(e => e.MatchId).HasMaxLength(32);
            entity.Property(e => e.SteamId).HasMaxLength(32);
            entity.Property(e => e.PositionData).HasColumnType("jsonb");
        });
    }
}

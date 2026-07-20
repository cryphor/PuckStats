using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using PuckStats.Api.Controllers;
using PuckStats.Api.Data;

namespace PuckStats.Api.Services;

/// <summary>
/// Background service that processes incoming telemetry batches asynchronously.
/// Decouples HTTP ingestion from database writes for throughput.
/// </summary>
public class TelemetryProcessorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TelemetryProcessorService> _logger;
    private readonly ConcurrentQueue<TelemetryBatchRequest> _batchQueue = new();
    private readonly ConcurrentQueue<MatchSubmissionRequest> _matchQueue = new();
    private readonly SemaphoreSlim _batchSignal = new(0);

    public TelemetryProcessorService(IServiceScopeFactory scopeFactory, ILogger<TelemetryProcessorService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public Task EnqueueTelemetryBatch(TelemetryBatchRequest batch)
    {
        _batchQueue.Enqueue(batch);
        _batchSignal.Release();
        return Task.CompletedTask;
    }

    public Task ProcessMatchSubmission(MatchSubmissionRequest match)
    {
        _matchQueue.Enqueue(match);
        return Task.CompletedTask;
    }

    public Task ProcessHeartbeat(HeartbeatRequest heartbeat)
    {
        // Lightweight: just track presence in Redis
        return Task.CompletedTask;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Telemetry processor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            await _batchSignal.WaitAsync(stoppingToken);

            // Process all queued batches
            while (_batchQueue.TryDequeue(out var batch))
            {
                try
                {
                    await ProcessBatchAsync(batch);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process telemetry batch for match {MatchId}", batch.MatchId);
                }
            }

            // Process match submissions
            while (_matchQueue.TryDequeue(out var match))
            {
                try
                {
                    await ProcessMatchAsync(match);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process match submission {MatchId}", match.MatchId);
                }
            }
        }
    }

    private async Task ProcessBatchAsync(TelemetryBatchRequest batch)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PuckStatsDbContext>();

        if (batch.Ticks == null || batch.Ticks.Length == 0) return;

        // Bulk insert ticks efficiently
        var entities = batch.Ticks.Select(t => new TelemetryTickEntity
        {
            MatchId = batch.MatchId,
            SteamId = batch.SteamId,
            Timestamp = t.Timestamp,
            TickNumber = t.TickNumber,
            PositionData = System.Text.Json.JsonSerializer.Serialize(new
            {
                x = t.PosX, y = t.PosY, z = t.PosZ,
                vx = t.VelX, vy = t.VelY, vz = t.VelZ
            }),
            Speed = MathF.Sqrt(t.VelX * t.VelX + t.VelY * t.VelY + t.VelZ * t.VelZ),
            InputDataJson = t.InputJson ?? "{}",
            MoveStateJson = t.MoveStateJson ?? "{}"
        }).ToList();

        db.TelemetryTicks.AddRange(entities);
        await db.SaveChangesAsync();

        _logger.LogDebug("Processed {Count} telemetry ticks for match {MatchId}",
            entities.Count, batch.MatchId);
    }

    private async Task ProcessMatchAsync(MatchSubmissionRequest match)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PuckStatsDbContext>();
        var playerService = scope.ServiceProvider.GetRequiredService<PlayerService>();

        // Create/update match
        var matchEntity = await db.Matches.FindAsync(match.MatchId);
        if (matchEntity == null)
        {
            matchEntity = new MatchEntity
            {
                MatchId = match.MatchId,
                ServerName = match.ServerName,
                StartTime = DateTime.UtcNow.AddSeconds(-match.MatchLengthSeconds),
                EndTime = DateTime.UtcNow,
                DurationSeconds = match.MatchLengthSeconds,
                BlueScore = match.BlueScore,
                RedScore = match.RedScore,
                Type = match.ServerName == "PRACTICE" ? "Practice" : "Public"
            };
            db.Matches.Add(matchEntity);
        }

        // Create/update match player
        var existing = await db.MatchPlayers
            .FirstOrDefaultAsync(mp => mp.MatchId == match.MatchId && mp.SteamId == match.SteamId);

        if (existing == null)
        {
            var mp = new MatchPlayerEntity
            {
                MatchId = match.MatchId,
                SteamId = match.SteamId,
                Team = match.Team,
                Goals = match.Goals,
                DistanceTraveled = match.DistanceTraveled,
                AverageSpeed = match.AverageSpeed,
                TopSpeed = match.TopSpeed,
                PuckTouches = match.PuckTouches,
                PossessionTimeSeconds = match.PossessionTimeSeconds
            };
            db.MatchPlayers.Add(mp);
        }

        await db.SaveChangesAsync();

        // Update player aggregate stats
        await playerService.UpdatePlayerFromMatch(match);
    }
}

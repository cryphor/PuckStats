using Microsoft.EntityFrameworkCore;
using PuckStats.Api.Controllers;
using PuckStats.Api.Data;
using PuckStats.Shared;
using PuckStats.ReplayParser;

namespace PuckStats.Api.Services;

/// <summary>
/// Handles replay file upload, parsing, storage, and AI analysis.
/// </summary>
public class ReplayService
{
    private readonly PuckStatsDbContext _db;
    private readonly ILogger<ReplayService> _logger;
    private readonly string _storagePath;

    public ReplayService(PuckStatsDbContext db, IConfiguration config, ILogger<ReplayService> logger)
    {
        _db = db;
        _logger = logger;
        _storagePath = config.GetValue<string>("ReplayStoragePath") ?? Path.Combine(Directory.GetCurrentDirectory(), "replays");
        Directory.CreateDirectory(_storagePath);
    }

    public async Task<long> ProcessUploadedReplay(IFormFile file, string? steamId)
    {
        var replayId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var filePath = Path.Combine(_storagePath, $"{replayId}.puckreplay");

        // Save to disk
        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var replayEntity = new ReplayEntity
        {
            Id = replayId,
            MatchId = "", // Will be populated by parsing
            UploadedBySteamId = steamId ?? "anonymous",
            FilePath = filePath,
            FileSizeBytes = file.Length,
            UploadedAt = DateTime.UtcNow,
            IsProcessed = false
        };

        _db.Replays.Add(replayEntity);
        await _db.SaveChangesAsync();

        // Parse in background
        _ = Task.Run(async () =>
        {
            try
            {
                using var parser = ReplayFileReader.Open(filePath);
                var parsed = parser.ParseFullReplay();

                replayEntity.MatchId = parsed.Header.MatchId;
                replayEntity.ParsedDataJson = System.Text.Json.JsonSerializer.Serialize(parsed, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                });
                replayEntity.IsProcessed = true;

                // Store heatmaps
                foreach (var (type, heatmap) in new[] {
                    ("Skating", parsed.SkatingHeatmap),
                    ("Shot", parsed.ShotHeatmap),
                    ("Possession", parsed.PossessionHeatmap)
                })
                {
                    _db.Heatmaps.Add(new HeatmapEntity
                    {
                        MatchId = parsed.Header.MatchId,
                        Type = type,
                        Data = System.Text.Json.JsonSerializer.Serialize(heatmap)
                    });
                }

                await _db.SaveChangesAsync();
                _logger.LogInformation("Replay {ReplayId} processed: {MatchId}, {FrameCount} frames",
                    replayId, parsed.Header.MatchId, parsed.Events.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse replay {ReplayId}", replayId);
            }
        });

        return replayId;
    }

    public async Task<object?> GetReplayData(long replayId)
    {
        var replay = await _db.Replays.FindAsync(replayId);
        if (replay == null || !replay.IsProcessed) return null;

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<object>(replay.ParsedDataJson);
        }
        catch { return null; }
    }

    public async Task<List<ReplayInfoDto>> GetPlayerReplays(string steamId, int page, int pageSize)
    {
        return await _db.Replays
            .Where(r => r.UploadedBySteamId == steamId)
            .OrderByDescending(r => r.UploadedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ReplayInfoDto
            {
                Id = r.Id,
                MatchId = r.MatchId,
                UploadedAt = r.UploadedAt,
                HasAiAnalysis = !string.IsNullOrEmpty(r.AiAnalysisJson) && r.AiAnalysisJson != "{}"
            })
            .ToListAsync();
    }

    public async Task<object> TriggerAiAnalysis(long replayId)
    {
        var replay = await _db.Replays.FindAsync(replayId);
        if (replay == null || !replay.IsProcessed)
            throw new InvalidOperationException("Replay not found or not processed");

        // AI analysis would call external service (OpenAI, etc.)
        // For now, generate a basic analysis from the parsed data
        try
        {
            var parsed = System.Text.Json.JsonSerializer.Deserialize<ParsedReplay>(
                replay.ParsedDataJson,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            var summary = "Match analysis generated.";
            var keyMoments = new List<object>();
            var suggestions = new List<string>
            {
                "Review positioning during defensive zone coverage.",
                "Focus on maintaining puck possession through the neutral zone."
            };

            var analysis = new { Summary = summary, KeyMoments = keyMoments, Suggestions = suggestions, GeneratedAt = DateTime.UtcNow };
            replay.AiAnalysisJson = System.Text.Json.JsonSerializer.Serialize(analysis);
            await _db.SaveChangesAsync();

            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI analysis failed for replay {ReplayId}", replayId);
            throw;
        }
    }

    public async Task<List<object>> GetGoalBreakdowns(long replayId)
    {
        var replay = await _db.Replays.FindAsync(replayId);
        if (replay == null) return new List<object>();

        var events = await _db.MatchEvents
            .Where(e => e.MatchId == replay.MatchId && e.Type == "Goal")
            .OrderBy(e => e.Timestamp)
            .ToListAsync();

        return events.Select(e => new
        {
            e.Timestamp,
            e.Period,
            e.PlayerSteamId,
            e.SecondaryPlayerSteamId,
            e.Team
        }).Cast<object>().ToList();
    }

    public async Task<List<ShiftData>> GetShiftAnalytics(long replayId, string steamId)
    {
        return await _db.Shifts
            .Where(s => s.MatchId == _db.Replays.Where(r => r.Id == replayId).Select(r => r.MatchId).FirstOrDefault()
                     && s.SteamId == steamId)
            .OrderBy(s => s.StartTime)
            .Select(s => new ShiftData
            {
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                DistanceTraveled = s.DistanceTraveled,
                AverageSpeed = s.AverageSpeed,
                PossessionPercent = s.PossessionPercent,
                OffensiveImpact = s.OffensiveImpact,
                DefensiveImpact = s.DefensiveImpact,
                ShiftScore = s.ShiftScore
            })
            .ToListAsync();
    }

    public async Task<bool> DeleteReplay(long replayId, string steamId)
    {
        var replay = await _db.Replays.FindAsync(replayId);
        if (replay == null || replay.UploadedBySteamId != steamId) return false;

        // Delete file
        if (File.Exists(replay.FilePath))
            File.Delete(replay.FilePath);

        _db.Replays.Remove(replay);
        await _db.SaveChangesAsync();
        return true;
    }
}

using Microsoft.EntityFrameworkCore;
using PuckStats.Api.Controllers;
using PuckStats.Api.Data;
using PuckStats.Shared;
using PuckStats.Analytics;

namespace PuckStats.Api.Services;

public class AnalyticsService
{
    private readonly PuckStatsDbContext _db;
    private readonly RatingEngine _ratingEngine;

    public AnalyticsService(PuckStatsDbContext db, RatingEngine ratingEngine)
    {
        _db = db;
        _ratingEngine = ratingEngine;
    }

    public async Task<PlayerAnalyticsResponse?> GetPlayerAnalytics(string steamId)
    {
        var ratings = await _db.PlayerRatings.FindAsync(steamId);
        if (ratings == null) return null;

        var player = await _db.Players.FindAsync(steamId);
        var recentMatches = await _db.MatchPlayers
            .Where(mp => mp.SteamId == steamId)
            .OrderByDescending(mp => mp.Id)
            .Take(5)
            .ToListAsync();

        // Aggregate telemetry
        var aggregate = await BuildAggregate(steamId);

        // Recompute ratings with latest data
        var computed = _ratingEngine.ComputeRatings(aggregate);
        computed.Overall = _ratingEngine.ComputeOverall(computed);
        computed.Archetype = _ratingEngine.DetectArchetype(computed);
        var percentiles = _ratingEngine.ComputePercentiles(computed);

        // Get heatmaps from recent matches
        var matchIds = recentMatches.Select(m => m.MatchId).ToList();
        var heatmaps = await _db.Heatmaps
            .Where(h => matchIds.Contains(h.MatchId))
            .ToListAsync();

        return new PlayerAnalyticsResponse
        {
            SteamId = steamId,
            Ratings = computed,
            Percentiles = percentiles,
            SkatingHeatmap = ParseHeatmap(heatmaps.FirstOrDefault(h => h.Type == "Skating")),
            ShotHeatmap = ParseHeatmap(heatmaps.FirstOrDefault(h => h.Type == "Shot")),
            PossessionHeatmap = ParseHeatmap(heatmaps.FirstOrDefault(h => h.Type == "Possession")),
            ScoutingReport = _ratingEngine.GenerateScoutingReport(computed, aggregate)
        };
    }

    public async Task<List<RatingTrend>> GetRatingHistory(string steamId, int days)
    {
        var since = DateTime.UtcNow.AddDays(-days);
        var history = await _db.RatingHistory
            .Where(h => h.SteamId == steamId && h.RecordedAt >= since)
            .OrderBy(h => h.RecordedAt)
            .ToListAsync();

        var trends = new Dictionary<string, List<TrendPoint>>();
        foreach (var entry in history)
        {
            try
            {
                var ratings = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(entry.RatingsJson);
                if (ratings == null) continue;
                foreach (var (category, value) in ratings)
                {
                    if (!trends.ContainsKey(category))
                        trends[category] = new List<TrendPoint>();
                    trends[category].Add(new TrendPoint { Date = entry.RecordedAt, Rating = value });
                }
            }
            catch { }
        }

        return trends.Select(kvp => new RatingTrend
        {
            Category = kvp.Key,
            Points = kvp.Value
        }).ToList();
    }

    public async Task<CompareResult> ComparePlayers(string steamIdA, string steamIdB)
    {
        var a = await GetPlayerAnalytics(steamIdA);
        var b = await GetPlayerAnalytics(steamIdB);

        if (a?.Ratings == null || b?.Ratings == null)
            throw new InvalidOperationException("One or both players not found");

        var differences = new Dictionary<string, int>
        {
            ["Skating"] = a.Ratings.Skating - b.Ratings.Skating,
            ["Shooting"] = a.Ratings.Shooting - b.Ratings.Shooting,
            ["Stickhandling"] = a.Ratings.Stickhandling - b.Ratings.Stickhandling,
            ["Passing"] = a.Ratings.Passing - b.Ratings.Passing,
            ["Inputs"] = a.Ratings.Inputs - b.Ratings.Inputs,
            ["GameSense"] = a.Ratings.GameSense - b.Ratings.GameSense,
            ["Overall"] = a.Ratings.Overall - b.Ratings.Overall,
        };

        return new CompareResult
        {
            PlayerA = null!, // Placeholder - would fetch full profiles
            PlayerB = null!,
            Differences = differences
        };
    }

    public async Task<DashboardData?> GetDashboard(string steamId)
    {
        var player = await _db.Players.FindAsync(steamId);
        if (player == null) return null;

        var ratings = await _db.PlayerRatings.FindAsync(steamId);
        var trends = await GetRatingHistory(steamId, 30);
        var recent = await _db.MatchPlayers
            .Where(mp => mp.SteamId == steamId)
            .OrderByDescending(mp => mp.Id)
            .Take(5)
            .Select(mp => new RecentMatch { MatchId = mp.MatchId, Goals = mp.Goals, Assists = mp.Assists, Rating = mp.MatchRating })
            .ToListAsync();

        return new DashboardData
        {
            OverallRating = player.OverallRating,
            Ratings = ratings != null ? new PlayerRatings { } : new PlayerRatings(),
            Percentiles = ratings != null ? new Percentiles { } : new Percentiles(),
            Trends = trends,
            RecentGames = recent,
            SessionsThisWeek = 0, // Would track via session data
            HoursPlayed = (float)(player.TotalPlayTimeSeconds / 3600.0)
        };
    }

    private async Task<PlayerTelemetryAggregate> BuildAggregate(string steamId)
    {
        // Aggregate telemetry from recent matches
        var ticks = await _db.TelemetryTicks
            .Where(t => t.SteamId == steamId)
            .OrderByDescending(t => t.Id)
            .Take(10000)
            .ToListAsync();

        var aggregate = PlayerTelemetryAggregate.Neutral();

        if (ticks.Count > 0)
        {
            var speeds = ticks.Select(t => t.Speed).Where(s => s > 0).ToList();
            if (speeds.Count > 0)
            {
                speeds.Sort();
                aggregate.AvgSpeed = speeds.Average();
                aggregate.AvgTopSpeed = speeds.TakeLast(Math.Max(1, speeds.Count / 20)).Average();
                aggregate.AvgBurstSpeed = speeds.TakeLast(Math.Max(1, speeds.Count / 10)).Average();
            }
        }

        return aggregate;
    }

    private static HeatmapData? ParseHeatmap(HeatmapEntity? entity)
    {
        if (entity == null) return null;
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<HeatmapData>(entity.Data);
        }
        catch { return null; }
    }
}

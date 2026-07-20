using Microsoft.EntityFrameworkCore;
using PuckStats.Api.Data;

namespace PuckStats.Api.Services;

/// <summary>
/// Leaderboard computation and caching service.
/// </summary>
public class LeaderboardService
{
    private readonly PuckStatsDbContext _db;

    public LeaderboardService(PuckStatsDbContext db)
    {
        _db = db;
    }

    public async Task RecomputeGlobalLeaderboards()
    {
        // Triggered periodically to refresh rankings
        var allPlayers = await _db.PlayerRatings
            .OrderByDescending(r => r.Overall)
            .Take(1000)
            .ToListAsync();

        // Percentile computation across entire population
        if (allPlayers.Count < 2) return;

        // For each rating category, compute percentiles
        var categories = new Dictionary<string, Func<PlayerRatingEntity, int>>
        {
            ["Skating"] = r => r.Skating,
            ["Shooting"] = r => r.Shooting,
            ["Stickhandling"] = r => r.Stickhandling,
            ["Passing"] = r => r.Passing,
            ["Inputs"] = r => r.Inputs,
            ["StickMotion"] = r => r.StickMotion,
            ["OffensivePlay"] = r => r.OffensivePlay,
            ["DefensivePlay"] = r => r.DefensivePlay,
            ["Positioning"] = r => r.Positioning,
            ["GameSense"] = r => r.GameSense,
            ["Overall"] = r => r.Overall
        };

        foreach (var (category, selector) in categories)
        {
            var sorted = allPlayers.OrderByDescending(selector).ToList();
            int total = sorted.Count;

            for (int i = 0; i < total; i++)
            {
                int percentile = total > 1 ? (int)((1.0 - (double)i / (total - 1)) * 100) : 50;
                // Update percentile in database
                var entity = await _db.PlayerRatings.FindAsync(sorted[i].SteamId);
                if (entity != null)
                {
                    switch (category)
                    {
                        case "Skating": entity.SkatingPercentile = percentile; break;
                        case "Shooting": entity.ShootingPercentile = percentile; break;
                        case "Stickhandling": entity.StickhandlingPercentile = percentile; break;
                        case "Passing": entity.PassingPercentile = percentile; break;
                        case "Inputs": entity.InputsPercentile = percentile; break;
                        case "StickMotion": entity.StickMotionPercentile = percentile; break;
                        case "OffensivePlay": entity.OffensivePlayPercentile = percentile; break;
                        case "DefensivePlay": entity.DefensivePlayPercentile = percentile; break;
                        case "Positioning": entity.PositioningPercentile = percentile; break;
                        case "GameSense": entity.GameSensePercentile = percentile; break;
                        case "Overall": entity.OverallPercentile = percentile; break;
                    }
                }
            }
        }

        await _db.SaveChangesAsync();
    }
}

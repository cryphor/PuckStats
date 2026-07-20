using Microsoft.EntityFrameworkCore;
using PuckStats.Api.Controllers;
using PuckStats.Api.Data;
using PuckStats.Shared;
using PuckStats.Analytics;

namespace PuckStats.Api.Services;

/// <summary>
/// Service for player profile management, leaderboards, and stats updates.
/// </summary>
public class PlayerService
{
    private readonly PuckStatsDbContext _db;
    private readonly RatingEngine _ratingEngine;
    private readonly ILogger<PlayerService> _logger;

    public PlayerService(PuckStatsDbContext db, RatingEngine ratingEngine, ILogger<PlayerService> logger)
    {
        _db = db;
        _ratingEngine = ratingEngine;
        _logger = logger;
    }

    public async Task<PlayerProfile?> GetPlayerProfile(string steamId)
    {
        try
        {
            var player = await _db.Players.FindAsync(steamId);
            if (player == null)
            {
                // Player not yet in DB — return a default profile
                return new PlayerProfile
                {
                    SteamId = steamId,
                    Username = steamId.Length > 10 ? steamId[..10] : steamId,
                    AvatarUrl = "",
                    Ratings = _ratingEngine.ComputeRatings(PlayerTelemetryAggregate.Neutral()),
                    Percentiles = _ratingEngine.ComputePercentiles(new PlayerRatings()),
                    Archetype = Archetype.Unknown,
                    TotalMatches = 0,
                    TotalGoals = 0,
                    TotalAssists = 0,
                    TotalSaves = 0,
                    WinRate = 0,
                    RecentMatches = Array.Empty<RecentMatch>(),
                    LastUpdated = DateTime.UtcNow
                };
            }

            var ratings = await _db.PlayerRatings.FindAsync(steamId);
            var recentMatches = await _db.MatchPlayers
                .Where(mp => mp.SteamId == steamId)
                .OrderByDescending(mp => _db.Matches
                    .Where(m => m.MatchId == mp.MatchId)
                    .Select(m => m.StartTime)
                    .FirstOrDefault())
                .Take(10)
                .Select(mp => new RecentMatch
                {
                    MatchId = mp.MatchId,
                    Team = mp.Team.ToString(),
                    Goals = mp.Goals,
                    Assists = mp.Assists,
                    Rating = mp.MatchRating
                })
                .ToListAsync();

            return new PlayerProfile
            {
                SteamId = player.SteamId,
                Username = player.Username,
                AvatarUrl = player.AvatarUrl,
                Ratings = ratings != null ? EntityToRatings(ratings) : new PlayerRatings(),
                Percentiles = ratings != null ? EntityToPercentiles(ratings) : new Percentiles(),
                Archetype = Enum.TryParse<Archetype>(player.Archetype, out var arch) ? arch : Archetype.Unknown,
                TotalMatches = player.TotalMatches,
                TotalGoals = player.TotalGoals,
                TotalAssists = player.TotalAssists,
                TotalSaves = player.TotalSaves,
                WinRate = player.TotalMatches > 0 ? (float)player.TotalWins / player.TotalMatches : 0,
                RecentMatches = recentMatches.ToArray(),
                LastUpdated = player.LastUpdated
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "DB query failed for player {SteamId}", steamId);
            return new PlayerProfile
            {
                SteamId = steamId,
                Username = "Player",
                Ratings = new PlayerRatings { Overall = 50 },
                Archetype = Archetype.Unknown,
                LastUpdated = DateTime.UtcNow
            };
        }
    }

    public async Task UpdatePlayerFromMatch(MatchSubmissionRequest match)
    {
        var player = await _db.Players.FindAsync(match.SteamId);
        if (player == null)
        {
            player = new PlayerEntity
            {
                SteamId = match.SteamId,
                Username = match.Username,
                FirstSeen = DateTime.UtcNow
            };
            _db.Players.Add(player);
        }

        player.TotalMatches++;
        player.TotalDistanceTraveled += match.DistanceTraveled;
        player.TotalPlayTimeSeconds += match.MatchLengthSeconds;
        player.LastSeen = DateTime.UtcNow;
        player.LastUpdated = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task<List<LeaderboardEntry>> GetLeaderboard(string category, int page, int pageSize)
    {
        var query = _db.PlayerRatings.AsQueryable();

        // Order by relevant column
        query = category switch
        {
            "Overall" => query.OrderByDescending(r => r.Overall),
            "Skating" => query.OrderByDescending(r => r.Skating),
            "Shooting" => query.OrderByDescending(r => r.Shooting),
            "Stickhandling" => query.OrderByDescending(r => r.Stickhandling),
            "Passing" => query.OrderByDescending(r => r.Passing),
            "Inputs" => query.OrderByDescending(r => r.Inputs),
            "GameSense" => query.OrderByDescending(r => r.GameSense),
            _ => query.OrderByDescending(r => r.Overall)
        };

        var rawEntries = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Join(_db.Players,
                r => r.SteamId,
                p => p.SteamId,
                (r, p) => new { r, p })
            .ToListAsync();

        var entries = rawEntries.Select(x => new LeaderboardEntry
        {
            SteamId = x.r.SteamId,
            Username = x.p.Username,
            Rating = category switch
            {
                "Skating" => x.r.Skating,
                "Shooting" => x.r.Shooting,
                "Stickhandling" => x.r.Stickhandling,
                "Passing" => x.r.Passing,
                "Inputs" => x.r.Inputs,
                "GameSense" => x.r.GameSense,
                _ => x.r.Overall
            },
            MatchesPlayed = x.p.TotalMatches,
            Archetype = Enum.TryParse<Archetype>(x.p.Archetype, out var a) ? a : Archetype.Unknown
        }).ToList();

        // Assign ranks
        for (int i = 0; i < entries.Count; i++)
            entries[i].Rank = (page - 1) * pageSize + i + 1;

        return entries;
    }

    private static PlayerRatings EntityToRatings(PlayerRatingEntity e) => new()
    {
        Skating = e.Skating, Shooting = e.Shooting,
        Stickhandling = e.Stickhandling, Passing = e.Passing,
        Inputs = e.Inputs, StickMotion = e.StickMotion,
        OffensivePlay = e.OffensivePlay, DefensivePlay = e.DefensivePlay,
        Positioning = e.Positioning, GameSense = e.GameSense, Overall = e.Overall
    };

    private static Percentiles EntityToPercentiles(PlayerRatingEntity e) => new()
    {
        SkatingPercentile = e.SkatingPercentile,
        ShootingPercentile = e.ShootingPercentile,
        StickhandlingPercentile = e.StickhandlingPercentile,
        PassingPercentile = e.PassingPercentile,
        InputsPercentile = e.InputsPercentile,
        StickMotionPercentile = e.StickMotionPercentile,
        OffensivePlayPercentile = e.OffensivePlayPercentile,
        DefensivePlayPercentile = e.DefensivePlayPercentile,
        PositioningPercentile = e.PositioningPercentile,
        GameSensePercentile = e.GameSensePercentile,
        OverallPercentile = e.OverallPercentile
    };
}

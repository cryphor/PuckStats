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
        var player = await _db.Players.FindAsync(steamId);
        if (player == null) return null;

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
                Team = Enum.TryParse<PlayerTeam>(mp.Team, out var t) ? t : PlayerTeam.None,
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

        var entries = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Join(_db.Players,
                r => r.SteamId,
                p => p.SteamId,
                (r, p) => new LeaderboardEntry
                {
                    SteamId = r.SteamId,
                    Username = p.Username,
                    Rating = category switch
                    {
                        "Overall" => r.Overall,
                        "Skating" => r.Skating,
                        "Shooting" => r.Shooting,
                        "Stickhandling" => r.Stickhandling,
                        "Passing" => r.Passing,
                        "Inputs" => r.Inputs,
                        "GameSense" => r.GameSense,
                        _ => r.Overall
                    },
                    MatchesPlayed = p.TotalMatches,
                    Archetype = Enum.TryParse<Archetype>(p.Archetype, out var a) ? a : Archetype.Unknown
                })
            .ToListAsync();

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

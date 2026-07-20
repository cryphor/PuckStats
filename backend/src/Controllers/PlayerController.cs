using Microsoft.AspNetCore.Mvc;
using PuckStats.Api.Services;
using PuckStats.Shared;

namespace PuckStats.Api.Controllers;

/// <summary>
/// Player profile and analytics endpoints.
/// </summary>
[ApiController]
[Route("api/player")]
public class PlayerController : ControllerBase
{
    private readonly PlayerService _playerService;
    private readonly AnalyticsService _analyticsService;
    private readonly ReplayService _replayService;

    public PlayerController(PlayerService playerService, AnalyticsService analyticsService, ReplayService replayService)
    {
        _playerService = playerService;
        _analyticsService = analyticsService;
        _replayService = replayService;
    }

    /// <summary>
    /// Get a player's full profile with ratings, percentiles, and recent matches.
    /// </summary>
    [HttpGet("{steamId}")]
    public async Task<ActionResult<PlayerProfile>> GetPlayer(string steamId)
    {
        var profile = await _playerService.GetPlayerProfile(steamId);
        return Ok(profile);
    }

    /// <summary>
    /// Get detailed analytics for a player.
    /// </summary>
    [HttpGet("{steamId}/analytics")]
    public async Task<ActionResult<PlayerAnalyticsResponse>> GetPlayerAnalytics(string steamId)
    {
        var analytics = await _analyticsService.GetPlayerAnalytics(steamId);
        if (analytics == null)
            return NotFound(new { error = "Player not found" });
        return Ok(analytics);
    }

    /// <summary>
    /// Get a player's replays.
    /// </summary>
    [HttpGet("{steamId}/replays")]
    public async Task<ActionResult<List<ReplayInfoDto>>> GetPlayerReplays(string steamId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var replays = await _replayService.GetPlayerReplays(steamId, page, pageSize);
        return Ok(replays);
    }

    /// <summary>
    /// Get rating history for trend charts.
    /// </summary>
    [HttpGet("{steamId}/rating-history")]
    public async Task<ActionResult<List<RatingTrend>>> GetRatingHistory(string steamId, [FromQuery] int days = 90)
    {
        var history = await _analyticsService.GetRatingHistory(steamId, days);
        return Ok(history);
    }

    /// <summary>
    /// Compare two players side by side.
    /// </summary>
    [HttpGet("compare")]
    public async Task<ActionResult<CompareResult>> ComparePlayers([FromQuery] string playerA, [FromQuery] string playerB)
    {
        var result = await _analyticsService.ComparePlayers(playerA, playerB);
        return Ok(result);
    }

    /// <summary>
    /// Get leaderboards.
    /// </summary>
    [HttpGet("leaderboard/{category}")]
    public async Task<ActionResult<List<LeaderboardEntry>>> GetLeaderboard(
        string category = "Overall",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var leaderboard = await _playerService.GetLeaderboard(category, page, pageSize);
        return Ok(leaderboard);
    }

    /// <summary>
    /// Get dashboard data for the landing page.
    /// </summary>
    [HttpGet("{steamId}/dashboard")]
    public async Task<ActionResult<DashboardData>> GetDashboard(string steamId)
    {
        var dashboard = await _analyticsService.GetDashboard(steamId);
        if (dashboard == null)
            return NotFound(new { error = "Player not found" });
        return Ok(dashboard);
    }
}

public class PlayerAnalyticsResponse
{
    public string SteamId { get; set; } = "";
    public PlayerRatings? Ratings { get; set; }
    public Percentiles? Percentiles { get; set; }
    public Dictionary<string, List<TrendPoint>>? Trends { get; set; }
    public HeatmapData? SkatingHeatmap { get; set; }
    public HeatmapData? ShotHeatmap { get; set; }
    public HeatmapData? PossessionHeatmap { get; set; }
    public PassingNetwork? PassingNetwork { get; set; }
    public List<ShiftData>? RecentShifts { get; set; }
    public ScoutingReport? ScoutingReport { get; set; }
}

public class ReplayInfoDto
{
    public long Id { get; set; }
    public string MatchId { get; set; } = "";
    public DateTime UploadedAt { get; set; }
    public string ServerName { get; set; } = "";
    public int BlueScore { get; set; }
    public int RedScore { get; set; }
    public float DurationSeconds { get; set; }
    public bool HasAiAnalysis { get; set; }
}

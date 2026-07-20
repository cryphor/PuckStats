using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PuckStats.Api.Hubs;
using PuckStats.Api.Services;

namespace PuckStats.Api.Controllers;

/// <summary>
/// Replay upload, parsing, and analysis endpoints.
/// </summary>
[ApiController]
[Route("api/replay")]
public class ReplayController : ControllerBase
{
    private readonly ReplayService _replayService;
    private readonly IHubContext<ReplayHub> _replayHub;
    private readonly ILogger<ReplayController> _logger;

    public ReplayController(ReplayService replayService, IHubContext<ReplayHub> replayHub, ILogger<ReplayController> logger)
    {
        _replayService = replayService;
        _replayHub = replayHub;
        _logger = logger;
    }

    /// <summary>
    /// Upload a .puckreplay file for analysis.
    /// </summary>
    [HttpPost("upload")]
    [RequestSizeLimit(500_000_000)] // 500MB
    public async Task<IActionResult> UploadReplay(IFormFile file, [FromForm] string? steamId)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "No file provided" });

        if (!file.FileName.EndsWith(".puckreplay", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = "Only .puckreplay files are accepted" });

        var replayId = await _replayService.ProcessUploadedReplay(file, steamId);

        await _replayHub.Clients.All.SendAsync("ReplayUploaded", new { replayId, fileName = file.FileName });

        return Ok(new { replayId, fileName = file.FileName, size = file.Length });
    }

    /// <summary>
    /// Get parsed replay data including events, heatmaps, and passing networks.
    /// </summary>
    [HttpGet("{replayId}")]
    public async Task<IActionResult> GetReplay(long replayId)
    {
        var replay = await _replayService.GetReplayData(replayId);
        if (replay == null)
            return NotFound(new { error = "Replay not found" });
        return Ok(replay);
    }

    /// <summary>
    /// Trigger AI analysis on a replay.
    /// </summary>
    [HttpPost("{replayId}/analyze")]
    public async Task<IActionResult> AnalyzeReplay(long replayId)
    {
        var result = await _replayService.TriggerAiAnalysis(replayId);
        return Ok(result);
    }

    /// <summary>
    /// Get goal breakdowns from a replay.
    /// </summary>
    [HttpGet("{replayId}/goals")]
    public async Task<IActionResult> GetGoalBreakdowns(long replayId)
    {
        var goals = await _replayService.GetGoalBreakdowns(replayId);
        return Ok(goals);
    }

    /// <summary>
    /// Get shift analytics from a replay.
    /// </summary>
    [HttpGet("{replayId}/shifts/{steamId}")]
    public async Task<IActionResult> GetShiftAnalytics(long replayId, string steamId)
    {
        var shifts = await _replayService.GetShiftAnalytics(replayId, steamId);
        return Ok(shifts);
    }

    /// <summary>
    /// Delete a replay.
    /// </summary>
    [HttpDelete("{replayId}")]
    public async Task<IActionResult> DeleteReplay(long replayId, [FromHeader(Name = "X-Steam-Id")] string steamId)
    {
        var deleted = await _replayService.DeleteReplay(replayId, steamId);
        if (!deleted)
            return NotFound(new { error = "Replay not found or unauthorized" });
        return NoContent();
    }
}

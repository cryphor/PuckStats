using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PuckStats.Api.Data;
using PuckStats.Api.Services;
using PuckStats.Api.Hubs;
using PuckStats.Shared;

namespace PuckStats.Api.Controllers;

/// <summary>
/// Telemetry ingestion endpoint. Receives batch telemetry from the PuckStats mod.
/// </summary>
[ApiController]
[Route("api/telemetry")]
public class TelemetryController : ControllerBase
{
    private readonly TelemetryProcessorService _processor;
    private readonly IHubContext<TelemetryHub> _telemetryHub;
    private readonly ILogger<TelemetryController> _logger;

    public TelemetryController(
        TelemetryProcessorService processor,
        IHubContext<TelemetryHub> telemetryHub,
        ILogger<TelemetryController> logger)
    {
        _processor = processor;
        _telemetryHub = telemetryHub;
        _logger = logger;
    }

    /// <summary>
    /// Receive a batch of telemetry ticks from a client mod.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> PostTelemetry([FromBody] TelemetryBatchRequest request)
    {
        if (string.IsNullOrEmpty(request.MatchId) || string.IsNullOrEmpty(request.SteamId))
            return BadRequest(new { error = "MatchId and SteamId are required" });

        // Queue for async processing
        await _processor.EnqueueTelemetryBatch(request);

        // Notify real-time subscribers
        await _telemetryHub.Clients.Group(request.MatchId)
            .SendAsync("TelemetryUpdate", new
            {
                request.MatchId,
                TickCount = request.Ticks?.Length ?? 0,
                LatestTimestamp = request.Ticks?.LastOrDefault()?.Timestamp
            });

        return Accepted(new { queued = request.Ticks?.Length ?? 0 });
    }

    /// <summary>
    /// Submit a completed match for analysis.
    /// </summary>
    [HttpPost("match")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> PostMatch([FromBody] MatchSubmissionRequest request)
    {
        await _processor.ProcessMatchSubmission(request);
        return Accepted(new { matchId = request.MatchId });
    }

    /// <summary>
    /// Heartbeat for real-time connection tracking.
    /// </summary>
    [HttpPost("heartbeat")]
    public async Task<IActionResult> PostHeartbeat([FromBody] HeartbeatRequest request)
    {
        await _processor.ProcessHeartbeat(request);
        return Ok();
    }
}

public class TelemetryBatchRequest
{
    public string MatchId { get; set; } = "";
    public string SteamId { get; set; } = "";
    public string SessionId { get; set; } = "";
    public TelemetryTickDto[]? Ticks { get; set; }
}

public class TelemetryTickDto
{
    public float Timestamp { get; set; }
    public int TickNumber { get; set; }
    public float PosX { get; set; }
    public float PosY { get; set; }
    public float PosZ { get; set; }
    public float VelX { get; set; }
    public float VelY { get; set; }
    public float VelZ { get; set; }
    public float AccelX { get; set; }
    public float AccelY { get; set; }
    public float AccelZ { get; set; }
    public float DirX { get; set; }
    public float DirY { get; set; }
    public float DirZ { get; set; }
    public string? InputJson { get; set; }
    public string? MoveStateJson { get; set; }
    public float StickAngle { get; set; }
}

public class MatchSubmissionRequest
{
    public string MatchId { get; set; } = "";
    public string ServerName { get; set; } = "";
    public float MatchLengthSeconds { get; set; }
    public int BlueScore { get; set; }
    public int RedScore { get; set; }
    public string SteamId { get; set; } = "";
    public string Username { get; set; } = "";
    public string Team { get; set; } = "";
    public float DistanceTraveled { get; set; }
    public float AverageSpeed { get; set; }
    public float TopSpeed { get; set; }
    public int Goals { get; set; }
    public int PuckTouches { get; set; }
    public float PossessionTimeSeconds { get; set; }
}

public class HeartbeatRequest
{
    public string SteamId { get; set; } = "";
    public string MatchId { get; set; } = "";
    public float Timestamp { get; set; }
}

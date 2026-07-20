using Microsoft.AspNetCore.SignalR;

namespace PuckStats.Api.Hubs;

/// <summary>
/// Real-time hub for replay viewer collaboration and live replay processing.
/// </summary>
public class ReplayHub : Hub
{
    private readonly ILogger<ReplayHub> _logger;

    public ReplayHub(ILogger<ReplayHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Join a replay viewing session (for collaborative analysis).
    /// </summary>
    public async Task JoinReplaySession(long replayId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"replay:{replayId}");
        _logger.LogInformation("Client joined replay session {ReplayId}", replayId);
    }

    /// <summary>
    /// Broadcast playback position for synchronized viewing.
    /// </summary>
    public async Task SyncPlayback(long replayId, float timestamp, bool isPlaying)
    {
        await Clients.OthersInGroup($"replay:{replayId}")
            .SendAsync("PlaybackSync", new { timestamp, isPlaying });
    }

    /// <summary>
    /// Share a camera position in the replay viewer.
    /// </summary>
    public async Task ShareCamera(long replayId, string cameraMode, float x, float y, float z, float zoom)
    {
        await Clients.OthersInGroup($"replay:{replayId}")
            .SendAsync("CameraSync", new { cameraMode, x, y, z, zoom });
    }
}

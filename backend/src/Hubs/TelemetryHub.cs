using Microsoft.AspNetCore.SignalR;

namespace PuckStats.Api.Hubs;

/// <summary>
/// Real-time telemetry hub. Clients subscribe to match groups to receive
/// live telemetry updates as they happen.
/// </summary>
public class TelemetryHub : Hub
{
    private readonly ILogger<TelemetryHub> _logger;

    public TelemetryHub(ILogger<TelemetryHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Subscribe to live telemetry for a match.
    /// </summary>
    public async Task SubscribeToMatch(string matchId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, matchId);
        _logger.LogInformation("Client {ClientId} subscribed to match {MatchId}", Context.ConnectionId, matchId);
    }

    /// <summary>
    /// Unsubscribe from match telemetry.
    /// </summary>
    public async Task UnsubscribeFromMatch(string matchId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, matchId);
    }

    /// <summary>
    /// Subscribe to a player's live data (for the player's own dashboard).
    /// </summary>
    public async Task SubscribeToPlayer(string steamId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"player:{steamId}");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client {ClientId} disconnected", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}

using Microsoft.AspNetCore.Components.Server.Circuits;

namespace BattleshipGame.Services;

/// <summary>
/// Blazor Server circuit handler that detects when a player's browser connection
/// drops and notifies the game session.
///
/// <para>
/// Blazor Server keeps circuits alive for a configurable period after the
/// WebSocket connection is lost (to allow automatic reconnection). This handler
/// fires on the <em>connection</em> events — not the circuit disposal events —
/// so the opponent sees the disconnect banner as quickly as possible.
/// </para>
///
/// <para>
/// Registered as scoped so that each circuit gets its own instance sharing the
/// same <see cref="PlayerCircuitTracker"/> that page components wrote to.
/// </para>
/// </summary>
public class GameCircuitHandler : CircuitHandler
{
    private readonly PlayerCircuitTracker _tracker;
    private readonly GameSessionService _sessionService;

    public GameCircuitHandler(PlayerCircuitTracker tracker, GameSessionService sessionService)
    {
        _tracker = tracker;
        _sessionService = sessionService;
    }

    /// <summary>
    /// Fires when the WebSocket connection to this circuit's browser tab is lost.
    /// Marks the associated player as disconnected so their opponent sees a banner.
    /// </summary>
    public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        TryMarkDisconnected();
        return base.OnConnectionDownAsync(circuit, cancellationToken);
    }

    /// <summary>
    /// Fires when the WebSocket connection is re-established (Blazor auto-reconnect
    /// succeeded — the player's browser reconnected within the retention window).
    /// Clears the disconnected state so both players can continue playing.
    /// </summary>
    public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        TryMarkReconnected();
        return base.OnConnectionUpAsync(circuit, cancellationToken);
    }

    private void TryMarkDisconnected()
    {
        if (_tracker.SessionCode is null || _tracker.PlayerName is null) return;
        var session = _sessionService.GetSession(_tracker.SessionCode);
        session?.MarkPlayerDisconnected(_tracker.PlayerName);
    }

    private void TryMarkReconnected()
    {
        if (_tracker.SessionCode is null || _tracker.PlayerName is null || _tracker.Token is null)
            return;
        var session = _sessionService.GetSession(_tracker.SessionCode);
        session?.MarkPlayerReconnected(_tracker.PlayerName, _tracker.Token);
    }
}

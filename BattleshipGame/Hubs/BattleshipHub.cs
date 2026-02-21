using BattleshipGame.Services;
using Microsoft.AspNetCore.SignalR;

namespace BattleshipGame.Hubs;

/// <summary>
/// SignalR hub used solely for sending targeted push notifications to specific players.
/// All game state lives in <see cref="GameSessionService"/> — the hub is purely a
/// notification bus, keeping game logic fully server-side and testable.
/// </summary>
public class BattleshipHub : Hub
{
    private readonly GameSessionService _sessions;

    public BattleshipHub(GameSessionService sessions)
    {
        _sessions = sessions;
    }

    // Disconnect detection is handled by GameCircuitHandler (a scoped CircuitHandler),
    // which fires on OnConnectionDownAsync / OnConnectionUpAsync within the player's
    // Blazor circuit — no hub-level disconnect logic needed here.
}

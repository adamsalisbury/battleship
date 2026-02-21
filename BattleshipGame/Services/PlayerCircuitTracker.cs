namespace BattleshipGame.Services;

/// <summary>
/// Scoped (per-circuit) service that records which game session and player
/// currently own this Blazor Server circuit.
///
/// <para>
/// Page components inject this service and set its properties in
/// <c>OnInitialized</c>. When the circuit's SignalR connection drops,
/// <see cref="GameCircuitHandler"/> reads these fields to identify which
/// player disconnected and marks them accordingly in the session.
/// </para>
/// </summary>
public class PlayerCircuitTracker
{
    /// <summary>The 4-character game code for the session this circuit is participating in.</summary>
    public string? SessionCode { get; set; }

    /// <summary>The name of the player whose browser tab owns this circuit.</summary>
    public string? PlayerName { get; set; }

    /// <summary>
    /// The session token (matches the URL query param <c>?token=…</c>).
    /// Used by <see cref="GameCircuitHandler"/> to re-authenticate when the
    /// connection comes back up after a brief drop.
    /// </summary>
    public string? Token { get; set; }
}

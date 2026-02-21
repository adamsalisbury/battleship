namespace BattleshipGame.Models;

/// <summary>Represents a player in a game session.</summary>
public class Player
{
    public string Name { get; init; }

    /// <summary>
    /// The session token for this player (matches the URL query param ?token=...).
    /// Used to validate reconnection attempts after a disconnect.
    /// </summary>
    public string ConnectionId { get; set; }

    /// <summary>
    /// Whether this player is the session host (created the game).
    /// The host always takes the first turn.
    /// </summary>
    public bool IsHost { get; init; }

    public PlayerBoard Board { get; } = new();

    // ─── Disconnect tracking ──────────────────────────────────────────────────

    /// <summary>True when the player's Blazor circuit has dropped and not yet recovered.</summary>
    public bool IsDisconnected { get; private set; }

    /// <summary>UTC timestamp of when the player was marked disconnected. Null if not disconnected.</summary>
    public DateTime? DisconnectedAt { get; private set; }

    public Player(string name, string connectionId, bool isHost)
    {
        Name = SanitiseName(name);
        ConnectionId = connectionId;
        IsHost = isHost;
    }

    /// <summary>Marks the player as disconnected and records the time.</summary>
    public void MarkDisconnected()
    {
        IsDisconnected = true;
        DisconnectedAt = DateTime.UtcNow;
    }

    /// <summary>Clears the disconnected state (player has reconnected).</summary>
    public void MarkReconnected()
    {
        IsDisconnected = false;
        DisconnectedAt = null;
    }

    private static string SanitiseName(string raw)
    {
        var trimmed = raw.Trim();
        // Strip any HTML-like tags
        trimmed = System.Text.RegularExpressions.Regex.Replace(trimmed, "<[^>]*>", "");
        trimmed = trimmed.Trim();
        if (trimmed.Length > 20) trimmed = trimmed[..20];
        return string.IsNullOrEmpty(trimmed) ? "Player" : trimmed;
    }
}

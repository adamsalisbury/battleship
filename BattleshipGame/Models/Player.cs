namespace BattleshipGame.Models;

/// <summary>Represents a player in a game session.</summary>
public class Player
{
    public string Name { get; init; }

    /// <summary>SignalR connection ID for this player.</summary>
    public string ConnectionId { get; set; }

    /// <summary>
    /// Whether this player is the session host (created the game).
    /// The host always takes the first turn.
    /// </summary>
    public bool IsHost { get; init; }

    public PlayerBoard Board { get; } = new();

    public Player(string name, string connectionId, bool isHost)
    {
        Name = SanitiseName(name);
        ConnectionId = connectionId;
        IsHost = isHost;
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

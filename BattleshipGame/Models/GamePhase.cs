namespace BattleshipGame.Models;

/// <summary>The current phase of a game session.</summary>
public enum GamePhase
{
    /// <summary>Waiting for the second player to join.</summary>
    Lobby,

    /// <summary>Both players are placing their ships.</summary>
    Placement,

    /// <summary>Active battle — players are taking turns.</summary>
    Battle,

    /// <summary>Game over; winner determined.</summary>
    Finished
}

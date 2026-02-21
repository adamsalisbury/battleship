namespace BattleshipGame.Models;

/// <summary>Represents the shot state of a single cell on a grid.</summary>
public enum CellState
{
    /// <summary>Cell has not been shot at.</summary>
    Unknown,

    /// <summary>Shot fired here — no ship present.</summary>
    Miss,

    /// <summary>Shot fired here — ship present.</summary>
    Hit
}

namespace BattleshipGame.Models;

/// <summary>
/// Represents one player's 10×10 grid: their placed ships and the shot history
/// taken against their grid by the opponent.
/// </summary>
public class PlayerBoard
{
    /// <summary>Ships placed by this player. Empty during Lobby/Placement phases.</summary>
    public List<Ship> Ships { get; } = [];

    /// <summary>
    /// Shot state of each cell — indexed [row, col] (0-based).
    /// Tracks shots taken BY THE OPPONENT against this board.
    /// </summary>
    public CellState[,] ShotGrid { get; } = new CellState[GridConstants.GridSize, GridConstants.GridSize];

    /// <summary>Total number of hits landed on ships on this board.</summary>
    public int TotalHitsReceived { get; private set; }

    /// <summary>Whether the player has confirmed their placement and is ready to fight.</summary>
    public bool IsReady { get; set; }

    /// <summary>True when all ships on this board have been sunk.</summary>
    public bool AllShipsSunk => TotalHitsReceived >= GridConstants.TotalHitsToWin;

    /// <summary>
    /// Attempts to fire a shot at the given cell. Returns null if the cell was
    /// already shot. Otherwise updates state and returns the shot result.
    /// </summary>
    public ShotResult? FireAt(int row, int col)
    {
        if (ShotGrid[row, col] != CellState.Unknown)
            return null;

        var hitShip = Ships.FirstOrDefault(s => s.Cells.Contains((row, col)));
        if (hitShip is null)
        {
            ShotGrid[row, col] = CellState.Miss;
            return ShotResult.Miss;
        }

        ShotGrid[row, col] = CellState.Hit;
        TotalHitsReceived++;
        var sank = hitShip.RecordHit();
        return sank ? ShotResult.Sunk : ShotResult.Hit;
    }

    /// <summary>
    /// Validates and places a ship on this board.
    /// Returns false if the ship overlaps an existing ship or goes out of bounds.
    /// </summary>
    public bool TryPlaceShip(Ship ship)
    {
        if (!ship.FitsOnGrid()) return false;

        var newCells = ship.Cells.ToHashSet();
        var occupied = Ships.SelectMany(s => s.Cells).ToHashSet();
        if (newCells.Overlaps(occupied)) return false;

        Ships.Add(ship);
        return true;
    }

    /// <summary>Removes all placed ships (used during re-placement).</summary>
    public void ClearShips() => Ships.Clear();

    /// <summary>Returns true if this board has all required fleet ships placed.</summary>
    public bool HasFullFleet() =>
        GridConstants.Fleet.All(f => Ships.Any(s => s.Type == f.Type));
}

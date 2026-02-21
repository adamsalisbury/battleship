namespace BattleshipGame.Models;

/// <summary>Represents a single ship placed on a player's grid.</summary>
public class Ship
{
    public ShipType Type { get; init; }

    /// <summary>Size in cells, derived from the ShipType enum value.</summary>
    public int Size => (int)Type;

    public string Name => Type.ToString();

    /// <summary>Whether the ship is oriented horizontally (true) or vertically (false).</summary>
    public bool IsHorizontal { get; set; }

    /// <summary>Top-left origin column (0–9, maps to A–J).</summary>
    public int Column { get; set; }

    /// <summary>Top-left origin row (0–9, maps to 1–10).</summary>
    public int Row { get; set; }

    /// <summary>The set of (row, col) coordinates this ship occupies.</summary>
    public IEnumerable<(int Row, int Col)> Cells =>
        Enumerable.Range(0, Size).Select(i =>
            IsHorizontal ? (Row, Column + i) : (Row + i, Column));

    /// <summary>Hit count — ship is sunk when this equals Size.</summary>
    public int HitCount { get; set; }

    public bool IsSunk => HitCount >= Size;

    /// <summary>Returns true when the ship fits entirely within a 10×10 grid.</summary>
    public bool FitsOnGrid()
    {
        if (Column < 0 || Row < 0) return false;
        if (IsHorizontal) return Column + Size <= GridConstants.GridSize;
        return Row + Size <= GridConstants.GridSize;
    }

    /// <summary>Records a hit on this ship. Returns whether the hit sank it.</summary>
    public bool RecordHit()
    {
        HitCount++;
        return IsSunk;
    }
}

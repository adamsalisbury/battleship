namespace BattleshipGame.Models;

/// <summary>Grid and fleet constants shared across the game.</summary>
public static class GridConstants
{
    public const int GridSize = 10;

    /// <summary>Column labels A–J.</summary>
    public static readonly string[] ColumnLabels = ["A", "B", "C", "D", "E", "F", "G", "H", "I", "J"];

    /// <summary>The standard fleet definition. Order determines placement UI sequence.</summary>
    public static readonly (ShipType Type, string Name)[] Fleet =
    [
        (ShipType.Carrier,    "Carrier"),
        (ShipType.Battleship, "Battleship"),
        (ShipType.Cruiser,    "Cruiser"),
        (ShipType.Submarine,  "Submarine"),
        (ShipType.Destroyer,  "Destroyer")
    ];

    public const int TotalHitsToWin = 17; // 5+4+3+3+2
}

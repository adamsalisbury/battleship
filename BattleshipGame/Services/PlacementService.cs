using BattleshipGame.Models;

namespace BattleshipGame.Services;

/// <summary>
/// Helper service for ship placement operations, including random auto-placement.
/// </summary>
public static class PlacementService
{
    /// <summary>
    /// Generates a valid random placement of the full fleet on a 10×10 grid.
    /// Guarantees no overlaps and all ships fit within bounds.
    /// </summary>
    public static List<Ship> GenerateRandomPlacement()
    {
        List<Ship> placed;
        do
        {
            placed = AttemptRandomPlacement();
        }
        while (placed.Count < GridConstants.Fleet.Length);

        return placed;
    }

    private static List<Ship> AttemptRandomPlacement()
    {
        var placed = new List<Ship>();
        var occupiedCells = new HashSet<(int, int)>();

        foreach (var (type, _) in GridConstants.Fleet)
        {
            var ship = TryPlaceRandomly(type, occupiedCells);
            if (ship is null) return placed; // Failed — caller will retry

            placed.Add(ship);
            foreach (var cell in ship.Cells)
                occupiedCells.Add(cell);
        }

        return placed;
    }

    private static Ship? TryPlaceRandomly(ShipType type, HashSet<(int, int)> occupied)
    {
        const int MaxAttempts = 100;

        for (var attempt = 0; attempt < MaxAttempts; attempt++)
        {
            var isHorizontal = Random.Shared.Next(2) == 0;
            var size = (int)type;

            var row = Random.Shared.Next(GridConstants.GridSize);
            var col = Random.Shared.Next(GridConstants.GridSize);

            var ship = new Ship
            {
                Type = type,
                IsHorizontal = isHorizontal,
                Row = row,
                Column = col
            };

            if (!ship.FitsOnGrid()) continue;

            var cells = ship.Cells.ToHashSet();
            if (cells.Overlaps(occupied)) continue;

            return ship;
        }

        return null;
    }
}

using BattleshipGame.Models;

namespace BattleshipGame.Tests.Models;

public class ShipTests
{
    [Fact]
    public void Size_ReturnsEnumValue()
    {
        var carrier = new Ship { Type = ShipType.Carrier };
        Assert.Equal(5, carrier.Size);

        var destroyer = new Ship { Type = ShipType.Destroyer };
        Assert.Equal(2, destroyer.Size);
    }

    [Fact]
    public void Cells_HorizontalShip_ReturnsCorrectCells()
    {
        var ship = new Ship { Type = ShipType.Destroyer, IsHorizontal = true, Row = 0, Column = 3 };
        var cells = ship.Cells.ToList();

        Assert.Equal(new List<(int Row, int Col)> { (0, 3), (0, 4) }, cells);
    }

    [Fact]
    public void Cells_VerticalShip_ReturnsCorrectCells()
    {
        var ship = new Ship { Type = ShipType.Cruiser, IsHorizontal = false, Row = 2, Column = 5 };
        var cells = ship.Cells.ToList();

        Assert.Equal(new List<(int Row, int Col)> { (2, 5), (3, 5), (4, 5) }, cells);
    }

    [Fact]
    public void FitsOnGrid_HorizontalWithinBounds_ReturnsTrue()
    {
        var ship = new Ship { Type = ShipType.Battleship, IsHorizontal = true, Row = 0, Column = 6 };
        // Column 6 + size 4 = column 10, which is exactly the boundary (0-9)
        Assert.True(ship.FitsOnGrid());
    }

    [Fact]
    public void FitsOnGrid_HorizontalOutOfBounds_ReturnsFalse()
    {
        var ship = new Ship { Type = ShipType.Battleship, IsHorizontal = true, Row = 0, Column = 7 };
        // Column 7 + size 4 = 11, out of bounds
        Assert.False(ship.FitsOnGrid());
    }

    [Fact]
    public void FitsOnGrid_VerticalOutOfBounds_ReturnsFalse()
    {
        var ship = new Ship { Type = ShipType.Carrier, IsHorizontal = false, Row = 8, Column = 0 };
        // Row 8 + size 5 = 13, out of bounds
        Assert.False(ship.FitsOnGrid());
    }

    [Fact]
    public void RecordHit_SinksWhenHitsEqualSize()
    {
        var ship = new Ship { Type = ShipType.Destroyer };
        Assert.False(ship.RecordHit()); // 1st hit — not sunk
        Assert.True(ship.RecordHit());  // 2nd hit — sunk (size=2)
        Assert.True(ship.IsSunk);
    }
}

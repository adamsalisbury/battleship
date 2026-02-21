using BattleshipGame.Models;
using FluentAssertions;

namespace BattleshipGame.Tests.Models;

public class ShipTests
{
    [Fact]
    public void Size_ReturnsEnumValue()
    {
        var carrier = new Ship { Type = ShipType.Carrier };
        carrier.Size.Should().Be(5);

        var destroyer = new Ship { Type = ShipType.Destroyer };
        destroyer.Size.Should().Be(2);
    }

    [Fact]
    public void Cells_HorizontalShip_ReturnsCorrectCells()
    {
        var ship = new Ship { Type = ShipType.Destroyer, IsHorizontal = true, Row = 0, Column = 3 };
        var cells = ship.Cells.ToList();

        cells.Should().BeEquivalentTo(new[] { (0, 3), (0, 4) });
    }

    [Fact]
    public void Cells_VerticalShip_ReturnsCorrectCells()
    {
        var ship = new Ship { Type = ShipType.Cruiser, IsHorizontal = false, Row = 2, Column = 5 };
        var cells = ship.Cells.ToList();

        cells.Should().BeEquivalentTo(new[] { (2, 5), (3, 5), (4, 5) });
    }

    [Fact]
    public void FitsOnGrid_HorizontalWithinBounds_ReturnsTrue()
    {
        var ship = new Ship { Type = ShipType.Battleship, IsHorizontal = true, Row = 0, Column = 6 };
        // Column 6 + size 4 = column 10, which is exactly the boundary (0-9)
        ship.FitsOnGrid().Should().BeTrue();
    }

    [Fact]
    public void FitsOnGrid_HorizontalOutOfBounds_ReturnsFalse()
    {
        var ship = new Ship { Type = ShipType.Battleship, IsHorizontal = true, Row = 0, Column = 7 };
        // Column 7 + size 4 = 11, out of bounds
        ship.FitsOnGrid().Should().BeFalse();
    }

    [Fact]
    public void FitsOnGrid_VerticalOutOfBounds_ReturnsFalse()
    {
        var ship = new Ship { Type = ShipType.Carrier, IsHorizontal = false, Row = 8, Column = 0 };
        // Row 8 + size 5 = 13, out of bounds
        ship.FitsOnGrid().Should().BeFalse();
    }

    [Fact]
    public void RecordHit_SinksWhenHitsEqualSize()
    {
        var ship = new Ship { Type = ShipType.Destroyer };
        ship.RecordHit().Should().BeFalse(); // 1st hit — not sunk
        ship.RecordHit().Should().BeTrue();  // 2nd hit — sunk (size=2)
        ship.IsSunk.Should().BeTrue();
    }
}

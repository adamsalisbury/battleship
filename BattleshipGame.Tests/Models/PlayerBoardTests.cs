using BattleshipGame.Models;
using FluentAssertions;

namespace BattleshipGame.Tests.Models;

public class PlayerBoardTests
{
    private static Ship MakeDestroyerAt(int row, int col, bool horizontal = true) =>
        new Ship { Type = ShipType.Destroyer, Row = row, Column = col, IsHorizontal = horizontal };

    [Fact]
    public void TryPlaceShip_ValidPosition_ReturnsTrue()
    {
        var board = new PlayerBoard();
        var ship = MakeDestroyerAt(0, 0);

        board.TryPlaceShip(ship).Should().BeTrue();
        board.Ships.Should().HaveCount(1);
    }

    [Fact]
    public void TryPlaceShip_OutOfBounds_ReturnsFalse()
    {
        var board = new PlayerBoard();
        var ship = new Ship { Type = ShipType.Destroyer, Row = 0, Column = 9, IsHorizontal = true };

        board.TryPlaceShip(ship).Should().BeFalse();
        board.Ships.Should().BeEmpty();
    }

    [Fact]
    public void TryPlaceShip_Overlap_ReturnsFalse()
    {
        var board = new PlayerBoard();
        board.TryPlaceShip(MakeDestroyerAt(0, 0));

        // Second ship overlaps
        var overlapping = MakeDestroyerAt(0, 1);
        board.TryPlaceShip(overlapping).Should().BeFalse();
        board.Ships.Should().HaveCount(1);
    }

    [Fact]
    public void FireAt_Miss_RecordsAndReturnsMiss()
    {
        var board = new PlayerBoard();
        board.TryPlaceShip(MakeDestroyerAt(3, 3));

        var result = board.FireAt(0, 0);

        result.Should().Be(ShotResult.Miss);
        board.ShotGrid[0, 0].Should().Be(CellState.Miss);
        board.TotalHitsReceived.Should().Be(0);
    }

    [Fact]
    public void FireAt_Hit_RecordsAndReturnsHit()
    {
        var board = new PlayerBoard();
        board.TryPlaceShip(MakeDestroyerAt(0, 0));

        var result = board.FireAt(0, 0);

        result.Should().Be(ShotResult.Hit);
        board.ShotGrid[0, 0].Should().Be(CellState.Hit);
        board.TotalHitsReceived.Should().Be(1);
    }

    [Fact]
    public void FireAt_AlreadyShot_ReturnsNull()
    {
        var board = new PlayerBoard();
        board.FireAt(5, 5);

        var second = board.FireAt(5, 5);
        second.Should().BeNull();
    }

    [Fact]
    public void FireAt_SinksShip_ReturnsSunk()
    {
        var board = new PlayerBoard();
        board.TryPlaceShip(MakeDestroyerAt(0, 0)); // size 2, horizontal at (0,0) and (0,1)

        board.FireAt(0, 0);
        var result = board.FireAt(0, 1);

        result.Should().Be(ShotResult.Sunk);
    }

    [Fact]
    public void AllShipsSunk_WhenAllHitsLanded_ReturnsTrue()
    {
        // Place each fleet ship on a distinct row (vertical, column 0)
        var fullBoard = new PlayerBoard();
        int row = 0;
        foreach (var (type, _) in GridConstants.Fleet)
        {
            var ship = new Ship { Type = type, Row = row, Column = 0, IsHorizontal = true };
            fullBoard.TryPlaceShip(ship).Should().BeTrue($"ship {type} should fit at row {row}");
            row++; // Each ship on its own row; max size is 5, so they never overlap
        }

        // Sink every cell of every ship
        foreach (var ship in fullBoard.Ships)
            foreach (var (r, c) in ship.Cells)
                fullBoard.FireAt(r, c);

        fullBoard.AllShipsSunk.Should().BeTrue();
        fullBoard.TotalHitsReceived.Should().Be(GridConstants.TotalHitsToWin);
    }

    [Fact]
    public void ClearShips_RemovesAllShips()
    {
        var board = new PlayerBoard();
        board.TryPlaceShip(MakeDestroyerAt(0, 0));
        board.ClearShips();

        board.Ships.Should().BeEmpty();
    }
}

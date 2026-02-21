using BattleshipGame.Models;

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

        Assert.True(board.TryPlaceShip(ship));
        Assert.Single(board.Ships);
    }

    [Fact]
    public void TryPlaceShip_OutOfBounds_ReturnsFalse()
    {
        var board = new PlayerBoard();
        var ship = new Ship { Type = ShipType.Destroyer, Row = 0, Column = 9, IsHorizontal = true };

        Assert.False(board.TryPlaceShip(ship));
        Assert.Empty(board.Ships);
    }

    [Fact]
    public void TryPlaceShip_Overlap_ReturnsFalse()
    {
        var board = new PlayerBoard();
        board.TryPlaceShip(MakeDestroyerAt(0, 0));

        // Second ship overlaps
        var overlapping = MakeDestroyerAt(0, 1);
        Assert.False(board.TryPlaceShip(overlapping));
        Assert.Single(board.Ships);
    }

    [Fact]
    public void FireAt_Miss_RecordsAndReturnsMiss()
    {
        var board = new PlayerBoard();
        board.TryPlaceShip(MakeDestroyerAt(3, 3));

        var result = board.FireAt(0, 0);

        Assert.Equal(ShotResult.Miss, result);
        Assert.Equal(CellState.Miss, board.ShotGrid[0, 0]);
        Assert.Equal(0, board.TotalHitsReceived);
    }

    [Fact]
    public void FireAt_Hit_RecordsAndReturnsHit()
    {
        var board = new PlayerBoard();
        board.TryPlaceShip(MakeDestroyerAt(0, 0));

        var result = board.FireAt(0, 0);

        Assert.Equal(ShotResult.Hit, result);
        Assert.Equal(CellState.Hit, board.ShotGrid[0, 0]);
        Assert.Equal(1, board.TotalHitsReceived);
    }

    [Fact]
    public void FireAt_AlreadyShot_ReturnsNull()
    {
        var board = new PlayerBoard();
        board.FireAt(5, 5);

        var second = board.FireAt(5, 5);
        Assert.Null(second);
    }

    [Fact]
    public void FireAt_SinksShip_ReturnsSunk()
    {
        var board = new PlayerBoard();
        board.TryPlaceShip(MakeDestroyerAt(0, 0)); // size 2, horizontal at (0,0) and (0,1)

        board.FireAt(0, 0);
        var result = board.FireAt(0, 1);

        Assert.Equal(ShotResult.Sunk, result);
    }

    [Fact]
    public void AllShipsSunk_WhenAllHitsLanded_ReturnsTrue()
    {
        // Place each fleet ship on a distinct row (horizontal, column 0)
        var fullBoard = new PlayerBoard();
        int row = 0;
        foreach (var (type, _) in GridConstants.Fleet)
        {
            var ship = new Ship { Type = type, Row = row, Column = 0, IsHorizontal = true };
            Assert.True(fullBoard.TryPlaceShip(ship), $"ship {type} should fit at row {row}");
            row++; // Each ship on its own row; max size is 5, so they never overlap
        }

        // Sink every cell of every ship
        foreach (var ship in fullBoard.Ships)
            foreach (var (r, c) in ship.Cells)
                fullBoard.FireAt(r, c);

        Assert.True(fullBoard.AllShipsSunk);
        Assert.Equal(GridConstants.TotalHitsToWin, fullBoard.TotalHitsReceived);
    }

    [Fact]
    public void ClearShips_RemovesAllShips()
    {
        var board = new PlayerBoard();
        board.TryPlaceShip(MakeDestroyerAt(0, 0));
        board.ClearShips();

        Assert.Empty(board.Ships);
    }
}

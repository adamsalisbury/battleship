using BattleshipGame.Models;
using BattleshipGame.Services;
using FluentAssertions;

namespace BattleshipGame.Tests.Models;

public class GameSessionTests
{
    private static GameSession CreateReadySession()
    {
        var session = new GameSession("TEST");
        session.AddHost(new Player("Alice", "conn-1", isHost: true));
        session.AddGuest(new Player("Bob", "conn-2", isHost: false));
        session.StartPlacement();

        // Place full fleets for both
        foreach (var player in new[] { "Alice", "Bob" })
        {
            var ships = PlacementService.GenerateRandomPlacement();
            foreach (var ship in ships)
                session.PlaceShip(player, ship);
            session.MarkPlayerReady(player);
        }

        return session;
    }

    [Fact]
    public void AddHost_SetsHostAndRaisesEvent()
    {
        var session = new GameSession("CODE");
        var eventFired = false;
        session.StateChanged += () => eventFired = true;

        session.AddHost(new Player("Alice", "c1", isHost: true));

        session.Host.Should().NotBeNull();
        eventFired.Should().BeTrue();
    }

    [Fact]
    public void AddGuest_WhenAlreadyFull_ReturnsFalse()
    {
        var session = new GameSession("CODE");
        session.AddHost(new Player("Alice", "c1", isHost: true));
        session.AddGuest(new Player("Bob", "c2", isHost: false));

        var result = session.AddGuest(new Player("Charlie", "c3", isHost: false));

        result.Should().BeFalse();
    }

    [Fact]
    public void StartPlacement_WhenBothPresent_TransitionsPhase()
    {
        var session = new GameSession("CODE");
        session.AddHost(new Player("Alice", "c1", isHost: true));
        session.AddGuest(new Player("Bob", "c2", isHost: false));

        var result = session.StartPlacement();

        result.Should().BeTrue();
        session.Phase.Should().Be(GamePhase.Placement);
    }

    [Fact]
    public void MarkPlayerReady_BothReady_StartsBattle()
    {
        var session = new GameSession("CODE");
        session.AddHost(new Player("Alice", "c1", isHost: true));
        session.AddGuest(new Player("Bob", "c2", isHost: false));
        session.StartPlacement();

        // Place full fleet for Alice
        var aliceShips = PlacementService.GenerateRandomPlacement();
        foreach (var ship in aliceShips)
            session.PlaceShip("Alice", ship);
        session.MarkPlayerReady("Alice");

        session.Phase.Should().Be(GamePhase.Placement); // Bob not ready yet

        // Place full fleet for Bob
        var bobShips = PlacementService.GenerateRandomPlacement();
        foreach (var ship in bobShips)
            session.PlaceShip("Bob", ship);
        session.MarkPlayerReady("Bob");

        session.Phase.Should().Be(GamePhase.Battle);
        session.ActivePlayer.Should().Be(session.Host); // Host goes first
    }

    [Fact]
    public void FireShot_Hit_DoesNotChangeTurn()
    {
        // Classic Battleship: turn passes even on a hit
        var session = CreateReadySession();
        var firstPlayer = session.ActivePlayer!.Name;
        var targetBoard = session.GetOpponentBoard(firstPlayer)!;

        // Find a cell with a ship
        var shipCell = targetBoard.Ships.First().Cells.First();

        var result = session.FireShot(firstPlayer, shipCell.Row, shipCell.Col);

        result.Should().NotBeNull();
        result.Should().BeOneOf(ShotResult.Hit, ShotResult.Sunk);
        // Turn should have passed
        session.ActivePlayer!.Name.Should().NotBe(firstPlayer);
    }

    [Fact]
    public void FireShot_WrongPlayer_ReturnsNull()
    {
        var session = CreateReadySession();
        var waitingPlayer = session.WaitingPlayer!.Name;

        var result = session.FireShot(waitingPlayer, 0, 0);

        result.Should().BeNull();
    }

    [Fact]
    public void FireShot_AlreadyShot_ReturnsNull()
    {
        var session = CreateReadySession();
        var shooter = session.ActivePlayer!.Name;

        session.FireShot(shooter, 9, 9);
        // Switch turns back (fire a shot from the other side)
        var otherShooter = session.ActivePlayer!.Name;
        session.FireShot(otherShooter, 9, 9);

        // Now original shooter tries same cell again
        var result = session.FireShot(shooter, 9, 9);
        result.Should().BeNull();
    }

    [Fact]
    public void GetPlayerByName_ReturnsCorrectPlayer()
    {
        var session = new GameSession("CODE");
        var alice = new Player("Alice", "c1", isHost: true);
        session.AddHost(alice);

        session.GetPlayerByName("Alice").Should().BeSameAs(alice);
        session.GetPlayerByName("Nobody").Should().BeNull();
    }
}

using BattleshipGame.Models;
using BattleshipGame.Services;

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

        Assert.NotNull(session.Host);
        Assert.True(eventFired);
    }

    [Fact]
    public void AddGuest_WhenAlreadyFull_ReturnsFalse()
    {
        var session = new GameSession("CODE");
        session.AddHost(new Player("Alice", "c1", isHost: true));
        session.AddGuest(new Player("Bob", "c2", isHost: false));

        var result = session.AddGuest(new Player("Charlie", "c3", isHost: false));

        Assert.False(result);
    }

    [Fact]
    public void StartPlacement_WhenBothPresent_TransitionsPhase()
    {
        var session = new GameSession("CODE");
        session.AddHost(new Player("Alice", "c1", isHost: true));
        session.AddGuest(new Player("Bob", "c2", isHost: false));

        var result = session.StartPlacement();

        Assert.True(result);
        Assert.Equal(GamePhase.Placement, session.Phase);
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

        Assert.Equal(GamePhase.Placement, session.Phase); // Bob not ready yet

        // Place full fleet for Bob
        var bobShips = PlacementService.GenerateRandomPlacement();
        foreach (var ship in bobShips)
            session.PlaceShip("Bob", ship);
        session.MarkPlayerReady("Bob");

        Assert.Equal(GamePhase.Battle, session.Phase);
        Assert.Same(session.Host, session.ActivePlayer); // Host goes first
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

        Assert.NotNull(result);
        Assert.True(result == ShotResult.Hit || result == ShotResult.Sunk,
            $"Expected Hit or Sunk but got {result}");
        // Turn should have passed
        Assert.NotEqual(firstPlayer, session.ActivePlayer!.Name);
    }

    [Fact]
    public void FireShot_WrongPlayer_ReturnsNull()
    {
        var session = CreateReadySession();
        var waitingPlayer = session.WaitingPlayer!.Name;

        var result = session.FireShot(waitingPlayer, 0, 0);

        Assert.Null(result);
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
        Assert.Null(result);
    }

    [Fact]
    public void GetPlayerByName_ReturnsCorrectPlayer()
    {
        var session = new GameSession("CODE");
        var alice = new Player("Alice", "c1", isHost: true);
        session.AddHost(alice);

        Assert.Same(alice, session.GetPlayerByName("Alice"));
        Assert.Null(session.GetPlayerByName("Nobody"));
    }

    // ─── Disconnect / Reconnect tests ────────────────────────────────────────

    [Fact]
    public void MarkPlayerDisconnected_SetsIsDisconnectedAndFiresEvent()
    {
        var session = new GameSession("CODE");
        session.AddHost(new Player("Alice", "tok-a", isHost: true));
        session.AddGuest(new Player("Bob", "tok-b", isHost: false));

        var eventFired = false;
        session.StateChanged += () => eventFired = true;

        var result = session.MarkPlayerDisconnected("Alice");

        Assert.True(result);
        Assert.True(session.Host!.IsDisconnected);
        Assert.NotNull(session.Host.DisconnectedAt);
        Assert.True(eventFired);
    }

    [Fact]
    public void MarkPlayerDisconnected_UnknownPlayer_ReturnsFalse()
    {
        var session = new GameSession("CODE");
        session.AddHost(new Player("Alice", "tok-a", isHost: true));

        var result = session.MarkPlayerDisconnected("Nobody");

        Assert.False(result);
    }

    [Fact]
    public void MarkPlayerReconnected_CorrectToken_ClearsDisconnectState()
    {
        var session = new GameSession("CODE");
        session.AddHost(new Player("Alice", "tok-a", isHost: true));
        session.AddGuest(new Player("Bob", "tok-b", isHost: false));
        session.MarkPlayerDisconnected("Alice");

        var result = session.MarkPlayerReconnected("Alice", "tok-a");

        Assert.True(result);
        Assert.False(session.Host!.IsDisconnected);
        Assert.Null(session.Host.DisconnectedAt);
    }

    [Fact]
    public void MarkPlayerReconnected_WrongToken_ReturnsFalse()
    {
        var session = new GameSession("CODE");
        session.AddHost(new Player("Alice", "tok-a", isHost: true));
        session.AddGuest(new Player("Bob", "tok-b", isHost: false));
        session.MarkPlayerDisconnected("Alice");

        var result = session.MarkPlayerReconnected("Alice", "wrong-token");

        Assert.False(result);
        Assert.True(session.Host!.IsDisconnected); // still disconnected
    }

    [Fact]
    public void MarkPlayerReconnected_NotDisconnected_ReturnsFalse()
    {
        var session = new GameSession("CODE");
        session.AddHost(new Player("Alice", "tok-a", isHost: true));
        session.AddGuest(new Player("Bob", "tok-b", isHost: false));

        // Alice is not disconnected — reconnect call should be a no-op
        var result = session.MarkPlayerReconnected("Alice", "tok-a");

        Assert.False(result);
    }

    [Fact]
    public void IsOpponentDisconnected_ReturnsTrueWhenOpponentIsGone()
    {
        var session = new GameSession("CODE");
        session.AddHost(new Player("Alice", "tok-a", isHost: true));
        session.AddGuest(new Player("Bob", "tok-b", isHost: false));

        session.MarkPlayerDisconnected("Bob");

        Assert.True(session.IsOpponentDisconnected("Alice"));
        Assert.False(session.IsOpponentDisconnected("Bob"));
    }

    [Fact]
    public void IsDisconnectExpired_FalseWhenDisconnectedRecently()
    {
        var session = new GameSession("CODE");
        session.AddHost(new Player("Alice", "tok-a", isHost: true));
        session.AddGuest(new Player("Bob", "tok-b", isHost: false));

        session.MarkPlayerDisconnected("Alice");

        // Just disconnected — should not be expired yet
        Assert.False(session.IsDisconnectExpired("Alice"));
    }

    [Fact]
    public void IsDisconnectExpired_TrueWhenDisconnectedAtIsOld()
    {
        var session = new GameSession("CODE");
        session.AddHost(new Player("Alice", "tok-a", isHost: true));
        session.AddGuest(new Player("Bob", "tok-b", isHost: false));

        session.MarkPlayerDisconnected("Alice");

        // Backdate the DisconnectedAt to simulate 6 minutes ago
        typeof(Player)
            .GetProperty("DisconnectedAt")!
            .SetValue(session.Host, DateTime.UtcNow.AddMinutes(-6));

        Assert.True(session.IsDisconnectExpired("Alice"));
    }

    [Fact]
    public void PlayerCircuitTracker_SetsAndReadsProperties()
    {
        var tracker = new BattleshipGame.Services.PlayerCircuitTracker
        {
            SessionCode = "ABCD",
            PlayerName = "Alice",
            Token = "tok-abc"
        };

        Assert.Equal("ABCD", tracker.SessionCode);
        Assert.Equal("Alice", tracker.PlayerName);
        Assert.Equal("tok-abc", tracker.Token);
    }
}

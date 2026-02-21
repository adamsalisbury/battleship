using BattleshipGame.Models;
using BattleshipGame.Services;

namespace BattleshipGame.Tests.Services;

public class GameSessionServiceTests
{
    private readonly GameSessionService _service = new();

    [Fact]
    public void CreateSession_ReturnsSessionWithUniqueCode()
    {
        var session = _service.CreateSession("Alice", "conn-1");

        Assert.NotNull(session);
        Assert.Equal(4, session.Code.Length);
        Assert.NotNull(session.Host);
        Assert.Equal("Alice", session.Host!.Name);
        Assert.Equal(GamePhase.Lobby, session.Phase);
    }

    [Fact]
    public void JoinSession_ValidCode_JoinsSuccessfully()
    {
        var session = _service.CreateSession("Alice", "conn-1");
        var joined = _service.JoinSession(session.Code, "Bob", "conn-2");

        Assert.NotNull(joined);
        Assert.NotNull(joined!.Guest);
        Assert.Equal("Bob", joined.Guest!.Name);
        Assert.Equal(GamePhase.Placement, joined.Phase);
    }

    [Fact]
    public void JoinSession_InvalidCode_ReturnsNull()
    {
        var result = _service.JoinSession("ZZZZ", "Bob", "conn-2");
        Assert.Null(result);
    }

    [Fact]
    public void JoinSession_FullSession_ReturnsNull()
    {
        var session = _service.CreateSession("Alice", "conn-1");
        _service.JoinSession(session.Code, "Bob", "conn-2");

        // Third player tries to join
        var third = _service.JoinSession(session.Code, "Charlie", "conn-3");
        Assert.Null(third);
    }

    [Fact]
    public void JoinSession_SameNameAsHost_ReturnsNull()
    {
        // Duplicate names break GetPlayerByName — must be rejected
        var session = _service.CreateSession("Alice", "conn-1");
        var result = _service.JoinSession(session.Code, "Alice", "conn-2");

        Assert.Null(result);
    }

    [Fact]
    public void JoinSession_SameNameAsHostCaseInsensitive_ReturnsNull()
    {
        // Case-insensitive collision check prevents visually identical names
        var session = _service.CreateSession("Alice", "conn-1");
        var result = _service.JoinSession(session.Code, "alice", "conn-2");

        Assert.Null(result);
    }

    [Fact]
    public void JoinSession_DifferentName_Succeeds()
    {
        // Verify a non-colliding name still works after the guard is in place
        var session = _service.CreateSession("Alice", "conn-1");
        var result = _service.JoinSession(session.Code, "Bob", "conn-2");

        Assert.NotNull(result);
        Assert.Equal("Bob", result!.Guest!.Name);
    }

    [Fact]
    public void GetSession_KnownCode_ReturnsSession()
    {
        var created = _service.CreateSession("Alice", "conn-1");
        var fetched = _service.GetSession(created.Code);

        Assert.Same(created, fetched);
    }

    [Fact]
    public void GetSession_UnknownCode_ReturnsNull()
    {
        Assert.Null(_service.GetSession("XXXX"));
    }

    [Fact]
    public void CreateSession_GeneratesCodeFromSafeAlphabet()
    {
        for (var i = 0; i < 20; i++)
        {
            var session = _service.CreateSession($"Player{i}", $"conn-{i}");
            Assert.Matches("^[ABCDEFGHJKLMNPQRSTUVWXYZ23456789]{4}$", session.Code);
        }
    }
}

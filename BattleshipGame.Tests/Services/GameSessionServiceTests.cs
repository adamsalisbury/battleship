using BattleshipGame.Models;
using BattleshipGame.Services;
using FluentAssertions;

namespace BattleshipGame.Tests.Services;

public class GameSessionServiceTests
{
    private readonly GameSessionService _service = new();

    [Fact]
    public void CreateSession_ReturnsSessionWithUniqueCode()
    {
        var session = _service.CreateSession("Alice", "conn-1");

        session.Should().NotBeNull();
        session.Code.Should().HaveLength(4);
        session.Host.Should().NotBeNull();
        session.Host!.Name.Should().Be("Alice");
        session.Phase.Should().Be(GamePhase.Lobby);
    }

    [Fact]
    public void JoinSession_ValidCode_JoinsSuccessfully()
    {
        var session = _service.CreateSession("Alice", "conn-1");
        var joined = _service.JoinSession(session.Code, "Bob", "conn-2");

        joined.Should().NotBeNull();
        joined!.Guest.Should().NotBeNull();
        joined.Guest!.Name.Should().Be("Bob");
        joined.Phase.Should().Be(GamePhase.Placement);
    }

    [Fact]
    public void JoinSession_InvalidCode_ReturnsNull()
    {
        var result = _service.JoinSession("ZZZZ", "Bob", "conn-2");
        result.Should().BeNull();
    }

    [Fact]
    public void JoinSession_FullSession_ReturnsNull()
    {
        var session = _service.CreateSession("Alice", "conn-1");
        _service.JoinSession(session.Code, "Bob", "conn-2");

        // Third player tries to join
        var third = _service.JoinSession(session.Code, "Charlie", "conn-3");
        third.Should().BeNull();
    }

    [Fact]
    public void GetSession_KnownCode_ReturnsSession()
    {
        var created = _service.CreateSession("Alice", "conn-1");
        var fetched = _service.GetSession(created.Code);

        fetched.Should().BeSameAs(created);
    }

    [Fact]
    public void GetSession_UnknownCode_ReturnsNull()
    {
        _service.GetSession("XXXX").Should().BeNull();
    }

    [Fact]
    public void CreateSession_GeneratesCodeFromSafeAlphabet()
    {
        for (var i = 0; i < 20; i++)
        {
            var session = _service.CreateSession($"Player{i}", $"conn-{i}");
            session.Code.Should().MatchRegex("^[ABCDEFGHJKLMNPQRSTUVWXYZ23456789]{4}$");
        }
    }
}

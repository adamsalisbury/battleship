using BattleshipGame.Models;
using System.Collections.Concurrent;

namespace BattleshipGame.Services;

/// <summary>
/// Singleton service that owns all active <see cref="GameSession"/> instances.
/// Handles session creation, joining, and cleanup.
/// </summary>
public class GameSessionService
{
    private readonly ConcurrentDictionary<string, GameSession> _sessions = new();

    // Code alphabet: no O, 0, I, 1 to avoid visual ambiguity
    private const string CodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private const int CodeLength = 4;

    // Sessions idle for more than 2 hours are eligible for cleanup
    private static readonly TimeSpan SessionExpiry = TimeSpan.FromHours(2);

    // ─── Session creation ─────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new game session with a unique 4-character code.
    /// Returns the created session and sets the host player.
    /// </summary>
    public GameSession CreateSession(string hostName, string connectionId)
    {
        var code = GenerateUniqueCode();
        var session = new GameSession(code);
        var host = new Player(hostName, connectionId, isHost: true);

        _sessions[code] = session;
        session.AddHost(host);

        return session;
    }

    /// <summary>
    /// Joins an existing session by code. Returns the session on success,
    /// or null if the code is invalid / session is full.
    /// </summary>
    public GameSession? JoinSession(string code, string guestName, string connectionId)
    {
        var normalised = code.Trim().ToUpperInvariant();
        if (!_sessions.TryGetValue(normalised, out var session))
            return null;

        if (session.IsFull) return null;
        if (session.Phase != GamePhase.Lobby) return null;

        var guest = new Player(guestName, connectionId, isHost: false);
        var added = session.AddGuest(guest);
        if (!added) return null;

        // Automatically move to Placement once both players are present
        session.StartPlacement();

        return session;
    }

    /// <summary>Retrieves a session by code. Returns null if not found.</summary>
    public GameSession? GetSession(string code) =>
        _sessions.TryGetValue(code.Trim().ToUpperInvariant(), out var s) ? s : null;

    /// <summary>Removes expired sessions from the registry.</summary>
    public void PurgeExpiredSessions()
    {
        foreach (var (code, session) in _sessions)
        {
            if (session.IsExpired(SessionExpiry))
                _sessions.TryRemove(code, out _);
        }
    }

    // ─── Code generation ─────────────────────────────────────────────────────

    private string GenerateUniqueCode()
    {
        string code;
        do
        {
            code = GenerateCode();
        }
        while (_sessions.ContainsKey(code));
        return code;
    }

    private static string GenerateCode()
    {
        var chars = new char[CodeLength];
        for (var i = 0; i < CodeLength; i++)
            chars[i] = CodeAlphabet[Random.Shared.Next(CodeAlphabet.Length)];
        return new string(chars);
    }
}

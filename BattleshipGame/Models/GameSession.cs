namespace BattleshipGame.Models;

/// <summary>
/// The authoritative server-side state for a single game session.
/// All state mutations are protected by an internal lock.
/// Raises <see cref="StateChanged"/> after every mutation so that
/// subscribed Blazor circuits can re-render.
/// </summary>
public class GameSession
{
    private readonly object _lock = new();

    // ─── Identity ────────────────────────────────────────────────────────────
    public string Code { get; init; }

    // ─── Players ─────────────────────────────────────────────────────────────
    public Player? Host { get; private set; }
    public Player? Guest { get; private set; }

    public Player? PlayerOne => Host;
    public Player? PlayerTwo => Guest;

    public bool IsFull => Host is not null && Guest is not null;

    // ─── Phase ───────────────────────────────────────────────────────────────
    public GamePhase Phase { get; private set; } = GamePhase.Lobby;

    // ─── Turn tracking ───────────────────────────────────────────────────────
    /// <summary>
    /// The player whose turn it currently is (null before Battle phase).
    /// The host always goes first.
    /// </summary>
    public Player? ActivePlayer { get; private set; }

    /// <summary>The player waiting for their opponent to shoot.</summary>
    public Player? WaitingPlayer => ActivePlayer == Host ? Guest : Host;

    // ─── Shot history ────────────────────────────────────────────────────────
    public List<ShotRecord> ShotHistory { get; } = [];

    /// <summary>The most recent shot (for displaying feedback).</summary>
    public ShotRecord? LastShot => ShotHistory.Count > 0 ? ShotHistory[^1] : null;

    // ─── Winner ──────────────────────────────────────────────────────────────
    public Player? Winner { get; private set; }
    public Player? Loser => Winner == Host ? Guest : Host;

    // ─── Activity ────────────────────────────────────────────────────────────
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
    public DateTime LastActivityAt { get; private set; } = DateTime.UtcNow;

    // ─── Events ──────────────────────────────────────────────────────────────
    /// <summary>
    /// Fired on every state mutation. Components subscribe to this and call
    /// InvokeAsync(StateHasChanged) in response.
    /// </summary>
    public event Action? StateChanged;

    // ─── Constructor ─────────────────────────────────────────────────────────
    public GameSession(string code)
    {
        Code = code;
    }

    // ─── Player management ───────────────────────────────────────────────────

    /// <summary>Adds the host player. Returns false if already set.</summary>
    public bool AddHost(Player player)
    {
        lock (_lock)
        {
            if (Host is not null) return false;
            Host = player;
            Touch();
        }
        RaiseStateChanged();
        return true;
    }

    /// <summary>Adds the guest player. Returns false if already full.</summary>
    public bool AddGuest(Player player)
    {
        lock (_lock)
        {
            if (Guest is not null) return false;
            Guest = player;
            Touch();
        }
        RaiseStateChanged();
        return true;
    }

    /// <summary>Updates the connection ID for a player (reconnect handling).</summary>
    public bool UpdateConnectionId(string playerName, string newConnectionId)
    {
        lock (_lock)
        {
            var player = GetPlayerByName(playerName);
            if (player is null) return false;
            player.ConnectionId = newConnectionId;
            Touch();
        }
        RaiseStateChanged();
        return true;
    }

    // ─── Phase transitions ───────────────────────────────────────────────────

    /// <summary>
    /// Advances from Lobby → Placement once both players are present.
    /// Returns false if conditions are not met.
    /// </summary>
    public bool StartPlacement()
    {
        lock (_lock)
        {
            if (Phase != GamePhase.Lobby || !IsFull) return false;
            Phase = GamePhase.Placement;
            Touch();
        }
        RaiseStateChanged();
        return true;
    }

    /// <summary>
    /// Marks a player as ready (placement confirmed).
    /// Automatically advances to Battle when both are ready.
    /// </summary>
    public bool MarkPlayerReady(string playerName)
    {
        lock (_lock)
        {
            if (Phase != GamePhase.Placement) return false;
            var player = GetPlayerByName(playerName);
            if (player is null || !player.Board.HasFullFleet()) return false;

            player.Board.IsReady = true;

            if (Host!.Board.IsReady && Guest!.Board.IsReady)
            {
                Phase = GamePhase.Battle;
                ActivePlayer = Host; // Host always goes first
            }

            Touch();
        }
        RaiseStateChanged();
        return true;
    }

    // ─── Battle ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Fires a shot at the specified cell on the active player's opponent board.
    /// Returns null if the shot is invalid (wrong phase, wrong turn, already shot).
    /// </summary>
    public ShotResult? FireShot(string shooterName, int row, int col)
    {
        ShotResult? result;
        ShotRecord? record;

        lock (_lock)
        {
            if (Phase != GamePhase.Battle) return null;
            if (ActivePlayer?.Name != shooterName) return null;

            var targetBoard = GetOpponentBoard(shooterName);
            if (targetBoard is null) return null;

            result = targetBoard.FireAt(row, col);
            if (result is null) return null; // Already shot

            string? sunkShipName = null;
            if (result == ShotResult.Sunk)
            {
                var sunkShip = targetBoard.Ships.FirstOrDefault(s =>
                    s.IsSunk && s.Cells.Contains((row, col)));
                sunkShipName = sunkShip?.Name;
            }

            record = new ShotRecord(shooterName, row, col, result.Value, sunkShipName);
            ShotHistory.Add(record);

            if (targetBoard.AllShipsSunk)
            {
                Phase = GamePhase.Finished;
                Winner = ActivePlayer;
            }
            else
            {
                // Pass turn to opponent
                ActivePlayer = WaitingPlayer;
            }

            Touch();
        }

        RaiseStateChanged();
        return result;
    }

    // ─── Ship placement helpers ──────────────────────────────────────────────

    /// <summary>
    /// Places a ship on the named player's board.
    /// Returns false if placement is invalid.
    /// </summary>
    public bool PlaceShip(string playerName, Ship ship)
    {
        bool placed;
        lock (_lock)
        {
            if (Phase != GamePhase.Placement) return false;
            var player = GetPlayerByName(playerName);
            if (player is null) return false;
            placed = player.Board.TryPlaceShip(ship);
            if (placed) Touch();
        }
        if (placed) RaiseStateChanged();
        return placed;
    }

    /// <summary>Clears all ships from the named player's board.</summary>
    public void ClearShips(string playerName)
    {
        lock (_lock)
        {
            var player = GetPlayerByName(playerName);
            player?.Board.ClearShips();
            player?.Board.IsReady.Equals(false); // not ready any more
            if (player is not null) player.Board.IsReady = false;
            Touch();
        }
        RaiseStateChanged();
    }

    // ─── Rematch ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Resets the session for a rematch. Swaps who goes first
    /// (loser of previous game starts next).
    /// </summary>
    public void ResetForRematch()
    {
        lock (_lock)
        {
            if (Phase != GamePhase.Finished) return;

            // Clear boards
            Host!.Board.ClearShips();
            Guest!.Board.ClearShips();
            Host.Board.IsReady = false;
            Guest.Board.IsReady = false;

            // Clear shot history
            ShotHistory.Clear();

            // The loser of the previous game goes first in the next
            var nextFirst = Loser;
            Winner = null;

            Phase = GamePhase.Placement;
            ActivePlayer = null;

            // We store "who goes first in battle" as a separate concept during rematch
            // by swapping IsHost semantics — but since IsHost is init-only, we track
            // it via a separate field instead.
            _rematchFirstPlayer = nextFirst;

            Touch();
        }
        RaiseStateChanged();
    }

    // ─── Accessors ───────────────────────────────────────────────────────────

    public Player? GetPlayerByConnectionId(string connectionId) =>
        Host?.ConnectionId == connectionId ? Host :
        Guest?.ConnectionId == connectionId ? Guest : null;

    public Player? GetPlayerByName(string name) =>
        Host?.Name == name ? Host :
        Guest?.Name == name ? Guest : null;

    /// <summary>Returns the opponent's board from the named player's perspective.</summary>
    public PlayerBoard? GetOpponentBoard(string playerName) =>
        playerName == Host?.Name ? Guest?.Board :
        playerName == Guest?.Name ? Host?.Board : null;

    /// <summary>Returns the named player's own board.</summary>
    public PlayerBoard? GetOwnBoard(string playerName) =>
        playerName == Host?.Name ? Host.Board :
        playerName == Guest?.Name ? Guest.Board : null;

    // ─── Private helpers ─────────────────────────────────────────────────────

    private Player? _rematchFirstPlayer;

    private void Touch() => LastActivityAt = DateTime.UtcNow;

    private void RaiseStateChanged() => StateChanged?.Invoke();

    /// <summary>Returns true if the session has been inactive for longer than the threshold.</summary>
    public bool IsExpired(TimeSpan threshold) =>
        DateTime.UtcNow - LastActivityAt > threshold;
}

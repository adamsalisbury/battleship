# Backlog — Technical / Quality Work

## Outstanding

- **Replace FluentAssertions with xUnit-native assertions** — FA 8.x now requires a commercial license for commercial use. Swap to `Assert.Equal`, `Assert.True`, etc. from xUnit, or pin to FA 6.x (last free version under LGPL). Evaluate before publishing.

- **GameSession.ResetForRematch — rematch first-player logic incomplete** — `_rematchFirstPlayer` field is set but never used when transitioning to Battle on the second game. Complete the logic to use it when calling `MarkPlayerReady` in rematch scenarios.

- **Player name collision handling** — If both players enter the same name, `GetPlayerByName` returns the first match. Add validation in `JoinSession` to reject duplicate names.

- **ClearShips double-negation bug** — In `GameSession.ClearShips`, the line `player?.Board.IsReady.Equals(false)` is a no-op (calling Equals without assigning). Already patched on the next line but worth cleaning up.

- **FluentAssertions commercial licence warning** — Evaluate alternative assertion library (e.g., Shouldly, or drop FA).

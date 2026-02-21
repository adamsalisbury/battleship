# Backlog — Technical / Quality Work

## Outstanding

- **Replace FluentAssertions with xUnit-native assertions** — FA 8.x now requires a commercial license for commercial use. Swap to `Assert.Equal`, `Assert.True`, etc. from xUnit, or pin to FA 6.x (last free version under LGPL). Evaluate before publishing.

- **Player name collision handling** — If both players enter the same name, `GetPlayerByName` returns the first match. Add validation in `JoinSession` to reject duplicate names.

- **ClearShips double-negation bug** — In `GameSession.ClearShips`, the line `player?.Board.IsReady.Equals(false)` is a no-op (calling Equals without assigning). Already patched on the next line but worth cleaning up.

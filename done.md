# Done — Chronological Log

## Iteration 1 — Project Bootstrap & Lobby Flow
**Date:** 2026-02-21

### What was done
- Designed full Battleship game: rules, fleet (Carrier 5, Battleship 4, Cruiser 3, Submarine 3, Destroyer 2), 10×10 grid, 1-shot-per-turn, host goes first.
- Scaffolded Blazor Server project (`BattleshipGame`) + xUnit test project (`BattleshipGame.Tests`) in a solution (`Battleship.sln`).
- Built core models: `Ship`, `PlayerBoard`, `Player`, `GameSession`, `GridConstants`, `CellState`, `GamePhase`, `ShipType`, `ShotResult`, `ShotRecord`.
- Built `GameSessionService` (singleton, ConcurrentDictionary, 4-char game codes with safe alphabet).
- Built `PlacementService` (random auto-placement for full fleet).
- Built `SessionCleanupService` (background service, 30-min interval, 2-hour session expiry).
- Wired up `BattleshipHub` (SignalR notification hub).
- Built `Home.razor` — landing page with name entry, New Game / Join Game flow.
- Built `Lobby.razor` — waiting room showing both player names, 4-char game code, real-time update via `StateChanged` event.
- Built `Placement.razor` — status panel, auto-place-and-ready stub (full UI in iteration 2).
- Built `Battle.razor` — turn indicator and grid layout stubs (full UI in iteration 3).
- Written 31 unit tests covering Ship, PlayerBoard, GameSession, GameSessionService — all passing.
- Full dark naval theme CSS (`app.css`) with design tokens, animations, responsive components.

### Notes
- FluentAssertions 8.x now requires commercial licence — noted in technical backlog for resolution.
- GameSession._rematchFirstPlayer set but not yet wired into battle start logic (noted in technical backlog).
- Two players can connect, see each other's names in the lobby, and reach the placement stub.
- Build: 0 warnings, 0 errors. Tests: 31/31 passing.

---

## Iteration 2 — Ship Placement UI
**Date:** 2026-02-21

### What was done
- Added `PlayerBoard.RemoveShip(ShipType)` — removes a single ship type without clearing the full board.
- Added `GameSession.RemoveShip(playerName, shipType)` — thread-safe wrapper; fires `StateChanged` on success.
- Rewrote `Placement.razor` with fully interactive placement UI:
  - 10×10 CSS grid with column labels (A–J) and row labels (1–10).
  - **Fleet panel** (right column): lists all 5 ships with pip indicators coloured by ship type (Carrier=blue, Battleship=teal, Cruiser=amber, Submarine=purple, Destroyer=red). Shows Placed / Placing / Not placed badge per ship.
  - **Select & place**: click a ship in the fleet panel → ship is "selected"; hover over the grid → colour-coded preview (green = valid, red = invalid); click to place.
  - **Re-placement**: clicking a placed ship removes it and re-selects it for placement.
  - **Orientation toggle**: Horizontal / Vertical buttons; active state highlighted in blue.
  - **Randomise**: clears board and auto-places full fleet via `PlacementService`.
  - **Clear**: resets board (disabled if board already empty).
  - **Ready**: enabled only when all 5 ships are placed; calls `MarkPlayerReady` which advances to Battle when both players are ready.
  - **Waiting spinner**: displayed instead of controls once player is ready.
  - Opponent "Placing… / ✓ Ready" status updates in real time via `StateChanged` event.
- Added ship-colour CSS design tokens (`--ship-carrier`, `--ship-battleship`, etc.).
- Added full placement-screen CSS: game grid, cell states (empty hover, preview valid/invalid, ship colours with hover lightening via `color-mix()`), fleet panel, fleet items, orientation buttons, ready button.
- Build: 0 warnings, 0 errors. Tests: 31/31 passing.

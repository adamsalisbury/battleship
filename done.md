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

---

## Iteration 3 — Battle Grid UI
**Date:** 2026-02-21

### What was done
- Rewrote `Battle.razor` with fully interactive dual-grid battle UI:
  - **Own grid (Your Waters)**: 10×10 grid showing own ship placements + opponent's shots.
    - Untouched ship cells: coloured by ship type (reusing `cell-ship-{name}` CSS).
    - Opponent misses: grey dot marker (`.cell-own-miss` via `::after` pseudo-element).
    - Opponent hits: solid red background + ✕ cross marker (`.cell-own-hit`).
    - Sunk ships: darker red + ✕ marker (`.cell-own-sunk`).
    - No click handler — cursor set to `default` via `.battle-own-grid .grid-cell` override.
  - **Enemy grid (Enemy Waters)**: 10×10 grid tracking shots taken at opponent.
    - Unknown cells: dark panel colour; on your turn + hover → blue glow (`.cell-enemy-targetable`).
    - Misses: grey dot marker (`.cell-enemy-miss`).
    - Hits (alive ships): red background + ✕ cross (`.cell-enemy-hit`).
    - Sunk ships revealed: ship colour + red inset shadow + ✕ (`.cell-enemy-sunk cell-ship-{name}`).
  - Click fires a shot via `_session.FireShot(playerName, row, col)` with guards:
    - Must be player's turn, must be Battle phase, cell must be Unknown.
  - **Shot result toast**: fixed-position notification (`.shot-toast`) auto-dismisses after 3.5 s.
    - 💧 Miss (grey) / 🔥 Hit (orange) / 💥 Sunk + ship name (red).
    - Timer uses `System.Threading.Timer` + `InvokeAsync` for thread-safe dismiss.
  - **Turn indicator**: shows "Your turn — pick a target" vs "Opponent is aiming…" vs "Battle Over".
  - **Game over overlay**: existing result-overlay shown when Phase == Finished.
- Added `position: relative` to `.grid-cell` (required for `::after` pseudo-element markers).
- Added ~150 lines of battle-specific CSS:
  - `.battle-own-grid` and `.battle-enemy-grid` grid-level cursor overrides.
  - All cell state classes with pseudo-element markers.
  - Toast styles (`.shot-toast`, `.toast-miss`, `.toast-hit`, `.toast-sunk`) with `toastSlideIn` keyframe.
- Build: 0 warnings, 0 errors. Tests: 31/31 passing.

---

## Iteration 4 — Fleet Status Sidebar
**Date:** 2026-02-21

### What was done
- Added a compact fleet status panel below each 10×10 battle grid:
  - **Own fleet panel**: lists all 5 ships in GridConstants.Fleet order. Each row shows a coloured identity dot, ship name (fixed 86px width for alignment), damage pips (ship colour = intact, red = hit), and a status badge (Afloat / Damaged / Sunk). Damaged rows get a subtle amber background tint; Sunk rows are dimmed to 55% opacity.
  - **Enemy fleet panel**: lists all 5 fleet ship types. Each row shows identity dot, ship name, pips (ship colour = active, dark red = sunk), and a status badge (Active / Sunk). Only confirms sunk status — does not reveal hit count on unsunk ships.
- Added helper methods to Battle.razor:
  - `GetOwnFleetOrdered()` — returns own ships in fleet definition order via LINQ + OfType<Ship>().
  - `GetOwnShipStatusClass(Ship)` / `GetOwnShipStatusLabel(Ship)` — switch expressions on IsSunk / HitCount.
  - `IsEnemyShipSunk(ShipType)` — null-safe check on opponent board's ship list.
- Added ~85 lines of CSS: `.fleet-status-panel`, `.fleet-status-title`, `.fleet-status-row`, `.fleet-status-color-dot`, `.color-dot-{name}`, `.fleet-status-name`, `.fleet-status-pips`, `.pip-hit`, `.pip-sunk`, `.fleet-status-badge`, `.badge-{afloat|damaged|sunk|active}`, row state modifiers.
- Reused existing `.fleet-pip` size class (16×10 px) from placement CSS for visual consistency.
- Build: 0 warnings, 0 errors. Tests: 31/31 passing.

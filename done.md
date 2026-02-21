# Done — Chronological Log

## Iteration 9 — Session Expiry UX
**Date:** 2026-02-21

### What was done
- **Bug fixed**: Navigating to `/placement/{CODE}` or `/battle/{CODE}` with an expired session code previously showed a permanent loading spinner. Root cause: the `@if (_session is null)` condition matched before `_error is not null` was checked, hiding the error message entirely.
- **`SessionExpiredCard.razor`** (new, `Components/Shared/`): Shared component rendering a full-screen centred card with an anchor SVG icon, a bold `Title`, a descriptive `Message`, an optional italic `Detail` line, and a primary "⊕ Start a New Game" CTA linking to `/`. Inherits render mode from the host page (no `@rendermode` needed).
- **`_Imports.razor`**: Added `@using BattleshipGame.Components.Shared` so `SessionExpiredCard` is available to all pages without per-file `@using` directives.
- **`Placement.razor`**: Restructured from nested `@if`/`else if`/`else` inside `.placement-screen` to a clean top-level three-way branch: error → `SessionExpiredCard`; loading → spinner; session ready → full `.placement-screen`. Prevents the `.placement-screen` div (with `min-height: 100vh`) from rendering alongside the error card.
- **`Battle.razor`**: Same top-level restructure. Added `_sessionNotFound` bool: when `true`, card shows "Game Not Found" + expiry info; when `false` (wrong phase), card shows "Game Unavailable" with context-appropriate detail.
- **`Lobby.razor`**: Join failure (`JoinSession` returns null) now correctly shows `SessionExpiredCard` instead of being consumed by the null-session loading branch. Restructured to top-level `@if (_error is not null)` / `else if (_session is null)` / `else` pattern.
- **`Home.razor`**: Join error message for `GetSession` returning null updated to mention session expiry: "the session may have expired (sessions last 2 hours)".
- **`app.css`**: New `.expired-screen` (full-page flex centring), `.session-expired-card` (max-width card with `fadeSlideDown` animation), `.expired-anchor-icon`, `.expired-title`, `.expired-message`, `.expired-detail` (indented info box), `.expired-actions`, `.expired-cta` (min-width button).

### Notes
- Build: 0 warnings, 0 errors. Tests: 40/40 passing.
- `SessionExpiredCard` is purely presentational — no game logic, no SignalR. Parameters: `Title`, `Message`, `Detail` (all optional with sensible defaults).

---

## Iteration 8 — Disconnect / Reconnect Handling
**Date:** 2026-02-21

### What was done
- **Player model**: Added `IsDisconnected` (bool) and `DisconnectedAt` (nullable DateTime) properties, plus `MarkDisconnected()` and `MarkReconnected()` methods.
- **GameSession**: Added `MarkPlayerDisconnected(playerName)` — marks disconnected and fires `StateChanged`; `MarkPlayerReconnected(playerName, token)` — validates token before clearing (prevents slot hijacking); `IsPlayerDisconnected`, `IsOpponentDisconnected`, `IsDisconnectExpired` helpers (5-minute reconnect window).
- **PlayerCircuitTracker** (new, scoped): Stores `SessionCode`, `PlayerName`, `Token` for the current Blazor circuit. Page components write to it in `OnInitialized`; the circuit handler reads it to identify which player disconnected.
- **GameCircuitHandler** (new, scoped `CircuitHandler`): Subscribes to `OnConnectionDownAsync` (marks player disconnected immediately when WebSocket drops) and `OnConnectionUpAsync` (clears disconnect state when Blazor auto-reconnect succeeds). Fires before the 30-second circuit retention timeout.
- **Program.cs**: Registered `PlayerCircuitTracker` (scoped) and `GameCircuitHandler` (scoped). Configured `DisconnectedCircuitRetentionPeriod = 30 s` and `DisconnectedCircuitMaxRetained = 20` so auto-reconnect attempts are brief; manual reconnect via URL is valid for 5 minutes.
- **BattleshipHub**: Removed stale TODO comment; disconnect now fully handled via `CircuitHandler`.
- **Lobby.razor**: Sets `CircuitTracker` fields on init so the handler can identify the player if they disconnect during the lobby.
- **Placement.razor**: Registers circuit tracker; calls `MarkPlayerReconnected` on init if the returning player was previously disconnected. Shows animated disconnect banner (📡 orange/info style) with "expired" variant (⚠️ red) and Back to Home link.
- **Battle.razor**: Same circuit tracker + reconnect logic as Placement. Turn indicator shows "⏸️ Game paused" state. `OnEnemyCellClick` blocks shots while opponent is disconnected. `GetEnemyCellClass` suppresses targetable hover cells while paused.
- **app.css**: `.disconnect-banner`, `.disconnect-expired`, `.disconnect-icon`, `.disconnect-msg`, `.disconnect-home-btn` + `@keyframes disconnectSlideDown` (0.35s slide-in from top).
- **9 new unit tests** covering all disconnect/reconnect paths: disconnect marks state, wrong token rejected, correct token reconnects, opponent detection, expiry logic, circuit tracker properties. Total: 40/40 passing.

### Notes
- Build: 0 warnings, 0 errors. Tests: 40/40 passing.
- Auto-reconnect (brief drops, < 30 s): fully transparent — `OnConnectionUpAsync` clears state before the opponent even sees the banner.
- Manual reconnect (tab closed/refreshed): player navigates back to same URL; page `OnInitialized` calls `MarkPlayerReconnected`; opponent banner disappears immediately via `StateChanged`.
- Disconnect detection is immediate (fires when WebSocket drops), not delayed by circuit retention.

---

## Iteration 7 — Visual Polish Pass
**Date:** 2026-02-21

### What was done
- **Shot miss animation**: `::before` expanding ring ripple (`missRipple` keyframe, 0.65s ease-out); `::after` dot pops in via `dotPopIn`. Applied to both `cell-own-miss` and `cell-enemy-miss`.
- **Shot hit animation**: `::before` orange burst overlay (`hitBurst`, 0.55s, fades out); `::after` ✕ cross pops in with slight rotation (`crossPopIn`, 0.3s with 0.12s delay). Applied to both `cell-own-hit` and `cell-enemy-hit`.
- **Ship sunk animation**: `sunkFlash` keyframe (brightness 2.2→1.0, 0.8–0.9s) applied to `cell-own-sunk` and `cell-enemy-sunk`. Enemy sunk also gets the `hitBurst` overlay `::before` (0.75s) for a dramatic reveal of the ship colour beneath. All cells of a sinking ship animate simultaneously via CSS class changes.
- **Turn indicator transition**: Added `transition: background/color/border/box-shadow 350ms ease` to base `.turn-indicator`. Added `animation: turnActivate` (scale 0.94→1.03→1.0, 0.45s cubic-bezier spring) to `.turn-indicator.my-turn` — fires whenever the turn switches to this player.
- **Shot history chip animation**: Added `@key="shotKey"` to the shot history `@for` loop (key = index in full `ShotHistory` list) so each new chip gets a fresh DOM element. Added `animation: chipSlideIn` (slide from left + scale, 0.3s spring) to `.shot-chip-latest` so new shots slide into the bar.
- **Refined enemy hover**: `.cell-enemy-targetable` now has 80ms CSS `transition`, stronger blue glow (`box-shadow: inset 2px + outer 10px`) and a targeting `＋` crosshair via `::before` pseudo-element on hover. `z-index: 1` ensures the glow visually appears above adjacent cells.
- **New @keyframes**: `missRipple`, `dotPopIn`, `hitBurst`, `crossPopIn`, `sunkFlash`, `turnActivate`, `chipSlideIn` — all added to `app.css` keyframe section.
- Moved technical debt items (FA replacement, name collision, ClearShips bug) to Iteration 11 in to-do.md for a dedicated sweep.

### Notes
- Build: 0 warnings, 0 errors. Tests: 31/31 passing.
- All animations are purely CSS — no C# timer tracking needed. Class changes from Blazor's DOM diffing trigger animations automatically.

---

## Iteration 6 — Win/Loss Screen & Rematch Flow
**Date:** 2026-02-21

### What was done
- Replaced the basic one-liner result overlay with a polished full-screen stats card:
  - **Banner**: "⚓ Battle Over" + personalised winner/loser headline using player names.
  - **Stats grid**: Two-column layout (winner left, loser right) separated by a VS divider. Each column shows: shots fired, hits, accuracy %, ships sunk (out of 5). Uses gold/red colour coding for winner/loser columns.
  - **Rematch voting**: "🔄 Play Again" button (green); turns into a muted "✓ Rematch vote cast" (disabled) after clicking. Vote status line updates live: "Waiting for {opponent}…" or "⚡ {opponent} wants a rematch — accept above!"
  - **Back to Home** link always available as an escape.
- Added `GameSession.ProposeRematch(playerName)` — thread-safe vote accumulation. When both players vote, automatically calls `ResetForRematchCore()` inside the same lock.
- Added `GameSession.HasVotedRematch(playerName)` and `RematchVoteCount` accessors (both lock-protected).
- Refactored `ResetForRematch()` into public wrapper + private `ResetForRematchCore()` to allow calling from `ProposeRematch` without re-entering the lock (deadlock prevention).
- Fixed `MarkPlayerReady` to use `_rematchFirstPlayer ?? Host` for first-turn assignment — loser of the previous game now correctly goes first in rematches (`_rematchFirstPlayer` was previously stored but never consumed). Resolves to-do-technical item.
- Updated `OnSessionStateChanged()` in Battle.razor: when Phase transitions to Placement (rematch accepted), both circuits navigate to `/placement/{Code}?name=…&token=…` automatically.
- Added stat helpers to Battle.razor `@code`: `GetShotCount()`, `GetHitCount()`, `GetAccuracy()`, `GetShipsSunkCount()` — all computed from `ShotHistory` and board state.
- Added result-screen CSS section to `app.css`: `.result-card-wide`, `.result-banner`, `.result-stats-grid`, `.result-stat-col`, `.stat-col-winner/loser`, `.result-vs-divider`, `.result-stat-header/row/label/value/total`, `.result-role-badge`, `.badge-winner/loser`, `.result-actions`, `.btn-success`, `.btn-voted`, `.result-vote-status`.
- Also delivered Iteration 7 (Rematch Flow) in the same iteration.

### Notes
- Build: 0 warnings, 0 errors. Tests: 31/31 passing.
- Rematch first-player logic (to-do-technical item) resolved and closed.

---

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

## Iteration 5 — Shot History Bar
**Date:** 2026-02-21

### What was done
- Added a shot history bar to `Battle.razor` positioned between the turn indicator and the battle grids.
- Displays the last 5 shots, newest first, as coloured chips:
  - **Most recent chip** (`.shot-chip-latest`): fully opaque, slightly larger, shows full description: icon + coordinate + "{ShooterName} — {result}" (e.g. "🔥 B7  Alice — hit!").
  - **Older chips**: show icon + coordinate only; fade progressively (68% → 50% → 35% → 18% opacity) so the player's eye is drawn to the newest shot.
  - **Colour coding**: miss = grey, hit = orange, sunk = red — matching the toast colours.
- Before any shots: a placeholder "No shots fired yet" in italic muted text.
- `RecentShots` computed property added to `@code` block (reads `ShotHistory`, reverses, takes 5).
- `FormatCoord(int row, int col)` static helper added; produces Battleship-standard notation e.g. "B7".
- ~90 lines of CSS added to `app.css`: `.shot-history-bar`, `.shot-history-list`, `.shot-chip` and variants.
- Razor parser note: avoid `@{}` code blocks inside outer `@if/else {}` branches; compute data in `@code` properties instead.
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

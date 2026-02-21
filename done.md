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

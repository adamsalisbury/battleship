# Battleship — Project Document

## Game Design

### Overview
Classic two-player Battleship implemented as a real-time browser game using Blazor Server + SignalR.
Desktop-only, landscape viewport, minimum 1280×720.

### Rules
- Each player has a 10×10 grid (columns A–J, rows 1–10).
- Players secretly place their fleet on their own grid before battle begins.
- Players alternate turns shooting at the opponent's grid by clicking a cell.
- A shot result is immediately announced: Hit or Miss.
- A ship is "sunk" when all its cells are hit; this is announced to both players.
- The first player to sink all of the opponent's fleet wins.

### Fleet Composition (Standard)
| Ship       | Size |
|------------|------|
| Carrier    | 5    |
| Battleship | 4    |
| Cruiser    | 3    |
| Submarine  | 3    |
| Destroyer  | 2    |
Total: 17 cells to sink.

### Ship Placement
- Each player independently places ships during the Placement phase.
- Ships may be placed horizontally or vertically.
- Ships may not overlap or extend beyond the grid.
- No adjacent ships rule: ships may touch at corners but NOT at sides (classic rule: adjacency allowed — we use standard Hasbro rules where ships may touch).
- Players can rotate ships and drag-and-drop (or click) to place.
- A "Randomise" button auto-places all ships randomly.
- Placement is confirmed with a "Ready" button. Battle begins when BOTH players are ready.

### Turn Order
- The host (player who created the game) goes first.
- Turn passes after each shot (hit or miss — only one shot per turn, classic rules).
- On a hit, the player does NOT get an extra shot (we use the standard 1-shot-per-turn rule for fairness in a timed/real-time context).

### Win Condition
- All 5 opponent ships sunk → winner announced.
- Rematch option offered to both players.

### Visual Feedback
- Own grid shows: ship placements, opponent's hits (red), opponent's misses (white dots).
- Opponent grid shows: hits (red flame), misses (white dots), unsunk areas unknown.
- Sunk ships are revealed on opponent grid.
- Turn indicator shows whose turn it is.
- Hit/miss animation feedback.

---

## Architecture

### Solution Structure
```
BattleshipGame/               ← Blazor Server single project
  Components/
    Pages/
      Home.razor              ← Landing page (name + new/join)
      Lobby.razor             ← Waiting room (shows both players, game code)
      Placement.razor         ← Ship placement phase
      Battle.razor            ← Main game view
      Results.razor           ← Win/loss screen with rematch
    Shared/
      Grid.razor              ← Reusable 10x10 grid component
      ShipStatus.razor        ← Fleet status sidebar
  Models/
    GameSession.cs            ← Server-side game state
    GamePhase.cs              ← Enum: Lobby, Placement, Battle, Finished
    Cell.cs                   ← Grid cell state
    Ship.cs                   ← Ship definition and placement
    Player.cs                 ← Player state
    ShotResult.cs             ← Enum: Miss, Hit, Sunk
    ShipType.cs               ← Enum: Carrier, Battleship, Cruiser, Submarine, Destroyer
  Services/
    GameSessionService.cs     ← Singleton: session registry, join/create logic
  Hubs/
    BattleshipHub.cs          ← SignalR hub for real-time events
  wwwroot/
    css/
      app.css                 ← Game styles (dark theme, grid visuals)
```

### State Management
- `GameSessionService` (singleton) holds a `ConcurrentDictionary<string, GameSession>`.
- `GameSession` is the source of truth for all game state.
- Blazor components hold only local UI state (hover, selection).
- All mutations go through `GameSessionService` methods.
- SignalR hub broadcasts state-change events to both players' circuits.
- Each Blazor component subscribes to `GameSession.StateChanged` event and calls `InvokeAsync(StateHasChanged)`.

### Real-Time Pattern
- Singleton service with C# events (as per memory notes).
- `GameSession.StateChanged` fires server-side.
- Each player's circuit subscribes and re-renders.
- Components dispose subscriptions in `Dispose()`.

### Code Generation
- 4-character uppercase codes, alphabet: `"ABCDEFGHJKLMNPQRSTUVWXYZ23456789"` (no O, 0, I, 1).

---

## Current State
- Iteration 6 complete: Polished full-screen Win/Loss screen with per-player stats (shots fired, hits, accuracy, ships sunk) and rematch voting. Both players must click "Play Again" for a rematch; when both vote, the session resets and both circuits auto-navigate to the Placement phase. The loser of the previous game goes first in the next (correctly wired now). Rematch flow (previously Iteration 7) delivered in the same pass.

## Design Decisions
- One shot per turn (no extra shot on hit) — simplest, most common ruleset.
- Adjacent ships allowed (Hasbro standard).
- Host goes first — known upfront, no coin flip needed.
- Dark theme, naval colour palette.
- No mobile support required (desktop landscape only).

## Known Technical Debt
- See to-do-technical.md

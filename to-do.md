# Backlog — Feature Work

Items are prioritised top-to-bottom. Each item is one iteration.

## Iteration 4
- **Fleet Status Sidebar** — Both grids show a fleet status list: ship name, size, status (Afloat / Hit / Sunk). Own fleet shows damage received per ship. Opponent fleet shows sunk ships.

## Iteration 5
- **Shot History / Last Shot Banner** — Recent shot banner below the turn indicator showing who shot where and what the result was.

## Iteration 6
- **Win / Loss Screen** — Polished full-screen result overlay: winner's name, fleet summary (ships sunk / remaining), total shots fired, accuracy %. "Play Again" option offered to both players (requires both to accept before rematching).

## Iteration 7
- **Rematch Flow** — After game ends, either player can propose a rematch. When both accept, boards reset and placement phase begins again. The loser of the previous game goes first in the next.

## Iteration 8
- **Visual Polish Pass** — Animations: shot splash animation on hit/miss, ship sink animation, turn-change transition. Grid cell hover states refined. Loading states. Sound effects (optional — visual only is fine).

## Iteration 9
- **Disconnect / Reconnect Handling** — If a player disconnects (browser close/refresh), detect it via SignalR OnDisconnectedAsync, display "Opponent disconnected" to remaining player. Allow reconnection via the same URL with same token within 5 minutes.

## Iteration 10
- **Session Expiry UX** — If game session is not found (expired), show a clear error with "Start a new game" link rather than a generic error.

## Iteration 11
- **Accessibility & Keyboard Nav** — All interactive elements keyboard-navigable. Grid cells have aria labels ("A1", "B3", etc.). Screen reader announcements for shot results.

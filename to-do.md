# Backlog — Feature Work

Items are prioritised top-to-bottom. Each item is one iteration.

## Iteration 7
- **Visual Polish Pass** — Animations: shot splash animation on hit/miss, ship sink animation, turn-change transition. Grid cell hover states refined. Loading states. Sound effects (optional — visual only is fine).

## Iteration 8
- **Disconnect / Reconnect Handling** — If a player disconnects (browser close/refresh), detect it via SignalR OnDisconnectedAsync, display "Opponent disconnected" to remaining player. Allow reconnection via the same URL with same token within 5 minutes.

## Iteration 9
- **Session Expiry UX** — If game session is not found (expired), show a clear error with "Start a new game" link rather than a generic error.

## Iteration 10
- **Accessibility & Keyboard Nav** — All interactive elements keyboard-navigable. Grid cells have aria labels ("A1", "B3", etc.). Screen reader announcements for shot results.

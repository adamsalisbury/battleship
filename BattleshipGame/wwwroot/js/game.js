// Battleship Game — JS interop helpers
// Kept minimal: only what Blazor Server cannot do natively.

/**
 * Programmatically focuses a DOM element by its ID.
 * Used for roving-tabindex keyboard navigation within game grids.
 * @param {string} id - The element's id attribute value.
 */
window.focusElement = function (id) {
    const el = document.getElementById(id);
    if (el) el.focus({ preventScroll: false });
};

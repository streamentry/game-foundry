# I349 — The heading stays clear

Run 3, batch 3 (P12).

## Player

"At first view the menu button covers the 'W' of 'What will you help grow?'"

## PM decision

The heading keeps clear of the floating menu button on both sides at every scroll position. It wraps within the clear space instead of sliding under the button.

## Dev

- The "Choose title" label sits in a row padded 72 pt each side. `decisionHeading` still points at the label, so the preview's reveal maths is unchanged.

## Result

No first letters hidden. The same pattern can apply to any heading that scrolls under the chrome.

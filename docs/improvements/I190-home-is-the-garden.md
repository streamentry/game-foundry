# I190 — Home is your garden, full screen, with one round play button

Date: 2026-11-26. Branch: feat/mosselora-world-first.

## Player (b211)

Home was the garden on the top 60% and a paper panel below it: the wordmark, "A living garden that remembers.", the stepping stones, a "Fen remembers the wisteria you grew." line and a long "→ First Bend" button. It reads like a landing page. I opened the app to see my garden and half of it was covered by a form.

## PM decision

- **Home is the garden, edge to edge.** No paper, no wordmark (the launch screen already shows it), no tagline, no memory line on home: the neighbours stand in the garden and talk when tapped (I184).
- At the bottom, standing on the grass: the stepping stones of the walk, the **same plank sign** that hangs over the next board, and **one round play token** (96 pt) with an arrow. Its name ("First Bend", "Continue") is still spoken by screen readers and shown on press-and-hold.
- The map frames what you made in the upper part of the screen, clear of the token. When the newest thing stands at the painting's lower edge, the land **runs on below the painting** (a mirrored meadow, no seam) so the view can still lift it above the token.
- Landscape keeps its side column for now.

## Dev

- `BuildTitleScreen`: `worldHome` (map shown, portrait) makes the village window full screen, hides the melt, skips the wordmark, tagline and keeper line, and builds `NextStreamLine` (English) plus a round `IconButton` ("Play", 96 pt, `WaterAction`) in a bottom-aligned stack. `FitHubStack` is not needed there.
- `GardenMapOnHub`: focus at 0.36 from the top in portrait, and a 22% meadow apron.
- `GardenMapView`: the painting moved to a child (`Garden painting`) so the land can be taller than it; taps read the painting's own rect; the dry wash belongs to the painting. New `apron` parameter: a `RawImage` of the painting's bottom strip, flipped, joins seamlessly below; while the valley is still dry it carries the same straw wash.

## Result (b212 → b214; portrait, landscape and 150% tours, 23 each, all complete)

- `I190-b212.jpg`: a fresh garden on home (stones, "First Bend" plank, round token) and the finished garden, where the token covered Mira and Fen at the painting's bottom edge.
- `I190-home-is-the-garden.jpg` (b214): the finished garden at 100% and 150% with Mira, Orin and Fen standing clear above the token, the wisteria and fir on the mirrored meadow; stream 2's board and the memory card unchanged.

Project verify passes. .NET tests not run (no Core change).

**Next:** the corner book button and the menu become one fan menu (I191); the empty fresh garden asks for something to do on it (free planting, I193+).

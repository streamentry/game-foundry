# I102 — Game uses the repo's brand logo; scrapbook journal

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

## Request

"please use the logo that we have in this repo". Until now the game drew a flat green Codex wordmark (`art/generated/wordmark.png`). The repo's real brand logo is `games/evergrove/brand/mosselora-logo.png`: carved cream letters on a mossy log, with a butterfly, a lit cottage, wildflowers and a little waterfall. The app icon and mosselora.com already use it. Now the game does too, everywhere it shows its name.

## Changes (b83–b85)

- **Art pipeline (`tools/evergrove_art_pipeline.py`):**
  - New `knock_out_border()`. It removes only the white background connected to the image edge, by flood fill from the border. The usual knockout removes every near-white pixel, which would have punched holes in the logo's cream letters and white flowers.
  - The `wordmark` runtime sprite is now built from the brand logo (1024×398, about 2.6:1), falling back to the old master only if the logo is missing.
  - The iOS launch image `brand/launch-portrait.png` is built from the brand logo too (980 px wide, centred).
- **Sizes:** the brand logo is far less wide than the old wordmark (2.6:1 against 4.4:1), so the old heights would have shrunk its letters.
  - Hub: 56 → 96 pt.
  - Page header: 46 → 64 pt on puzzle screens and 72 pt on story screens, still inset past the menu and book buttons (I79).
  - The hub's lower section already sizes to its content (I85), so the painting moves up to make room.
- **Memory-screen fit kept (I78):** the taller header pushed the two action buttons partly off a 402×874 phone. The memory screen's village banner now draws at 0.68× (was 0.9×) at 100% text, and both buttons fit again.
- **Also in this commit: scrapbook journal.** This came from the I100 review ("the journal switches to flat, uniform cards"). Each journal page now sits at a fixed small tilt (−1.6° to +1.6°, a repeating pattern, never random), with a slightly deeper lift. It is visual only: layout, taps and reading order are unchanged. Its blind Haiku comparison was cut off by a usage limit and was not re-run.

## Evidence

`I102-repo-logo-and-scrapbook.png`, left to right:

1. b73 hub with the old flat wordmark.
2. b83 hub, live, with the brand logo.
3. b83 stream 2: the logo between the menu and book buttons, with the whole board on screen.
4. b84 memory screen: the logo, plus the change-choice and continue buttons fully visible.
5. The regenerated launch image.
6. b82 twelve-memory journal with tilted scrapbook pages.

A live launch on b85 caught the launch cover's brand logo fading into the hub's.

The verifier passes, and the b83, b84 and b85 QA tours each captured all 20 screens.

## Still unverified

- The logo at 125% and 150% text; the header height does not depend on text size.
- Small enclosed white gaps between leaves remain in the cut-out, invisible on the cream page.
- No players aged 40+ have tried the game (m3-session-kit P1–P9).

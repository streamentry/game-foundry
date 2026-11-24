# Iteration 51: Unity Procedural Vector Sound Icon

## Problem & Friction
The quiet audio setting was displayed as a full-width text row ("Quiet audio setting: On/Off").

## Game Design Solution
Replaced with dynamic speaker icons `🔊` / `🔇` (`MosseloraTheme.GlyphSoundOn` / `Off`):
1. **Compact Ambient Control:** Fits neatly into the top HUD and drawer.
2. **Instant Status Feedback:** One tap toggles and visibly indicates sound state.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.

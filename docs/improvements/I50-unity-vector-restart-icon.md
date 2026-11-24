# Iteration 50: Unity Procedural Vector Restart Icon

## Problem & Friction
Text button "Restart" consumed excessive horizontal width on mobile rows.

## Game Design Solution
Adopted the circular renewal symbol `↺` (`MosseloraTheme.GlyphRestart`):
1. **Circular Stream Rebirth:** Communicates gentle reset without punitive failure connotations.
2. **Harmonious Sizing:** Pairs symmetrically with Undo and Hint in the action toolbar.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.

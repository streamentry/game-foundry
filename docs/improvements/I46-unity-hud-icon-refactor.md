# Iteration 46: Unity EvergroveApp Header HUD Refactor

## Problem & Friction
In the Unity client, `EvergroveApp.cs` generated bulky text-labeled buttons for all actions, cluttering mobile screens.

## Game Design Solution
Introduced the `IconButton` architecture in `EvergroveApp.cs`:
1. **Compact 52px Geometry:** Renders rounded procedural icon tokens with high-contrast glyphs.
2. **Accessible Speech Integration:** Pairs internal glyph rendering with full semantic labels for assistive technologies.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Static guards verified.

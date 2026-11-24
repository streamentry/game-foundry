# Iteration 47: Unity Slide-Out Menu Drawer

## Problem & Friction
Settings, audio toggles, and memory journals in Unity were stacked directly in the main scroll view, pushing the puzzle down.

## Game Design Solution
Added `menuDrawerOpen` state to `EvergroveApp.cs`:
1. **Collapsible Settings Hierarchy:** Moves secondary options into an on-demand slide-out sheet.
2. **Zero In-Game Distraction:** Leaves 100% of the screen focused on water streams and diorama beauty.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.

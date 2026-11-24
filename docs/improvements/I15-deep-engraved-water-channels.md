# Iteration 15: Deep Engraved Water Channels

## Problem & Friction
The pipe pathways looked like lines drawn on top of tiles rather than channels cut into stone.

## Game Design Solution
Simulated deep chiseled conduits using dual-axis drop shadows:
1. **Engraved Inset Effect:** An upward dark cast shadow paired with a downward specular reflection simulates water flowing inside a carved stone groove.
2. **Luminous Water Glow:** Connected tiles emit a subtle internal cyan radiance.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.

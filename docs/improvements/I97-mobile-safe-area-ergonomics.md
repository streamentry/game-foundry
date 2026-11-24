# Iteration 97: Mobile Safe-Area Inset Ergonomics

## Problem & Friction
Modern smartphones with curved display edges, camera punch-holes, and gesture navigation bars occasionally overlapped corner buttons.

## Game Design Solution
Enhanced safe-area inset calculations:
1. **Full-Bleed Margin Guarantees:** Ensures at least 18px clearance beyond all notch boundaries.
2. **Landscape Resilience:** Buttons never collide with iPhone Dynamic Islands or Android home bars.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.

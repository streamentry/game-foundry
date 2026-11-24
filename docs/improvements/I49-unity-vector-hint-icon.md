# Iteration 49: Unity Procedural Vector Hint Icon

## Problem & Friction
Hint buttons with changing multi-word text labels ("Notice row...", "Align one pipe") resized unpredictably on mobile.

## Game Design Solution
Standardized the Hint symbol `💧` (`MosseloraTheme.GlyphHint`):
1. **Water Drop Metaphor:** Aligns directly with guiding mountain water to thirsty land.
2. **Stable Dimensions:** 52px fixed touch target prevents UI jumping during play.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.

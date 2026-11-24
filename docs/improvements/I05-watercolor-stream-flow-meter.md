# Iteration 05: Watercolor Stream Flow Meter

## Problem & Friction
The previous tile counter was a plain boxed fraction (`4 of 9 tiles reached`) that read like a dry telemetry metric.

## Game Design Solution
Redesigned the counter into an organic water-spirit pebble badge:
1. **Animated Water Droplet Glyphs:** Ambient floating water droplet communicates fluid vitality.
2. **Parchment Enamel Fill:** Aquamarine enamel gradient with soft inner luster.
3. **Accessibility Preserved:** Preserves exact `#water-count` element text for screen readers and test contracts.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.

# Iteration 17: Watercolor Droplet Particle Bursts

## Problem & Friction
When a tile connected, there was no burst of tactile celebration, making moves feel dry.

## Game Design Solution
Spawned ephemeral watercolor droplet particles:
1. **Dynamic Splash:** Four microscopic water droplets burst outward from the tile center on rotation.
2. **Smooth Fade-Out:** Dissolves over 450ms without DOM buildup or layout thrashing.

## Verification
- `npm test`: 10/10 passed.
- `python3 tools/verify_evergrove_project.py`: Verified.

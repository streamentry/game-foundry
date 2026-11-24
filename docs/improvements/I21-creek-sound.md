# I21 — Creek ambience follows real water reach; pipes tock

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

**Player problem.** With the chime switched on, the game was silent apart from the solve chime. A creek you restore should sound like one, and a pipe you turn should feel like wood under your fingers.

**Change (only when the player turns sound on; the default stays silent, per DESIGN-SYSTEM.md).**
- **Creek ambience.** A procedural 3.75 s seamless loop: brown noise, low-passed so it sounds like water rather than hiss, with a slow burble and a crossfaded seam. Its volume follows the *real* spring reach: silent when dry, 0.05 + 0.20 × reach as water connects, fullest when solved. It stops on the ending, or when sound is turned off.
- **Pipe tock.** Every real turn plays a short wooden "tock" (330/495 Hz body with an exponential decay and a 2 ms noise click).
Both clips are generated at runtime: no audio files, no licensing, no SDK.

**Vision fit.** Causal, not decorative: the sound tells the truth about the water, as the painting does.

**Evidence.** b24 QA tour, 17 screens, 0 exceptions in the app log. **Not verified:** listening on a device, the mute switch, volume balance with the chime, and how it feels to adults 45+.

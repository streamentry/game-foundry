# I27 — Progress sentence becomes spoken-only; droplets and mouths show it

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

**Player problem (I25 finding 2).** "Water reaches 4/9 tiles · 10 open joins" was a stats sentence under every board: web-like text repeating what the board already shows.

**Change.** The sentence is now **spoken, not shown**. It stays a transparent text over the droplet meter, so VoiceOver/TalkBack still announce exact reach and open joins after each move, and the QA tour still finds "Water feedback". On screen, the **droplets** show reach and the **cream mouths** on pipe ends show open joins (I-open-mouths). Landmark taps are explained by the protected-habitat line that already sits above the board. On solve, the full droplet row and the green board say it.

**Evidence.** b26 tour (`I26-I28-band-coach.png`): stream 1 and stream 2 boards go straight from droplets to tools.

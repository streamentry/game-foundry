# I82 — Hub reads alone; spoken labels verified from the bridge

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

## Why

I81 added the spoken label "Mira. Has something new to say", but it was checked only in code. The bridge's label log (`MOSSELORA_SR_NODES`) ran only inside the scripted tour, and the tour skips the hub.

## Change

- **Dev-only switch:** launching with `-mosseloraSrLog 1` (on the iOS Simulator, `SIMCTL_CHILD_mosseloraSrLog=1`) logs the full screen-reader tree on every refresh, on any screen. This is the exact label list `ScreenReaderBridge` publishes to VoiceOver/TalkBack. It is guarded by `UNITY_EDITOR || DEVELOPMENT_BUILD`, and the verifier's dev-control guard passes.
- **Bug found and fixed with it (b57 → b58):** while the hub covered the page, the tree still held the board beneath it: 9 tiles, Undo, Guide me, Restart, Menu and duplicate neighbors, 28 nodes in all. A VoiceOver user on the hub could swipe into controls they cannot see. `Collect` now reads only the hub while it is open, and the whole canvas otherwise.

## Evidence (live launches on the iOS Simulator)

The full logged trees are in `I82-screen-reader-trees.md.txt`.

- **b57 hub:** 28 nodes, including "Row 1, column 1: corner pipe…" and "Button: Undo" behind the hub. It also shows "Button: Mira. Has something new to say", which confirms I81 on the real bridge.
- **b58 hub:** 6 nodes, in top-to-bottom order:
  - "Mira. Has something new to say"
  - "Orin. Has something new to say"
  - "Fen. Has something new to say"
  - the tagline header
  - "First Bend"
  - the Play button, labelled "First Bend"
- **b58 after Play:** 22 nodes, the board and its tools. On the page, the neighbors read as plain names; news is a hub-only label.
- The verifier passes, and the b57 and b58 QA tours captured all 19 screens.

## Still unverified

- This is the label list the bridge publishes, not audio. I have not heard it through VoiceOver or TalkBack on a device.
- The hub's wordmark has no spoken node of its own; the tagline header leads instead.
- No players aged 40+ have tried this (m3-session-kit P1–P9).

# I93 — Screen readers meet the hub's menu first

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

## Problem

I83 put the menu (and, once there are memories, the book) on the hub. Those buttons must be the hub's last children so they draw above the painting. `ScreenReaderBridge` reads in hierarchy order, so VoiceOver and TalkBack users met them last, after Play, while sighted players see them first, top-left and top-right. This was noted as open in I83.

## Change: implemented by a Haiku sub-agent, reviewed and verified here

A Haiku sub-agent got an exact spec and made the change in the worktree, with no commit and no build:

- **`ScreenReaderBridge.Collect`:** gains an optional `Transform skip`. `Walk` passes it down and skips that subtree.
- **`EvergroveApp` semantics refresh:** when the hub is open and has a `Hub chrome` child, the hub chrome's nodes are collected first, then the rest of the hub with that subtree skipped. Drawing order is untouched.

My review covered three points:

- The diff matches the spec, touching only the two files.
- `SemanticNode` is a public nested class, so the qualified type compiles.
- Both `Walk` call sites pass `skip`, and every other `Collect` call keeps the default.

## Evidence (live, iOS Simulator, `-mosseloraSrLog 1`)

The full logged trees are in `I93-hub-reading-order.md.txt`.

- **b72 hub (fresh garden), 7 nodes:**
  1. Menu
  2. Mira. Has something new to say
  3. Orin. Has something new to say
  4. Fen. Has something new to say
  5. Header: A living garden that remembers.
  6. First Bend
  7. Play ("First Bend")

  Each control appears exactly once.
- **b72 after Play:** the board tree is unchanged, with the same 22 nodes as in I82.
- The verifier passes, and the b72 QA tour captured all 19 screens.

## Still unverified

- I have not heard this through VoiceOver or TalkBack on a device; the evidence is the label list the bridge publishes.
- The book button's position in the order with memories present was not logged this time; it follows the same rule.
- Codex is still blocked by credits.
- No players aged 40+ have tried this (m3-session-kit P1–P9).

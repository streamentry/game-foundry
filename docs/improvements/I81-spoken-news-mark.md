# I81 — Screen readers hear which neighbor has news

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

## Problem

I80 added a parchment speech mark on hub neighbors who have something new to say. It was a picture only. A screen-reader user heard only "Mira", so the feature that answers "what's new today?" was invisible to them.

## Change (b56)

- New localization key `has_news`, with English source "Has something new to say". The catalog now has 115 keys, and the test and verifier counts are bumped to match.
- All 29 locale catalogs gained the phrase. A Haiku sub-agent drafted the translations. I replaced its Russian draft, "Есть что новое сказать" (awkward), with "Хочет рассказать что-то новое".
- New helper `SpeakInstead(control, spoken)`. It renames the visible `Caption` plate to `Shown name` and adds a hidden `Caption`, which `ScreenReaderBridge` reads. On a hub portrait with news, that hidden caption reads "Mira. Has something new to say."
- Nothing visible changed: the name plate still shows just "Mira".

## Evidence

- The verifier reports an English source plus 29 complete catalogs, 115 keys each.
- C# tests pass: 40,529 assertions.
- The b56 QA tour captured all 19 screens.
- `I81-spoken-news-mark.png` is the b56 hub, launched live: plates and marks are unchanged.

## Not verified

- I have not heard the spoken label through VoiceOver or TalkBack. The bridge's QA log runs only inside the scripted tour, and the tour skips the hub, so this is code-checked only.
- The translations are AI drafts and still need native review, as with every other catalog key.
- No players aged 40+ have tried this (m3-session-kit P1–P9).

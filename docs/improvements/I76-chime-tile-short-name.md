# I76 — Sound tile shows its name, not a sentence

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

## Problem

The menu's sound tile was the only settings tile that showed a full instruction sentence on the wood: "Gentle chime: OFF (tap to enable)". Its neighbours show just their name ("Gentle motion", "Text size 100%") and use the icon and the tile tone for state. The long sentence made the menu read like a web form and broke the icon-first rule (seen in I75's menu screenshot).

## Change (b49)

- New helper `ShowShortName(Button)`. The visible caption becomes the part before the colon (`:` or the full-width `：`). A hidden `Caption` node keeps the full sentence, so `ScreenReaderBridge`, which reads the `Caption` child, still speaks "Gentle chime: OFF (tap to enable)".
- State is carried the same way as Gentle motion: a muted speaker on wood when off, a speaker on the forest-green tile when on.
- All 29 locale catalogs split cleanly at their colon, for example "Sanfter Klang", "優しい音", "Тихий звон" and "نغمة لطيفة". `GameLocalization.Display` does not reorder RTL text, so the split is safe. A caption without a colon keeps its full text.

## Evidence

- `I76-chime-tile-short-name.png`, left to right: b48 menu (long sentence), b49 menu (chime off), b49 menu (chime on). All three are live taps on the iOS Simulator.
- The verifier passes, and the b49 QA tour captured all 19 screens.

## Still unverified

- I did not hear the spoken label through VoiceOver; it is code-checked only.
- No players aged 40+ have tried this yet (m3-session-kit P1–P9).

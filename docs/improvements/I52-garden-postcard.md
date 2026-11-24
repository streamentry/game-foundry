# I52 — Spoiler-free garden postcard to share with a friend

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

**Player problem.** The vision's growth path starts with **no-server sharing** (§4.4 rung 1; §6 "the only no-SDK path"). There was no way to show a friend your garden.

**Change.** A **garden postcard**. In the menu sheet, "Copy garden postcard" (Codex speech-bubble-leaf glyph, labelled, as the menu allows) copies a short spoiler-free text to the clipboard:
```
Mosselora
🌉🌳🌼…        ← your choices, in order, as pictures
●●○○○○○○○○○○  Stream 3 / 12
```
- **No puzzle layouts, score, link, identifier or network.** It is only what the player chose and how far they have walked.
- The text comes from a new engine-free `Core/GardenPostcard.cs`, so it is deterministic and unit-tested.
- A confirmation line ("Postcard copied. Paste it into a message to share your garden.") appears until the menu closes.
- Two new keys, `postcard` and `postcard_copied`, in all 29 draft catalogs (Haiku sub-agent translations; native review still required). Catalogs now hold 108 keys.

**Evidence.** C# tests pass, 39,917 assertions, including six new postcard assertions: fresh garden, choices in order, stones walked, deterministic, no link or identifier. The verifier reports 108 keys × 30. The b35 QA tour shows 19 screens. **Not verified:** pasting into real messaging apps; emoji rendering differs by platform.

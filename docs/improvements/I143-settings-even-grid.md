# I143 — Settings sit in an even grid of matching tiles

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player (opened the menu from the home screen, b147)

The five settings tiles were sized by their captions: "Language: English" was wider than "Text size 100%", "Larger board 1×" narrower than "Gentle motion", and the fifth ("Gentle chime") stretched across the whole panel. Nothing lined up, so the panel looked like a form whose fields happened to be different lengths, not a set of matching wooden switches.

## PM decision

The settings panel becomes an **even grid**: two equal tiles per row, and an odd tile **centred at the same width**. The icons, states and words are unchanged. (The development-build rows below, playtest mode, matched control and 2.5D, don't ship to players and are left as they are.)

## Dev

`EvergroveApp.SettingsRow`: after the rows are built, `EvenTiles(row)` gives each tile `minWidth 0`, `preferredWidth 0` and `flexibleWidth 1`, and turns off the row's `childForceExpandWidth`. A lone tile gets a `0.5`-flex spacer on each side, so it's half wide and centred.

## Result (b148 → b149, iPhone 17 simulator, menu opened on the home screen)

- b148: the rows evened out, but the lone chime tile was only a third wide, because force-expand gave each spacer a full share. b149 turns force-expand off for these rows.
- b149: four equal tiles in two rows, and "Gentle chime" centred beneath them at the same width. Local before/after: `docs/improvements/I143-settings-before.png`, `I143-settings-after.png` (gitignored).
- The scripted tour captures all 20 screens. Verify passes.

**Not verified:** 150% text, where "Language: English" may wrap to two lines in a half-width tile, and the in-game settings (without the language tile), where Text size becomes the lone centred tile.

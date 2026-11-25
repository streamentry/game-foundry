# I174 — The almanac: everything you have ever grown or built

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player

With 200 things to make, a player needs a place to see what they've found, and what's still out there. That's the gentle reason to begin another garden (end-game plan E2). The taste charter rules out rewards and pressure, so no badges, no "complete the set!".

## PM decision

- **The almanac** is a second page of the book. Two round tabs sit at its top: the book (your pages) and a painted sapling (the almanac).
- Three sections: **Mira's trees, Fen's flowers, Orin's builds**, each led by the neighbour's portrait and a plain count ("7 / 50").
- Every item is shown. **Made ones are painted; the rest are faint pencil silhouettes** of the same picture, so you can see the shape of what's to come without a word. Tap a made one to see its name.
- It records every item you've ever made, across all gardens (saved on the device, so it survives starting a new garden).

## Dev

- New `Core/Almanac.cs`: a found set of art keys, `Parse`, `Serialize` (stable, sorted), `Add` (returns whether it's new), `Has`, `CountOf(family)` and `IsItemKey` (validates against the catalogue, tolerating junk). Tests in `AlmanacTests` (empty, junk, out of range, duplicates, round trip, full 200).
- `EvergroveApp`: `almanac` (PlayerPrefs `evergrove:almanac`), `RecordInAlmanac()` on load and after each choice, `JournalTabs()` and `AlmanacPage()`. The QA tour plays garden 0 with an in-memory almanac and never writes the player's almanac or garden number.
- `DevQaTour`: opens the almanac tab and saves `10d-almanac.png` (22 portrait, 22 landscape and 22 at 150% shots).

## Result (b186, iPhone 17 simulator)

`I174-almanac.jpg` (gitignored): portrait (tabs, "1 / 50" with Mira, one painted tree among silhouettes), 150% (larger counts, flowers below) and landscape (inside the sideways sheet). Tours pass. .NET tests: 52,709 assertions (with the garden map's). Verify passes.

**Not verified:** silhouette contrast on a real screen (16% ink is deliberately faint), and whether players read it as "still to find" rather than "locked" (a P-item for M3).

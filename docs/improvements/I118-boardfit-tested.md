# I118 — Landscape board sizing is a pure, tested rule

Date: 2026-11-25. Branch: feat/mosselora-painted-ui (PR #76).

## Why

The landscape board sizing from I114–I116 (tiles sized by the height left over, with the Undo/Hint rail beside an unsolved board) lived inside the Unity view code and had only been checked by eye on one simulator. AGENTS.md asks for pure, testable rules. It is also the part most likely to break on a phone of a different height, and image tools were unavailable this session.

## Change

- New pure module `Core/BoardFit.cs` (no UnityEngine): `Portrait`, `Landscape`, `Span`, `MaxTile`, plus the rail and chrome constants.
- `EvergroveApp.BuildPuzzle` calls it instead of inline math; `ToolRailWidth`/`ToolRailGap` alias the module's constants. The formulas are the same as before, so behaviour is unchanged.

## Tests (`BoardFitTests`, 8,368 new assertions; 49,481 in total)

- Portrait keeps the EG-007 sizing: a 90 pt cap on a 402 pt phone, a 48 pt floor, and a 67 pt cap at 4x4.
- iPhone 16 Pro class landscape (392 × 361 pt): unsolved tiles are **78 pt** with the rail, solved tiles 53 pt.
- Properties over heights 300–460 pt, column widths 260–480 pt, sizes 3–5, solved and unsolved:
  - tiles never drop below the 44 pt touch target or exceed the size cap;
  - when the minimum does not bind, board plus chrome fit the height;
  - the rail always has room beside an unsolved board;
  - a taller screen never shrinks the tiles.

## Verification

- b113 Unity build: 0 `error CS`. The portrait QA tour ran to DONE (20 screens), and the numeric diff against b112 is at most 440 differing pixels per screen, which is animation noise.
- Landscape was not re-shot this session (image and simulator tools were unavailable). Behaviour is unchanged by construction: the same formula, now covered by tests.

## Not verified

- iPad landscape, larger text, Android, real devices, and players aged 40+ (**age fit not validated**).

# v1 batch 21: item 20 (ticked, verified by line conservation, build and a before/after tour)

Date: 2026-11-28. Branch `feat/mosselora-v1-batch-21`, stacked on #152. One Haiku agent split the file with a cut-and-paste script; the coordinator verified conservation independently, built the app and compared tours before and after.

**20 Split `EvergroveApp.cs`.** The 9,907-line file is now `EvergroveApp.cs` (470 lines: header, fields, lifecycle) plus 28 area partials, such as Hub, Map, Menu, Sheets, Journal, Puzzle, Board, Family, Letters, Neighbours, Settings, Research and Qa. The largest is 719 lines, and the helper widgets moved to `EvergroveApp.Widgets.cs`. Members moved verbatim, and dev-only `#if` blocks moved whole.

**Tests:**
- **No file over 800 lines:** `FileSizeTests` enforces this for every script. Five other large files (GardenMapView, DevQaTour, TreePlay, MosseloraArt, WorldModel) are explicitly allowlisted at their current sizes and must not grow.
- **Build unchanged:** the multiset of non-blank code lines is identical before and after (0 removed; only file boilerplate added). Unity and Xcode builds succeed, and all source-audit tests pass. Tests that read `EvergroveApp.cs` now read all its partials through `UnityAppSource`; no assertion was weakened.
- **Tours unchanged:** the tour after the split produced the same 135 shots, and 131 are near pixel-identical to the tour before it. The other four are explained: rain and spring particle randomness, a toast caught mid-fade, and a journal page chosen by the tour's unordered `FindObjectsByType` lookup (tour nondeterminism, not a code change).

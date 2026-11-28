# v1 checklist status audit

Date: 2026-11-27. Checklist: [100-improvements-v1.md](100-improvements-v1.md). Scope: the 84 unticked items. The 16 ticked items were not re-audited, except that the batch logs for 35, 36, 52, 58, 74, 79, 76, 78 and 96 match their ticks.

Read-only audit of the worktree. Evidence came from the I-series logs, the v1 batch logs, `games/evergrove/Assets/Scripts/Core` and `Unity`, `games/evergrove/Tests`, `docs/research/decision-log.md` and `games/evergrove/docs/{qa,playtest,accessibility}`. Nothing was ticked here.

## Classes

- **READY-TO-TICK**: built, and a repository test matches the stated test.
- **BUILT-TEST-NOT-RUN**: built, but the stated test is a tour, device or review run that has no recorded result.
- **PARTIAL**: some of the item is built. The table says what is missing.
- **BUILDABLE**: not built. Code and a code or tour test can prove it, with no founder decision, players, devices or new art.
- **NEEDS-DEVICE-OR-ART**: needs a physical device measurement, new art, composed audio or a human visual review.
- **NEEDS-PLAYERS**: the stated test needs real players, families, native reviewers or blind testers.
- **NEEDS-FOUNDER**: blocked on a founder decision or authorization.

Convention: a built item whose test needs outside people is NEEDS-PLAYERS. A built item whose test the team can run in the simulator is BUILT-TEST-NOT-RUN. Batch tours were run and logged in the decision log, but no item-level audit was recorded.

## Counts (84 unticked)

| Class | Count |
| --- | --- |
| READY-TO-TICK | 3 |
| BUILT-TEST-NOT-RUN | 3 |
| PARTIAL | 32 |
| BUILDABLE | 16 |
| NEEDS-DEVICE-OR-ART | 11 |
| NEEDS-PLAYERS | 11 |
| NEEDS-FOUNDER | 8 |
| **Total** | **84** |

## Audit table

| N | Title | Class | Evidence | What's missing |
| --- | --- | --- | --- | --- |
| 1 | One silent opening shot | BUILT-TEST-NOT-RUN | I22, I190: launch goes to the painted garden | Launch-to-first-tap timing on a mid-range phone |
| 2 | Learn by one gesture | NEEDS-PLAYERS | I350, I147, I138: first tap taught | Five uncoached 40+ players finish the first board |
| 3 | Name the garden | BUILDABLE | None; `RiverNames` covers stretches only | Name entry, cottage sign, postcard; rename rate needs players |
| 4 | Mira explains once | PARTIAL | I16 coach bubble; I151, I163 picture tip | No three-line cap or word-count test |
| 5 | Remember the hand | BUILDABLE | None found | Handedness prompt, mirrored tray; thumb-reach needs device |
| 6 | Text size first | BUILDABLE | `TextScale.cs` and Settings size exist | First-run slider with live preview; 150% tour |
| 7 | First win in two minutes | NEEDS-PLAYERS | Board sizing rules exist | Median first-board time from playtest kit |
| 8 | No sign-up wall; family after day 3 | PARTIAL | Solo play has no accounts; `DaysPlayed` only on cottage card | Family features not gated to day 3; airplane run unrecorded |
| 9 | Restore the player can trust | PARTIAL | "Saved" mark (batch 4, `EvergroveApp.Saving.cs`); `SaveFuzzTests` | No "saved on this phone" line; no last-saved time |
| 10 | Two-sentence store promise | NEEDS-PLAYERS | None | Store copy; five-second test with 40+ readers |
| 11 | One menu button | BUILT-TEST-NOT-RUN | I191 corner button opens fan | Count of resting buttons not recorded |
| 12 | Icons that need no word | NEEDS-PLAYERS | I02, I194 icon tokens | Card sort with 8 players aged 40+ |
| 13 | Context buttons | PARTIAL | I204 `TreePlay` card | No "move" action beside a tapped thing; taps not measured |
| 14 | Back in the same place | PARTIAL | I101 close buttons stay in reach | No single back icon or gesture; no shot audit |
| 15 | Menu remembers | BUILDABLE | None | Last-used item ordering; logged second-tap test needs players |
| 16 | Badges mean one thing | PARTIAL | I112, I145 new-memory mark | Other badge sources not audited; clear-on-look untested |
| 17 | Sheets slide, never pop | PARTIAL | I19 page crossfade; I06 gentle motion | Crossfade only; no edge entry; reduced-motion not verified |
| 18 | Hide the HUD while looking | BUILDABLE | No idle-fade code (grep idle/fade) | 6 s idle timer, 20% opacity; screenshot pairs |
| 19 | Long-press explains anything | BUILDABLE | No long-press code | Caption system; raycast coverage script |
| 20 | Split `EvergroveApp.cs` | PARTIAL | Almanac, Economy, Market, Saving, ChoicePreview partials | Main file 9,585 lines; Hub, Sheets, Map, Notes not split |
| 21 | Parallax depth on the map | NEEDS-DEVICE-OR-ART | None (I26 is a sky wash, not parallax) | Three-layer parallax; 60 fps on oldest iPhone |
| 22 | Cast shadows that follow the sun | NEEDS-DEVICE-OR-ART | `DayLight` phases; `VillageDiorama` shadows Off | Sun-angle shadows; dawn, noon, dusk review |
| 23 | Water that reflects | NEEDS-DEVICE-OR-ART | None | Reflection shader; GPU frame time on device |
| 24 | Footsteps for neighbours | NEEDS-DEVICE-OR-ART | None | Walk frames; tour video review |
| 25 | Painted board-to-garden transition | NEEDS-DEVICE-OR-ART | I293 solved board becomes the thing; I19 | Clip review for one continuous motion |
| 26 | Seasonal palette grading | PARTIAL | I338–I341 per-thing looks (`SeasonLook.cs`); base contrast audit | No whole-scene grade table; no per-season contrast test |
| 27 | A composed score | NEEDS-DEVICE-OR-ART | I10 ambience; I71 volume | Composed loops and stingers; seam check |
| 28 | Sound in space | NEEDS-DEVICE-OR-ART | `GardenSounds.cs`; no stereo pan in code | Panning by screen position; headphone check |
| 29 | Haptics with meaning | PARTIAL | `Haptics.cs` Tick and Success; I140 | Haptics used for more than three cues; switch untested |
| 30 | Art consistency pass | NEEDS-DEVICE-OR-ART | None | Contact sheet reviewed by a person |
| 31 | Snug fits in Garden Pieces | READY-TO-TICK | I373; `GardenPiecesFitTests` | Suite pass (see below) |
| 32 | More good-neighbour pairings | READY-TO-TICK | I368, I380; `PairingTests` | Rule checked for presence, not one-line length |
| 33 | Garden layouts that remember | PARTIAL | Pairings saved and transferred (`PairingTests` save round-trip) | "How my garden grew" almanac page not built |
| 34 | Difficulty the player chooses | PARTIAL | I336 leaves on four kinds (`DifficultyTests`) | Other kinds still tier-based; no gentle level per kind |
| 37 | Boards from the player's garden | PARTIAL | `OwnGardenBoards.cs`; `OwnGardenBoardTests` (uniqueness) | Not wired into the app; sharing and mosaic missing |
| 38 | Chains between puzzles | PARTIAL | I350 water reaches garden; I293 | Link-by-link audit unknown; 2 s on-screen check missing |
| 39 | Undo everywhere, without limit | PARTIAL | `UndoHistoryTests` "restores exact board" | Capped at 10 (`UndoLimit`); no redo; no step count |
| 40 | Mistakes shown kindly | BUILDABLE | Conflicts shown automatically in Clash colour (Sudoku, Sums, Doorstep boards) | Lens toggle; persisted setting; no red |
| 41 | Scroll beyond the mist | PARTIAL | I360, I192; `RiverGrowsTests`; `ReachesCeiling = 20` | Save format has a 20-stretch ceiling; spec says none |
| 42 | Each stretch has a character | PARTIAL | I360, I362 names and plank signs | Per-stretch palette, neighbour and puzzle flavour missing |
| 43 | A boat to travel | BUILDABLE | None (catalogue "rowing boat" only) | Boat travel between stretches; two-tap reach test |
| 44 | Landmarks for wayfinding | NEEDS-DEVICE-OR-ART | `LandmarkGraphic` is a pipe-puzzle marker | Tall landmark art per stretch; wayfinding test needs players |
| 45 | A river map | PARTIAL | `GardenMapView`; I176 garden map | No painted scroll map; family gardens not pinned |
| 46 | Things grow outside the garden | PARTIAL | `MeadowLife.cs` (I363); `LivingValleyTests` | No spreading over days; no date-seeded growth |
| 47 | Build simple paths and bridges | BUILDABLE | I10 stepping stones are display only | Placeable stones; grid snap; undo test |
| 48 | Weather moves down the river | BUILDABLE | `DayWeather` deterministic by date (`WeatherTests`) | Upstream forecast line; next-day carry-over |
| 49 | Night creatures per stretch | PARTIAL | `Visitors.cs` Owl (night, I374, I376), Heron, Kingfisher | Moths and frogs as visitors missing; not per stretch |
| 50 | Performance budget for endless world | NEEDS-DEVICE-OR-ART | None (no windowing or pooling found) | Stretch windowing and pooling; memory profile on device |
| 51 | One small thing each day | NEEDS-PLAYERS | `CottageSummary.Waiting` (I299) | Logged day-2 to day-7 sessions |
| 54 | A morning and an evening | PARTIAL | `DayLight` dew (I366), lamp (I296), Companion morning line | No dew boards or lantern letters; clock tour not recorded |
| 55 | Calendar of the river | PARTIAL | `GardenSeason.Festival`, spring only (I317) | No locale frost or solstice calendar; locale review |
| 56 | An end to each session | BUILDABLE | None (no goodnight line) | Evening sign-off line; natural-ending test needs players |
| 57 | Notifications only for arrivals | PARTIAL | `GardenNoteTests` backs off after 3 unopened (I273–I277) | Notes cover drops, replays and waiting things, not only letters and parcels; no week-log test |
| 59 | The year has a shape | PARTIAL | I312 tales per season; `SeasonTests`; `YearAfterStory` | No per-season unlock; no year simulation test |
| 61 | Family-circle backend | NEEDS-FOUNDER | Decided 2026-11-25, not built | Founder approval; data map before code |
| 62 | Invite by link or QR | NEEDS-FOUNDER | Depends on 61 | Backend and join flow |
| 63 | Visit a relative's garden live | PARTIAL | I216 visit from copied garden file (read-only) | No live fetch; no gift or note; no 3G test |
| 64 | Help with their board | PARTIAL | I226 help parcels name a helper | Help is a bloom, not a placed move; no conflict test |
| 65 | Share food as real objects | READY-TO-TICK | `FruitTests`, `FamilyParcelTests`, `KitchenTests` Sending and Opening | Suite pass (see below) |
| 66 | Plant a tree together | BUILDABLE | None | Shared-tree rule over parcels; two-time-zone test in code |
| 67 | Voice notes in letters | NEEDS-DEVICE-OR-ART | None | Recording, captions, VoiceOver on device |
| 68 | Child-safe co-play | NEEDS-FOUNDER | Store child-audience declaration open | Founder decision and age declaration |
| 69 | Same-room play | NEEDS-PLAYERS | I264 take turns; I268 shape cue; `TogetherTests` | Grandparent-and-child session in playtest kit |
| 70 | Family album | PARTIAL | I269 family page (local only) | Sync needs item 61 |
| 72 | Everything has a reason to cost | NEEDS-PLAYERS | Batch 2 captions give each reason | Visible reason line; players predict price order |
| 75 | Recipes | NEEDS-FOUNDER | `Recipes.cs`, jam only (batch 8) | Second harvest or new food is a founder decision |
| 81 | Neighbours remember replies | BUILDABLE | I235 write-back exists; no reply chips | Three reply chips shaping the next letter; echo test |
| 82 | They notice habits, gently | PARTIAL | `Companion.HabitLine` (I419) | Code needs 6 returns at 60% share; spec says 5 morning visits |
| 83 | Each neighbour has a life arc | BUILDABLE | None | Orin boat and Fen mill arcs with end-state tests |
| 84 | Neighbours ask for help | PARTIAL | I313 `NeighbourWish`; `WorldIdeasTests` | No explicit test that each wish is reachable |
| 85 | Neighbours can be quiet | BUILDABLE | None | "Speak less" setting; persistence test |
| 86 | Neighbour speaks properly | NEEDS-PLAYERS | 29 catalogues ship English-only new text | Native reviewer sign-off |
| 87 | They mark real dates you choose | BUILDABLE | None (I421 anniversary is automatic) | Optional local date entry; data-stays test |
| 89 | Optional on-device language model | NEEDS-FOUNDER | Not started | Founder decision; on-device spike |
| 90 | Goodbyes that matter | PARTIAL | I359, I420 `WelcomeBack`; `DailyRhythmAuditTests` (1, 3, 14, 60 days) | No 90-day test; garden-specific wording unverified |
| 91 | 48 pt touch targets everywhere | PARTIAL | `BoardFit` tiles at least 48 pt (`Program.cs`); I18 | No raycast scan of all targets |
| 92 | A colour-blind pass | PARTIAL | I268 shape cue on the turn token | Other colour cues unpaired; simulation sheets not made |
| 93 | VoiceOver and TalkBack finish a board | NEEDS-PLAYERS | `ScreenReaderBridge.cs` | Blind-tester session on one puzzle kind |
| 94 | One-hand landscape and portrait | BUILT-TEST-NOT-RUN | I114–I116 landscape; batch tours in simulator | One-thumb reach audit not recorded |
| 95 | Crash reporting with consent | NEEDS-FOUNDER | None | Consent decision; privacy review |
| 97 | Real playtest with adults 40+ | NEEDS-PLAYERS | Playtest kit; run-3 notes are team-only | 8–12 adults over 7 days; report in decision log |
| 98 | Day-7 threshold before launch | NEEDS-FOUNDER | No dated number in decision log | Founder sets the number |
| 99 | Store-ready checks | NEEDS-FOUNDER | `docs/store-publishing-workflow.md` has no tick list | Checklist completion; authorization to submit |
| 100 | Delete what didn't earn its place | NEEDS-PLAYERS | None | Playtest usage evidence first |

## Ready to tick

Each item below has a built feature and a passing test in the Core suite (`games/evergrove/Tests`, run by `Program.cs`). Ticking is still a separate step.

- **31 Snug fits.** `GardenPiecesFitTests.Run`, assertion "snug glow says far less than the plain glow" (`snugTotal * 2 < plainTotal`). I373 records 4,634 snug of 16,209 plain squares.
- **32 Good-neighbour pairings.** `PairingTests.Run`, assertion "each has its rule written" across all pairings, with the I380 deeper pairings tested in the same file. Limit: the test checks rule text is non-empty, not that it is one line.
- **65 Share food.** `FruitTests.Sending` and `FruitTests.Opening` (fruit round trip, "opening adds the fruit"), `KitchenTests.Sending` and `KitchenTests.Opening` (jam: "two stay, three arrived"), and `FamilyParcelTests.Sending` and `Opening` (seeds and gifts arrive once).

## Buildable next

Ordered by player value against effort. Each is a code change with a code or tour test.

1. **85 Speak less.** Small setting that halves remarks. Protects the calm-companion rule for 40+ players.
2. **40 Mistakes shown kindly.** Conflicts currently flash in Clash colour on every board. A persisted lens toggle is small and affects every puzzle.
3. **6 Text size on first launch.** Moves the existing size control into a first-run card with live preview. Verified by the 150% tour.
4. **19 Long-press explains anything.** One caption system replaces help pages. A coverage script can list uncaptioned targets.
5. **81 Neighbours remember replies.** Three reply chips feed the next letter. Echo within three days is a code test.
6. **18 Hide the HUD while looking.** 6 s idle fade to 20%. Small, with screenshot pairs.
7. **5 Remember the hand.** One-time handedness prompt that mirrors the tray. Needs a thumb-reach check on a device later.
8. **3 Name the garden.** Optional name with `RiverNames` as the suggestion, shown on the sign and postcard.

Quick PARTIAL fixes worth doing alongside: 82 (use the spec's 5 morning visits), 39 (redo and an unlimited history), 20 (split the 9,585-line file).

## Verification

- Core suite (`dotnet run --project games/evergrove/Tests/Evergrove.Core.Tests.csproj --configuration Release`, exit 0): "Evergrove C# tests passed: 1765932 assertions". The three READY-TO-TICK test groups run inside this suite and passed with it.
- `python3 tools/check_evergrove_docs.py`: "Evergrove documentation contract: 12 documents, 145 verified local links, 12 puzzles, v4 saves, 30 language choices, 127 keys."
- Not run: the Unity editor, tours, and any device. No class above depends on them except where stated.

## Vision run 20261128-0855 (outside the v1 checklist)

- Protected pipe names its cause: `LandmarkCause` (Living mode, English, truth-guarded like the journal). Tests: `LandmarkCauseTests`. Tour and device: not run.
- Solve bloom follows the real stream: `FlowOrder`. Tests: `FlowOrderTests` (all 12 boards and 33 variants). Tour and device: not run.
- Restore card compares both gardens: `GardenCompare`. Tests: `GardenCompareTests`. Tour and device: not run.
- Root-stone solver gate (depth proposal C1, gate K5), not wired: `RootedBoards`. 200/200 unique 4x4 boards for one and for two roots. Tests: `RootedBoardsTests`.

## Vision run 20261128-0927 (outside the v1 checklist)

- Forced moves measured: `DeductionDepth` fills `forcedMoveCount`; every authored board is fully forced by local deduction (a depth finding, not a label). Tests: `DeductionDepthTests`, puzzle catalog regenerated.
- Lantern letter names the latest chosen thing, truth-guarded (Living mode, a saved choice). Tests: `LanternThingTests`. Tour and device: not run.
- Together remark counts three or four players truthfully; two keep the old line. Tests: `TogetherCountTests`, `HonestCompanionTests`. Tour and device: not run.
- Turn colours in four lightness steps (`TurnPalette`, min contrast 1.36:1). Tests: `TurnPaletteTests`. Greyscale screenshot: not run.
- Spoken turn caption on the take-turns token (`TakeTurns.Spoken`, `TreePlay.Turns.cs`). Tests: `SpokenTurnTests`. VoiceOver/TalkBack: not run.

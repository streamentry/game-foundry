# Mosselora: top 100 improvements, v1

A checklist of 100 improvements still to make, written 2026-11-27 after iterations I1–I372. Each item builds on what's already in the game (the I-number in brackets) rather than repeating it. Tick an item only when it's built **and** its test has been run. A shipped feature isn't the same as a proven one: items about players stay unticked until real adults aged 40+ have tried them.

Each item has three parts: **what to change**, why it matters, and *Test*, the cheapest check that could prove it wrong.

---

## A. The first five minutes

- [ ] **1. One silent opening shot.** Open on the garden at the current local hour and weather, with no logo card, so the first frame already shows the game. *Test:* time from launch to the first tap that does something, under 10 s on a mid-range phone.
- [ ] **2. Learn by one gesture.** The first thing asked is a single tap that waters the garden (I350). Teach every other control only when it's first needed. *Test:* five first-time 40+ players finish the first board without help.
- [ ] **3. Name the garden.** Let the player name the garden (optional, with a suggested name from `RiverNames`), and show it on the cottage sign and on postcards. *Test:* what share of players change the suggested name.
- [x] **4. Mira explains one thing, once.** Cap the opening dialogue at three short lines. Any later help comes from the neighbours in context (I370), never from a tutorial screen. *Test:* count words shown before the first puzzle (goal: under 40).
- [ ] **5. Remember the hand.** On the first long-press, ask once whether the player is left- or right-handed, and mirror the tray and menu to suit. *Test:* thumb-reach heatmap in landscape on a 6.7″ phone.
- [ ] **6. Text size first, not in Settings.** On first launch, show the garden with a size slider whose preview updates live. *Test:* 150% tour (I-series tour) shows no clipped text on any first-run screen.
- [ ] **7. A first win in two minutes.** The first board is sized to finish in about two minutes and ends in a visible change to the garden. *Test:* median first-board time in the playtest kit.
- [ ] **8. No sign-up wall, ever, for solo play.** Audit every first-run path so that family features appear only after the third day, and only as an offer. *Test:* fresh install, airplane mode, play day 1 end to end.
- [x] **9. Restore that the player can trust.** Before any reinstall prompt, show "Your garden is saved on this phone" with the last-saved time. *Test:* kill the app mid-move, relaunch, compare the garden.
- [ ] **10. A two-sentence store promise.** The store promise has to match the first five minutes exactly. Rewrite the store's first line after watching what players do in their first session. *Test:* five-second test of the listing with 40+ readers.

## B. Navigation: icons, one menu, nothing extra on screen

- [ ] **11. One menu button.** Collapse every non-garden control into a single leaf button in the corner, opening to a radial or fan menu of icons. *Test:* count buttons visible at rest on the hub (target: no more than 2).
- [ ] **12. Icons that need no word.** Replace each remaining text button with a painted icon. Keep the word only as an accessibility label and a long-press caption. *Test:* icon-recognition card sort with 8 players aged 40+; any icon below 80% recognition gets redrawn.
- [ ] **13. Context buttons appear where the thumb is.** A tapped thing shows its two actions beside it (play, move) and nothing else. *Test:* taps per action before and after.
- [ ] **14. Back is always the same place.** One back gesture and one back icon position across all sheets (FamilySheet, almanac, nursery). *Test:* tour shot audit of every sheet.
- [ ] **15. The menu remembers.** The fan opens with the last-used item nearest the thumb. *Test:* second-tap distance in a logged session.
- [x] **16. Badges that mean one thing.** One small dot means "something new for you" and nothing else. No counts and no red. *Test:* list every badge source; each must clear when looked at.
- [ ] **17. Sheets slide, never pop.** Every panel enters from the edge it belongs to (letters from the letterbox, nursery from the gate), with motion limited to transform and opacity. *Test:* reduced-motion setting replaces each slide with a fade.
- [ ] **18. Hide the HUD while looking.** After 6 s with no touch, the menu and purse fade to 20% so the garden fills the screen. Any touch brings them back. *Test:* screenshot pairs at rest and in use.
- [ ] **19. Long-press explains anything.** Any icon or thing, long-pressed, shows its name and one line. This replaces help pages. *Test:* coverage script lists every raycast target without a caption.
- [ ] **20. Split `EvergroveApp.cs`.** Break the monolith into Hub, Sheets, Map and Notes partials so navigation changes stop colliding. *Test:* no file over 800 lines; build and tours unchanged.

## C. Immersion: art, light, sound

- [ ] **21. Parallax depth on the map.** Three layers (far hills, garden, near grass) shift a few points on pan. *Test:* 60 fps on the oldest supported iPhone.
- [ ] **22. Cast shadows that follow the sun.** Things cast one soft shadow whose angle follows `DayLight`. *Test:* dawn, noon and dusk tour shots.
- [ ] **23. Water that reflects.** The river mirrors the trees beside it, a faint flipped copy with a ripple shader. *Test:* GPU frame time on device.
- [ ] **24. Footsteps for neighbours.** Neighbours walk between spots with a short step animation instead of appearing. *Test:* tour video at 0.5× speed.
- [ ] **25. A painted transition between board and garden.** The board folds into the thing it came from (extending I293). *Test:* a side-by-side clip reviewed for one continuous motion.
- [ ] **26. Seasonal palette grading.** One colour-grade table per season over the whole scene, not per object (I338–I341). *Test:* grayscale check that contrast stays above 4.5:1 for text in every season.
- [ ] **27. A composed score.** Replace the synthesized ambience with short composed loops per season and hour, plus stingers for finishing a board. *Test:* volume and mute persist; no loop seam audible in 10 minutes.
- [ ] **28. Sound in space.** Pan the river, birds and neighbours' voices by where they are on screen. *Test:* headphone check.
- [ ] **29. Haptics with meaning.** Three haptics only: placed, finished, and something arrived. *Test:* the haptics switch turns all three off.
- [ ] **30. Art consistency pass.** Hold one brush and one line weight across all sprites, and redraw any outlier. *Test:* contact sheet of every sprite at one scale, reviewed for outliers.

## D. Depth: simple to play, more to discover

- [x] **31. Snug fits in Garden Pieces.** Glow only snug placements that leave no unfillable pocket (I373 in progress), so the hint teaches corners-first play. *Test:* snug glow covers under half of the plain-fit squares.
- [x] **32. More good-neighbour pairings.** Extend I368 with second-order pairings, such as a bed beside a grove. Each is a rule you can say out loud. *Test:* every pairing has a one-line rule and a test.
- [x] **33. Garden layouts that remember.** Pairings found per garden layout give a gentle almanac page called "how my garden grew". *Test:* save round-trip.
- [x] **34. Difficulty the player chooses, honestly labelled.** Leaves 1–3 (I336) on every puzzle kind, with the chosen level remembered per kind. *Test:* no kind lacks a gentle level.
- [x] **35. A weekly deeper board.** One harder board each week per favourite kind, with no timer, and no penalty for skipping it. *Test:* generator proves uniqueness at the top tier.
- [x] **36. Technique names.** When a player uses a known technique (a naked pair in seed sudoku), the almanac names it the first time. *Test:* the detector agrees with the solver on 1,000 boards.
- [ ] **37. Boards drawn from the player's own garden.** Mosaic and pictures-by-numbers boards use shapes of things the player grew. *Test:* generated boards are unique and solvable.
- [ ] **38. Chains between puzzles.** Finishing a stream path waters plots; finishing plots feeds the nursery. Each link is one visible, logical cause. *Test:* the cause shows on screen within 2 s.
- [ ] **39. Undo everywhere, without limit.** Every board gets unlimited undo and redo with a visible step count. *Test:* a property test that undo restores the exact state.
- [x] **40. Mistakes shown kindly.** Show conflicts only on request (a lens icon), never automatically in red. *Test:* setting persists; no red anywhere.

## E. The open world: the endless river

- [ ] **41. Scroll beyond the mist as stretches open.** The world extends downward by named stretches (I360). Each opens through play, never by payment. *Test:* save format has no maximum stretch count.
- [ ] **42. Each stretch has a character.** Water meadow, mill, heron pool. Each has its own palette, its own neighbour and its own puzzle flavour. *Test:* three stretches recognisable from a single screenshot.
- [ ] **43. A boat to travel.** A small boat ride between stretches replaces scrolling for long distances. *Test:* any stretch is reachable in 2 taps.
- [ ] **44. Landmarks for wayfinding.** One tall landmark per stretch (mill, bridge, willow) is visible from the next, so players can't get lost. *Test:* a "where are you" test with 5 players.
- [ ] **45. A river map.** A painted scroll map of opened stretches, with your family's gardens pinned on it. *Test:* the map opens from the menu with one tap.
- [x] **46. Things grow outside the garden.** Wild places along the river where planted things spread on their own over days. *Test:* growth is deterministic by date and seed.
- [x] **47. Build simple paths and bridges.** Place stepping stones that join areas, with Minecraft-like freedom kept at a gentle scale. *Test:* placement is grid-snapped and undoable.
- [x] **48. Weather moves down the river.** Rain upstream today reaches you tomorrow, said by a neighbour. *Test:* same seed, same forecast.
- [ ] **49. Night creatures per stretch.** Owls, moths and frogs, visible only in the evening. Each is logged in "who visits" (I327). *Test:* a night tour shot per stretch.
- [ ] **50. Performance budget for an endless world.** Only the visible stretch plus one on either side is built, and the rest is pooled. *Test:* memory flat after 30 stretches of scrolling.

## F. A daily rhythm that is not a chore

- [ ] **51. One small thing each day.** The garden always has one thing waiting (a ripe fruit, a letter, a board), and it's done in under five minutes. *Test:* logged day-2 to day-7 sessions.
- [x] **52. No streak that can break.** Count days played as a growing total, never a streak that resets. *Test:* skipping 3 days costs nothing.
- [x] **53. Missing days are made kind.** On return, the garden shows what grew while you were away (I359), never what died. *Test:* returning after 14 days shows only gains.
- [ ] **54. A morning and an evening.** Different small things wait by local time of day (dew boards in the morning, lantern letters in the evening). *Test:* clock QA hook tours.
- [ ] **55. Calendar of the river.** Real seasonal days per locale (first frost, longest day) become small festivals, not cultural events the game can't do justice to. *Test:* each festival line reviewed per locale.
- [ ] **56. An end to each session.** After the day's thing, a neighbour says goodnight and the garden quietens. This invites stopping, not more play. *Test:* players report a natural ending point.
- [x] **57. Notifications only for real arrivals.** Notifications fire only for a family letter or a parcel, at most one a day, and are opt-in (I273–I277). *Test:* a week's log shows no more than 7.
- [x] **58. A diary page per week.** The almanac writes one painted page per week, made from what happened. *Test:* the page is generated deterministically from the save.
- [x] **59. The year has a shape.** A season tale (I312) unlocks one new kind of thing per season, so the year has chapters. *Test:* a year simulation shows something new every season.
- [x] **60. Come back for people, not prizes.** Every daily reason should trace to the garden or family, never to a timed reward. *Test:* audit list of every return reason.

## G. Family: playing together across distance

- [ ] **61. Build the family-circle backend.** As decided on 2026-11-25: opt-in, simple sign-up, minimal data, account deletion in the app. *Test:* privacy review and a data-map document before any code.
- [ ] **62. Invite by a link or QR code.** A grandparent shows a QR; the grandchild scans it and joins. No usernames to type. *Test:* 3 families join in under 2 minutes each.
- [ ] **63. Visit a relative's garden live.** See their garden as it is now, leave one gift and one note (extending I216). *Test:* a visit works on a slow 3G connection profile.
- [ ] **64. Help with their board.** A visitor can place one move on a family member's board, which they see as "Lan helped here." *Test:* the move survives conflict resolution.
- [ ] **65. Share food.** Jam, fruit and baskets sent to family appear on their table as real objects (extending I228–I232 and I328). *Test:* a send-and-receive round trip.
- [ ] **66. Plant a tree together.** A shared tree grows only when both gardens tend it during the same week. *Test:* the rule holds with clocks in two time zones.
- [ ] **67. Voice notes in letters.** Optional voice notes up to 30 s for players who don't like typing, with an auto-caption shown under each one. *Test:* recording works with VoiceOver on.
- [ ] **68. Child-safe co-play.** Before any build designed for children: the store age declaration, no free text from strangers, and a family-only circle. *Test:* review against the store's child-audience rules.
- [ ] **69. Same-room play.** Two players on one phone take turns (I264) with clear colour and shape markers. *Test:* a grandparent-and-child session in the playtest kit.
- [ ] **70. Family page as a living album.** Moments together (I263, I269) form a page the family can each see. *Test:* the album syncs within a minute.

## H. Economy: simple, logical, fair

- [x] **71. Write the economy on one page.** Seeds in, seeds out, every source and sink, with a daily cap. *Test:* a simulation of 365 days shows no runaway balance.
- [ ] **72. Everything has a reason to cost what it costs.** Nursery prices follow growth time and size, and the price label shows why. *Test:* players predict price order correctly.
- [x] **73. Giving is always cheaper than buying.** Gifts to family cost less than buying the same thing for yourself, which makes generosity the rational choice. *Test:* economy table check.
- [x] **74. A market stall.** Sell surplus fruit and jam to neighbours at prices that move with the season. *Test:* prices are deterministic by date.
- [ ] **75. Recipes.** Two harvests combine into a new food (apple and honey, for example) and recipes are found by trying them. *Test:* every recipe is listed in the almanac once found.
- [x] **76. Storage has a limit you can grow.** A larder with shelves gives a reason to cook, give or sell. *Test:* a full larder never loses items silently.
- [x] **77. No currency for sale that buys power.** If the game is ever monetised, use a one-time unlock or cosmetic season packs only. *Test:* monetisation spec reviewed against the vision's §7.
- [x] **78. Visible ledger.** A small book shows the last 20 seed movements, so nothing in the purse is a mystery. *Test:* every change of `seeds` writes a ledger line.
- [x] **79. Neighbours trade too.** Orin trades wood for fruit and Fen trades stone for jam, at stable, sensible rates. *Test:* no trade loop yields profit.
- [x] **80. Rewards scale down when repeated.** The first solve of the day pays fully and later ones pay less. Play stays open and farming stays pointless. *Test:* simulated grinding gains less than a normal day's play.

## I. Neighbours as quiet companions

- [ ] **81. Neighbours remember what you said.** Reply chips in letters (three icons, not free text) shape the next letter. *Test:* a reply is echoed within 3 days.
- [x] **82. They notice your habits, gently.** "You come by in the mornings" appears after a real pattern, and never in a way that judges. *Test:* the line appears only after 5 morning visits.
- [x] **83. Each neighbour has a life arc.** Orin builds a boat over a season and Fen mends the mill. Their projects move forward as you play. *Test:* each arc has an end.
- [x] **84. Neighbours ask for help.** Small requests (I313 wishes) that need your garden's logic, such as "a flower bed near my door". *Test:* each wish is satisfiable from the current garden.
- [x] **85. Neighbours can be quiet.** A setting for "speak less" halves remarks, for players who prefer silence. *Test:* setting persists.
- [ ] **86. A neighbour speaks your language properly.** Neighbour lines are localised by native writers, not translated, starting with the first localisation market. *Test:* a native reviewer signs off.
- [x] **87. They mark real dates you choose.** The player may add a birthday or anniversary, and the neighbours mark it. It stays local and is never sent anywhere. *Test:* data stays on the device.
- [x] **88. Honest companions.** Neighbours never claim feelings about the player's health or loneliness, and never imply they're real people. *Test:* a line audit against vision §1a's "what we must not claim".
- [ ] **89. Optional on-device language model, researched first.** Only if it runs offline and passes a safety review: a neighbour could vary wording, never facts. *Test:* a spike that compares wording quality and latency on device.
- [x] **90. Goodbyes that matter.** If the player stops for months, the return letter is warm, short and specific to their garden. *Test:* the 90-day absence path in QA.

## J. Accessibility, trust and proof

- [ ] **91. Touch targets of at least 48 pt everywhere.** Audit every raycast target and enlarge any below 48 pt. *Test:* an automated scan in the tour.
- [ ] **92. A colour-blind pass.** Every colour cue is paired with a shape or pattern (puzzle colours, pairing sprigs, fit glow). *Test:* deuteranopia and protanopia simulations on the tour sheets.
- [ ] **93. VoiceOver and TalkBack can finish a board.** Do this for at least one puzzle kind first. *Test:* a blind-tester session.
- [ ] **94. One-hand landscape and portrait.** Every action is reachable with one thumb in both orientations. *Test:* landscape and 150% tours (per the landscape-verification memory).
- [ ] **95. Crash reporting with consent.** Add a privacy-reviewed crash reporter that is off until accepted. *Test:* the privacy label matches what's collected.
- [x] **96. Save is never lost.** Use journaled saves (SaveJournal) with a visible "saved" mark and an export to Files. *Test:* a fuzz test that kills the app during save.
- [ ] **97. Real playtest with adults aged 40+.** Run the daily-loop kit with 8–12 players across the 40–49, 50–64 and 65+ age bands, for 7 days. *Test:* the written report goes in the decision log.
- [ ] **98. Set the Day-7 threshold before launch.** Write down the retention number that would mean "continue" or "stop" before any data exists. *Test:* the number is in the decision log, dated.
- [ ] **99. Store-ready checks.** Check Apple 4.3 uniqueness, the privacy disclosure, the age rating with family play, screenshots of real play, and an English (US) listing. *Test:* `docs/store-publishing-workflow.md` checklist all ticked; submission only with explicit authorization.
- [ ] **100. Delete what didn't earn its place.** After the playtest, remove any feature that no player used or understood, following the AGENTS.md delete-before-optimise rule. *Test:* a list of removed features with the evidence for each.

---

### How to use this list

- Work in batches of five, each logged as `docs/improvements/I[xx]-*.md`, and tick the box here in the same commit.
- Items 61, 68, 89, 95 and 99 need decisions or authorization from the founder before building.
- Items 97 and 98 come before any claim that the game is something people will "play every day".

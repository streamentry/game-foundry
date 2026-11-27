# Return reasons, v1

Checklist 60: come back for people and the garden, never for prizes or pressure.
Every reason the game gives a player to return, daily or periodic, is listed once below.
`Tests/DailyRhythmAuditTests.cs` parses this table and checks the code references, the
garden or family tie, and that no row expires or punishes absence, except the rows named in
its `KnownViolations` list.

Columns: **Garden or family** means the reason traces to the player's garden, its neighbours
or family. **Expires or punishes absence** means the reason is withdrawn, reduced, or made
worse by being away. Both must read `yes` or `no` only.

| Reason | Trigger | Where | Offers | Garden or family | Expires or punishes absence | Notes |
| --- | --- | --- | --- | --- | --- | --- |
| Garden note: letter waiting | An unread letter is in the box (family or neighbour); note at the chosen hour, once a day at most | `Assets/Scripts/Core/GardenNote.cs:ForWeek`, `Assets/Scripts/Unity/GardenNotes.cs:Schedule` | Opens the letter box | yes | no | "It'll keep until you're ready." |
| Garden note: cutting in basket | A family or neighbour cutting has arrived and is in the basket; note at the chosen hour, once a day at most | `Assets/Scripts/Core/GardenNote.cs:For`, `Assets/Scripts/Core/GardenNote.cs:ForWeek` | Opens the basket | yes | no | Cutting stays until planted. |
| Garden note: daily reminder | Game closed with an arrival waiting; one note at the chosen hour, quiet hours 8 to 20 | `Assets/Scripts/Unity/GardenNotes.cs:Schedule`, `Assets/Scripts/Core/GardenNote.cs:NextTime` | Nothing by itself; only the two arrivals above | yes | no | Only arrivals, opt-in, at most one a day; stops after three unopened notes. Takes nothing away. |
| Since you were last here | Opened after two or more days away | `Assets/Scripts/Core/DayWeather.cs:SinceLastVisit`, `Assets/Scripts/Unity/EvergroveApp.cs:NoteReturn` | Weather of the days away; "the garden looked after itself" | yes | no | Said once per return. |
| Today's weather | Each local day | `Assets/Scripts/Core/DayWeather.cs:For`, `Assets/Scripts/Unity/EvergroveApp.cs:TodayWeather` | The day's sky on the hub | yes | no | Redrawn every day; nothing is withheld. |
| Neighbour's word on the weather | A rainy, windy, hot or snowy day | `Assets/Scripts/Core/DayWeather.cs:Remark` | A neighbour's remark | yes | no | Fair days say nothing. |
| Rainbow after rain | A fair day after a rainy one, in daylight | `Assets/Scripts/Core/DayWeather.cs:Rainbow`, `Assets/Scripts/Unity/EvergroveApp.cs:ApplyDayLight` | A rainbow over the garden | yes | no | Judgement call: it appears on one day only. It is a sight, not a reward, and nothing owned is withheld if missed. Founder to confirm. |
| Light, dew and fireflies | Local time of day and season | `Assets/Scripts/Core/DayLight.cs:PhaseAt`, `Assets/Scripts/Core/DayLight.cs:Dew`, `Assets/Scripts/Unity/EvergroveApp.cs:ApplyDayLight` | Mood of the garden at that hour | yes | no | Cosmetic only; the same as the clock. |
| Hour remark | Local time of day | `Assets/Scripts/Core/DayLight.cs:Remark`, `Assets/Scripts/Core/CompanionFacts.cs:CompanionFacts` | A neighbour's word for the hour | yes | no | Cosmetic. |
| Visitors come to the garden | Garden has the right neighbour pairings or flowers, trees or a river; season and hour | `Assets/Scripts/Core/Visitors.cs:Welcomes`, `Assets/Scripts/Core/Visitors.cs:Out` | Bees, birds, heron, hedgehog, owl, kingfisher | yes | no | Come by season and hour, not by absence. |
| Keeper's word about a visitor | Some days, when a visitor has come | `Assets/Scripts/Core/Visitors.cs:KeeperRemark` | A neighbour's remark | yes | no | Remark only. |
| Night creatures on each stretch | Evening (dusk and night) on any open stretch of the river | `Assets/Scripts/Core/NightCreatures.cs:For`, `Assets/Scripts/Unity/GardenNightCreatures.cs:Place` | A moth or frog by the water, logged in who visits once seen | yes | no | Comes by the hour and the stretch, never by absence. |
| Dew board in the morning | Morning, on this device's clock, with the light on | `Assets/Scripts/Core/DayRhythm.cs:Offer`, `Assets/Scripts/Unity/EvergroveApp.DayRhythm.cs:DayRhythmTokens` | A small board by the garden, dealt by the day | yes | no | Costs and pays nothing; dealt again tomorrow. |
| Lantern letter in the evening | Dusk, once a day on this device, with the light on | `Assets/Scripts/Core/DayRhythm.cs:LetterDue`, `Assets/Scripts/Unity/EvergroveApp.DayRhythm.cs:ShowLanternLetter` | A neighbour's short note by the letterbox | yes | no | At most one a day; no reply needed; nothing is withheld if it is not read. |
| Garden cat naps | Always, beside the last tended thing (day) or by the door (dusk) | `Assets/Scripts/Core/GardenCat.cs:NapBy` | A cat to see | yes | no | Has no needs. |
| Seasonal yield from a neighbour | A new season turn, by progress; neighbour pairing present | `Assets/Scripts/Core/SeasonalYields.cs:TakeSeasonYield`, `Assets/Scripts/Core/SeasonalYields.cs:Needs` | Seedling, seeds, fruit or jam | yes | no | Turns move with puzzles solved, not the clock. A season's yield lapses only if progress moves past it without the pairing. Absence alone never lapses it. Flagged for design review. |
| Neighbour's cutting at a milestone | Puzzles solved reach a milestone | `Assets/Scripts/Core/NeighbourGifts.cs:Due`, `Assets/Scripts/Core/NeighbourGifts.cs:CuttingFor` | A cutting in the basket | yes | no | A few milestones only. |
| Neighbour's wish | A wish counted from the save comes true | `Assets/Scripts/Core/NeighbourWish.cs:For` | A thank-you letter | yes | no | Never a timer. |
| Neighbour notices your garden | Talking to a neighbour, when there is something new and true | `Assets/Scripts/Core/NeighbourNotice.cs:Line` | A line about a plant, a puzzle or the river | yes | no | Null when nothing is new. |
| Neighbour answers your reply | A reply chip was sent to a neighbour's letter within the last three days and that neighbour has not answered it yet | `Assets/Scripts/Core/ReplyChips.cs:EchoDue`, `Assets/Scripts/Unity/EvergroveApp.Letters.cs:ReplyEchoSaid` | A neighbour's one-line answer to the reply, said once on the hub | yes | no | Once per reply. A reply not answered within three days lapses silently, as the lantern letter does; nothing owned is withheld. |
| Family parcel: letter, gift, help or lantern | A family parcel is opened by the player | `Assets/Scripts/Core/WorldModel.Family.cs:Open`, `Assets/Scripts/Core/FamilyParcel.cs:TryRead`, `Assets/Scripts/Core/MemoryShelf.cs:For` | Letter, seeds, cutting, fruit, jam, board, help or lantern | yes | no | Letters keep until read. A full larder refuses a parcel and changes nothing; the same text opens later. The letter box keeps the newest 20; the oldest leaves on a new arrival (capacity, not absence). |
| Take turns on one device | Two or more people share one device | `Assets/Scripts/Core/WorldModel.Family.cs:MarkTogether` | A puzzle solved together | yes | no | Cooperative; no scores between players. |
| Replay deals a fresh board | A solved thing is replayed | `Assets/Scripts/Core/ReplayBoards.cs:Seed`, `Assets/Scripts/Core/ReplayBoards.cs:Tier`, `Assets/Scripts/Core/ReplayBoards.cs:Crowned` | A new board, deeper over time | yes | no | Replays never expire. |
| First solve and new pairing seeds | First solve of a thing, or first find of a good-neighbour pairing | `Assets/Scripts/Core/WorldModel.cs:SolveTree`, `Assets/Scripts/Core/Pairings.cs:DiscoverPairing` | Seeds | yes | no | Once each. |
| Old well board | A stretch of river has been opened and its well board is unsolved | `Assets/Scripts/Core/OldWell.cs:Waiting`, `Assets/Scripts/Core/OldWell.cs:SeedFor` | Fruit | yes | no | Waits until solved. |
| Next puzzle of your garden | The previous puzzle is solved | `Assets/Scripts/Core/WorldModel.cs:Advance` | The next story board | yes | no | Progress only. |
| Hub recalls a memory | Each launch | `Assets/Scripts/Core/HubRecall.cs:Next`, `Assets/Scripts/Unity/EvergroveApp.cs:PickHubRecall` | A painting of an earlier choice | yes | no | Driven by launches and saved choices, never the clock. |
| Memory shelf | Opening the book | `Assets/Scripts/Core/MemoryShelf.cs:For` | Firsts kept in the book | yes | no | Read from the save; nothing extra stored. |
| Festival after the story | Spring turn after the story, by progress | `Assets/Scripts/Core/GardenSeason.cs:FestivalAfterStory`, `Assets/Scripts/Core/GardenSeason.cs:Festival` | A yearly festival | yes | no | Turns by progress, not the clock. |
| Nursery: buy a plant | Seeds in hand | `Assets/Scripts/Core/WorldModel.cs:Buy`, `Assets/Scripts/Core/Nursery.cs:Price` | A plant from the nursery | yes | no | Spending, not a timed offer. |

## Checked and not return reasons

- The daily replay cap (v1 #80; `WorldModel.cs:PayReplay`, `ReplayRewardOn`) is an earnings limit,
  not a return reason. The game never presents it as one: no prompt, notice or notification refers to
  it. The day's count simply starts again the next day. Nothing owned is removed, and the replay still
  plays. Decided 2026-11-27 by the coordinator (it was flagged by the audit as a possible lapse).
- `Assets/Scripts/Core/ReturnDiary.cs` is a research recorder of opened dates. It compiles only in
  research or development builds, offers nothing to the player, and is not a reason to return.
- Season, seasonal yields, visitors and the nursery turn with progress, never with the clock
  (`GardenSeason.ForStory` and `AfterStory` take progress). Absence changes nothing in them.
- The save has no days-played total and no streak. The only day-stamped field is `payDay`, a date
  stamp for the replay cap.
- No timers, countdowns, expiring offers or "you were away" penalties exist in the gameplay code.

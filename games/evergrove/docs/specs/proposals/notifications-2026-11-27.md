# Mosselora notifications: letters from the garden

**Status:** proposal and build plan, 2026-11-27 (founder request: "a masterpiece-level notification system to engage users").
**Builds in:** I273 to I277.

## 1. The bar

Most game notifications pull people back with pressure: streaks about to break, energy refilled, "we miss you", countdowns. Mosselora's vision rejects all of that (`vision.md`: "No streaks, energy, wall-clock gates, fake urgency, relationship meters or attachment pressure"). A masterpiece notification here is the opposite. It reads like a short note from a neighbour about something true in your own garden, arrives at a moment you chose, and you are glad to have had it, even on a day you don't open the game.

Engagement we want: a calm daily ritual the player *chooses*, and family moments not missed. Engagement we refuse: anxiety, guilt, interruption, or anything that would embarrass us if a player read our design notes.

## 2. Ten rules (each one is tested or enforced in code)

1. **Invited, never assumed.** Off until the player turns it on. The system permission prompt only ever follows the player's own yes in the game (a "soft ask" in Mosselora's own words first).
2. **The player chooses the hour.** One daily moment they pick (morning, afternoon, evening), like a cup of tea at the same time. Never at night (no notification between 21:00 and 08:00).
3. **One a day at most.** Usually fewer.
4. **Only when something is truly waiting.** If nothing is waiting, nothing is sent. No empty "come play" pings.
5. **True at the moment it is read.** The garden doesn't change while you're away (progress only moves when you play), so what was true when the game closed is still true. Nothing is invented.
6. **Warm, specific, short, from someone.** In a neighbour's voice, naming the actual thing. Never "we miss you", "don't lose", "hurry", "only today", "streak", "come back", counts of days away.
7. **Backs off by itself.** A note is queued only when the game closes, so at most one is ever waiting. If it goes unopened, nothing more is sent until the player opens the game again: one note, then silence. (A guard also stops scheduling after three unopened in a row, should a future version queue more than one.)
8. **Opens where it points.** Tapping it opens the game at what it mentioned (the letterbox, the basket, the thing).
9. **Nothing leaves the device.** Local notifications only: no server, no account, no tracking, no analytics, no third-party SDK.
10. **One switch, in Settings, in plain sight.** Easy to turn off; the hour is easy to change.

## 3. What may be said (priority order)

Chosen when the game closes, from the save, by a pure, tested Core function (`GardenNote`). The first true one wins:

| Waiting in the garden | Who says it | Example |
| --- | --- | --- |
| A letter not yet read | Orin | "There's a letter in your box from Nan. It'll keep until you're ready." |
| A cutting from family in the basket | the keeper of its kind | "Fen: A clover cutting is waiting in your basket for a sunny spot." |
| One drop from more river | Mira | "Mira: One more thing tended and the mist downstream will lift." |
| A thing still holding its first puzzle | its keeper | "Orin: Your weathervane still holds a puzzle of garden plots, whenever you fancy it." |
| A favourite to replay (only if nothing else) | its keeper | "Fen: The forget-me-nots would deal you a fresh table of pairs." |

Nothing else qualifies. Family parcels can't be detected while the game is closed (they travel by message apps), so the system never pretends to know a parcel arrived; the future opt-in family circle could add that honestly.

## 4. Asking for permission

- Never on first launch. The soft ask appears once, after the player has had a real reason to want it: the first time they close a letter, or finish the story's first season (chapter 3), whichever comes first.
- In the garden's own voice, with the hour chosen in the same card: "Would you like a short note from the garden once a day, at a time you choose? Only when something's waiting." Tokens: morning, afternoon, evening, not now.
- Only after a "yes" does iOS show its own permission prompt. "Not now" asks again at most once, much later (after a further 10 solves); after a second "not now", never again (Settings still has the switch).

## 5. Platforms

- **iOS:** a small native plugin on Apple's UserNotifications framework (`MosseloraNotifications.mm`), the same first-party approach as the existing share sheet and haptics plugins. One pending notification identifier, replaced every time the game closes, so there is never more than one queued.
- **Android:** not yet. The Android build schedules nothing and the Settings switch is hidden there until an Android implementation is reviewed (channel, exact-alarm policy).
- **Editor and tours:** a logging stub; the tour shoots the soft ask and the Settings switch, and logs the note that would be scheduled.

## 6. What we'll measure, and the stop rule

- Measured only through the consented research diary, never analytics: how many players turn it on, how many turn it off within a week, and whether returning players report the notes as welcome.
- **Stop rule:** if more than one in four players who turn it on turn it off within a week, or any playtester describes a note as pressure, the system goes back to the drawing board.

## 7. Never claim

That notifications improve wellbeing, memory or connection; that they increase retention (unmeasured); that the notes come from a real person.

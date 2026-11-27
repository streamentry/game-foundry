# I273 — Notifications: the design

## Founder request
- "We should have masterpiece-level notification system, to engage users,
  please write docs and then implement that."

## PM decision
- The design is written first: `games/evergrove/docs/specs/proposals/notifications-2026-11-27.md`.
- The bar is a note from a neighbour about something true in your own garden,
  at a moment you chose, that you're glad to get even on days you don't play.
  The opposite of streak alarms and "we miss you".
- Ten rules, each tested or enforced in code:
  - invited, never assumed;
  - your hour, never at night;
  - one a day at most;
  - only when something's truly waiting;
  - true when read;
  - warm, specific and from someone;
  - backs off by itself;
  - opens where it points;
  - nothing leaves the device;
  - one switch in Settings.
- What may be said, in priority order: an unread letter, a cutting in the
  basket, one drop from more river, a thing still holding its first puzzle, a
  favourite to replay. Nothing else qualifies.
- The permission prompt follows only the player's own yes, after there's a
  reason to want notes. A second "not now" means never asked again.
- iOS first, using Apple's own framework. Android waits for a reviewed
  implementation.
- Stop rule: if one in four players who turn it on switch it off within a week,
  or any playtester calls a note pressure, redesign.

## Dev
- Spec doc only at this step. Rule 7 was corrected after I275: notes are queued
  only on closing, so at most one is ever waiting.

## Result (b294)
- Tours complete in portrait, landscape and 150% text (84 shots each).
  - The river opens (the log shows "solves before 3 (wants 3)").
  - Mira's note waits sealed on the letterbox and is listed on the family
    page (`17u3`).
  - Settings shows the garden-notes switch (`17d`).
  - The soft ask (`17d2`) has legible hours. In b293 the hour labels were
    white on paper; fixed.
  - The tour logs the note that would be scheduled, for example "Orin: Your
    wisteria would deal you a fresh board of garden plots, if you fancy one."
- Found and fixed on the way:
  - the tour had stopped reaching the river step (b290 and b291);
  - the b292 Xcode link failure (see I275).
- Core tests: 347,183 assertions pass.
- Not verified: delivery of a real notification on a device, and the iOS
  permission prompt. Not yet tried by players aged 40+.

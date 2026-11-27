# I310 — A lantern left for you

Idea 43 of the [fifty](../../games/evergrove/docs/specs/proposals/world-games-50-ideas-2026-11-27.md), after wordless co-op games, where you leave a light for a stranger.

## Player

"Sometimes I've nothing to send. I'd just like them to know I thought of them."

## PM decision

"Leave a lantern" is a free gift in Family. The relative's garden gains a lantern on the path below the cottage, up to three shown, the newest nearest. It glows after dusk with the cottage lamp (I296). Tapping the cottage (I299) says who left them: "Lanterns from Nan and Sam". No reply is expected, and nothing is counted.

## Dev

- Core (subagent):
  - `ParcelGift.Lantern`: no amount, no kind, optional letter.
  - `WorldModel.Lanterns` holds sender names, newest last, at most 12.
  - The save gains `lanterns` through `WorldSnapshot` and `GardenTransfer`, with older saves still importing via `matchesPreLanterns`.
- `GardenMapView.SetLanterns` draws the catalogue's own painted paper lanterns (build-96), each with a soft glow at night.
- Tour: `QaLantern` opens a real lantern parcel from "Nan" before the night shots.

## Result

A wordless way to say "thinking of you". It stores names relatives typed themselves, like letters; no other data.

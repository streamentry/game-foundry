# v1 batch 24: items 81 and 22 (ticked, verified in simulator tours)

Date: 2026-11-28. Branch `feat/mosselora-v1-batch-24`, stacked on #155. Two Haiku agents built the items in parallel. The coordinator reviewed and fixed their work, then built the app and ran portrait and landscape tours four times until the shots were right.

## 81 Neighbours remember what you said

A neighbour's letter (Mira, Orin or Fen; never family) has three reply chips, icons only:

| Chip | Icon | Spoken label |
| --- | --- | --- |
| Warm | sun | "That made me smile" |
| Curious | speech leaf | "Tell me more" |
| Thanks | sprig | "Thank you" |

- **Choosing a chip:** tapping one keeps the reply on the device (PlayerPrefs, never the save or the garden file). The chosen chip wears the house check mark, as the settings toggles do.
- **The echo:** within three days, that neighbour's next words on the hub answer the reply, once. Each neighbour has one line per chip, nine in all. "Tell me more" draws on their own stories: Orin's boat and Fen's mill (`NeighbourArcs`), and Mira's shade tree.
- **Lapsing:** a reply not answered in three days lapses silently, with no penalty. A row was added to `return-reasons-v1.md`.
- **The rules are pure:** `Core/ReplyChips.cs`.

**Fixed in review:**
- Three of the agent's echo lines promised things the game never sends, such as "I'll have something cheerful for you soon" and "Come and see the pond soon". They were rewritten, and a test now forbids "I'll", "soon", "next time" and "tomorrow" in echo lines.
- Speaking to a neighbour asks `NeighborSays` twice, once for the heard-line note and once for the bubble. The first ask used up the echo, so the bubble would never have shown it. The answer is now kept for the rest of that day in that garden, with a test.
- The tour step swapped in a fresh world, which crashed the hub build in the tour (`IndexOutOfRangeException`; the game itself never creates that state). It now uses the tour's own garden. It also talked to the wrong neighbour, and in the hub, where neighbours don't speak; both are fixed.

**Tests:** `ReplyChipsTests` checks:
- the chips;
- the echo is due on days 0–3 and not on day 4, once only, and a new reply replaces the old;
- each neighbour's reply is kept apart;
- the round trip of the saved form, and that garbage input reads as empty;
- the echo lines have no loss words, no promises and fewer than 20 words;
- chips appear only on neighbour letters;
- the echo comes before arc remarks and stays for the day;
- the tour shots.

**Tours:**
- `21-reply-chips.png`: Mira's note with the three chips.
- `21-reply-chosen.png`: "Tell me more" checked.
- `21-reply-echo.png`, the next day: Mira says "A little more, then: the shade tree was here long before any of us."

## 22 Cast shadows that follow the sun

Every thing on the map casts one soft shadow, drawn under it from its feet, never taking taps. The pure rule `Core/SunShadows.cs` sets the shadow per phase:

| Phase | Falls | Length (× the thing's width) | Opacity |
| --- | --- | --- | --- |
| Dawn | west (left) | 1.4 | 0.34 |
| Morning | west | 0.85 | 0.40 |
| Noon | under the thing | 0.35 | 0.46 |
| Golden hour | east (right) | 1.2 | 0.36 |
| Dusk | east | 1.4 | 0.34 |
| Night | none | — | — |

- With the daylight setting off, there are no shadows.
- The shadow is a sibling of the thing on the view's node list, so every relayout rebuilds it with the thing.
- `GardenMapView.cs` did not grow: the call sits on an existing line.

**Fixed in review:** at the agent's opacity of 0.20–0.32, the shadows were invisible on the painted ground in the first tour. Opacity was raised and the shadow made thicker. Following the effects-by-count rule, the tour now logs `MOSSELORA_SUN_SHADOWS <phase> <count>`.

**Tours:**
- `22-shadow-dawn.png`, `22-shadow-noon.png` and `22-shadow-dusk.png` at 06:00, 12:00 and 19:00 (Dawn, Day, Dusk).
- The log reports 29 shadows at each phase in both orientations.
- In the shots the shadows are subtle: a soft darkening at the feet, longest at dawn and dusk.

## Checks

- Core tests: 5,181,906 assertions pass.
- Unity compile check is clean with and without `DEVELOPMENT_BUILD`; project and docs checks pass.
- Portrait and landscape tours: 150 shots each, `MOSSELORA_QA_TOUR_DONE`.

## Open

- The chip icons are existing ones (sun, speech leaf, sprig); there is no heart or leaf art.
- The echo lines are English only.
- The shadows are a soft blob, not a cast silhouette of each thing.
- Whether 40+ players notice or value either feature is untested.

# I73 — Live play: return hub shows what your choice shaped next

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

## What I did

I played stream 1 with real taps on the b46 iOS Simulator build: six rotations, then chose wildflowers. I relaunched the app to see the returning-player hub. I sent five screenshots from that session to a fresh Haiku reviewer, asking about daily appeal, which is the weakest axis.

## Haiku review (b46, live session)

| Immersion | Navigation | Icon-first | Polish | Daily appeal | Fit 40–49 |
|---|---|---|---|---|---|
| 7 | 5 | 6 | 7 | 5 | 6 |

The reviewer reported these defects. I added the tagline defect from my own observation.

- **Return hub:** an empty cream band sits between the memory painting and the village strip. Nothing tells the player what comes next, or that their choice changed anything.
- **Memory card:** the consequence line is small grey text.
- **Memory screen (my observation):** the tagline is set at 24 pt bold and runs into the vine corners.
- **Rejected:** "label the buttons Continue/Replay". This is the opposite of the icon-first brief. The spoken names and press-and-hold tips already carry the words.

## Changes (b47)

- **Hub:** the hero window now reaches down to 41% of the screen, and its melt is shorter, so the memory painting meets the village strip's hills and no blank band remains.
- **Hub:** the next stream line is now a row showing the painted icon of your last choice beside the stream's name ("🌼 Rainwater"), with one gentle swell. The stream your wildflowers shaped now shows that visibly. Before, it was the cryptic text "Rainwater · Wildflower sanctuary". The spoken name of Play is unchanged.
- **Memory screen:** the tagline is 19 pt, the hub's size, so it sits inside the vines.
- **Memory card:** the consequence line is 16 pt in full ink, up from 15 pt in moss.

## Evidence

- `I73-live-play-return-hub.png` shows, left to right: hub b46, hub b47, memory b46, memory b47. All four come from real play: solve, choose wildflowers, relaunch.
- Results: C# tests pass with 40,500 assertions, the verifier passes, and the b47 QA tour captured all 19 screens.

## Still unverified

- The daily-appeal score comes from an AI reviewer, not from players.
- The decisive test is still 8–12 uncoached sessions with adults 40+ (m3-session-kit P1–P9).

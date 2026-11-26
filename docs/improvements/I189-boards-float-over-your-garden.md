# I189 — Boards float over your own garden

Date: 2026-11-26. Branch: feat/mosselora-world-first.

This opens a new run of iterations (founder goal, 2026-11-26): *make Mosselora insanely great for adults 40+: icons instead of word buttons, smart navigation with one menu instead of every button at once, immersive rather than web-like, and an open garden you shape, like a gentle Minecraft or Roblox.* Each iteration is played first (player), decided (PM), built and checked on the simulator (dev), then committed and pushed.

## Player (b209, iPhone 17 simulator, played by hand from a fresh garden)

What I noticed, in the order I met it:

1. **Every screen is a web page.** Wordmark header, stepping stones, sign, board, drops, then a strip of garden cut off at the bottom, all on beige paper in one scrolling column. The garden, the thing I care about, is a 260 pt strip at the foot of the page and Fen is cut off in it.
2. **The logo is on every board.** The wordmark takes the top sixth of the screen on every stream. I know which game I'm in.
3. **Home is half garden, half paper form**: logo, tagline, stones and a long "→ First Bend" word button on paper.
4. **Word buttons where a picture would do**: the play button, the choice cards ("Plant a silver birch"), every settings row ("Language: English", "Text size 100%", "Gentle chime").
5. **The menu is a long form**: wooden word buttons stacked like a settings web page; the book has its own corner button as well.
6. **The garden is one fixed picture.** I can pan and place what I earned, but I can't zoom, I can't plant another tulip, and the land never gets bigger. It doesn't feel like *my world*.
7. On the memory card the tagline bubble sits over my garden; there's an orange smudge under the birch.
8. The choice cards wear a small book-page badge I can't read.

Good already: solving feels great, the drops meter and bulb are clear, the plank sign is lovely, placing the birch by tapping the grass worked first time.

## PM decision: the run's direction

**The garden is the world, and everything happens on top of it.** Order of work (highest player value first, each one shippable on its own):

| # | Iteration | Why |
|---|---|---|
| I189 | Boards float over your own full-screen garden; no logo header on boards | Kills the web-page feel where the player spends most time |
| I190 | Home is the full-screen garden with one round play button | First impression = your world, not a form |
| I191 | One menu: the corner button opens a fan of round icons (book, almanac, settings, postcard) | Fewer buttons on screen, smart navigation |
| I192 | Settings as big icon tiles; research/dev rows never in a player build | The menu stops being a form |
| I193+ | Free planting: every kind you've unlocked can be planted again and again from a seed basket | The Minecraft/Roblox loop, simplified: unlock by solving, build by placing |
| next | Pinch to zoom; drag to move; the land grows as streams are restored; ambient life; seasons on the map; a small daily gift | An open world that is alive and worth a daily visit |

Constraints kept: large targets, no timers, no streaks, every icon keeps a spoken label for screen readers, landscape and 150% text still work.

## This iteration

Portrait boards now stand **on the player's own garden map, full screen**: the same map as home, with everything they made standing where they put it, a light warm veil so the board stays the brightest thing, and the newest thing peeking out below the board. No wordmark header and no paper. The garden strip under the board, the creek banner behind it (I167) and the paper band that appeared on scroll are gone on these screens; the world behind replaces all three. The stones and the plank sign sit straight on the grass.

## Dev

`EvergroveApp`: a full-screen `Living world` node under the page (`worldStage`). `WorldStage(show)` rebuilds a non-interactive `GardenMapView` there (width 1.1×, focus low in the window) with a 20% paper veil. `Rebuild` shows it for living-mode portrait boards, replaces the wordmark header with 50 pt of room under the corner buttons, skips the page garden band, and hides the chrome band; `BuildPuzzle` skips `CreekBehindBoard` while it shows. Landscape, the memory card and the ending are unchanged. `DevQaTour` only scrolls to the village band if it exists.

## Result (b211; portrait, landscape and 150% tours, 23 screens each, all complete)

`I189-world-stage.jpg`: first board with Mira's tip, the solved board with its choice cards, stream 2 over the garden with the birch, the winter board with the gazebo, flowers and well standing below the board, and the memory card (unchanged). At 150% the tip bubble and board fit; landscape is unchanged. Project verify passes. .NET tests not run (no Core change).

**Not done yet:** the world doesn't show the season (winter looks like summer); the drops sit over items on the map; home is still half paper (I190).

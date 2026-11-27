# I291 — Season stones in garden tiles

Idea 2 of the [fifty](../../games/evergrove/docs/specs/proposals/world-games-50-ideas-2026-11-27.md), after mahjong's flower and season tiles, which match within their set.

## Player

"Every pair of tiles was identical. Nothing surprised me."

## PM decision

One set of four tiles is drawn as the four seasons: a mossy spring stone, a summer stone, an autumn stone with a leaf, a frosted winter stone. They look different, and any two match. The picture rule shows it the first few times: two different stones, an arrow, a tick.

## Dev

- `MahjongGame.SeasonFace = 0`. Face 0 is dealt twice at every tier, so it always has exactly four tiles. `SeasonTiles(game)` lists them in tile order. The matching rule is unchanged (same face matches), so no rules code moved.
- `MahjongBoard.PictureOf(t)` draws the four season stones (the existing `stone-*` art) on those tiles. The first-time rule gains a second pair: spring and autumn stones, then a tick, after a wider gap (`BoardKit.RuleGap`).
- Tests (first): exactly four season tiles at every tier, for 40 seeds per tier, listed in a fixed order.

## Result

The garden-tiles board has one family of differing tiles that match. Spring and summer stones look alike at small sizes. Unverified: whether players read "different, yet a pair" without words.

# I176 — Home opens on your own garden map

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player (founder: "dynamic / open world… the fixed picture makes it hard to put trees and flowers")

The home screen, the first view of every day, was a fixed painting: the same village whatever you'd made.

## PM decision (E3)

In the living garden, the home screen's top window is now **your garden map**: the top-down valley from I175, **pannable with one finger** (elastic, with inertia), and **every tree, flower and building you've made standing in it**. Each is placed by `GardenMap.Layout`: trees toward the edges, flowers in the meadow, builds on the creek bank, filling the valley top to bottom through the year. Things are drawn back to front, so nearer ones overlap further ones, and trees stand taller than flowers. It opens centred on the newest thing you made. Butterflies still visit (I157).

The village strip with Mira, Orin and Fen stays below for now. In E6 they come to walk the map instead.

## Dev

- `tools/evergrove_art_pipeline.py`: ships `garden-map.png` and derives `Resources/Evergrove/garden-map-mask.txt` (10×15) from the painting itself. Cells more than 10% teal are creek (`~`); the top-right corner (cottage) and tan cells in the two right columns (footpath) are built (`#`); everything else is land.
- New `Unity/GardenMapView.cs`: a `ScrollRect` over the plot image (1.5× the window's width, keeping the painting's 2:3), with item images bottom-pivoted on their cells (1.7 / 1.35 / 1.05 cells tall for trees, builds and flowers), sorted by row and re-laid out when the window changes size.
- `EvergroveApp`: `LoadGardenMap()` and `GardenMapOnHub(window)` (living mode; falls back to the painting if the art or mask is missing). Development-only `QaShowHub` lets the tour photograph the hub.
- `DevQaTour`: after the year's twelve streams, saves `17-hub-map.png` (23 screens per tour).

## Result (b187, iPhone 17 simulator)

`I176-hub-map.jpg` (gitignored): portrait, landscape and 150% hubs show the map with the creek, the cottage, and the year's items (outdoor stage, weeping willow, juniper and more as you pan) above the neighbours, the wordmark, Fen's line about the wisteria, and Play. Portrait, landscape and 150% tours pass (23/23/23). Verify passes.

**Not verified:** panning by hand on a device, and how it feels with 12+ items (the next iteration places them by hand). The village strip below now repeats the creek, and folds into the map in E6.

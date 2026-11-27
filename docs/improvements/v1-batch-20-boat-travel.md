# v1 batch 20: item 43 (ticked, verified in a simulator tour)

Date: 2026-11-28. Branch `feat/mosselora-v1-batch-20`, stacked on #151. One Haiku agent built it; the coordinator added the back-gesture line, built the app and ran the QA tour.

**43 A boat to travel.** The menu's "Travel by boat" opens a sheet listing every open stretch of the river by name. It never lists a stretch still in mist, and the current one is marked with a check and "you are here". Tapping a stretch glides the rowing boat (the catalogue sprite) along the river while the view eases there in ≤ 1.1 s. With Gentle motion off, it jumps straight there. The back gesture closes the sheet.

**Test:** `RiverTravelTests` shows the flow is exactly 2 taps (menu entry, then stretch) for every stop, for 1–200 open stretches. Tour shots `17zc-boat-travel.png` and `17zd-boat-arrived.png` show the sheet and arrival on the simulator.

**Open:** the tour travelled to the stretch it was already on, so the glide itself wasn't captured. The "you are here" mark follows the view, not the last boat stop.

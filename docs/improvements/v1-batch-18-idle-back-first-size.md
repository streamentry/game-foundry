# v1 batch 18: items 6, 14, 18 (ticked, verified in simulator tours)

Date: 2026-11-28. Branch `feat/mosselora-v1-batch-18`, stacked on the tour-fix PR (#148). Three Haiku agents built the features; the coordinator added tour shots, ran the QA tour on the iOS Simulator (iPhone, iOS 26.5) and fixed what the tours showed.

- **18 Hide the HUD while looking.** After 6 s with no touch on the home garden, the corner controls (menu, seed purse, letterbox, basket) fade to 20% (`Core/IdleFade`, alpha only, smoothstep over 0.8 s; with Gentle motion off they simply dim). Any touch brings them back, and a tap on a faded button still works. Nothing fades while a sheet, board, letter or visit is open. **Tour test:** `17-hub-idle.png` and `17-hub-awake.png` are the at-rest and in-use pair. The first tour showed the menu button not fading, because it lives in the hub's own chrome bar; fixed and re-verified.
- **14 Back is always the same place.** Every sheet and card (menu, journal, seed book, stall, name, dates, deeper boards, family, letter, choice preview, puzzle card) now closes from one top-right control: 56 pt, the cancel icon, a 12 pt inset (`Core/BackPlacement`). One back gesture (Escape or Android back) closes the topmost layer, in a fixed order. `BackPlacementTests` audits 11 sites. **Tour test:** sheet shots across the tour (for example `17u-letter.png`) show the corner close. **Open:** the transient "Good neighbours" toast can briefly cover a card's corner until it fades. Android back still needs a check on a device.
- **6 Text size first, not in Settings.** On first launch, a small panel over the first board offers three sizes (100/125/150%) that apply live, plus a tick to dismiss; it never blocks and appears once. First-run words stay at 25 (goal: under 40). **Tour test:** in the 150% tour (`tour-b18x`), `01-chapter1-start.png` and `01a-first-size.png` show no clipped text on the first-run screens.

## Verification

- `.NET` suite, run as CI does: passes (back controls, idle fade, first-size and first-minutes audits included).
- Unity 6.3 runtime compile: 0 errors with both the release and dev define sets.
- Simulator builds plus QA tours in portrait 100% and 150% ran to `MOSSELORA_QA_TOUR_DONE`. The landscape argument didn't take effect in this batch's run; the earlier landscape tour (`tour-v1l3`) covered the card fit.
- **Not run:** a physical device.

# I219 — Restoring says "restore", not "erase"

## Player (from I213's hands-on)
- Restoring a garden I wanted, the confirm button read "Confirm: erase this
  garden" in alarm red. True (it replaces the garden on this phone), but at the
  moment you're getting your garden back, "erase" makes you stop and wonder if
  you're about to lose it.

## PM decision
- The confirm button names the action: "Restore from garden file", in the
  restore's blue, with its own icon. The plain warning above it stays: "This
  replaces your current garden." The red "erase" wording stays where it's
  right, on starting a new garden.
- No new string: the existing key is reused, so all 29 draft translations stay
  complete.

## Dev
- `EvergroveApp.BuildGardenFileSection`: confirm button caption
  `garden_restore`, icon `IconRestoreFile`, tone `WaterAction` (was `erase`,
  `IconConfirm`, `EarthAction`).

## Result
- Seen in the next batch's settings tour; not tested with players.
- `I219-restore.jpg` (local only): settings and the garden-file card (b266). The
  tour doesn't open the confirm step, so the new button was checked in code,
  not on screen.

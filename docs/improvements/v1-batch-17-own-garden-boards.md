# v1 batch 17: item 37 (ticked)

Date: 2026-11-27. Branch `feat/mosselora-v1-batch-17`, stacked on #144. One Haiku agent did the work, and the coordinator reviewed it.

**37 Boards drawn from the player's own garden.**
- Your own things' nonograms are now drawn from pictures of what you grew (`OwnGardenBoards.NonogramFor`), and each is proven to have exactly one solution.
- Your own mosaics use the painting of the thing you're solving.
- A relative's shared board, the weekly deeper board and the well keep the standard generators exactly. The standard generator's fingerprint over seeds 0–119 is unchanged.
- **Sharing stays exact:** `SharedSeedFor` finds the standard seed that deals the identical board, so a relative gets the very board you solved. All 765 sampled own boards have one, and "send" is hidden only if none exists.
- Undo rebuilds the same own board, and the leaves chooser measures own boards. 270 of 306 chosen levels find an own board at that level; the rest keep the dealt board and show its real leaves.

## Verification

- `.NET` suite, run as CI does: 2,183,122 assertions pass.
- Unity 6.3 runtime compile: 0 errors with both the release and dev define sets.
- `verify_evergrove_project.py` and `check_evergrove_docs.py`: OK.
- **Not run:** editor tour and device.

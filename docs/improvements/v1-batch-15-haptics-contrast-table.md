# v1 batch 15: items 26, 29, 65 (ticked)

Date: 2026-11-27. Branch `feat/mosselora-v1-batch-15`, stacked on #142. Three Haiku agents worked in parallel on separate files, and the coordinator reviewed their work.

- **29 Haptics with meaning.** `Haptics` now exposes exactly three cues behind one switch (`evergrove:haptics`, in settings):
  - `Placed`: 5 call sites, such as a tile turned or a thing placed.
  - `Finished`: a board solved.
  - `Arrived`: a letter arrives; this cue is new.
  
  Haptics on giving fruit and on tapping a visitor were dropped as meaningless. `HapticsMeaningTests` proves every call site uses only the three cues and that each cue returns before any native call when the switch is off. **Review fix:** until the player sets the switch, it follows their earlier Gentle motion choice, which used to gate haptics, so nobody who turned motion off suddenly feels buzzing.
- **26 Seasonal palette grading.** `SeasonGrade` is one light overlay per season over the whole painted scene (spring, summer, autumn, winter tints at 6–10%), drawn as a single layer on the map. Built things and text sit above it. `SeasonContrastTests` checks 10 text/background pairs × 4 seasons against the real palette parsed from source. It found and fixed two failures: river-name signs (EarthInk, 4.15:1 → Ink, 8.16:1) and the beyond-the-mist text in autumn (3.55:1 → 4.91:1). The minimum is now 4.91:1.
- **65 Share food.** `FamilyTable.Items` lists the pantry's fruit and jam, and the cottage card draws one small pear or jar per item (up to 9 and 6), each with a long-press caption giving the true count. `SharedFoodTests` runs a full send-and-receive round trip: received food appears as objects, the sender's counts drop, and the list survives save round trips. **Limit (founder decision):** the save doesn't record which food came from family or who sent it, so the table shows the whole pantry with counts only. Exact "from Anh" needs a save-format change.

## Verification

- `.NET` suite, run as CI does: passes, with the haptics and contrast summaries printed.
- Unity 6.3 runtime compile: 0 errors with both the release and dev define sets.
- `verify_evergrove_project.py` and `check_evergrove_docs.py`: OK.
- **Not run:** editor tour and device. Haptic feel and the settings row width with a fifth tile are untested. The new haptics toggle label is English only.

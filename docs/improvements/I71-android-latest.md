# I71 — Android emulator re-check of the latest design

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

**What.** Re-verification of the latest design (as of I69) on **Android**. The previous Android run (I45) predates the plaques, picture-first choices, icon-only hub, goal glows, seasonal stones, vignettes and accessibility fixes.

**Method.** A development APK built from the I69 source, installed on this session's own emulator (AVD `mosselora_painted`, Android 36, 1080×2400, confirmed by `ro.boot.qemu.avd_name`). The QA tour was launched via the `--es unity` extra.

**Result: PASS (emulator).** `MOSSELORA_QA_TOUR_DONE 19`, and all 19 PNGs were pulled. In the device's Bengali locale (`I71-android-latest.png`):
- painted enamel and wood plaques on the menu and tools
- the picture-plus-sentence coach with the glowing spring and garden
- seasonal stones
- picture-first choice cards with the keepers' portraits
- the memory vignette of Mira planting with one remembered line
- the painted tree on the rooted tile
- neighbors on cream name plates

**Not verified.** A physical Android phone, TalkBack, and native Bengali review (EG-006). English remains the only shipping language.

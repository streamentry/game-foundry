# I45 — Android emulator run of the new design (Bengali)

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

**What.** A cross-platform check of everything since I01 on **Android**, which the iOS-only iterations had not covered.

**Method.** A development APK built from the I43 source (`EvergroveBuild.Build`, Android target), installed on this session's own Android 36 emulator (AVD `mosselora_painted`, 1080×2400, identity confirmed by `ro.boot.qemu.avd_name`, so no other session's device was touched). The QA tour ran via the `--es unity '-mosseloraQaTour …'` launch extra.

**Result: PASS (emulator, not a phone).** Logcat shows `MOSSELORA_QA_TOUR_DONE 19`, and all 19 PNGs were pulled. The emulator's device language is Bengali, so the run doubles as a non-Latin script check (`I45-android-bengali.png`):
- Mira's coach bubble, the garden bed board, the droplets and the icon tools with their stream-1 captions render in Bengali.
- The three painted choice cards show their keepers' portraits.
- The journal sheet's scrapbook pages wrap Bengali text inside the cards.
- The winter village shows on stream 11, with the 4×4 board in its bed.

**Not verified.** A physical Android phone, TalkBack, and Bengali shaping quality: the existing EG-006 spike says Bengali still needs native review, and English remains the only shipping language.

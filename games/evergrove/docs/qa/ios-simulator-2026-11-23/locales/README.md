# iOS Simulator locale check: Japanese (ja) and Vietnamese (vi)

Date: 2026-11-23. Scope: visual check of two near-term locales in the real game UI on the iOS Simulator. Not a certification.

## Method

- Build: development iOS Simulator app (`MosseloraPlaytest.app`, bundle `com.streamentry.evergrove.playtest`) exported with `EvergroveBuild.Build` (default development channel, `BuildOptions.Development`, `EVERGROVE_IOS_SDK=simulator`), then built with `xcodebuild` (Debug, iphonesimulator, arm64).
- Device: "Mosselora QA iPhone" (UDID 938A516A-5AD8-459E-A727-503402F3D304), iOS Simulator, 1206x2622 screenshots.
- Language: simulator global `AppleLanguages` / `AppleLocale` set to `ja` / `ja_JP` or `vi` / `vi_VN`, device shut down and rebooted. The app was uninstalled and reinstalled before each run, so each run had a fresh save and no language preference. Locale detection is Automatic (device language).
- Tour: `SIMCTL_CHILD_mosseloraQaTour=<container>/Documents/tour-<lang>` launches the `DevQaTour` script, which drives the real buttons and tiles and writes 14 PNGs. All 14 were produced for each language.
- Confirmation of locale: the in-game language button reads `言語: 日本語` (ja) and `Ngôn ngữ: Tiếng Việt` (vi), and body copy is in the target language.
- Simulator restored to `AppleLanguages en` / `AppleLocale en_US` and rebooted afterwards.

Screens reviewed: 01, 03, 04, 10, 11 per language. Full sets are in `/Volumes/SSD/mosselora-builds/tour-ja/` and `/Volumes/SSD/mosselora-builds/tour-vi/`.

## Findings

Reviewer is not a native speaker. Verdicts are visual checks only.

### Japanese (ja)

| Screen | Verdict | Note |
| --- | --- | --- |
| 01-chapter1-start.png | PASS | Kana, kanji and full-width punctuation render with no tofu. Buttons and hint text wrap cleanly. |
| 03-chapter1-solved-choice.png | PASS | Three choice cards wrap in two lines and stay inside their boxes. |
| 04-consequence-shade-tree.png | PASS | Consequence text, the Continue button and the status chips fit. `▦ Standard` shows a symbol glyph, not a box. |
| 10-journal.png | ISSUE (minor) | Line break in the water-route note leaves a lone `。` at the start of a new line (line-start punctuation, kinsoku rule). Dev-only buttons such as `Start matched control (dev)` remain in English. |
| 11-ending.png | PASS | Ending heading and body are in Japanese, no clipping. Dev-only buttons are in Japanese. |

Overall: PASS with one minor line-break issue. Dev-only buttons are not localized (not player-facing).

### Vietnamese (vi)

| Screen | Verdict | Note |
| --- | --- | --- |
| 01-chapter1-start.png | PASS | Diacritics (ò, ệ, ớ, ỏ and similar) stack correctly with no clipping. Hint text wraps to two lines without overlap. Action buttons (`Hoàn tác`, `Gợi ý cho tôi`) fit. |
| 03-chapter1-solved-choice.png | PASS | Choice titles and descriptions with diacritics fit inside the cards. |
| 04-consequence-shade-tree.png | PASS | Journal-style heading and buttons fit. Dev-only `Start matched control (dev)` is in English. |
| 10-journal.png | PASS | Multi-line diacritic text in the choice summary wraps cleanly. Dev-only buttons in English. |
| 11-ending.png | ISSUE | The final footer line at the bottom of the screen is cut off at the viewport edge. Needs a re-check after scrolling, to tell a real clipping bug from the capture position. |

Overall: PASS with one open item (footer clipping on screen 11) that needs a re-check. Dev-only buttons are in English.

## Limits

- Machine-drafted translations, unreviewed. Native-speaker review is required before any claim of quality.
- Non-native review only. Japanese line breaking and Vietnamese diacritic placement were judged visually, not against a style guide.
- Simulator fonts are iOS system fonts (the uGUI font chosen by `LocaleFonts`). Physical-device glyph coverage and font fallback are not tested here.
- Text shaping and RTL were out of scope. Arabic, Hebrew and Indic locales were not run.
- Dev-only buttons are not player-facing and are excluded from the verdicts.
- Physical iPhone and native-speaker review still required.

## Lead review addendum (2026-11-23)

Found on `ja/04-consequence-shade-tree.png`; these likely apply to every non-English locale:
1. **Untranslated character name in the memory card:** "第1章 · Mira · 木陰をつくる木を植える" uses the Latin name "Mira", while the NPC line above uses "ミラ". The memory-card name is not localized.
2. **Board-size button is English:** "▦ Standard" / "▦ Large" are English words, not a language-neutral label like "Aa 100%".

## Follow-up: kinsoku and vi footer re-check (2026-11-23)

- **Japanese line-start `。` (not fixed).** Tried inserting U+2060 WORD JOINER before closing punctuation and after opening brackets in the label/button text path (legacy uGUI Text). Rebuilt for the iOS Simulator and re-ran the ja tour. `ja/10-journal-after-fix.png` still shows a lone `。` at the start of the last line after "回転できません", so uGUI does not honor U+2060 for this wrap. The change was reverted and nothing was committed for it. Options to try next: a line-break-safe font or TextMeshPro, or a small text-layout pass that moves the punctuation onto the previous line. Not verified.
- **Vietnamese ending footer (ambiguous, not fixed).** `vi/11-ending-after-fix.png` (same as `vi/11-ending.png`) shows the last footer line cut at the bottom edge of the capture. The tour captures one viewport and does not scroll, so this does not prove clipping. The footer is probably below the fold in the scroll area. A swipe-based check on the simulator is still needed to decide.
- Simulator language restored to `en` / `en_US` and rebooted. Dev-only buttons are still English, as noted above.

## Verdict: vi ending footer (2026-11-23)

- **Not clipped; it was below the fold.** The tour now scrolls the main ScrollRect to the bottom and captures `vi/11b-ending-bottom.png`. The footer line ("Không cần mạng · Không giới hạn giờ · Không quảng cáo · Không tài khoản", wrapping to "khoản") is fully visible with clear space below it. The unscrolled `vi/11-ending.png` only cut it off because the capture stopped at the first viewport.
- Bottom captures `04b` and `10b` were also added to the dev-only tour; those screenshots were not reviewed for this verdict.
- No player-facing layout change was made.

## Result: CJK line-start punctuation fixed via measured line breaks (2026-11-23)

- Approach: uGUI does not honor U+2060, so `KinsokuLineBreaks` (Core) measures each CJK label's wrapped lines and inserts an explicit `\n` one character earlier when a line would start with closing punctuation (or end with an opening bracket). `CjkLineBreakFixer` (Unity) keeps the original source and re-fits on width or font-size change. Applied through `Label` and the feedback label, only for `ja`, `zh-Hans` and `zh-Hant`.
- Evidence: `ja/10-journal-kinsoku-fixed.png`. The water-route note now ends `回転できませ` / `ん。`, and no line starts with `。` or `、`. The previous lone-`。` line is gone. Checked on the iOS Simulator (ja, fresh install); 04 and 11 show no line starting with closing punctuation.
- Limits: only Label text is covered. Button captions and the dev-only English buttons are not. Verified on the Simulator only, not on a physical device or with a native-speaker review.
- Simulator language restored to `en` / `en_US` and rebooted.

## Persian (fa) and Urdu (ur), lead check after the RTL fix (2026-11-23)

These tours use the same iOS Simulator development build as the fixed Arabic run (line-by-line bidi, no exception console). Non-native reading:
- **fa: PASS.** Letters are joined, the text is right to left and right-aligned, ZWNJ is honored (انتخاب‌های, می‌کند), and "MOSSELORA" and "1" are not reversed.
- **ur: PASS (rendering), font style open.** Letters are joined, including Urdu-specific forms (ے ہ ڑ), and text is right to left. iOS draws it with a Naskh-style system font. Urdu readers usually expect Nastaliq, so a native reviewer must decide whether that is acceptable.
- No Development Console exceptions appeared on any captured screen. Both locales stay off the shipping allowlist pending native review.

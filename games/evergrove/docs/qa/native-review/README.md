# Native review packets

Each packet lists every player-facing catalog string for one locale next to the English source, with a placeholder check and blank verdict columns. Generate or refresh one with:

```sh
python3 tools/export_locale_review.py <locale> --out games/evergrove/docs/qa/native-review/<locale>-review-packet-<date>.md
```

Re-running the exporter regenerates the whole packet, including its screenshot links and blank sign-off block. Record verdicts privately, not in this repository. A packet is not a certification; the lifecycle and certified-record fields are in [05-ACCESSIBILITY-AND-LOCALIZATION](../../specs/05-ACCESSIBILITY-AND-LOCALIZATION.md).

## How to review

1. Read the packet's table, row by row, against the English source. Use the simulator screenshots as context for where each string appears.
2. View the locale's screenshots, linked from the packet.
3. If you can, play the first stream and reach the first choice on a real iPhone. Simulator screenshots do not show touch targets, real fonts or VoiceOver.

## What to judge

- **Meaning:** Does each line say what the English says, without adding claims about the game, health or memory?
- **Tone:** Warm, clear and respectful of adults aged 45 and older. Avoid childish phrasing, "senior" stereotypes and over-explaining. Keep adult competence and agency.
- **Gender and address:** Check how the NPC lines (Mira, Orin, Fen) refer to the player and themselves. Keep one consistent register across the catalog.
- **Numerals and counters:** Check the stream counters (`STREAM {0} / {1}`) and the progress line (`{0}/{1}` tiles, `{2}` joins).
- **Culture and consistency:** Check that terms (for example for "stream", "garden", "memory") are used the same way on every screen and match the game's choices.
- **Placeholders:** The Placeholders column is only a mechanical flag. A native reviewer still needs to check that the sentence reads correctly with the values in place.
- **Line breaks:** Note any line that breaks badly, especially a lone punctuation mark at the start of a line (for Japanese, the journal kinsoku issue in the screenshot set).
- **Compact layout:** Note any text that is too long for its button or card.

## What NOT to do

- Do not put reviewer names, email addresses, phone numbers, social handles or other contact details in this public repository. Record reviewer identity privately, outside the repo, and reference it only by an internal ID in the sign-off block.
- Do not change the catalog, the English source or any game code as part of this review. Record suggested changes in the verdict and note columns; a separate change applies them.
- Do not record a verdict of OK for a locale as a certification. Certification needs the full lifecycle in `docs/specs/05-ACCESSIBILITY-AND-LOCALIZATION.md`.
- Do not treat the existing simulator PASS marks (from a non-native reviewer) as native review.

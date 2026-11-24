# I05 — Localized names for menu, close and size controls

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

**Player problem.** Icon-only controls must still have a proper name. VoiceOver and TalkBack users need one, and so does anyone using press-and-hold to learn an icon. The menu button borrowed "Language · Aa", the close button said "Continue", and the size buttons said "Aa 100%" and "▦ 1×".

**Change.** Four new localization keys: `menu`, `close`, `text_size` ("Text size") and `board_size` ("Larger board"). They are in the English source table and all 29 draft catalogs (105 keys each). The size buttons speak a name plus the current state, for example "Text size 125%" or "Larger board 1.2×".

**How the translations were made.** Three Haiku sub-agents drafted the strings in parallel (10/10/9 locales), each writing a JSON map that a script merged into the catalogs, appending 16 lines per file with no reformatting. Samples: de "Menü / Schließen / Textgröße / Größeres Spielfeld"; ja "メニュー / 閉じる / 文字の大きさ / 盤面を大きく". **These are drafts.** Like every non-English catalog they need native review before they ship (LOCALIZATION.md), and English remains the only shipping language.

**Evidence.** C# tests pass (39,796 assertions; key count 105). The verifier reports 29 complete catalogs with 105 keys each. In the b17 screen-reader tree the menu button is spoken as "Menu".

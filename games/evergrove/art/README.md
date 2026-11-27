# Mosselora painted UI art

Original watercolor/gouache storybook art for the in-game UI, generated on 2026-11-23 with the Codex CLI `imagegen` skill (built-in `image_gen` tool, OpenAI image model). Reference screenshots in `../screenshots/` (themselves Codex-generated design mockups) were attached as **style references only**; no third-party game art, logos or trade dress were supplied or requested.

| Master (`generated/`) | Runtime sprites (`Assets/Resources/Evergrove/Art/`) | Used for |
| --- | --- | --- |
| `banner-stream-flowing.png` | `banner-stream-flowing` | Village strip, alpha follows real spring reach |
| `banner-stream-dry.png` (edit of the flowing master) | `banner-stream-dry` | Village strip before water arrives |
| `vine-corner.png` | `vine-corner` (mirrored at runtime for the right corner) | Page header |
| `wordmark.png` | `wordmark` | In-app header wordmark (title text kept for screen readers) |
| `icons-choices.png` (2x2 sheet) | `icon-shade-tree`, `icon-bridge`, `icon-wildflowers`, `icon-foundation` | Choice cards and painted landmarks |
| `icons-small.png` (2x2 sheet) | `glyph-sprout`, `glyph-droplet`, `glyph-sprig`, `glyph-waves` | Garden/spring tile markers, dividers |
| `icons-ui.png` (4x4 sheet of black glyphs, recolored to white) | `ui-undo`, `ui-hint`, `ui-restart`, `ui-language`, `ui-text-size`, `ui-board-size`, `ui-chime-on`, `ui-chime-off`, `ui-journal`, `ui-continue`, `ui-change`, `ui-copy-file`, `ui-restore-file`, `ui-new-garden`, `ui-confirm`, `ui-cancel` | Icon-first buttons, tinted per tone |
| `icons-food.png` (2x2 sheet, prompt `prompts/food.txt`, 2026-11-27, `icons-small.png` attached as style reference) | `food-pear`, `food-basket`, `food-jam`, `food-apple` | Fruit picked from your trees, shared with family and neighbours (I228) |
| `neighbors.png` (3 medallions) | `portrait-mira`, `portrait-orin`, `portrait-fen` | Neighbor portrait buttons |
| `hero-village.png` | `hero-village` | Ending screen painting |

Exact prompts: `prompts/*.txt`. Regenerate runtime sprites after replacing a master:

```bash
python3 tools/evergrove_art_pipeline.py
```

Rights: AI-generated imagery; confirm the account's image-generation terms and record the decision before store submission. Not a substitute for an art-rights review.

# Store listing translation brief

Repo: `/Volumes/SANDISK-Data/game-foundry/.claude/worktrees/store-catalog`. No git commands.
Only create `store/metadata/<slug>/i18n/<lang>.json` for the languages you are given.

Sources: every `store/metadata/<slug>/i18n/en.json` (33 games) with keys `subtitle`,
`keywords`, `short_description`, `description`.

Write one Python script per language that holds your translations in a dict and writes
the files with `json.dump(..., ensure_ascii=False, indent=1)`, so escaping is correct:

```json
{"lang": "<key>", "status": "machine translation — native review pending",
 "subtitle": "…", "keywords": "…", "short_description": "…", "description": "…"}
```

Rules
- Natural, fluent store copy for adult puzzle players; calm tone; same meaning. No added
  claims (no health/brain-training claims, no "best", "#1", no prices).
- Keep the game's product name (e.g. "Kakuro Compass") in English Latin script. Generic
  puzzle names (Kakuro, Nonogram, Hitori, Shikaku, Takuzu…) may use the usual local term.
- Limits: `subtitle` ≤ 30 characters; `short_description` ≤ 80 characters;
  `description` ≤ 4000 characters; `keywords` comma-separated, no spaces after commas,
  ≤ 100 UTF-8 BYTES (non-Latin scripts use 2–3 bytes per character: use fewer terms),
  and no words from the game name.
- Keep the description's paragraph breaks (`\n`).
- Word games (vowel-garden, word-ladder-lane, word-search-shores, word-square-studio):
  the puzzles are in English — say "English word puzzles" naturally.

Finish by running `python3 tools/store_catalog/check_i18n.py <your langs>` and fix until it
prints `0 problems`. Report only the checker's final line.

**Do not spawn sub-agents or use the Agent/Task tool. Do all the work yourself.**

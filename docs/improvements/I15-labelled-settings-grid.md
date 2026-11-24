# I15 — Menu settings become labelled tiles

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

**Player problem (I12 finding 6).** The menu's settings were five unlabeled icons in a tight row. The settings page is exactly where a 45+ player should not have to guess.

**Change.** The settings panel in the menu sheet is now a grid of labeled tiles, two to a row (language + text size, board size + gentle motion, sound). Each tile shows its icon, its live state (dots for text and board size, tone for on/off) and its localized name underneath. The play screen stays icon-first; words live in the menu.

**Evidence.** b21 QA tour (`I13-I15-title-memory-menu.png`, right): "Larger board 1×", "Gentle motion" (on, forest tone) and "Gentle chime: OFF".

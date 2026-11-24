# I06 — Gentle motion setting turns play-triggered motion off

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

**Player problem.** Iterations before I01 added motion triggered by play: pipes swing, tiles bloom, a butterfly flies and the page glides. Some players, including many with vestibular sensitivity (more common with age), need a way to turn such motion off. The project's own EG-114 rule says no decorative motion may ship without that control.

**Change.** A **Gentle motion** setting (Codex butterfly glyph: `ui-motion-on` / `ui-motion-off`, from `art/generated/icons-ui-4.png`) is now the fourth icon in the menu's settings row. It is on by default and saved in PlayerPrefs (`evergrove:gentle-motion:v1`, kept apart from the save file). Off means pipes snap instead of swinging (`PipeGraphic.Animate`), no bloom, no butterfly, and an instant jump to the choices after the solve hold. A new localized key, `gentle_motion`, was drafted for the 29 catalogs by a Haiku sub-agent (de "Sanfte Animation", ja "やさしいアニメーション", vi "Chuyển động nhẹ nhàng"); catalogs now hold 106 keys.

**Standard.** WCAG 2.2 SC 2.3.3, "Animation from Interactions": motion triggered by interaction can be disabled.

**Evidence.** C# tests pass (39,825 assertions). The verifier reports 106 keys across all 30 catalogs. The b18 tour shows the butterfly toggle on (forest tone) in the menu (`I06-gentle-motion.png`).

**Open question.** Default on vs. off. Unity 6.3 exposes no OS reduce-motion flag, so the OS preference is not yet followed.

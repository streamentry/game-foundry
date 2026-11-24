# I66 — Choice cards go picture-first and get a spoken caption

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

**Player problem.** The three choice cards were the most text-heavy buttons left on the play path: a name plus a consequence sentence, in three narrow columns. And a real accessibility bug: the cards had no `Caption` child, so screen readers announced the internal object name ("Choice ShadeTree") instead of the choice.

**Change.**
- **Picture first.** Each card shows its painting, the keeper's portrait and the short name ("Plant a shade tree"). The consequence sentence is no longer printed; it is said on the memory screen that follows (I50), where it is earned.
- **Spoken caption.** Each card has a hidden `Caption` with "name. consequence", so VoiceOver/TalkBack read the whole choice.
- Cards are shorter (min 150 pt at 100% text). At larger text they still stack as wide cards (I46).

**Evidence.** b43 QA tour (19 screens) and live captures (`I66-picture-first-choices.png`): hub, stream 1, the picture-first choice row, and stream 2 with painted plaques.

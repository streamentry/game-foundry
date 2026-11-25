# I126 — The 40+ playtest kit now tests the stone path, the serif and sideways play

Date: 2026-11-25. Branch: feat/mosselora-painted-ui (PR #76).

## Why

Every bet made in I112–I125 is labelled "age fit not validated". The M3 playtest kit is how players aged 40+ would settle them, but its painted-build addendum stopped at I69. A session run with the current kit would never test the stone path, the book serif or sideways play.

## Change (docs only)

`docs/playtest/m3-session-kit.md` §7 now covers I01–I125 and adds three bets, each with an observable signal and a predeclared failure threshold:

| # | Bet | Fails if |
|---|---|---|
| P10 | The painted stone path shows progress (I120, I123, I124) | 3+ of 10 can't say which stones are done, or which one is theirs |
| P11 | The story reads comfortably in the book serif (I117, I125) | 3+ of 10 struggle to read story lines that they read easily as labels |
| P12 | Sideways play is wanted and works (I114–I116, I122) | 2+ of those who rotate get lost; the rotation rate is recorded either way |

The reversal rule names a fallback for each, so a failed bet reverts to a known option instead of an improvised redesign: P10 makes the spoken "Stream 3 of 12" visible or returns to the dots; P11 sets story lines back in the rounded sans; P12 locks to portrait, or keeps landscape if it is rarely used but harmless.

## Verification

- Docs only; no code changed. Recording thresholds before any session follows AGENTS.md's "predeclare, then test" discipline.

## Not done

- No participant has been recruited and no session has run. Per the recruitment kit, M3 also requires an M2 device build that has been smoke-tested on a real phone first.

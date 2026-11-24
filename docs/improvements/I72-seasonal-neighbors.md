# I72 — Neighbors speak of their landmark as the seasons turn

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

**Player problem (daily appeal 5/10).** Each neighbor had only two lines: before and after you built their landmark. Over twelve streams the village felt static. A truthful reason to return is that **the people notice the year turning** with you.

**Change.** In autumn (streams 7–9) and winter (10–12), with seasons arriving by progress as in I35, each neighbor speaks of the landmark they keep:
- Mira: "Your tree is turning gold. It will lose its leaves and come back greener." / "Snow rests on the branches of your tree. It is sleeping, not gone."
- Orin: "Leaves drift under the crossing now. It still holds, plank by plank." / "I keep the crossing clear of snow. People still walk it every day."
- Fen: "Your wildflowers are going to seed. The bank will be fuller next spring." / "The flower beds sleep under the snow. The seeds are safe in the earth."

The rules are truthful: a seasonal line appears **only once that neighbor's landmark exists in the save**, otherwise they still ask for it. A finished garden returns to the summer lines for the festival. No clock is read.

**Implementation.** `LocalizedVillage.Speak` (engine-free Core) plus six new keys, now 114 per catalog. A Haiku sub-agent drafted the 29 translations (for example de "Dein Baum wird golden…", ja "花壇は雪の下で眠っています…"); they need native review, and English remains the only shipping language.

**Evidence.** C# tests pass at 40,500 assertions, including new `SeasonalNeighborTests`: no seasonal line without the landmark, stream 7 is autumn, winter lines appear, and a finished garden goes back to summer. The verifier reports 114 keys × 30.

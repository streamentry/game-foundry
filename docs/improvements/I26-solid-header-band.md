# I26 — A solid header band keeps scrolled content clear of the chrome

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

**Player problem (I25 finding 1).** When the page scrolled, the floating menu and memory buttons sat over the board and its tool buttons; the faded band behind them was too weak. That looks broken.

**Change.** The chrome's band is now a **solid parchment strip as tall as the chrome (74 pt), plus a 30 pt fade** below it. A CanvasGroup on the band follows the scroll offset: it is invisible at the top of the page and fully present after 24 pt of scroll. Scrolled content now passes cleanly under it.

**Evidence.** b26 QA tour, 17 screens (`I26-I28-band-coach.png`). Stream 2 and the scrapbook are scrolled, and the board, tools and cards sit below the band, nothing under the buttons.

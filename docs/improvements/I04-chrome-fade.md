# I04 — Wordmark clears the chrome; content fades under it on scroll

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

**Player problem.** After I03 the floating menu button overlapped the "M" of the wordmark. When the page scrolled, text such as the stream pill slid under the buttons and collided with them. That looks like a web page with sticky buttons, not a game.

**Change.**
- On puzzle screens the wordmark is 46 pt tall (58 pt elsewhere), so it sits cleanly between the menu and book buttons.
- A paper-coloured fade band (a procedural vertical gradient) sits behind the chrome. Its opacity follows the page's scroll offset: it is invisible at the top and fully present after 36 pt of scroll, so content melts away under the buttons.

**Evidence.** b17 iOS Simulator QA tour, 17 screens (`I04-chrome-fade.png`). On stream 1 the wordmark clears the menu button. On a scrolled stream 2 the pill fades under the chrome. With the menu open, the sheet covers the chrome.

**Not verified.** Very small phones (iPhone SE class) and landscape tablets.

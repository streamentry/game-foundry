# I194 — Settings are round tokens; no wordmark over your garden

Date: 2026-11-26. Branch: feat/mosselora-world-first.

## Player (b220)

- The settings were five wooden tiles with sentences on them: "Language: English", "Text size 100%", "Larger board 1×", "Gentle motion", "Gentle chime". It looks like a form on a website.
- The memory card still opens with the big Mosselora wordmark and "A living garden that remembers." on a cream plate floating over my garden. The rest of the game no longer does this.

## PM decision

- **Settings are round tokens in two centred rows**: globe, Aa (with three dots for 100/125/150%), magnifier (two dots for the board size), butterfly (motion) and speaker (chime). Each token's tone shows on/off, and its full name is still spoken and shown on press-and-hold. (I143's "settings are where words belong" is retired at the founder's direction: pictures first, words on request.)
- **No wordmark or tagline over the garden on the memory card**; the map gets that room and frames the new thing a little higher.
- Research and dev rows below the settings appear only in development builds (unchanged; checked by the release-candidate source guard).

## Dev

`EvergroveApp.SettingsRow`: rows of `SettingToken` (76 pt) round `IconButton`s, unstretched and centred (spacing 22); `TextSizeButton` and `BoardSizeButton` keep their level dots inside the round token; the chime's short-name trick is no longer needed. `PageGardenMap` no longer calls `HeroTitle`; the hero map focuses at 0.55. `DevQaTour`: new shot `17d-settings.png`.

## Result (b221 → b222; portrait, landscape and 150% tours, 26 each, all complete)

`I194-settings-tokens.jpg`: settings at 100% and 150% (Aa lit, three dots), and sideways. `I194-b221.jpg`: the first layout (three rows) and the memory card without the wordmark at 100% and 150%. Project verify passes.

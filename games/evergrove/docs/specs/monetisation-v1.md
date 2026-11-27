# Monetisation policy v1 (Mosselora, internal codename Evergrove)

**Status:** policy draft for founder review. It sets no price, configures no store product and adds no code. Mosselora is free today. This file answers checklist item 77: *no currency for sale that buys power; if ever monetised, only a one-time unlock or cosmetic season packs.* Related: [economy-v1](economy-v1.md), [vision §7](../../vision.md#7-a-business-model-that-a-century-allows), [GTM spec](07-VALIDATION-AND-GTM.md).

## 1. Status today

Checked in this worktree on 2026-11-27.

| Area | What exists | Source |
| --- | --- | --- |
| Price | Free. Apple $0.00 saved for 175 territories; Google Play free. Owner confirmed free pricing for all titles on 2026-11-23. | [en-US.json](../../../../store/metadata/evergrove/en-US.json) (`release_decisions.price`); [store workflow](../../../../docs/store-publishing-workflow.md) |
| In-app purchases | None. No StoreKit, Google Play Billing or Unity Purchasing code, and no purchasing package in the manifest. | [manifest.json](../../Packages/manifest.json); grep of `Assets/Scripts`, `Packages`, `ProjectSettings` |
| Ads | None. No ad SDK in the manifest or code. | same |
| Third-party analytics | None in the default build. `LocalPlaytest.cs` compiles only under `UNITY_EDITOR`, `DEVELOPMENT_BUILD` or `EVERGROVE_RESEARCH_TOOLS`, collects no identifiers and is never persisted. | [LocalPlaytest.cs](../../Assets/Scripts/Core/LocalPlaytest.cs) |
| Unity analytics flag (unresolved) | `ProjectSettings.asset` has `submitAnalytics: 1`, while `UnityConnectSettings.asset` has `UnityAnalyticsSettings: m_Enabled: 0`. I did not establish what the first setting sends. | [ProjectSettings.asset](../../ProjectSettings/ProjectSettings.asset); [UnityConnectSettings.asset](../../ProjectSettings/UnityConnectSettings.asset) |
| Accounts | None in the offline build. The Cloudflare reference client is disabled and not attached by the default bootstrap. The backend is a bearer-capability prototype, "not a production identity system". | [WorldClient.cs](../../Assets/Scripts/Unity/WorldClient.cs); [backend README](../../backend/README.md) |
| Network permission | The Android build keeps `android.permission.INTERNET`. No controlled network capture exists. | [en-US.json](../../../../store/metadata/evergrove/en-US.json) (`privacy_and_data_safety`) |
| Hints, undo, restart | Free. The listing says "Optional hints, undo and restart". No paywall in code. | [en-US.json](../../../../store/metadata/evergrove/en-US.json) (`apple.description`) |
| Currency | Seeds only, earned in play. Nothing can be bought with money. | [economy-v1](economy-v1.md) |

The listing's "Play offline, without an account" matches the code today. Section 6 covers the family-circle plan, which would change that.

## 2. Policy

**The rule.** Mosselora may be monetised only by one of these two routes, and only after the founder records a decision (section 6):

- **(a) One-time unlock.** One purchase, restorable through the store, for content that is not yet shipped. The 12 chapters in the store copy stay free (see tension T2).
- **(b) Cosmetic season packs.** Looks only (section 3).

**Never, in any build:**

| Never | Reason |
| --- | --- |
| Currency for sale (seeds, fruit, jam, or any balance) | Buying a balance is buying progress. It would also make "giving is cheaper than buying" meaningless. |
| Plants, nursery rows or cuttings for sale | Plants cost seeds; selling them is selling progress. |
| Energy, lives, timers or waits to skip | The game has no clock gates ([product spec](01-PRODUCT-AND-EXPERIENCE.md): "never gate content by wall-clock time"). A paid skip would sell relief from pressure we chose to add. |
| Paying past the daily replay cap | Paid replays beyond the cap are pay-to-progress and defeat [economy-v1](economy-v1.md). |
| Loot boxes or random paid rewards | Paid randomness is a gamble. Adults 40+ and family play both need it absent. |
| Ads of any kind | Ad SDKs add third-party data flows. They also conflict with child-directed rules. |
| Subscriptions that gate core play | Solo offline play must stay complete. A subscription would make the core game rent. |
| Pay-to-progress, paid hints or paid narrative outcomes | Players who pay should not get a better garden. Hints stay free ([vision §7](../../vision.md#7-a-business-model-that-a-century-allows)). |
| Countdowns, "limited time" offers or streak saves | Pressure works against gentle play. Absence is never punished ([vision §1a](../../vision.md#1a-family-play-connection-not-only-entertainment), rule 5). |
| Purchases by children without a parent gate | Children 7+ are co-players ([AGENTS.md](../../../../AGENTS.md)). Section 4 sets the gate. |
| Selling player data | Vision §7 rejects it. The family circle (section 6) would hold data, so the ban matters more. |

## 3. Fit with the economy

- **No paid input exists.** Seeds come from first solves, capped replays and parcels. Sinks are the nursery, keepsakes and parcels. No code writes money into the ledger, and no code calls a store ([economy-v1](economy-v1.md), "Monetisation stance").
- **A cosmetic pack may contain:** colour palettes, ornament sets, village artwork variants, journal stamps. All are looks only.
- **A cosmetic pack may not contain:** seeds, fruit, jam, plants, faster growth, extra replays, a larger cap (`MaxSeeds`, `MaxGifts`, larder limits), a seasonal yield that only buyers receive, or any change to a choice's outcome (shade trees, footbridge or wildflowers).
- **Every puzzle stays reachable free.** A pack cannot change puzzle order, difficulty, hint stages or the number of chapters.
- **Forged parcels (economy-v1 finding 1)** are a free-economy integrity problem, not a monetisation one, because nothing is paid yet. Fix it before any family transfer ships. Do not let a paid item depend on the parcel code.

## 4. Children and stores (owner decisions; none done)

Nothing in this section has been started or verified. Each item is for the account owner to decide. Before any paid item ships in a build designed for children:

1. **Decide the audience first.** The store record says "18 and over" for Google Play ([en-US.json](../../../../store/metadata/evergrove/en-US.json), `google_play.target_audience`). The same file also records a Play submission as "13 and older". Reconcile these. Apple's Kids Category is unselected.
2. **Apple Kids rules, if the build is designed for children.** The owner must check the current guideline text. As I understand it, third-party ads and analytics are not allowed in Kids apps, and purchases and external links need a parental gate. This spec does not restate the rules.
3. **Google Play Families policy, if the build is designed for children.** The owner must check the current policy text, which covers ads, purchases and data for child-directed apps.
4. **Parental gate** before any purchase, purchase link, account sign-up or invite. A child cannot complete a purchase. The adult sees the purchase history.
5. **No child data in purchase records.** An unlock belongs to the store account, not to a child profile. COPPA applies to child data under 13 in the US ([vision §1a](../../vision.md#1a-family-play-connection-not-only-entertainment)).
6. **Restore flow built and tested** on both platforms before any one-time unlock ([GTM spec](07-VALIDATION-AND-GTM.md): "may need its own secure restore flow").
7. **No purchase automation.** Pricing, product setup and any purchase are done by the owner, not by an agent ([AGENTS.md](../../../../AGENTS.md): "Never automate spending").
8. **New SDKs need an audit** (version, licence, privacy, platform support) before they are added. A Unity purchasing package would be a new dependency.

## 5. Review against vision §7

Test from §7: *Would we be comfortable if a player's grandchild read how we made money?* Each row below is a statement from [vision §7](../../vision.md#7-a-business-model-that-a-century-allows).

| §7 statement | Agrees? | How, or the tension |
| --- | --- | --- |
| "Pricing is set to free in App Store Connect" | Agrees | Stays free until decision D1. |
| "Free demo, then a one-time full unlock" | **Tension T2** | The store copy and the code give all 12 chapters free. A demo split would change what the listing promises. v1 keeps shipped chapters free and allows a one-time unlock only for unshipped content. Founder decision D2. |
| "Substantial optional expansion packs only after content demand" | **Tension T3** | "Every puzzle stays reachable free" leaves no room for a paid puzzle pack. v1 does not allow one. Founder decision D3. |
| "Core puzzles, journal and saves stay offline" | Agrees for solo play; **tension T4** with the family circle | The planned family circle ([vision §1a](../../vision.md#1a-family-play-connection-not-only-entertainment)) would hold garden moves and choices on a server. Solo play must stay complete and offline. |
| "A store purchase needs a secure way to restore it" | Agrees; not built | Restore is a precondition for any unlock (section 4, item 6). |
| Rejected: forced ads | Agrees | Banned. |
| Rejected: paid hints | Agrees | Hints stay free. |
| Rejected: lives | Agrees | None in code. |
| Rejected: subscriptions | Agrees | Banned. |
| Rejected: streak penalties | Agrees | No streaks; no guilt ([vision §1a](../../vision.md#1a-family-play-connection-not-only-entertainment), rule 5). |
| Rejected: paid narrative outcomes | Agrees | Choices are free. |
| Rejected: seasonal exclusives | **Tension T1** with item 77's "season packs" | A season pack is allowed only if nothing in it is exclusive to buyers or to one season in a way that removes free play. Founder decision D4. |
| Rejected: loot boxes | Agrees | Banned. |
| Rejected: real-money economies | Agrees | No currency for sale. |
| Rejected: selling player data | Agrees | No ads and no analytics SDK. The family circle raises the stake, so it is in the never list. |

**Overall:** the bans in §7 are all kept. The positive model in §7 (free demo plus one-time unlock, then expansions) is only partly compatible with item 77 and with "every puzzle stays reachable free". I have not resolved these tensions. They are listed as decisions below.

## 6. Open decisions for the founder

Each decision is open. None is made by this file.

1. **D1. Monetise at all, and when?** The GTM spec asks for price-intent evidence before a price is chosen ([07](07-VALIDATION-AND-GTM.md), "price intent"). Options: stay free until evidence exists, or test a price. Current evidence on willingness to pay: **unknown**.
2. **D2. Demo split or all-free base (T2).** Should any of the 12 shipped chapters be paid? The listing says free today. If an unlock exists, it covers unshipped content only.
3. **D3. Puzzle expansions (T3).** May a future pack of new puzzles be a one-time paid unlock? This conflicts with "every puzzle stays reachable free" as written. Price points: **unknown, not set**.
4. **D4. Season packs (T1).** Are season cosmetics time-limited? A time limit conflicts with the "limited time" ban. Can a pack be bought after its season ends?
5. **D5. Audience (section 4, item 1).** Is the build designed for children aged 7 and up? Reconcile the "18 and over" and "13 and older" records in [en-US.json](../../../../store/metadata/evergrove/en-US.json).
6. **D6. Unity analytics flag.** Confirm what `submitAnalytics: 1` does, and whether it must be off, before any privacy or Data safety answer is given.
7. **D7. Family circle and paid items (T4).** If a family circle with accounts ships, must any paid item stay usable without an Evergrove account? This spec assumes yes.
8. **D8. Price points.** Unknown. No price, revenue or market figure in this repository supports a number.

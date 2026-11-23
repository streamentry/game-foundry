# iOS and Android game publishing workflow

**Status checked:** 2026-11-23  
**Current batch:** Good Eye, Kakuro Compass, Odd Bloom, One Path, Quiet Camp, Quiet Shot, Shikaku Garden, and Takuzu Lab. The user requested automatic release after store approval for eligible titles.

This workflow takes each game through a separate, evidence-based store release. That authorization does not waive Apple or Google review, device testing, privacy disclosures, signing, or the account owner's legal declarations.

Mosselora is a separate Unity 6.3 candidate, not one of the eight Godot titles below. `Evergrove` remains its internal project codename, source path and package namespace. Its platform state is tracked in [`store/release-status.csv`](../store/release-status.csv), English metadata in [`store/metadata/evergrove/en-US.json`](../store/metadata/evergrove/en-US.json), and the sequential M1–M6 gates in [`games/evergrove/docs/specs/09-EXECUTION-CONTROL.md`](../games/evergrove/docs/specs/09-EXECUTION-CONTROL.md).

## Current portfolio state

The eight requested projects have Web Playtest presets. The production source still has no iOS or Android export presets, stable app identifiers, or signed release artifacts. The signed-in App Store Connect and Play Console app lists were rechecked on 2026-11-23; neither console returned an exact-title result for any of the eight. No record was created.

Hosted CI checks for prototype PRs #15–#19 passed on 2026-11-22; the status of upstream PR #20 was not checked here. Local Godot imports and existing game tests passed for all eight requested titles. Local research validation and all 50 Python unit tests passed. These checks do not prove native signed builds, physical-device behavior, store declarations, or release readiness.

| Game | Current role | Store readiness |
| --- | --- | --- |
| Good Eye | visual-mechanic research prototype | Not ready; native export, physical-device QA and audience validation are missing |
| Kakuro Compass | Kakuro proof of play | Not ready; native export, physical-device QA and audience validation are missing |
| Odd Bloom | visual playtest prototype | Not ready; native export, physical-device QA and audience validation are missing |
| One Path | graph-puzzle research prototype | Not ready; native export, physical-device QA and audience validation are missing |
| Quiet Camp | Tents & Trees proof of gameplay | Not ready; native export, physical-device QA and audience validation are missing |
| Quiet Shot | spatial-prediction prototype | Not ready; native export, physical-device QA and audience validation are missing |
| Shikaku Garden | Shikaku playtest prototype | Not ready; native export, physical-device QA and audience validation are missing |
| Takuzu Lab | gameplay engineering reference | Not a production selection; the README explicitly says not to launch it as a catalogue of near-identical apps |

Do not call a web artifact an iOS/Android build. Do not call a build upload, review approval, or configured release setting a published app. Record each store state separately.

## 1. Freeze one release candidate at a time

For each game, record the source commit, game version, app name, intended regions, monetization, and release owner. Review the game's design and playtest evidence, deterministic rules, save/resume, first-session instructions, accessibility, rights, privacy and the direct competitor set. Test on representative physical iPhone and Android devices, including pause/resume, offline behavior, small screens and text scaling. Record any audience test with age bands and denominators; do not infer adult-45+ demand from general search volume.

The eight games are distinct prototypes, not eight approved products. Check Apple's App Review Guideline 4.3 and make sure every submitted app has a meaningfully distinct experience and original presentation. A mass submission is not a substitute for user or market evidence.

## 2. Build truthful English master listings

Keep one versioned source per game, for example `store/metadata/<game-slug>/en-US.json`, with:

- App name, subtitle (Apple), short description (Google Play), full description, keywords and support URL.
- Feature claims tied to a current build. No health, memory or cognition claims without suitable evidence.
- The actual offline, account, ads, purchases, analytics and data behavior of the submitted binary.
- Versioned privacy-policy and support URLs that work in a public browser.

Check the live platform limits before each submission. Current official limits include a 30-character app title, 30-character Apple subtitle and 100-character Apple keyword field; Google Play uses a 30-character title, 80-character short description and 4,000-character full description. Apple descriptions are limited to 4,000 characters.

## 3. Localize the listing into the top-30 language queue

Use the 30-row roster in [`gkp-top-30-language-seeds-2026-11-21.csv`](research/gkp-top-30-language-seeds-2026-11-21.csv) as the requested starting queue. It ranks language speakers, not paying players, game demand, storefront reach, or adults aged 45+. Its country entries are comparison anchors; it is not proof of market size.

For each game:

1. Map each language row to a locale the current Apple and Google consoles support. Do not invent a locale for a language the store does not offer. Preserve distinctions such as Brazilian vs. European Portuguese and Simplified vs. Traditional Chinese where the stores support them.
2. Draft the complete store fields from the approved English master. Preserve rules, game names, difficulty, offline behavior and feature limits; do not add promises to improve discovery.
3. Mark every machine translation `draft — native review pending`. Have a native reviewer check meaning, spelling, cultural fit, game terminology and store character limits. Back-translate or run a second-language QA pass for errors; that is not a replacement for native review.
4. Do not submit unreviewed translations as if they were reviewed. Use the English listing until a locale is approved. Keep a per-field review status and reviewer/date.
5. Localize text embedded in screenshots or videos. Do not reuse English text in a localized screenshot without checking it at phone size.

Apple lets teams add metadata localizations in App Store Connect; the localized screenshot and description behavior is documented in its localization guide. Google Play allows custom store-listing translations and graphics; machine translation alone does not establish quality. Localizing store text does not localize the game UI. Track those as separate work.

## 4. Make store art from the real game

Use the `imagegen-frontend-mobile` skill for original app-icon concepts and supplementary promotional artwork. Generate a separate, title-specific direction for each game; use the actual mechanic, palette and current product name. Keep generated artwork free of invented UI, fake testimonials, ratings, awards, competitors' marks, unsupported claims and copied trade dress.

Initial image-generation art concepts for all eight projects are saved under `store/assets/<game-slug>/concepts/`. They are not final store graphics. The output has not been resized or checked against current store upload specifications.

**Store screenshots must be captured from the running build.** Do not use ImageGen to fabricate gameplay screenshots or an app preview. Capture actual screens on the target device/simulator, then add only truthful, legible captions and decorative framing around the untouched gameplay. Record the device model, OS, app version, locale and capture date. An app-preview video must also show real in-app footage. Verify each asset against the current store size and format specifications before upload.

Keep editable source art and approved exports under `store/assets/<game-slug>/<version>/`; retain the source screenshot separately from any captioned marketing composition so the actual UI can be audited.

## 5. Create or verify each store app record

Use the user's requested **built-in Browser** for signed-in store-console work. Do not ask for passwords, 2FA codes or private keys in chat. Confirm the account, developer role, existing app records, public title, age rating, countries, price and account owner before creating a new record. Choose stable, unique iOS bundle and Android package identifiers; never change them after first distribution. Store identifiers and signing material outside public Git.

Creating a record is separate from creating a binary. Upload the iOS build with Xcode or Transporter (the browser console does not replace binary signing/upload). Upload a signed Android App Bundle through Play Console. Use an internal TestFlight group and Google Play internal track first; install both builds on real devices and capture the install/build identifiers.

## 6. Complete platform declarations and submit

Prepare privacy, data-safety, encryption/export, content-rating, ads, age-group, copyright and contact details from the actual binary and SDK inventory. The account owner must personally confirm declarations that certify legal or business facts. No data collection in current prototype source is not proof that the final signed binaries or future SDKs collect none.

Before submitting, verify the final artifact checksum, app identifier, version/build, target SDK, signing identity, screenshots, localization state, policy declarations and review instructions. Submit one game at a time. Read back the exact App Store Connect and Play Console status and any review message before moving to the next game.

## 7. Set release after review approval

This batch requests automatic release after store approval:

- **Apple:** for each platform version, select **Automatically release this version** in App Store Connect's App Store Version Release section and save. Verify the selection on the version before submission. Do not choose a dated release unless the user supplies a date.
- **Google Play:** use standard publishing for the intended release and keep Managed publishing off. If Managed publishing is enabled, approved changes wait for a manual publish action. Read the selected setting back before submitting.

The option schedules release only after the platform accepts and approves the submission. It does not bypass review or guarantee the exact time the app appears in every storefront. Record `approved`, `pending publication`, and `available` as separate states; verify the live listing after release.

## 8. Per-game completion record

Track these fields in the release log before calling a game published:

| Gate | Evidence to record |
| --- | --- |
| Source | Commit SHA, release version, reproducible build instructions |
| Game quality | Rule tests, accessibility and physical-device results, player feedback and unresolved issues |
| Native artifacts | Signed iOS build and Android AAB; identifiers, checksums, SDK/OS targets |
| Store content | App record, reviewed locales, screenshots, preview video, privacy/support URLs |
| Declarations | Data/privacy, age/content rating, rights and export answers confirmed by the account owner |
| Test tracks | Build IDs, install outcomes, test devices and tester issues |
| Review | Submission ID, review status and messages |
| Release setting | Apple automatic-release selection and Google standard/managed-publishing setting |
| Publication | Public listing URLs and observed availability by storefront/date |

## Current execution blockers

- **Demand data:** Semrush keyword reports were unavailable because the account had no API units. Existing Google Keyword Planner figures are broad web-search bands and do not establish App Store volume or adult-45+ interest.
- **Eligibility:** none of the eight is ready for store review. All still lack measured adult-45+ usability, audience fit, and market/retention evidence. Kakuro remains a proof-of-play candidate, not a production approval; Good Eye's local fixes do not replace player tests. Takuzu Lab is explicitly not a production selection.
- **Store records:** the signed-in App Store Connect Apps page and Play Console were readable in the built-in Browser. Exact-title searches returned no record for any requested game. No records, listings, uploads, submissions, or release settings were changed. Play Console identifies the account as a personal account named Cloudjet Solutions PTE, with 24 apps; the dashboard shows 11 apps with policy tasks and one submission not yet sent for review. Account roles and the account owner's legal facts still need direct confirmation before any record or declaration.
- **Source checks:** all eight Godot 4.7.2 projects imported and passed their existing rules, persistence, and scene scripts. The research backlog validator passed. All 50 repository Python unit tests passed after rerunning the eight web-playtest tests with loopback permission.
- **Android debug proofs:** temporary debug APKs for all eight games were signed with Godot's debug key, installed, and launched on an Android 16/API 36 Pixel 7 AVD. All eight manifests report min SDK 24 and target SDK 36; all eight Gradle AABs built but are unsigned. None was uploaded. Eight real AVD screenshots and short screen recordings are under each game's native-proof/android-api36-pixel7-avd/ folder; see the [capture index](../store/assets/native-proof-index.md). These are proof captures, not store-ready assets.
- **iOS debug proofs:** temporary Xcode projects for all eight titles exported, and all eight iOS arm64 device targets compiled unsigned. Kakuro's Simulator build failed because the shared Godot 4.7.2 Simulator slice contains x86_64 objects only while this Apple Silicon target requires arm64. No physical iPhone is attached. Xcode has zero valid code-signing identities; eleven provisioning profiles exist, but none is verified for these unassigned bundle IDs. No IPA or TestFlight upload exists.
- **Android toolchain:** API 36, Build Tools 36.1.0, NDK 29.0.14206865, and current command-line tools are now installed. Godot used the installed JDK 21 for local proofs; JDK 17, the repository's pinned baseline, is absent. The APK export required ETC2/ASTC texture import, so that setting has now been added to the eight requested project.godot files and all eight re-imported successfully. The projects still lack production app icons.
- **Release artifacts:** all temporary Android package IDs, iOS bundle IDs, and test icons use example values in /private/tmp only. The production projects still have no export presets, stable store identifiers, production icons, or signed release artifacts. Per-platform upload/review/public states remain in store/release-status.csv.
- **Metadata and assets:** eight English listing masters and 200 description drafts across 25 unique mapped locales are staged under store/metadata/. No translation has native review; unsupported and proxy-only roster rows are marked. The eight existing generated feature-art concepts are 1774 × 887 concepts, not final store graphics. AVD screenshots and preview recordings exist for all eight but are not physical-device or app-store-ready captures.
- **Declarations:** no age rating, privacy/Data safety, export-compliance, rights, countries, or account-owner legal declaration was submitted. The user confirmed free pricing for all titles on 2026-11-23. Evergrove's Apple $0.00 price schedule is saved for 175 territories; Apple App Availability and all Google Play price settings remain unconfigured. Complete remaining declarations from final signed binaries and hand owner certifications to the account owner.

## Submission and publication rule

The release ledger distinguishes upload, submission, approval, automatic/standard release setting, and verified public availability. Every current row remains unuploaded, unsubmitted, unapproved, and unpublished. A green local test, emulator capture, or unsigned build is not a release.

## Mosselora candidate status (internal codename Evergrove) — 2026-11-23

- Local `HEAD` is `9182b126554a041e04858d28115982a744cffbe5`, matching fetched `origin/main`. Local edits to publishing docs, site config/README and Mosselora store metadata/release ledger remain in place. No changes were discarded.
- The pure C# suite passes **38,835 assertions** with .NET 8.0.428. The Python project verifier passes static manifest, rendering, consent and build-contract checks and finds 29 complete offline locale catalogs (81 keys each). A recorded Unity 6000.3.0f1 `EvergroveSceneSetup.VerifyProject` smoke passed editor compilation, 12 puzzles, 29 imported locales and snapshot schema 4. The macOS Development player has 29 QA screenshots: three standalone frames, 13 core-loop tour states, and 13 states at 150% text size in a 375x667 viewport. The added anchored-tile tap state verifies the explanation appears without a move or save. The tour invokes real UI buttons in an in-memory world without touching player saves. The captures show connected/dry and spring-wave cues, protected tiles, journal/ending screens, and larger text. They remain development-player desktop evidence, not phone touch/OS-text-size or 45+ review. The newer matched-mode and garden-transfer source changes also pass the pure C# suite. These are source/import/logic results only, not an Android/iOS export, device run, player study or store artifact.
- The only discoverable Unity Editor is `/Volumes/SSD/Unity/editors/6000.3.0f1/Unity.app`; it reports the pinned version, but macOS code-signature verification fails and the Unity CLI cannot register it. Only `MacStandaloneSupport` is installed. The recorded iOS build attempt reports zero entitlements; Android support is absent. No native build or device capture exists.
- `EvergroveBuild.Build()` remains a **Development Build** entrypoint: Android test APK with `buildAppBundle=false` and `com.streamentry.evergrove.playtest`, unsigned iOS Xcode output with the playtest identifier, or a local macOS QA player. The `EVERGROVE_IOS_SDK=simulator` option only chooses the iOS SDK; it does not make the export signed or production-ready. The iOS Bundle ID `com.streamentry.evergrove` is registered; the Play package name is available but not reserved.
- GitHub Actions history contains no run of `evergrove-unity-playtest.yml` and no preview artifact to reuse. The only available action is a manual development build requiring maintainer-owned Unity license secrets; it cannot produce a signed store release.
- Google Play requires new apps and updates submitted on or after 2026-08-31 to target Android 16/API 36 or higher; the final Evergrove binary's target must be verified from the actual signed AAB before upload. See [Google Play's current target API policy](https://support.google.com/googleplay/android-developer/answer/11926878?hl=en-GB_ALL).
- The current public working name is **Mosselora**; **Evergrove** remains the internal codename, source path, Bundle ID namespace, package namespace and SKU stem. App Store Connect's existing app record (Apple ID `6838015597`, Bundle ID `com.streamentry.evergrove`) was renamed to Mosselora and saved with subtitle `A Living Garden`, primary category **Games**, and subcategory **Puzzle**. Version 1.0 remains `Prepare for Submission`, with automatic release selected. The user-selected name is still pending formal trademark clearance. The App Store record has no build, screenshots, support/privacy URLs or review contact. Play Console's app-table search returned no exact Mosselora result. The Create App draft now contains Mosselora and the available package `com.streamentry.evergrove`; no record was created because the account-owner policy and U.S. export-law attestations remain unchecked.
- The Play developer account is marked **Personal**; its account creation date has not been verified. Google requires personal accounts created after 2023-11-13 to run a closed test with at least 12 opted-in testers continuously for 14 days before production access; confirm whether that account gate applies to each new title before planning public release. See [Google Play's official testing requirement](https://support.google.com/googleplay/android-developer/answer/14151465?hl=en-419).
- No store submission is eligible: M1 is **partial** and M2–M6 remain open; no 45+ playtest or matched-value sessions establish the distinct experience needed to address Apple's [Guideline 4.3](https://developer.apple.com/app-store/review/guidelines/), and trademark clearance for Mosselora, Android app record/package, store URLs, owner declarations, rating, territories, signed builds and physical-device QA are unresolved. A macOS Development-player privacy inspection found no third-party SDK assembly and no outbound TCP during a short run, but observed a development Player Connection listener; this is not mobile release-binary or network-capture evidence. The computed contrast audit initially flagged 21 pairs. Some fixes are applied and visible in desktop screenshots; the audit has not been rerun against the final UI, and other contrast findings plus phone/45+ validation remain open. Apple free pricing is configured; App Availability remains unset, and Google Play pricing is not configured.
- A source-based English listing draft, 25 top-30-roster locale description drafts (all native review pending), and one ImageGen key-art concept are staged under `store/`. The source-gated consumer candidate is English-only; English is not M5-certified. The concept is not a screenshot. Store-ready mobile screenshots and preview footage still require a running, release-like mobile build.
## Primary references

- [Apple: release options after App Review](https://developer.apple.com/help/app-store-connect/manage-your-apps-availability/select-an-app-store-version-release-option/)
- [Apple: localize app information](https://developer.apple.com/help/app-store-connect/manage-app-information/localize-app-information/)
- [Apple: product-page fields and limits](https://developer.apple.com/app-store/product-page/)
- [Apple: screenshot and app-preview uploads](https://developer.apple.com/help/app-store-connect/manage-app-information/upload-app-previews-and-screenshots/)
- [Apple: upload builds](https://developer.apple.com/help/app-store-connect/manage-builds/upload-builds/)
- [Apple: App Review Guidelines](https://developer.apple.com/app-store/review/guidelines/)
- [Google Play: create and set up an app](https://support.google.com/googleplay/android-developer/answer/9859152)
- [Google Play: translate and localize an app](https://support.google.com/googleplay/android-developer/answer/9844778)
- [Google Play: publish an app](https://support.google.com/googleplay/android-developer/answer/9859751)
- [Android: target API requirements](https://developer.android.com/google/play/requirements/target-sdk)
- [Apple: current App Store metadata locales](https://developer.apple.com/help/app-store-connect/reference/app-information/app-store-localizations/)
- [Google Play: current store-listing languages](https://support.google.com/googleplay/android-developer/answer/9844778)
- [Godot 4.7: iOS export guide](https://docs.godotengine.org/en/4.7/tutorials/export/exporting_for_ios.html)
- [Godot issue 122379: iOS Simulator template architecture defect](https://github.com/godotengine/godot/issues/122379) (the local 4.7.2 template slice was checked directly)

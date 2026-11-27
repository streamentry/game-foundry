# Mosselora store-owner handoff — 2026-11-25

## Confirm before submitting the mobile release

The [Mosselora privacy policy](https://mosselora.com/privacy/) now covers iOS and Android. The game stores progress and settings on the device. Copying a garden or sharing a postcard uses the device clipboard or share sheet. There are no player accounts, ads, or production cloud sync in this release. The support inbox and website are covered separately.

| Where | Prepared answer or action | Owner confirmation |
| --- | --- | --- |
| Both stores | Proposed Apple App Privacy answer: no data collected and no tracking. Proposed Google Data safety answer: no user data collected or shared. The shipped settings disable Unity Analytics, Ads and Cloud Diagnostics. Build 3’s Android manifest includes Internet permission. That does not establish a transfer, and physical device network verification is still open. | Confirm that the company and any SDK, reporting, or backend service collect no player data or diagnostics from the submitted build, and that the published policy accurately describes the support inbox. |
| Apple Content Rights and copyright | Copyright line in the form `2026 <legal rights holder>`. | Provide the exact legal copyright holder and confirm rights to distribute all shipped artwork, fonts, audio, code and other content. Identify any third-party content Apple’s question covers and confirm its distribution rights. |
| Apple export compliance | The release contains Unity’s runtime and networking capability. Static review does not establish the correct encryption classification. | On the build’s App Encryption Documentation form, confirm which listed encryption category applies and supply supporting documents if Apple requires them. |
| Apple age rating and Google IARC | Puzzle game. Draft answers: no violence, sexual content, profanity, gambling, purchases, chat, or public user-generated content. Garden and postcard sharing is initiated by the player through the operating system. | Verify those answers against the shipped content and personally accept the IARC terms in Play Console. |
| Google target audience and Apple Kids Category | Play currently saves 18+. The product direction names children ages 7+ as co-players, while the release is a solo offline game. Apple’s content rating and Kids Category are separate choices. | Confirm whether this first release is marketed to adults only or also targets children ages 7+. If it targets children, select the applicable Play age bands, confirm Families compliance, and confirm whether to opt in to Apple’s Kids Category. |
| Google Trader status and Apple China Mainland | Play Trader status is pending. Apple China Mainland remains included and shows missing filings. | Confirm Play trader or non-trader status and verify any required public details in the console. For Apple, provide the China Mainland filings if retaining that territory, or explicitly authorize excluding it until the filings are ready. |

Build 4 includes the menu link both stores require. Apple build 4 is VALID and attached to version 1.0. Play code 4 is the only bundle in the internal draft. The published policy URL is saved in both stores. Verify the link on physical devices before submission. [Apple App Review Guidelines](https://developer.apple.com/app-store/review/guidelines/) · [Google Play review preparation](https://support.google.com/googleplay/android-developer/answer/9859455)

**Update, 2026-11-26.** Both current-source builds succeeded and were signed. Apple build 3 (1.0.0) is `VALID` after the API upload. Google Play accepted version code 3 (1.0.0) and saved it to the internal testing draft. The remaining checklist covers preparation for review and rollout. The earlier build and signing blockers below are historical. See the [release status](../../../../store/release-status.csv) for the current state.

This is a handoff of account-owner actions, not a completed legal review. No declaration in this checklist has been submitted. Current worktree HEAD is `5bfe306d3bca4fa76a1ff261c74316fc5e7ea555`, a docs-only branch over `main` `609881069aea4fc4af5b7b82709a8d3be46c5b23`. It adds ages 7+ as family co-players and a future data-collecting remote circle; neither feature is implemented or authorized, and no verified signed build exists.

## Apple App Store Connect

The first resumed Browser check showed an App Store Connect login URL with `authResult=FAILED`; `requestUserTakeover()` was invoked and no account response was entered. A later authenticated Browser session read the current version and availability state below. Build 2 predates current source and remains unsubmitted.

Latest Browser readback confirms the Apple record is still `Prepare for Submission`, build 2 remains attached, automatic release is selected, and App Privacy / Privacy Policy URL remain unset. App Availability is all 175 countries/regions, each set to Available on App Release; the price matrix shows $0.00 across all 175 (read back 2026-11-26). China Mainland is included, and App Compliance still shows its Security Assessment and AI-generated/Synthetic Content items as Missing Info. Apple also identifies the developer as a trader in its DSA section. No compliance declaration was entered.

The account owner/Admin must complete these items against the eventual release candidate:

- [ ] **Content rights:** review the shipped asset and dependency inventory, including generated art, fonts, audio, third-party text/assets, and licenses. Submit only the truthful rights answer.
- [ ] **China Mainland distribution:** provide the required Security Assessment proof/report and AI-generated or synthetic content services declaration/filing before distributing in China Mainland. App Store Connect currently marks both rows Missing Info.
- [ ] **Age rating:** answer the questionnaire from the finished game content and interactions.
- [ ] **App Privacy:** confirm collection, use, sharing, identifiers, diagnostics, and any network behavior from the final binary and SDK/network review. Source inspection alone is not enough.
- [ ] **Privacy Policy URL:** use a public policy that covers the mobile game. The current listing master leaves this unset because the existing page has not been verified against a current signed build.
- [ ] **Copyright:** enter the legally correct owner and year. The developer-team display name is not sufficient proof of the copyright holder.
- [ ] **App Review contact:** first/last name and email are now saved in App Store Connect, with no-sign-in review notes entered. The phone field remains blank; the account owner must supply a reachable phone. Do not store the person's name or phone in the repository. `hi@mosselora.com` remains the public support address.

## Google Play Console

Latest Browser readback: draft app `4974796025760682569`, package `com.streamentry.evergrove`; default listing screenshots were accepted earlier, no bundle is uploaded, and setup is 8/12 complete. Privacy policy, Content rating, Data safety, and Trader status remain pending. Google Play App pricing reads Free. Production Countries/regions now reads Targeted (178): all 177 listed countries/regions plus Rest of World. This selection is saved as changes not yet submitted for review; the Production track is Inactive and Send app for review is disabled. The account shows as Personal. Managed publishing is off, so standard publishing is selected. Google Play Games on PC is opted out; Android XR remains on the mobile track by default. Do not submit owner certifications without the final signed build and the owner's actual legal facts.

The account owner must complete or confirm:

- [ ] **IARC:** accept the Terms of Use and answer the content questionnaire.
- [ ] **Data safety:** declare collection, sharing, security, and deletion from the final signed binary, SDK inventory, and network review.
- [ ] **Trader status:** make the account's required business/trader declaration from the account owner's actual legal status.
- [ ] **Target audience:** Play currently stores 18+. The founder direction now identifies children aged 7+ as co-players; decide whether children are part of the app’s intended audience and complete the applicable Families declaration before submission. Do not claim child co-play as implemented.
- [ ] **Production access:** verify whether this Personal account is subject to the closed-test requirement; its creation date and current eligibility were not visible in the last record.
- [ ] **Release setting:** keep standard publishing selected with Managed publishing off, then read the saved setting back before submission.

## Engineering evidence still required first

- A licensed Unity 6.3 build from current worktree `5bfe306` with research controls excluded, plus verified iOS and Play signing.
- Physical iPhone and Android device QA, and current screenshots/preview footage from those builds.
- Current binary privacy/network review. The source's optional `WorldClient` is not attached to the default offline bootstrap, but that does not establish final binary behavior.
- Resolve the Unity Editor code-signature problem. Do not use the retained Android AAB: `jarsigner` reports `CN=Android Debug`, not the Play upload key.
- Restore Unity Hub/Editor licensing so the `com.unity.editor.headless` entitlement is available for batch builds. The latest isolated smoke did not pass its licensing gate.
- The installed Unity Hub and Editor app bundles fail strict signature checks, both cached Editor packages fail integrity checks, and the Hub could not read its saved account token. Use a verified Hub/Editor installation and sign in/activate the Unity license in that app; do not send credentials in chat.
- Fresh check: `codesign --verify --deep --strict` still reports the installed Unity 6.3 Editor has an invalid arm64 signature. `unity auth status --format json` returns `LOCAL_STORE_UNWRITABLE` in the workspace sandbox; the license-status query and an escalated auth-status attempt both hung and were interrupted. No credentials were entered or displayed, and no build or license activation completed.
- LaunchServices cannot open the installed Hub bundle and the read-only license status command timed out. A Unity CLI sign-in refresh was attempted but could not reach the sign-in service; no credentials were entered and no license activation occurred.
- Unity CLI now reports a stored OAuth login with a stale session; refresh failed because the sign-in service was unreachable. The user must refresh the Unity session through a verified Hub/account flow before a licensed headless build can run.
- Keep English as the only submission locale until native reviewers approve translations. The mapped locale drafts have not been natively reviewed.

Apple automatic release is already recorded as selected on build 2; it still requires a new eligible build and review. Nothing is approved, publicly available, or scheduled for public release yet.

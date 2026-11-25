# Mosselora store-owner handoff — 2026-11-25

This is a handoff of account-owner actions, not a completed legal review. No declaration in this checklist has been submitted during the latest-source recheck. The latest source is `8c9e543`; no verified signed build exists for it.

## Apple App Store Connect

The first resumed Browser check showed an App Store Connect login URL with `authResult=FAILED`; `requestUserTakeover()` was invoked and no account response was entered. A later authenticated Browser session read the current version and availability state below. Build 2 predates current source and remains unsubmitted.

Latest Browser readback confirms the Apple record is still `Prepare for Submission`, build 2 remains attached, automatic release is selected, and App Privacy / Privacy Policy URL remain unset. App Availability is now United States only, effective on app release. China Mainland is excluded; App Compliance still shows two China-specific items as missing. Apple also identifies the developer as a trader in its DSA section. This handoff does not change or complete any of those legal declarations.

The account owner/Admin must complete these items against the eventual release candidate:

- [ ] **Content rights:** review the shipped asset and dependency inventory, including generated art, fonts, audio, third-party text/assets, and licenses. Submit only the truthful rights answer.
- [ ] **Age rating:** answer the questionnaire from the finished game content and interactions.
- [ ] **App Privacy:** confirm collection, use, sharing, identifiers, diagnostics, and any network behavior from the final binary and SDK/network review. Source inspection alone is not enough.
- [ ] **Privacy Policy URL:** use a public policy that covers the mobile game. The current listing master leaves this unset because the existing page has not been verified against a current signed build.
- [ ] **Copyright:** enter the legally correct owner and year. The developer-team display name is not sufficient proof of the copyright holder.
- [ ] **App Review contact:** provide the responsible person's first and last name, email, and phone. `hi@mosselora.com` is the public support address; it does not supply the named review contact or phone.

## Google Play Console

Latest Browser readback: draft app `4974796025760682569`, package `com.streamentry.evergrove`; default listing screenshots were accepted earlier, no bundle is uploaded, and setup is 8/12 complete. The remaining tasks are Privacy policy, Content rating, Data safety, and Trader status. The account shows as Personal; country/region rollout remains unset. Managed publishing is off and Send app for review is disabled. Google Play Games on PC is opted out; Android XR remains on the mobile track by default. Do not submit owner certifications without the final signed build and the owner's actual legal facts.

The account owner must complete or confirm:

- [ ] **IARC:** accept the Terms of Use and answer the content questionnaire.
- [ ] **Data safety:** declare collection, sharing, security, and deletion from the final signed binary, SDK inventory, and network review.
- [ ] **Trader status:** make the account's required business/trader declaration from the account owner's actual legal status.
- [ ] **Target audience:** confirm the saved 18+ selection against the product and current policy.
- [ ] **Production access:** verify whether this Personal account is subject to the closed-test requirement; its creation date and current eligibility were not visible in the last record.
- [ ] **Release setting:** keep standard publishing selected with Managed publishing off, then read the saved setting back before submission.

## Engineering evidence still required first

- A licensed Unity 6.3 build from `8c9e543` with research controls excluded, plus verified iOS and Play signing.
- Physical iPhone and Android device QA, and current screenshots/preview footage from those builds.
- Current binary privacy/network review. The source's optional `WorldClient` is not attached to the default offline bootstrap, but that does not establish final binary behavior.
- Resolve the Unity Editor code-signature problem. Do not use the retained Android AAB: `jarsigner` reports `CN=Android Debug`, not the Play upload key.
- Restore Unity Hub/Editor licensing so the `com.unity.editor.headless` entitlement is available for batch builds. The latest isolated smoke did not pass its licensing gate.
- The installed Unity Hub and Editor app bundles fail strict signature checks, both cached Editor packages fail integrity checks, and the Hub could not read its saved account token. Use a verified Hub/Editor installation and sign in/activate the Unity license in that app; do not send credentials in chat.
- LaunchServices cannot open the installed Hub bundle and the read-only license status command timed out. No Unity sign-in or license activation was attempted by this task.
- Keep English as the only submission locale until native reviewers approve translations. The mapped locale drafts have not been natively reviewed.

Apple automatic release is already recorded as selected on build 2; it still requires a new eligible build and review. Nothing is approved, publicly available, or scheduled for public release yet.

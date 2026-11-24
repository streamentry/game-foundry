# Mosselora 1.0 store screenshot candidates

These JPEGs come from earlier Unity 6.3 build-1 captures. The PNG originals remain in the linked QA folders. JPEG conversion retained the capture dimensions and removed the alpha channel; no gameplay or text was added. Current source now includes the Android engine-stripping fix in `87a0bcd`; recapture from a newly built current candidate before any store upload.

| Store | Asset | Capture source | Device / OS | Dimensions | Review |
| --- | --- | --- | --- | --- | --- |
| Apple | `apple/iphone-1206x2622-puzzle.jpg` | [Simulator first launch](../../../../games/evergrove/docs/qa/ios-simulator-release-candidate-2026-11-23/mosselora-first-launch-build1.png) | iPhone Simulator, iOS 26.5, app build 1 | 1206 × 2622 | Matches Apple's listed iPhone Dynamic Island medium-display size. Actual Simulator capture; not a physical-device capture. |
| Google Play | `google-play/phone-1080x2400-01-puzzle.jpg` | [Android first launch](../../../../games/evergrove/docs/qa/android-emulator-release-candidate-2026-11-23/mosselora-first-launch.png) | Pixel 7 AVD, Android 16 / API 36, version 1.0.0 (1) | 1080 × 2400 | Actual emulator capture. |
| Google Play | `google-play/phone-1080x2400-02-solved.jpg` | [Solved puzzle](../../../../games/evergrove/docs/qa/android-emulator-release-candidate-2026-11-23/mosselora-puzzle-complete.png) | Pixel 7 AVD, Android 16 / API 36, version 1.0.0 (1) | 1080 × 2400 | Actual emulator capture. |
| Google Play | `google-play/phone-1080x2400-03-choice-outcome.jpg` | [Choice outcome](../../../../games/evergrove/docs/qa/android-emulator-release-candidate-2026-11-23/mosselora-after-tree-choice.png) | Pixel 7 AVD, Android 16 / API 36, version 1.0.0 (1) | 1080 × 2400 | Actual emulator capture. |

Apple currently requires at least one iPhone screenshot; the iOS file matches a listed accepted size but is from earlier build 1. Google Play's live listing form requires 2–8 phone screenshots in 9:16 or 16:9, with each side 320–3,840 px; it recommends four screenshots for promotion. The three files here are from an older build and are not uploaded. Recapture at least two from the current build before treating the Play listing as complete.

The current ASC editor exposes the new Header/Search Results creative asset area but no standard screenshot upload well. No candidate above has been uploaded. Apple preview clips are retained separately as raw captures and still need accepted-resolution and metadata validation before upload.

References checked 2026-11-23:

- [Apple screenshot specifications](https://developer.apple.com/help/app-store-connect/reference/app-information/screenshot-specifications/)
- [Google Play preview asset requirements](https://support.google.com/googleplay/android-developer/answer/9866151)

## Google Play draft promotional assets — 2026-11-24

- `google-play/app-icon-512.png` is a 512 × 512 PNG resized from the Mosselora brand master `games/evergrove/brand/mosselora-app-icon-1024.png`. It is 508 KB and meets the Play form's displayed 512 px / 1 MB limits. SHA-256: `d53b9884487cf9de8066e5e1a181709e98c49a2ed78d258f3217b1d7c7d6bd97`.
- `google-play/feature-graphic-1024x500.png` is a center crop and resize of the ImageGen `evergrove-living-garden-promo-concept-2026-11-23.png`. It is 1024 × 500, 1.1 MB, and contains no text or game UI. SHA-256: `7c592462e7aa53d2263ce018b655dffe992f2cc7e8fec6c4ad33defea75c0660`.
- These are promotional assets, not gameplay evidence. The phone screenshots above are from an earlier build and remain ineligible for the current listing. The draft still needs at least two current-build phone screenshots before Play can review the listing; do not substitute generated art for screenshots or preview footage.

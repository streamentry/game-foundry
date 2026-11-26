# Mosselora 1.0 store screenshot candidates

This folder contains historical build-1 JPEGs and build-2 PNG captures from actual Unity builds. The build-2 media predates current `HEAD` `8c9e543`; recapture it from a current-source build before submission. The iOS build-2 source is recorded as `84c0592`; the Android AAB does not retain an exact source SHA. No gameplay or text was added to the screenshot captures.

| Store | Asset | Capture source | Device / OS | Dimensions | Review |
| --- | --- | --- | --- | --- | --- |
| Apple | `apple/iphone-1206x2622-puzzle.jpg` | [Simulator first launch](../../../../games/evergrove/docs/qa/ios-simulator-release-candidate-2026-11-23/mosselora-first-launch-build1.png) | iPhone Simulator, iOS 26.5, app build 1 | 1206 × 2622 | Matches Apple's listed iPhone Dynamic Island medium-display size. Actual Simulator capture; not a physical-device capture. |
| Google Play | `google-play/phone-1080x2400-01-puzzle.jpg` | [Android first launch](../../../../games/evergrove/docs/qa/android-emulator-release-candidate-2026-11-23/mosselora-first-launch.png) | Pixel 7 AVD, Android 16 / API 36, version 1.0.0 (1) | 1080 × 2400 | Actual emulator capture. |
| Google Play | `google-play/phone-1080x2400-02-solved.jpg` | [Solved puzzle](../../../../games/evergrove/docs/qa/android-emulator-release-candidate-2026-11-23/mosselora-puzzle-complete.png) | Pixel 7 AVD, Android 16 / API 36, version 1.0.0 (1) | 1080 × 2400 | Actual emulator capture. |
| Google Play | `google-play/phone-1080x2400-03-choice-outcome.jpg` | [Choice outcome](../../../../games/evergrove/docs/qa/android-emulator-release-candidate-2026-11-23/mosselora-after-tree-choice.png) | Pixel 7 AVD, Android 16 / API 36, version 1.0.0 (1) | 1080 × 2400 | Actual emulator capture. |

The build-1 JPEGs in this table are historical. On 2026-11-25, App Store Connect accepted the build-2 iPhone and iPad captures below; Play Console accepted the two build-2 phone captures below. Those uploads belong to the older build and must be replaced after a current-source release build. They are simulator/emulator captures, not physical-device evidence.

Apple's screenshots were accepted in the prior console session. No current-source preview video has been captured or uploaded. Earlier raw clips are from older builds and need recapture and format validation before use.

References checked 2026-11-23:

- [Apple screenshot specifications](https://developer.apple.com/help/app-store-connect/reference/app-information/screenshot-specifications/)
- [Google Play preview asset requirements](https://support.google.com/googleplay/android-developer/answer/9866151)

## Google Play draft promotional assets — 2026-11-24

- `google-play/app-icon-512.png` is a 512 × 512 PNG resized from the Mosselora brand master `games/evergrove/brand/mosselora-app-icon-1024.png`. It is 508 KB and meets the Play form's displayed 512 px / 1 MB limits. SHA-256: `d53b9884487cf9de8066e5e1a181709e98c49a2ed78d258f3217b1d7c7d6bd97`.
- `google-play/feature-graphic-1024x500.png` is a center crop and resize of the ImageGen `evergrove-living-garden-promo-concept-2026-11-23.png`. It is 1024 × 500, 1.1 MB, and contains no text or game UI. SHA-256: `7c592462e7aa53d2263ce018b655dffe992f2cc7e8fec6c4ad33defea75c0660`.
- These are promotional assets, not gameplay evidence. Do not substitute generated art for screenshots or preview footage.

## Build-2 screenshots — captured 2026-11-25

| Store | Asset | Device / OS | Dimensions | Console result | Current use |
| --- | --- | --- | --- | --- | --- |
| Apple | `apple/iphone-build2.png` | iPhone 17 Pro Simulator, iOS 26.5 | 1206 × 2622 | Accepted in App Store Connect | Historical build 2; recapture from current `HEAD` |
| Apple | `apple/ipad-13-build2.png` | iPad Pro 13-inch Simulator, iOS 26.5 | 2064 × 2752 | Accepted in App Store Connect | Historical build 2; recapture from current `HEAD` |
| Google Play | `google-play/mosselora-play-01.png` | Pixel 7 AVD, Android 16 / API 36 | 1080 × 1920 | Accepted in default listing | Historical build 2; recapture from current `HEAD` |
| Google Play | `google-play/mosselora-play-02.png` | Pixel 7 AVD, Android 16 / API 36 | 1080 × 1920 | Accepted in default listing | Historical build 2; recapture from current `HEAD` |

The build-2 screenshots are genuine simulator/emulator captures, not generated images. The public store pages have not been submitted. Keep the ImageGen feature graphic separate from gameplay evidence.

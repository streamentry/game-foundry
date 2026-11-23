# Mosselora 1.0 store screenshot candidates

These JPEGs come from earlier Unity 6.3 build-1 captures. The PNG originals remain in the linked QA folders. JPEG conversion retained the capture dimensions and removed the alpha channel; no gameplay or text was added. Current source now includes the Android engine-stripping fix in `87a0bcd`; recapture from a newly built current candidate before any store upload.

| Store | Asset | Capture source | Device / OS | Dimensions | Review |
| --- | --- | --- | --- | --- | --- |
| Apple | `apple/iphone-1206x2622-puzzle.jpg` | [Simulator first launch](../../../../games/evergrove/docs/qa/ios-simulator-release-candidate-2026-11-23/mosselora-first-launch-build1.png) | iPhone Simulator, iOS 26.5, app build 1 | 1206 × 2622 | Matches Apple's listed iPhone Dynamic Island medium-display size. Actual Simulator capture; not a physical-device capture. |
| Google Play | `google-play/phone-1080x2400-01-puzzle.jpg` | [Android first launch](../../../../games/evergrove/docs/qa/android-emulator-release-candidate-2026-11-23/mosselora-first-launch.png) | Pixel 7 AVD, Android 16 / API 36, version 1.0.0 (1) | 1080 × 2400 | Actual emulator capture. |
| Google Play | `google-play/phone-1080x2400-02-solved.jpg` | [Solved puzzle](../../../../games/evergrove/docs/qa/android-emulator-release-candidate-2026-11-23/mosselora-puzzle-complete.png) | Pixel 7 AVD, Android 16 / API 36, version 1.0.0 (1) | 1080 × 2400 | Actual emulator capture. |
| Google Play | `google-play/phone-1080x2400-03-choice-outcome.jpg` | [Choice outcome](../../../../games/evergrove/docs/qa/android-emulator-release-candidate-2026-11-23/mosselora-after-tree-choice.png) | Pixel 7 AVD, Android 16 / API 36, version 1.0.0 (1) | 1080 × 2400 | Actual emulator capture. |

Apple currently requires at least one iPhone screenshot; the iOS file matches a listed accepted size but is from earlier build 1. Google Play currently requires at least two screenshots across device types to publish a listing. The three files here cover phone only, so the second device type is still missing. Their 1080 × 2400 ratio is within Google's 2.3:1 maximum but is not its recommended 9:16 portrait or 16:9 landscape game format. Re-capture or frame the Google set for 9:16 and add a real supported second device type before treating it as complete.

The current ASC editor exposes the new Header/Search Results creative asset area but no standard screenshot upload well. No candidate above has been uploaded. Apple preview clips are retained separately as raw captures and still need accepted-resolution and metadata validation before upload.

References checked 2026-11-23:

- [Apple screenshot specifications](https://developer.apple.com/help/app-store-connect/reference/app-information/screenshot-specifications/)
- [Google Play preview asset requirements](https://support.google.com/googleplay/android-developer/answer/9866151)

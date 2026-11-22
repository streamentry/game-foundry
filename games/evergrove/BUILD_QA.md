# Evergrove: getting a real Unity build (honest release gate)

As of 2026-11-22, pure C# domain tests run in GitHub Actions. **No licensed Unity Editor build or real-device runtime evidence has been provided in the repository.** Do not equate a clean `.NET` test run with a mobile-ready Unity client.

## One-click preview builds

A manual [Build Evergrove Unity Playtest](../../.github/workflows/evergrove-unity-playtest.yml) workflow can produce an **unsigned Android test APK**, **unhosted WebGL ZIP artifact**, or **unsigned iOS Xcode project**. It uses `game-ci/unity-builder@v4`, a custom Editor build method `EvergroveBuild.Build` and the requested Unity 6.3 version from `ProjectSettings/ProjectVersion.txt`.

**Maintainer setup (one time):** Open repository **Settings → Secrets and variables → Actions** and configure a supported Unity license. GameCI Personal activation requires `UNITY_LICENSE`; Pro activation uses `UNITY_SERIAL`, `UNITY_EMAIL`, `UNITY_PASSWORD`. Do not paste credentials into chat, commit them or log them. See [GameCI activation instructions](https://game.ci/docs/github/activation/). This workflow refuses to run without license information, and it never publishes an app or deploys the optional Cloudflare backend.

Once credentials are configured, visit **Actions → Build Evergrove Unity Playtest → Run workflow** and choose **Android**, **WebGL** or **iOS**. After success, download the artifact from the workflow run. Android produces `Evergrove-Playtest.apk` (not a signed Play Store release). iOS produces an unsigned **Xcode project**: open and sign it on your own Mac with Xcode; it is **not an IPA**. WebGL produces a folder for self-hosting; serve over localhost or HTTPS (do not double-click `index.html` from a `file://` URL).

The automated Unity build method creates the otherwise empty bootstrap scene. **No manually authored scene asset is needed in the repository.** Runtime UI is constructed in code. This is a conscious temporary workflow; final studio-quality work should be imported into proper scenes, prefabs and assets after player validation.

## Manual first-play acceptance checklist

- [ ] Opens on a compact Android phone and at least one iPhone in the Unity Editor / local Xcode workflow.
- [ ] Touching the spring's tiles actually rotates them. There are **no input-system exceptions**.
- [ ] Before the first completion the creek is visibly dry. Complete stream 1: the creek visibly fills in 2D and 2.5D.
- [ ] Can solve all 12 boards. Real players should be able to infer *why* water flows; use hints without feeling penalized.
- [ ] Choose a tree, a crossing or flowers. On return, show the same changes and the correct NPC memory.
- [ ] The 2D default works even if the miniature renderer/RenderTexture fails. Animation and sound are off by default.
- [ ] Force close after a move and after a choice; reopen without losing progress or mixing worlds.
- [ ] Undo a solved board and reconsider one choice. Avoid irreversible accidental erasure.
- [ ] No missing font, input package, UI sprite, scene import, shader errors or clipped controls at 320–430px logical widths.
- [ ] Test Android back behavior, iOS safe areas, OS text scaling, keyboard focus, VoiceOver/TalkBack and older device frame time.
- [ ] Run a privacy review: all data local, no analytics or network requests without informed opt-in.
- [ ] Save actual screenshots, Unity build logs and device findings as evidence. Mark a test complete only after someone has performed it.

## Product stop/continue rule

This build exists to help people actually experience the game. Compare `Living World` with `Puzzle Only` using manually balanced players. Continue development only if the living-world layer measurably improves comprehension and voluntary replay, and the distribution channel is viable. A successful Unity build proves **buildability**, not product-market fit.

References: [GameCI Unity Builder](https://game.ci/docs/github/builder/), [Unity 6.3 uGUI input setup](https://docs.unity.com/en-us/engine/6000.3/manual/uitoolkits/uielements/uie-support-for-runtime-ui/uie-render-runtime-ui/uie-runtime-event-system).

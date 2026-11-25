# I149 — The postcard opens the phone's share sheet

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player (Haiku walk-through of b152, as a 52-year-old)

On the ending: "**'Copy garden postcard'** and 'Explore another garden': **'Copy' is a web-clipboard word**… 'Save postcard' or 'Share' with a painted frame would fit better."

Playing it: tapping the postcard copied text to the clipboard and showed a toast: "Postcard copied. Paste it into a message to share your garden." That's three steps (copy, leave the game, open a message app and paste), described in words. For people over 40, sending something from a phone means **the share sheet**, the same one they use for photos.

## PM decision

- The postcard opens **the phone's own share sheet** (iOS `UIActivityViewController`, Android's `ACTION_SEND` chooser) with the same spoiler-free postcard text. The player picks Messages, Mail or WhatsApp, or cancels. The game uploads nothing and adds no link or identifier (the existing tests assert that).
- If no sheet can be shown (editor, failure), it falls back to the clipboard and the old toast.
- Label: "Copy garden postcard" becomes **"Send a garden postcard"** (English source). The 29 draft catalogues still say "copy" and need re-translation before any of them ships (none ship today).
- This is the same no-server sharing rung the vision lists (§4.4 rung 1). It's a better path to the existing postcard, not a new social feature.

## Dev

- New `Assets/Plugins/iOS/MosseloraShare.mm` (+ `.meta`): `_MosseloraShareText` presents the activity sheet from `UnityGetGLViewController()`, as a centred, arrowless popover on iPad.
- New `Assets/Scripts/Unity/NativeShare.cs`: `Text(text, title)` uses iOS `DllImport`, or on Android an `Intent.ACTION_SEND` (`text/plain`) via `createChooser` and `startActivity`. It returns false elsewhere or on failure (logging one warning).
- `EvergroveApp`: both postcard buttons (the ending and the menu card) try `NativeShare.Text` first, then the clipboard plus notice.
- `GameLocalization` English `postcard` string → "Send a garden postcard".

## Result

- **iOS (b157, iPhone 17 simulator, hands-on):** menu → "Send a garden postcard". The system share sheet rose over the game with the postcard text (preview titled "Mosselora"), offering Reminders, More, Copy and Save to Files (the simulator has no Messages or Mail accounts). The system log shows `UIActivityViewController … view did appear`. Swiping it down returned to the menu. Local screenshot: `docs/improvements/I149-share-sheet.png` (gitignored).
- **Android (a149):** the APK builds, so the share branch compiles. **Not run:** the chooser on the emulator.
- The scripted tour captures all 20 screens. Verify, the docs contract and the .NET tests (50,072 assertions, including the postcard's no-link/no-identifier checks) pass.

**Not verified:** Messages and WhatsApp on a real phone, the iPad popover, the Android chooser at runtime, and the clipboard fallback path (editor only).

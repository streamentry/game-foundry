# Evergrove localization: 30 languages, offline, state-safe

**Status:** 30 selectable languages and complete *draft* game-copy catalogs are in the repo; no native-language or phone-rendering sign-off is implied.

## Coverage

| Language code | Language (native) | Coverage |
| --- | --- | --- |
| en | English | Built-in canonical text |
| zh-Hans | 简体中文 | 73/73 |
| zh-Hant | 繁體中文 | 73/73 |
| es | Español | 73/73 |
| pt-BR | Português (Brasil) | 73/73 |
| fr | Français | 73/73 |
| de | Deutsch | 73/73 |
| ja | 日本語 | 73/73 |
| ko | 한국어 | 73/73 |
| ar | العربية | 73/73; RTL QA needed |
| hi | हिन्दी | 73/73; shaping QA needed |
| ru | Русский | 73/73 |
| id | Bahasa Indonesia | 73/73 |
| vi | Tiếng Việt | 73/73 |
| tr | Türkçe | 73/73 |
| th | ไทย | 73/73; line-break QA needed |
| it | Italiano | 73/73 |
| nl | Nederlands | 73/73 |
| pl | Polski | 73/73 |
| uk | Українська | 73/73 |
| fa | فارسی | 73/73; RTL QA needed |
| he | עברית | 73/73; RTL QA needed |
| ms | Bahasa Melayu | 73/73 |
| fil | Filipino | 73/73 |
| bn | বাংলা | 73/73; shaping QA needed |
| ur | اردو | 73/73; RTL QA needed |
| ta | தமிழ் | 73/73; shaping QA needed |
| te | తెలుగు | 73/73; shaping QA needed |
| sw | Kiswahili | 73/73 |
| sv | Svenska | 73/73 |

These 30 languages are a **commercially broad coverage set**, not a mathematically ranked list of the world's 30 most spoken languages. Country-specific translation for other regions and dialects may be added later when evidence warrants it.

## How detection and override work

First launch and **Automatic** mode use `CultureInfo.CurrentUICulture` (BCP-47) and `Application.systemLanguage`. `es-MX` resolves to `es`, `zh-Hant-HK` and `zh-TW` to `zh-Hant`, `zh-CN` to `zh-Hans`, and `pt-BR` or `pt-PT` to `pt-BR`. Unsupported languages fall back to English. Device-language changes may require restarting the game for Unity to report the new locale.

The **Language** button opens a full native-name picker plus **Automatic (device language)**. A manual selection writes `evergrove:language-choice:v1` to PlayerPrefs. Puzzle progression is stored separately in `evergrove-world-v2.json`; neither selection nor change touches it. No online translator, account or permission is required.

The game reads bundled `Assets/Resources/Evergrove/Locales/<locale>.json` using `JsonUtility` and validates the entire catalog before activation. A broken or unavailable resource falls back to English and the attempted selection is not saved. `GameLocalization` is Unity-free so routing and state separation can be tested with .NET.

## What is and is not translated

All **73 player-facing compact-copy keys** (water puzzle prompts/feedback, game modes, language switcher, settings, world state, causal choices, NPC dialogue, memories, journal and endings) have non-empty localizations. To avoid 30 unrelated narrative variants in an early experiment, **all 30 languages use one concise state-dependent script** (`LocalizedVillage.cs`); the richer original *English* prose (`VillageStory.cs`) is kept intact for future editorial expansion. These are editorial first drafts; a native speaker must review them before a store listing claims high-quality translation.

Twelve original **puzzle names** are English-only and are deliberately not shown in non-English sessions; stream number remains localized. Character proper names Mira, Orin and Fen are retained across locales. The local copy-to-clipboard playtest report intentionally stays in English structured key=value format for comparable researcher data; UI consent/explanation is localized.

## Text rendering and accessibility blocker

The game currently draws UI using legacy Unity **uGUI Text**, whose glyph/layout capabilities cannot be assumed to shape RTL Arabic/Persian/Urdu and Indic conjuncts correctly. Right alignment is **not** bidi or shaping. `LocaleFonts` picks likely fonts installed on the OS as a best effort, without bundling or redistributing font files. A **shipping-quality fix requires using Unity UI Toolkit's Advanced Text Generator** (HarfBuzz/ICU, suitable dynamic font assets and language direction), or a correctly tested equivalent text stack, plus native device QA. Until this work is done, these languages are translated and selectable but **not certified as visually correct**.

Sources:
- [Unity 6.3 — Set language direction](https://docs.unity.com/en-us/engine/6000.3/manual/uitoolkits/uielements/uie-work-with-text/language-direction)
- [Unity Advanced Text Generator](https://docs.unity.com/en-us/engine/6000.0/manual/uitoolkits/uielements/uie-work-with-text/uie-advanced-text-generator)
- [Unity systemLanguage](https://docs.unity3d.com/ScriptReference/Application-systemLanguage.html)

## Tests and future translation workflow

```bash
python3 tools/verify_evergrove_project.py
dotnet run --project games/evergrove/Tests/Evergrove.Core.Tests.csproj --configuration Release
```

The static validator checks all 29 catalog files, 73 keys, locale code consistency and interpolation syntax. .NET tests import every JSON resource, assert no missing keys or incorrect placeholders and verify changing locale cannot alter water puzzle/choice history.

**Before any international launch:** native-speaker review of each locale; screenshot QA of settings, choice cards, long dialogues and endings; real-device glyph coverage; true RTL and Indic text shaping; word breaking for Thai; screen-reader labels; and mobile performance. If the first-phone QA fails, keep that locale disabled in production instead of showing broken text.

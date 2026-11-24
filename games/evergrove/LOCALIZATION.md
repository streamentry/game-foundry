# Evergrove localization: 30 languages, offline, state-safe

**Status:** 30 **research/development** locale choices and complete *draft* catalogs are in the repo. Future **non-development player builds are source-gated to English only by default**, pending individually certified languages. This does **not** mean English is already certified or that a release is approved.

## Coverage

| Language code | Language (native) | Coverage |
| --- | --- | --- |
| en | English | Built-in canonical text |
| zh-Hans | 简体中文 | 101/101 |
| zh-Hant | 繁體中文 | 101/101 |
| es | Español | 101/101 |
| pt-BR | Português (Brasil) | 101/101 |
| fr | Français | 101/101 |
| de | Deutsch | 101/101 |
| ja | 日本語 | 101/101 |
| ko | 한국어 | 101/101 |
| ar | العربية | 101/101; RTL QA needed |
| hi | हिन्दी | 101/101; shaping QA needed |
| ru | Русский | 101/101 |
| id | Bahasa Indonesia | 101/101 |
| vi | Tiếng Việt | 101/101 |
| tr | Türkçe | 101/101 |
| th | ไทย | 101/101; line-break QA needed |
| it | Italiano | 101/101 |
| nl | Nederlands | 101/101 |
| pl | Polski | 101/101 |
| uk | Українська | 101/101 |
| fa | فارسی | 101/101; RTL QA needed |
| he | עברית | 101/101; RTL QA needed |
| ms | Bahasa Melayu | 101/101 |
| fil | Filipino | 101/101 |
| bn | বাংলা | 101/101; shaping QA needed |
| ur | اردو | 101/101; RTL QA needed |
| ta | தமிழ் | 101/101; shaping QA needed |
| te | తెలుగు | 101/101; shaping QA needed |
| sw | Kiswahili | 101/101 |
| sv | Svenska | 101/101 |

These 30 languages are a **commercially broad coverage set**, not a mathematically ranked list of the world's 30 most spoken languages. Country-specific translation for other regions and dialects may be added later when evidence warrants it.

## How detection and override work

First launch and **Automatic** mode use `CultureInfo.CurrentUICulture` (BCP-47) and `Application.systemLanguage`. `es-MX` resolves to `es`, `zh-Hant-HK` and `zh-TW` to `zh-Hant`, `zh-CN` to `zh-Hans`, and `pt-BR` or `pt-PT` to `pt-BR`. Unsupported languages fall back to English. Device-language changes may require restarting the game for Unity to report the new locale.

The **research-only Language** button opens all 30 native-name choices plus **Automatic (device language)**. The non-development candidate defaults to English and hides the picker until at least one more locale is explicitly allowed after certification. If an old research preference selects an uncertified locale, the player candidate falls back to English **without changing the saved world**. A manual research selection writes `evergrove:language-choice:v1` to PlayerPrefs. Puzzle progression is stored separately in `evergrove-world-v2.json`; neither selection nor change touches it. No online translator, account or permission is required.

### Regional and graphical first-puzzle caveats

`pt-PT` currently routes to `pt-BR` as a **fallback**, not an independently reviewed European Portuguese localization. Before marketing to Portugal, explicitly approve the regional fallback with a native reviewer, provide a dedicated `pt-PT` catalog, or avoid claiming that locale.

The first puzzle now uses **procedural blue wave lines for the spring and a green sprout for the garden**, in place of English `S/G` letters. All 30 concise first-puzzle prompts describe those shapes, but translations remain drafts and the icons are **not visually, contrast or screen-reader certified** until actual device checks. This change does not itself fix uGUI shaping.

**Locale lifecycle:** `Draft → Technical QA → Native QA → Store QA → Certified`. In source, [LocaleShippingPolicy](Assets/Scripts/Core/LocaleShippingPolicy.cs) now separates 30 research catalogs from a deliberately narrow **English-only non-development candidate allowlist**. No locale, including English, has a completed native/device/store certification record. English is a fallback candidate, **not a passed M5 gate**. Adding a shipping locale requires a reviewed allowlist code change and recorded M5 evidence, not merely a JSON file. See [execution control](docs/specs/09-EXECUTION-CONTROL.md).

The game reads bundled `Assets/Resources/Evergrove/Locales/<locale>.json` using `JsonUtility` and validates the entire catalog before activation. A broken or unavailable resource falls back to English and the attempted selection is not saved. `GameLocalization` is Unity-free so routing and state separation can be tested with .NET.

## What is and is not translated

All **121 player-facing compact-copy keys** (water puzzle prompts/feedback, game modes, language switcher, settings, world state, causal choices, NPC dialogue, memories, journal, endings the nine garden-file keys, six screen-reader keys, three character-name keys, and the painted-UI keys for the menu, close, text and board size, gentle motion, postcard, seasonal neighbour lines, the hub news label and varied repeat memories) have non-empty localizations. To avoid 30 unrelated narrative variants in an early experiment, **all 30 languages use one concise state-dependent script** (`LocalizedVillage.cs`); the richer original *English* prose (`VillageStory.cs`) is kept intact for future editorial expansion. These are editorial first drafts; a native speaker must review them before a store listing claims high-quality translation.

Twelve original **puzzle names** are English-only and are deliberately not shown in non-English sessions; stream number remains localized. Character proper names Mira, Orin and Fen are retained across locales. The local copy-to-clipboard playtest report intentionally stays in English structured key=value format for comparable researcher data; UI consent/explanation is localized.

## Text rendering and accessibility blocker

The game currently draws UI using legacy Unity **uGUI Text**, whose glyph/layout capabilities cannot be assumed to shape RTL Arabic/Persian/Urdu and Indic conjuncts correctly. Right alignment is **not** bidi or shaping. `LocaleFonts` picks likely fonts installed on the OS as a best effort, without bundling or redistributing font files. A **shipping-quality fix requires using Unity UI Toolkit's Advanced Text Generator** (HarfBuzz/ICU, suitable dynamic font assets and language direction), or a correctly tested equivalent text stack, plus native device QA. Until this work is done, these languages remain selectable **in the development/research channel only**, and are **not certified as visually correct**.

Sources:
- [Unity 6.3 — Set language direction](https://docs.unity.com/en-us/engine/6000.3/manual/uitoolkits/uielements/uie-work-with-text/language-direction)
- [Unity Advanced Text Generator](https://docs.unity.com/en-us/engine/6000.0/manual/uitoolkits/uielements/uie-work-with-text/uie-advanced-text-generator)
- [Unity systemLanguage](https://docs.unity3d.com/ScriptReference/Application-systemLanguage.html)

## Tests and future translation workflow

```bash
python3 tools/verify_evergrove_project.py
dotnet run --project games/evergrove/Tests/Evergrove.Core.Tests.csproj --configuration Release
```

The static validator checks all 29 catalog files, 121 keys, locale code consistency, interpolation syntax and source-level consumer-locale isolation. Pure C# tests also assert draft locale selection is allowed only by the research policy; they do **not** replace compiled-player verification. .NET tests import every JSON resource, assert no missing keys or incorrect placeholders and verify changing locale cannot alter water puzzle/choice history.

**Before any international launch:** native-speaker review of each locale; screenshot QA of settings, choice cards, long dialogues and endings; real-device glyph coverage; true RTL and Indic text shaping; word breaking for Thai; screen-reader labels; and mobile performance. If the first-phone QA fails, keep that locale disabled in production instead of showing broken text.

## Ending heading `ending_title` (machine-drafted, pending native review)

The 82nd key, `ending_title`, is the visible heading of the ending card (English: `YOUR GARDEN'S STORY`). Its 29 non-English translations are **machine-drafted** and remain unreviewed, pending native review, following the same draft status as the other catalogs. The English source is in `GameLocalization.cs`; the heading uses the same title style as the memory and journal headings and scales with `TextScale`.

## New v4 prototype copy (draft, not certified)

The 121 canonical text keys include five staged-help/save-warning phrases: `hint_notice`, `hint_focus`, `hint_more`, `hint_turn`, and `save_warning`. The first-puzzle instructions refer to geometric blue waves for the spring and a green sprout for the garden; they no longer depend on Latin `S/G` glyphs. Every translation remains an unreviewed **draft**. Font shaping, RTL/Indic display, layout, native signoff and device performance are still open.

The nine `garden_*` keys (copy, copied, restore, replace warning, and five plain-language import errors) support the settings garden file. They were added as **machine-drafted** text in all 29 catalogs and English, pending native review. Keys: `garden_copy`, `garden_copied`, `garden_restore`, `garden_replace`, `garden_err_empty`, `garden_err_not_garden`, `garden_err_newer`, `garden_err_damaged`, `garden_err_too_large`.

## Unreadable saved garden `save_unreadable` (machine-drafted, pending native review)

The 98th key, `save_unreadable`, is the banner shown when both save files exist but cannot be read (`WorldStore.RecoveryBlocked`). It is distinct from `save_warning`, which is kept for genuine write failures such as a full device. Its English source is in `GameLocalization.cs`; all 29 non-English translations are **machine-drafted** and remain unreviewed. The banner keeps the TextScale sizing used by the other save warning. It is not included in the screen-reader bridge.

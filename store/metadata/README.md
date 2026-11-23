# Store metadata drafts

This folder contains draft English masters for the eight requested games and Mosselora (internal project slug Evergrove), the top-30 roster locale map, and full-description drafts for the mapped locale groups.

- Each game folder's en-US.json holds the Apple and Google English fields plus unresolved release decisions.
- `evergrove/descriptions-draft.tsv` adds 25 mapped Mosselora listing-description drafts from the existing top-30 roster. Duplicate/proxy roster rows share one draft; unsupported Apple locales remain marked `unsupported`. In-game locale availability and certification are tracked separately.
- locale-map.csv maps each roster row to current Apple and Google Play store-listing locales. Unsupported language rows have no invented locale.
- descriptions-draft.tsv contains 200 full-description translations: eight games × 25 unique locales. Each row is marked draft — native review pending; reviewer and review date are blank.

No translation has been reviewed by a native speaker. The text is not approved for upload. Keep the English listing active until the relevant locale receives native review, terminology and character-limit checks, and a recorded reviewer/date. Evergrove's 25 translations are model-generated drafts. It is even more restricted: its non-development candidate is source-gated to English only, and English is not M5-certified. Do not enable translated store listings until matching in-game locales pass technical, native and device QA.

Proxy rows are deliberately collapsed: Modern Standard and Egyptian Arabic share the Arabic locale; Wu Chinese reuses the Simplified Chinese draft; Yue uses standard written Traditional Chinese, not a spoken Cantonese translation. Nigerian Pidgin and Hausa have no direct store-listing locale in the current official lists, so no translation is attributed to those languages. Western Punjabi is drafted in Shahmukhi under generic Punjabi, but both consoles need a locale/script presentation check.

All descriptions describe current prototype behavior. They do not claim cognitive or health benefits. Product names stay in English. The user confirmed all games are free on 2026-11-23. Mosselora's Apple $0.00 price schedule is saved across 175 territories, but App Availability is not configured; its Google Play app record was not created. Original-batch store price settings remain unconfigured. The user changed the public working name to **Mosselora**; Evergrove remains the internal project codename, slug and package namespace. The existing Apple record was renamed to Mosselora. Google Play has no exact-title Mosselora result; its package name is available but no record exists. Formal trademark clearance, Android package reservation, support/privacy URLs, age ratings, territories and account-owner declarations remain unresolved.

## Locale sources

Mappings were checked against Apple's [App Store localization shortcodes](https://developer.apple.com/documentation/appstoreconnectapi/managing-metadata-in-your-app-by-using-locale-shortcodes) and Google's [Play Console listing-language list](https://support.google.com/googleplay/android-developer/answer/9844778) on 2026-11-22. The platform codes differ for several languages (for example, Apple Arabic `ar-SA` and Google Arabic `ar`). Console availability remains the final check for each account and app.

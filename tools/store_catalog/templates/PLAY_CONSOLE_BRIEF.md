# Play Console setup brief (browser, owner-authorized answers)

The account owner explicitly authorized every answer below on 2026-11-28 (offline puzzle
games: no data collection, no ads, no login, adult audience). Use ONLY the built-in browser
tools (`mcp__Claude_Browser__*`). Pass `tabId: "seed"` on EVERY action — other tabs belong
to the user; never touch them. Do not change anything else: no account settings, no user
permissions, no other apps, never Mosselora (com.streamentry.evergrove). If a click is
denied by a safety check, skip that app and report — do not work around it.

Base URL: `https://play.google.com/console/u/0/developers/8959600245071053925/app/<APPID>/`
Pages are slow: wait 6–10 s after navigating; if the page is a blank spinner, wait more.
Take a 0.6-scale screenshot to confirm each page before clicking (coordinates below are in
the 800×877 screenshot frame; adapt if the layout differs). Each form's Save/Next button is
at bottom-right ≈ (757, 850); after saving the footer reads "Change saved".

Apps (slug: APPID):
battleship-harbor 4972638011466589542 · cage-hearth 4972735154459079443 ·
futoshiki-terrace 4973686158257263972 · good-eye 4972679698729579531 ·
hidato-walk 4975776348739790862 · hitori-grove 4974581484995859896 ·
kakuro-compass 4973576986328695357 · kropki-pairs 4975703225544739199 ·
kuromasu-windows 4975539601889662368 · loop-lantern 4973280796964578004 ·
nonogram-canvas 4975648021063860326 · number-pyramid 4973902952330702322 ·
bridge-isles (APPID unknown: open `…/developers/8959600245071053925/app-list`, search
"Bridge Isles", read the id from its URL).

First check `app-content/overview` for each app: some declarations may already be done
(battleship-harbor was partly set up). Do only what "Need attention" lists.

1. `app-content/privacy-policy`: click the field ≈(295,408), type
   `https://streamentry-puzzles.4rqnf2gvxf.workers.dev/privacy/`, Save.
2. `app-content/ads-declaration`: "No, my app does not contain ads" ≈(300,290), Save.
3. `app-content/testing-credentials`: "No" ≈(300,410), Save.
4. `app-content/content-rating-overview` → "Start questionnaire" ≈(138,331). Category step:
   email field ≈(510,235) type `hi@mosselora.com`; "Game" ≈(298,311); IARC terms checkbox
   ≈(298,526); Next. Questionnaire: sections open one at a time; answer **No** to every
   question (Violence, Fear, Sexuality, Gambling, Language, Controlled Substance, Crude
   Humor, Digital Purchases, then all 6 Miscellaneous). "No" radios are at x≈147: after each
   answer scroll down 3 and screenshot to find the next. Then Save ≈(701,850), Next
   (757,850) to Summary, Save (757,850).
5. `app-content/target-audience-content`: tick only "18 and over" ≈(298,547), Next, Save.
6. `app-content/data-privacy-security`: Next; "No" ≈(83,347); Next; Save.
7. `app-content/ad-id-declaration`: "No" ≈(83,525), Save.
8. `app-content/government-apps`: "No" ≈(83,295), Save.
9. `app-content/finance`: scroll down 15, tick "My app doesn't provide any financial
   features" ≈(300,664), Next, Save.
10. `app-content/health`: scroll down 15, tick "My app does not have any health features"
    ≈(300,664), Next, Save.
11. `app-content/trader-status`: leave the pre-selected "Yes, my trader status applies",
    leave ToS empty, Save.
12. `store-settings`: App category "Edit" ≈(733,152); open the Category dropdown
    ≈(310,297), scroll the list down 3, click "Puzzle" (verify the field shows Puzzle;
    for word-search/word games choose "Word" if offered, else Puzzle), Save.
13. `tracks/production?tab=countryAvailability`: if 0 countries, "Add countries / regions"
    ≈(373,418), tick the header select-all ≈(93,393), Save → should read Targeted (178).

Do NOT create or send releases (a later step does that).
After each app, open `app-dashboard` and use get_page_text to confirm "Set up your app" shows
12 of 12 complete (fix anything missed).

Final report: table `slug | APPID | setup x/12 | countries | errors`.

**Do not spawn sub-agents or use the Agent/Task tool. Do all the work yourself.**

# Native review packet: Mosselora (ja)

**Status: Draft — not native-reviewed. Simulator screenshots only; physical-device and store-copy review are separate steps.**

- Public name: Mosselora (internal codename Evergrove; do not put reviewer identities in this repo)
- Locale: `ja` (日本語)
- Source commit: `d0f437fdf71adb48a24dd4c9a3cf1e9ab40ce0d5` (`git rev-parse HEAD`)
- Localization source and catalog at that commit vs working tree: clean for these files
- Generated: 2026-11-24 by `tools/export_locale_review.py`
- Keys: 101 (source table in `games/evergrove/Assets/Scripts/Core/GameLocalization.cs`; catalog `games/evergrove/Assets/Resources/Evergrove/Locales/ja.json` has 101 entries)
- Placeholder flags: 0

Other locale-specific copy, outside the catalog:

- Language names and right-to-left flags: `games/evergrove/Assets/Scripts/Core/GameLocalization.cs`
- Locale-to-font selection (uGUI fallback): `games/evergrove/Assets/Scripts/Unity/LocaleRuntime.cs`
- Shipping allowlist (English only until certified): `games/evergrove/Assets/Scripts/Core/LocaleShippingPolicy.cs`
- Village copy and NPC names (via catalog keys): `games/evergrove/Assets/Scripts/Core/LocalizedVillage.cs`
- Dev-only buttons (for example `Start matched control (dev)`) are English and not player-facing; they are not in the catalog.

Verdict: OK / Change / Question. Leave the verdict and note blank in this file; record reviews privately as described in the sign-off section.

| Key | English source | ja text | Placeholders | Verdict | Reviewer note |
| --- | --- | --- | --- | --- | --- |
| `title` | MOSSELORA | MOSSELORA | OK |  |  |
| `subtitle` | A living garden that remembers. | 小さな選択が、世界を育てる。 | OK |  |  |
| `language` | Language | 言語 | OK |  |  |
| `auto` | Automatic (device language) | 自動（端末の言語） | OK |  |  |
| `language_help` | The device language is used until you choose another. | 変更するまでは端末の言語を使用します。 | OK |  |  |
| `living` | Living world | 生きている世界 | OK |  |  |
| `puzzle` | Puzzle only | パズルのみ | OK |  |  |
| `puzzle_desc` | Same water puzzles, without the living-world layer. | 同じ水路パズルを、村の物語なしで遊びます。 | OK |  |  |
| `memory_title` | A NEW MEMORY IN THE GROVE | 村に新しい思い出 | OK |  |  |
| `continue` | Continue along the stream | 小川の先へ進む | OK |  |  |
| `change` | Change this choice | この選択を変える | OK |  |  |
| `complete` | Twelve streams completed. Which puzzle did you enjoy most? | 12本の小川を完成！ 一番好きなパズルは？ | OK |  |  |
| `explore` | Explore another garden | 別の庭を育てる | OK |  |  |
| `erase` | Confirm: erase this garden | 確認：この庭を消す | OK |  |  |
| `keep` | Keep this garden | この庭を残す | OK |  |  |
| `neighbors` | Talk to a neighbor | 村人と話す | OK |  |  |
| `thanks` | Thanks for telling me | 話してくれてありがとう！ | OK |  |  |
| `playtest` | PLAYTEST MODE · Choose before your first move | テストモード · 最初の操作前に選択してください | OK |  |  |
| `art_2d` | Visual: 2D storybook (tap for 2.5D) | 表示：2D絵本（タップで2.5Dへ） | OK |  |  |
| `art_3d` | Visual: 2.5D miniature (tap for 2D) | 表示：2.5Dジオラマ（タップで2Dへ） | OK |  |  |
| `motion_on` | Miniature motion: ON (tap to stop) | ジオラマの動き：オン（タップで停止） | OK |  |  |
| `motion_off` | Miniature motion: OFF (tap to animate) | ジオラマの動き：オフ（タップで開始） | OK |  |  |
| `sound_on` | Gentle chime: ON (tap to mute) | 優しい音：オン（タップで消音） | OK |  |  |
| `sound_off` | Gentle chime: OFF (tap to enable) | 優しい音：オフ（タップで有効） | OK |  |  |
| `record` | Start an optional private playtest | 任意のプライベートテストを開始 | OK |  |  |
| `privacy` | Only starts when you tap. Nothing is uploaded or linked to you. | タップした時だけ開始。送信や個人との紐付けはしません。 | OK |  |  |
| `copy` | Copy my local playtest notes | テストメモをコピー | OK |  |  |
| `copied` | Copied to clipboard. | クリップボードにコピーしました。 | OK |  |  |
| `clear` | Stop and erase local notes | テストを終了してメモを削除 | OK |  |  |
| `offline` | Offline · No timer · No ads · No account | オフライン · 時間制限なし · 広告なし · 登録不要 | OK |  |  |
| `journal_show` | Read village memories | 村の思い出を読む | OK |  |  |
| `journal_hide` | Hide village memories | 村の思い出を閉じる | OK |  |  |
| `journal_title` | OUR CHOICES | 私たちの選択 | OK |  |  |
| `journal_empty` | The first memory is still waiting. | 最初の思い出はこれから。 | OK |  |  |
| `stream` | STREAM {0} / {1} | 小川 {0} / {1} | OK |  |  |
| `first` | Blue waves = spring · green sprout = garden. Tap to connect water. | 青い波は水源、緑の芽は庭です。タップして水をつなごう。 | OK |  |  |
| `solved` | The water flows through every tile. | すべてのマスに水が流れました。 | OK |  |  |
| `progress` | Water reaches {0}/{1} tiles · {2} open joins | 水が届いたマス {0}/{1} · 未接続 {2} か所 | OK |  |  |
| `undo_last` | Undo the last water turn | 直前の回転を戻す | OK |  |  |
| `next` | Next stream | 次の小川へ | OK |  |  |
| `choose` | What will you help grow? | 何を育てたい？ | OK |  |  |
| `undo` | Undo | 元に戻す | OK |  |  |
| `hint` | Guide me | ヒント | OK |  |  |
| `restart` | Restart stream | この小川をやり直す | OK |  |  |
| `how` | Tap to turn a pipe. Circles mark open ends; guide water to the garden. | タップして回転。すべての水路を隙間なくつなげよう。 | OK |  |  |
| `tree` | Plant a shade tree | 木陰をつくる木を植える | OK |  |  |
| `bridge` | Restore a crossing | 橋を修復する | OK |  |  |
| `flowers` | Grow wildflowers | 野の花を育てる | OK |  |  |
| `memory_tree` | Mira remembers the tree you planted. | ミラはあなたが植えた木を覚えています。 | OK |  |  |
| `memory_bridge` | Orin remembers the bridge you restored. | オリンはあなたが直した橋を覚えています。 | OK |  |  |
| `memory_flowers` | Fen remembers the flowers you grew. | フェンはあなたが育てた花を覚えています。 | OK |  |  |
| `world_none` | The old creek waits for water. | 古い小川は水を待っています。 | OK |  |  |
| `world_tree` | A young tree grows by the stream. | 小川のそばに若い木が育ちます。 | OK |  |  |
| `world_bridge` | The crossing connects both banks. | 橋が両岸をつなぎます。 | OK |  |  |
| `world_flowers` | Wildflowers brighten the bank. | 野の花が岸を彩ります。 | OK |  |  |
| `world_tree_bridge` | Your trees shade the restored bridge. | あなたの木が修復した橋に木陰をつくります。 | OK |  |  |
| `world_tree_flowers` | Butterflies gather beneath the trees. | 蝶が木の下に集まります。 | OK |  |  |
| `world_bridge_flowers` | Neighbors cross to see the meadow. | 村人が花畑を見るために橋を渡ります。 | OK |  |  |
| `world_all` | Trees, paths and flowers bring the village together. | 木と道と花が村をひとつにします。 | OK |  |  |
| `mira_new` | I'm Mira. Could we plant a tree beside the water? | 私はミラ。水辺に木を植えてみない？ | OK |  |  |
| `mira_after` | I remember the trees you planted. The village has shade now. | 植えてくれた木を覚えているよ。村に木陰ができたね。 | OK |  |  |
| `orin_new` | I'm Orin. I hope we can rebuild the crossing. | 私はオリン。いつか橋を直せたらいいな。 | OK |  |  |
| `orin_after` | The restored bridge brings us together. Thank you! | 橋が直って、みんなが会えるようになった。ありがとう！ | OK |  |  |
| `fen_new` | I'm Fen. I miss seeing wildflowers along the water. | 私はフェン。川辺の野の花が恋しいな。 | OK |  |  |
| `fen_after` | Your flowers have brought life back to the bank. | あなたの花で岸に命が戻ってきたよ。 | OK |  |  |
| `ending_all` | Our village thrives. Every choice helped make a home. | 村は元気いっぱい。すべての選択が居場所をつくりました。 | OK |  |  |
| `ending_tree` | A quiet orchard grew from your care. | あなたの心づかいで静かな果樹園が育ちました。 | OK |  |  |
| `ending_bridge` | Your bridges brought neighbors together. | 橋が村人の心をつなぎました。 | OK |  |  |
| `ending_flowers` | You made a meadow where life returned. | 命が戻ってくる花畑をつくりました。 | OK |  |  |
| `prompt_early` | A little water can change a place. | 少しの水で、景色は変わる。 | OK |  |  |
| `prompt_mid` | The village is beginning to grow. | 村が少しずつ育っています。 | OK |  |  |
| `prompt_late` | Every choice becomes part of this place. | すべての選択がこの場所の一部になります。 | OK |  |  |
| `journal_entry` | Chapter {0} · {1} · {2} | 第{0}章 · {1} · {2} | OK |  |  |
| `landmark_tree` | Tree roots anchor this pipe. It cannot rotate. | 木の根がこの水路を固定しています。回転できません。 | OK |  |  |
| `landmark_bridge` | Bridge foundations anchor this pipe. It cannot rotate. | 橋の土台がこの水路を固定しています。回転できません。 | OK |  |  |
| `landmark_flowers` | Wildflowers protect this watercourse. It cannot rotate. | 野の花がこの場所を守っています。回転できません。 | OK |  |  |
| `hint_notice` | Notice row {0}. One channel needs attention. | {0}行目を見てください。つながりを確かめましょう。 | OK |  |  |
| `hint_focus` | Row {0}, column {1}: compare the open ends. | {0}行{1}列：水路の開いた端を比べてください。 | OK |  |  |
| `hint_more` | Show a closer clue | もう少しヒントを見る | OK |  |  |
| `hint_turn` | Fix one connection | 水路を1か所直す | OK |  |  |
| `save_warning` | Progress could not be saved. Check device storage. | 進行状況を保存できませんでした。端末の空き容量をご確認ください。 | OK |  |  |
| `save_unreadable` | We couldn't read your saved garden. It is kept safe. Choose Explore another garden to start fresh. | 保存された庭を読み込めませんでした。庭は安全に保管されています。「別の庭を育てる」を選ぶと、新しく始められます。 | OK |  |  |
| `ending_title` | YOUR GARDEN'S STORY | 庭の物語 | OK |  |  |
| `garden_copy` | Copy garden file | 庭のファイルをコピー | OK |  |  |
| `garden_copied` | Garden file copied. Keep it somewhere safe. | 庭のファイルをコピーしました。安全な場所に保管してください。 | OK |  |  |
| `garden_restore` | Restore from garden file | 庭のファイルから復元 | OK |  |  |
| `garden_replace` | This replaces your current garden. | 現在の庭が置き換わります。 | OK |  |  |
| `garden_err_empty` | Nothing is copied yet. Copy a garden file first, then try again. | まだ何もコピーされていません。先に庭のファイルをコピーしてから、もう一度お試しください。 | OK |  |  |
| `garden_err_not_garden` | This is not a Mosselora garden file. | Mosselora の庭のファイルではありません。 | OK |  |  |
| `garden_err_newer` | This garden file came from a newer version of Mosselora. Update the game to open it. | このファイルは新しいバージョンの Mosselora で作られています。開くにはゲームを更新してください。 | OK |  |  |
| `garden_err_damaged` | This garden file is damaged or incomplete. Your garden was not changed. | このファイルは破損しているか不完全です。庭は変更されていません。 | OK |  |  |
| `garden_err_too_large` | This garden file is too large to be a Mosselora garden. Your garden was not changed. | このファイルは大きすぎて Mosselora の庭にはできません。庭は変更されていません。 | OK |  |  |
| `sr_tile_wet` | Row {0}, column {1}: {2}, connected to water. | {0}行目・{1}列目：{2}、水とつながっています。 | OK |  |  |
| `sr_tile_dry` | Row {0}, column {1}: {2}, dry. | {0}行目・{1}列目：{2}、乾いています。 | OK |  |  |
| `sr_straight` | straight pipe | 直管 | OK |  |  |
| `sr_corner` | corner pipe | 曲がり管 | OK |  |  |
| `sr_spring` | water spring | 水源 | OK |  |  |
| `sr_outlet` | garden outlet | 庭の出口 | OK |  |  |
| `name_mira` | Mira | ミラ | OK |  |  |
| `name_orin` | Orin | オリン | OK |  |  |
| `name_fen` | Fen | フェン | OK |  |  |

## How to review

Follow the shared guidance in [README.md](README.md): how to review, what to judge and what not to do.

Simulator screenshots for this locale (context only, not device evidence):

- [01-chapter1-start](../ios-simulator-2026-11-23/locales/ja/01-chapter1-start.png)
- [03-chapter1-solved-choice](../ios-simulator-2026-11-23/locales/ja/03-chapter1-solved-choice.png)
- [04-consequence-shade-tree](../ios-simulator-2026-11-23/locales/ja/04-consequence-shade-tree.png)
- [10-journal-after-fix](../ios-simulator-2026-11-23/locales/ja/10-journal-after-fix.png)
- [10-journal-kinsoku-fixed](../ios-simulator-2026-11-23/locales/ja/10-journal-kinsoku-fixed.png)
- [10-journal](../ios-simulator-2026-11-23/locales/ja/10-journal.png)
- [11-ending](../ios-simulator-2026-11-23/locales/ja/11-ending.png)

## Sign-off block (leave blank in this repository)

| Field | Value |
| --- | --- |
| Locale | ja |
| Internal reviewer ID (private record kept outside the repo) | |
| Native reviewer qualification (summary, no contact details) | |
| Review date | |
| Source commit (from header) | |
| Build hash tested on device | |
| Screenshot set reviewed | |
| Placeholders reviewed (Y/N, notes) | |
| Glyph coverage (device, font) | |
| Bidi / Indic shaping (N/A where not applicable) | |
| Line breaks (notes) | |
| Compact layout (iPhone, Android, large text) | |
| Store copy reviewed (Y/N) | |
| Decision (Pass / Pass with changes / Fail / Deferred) | |
| Outstanding changes (verdict Change / Question rows) | |

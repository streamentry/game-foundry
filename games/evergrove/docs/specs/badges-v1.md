# Badges v1 (checklist 16)

Rule: a small dot or mark means "something new for you" and nothing else. No counts, no red,
and every badge clears when the player looks at what it points to.

The audit is `Tests/FirstMinutesAuditTests.cs` (`BadgeAudit`). It fails on a new badge-like node
name that is not listed here, on a missing clear path, on a count, and on a red colour.

| ID | Badge (node) | Where | Shows | Triggered by | Cleared by | Colour |
|----|--------------|-------|-------|--------------|------------|--------|
| B1 | "Has a new memory" dot on the book | Memories token in the menu fan (`MarkUnreadMemory`) | dot, no number | a memory added since the book was last opened | opening the book (`MemoryJournal` writes `journal-seen`) | paper with sprig, no red |
| B1b | "Has a new memory" dot on the menu button | menu button, hub and garden chrome (`MarkUnreadMemory`, `MenuSeenKey`) | dot, no number | a memory added since the menu was last opened | opening the menu (`ToggleFan` writes `menu-seen` and removes the dot) | paper with sprig, no red |
| B2 | "New to your almanac" disc | nursery token (`NewToAlmanac`) | disc with book icon, no number | the plant is not yet in the almanac | the plant enters the almanac (`RecordInAlmanac`, `almanac.Add`) | cream with leaf-green icon |
| B3 | "Has something new to say" speech mark | neighbour portrait on the hub (`PortraitButton`) | speech icon, no number | the neighbour's line differs from the last one heard | talking to them (`OpenConversation` writes `heard-`) | paper with water-channel icon |
| B4 | "Unread letter" envelope | letterbox on the garden (`LetterBox`) | sealed envelope, no number | a letter has not been opened | opening the letter (`ShowLetter` writes `letters-read`) | painted art |
| B5 | "Ribbon" on a relative's cutting | basket gift token (`GiftTokens`) | small ribbon, no number | a cutting sent by family is waiting to be planted | planting it (`WorldModel.PlantGift` removes it from the gifts) | honey (was red, changed in I16 badge pass) |

## Removed in this pass

- Hint level dots (`Level dots`, previously on the Guide button): removed. The button's words
  already say the step ("Guide me", "Show a closer clue", "Fix one connection").

## Not badges (documented exclusions)

| Node | Why it is not a badge |
|------|-----------------------|
| Settings values "Text size 150%" and "Board size 1.2x" | setting values, not alerts |
| Seed and fruit counts in the purse | inventory, not a notification |
| Family sheet step dots ("Step 1 of 3") | pagination inside a sheet |
| "Saved" confirmation mark | transient save confirmation, fades after about 1.2 s |
| Icons inside badges ("Sprig mark", "Speech mark", "Almanac mark", "Sealed letter") | parts of B1, B2, B3 and B4 |
| "Coach disc" | Mira's portrait |

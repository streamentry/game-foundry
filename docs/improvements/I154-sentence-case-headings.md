# I154 — Story headings read as words, not capitals

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player

The three moments the game is about are headed in small all-caps green: the memory card ("A NEW MEMORY IN THE GROVE"), the book ("OUR CHOICES") and the ending ("YOUR GARDEN'S STORY"), each at 15pt. All caps removes word shapes (ascenders and descenders), so a line set in capitals reads more slowly, especially for older readers and at small sizes. The look is also a website's section label more than a storybook's chapter head.

## PM decision

Set the three headings in **sentence case at 18pt bold** (same green, same place): "A new memory in the grove", "Our choices", "Your garden's story". The wordmark stays as art.

## Dev

- `GameLocalization` English source: the three strings in sentence case (29 draft catalogues untouched; none ship).
- `EvergroveApp`: `memory_title`, `journal_title` and `ending_title` labels go from 15 to 18pt.

## Result (b162, iPhone 17 simulator, scripted tours)

`I154-sentence-case.jpg` (gitignored): the memory card before and after, the journal and the ending. Each heading is larger and reads as words, not a label. The ending also shows I149's "Send a garden postcard". All 20 portrait and 5 landscape screens are captured. Verify and the .NET tests pass.

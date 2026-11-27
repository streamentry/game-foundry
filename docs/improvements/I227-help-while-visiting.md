# I227 — Help while visiting

## Player
- Visiting Nan's garden, I can tap one of her things that still holds a puzzle
  and solve it for her: the same board she would get. It blooms in her garden
  in front of me. My phone's share sheet opens so I can send her the help.
- At home, opening help from Nan shows "Nan solved a puzzle in your garden for
  you. Look for the bloom." The thing wears a bloom ring. Orin says "Nan was
  here and mended one of the old paintings with you in mind."

## PM decision
- On a visit, their unsolved things show the small play mark (I221). Solved or
  already-helped things don't.
- The card is the same puzzle card with no seed pill. Help pays nothing, and the
  screen reader says "Help with it for them".
- The parcel is signed with your family name, or "Family" if you haven't set
  one.
- At home, a helped thing blooms but keeps its play mark: your own first solve
  and its seeds are still yours.

## Dev
- `EvergroveApp`:
  - `PuzzleCard` now opens the card for any host and art, split from
    `OpenThingPuzzle`'s own-garden bookkeeping.
  - `HelpWith(item)` plays their board (seed `garden * 1000 + item * 7 + 1`),
    then `SendParcel(Help)`, `FamilyParcel.Wrap` and a native share. The visit
    rebuilds with the thing blooming.
  - `PairsPictures` takes the garden's art.
- `TreePlay` hides the seed pill for help. `FamilySheet` gets the help outcome
  and the error wording. The play mark now follows "not yet solved by you",
  not "not bloomed".
- Tour: `17s2-helping`, `17s3-helped-their-garden`, `17s4-help-opened`,
  `17z-orin-help`.

## Known limits
- Like other parcels, help isn't addressed to one garden. A Help parcel opened
  in another relative's garden marks their thing at that index. It's harmless
  (no seeds, no solve), and the family-circle server can address it later.
- Not yet tried on two real phones.

## Result
- b271, all three tours complete, 52 shots each.
  - `17s2`: Nan's heather holds a Pairs table with no seed pill.
  - `17s3`: after the solve, the visit is framed on the heather in its gold ring
    (b269 framed their newest thing instead; fixed).
  - `17s4`: the help-opened card.
  - `17z`: Orin names Nan.
- Found on the 150% tour and fixed: the seed purse wrapped "100" onto two lines
  ("10" / "0"). It now sizes to its count, on one line, growing leftward.
- Core tests: 71,748 assertions pass.

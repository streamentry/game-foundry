# I230 — Send family a basket of fruit

## Player
- I want to send my grandson some of the pears I picked.

## PM decision
- Family's send view offers a basket, up to five fruit, at the start of the
  second row, before cuttings. It appears only when you have fruit. One tap
  picks it, and its chip shows how many go.
- The friendly line: "Lan sent you a basket of 5 fruit from their trees in
  Mosselora." On opening: "5 fruit, picked from their trees, are in your
  basket." A full basket is said plainly. Too little fruit: "Tend your trees to
  pick more."

## Dev
- `FamilySheet`:
  - `Hooks.Fruit`;
  - pick code `FruitBase` (2000 + count);
  - `ParcelGift.Fruit` (I228 rules);
  - wording for send, open and `NotEnoughFruit`.

## Result
- b272 `17p`: the basket token opens the second row, with chip "2", before the
  cutting.

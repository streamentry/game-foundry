# I216 — Visit a relative's garden

## Player
- The founder's goal is family members who live apart visiting each other's
  gardens. Restoring someone's garden file replaces your own, which is the
  opposite of a visit.

## PM decision
- In the Family sheet, a house token: copy their garden file (on their phone:
  Menu → Settings → Copy garden file, sent to you any way you like), tap the
  house, and you walk their river: their season, their things where they put
  them, their basket plantings and nursery plants, the golden rings under
  what they've solved. Read-only: nothing of theirs or yours changes.
- A wooden home token where the menu button lives brings you back; a small
  sign says "Visiting a garden". Pinch and pan work as at home.
- If the clipboard holds no garden file, the sheet says how to get one, in
  plain words.

## Dev
- `EvergroveApp.OpenVisit/VisitItems/CloseVisit`: `GardenTransfer.TryImport`
  into a separate `WorldModel` (never assigned to yours), its own river map
  and season painting, `GardenMapView` with no tap handlers, catalogue art from
  their garden number.
- `FamilySheet`: Visit token (house) and `DoVisit`. Top row trimmed to fit six
  items; the seed pill moved beside "Pick one gift". Sideways the whole card
  now shrinks to fit the screen height (never below 75%): the send token had
  been cut off at the bottom in the b254 sideways tour.
- Tour: builds a twelve-stream garden with nine plantings, copies its garden
  file and visits it.

## Result
- `I216-visit.jpg` (local only): visiting, 150%, sideways (b260); tours 44 shots each, complete.
- Not tested between two real phones yet.

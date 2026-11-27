# Colour cues v1 (checklist 92, colour-blind pass)

Rule: every state that a colour shows also has a shape, pattern, icon, mark or text cue, so
deuteranopia and protanopia players can read it without hue. Opacity or luminance alone is not
counted as a cue; a dark-versus-paper fill is a contrast difference, not a state marker.

Guarded by `games/evergrove/Tests/ColourCueTests.cs` (source-level: each site must still exist and
still call its cue in the same method; each settings token must change its icon or carry `stateOn`).
Output: `Colour cues: 34 sites, all paired`.

## Added in this pass

| Cue | Where (file:symbol) | Colour difference | Paired non-colour cue |
| --- | --- | --- | --- |
| Show-clashes lens on/off | BoardKit.cs:ClashLens, ToggleClashes | Water teal vs forest green | Check mark (`StateMark`) when on |
| Haptics on/off | EvergroveApp.cs:IconButton call "Haptics setting" | Forest vs paper | Check when on (same icon both states) |
| Day light on/off | EvergroveApp.cs "Day light setting" | Forest vs paper | Check when on (same sun icon) |
| Garden notes on/off | EvergroveApp.cs "Garden notes setting" | Forest vs paper | Check when on (same letter icon) |
| Speak less on/off | EvergroveApp.Quiet.cs:SpeakLessButton | Forest vs paper | Check when on (same chat icon) |
| Leaves level chosen | EvergroveApp.Difficulty.cs:AddLevelButton | Forest vs paper | Check when chosen |
| Stream hint cell | StreamBoard.cs:Hint, Paint | Honey vs soil | Focus ring (`MarkRing`) |
| Sums selected and hinted cell | SumsBoard.cs:Paint | Honey vs soil | Focus ring |
| Sudoku selected cell | SudokuBoard.cs:Paint | Honey vs soil | Focus ring (also keeps the 1.05 scale) |
| Sudoku given vs entered | SudokuBoard.cs:Paint | Tan vs green | Small mark, top-left, on givens |
| Nonogram hinted cell | NonogramBoard.cs:Hint, Paint | Honey vs soil | Focus ring |
| Mahjong selected tile | MahjongBoard.cs:Paint | Honey vs soil | Focus ring |
| Mahjong hinted pair | MahjongBoard.cs:Hint | Honey vs soil | Focus ring |
| Plots corner pick | PlotsBoard.cs:Paint | Honey vs soil | Focus ring |
| Plots bed tones | PlotsBoard.cs:PaintBedPictures | Six pastel tones | Per-bed picture (sprout, droplet, sprig, waves, wildflowers, tree) on first cell |
| Plots hinted bed | PlotsBoard.cs:Hint | Honey vs soil | Focus ring |
| Sow shown bed | SowBoard.cs:Paint | Honey vs soil | Focus ring |
| Sow ready bed | SowBoard.cs:Paint | Leaf green vs soil | Check when ready |
| Doorstep shown cell | DoorstepBoard.cs:Paint | Honey vs soil | Focus ring |
| Garden Pieces snug fit | PiecesBoard.cs:Paint | Pale green vs soil | Fit ring (`FitMarkName`) |
| Garden Pieces placed piece | PiecesBoard.cs:Paint, PieceTag | Owner colour | Piece number on its first cell |
| Garden Pieces chosen tray piece | PiecesBoard.cs:Paint | Honey vs paper | Focus ring on the tray slot |
| Pairs matched card | TreePlay.cs:PaintPairs | Mint vs cream | Check on the card |
| Pairs hinted pair | TreePlay.cs:Hint | Honey vs sage | Focus ring |
| Mosaic picked tile | TreePlay.cs:PaintMosaic | Teal vs paper | Focus ring (also keeps 1.06 scale) |
| Mosaic tile in place | TreePlay.cs:PaintMosaic | Honey vs paper | Check on the tile |

Shared helpers: `MosseloraArt.StateMark` (check or given sprite, top corner, ink flips on dark
tokens), `BoardKit.MarkRing` (full-cell BoldRing), `IconButton(..., stateOn:)`.

## Already paired before this pass

| Cue | Where | Colour difference | Paired non-colour cue |
| --- | --- | --- | --- |
| Water flowing vs dry pipe | PipeGraphic.cs:OnPopulateMesh | Teal vs brown | Solid stripe vs three-segment dashes (`AddDashes`) |
| Conflict (sums, sudoku, doorstep) | BoardKit.cs:MarkClash | Sand tile | Earth-ink ring in corner |
| Nonogram filled / marked | NonogramBoard.cs:Paint | Leaf vs soil | Sprout glyph / cross glyph |
| Pairs turn disc (I268) | TreePlay.cs:PaintTurn | Four turn colours | Icon flips for two players; position pips |
| Gentle motion, quiet audio | EvergroveApp.cs settings | Forest vs paper | Icon swap (motion on/off, chime on/off) |
| Undo / redo off | BoardKit.cs:UndoRow.Style | Faded | Button not interactable; spoken "nothing to undo"; count text |
| Board size, text size | EvergroveApp.cs | Forest vs paper | Level pips and value text (1.2x, 125%) |
| Good-neighbour sprigs | GardenMapView.cs (pairLinks) | One green for all pairs | Sprig shape; no per-pairing colour |
| Pairs face up / down | TreePlay.cs:PaintPairs | Card fill | Picture shown or sage back |
| Doorstep drawn petals | DoorstepBoard.cs:Paint | Ground | Petal glyph |
| Stream path cells | StreamBoard.cs:Paint | Teal vs soil | Water bars along the path |
| Economy delta | EvergroveApp.Economy.cs | Forest vs ink | Sign (+ / -) in text |
| Season and day | GardenMapView.cs | Grade tint | Season name in text |

## Not changed in this pass (follow-up)

- Tint-only selection tokens with a text or icon label but no check: tab buttons (almanac, ways,
  family, pages), nursery and gift pickers, basket toggle, family sheet send/choose, first-size
  chooser, main menu open state. Each is dark-on-paper, so the difference is luminance, not hue.
  Not counted as colour-only, but not yet paired with a mark.
- Mahjong blocked tiles dim by picture opacity (0.75) and a grey fill. Opacity is not hue; the tile
  is also not tappable.
- Garden map seasonal grading and lantern glow are atmosphere; the state they describe is in text.

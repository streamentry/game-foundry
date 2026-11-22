# Game Foundry: ten prototype experiments, 23–32

**Decision timestamp:** 2026-11-22. **Authority:** user-requested isolated playable engineering proofs, **not** mass production, App Store submissions or ASO market selection.

| Game and search hypothesis | Precisely testable mechanics | Why it might fail |
| --- | --- | --- |
| [Renzoku Meadow](../../games/renzoku-meadow/README.md) (GF-025, consecutive Latin squares) | 4×4 full Renzoku: fill 1–4 once per row/column, with marked consecutive edges and unmarked nonconsecutive edges. | Full negative-edge rule may surprise users familiar only with positive-dot Kropki, which is already another studio prototype. |
| [Word Ladder Lane](../../games/word-ladder-lane/README.md) (GF-031, four-letter word transformations) | Change exactly one letter at a time, using only available word cards, to reach a given goal in an exact shortest move budget. | Limited card dictionaries lower vocabulary burden but may feel too guided; free-entry word ladders have different appeal. |
| [Word Square Studio](../../games/word-square-studio/README.md) (GF-038, symmetric word squares) | Arrange three 3-letter English words so each vertical column matches the corresponding horizontal row. | Three-word puzzles may be too trivial for practiced word-game players without adding larger verified word squares. |
| [Vowel Garden](../../games/vowel-garden/README.md) (GF-043, missing-vowel phrase restoration) | Reconstruct original short phrases by inserting A/E/I/O/U into a visible consonant skeleton using topic clues. | Consonant-only prompts can admit multiple sensible phrases; a single canonical answer is potentially unfair without broader acceptance. |
| [Word Search Shores](../../games/word-search-shores/README.md) (GF-048, offline word search) | Find three target words in a 6×6 original letter grid by tapping their start and end along a straight horizontal, vertical or diagonal line. | Direct competitors have much more content; requiring two endpoint taps may be less natural than a continuous swipe for some players. |
| [Nonogram Canvas](../../games/nonogram-canvas/README.md) (GF-056, picture cross / Nonogram) | Fill a 5×5 image using exact row/column runs separated by at least one empty cell. | A 5×5 tutorial can demonstrate correctness but may provide little replay value, and clue headers may crowd narrow screens. |
| [Akari Evening](../../games/akari-evening/README.md) (GF-062, Light Up / Akari) | Place lamps that illuminate all open cells without any pair seeing each other, while honoring numbered wall adjacency. | Small boards and colored light feedback may be aesthetically appealing but insufficient to teach illumination to first-time players. |
| [Pipe Garden](../../games/pipe-garden/README.md) (GF-064, pipe rotation) | Rotate nine pipes on a 3×3 board into a single continuous, leak-free route from one endpoint to the other. | Rotating small pipes can become mechanical trial-and-error rather than strategic play, and 3×3 grids are beginner-only. |
| [Sliding Tiles](../../games/sliding-tiles/README.md) (GF-068, 8-puzzle sliding blocks) | Slide numbered tiles orthogonally into the empty square to reconstruct the ordered 1–8 board. | The 8-puzzle is extremely commoditized; deterministic reachability is engineering hygiene, not a commercial differentiator. |
| [Number Pyramid](../../games/number-pyramid/README.md) (GF-079, number sum pyramid) | Reconstruct four bottom digits 1–9 from scattered clues; each of the six upper numbers equals the sum of the two directly below. | Without scaffolded deduction explanations, this may feel like arithmetic guesswork rather than an enjoyable logic game. |

## Evidence we have versus what we do not

All ten projects are independent Godot 4.7.2/GDScript applications. Every game includes six original small exercises, a pure rule engine, a real interactive scene, Undo/Restart, offline versioned JSON progress, and automated rule, persistence and scene tests. Individual WebGL2 workflows build a ZIP and verify the exported HTML/JS/WASM/PCK assets over local HTTP, **not** public hosting or a physical browser/device audit.

**Scope of correctness proofs matters:**
- **Renzoku:** 4×4 full negative-edge convention; unmarked adjacent cells must **not** be consecutive, unlike Kropki Pairs' positive-dot-only beginner variant. Exact Latin rows/columns and all 24 neighbor edges are checked.
- **Word Ladder:** a single shortest route **within each small displayed word-card lexicon and prescribed step budget**. The entire English vocabulary is **not** modeled and there is no claim of one unique natural-language ladder.
- **Word Squares:** each three-word card set has one valid arrangement with row strings equal to column strings. Three-letter boards are tutorial-scale.
- **Vowel Garden:** one **authored canonical phrase** per puzzle; consonant skeletons may admit other valid English completions. This limitation is visible in the game; do not claim linguistic uniqueness or benchmark raw error rate without accounting for alternatives.
- **Word Search:** each target word has exactly one accepted straight-line placement on the generated 6×6 board; finding words in any order is not a unique solution sequence.
- **Nonogram:** 5×5 run clues uniquely determine a fully labeled filled/empty grid for each original picture.
- **Akari:** 4×4 opaque, numbered walls; lamp positions alone determine one correct all-lit configuration with no line-of-sight collision. Players can leave optional X notes blank.
- **Pipe Garden:** 3×3 single continuous route with unique **port configurations** given each immutable tile type, not necessarily unique aesthetic rotation of a symmetric straight pipe.
- **Sliding Tiles:** six reachable 8-puzzle scrambles with **shortest path depths of 4/6/8/10/12/14**. Multiple solution move sequences may exist; shortest distance, not puzzle uniqueness, is the tested criterion.
- **Number Pyramid:** four bottom values 1–9 and six derived sums, with uniquely determined base values from each shown clue set.

**Not measured:** official US/non-US Apple Ads non-branded keyword popularity, age-45+ usability, actual competitor advantage, voluntary next-day return, adoption, distribution cost, payment, profitability, or real iOS/Android screen-reader experience. The existing Kakuro conditional proof-of-play selection and ASO gates remain unchanged. Avoid Apple App Review 4.3 risk from many thin, near-identical store submissions.

## Cheapest falsification, beyond engineering

Recruit a small representative mix of adults 45–54, 55–64 and 65+ when feasible, with varying genre familiarity. Show a fresh player **one** of the prototypes and a direct category incumbent in counterbalanced order. Record unaided first valid action, demonstrated rule understanding, perceived effort without asking for positive feedback, whether they *voluntarily* play additional levels, and unprompted next-day return. Capture denominator, device, version, and whether they already knew the mechanic. Do not count inferred interest from choice of download or from a successful solver.

**Proposed pilot gate, not market benchmarks:** on a short 6–10 participant formative test, only fund deeper content if at least three people independently choose an extra board and repeatable comparative advantage over a mature incumbent is observed. Permit one targeted legibility/feedback revision; then park the mechanic if understanding or preference still fails.

**Primary risk and next action:** with 32 prototypes, the dominant uncertainty is real distribution and voluntary retention, not engineering ability. The next concrete highest-value act is permissioned public HTTPS deployment of a focused comparison experiment followed by observed player sessions. No automatic hosting, user recruitment, analytics collection, advertising purchases, or app-store launch is authorized by this implementation brief.

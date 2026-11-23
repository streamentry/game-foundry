# BidiSharp (vendored)

- Upstream: https://github.com/fsufyan/BidiSharp (official repository, MIT)
- Commit: `4c40a8c857dceab060b660e03bb1d3c14ee9086e` (HEAD at download)
- Downloaded: 2026-11-23, from the GitHub source archive for that commit, into `/Volumes/SSD/mosselora-builds/rtl-dl/`
- Algorithm: Unicode Bidirectional Algorithm (UAX #9). Upstream states Unicode 6.3 (tr9-28). Characters added after 6.3 are not classified; the Arabic and Hebrew blocks used by the catalogs are covered.
- License: MIT, see `LICENSE` in this folder (Copyright (c) 2019 Fayyad Sufyan).

## Vendored files and SHA-256

| File | SHA-256 (vendored) | SHA-256 (upstream) |
| --- | --- | --- |
| `Bidi.cs` | `286af7578b183c5f1099b69efdd2884a430026313fd25221b74d8650732c8d62` | `3a13d19803c0d8117e219ba3127bdd25cb7e2a740d6a1ae37120912a7d52cc96` |
| `BidiClass.cs` | `beda2bab7486f9e7390369a3feab4b0b2a18ccf66370f9b590a8a806d7f8bf95` | same |
| `Bidi_types.cs` | `392d3179d6e89718398cabf07095ad34141bea2a75aeba5a83a88e2109ae4761` | same |
| `LICENSE` | `ad0ec91fb653ec3d237e2af7774e2938e378d6379ffc013ee42be58b85937805` | (upstream LICENSE) |

Not vendored: the upstream test project, the solution file, `UnicodeData.txt` and the NuGet packaging files.

## Local patch (`Bidi.cs`, marked `EVERGROVE PATCH`)

1. `LogicalToVisual` takes an optional `forcedParagraphLevel` (0 or 1). The app passes 1 for ar, fa, ur and he so that a line starting with Latin text still reads right to left.
2. Brackets and angle quotes (`()[]{}<>` and the guillemets and single guillemets) are mirrored when their resolved level is odd (UAX #9 rule L4). Upstream reordered text without mirroring.

Known upstream issues left as they are (not reached by app input, which contains no isolate controls and no trailing whitespace on the reordered line): `GetTextLevels` tests `FSI` twice where `PDI` is meant, and rule L1 does not reset trailing whitespace on a line.

## Use in Evergrove

- `Assets/Scripts/Core/RtlText.cs` calls `Bidi.LogicalToVisual` after `ArabicShaper.Shape`.
- Tests: `games/evergrove/Tests/Program.cs` (`RtlTextTests`); the test project compiles these files through `Tests/Evergrove.Core.Tests.csproj`.
- Shipping status: not on the shipping allowlist. Research and development builds only.

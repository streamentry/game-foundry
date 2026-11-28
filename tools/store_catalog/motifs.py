"""Per-game icon motifs: a tiny grid drawn from each game's own rules.

Grid characters: '.' plain tile, '_' outlined tile, '#' accent tile,
'=' inset tile, 'o' dot, 'x' cross, '/' clue tile, ' ' gap,
digits/upper-case letters on a light tile, lower-case letters on an accent tile.
'circles': digits draw as circles (island puzzles). 'paths': cell polylines.
"""

CREAM = "#FBF6EA"


def _m(bg: tuple[str, str], accent: str, grid: list[str], **extra: object) -> dict:
    return {"bg": bg, "fg": CREAM, "accent": accent, "grid": grid, **extra}


MOTIFS: dict[str, dict] = {
    "akari-evening": _m(("#1F2350", "#11132E"), "#F4B740", ["#..o", ".o.#", "..#.", "o2.."]),
    "battleship-harbor": _m(("#0F3B5F", "#07233A"), "#E5654B", ["##._", "...#", "_..#", "#_.#"]),
    "bridge-isles": _m(("#0E5A5C", "#073638"), "#F2C14E", ["2  3", "    ", "    ", "1  2"], circles=True,
                       paths=[[(0, 0), (0, 3)], [(0, 3), (3, 3)], [(3, 0), (3, 3)]], path_width=0.07, path_color=CREAM),
    "cage-hearth": _m(("#7A2E1F", "#4A190F"), "#F2A65A", ["3_", "#1"], ink="#4A190F"),
    "futoshiki-terrace": _m(("#4B5320", "#2C3112"), "#D9C25C", ["2>1", "^ v", "3<4"]),
    "good-eye": _m(("#2D4A8A", "#182A52"), "#F06A5A", ["...", ".o.", "..."]),
    "hidato-walk": _m(("#5A3E8C", "#352257"), "#F5B85C", ["1..", "...", "..9"],
                      paths=[[(0, 0), (0, 1), (1, 0), (2, 0), (2, 1), (1, 1), (0, 2), (1, 2), (2, 2)]],
                      path_width=0.09),
    "hitori-grove": _m(("#245C3A", "#123521"), "#C9E265", ["3#1", "1.#", "#23"]),
    "kakuro-compass": _m(("#1D3E6E", "#0F2342"), "#4F8BD6", ["//", "/7"], ink="#0F2342"),
    "kropki-pairs": _m(("#3B3B5C", "#202036"), "#F4D35E", ["1:2", ": :", "2:4"]),
    "kuromasu-windows": _m(("#6B2D5C", "#3E1735"), "#F3A6C8", ["#.3", "..#", "4.."]),
    "loop-lantern": _m(("#3D2A6B", "#211542"), "#F7C548", ["   ", "   ", "   "],
                       paths=[[(0, 0), (0, 2), (2, 2), (2, 1), (1, 1), (1, 0), (0, 0)]], path_width=0.12),
    "nonogram-canvas": _m(("#2F3E46", "#18242A"), "#F08A5D", ["#..#", "####", ".##.", "#..#"]),
    "number-pyramid": _m(("#8A4B1E", "#552B0D"), "#F2B880", ["  9  ", " 4 5 ", "1 3 2"]),
    "numberlink-paths": _m(("#205B6E", "#0F3542"), "#F28F3B", ["1..2", "....", "....", "2..1"],
                           paths=[[(0, 0), (1, 0), (2, 1), (2, 2), (3, 3)], [(0, 3), (1, 3), (1, 2), (3, 1), (3, 0)]],
                           path_width=0.11),
    "nurikabe-bay": _m(("#145374", "#0A2F44"), "#4FB0C6", ["3.#", "##.", "#2#"]),
    "odd-bloom": _m(("#7B2D4F", "#47162D"), "#F7D08A", ["oo", "o#"]),
    "one-path": _m(("#26474E", "#11272B"), "#F4A259", ["....", "....", "....", "...."],
                   paths=[[(0, 0), (0, 3), (1, 3), (1, 1), (2, 1), (2, 3), (3, 3), (3, 0)]], path_width=0.12),
    "pipe-garden": _m(("#2E5E3E", "#163522"), "#7BC8A4", ["   ", "   ", "   "],
                      paths=[[(0, 0), (0, 1), (1, 1), (1, 2), (2, 2)], [(2, 0), (1, 0), (1, 1)]], path_width=0.2),
    "quiet-camp": _m(("#3F5A2C", "#223317"), "#B5D96A", ["#.o", ".._", "o#."]),
    "quiet-shot": _m(("#323F5C", "#1A2236"), "#EF8354", ["._.", "_o_", "._."]),
    "renzoku-meadow": _m(("#5E6B1F", "#363F0C"), "#E8D35A", ["3|4", "  |", "1 3"]),
    "ripple-rooms": _m(("#1F4E79", "#0F2C47"), "#8FD3F4", ["12", "#1"]),
    "shikaku-garden": _m(("#4C6B2F", "#2A3E17"), "#E3C567", ["##.", "##.", "4.3"]),
    "skyline-steps": _m(("#2B2D42", "#15162A"), "#EF8354", [" 3 ", "#.#", "##."]),
    "sliding-tiles": _m(("#5C4B3B", "#36291D"), "#E9B872", ["12", "3 "]),
    "star-atlas": _m(("#141B41", "#080C26"), "#F9E07F", ["_*_", "__*", "*__"]),
    "takuzu-lab": _m(("#2C4251", "#16242E"), "#E07A5F", ["o=o", "=o=", "o=o"]),
    "thermometer-studio": _m(("#8C2F39", "#55161E"), "#F4ACB7", [". .", ". .", "o o"],
                             paths=[[(2, 0), (0, 0)], [(2, 2), (1, 2)]], path_width=0.26),
    "vowel-garden": _m(("#2F6F4F", "#16412B"), "#F5C26B", ["Ga", "Rd"]),
    "word-ladder-lane": _m(("#6A3E1F", "#3E210D"), "#F4B860", ["COLD", "COrD", "wORD"]),
    "word-search-shores": _m(("#1C5D73", "#0C3646"), "#F6C177", ["sEA", "WaV", "TRe"]),
    "word-square-studio": _m(("#5B2A86", "#331452"), "#F5B700", ["ART", "RUE", "TEN"]),
}

# Star glyph is not in the grid alphabet: draw stars as accent dots.
MOTIFS["star-atlas"]["grid"] = ["_o_", "__o", "o__"]

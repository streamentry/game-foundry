#!/usr/bin/env python3
"""Independently prove the reference 4x4 puzzle has exactly one solution."""
from __future__ import annotations

from itertools import product

START = [
    [0, 0, 1, -1],
    [0, -1, -1, -1],
    [-1, -1, 1, -1],
    [-1, -1, -1, -1],
]


def valid_line(values: tuple[int, ...]) -> bool:
    return (
        values.count(0) == 2
        and values.count(1) == 2
        and all(len(set(values[i : i + 3])) != 1 for i in (0, 1))
    )


def solutions() -> list[tuple[tuple[int, ...], ...]]:
    options = [line for line in product((0, 1), repeat=4) if valid_line(line)]
    result = []
    for board in product(options, repeat=4):
        if len(set(board)) != 4:
            continue
        columns = list(zip(*board))
        if len(set(columns)) != 4 or not all(valid_line(c) for c in columns):
            continue
        result.append(board)
    return result


def matching(boards: list[tuple[tuple[int, ...], ...]]) -> list[tuple[tuple[int, ...], ...]]:
    return [
        board for board in boards
        if all(START[r][c] in (-1, board[r][c]) for r in range(4) for c in range(4))
    ]


def main() -> int:
    all_boards = solutions()
    matched = matching(all_boards)
    assert len(all_boards) == 72, f"Unexpected solution space: {len(all_boards)}"
    assert len(matched) == 1, f"Reference puzzle has {len(matched)} solutions, expected one"
    print("PASS: 72 complete valid 4x4 boards; supplied five-clue puzzle has exactly one solution")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

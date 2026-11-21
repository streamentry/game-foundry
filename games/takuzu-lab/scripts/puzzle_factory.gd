extends RefCounted
## Deterministic offline Takuzu puzzles. No assets, SDKs, or network.
## Every returned puzzle is verified to have exactly ONE solution.
## This is an engineering reference, not a claim that level difficulty is human-rated.

const Rules = preload("res://scripts/binary_rules.gd")
const SUPPORTED_SIZES: Array = [4, 6]


static func make(seed_value: int, size: int = 4) -> Dictionary:
    if not SUPPORTED_SIZES.has(size) or seed_value < 0:
        return {}
    var rng := RandomNumberGenerator.new()
    rng.seed = seed_value
    var solution: Array = _generate_solution(size, rng)
    if solution.is_empty():
        return {}
    var puzzle: Array = solution.duplicate(true)
    var positions: Array = []
    for position in range(size * size):
        positions.append(position)
    _shuffle(positions, rng)

    # Lower clue count is NOT a validated human difficulty score.
    var minimum_clues: int = 5 if size == 4 else 13
    var clues_left: int = size * size
    for position in positions:
        if clues_left <= minimum_clues:
            break
        var row: int = position / size
        var col: int = position % size
        var previous: int = puzzle[row][col]
        puzzle[row][col] = -1
        if count_solutions(puzzle, 2) == 1:
            clues_left -= 1
        else:
            puzzle[row][col] = previous

    if not Rules.is_valid(puzzle) or count_solutions(puzzle, 2) != 1:
        return {}
    return {
        "seed": seed_value,
        "size": size,
        "givens": puzzle,
        "solution": solution,
        "given_count": clues_left,
    }


static func for_day(day: String, size: int = 4) -> Dictionary:
    # Deterministic integer seed within a pinned Godot engine version.
    if day.length() != 10 or day.substr(4, 1) != "-" or day.substr(7, 1) != "-":
        return {}
    var parts: PackedStringArray = day.split("-")
    if parts.size() != 3 or not parts[0].is_valid_int() or not parts[1].is_valid_int() or not parts[2].is_valid_int():
        return {}
    var year: int = int(parts[0])
    var month: int = int(parts[1])
    var date: int = int(parts[2])
    if year < 2000 or year > 9999 or month < 1 or month > 12 or date < 1 or date > 31:
        return {}
    return make((year * 10000 + month * 100 + date) * 31 + size, size)


static func count_solutions(board: Array, limit: int = 2) -> int:
    var size: int = board.size()
    if not SUPPORTED_SIZES.has(size) or not Rules.is_valid(board):
        return 0
    var patterns: Array = _legal_rows(size)
    var choices: Array = []
    for row in board:
        var allowed: Array = []
        for pattern in patterns:
            var matches: bool = true
            for c in range(size):
                if row[c] != -1 and row[c] != pattern[c]:
                    matches = false
                    break
            if matches:
                allowed.append(pattern)
        if allowed.is_empty():
            return 0
        choices.append(allowed)
    return _count(choices, [], {}, size, maxi(1, limit))


static func _count(choices: Array, placed: Array, used: Dictionary, size: int, remaining: int) -> int:
    if placed.size() == size:
        return 1 if Rules.is_solved(placed) else 0
    var total: int = 0
    for pattern in choices[placed.size()]:
        var key: String = str(pattern)
        if used.has(key) or not _column_prefix_ok(placed, pattern, size):
            continue
        placed.append(pattern)
        used[key] = true
        total += _count(choices, placed, used, size, remaining - total)
        used.erase(key)
        placed.pop_back()
        if total >= remaining:
            return remaining
    return total


static func _generate_solution(size: int, rng: RandomNumberGenerator) -> Array:
    var legal: Array = _legal_rows(size)
    var choices: Array = []
    for r in range(size):
        var shuffled: Array = legal.duplicate(true)
        _shuffle(shuffled, rng)
        choices.append(shuffled)
    return _find(choices, [], {}, size)


static func _find(choices: Array, placed: Array, used: Dictionary, size: int) -> Array:
    if placed.size() == size:
        return placed.duplicate(true) if Rules.is_solved(placed) else []
    for pattern in choices[placed.size()]:
        var key: String = str(pattern)
        if used.has(key) or not _column_prefix_ok(placed, pattern, size):
            continue
        placed.append(pattern)
        used[key] = true
        var found: Array = _find(choices, placed, used, size)
        used.erase(key)
        placed.pop_back()
        if not found.is_empty():
            return found
    return []


static func _column_prefix_ok(placed: Array, candidate: Array, size: int) -> bool:
    var count: int = placed.size()
    for c in range(size):
        var zeroes: int = 0
        var ones: int = 0
        var last: int = -1
        var run: int = 0
        for r in range(count + 1):
            var v: int = candidate[c] if r == count else placed[r][c]
            if v == 0:
                zeroes += 1
            else:
                ones += 1
            if v == last:
                run += 1
            else:
                run = 1
                last = v
            if run >= 3:
                return false
        if zeroes > size / 2 or ones > size / 2:
            return false
    return true


static func _legal_rows(size: int) -> Array:
    var rows: Array = []
    for mask in range(1 << size):
        var row: Array = []
        for c in range(size):
            row.append((mask >> (size - 1 - c)) & 1)
        if row.count(0) != size / 2:
            continue
        var okay: bool = true
        for c in range(size - 2):
            if row[c] == row[c + 1] and row[c] == row[c + 2]:
                okay = false
                break
        if okay:
            rows.append(row)
    return rows


static func _shuffle(values: Array, rng: RandomNumberGenerator) -> void:
    for i in range(values.size() - 1, 0, -1):
        var j: int = rng.randi_range(0, i)
        var temporary: Variant = values[i]
        values[i] = values[j]
        values[j] = temporary

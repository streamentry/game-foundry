extends RefCounted
## Pure 0/1 puzzle rules: balanced rows/columns, no triples, unique completed lines.
## -1 represents a blank cell. Does not depend on UI or input devices.

static func is_valid(board: Array) -> bool:
    var n: int = board.size()
    if n < 2 or n % 2 != 0:
        return false
    var columns: Array = []
    for c in range(n):
        columns.append([])
    for row in board:
        if not (row is Array) or row.size() != n:
            return false
        for c in range(n):
            var value: Variant = row[c]
            if value != -1 and value != 0 and value != 1:
                return false
            columns[c].append(value)
        if not _valid_line(row, n):
            return false
    for col in columns:
        if not _valid_line(col, n):
            return false
    if _duplicates_finished_lines(board) or _duplicates_finished_lines(columns):
        return false
    return true


static func is_solved(board: Array) -> bool:
    if not is_valid(board):
        return false
    for row in board:
        if row.has(-1):
            return false
    return true


static func _valid_line(line: Array, n: int) -> bool:
    if line.count(0) > n / 2 or line.count(1) > n / 2:
        return false
    for i in range(n - 2):
        if line[i] != -1 and line[i] == line[i + 1] and line[i] == line[i + 2]:
            return false
    return true


static func _duplicates_finished_lines(lines: Array) -> bool:
    for a in range(lines.size()):
        if lines[a].has(-1):
            continue
        for b in range(a + 1, lines.size()):
            if not lines[b].has(-1) and lines[a] == lines[b]:
                return true
    return false

extends RefCounted
## Shikaku: fill all cells with nonoverlapping rectangles. Each rectangle
## contains EXACTLY ONE numbered clue, whose number equals its area.
## Pure rules are used for play, persisted-state validation and uniqueness QA.

const Factory = preload("res://scripts/level_factory.gd")


static func _covers(rect: Array, row: int, col: int) -> bool:
    return row >= rect[0] and row < rect[0] + rect[2] and col >= rect[1] and col < rect[1] + rect[3]


static func _collides(a: Array, b: Array) -> bool:
    return a[0] < b[0] + b[2] and b[0] < a[0] + a[2] and \
        a[1] < b[1] + b[3] and b[1] < a[1] + a[3]


static func _clues_in(puzzle: Dictionary, rect: Array) -> Array:
    var clues: Array = []
    for clue in puzzle["clues"]:
        if _covers(rect, int(clue[0]), int(clue[1])):
            clues.append(clue)
    return clues


static func rect_from_corners(a: Vector2i, b: Vector2i) -> Array:
    return [mini(a.y, b.y), mini(a.x, b.x),
        abs(a.y - b.y) + 1, abs(a.x - b.x) + 1]


static func valid_rectangle(puzzle: Dictionary, placed: Array, rect: Array) -> String:
    if rect.size() != 4:
        return "Choose two opposite corners."
    var row: int = rect[0]
    var col: int = rect[1]
    var height: int = rect[2]
    var width: int = rect[3]
    var size: int = puzzle["size"]
    if row < 0 or col < 0 or height < 1 or width < 1 or row + height > size or col + width > size:
        return "That rectangle does not fit the board."
    for previous in placed:
        if _collides(previous, rect):
            return "That area overlaps a rectangle. Undo or choose another."
    var clues: Array = _clues_in(puzzle, rect)
    if clues.size() != 1:
        return "Every rectangle needs exactly one number."
    if height * width != int(clues[0][2]):
        return "The number must equal the rectangle's cell count."
    return ""


static func place(puzzle: Dictionary, placed: Array, rect: Array) -> Dictionary:
    var error: String = valid_rectangle(puzzle, placed, rect)
    if not error.is_empty():
        return {"ok": false, "message": error}
    placed.append(rect.duplicate())
    if complete(puzzle, placed):
        return {"ok": true, "message": "Garden complete! Every number has its rectangle."}
    return {"ok": true, "message": "A good rectangle. Find the next number."}


static func complete(puzzle: Dictionary, placed: Array) -> bool:
    if placed.size() != puzzle["clues"].size():
        return false
    var valid: Array = []
    var total: int = 0
    for rectangle in placed:
        if not valid_rectangle(puzzle, valid, rectangle).is_empty():
            return false
        valid.append(rectangle)
        total += int(rectangle[2]) * int(rectangle[3])
    return total == int(puzzle["size"]) * int(puzzle["size"])


static func valid_placements(puzzle: Dictionary, raw: Variant) -> bool:
    if typeof(raw) != TYPE_ARRAY:
        return false
    if raw.size() > puzzle["clues"].size():
        return false
    var verified: Array = []
    for record in raw:
        if typeof(record) != TYPE_ARRAY or record.size() != 4:
            return false
        var rect: Array = []
        for value in record:
            if typeof(value) not in [TYPE_INT, TYPE_FLOAT]:
                return false
            var number: float = float(value)
            if not is_finite(number) or floorf(number) != number:
                return false
            rect.append(int(number))
        if not valid_rectangle(puzzle, verified, rect).is_empty():
            return false
        verified.append(rect)
    return true


static func count_solutions(puzzle: Dictionary, limit: int = 2) -> int:
    var n: int = puzzle["size"]
    var options: Array = []
    for clue in puzzle["clues"]:
        var r: int = clue[0]
        var c: int = clue[1]
        var area: int = clue[2]
        var rectangles: Array = []
        for height in range(1, n + 1):
            if area % height != 0:
                continue
            var width: int = area / height
            if width > n:
                continue
            for row in range(maxi(0, r - height + 1), mini(r, n - height) + 1):
                for col in range(maxi(0, c - width + 1), mini(c, n - width) + 1):
                    var rect: Array = [row, col, height, width]
                    if _clues_in(puzzle, rect).size() == 1:
                        rectangles.append(rect)
        if rectangles.is_empty():
            return 0
        options.append(rectangles)
    return _search(puzzle, options, [], [], limit)


static func _search(puzzle: Dictionary, options: Array, assigned: Array,
        placements: Array, limit: int) -> int:
    if assigned.size() == options.size():
        return 1 if complete(puzzle, placements) else 0
    var best_index: int = -1
    var best: Array = []
    for i in range(options.size()):
        if assigned.has(i):
            continue
        var legal: Array = []
        for rect in options[i]:
            var overlap: bool = false
            for used in placements:
                if _collides(used, rect):
                    overlap = true
                    break
            if not overlap:
                legal.append(rect)
        if legal.is_empty():
            return 0
        if best_index == -1 or legal.size() < best.size():
            best_index = i
            best = legal
    var next_assigned: Array = assigned.duplicate()
    next_assigned.append(best_index)
    var found: int = 0
    for rect in best:
        placements.append(rect)
        found += _search(puzzle, options, next_assigned, placements, limit - found)
        placements.pop_back()
        if found >= limit:
            return found
    return found


static func valid_board(puzzle: Dictionary) -> bool:
    var n: int = puzzle.get("size", 0)
    if n < 2 or n > 6 or typeof(puzzle.get("clues", null)) != TYPE_ARRAY:
        return false
    var positions: Dictionary = {}
    var summed: int = 0
    for clue in puzzle["clues"]:
        if typeof(clue) != TYPE_ARRAY or clue.size() != 3:
            return false
        var row: int = clue[0]
        var col: int = clue[1]
        var area: int = clue[2]
        if row < 0 or col < 0 or row >= n or col >= n or area < 1 or area > n * n:
            return false
        var key: int = row * n + col
        if positions.has(key):
            return false
        positions[key] = true
        summed += area
    return summed == n * n

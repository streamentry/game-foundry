extends RefCounted
## 1-4 each row and column, plus independently checked arithmetic cages.
## Cage operators: + addition, × multiplication, − absolute difference,
## ÷ exact quotient, = one-cell value. No copying third-party level packs.

static func blank(_level: Dictionary) -> Array:
    var a: Array = []
    a.resize(16)
    a.fill(0)
    return a

static func valid_entries(_level: Dictionary, raw: Variant) -> bool:
    if typeof(raw) != TYPE_ARRAY or raw.size() != 16:
        return false
    for v in raw:
        if typeof(v) not in [TYPE_INT, TYPE_FLOAT]:
            return false
        var num: float = float(v)
        if not is_finite(num) or floorf(num) != num or num < 0.0 or num > 4.0:
            return false
    return true

static func _adjacent(a: int, b: int) -> bool:
    return absi(int(a / 4) - int(b / 4)) + absi(a % 4 - b % 4) == 1

static func valid_level(level: Dictionary) -> bool:
    if int(level.get("size", 0)) != 4 or typeof(level.get("cages")) != TYPE_ARRAY:
        return false
    var unique: Dictionary = {}
    for cage in level["cages"]:
        var cells: Array = cage["cells"]
        var op: String = str(cage["op"])
        var target: int = int(cage["target"])
        if cells.size() < 1 or cells.size() > 3 or target < 1:
            return false
        if op not in ["+", "×", "−", "÷", "="]:
            return false
        if (cells.size() == 1) != (op == "="):
            return false
        if cells.size() > 2 and op not in ["+", "×"]:
            return false
        var discovered: Dictionary = {}
        var pending: Array = [int(cells[0])]
        while not pending.is_empty():
            var here: int = pending.pop_back()
            if discovered.has(here):
                continue
            discovered[here] = true
            for p in cells:
                if _adjacent(here, int(p)) and not discovered.has(int(p)):
                    pending.append(int(p))
        if discovered.size() != cells.size():
            return false
        for id in cells:
            var n: int = int(id)
            if n < 0 or n > 15 or unique.has(n):
                return false
            unique[n] = true
    return unique.size() == 16

static func _no_conflict(entries: Array) -> bool:
    for row in range(4):
        var seen_row: Dictionary = {}
        var seen_col: Dictionary = {}
        for col in range(4):
            var a: int = int(entries[row * 4 + col])
            var b: int = int(entries[col * 4 + row])
            if a != 0 and seen_row.has(a):
                return false
            if b != 0 and seen_col.has(b):
                return false
            if a > 0:
                seen_row[a] = true
            if b > 0:
                seen_col[b] = true
    return true

static func _cage_satisfied(cage: Dictionary, entries: Array) -> bool:
    var nums: Array = []
    for id in cage["cells"]:
        var v: int = int(entries[int(id)])
        if v == 0:
            return false
        nums.append(v)
    var target: int = int(cage["target"])
    match str(cage["op"]):
        "=":
            return nums[0] == target
        "+":
            var total: int = 0
            for v in nums:
                total += int(v)
            return total == target
        "×":
            var product: int = 1
            for v in nums:
                product *= int(v)
            return product == target
        "−":
            return absi(int(nums[0]) - int(nums[1])) == target
        "÷":
            var big: int = maxi(int(nums[0]), int(nums[1]))
            var small: int = mini(int(nums[0]), int(nums[1]))
            return big % small == 0 and int(big / small) == target
    return false

static func _partial_ok(level: Dictionary, entries: Array) -> bool:
    if not _no_conflict(entries):
        return false
    for cage in level["cages"]:
        var complete_cage: bool = true
        var sum_known: int = 0
        var product_known: int = 1
        var remaining: int = 0
        for id in cage["cells"]:
            var value: int = int(entries[int(id)])
            if value == 0:
                complete_cage = false
                remaining += 1
            else:
                sum_known += value
                product_known *= value
        var target: int = int(cage["target"])
        if complete_cage:
            if not _cage_satisfied(cage, entries):
                return false
        elif cage["op"] == "+":
            if sum_known + remaining > target or sum_known + remaining * 4 < target:
                return false
        elif cage["op"] == "×":
            if target % product_known != 0 or product_known > target or product_known * int(pow(4, remaining)) < target:
                return false
    return true

static func complete(level: Dictionary, entries: Array) -> bool:
    if not valid_entries(level, entries):
        return false
    for entry in entries:
        if int(entry) == 0:
            return false
    return _partial_ok(level, entries)

static func status(level: Dictionary, entries: Array) -> String:
    if complete(level, entries):
        return "Every cage and row agrees. A complete little mosaic!"
    if not _partial_ok(level, entries):
        return "Check a repeated number or an impossible cage total."
    return "Every row/column uses 1–4; each outlined cage obeys its clue."

## Read-only views for feedback (motion and status); no rule changes.
static func has_conflict(level: Dictionary, entries: Array) -> bool:
    return not _partial_ok(level, entries)

static func cage_done(cage: Dictionary, entries: Array) -> bool:
    return _cage_satisfied(cage, entries)

static func count_solutions(level: Dictionary, limit: int = 2) -> int:
    if not valid_level(level):
        return 0
    var result: Dictionary = {"count": 0}
    _search(level, blank(level), 0, result, limit)
    return int(result["count"])

static func _search(level: Dictionary, entries: Array, cell: int,
        result: Dictionary, limit: int) -> void:
    if int(result["count"]) >= limit:
        return
    if cell == 16:
        if complete(level, entries):
            result["count"] = int(result["count"]) + 1
        return
    for digit in range(1, 5):
        entries[cell] = digit
        if _partial_ok(level, entries):
            _search(level, entries, cell + 1, result, limit)
            if int(result["count"]) >= limit:
                entries[cell] = 0
                return
    entries[cell] = 0

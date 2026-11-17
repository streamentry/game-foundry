extends SceneTree

const Rules = preload("res://scripts/binary_rules.gd")
const Factory = preload("res://scripts/puzzle_factory.gd")

var checks: int = 0
var failures: int = 0


func _expect(ok: bool, message: String) -> void:
    checks += 1
    if not ok:
        failures += 1
        push_error("FAIL: " + message)


func _initialize() -> void:
    for size in [4, 6]:
        for seed_value in [0, 1, 7, 93, 20261115]:
            var generated: Dictionary = Factory.make(seed_value, size)
            _expect(not generated.is_empty(), "generated puzzle size=%d seed=%d" % [size, seed_value])
            if generated.is_empty():
                continue
            var board: Array = generated["givens"]
            var solution: Array = generated["solution"]
            _expect(Rules.is_valid(board), "puzzle is locally valid size=%d seed=%d" % [size, seed_value])
            _expect(Rules.is_solved(solution), "solution obeys all rules size=%d seed=%d" % [size, seed_value])
            _expect(Factory.count_solutions(board, 2) == 1, "unique puzzle size=%d seed=%d" % [size, seed_value])
            _expect(board == Factory.make(seed_value, size)["givens"], "deterministic seed size=%d seed=%d" % [size, seed_value])
            var count: int = 0
            for r in range(size):
                for c in range(size):
                    if board[r][c] != -1:
                        count += 1
                        _expect(board[r][c] == solution[r][c], "given matches solution")
            _expect(count == generated["given_count"], "given-count accurate")

    var day_a: Dictionary = Factory.for_day("2026-11-15", 4)
    var day_b: Dictionary = Factory.for_day("2026-11-15", 4)
    _expect(not day_a.is_empty(), "valid daily date")
    _expect(day_a["givens"] == day_b["givens"], "daily puzzle is reproducible")
    _expect(Factory.for_day("today", 4).is_empty(), "invalid day rejected")
    _expect(Factory.make(12, 5).is_empty(), "unsupported size rejected")
    var empty: Array = []
    for r in range(4):
        empty.append([-1, -1, -1, -1])
    _expect(Factory.count_solutions(empty, 2) == 2, "ambiguous puzzle is not unique")
    var impossible: Array = [[0, 0, 0, 1], [-1, -1, -1, -1], [-1, -1, -1, -1], [-1, -1, -1, -1]]
    _expect(Factory.count_solutions(impossible, 2) == 0, "contradictory puzzle rejected")

    if failures == 0:
        print("PASS: %d independent generator, determinism and uniqueness checks" % checks)
        quit(0)
    else:
        print("FAIL: %d / %d generator checks" % [failures, checks])
        quit(1)

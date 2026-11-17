extends SceneTree

const Factory = preload("res://scripts/puzzle_factory.gd")
const Store = preload("res://scripts/progress_store.gd")

var checks: int = 0
var failures: int = 0


func _expect(ok: bool, message: String) -> void:
    checks += 1
    if not ok:
        failures += 1
        push_error("FAIL: " + message)


func _initialize() -> void:
    var path: String = "user://_foundry_progress_test.json"
    var generated: Dictionary = Factory.make(12345, 4)
    var board: Array = generated["givens"].duplicate(true)
    var changed: bool = false
    for r in range(4):
        for c in range(4):
            if board[r][c] == -1 and not changed:
                board[r][c] = 0
                changed = true
    _expect(changed, "board includes editable cells")
    _expect(Store.write("free", "", generated, board, path), "atomic save succeeded")
    var restored: Dictionary = Store.restore(path)
    _expect(not restored.is_empty(), "restore succeeded")
    if not restored.is_empty():
        _expect(restored["board"] == board, "saved cells preserved")
        _expect(restored["puzzle"]["givens"] == generated["givens"], "verified puzzle regenerated")
        _expect(restored["puzzle"]["seed"] == 12345, "seed preserved")

    var payload: Dictionary = {
        "version": 1,
        "mode": "free",
        "day": "",
        "size": 4,
        "seed": 12345,
        "board": board.duplicate(true),
    }
    _expect(not Store.validate(payload).is_empty(), "valid in-memory payload")
    payload["version"] = 99
    _expect(Store.validate(payload).is_empty(), "unsupported save version rejected")
    payload["version"] = 1
    payload["board"] = board.duplicate(true)
    var corrupted_clue: bool = false
    for r in range(4):
        if corrupted_clue:
            break
        for c in range(4):
            if generated["givens"][r][c] != -1:
                payload["board"][r][c] = 1 - int(generated["givens"][r][c])
                corrupted_clue = true
                break
    _expect(Store.validate(payload).is_empty(), "tampered fixed clues rejected")

    var date: String = "2026-11-15"
    var daily: Dictionary = Factory.for_day(date, 4)
    _expect(Store.write("daily", date, daily, daily["givens"], path), "daily puzzle save succeeded")
    _expect(not Store.restore(path).is_empty(), "daily resume verified")
    var forged: Dictionary = {
        "version": 1, "mode": "daily", "day": "2026-11-14",
        "size": 4, "seed": daily["seed"], "board": daily["givens"]
    }
    _expect(Store.validate(forged).is_empty(), "daily date/seed mismatch rejected")

    var high_seed: Dictionary = Factory.make(4294967295, 6)
    _expect(not high_seed.is_empty(), "large unsigned random seed generates puzzle")
    _expect(Store.write("free", "", high_seed, high_seed["givens"], path), "high random seed saved")
    _expect(not Store.restore(path).is_empty(), "high random seed restored correctly")

    DirAccess.remove_absolute(ProjectSettings.globalize_path(path))
    if failures == 0:
        print("PASS: %d offline progress and integrity checks" % checks)
        quit(0)
    else:
        print("FAIL: %d / %d progress checks" % [failures, checks])
        quit(1)

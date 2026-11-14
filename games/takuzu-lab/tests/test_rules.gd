extends SceneTree

const Rules = preload("res://scripts/binary_rules.gd")

func _initialize() -> void:
    var solved: Array = [
        [0, 0, 1, 1],
        [0, 1, 0, 1],
        [1, 0, 1, 0],
        [1, 1, 0, 0]
    ]
    var partial: Array = [
        [0, 0, 1, -1],
        [0, -1, -1, -1],
        [-1, -1, 1, -1],
        [-1, -1, -1, -1]
    ]
    var too_many := solved.duplicate(true)
    too_many[0] = [0, 0, 0, 1]
    var duplicate_rows := solved.duplicate(true)
    duplicate_rows[1] = duplicate_rows[0].duplicate()
    var bad_value := partial.duplicate(true)
    bad_value[1][2] = 3
    var wrong_size: Array = [[0, 1], [0]]
    var tests: Array = [
        [Rules.is_valid(solved), "solved board is valid"],
        [Rules.is_solved(solved), "solved board passes complete check"],
        [Rules.is_valid(partial), "incomplete valid puzzle accepted"],
        [not Rules.is_solved(partial), "incomplete puzzle not solved"],
        [not Rules.is_valid(too_many), "triples and balance rejected"],
        [not Rules.is_valid(duplicate_rows), "duplicate complete rows rejected"],
        [not Rules.is_valid(bad_value), "out-of-range number rejected"],
        [not Rules.is_valid(wrong_size), "ragged board rejected"]
    ]
    var failures := 0
    for check in tests:
        if not check[0]:
            push_error("FAIL: " + check[1])
            failures += 1
    if failures == 0:
        print("PASS: 8 Takuzu rule checks")
        quit(0)
    else:
        quit(1)

extends SceneTree
const Levels = preload("res://scripts/level_factory.gd")
const World = preload("res://scripts/world.gd")
const Rules = preload("res://scripts/rules.gd")
var checks: int = 0
var failures: int = 0

func expect(ok: bool, message: String) -> void:
    checks += 1
    if not ok:
        failures += 1
        push_error("FAIL: " + message)

func cleanup() -> void:
    for path in [World.FILE_PATH, World.FILE_PATH + ".tmp"]:
        if FileAccess.file_exists(path):
            DirAccess.remove_absolute(ProjectSettings.globalize_path(path))

func solve(s: Dictionary) -> void:
    var answer: Array = Levels.reference_solution(int(s["round"]))
    var level: Dictionary = Levels.make(int(s["round"]))
    for i in range(9):
        var turns: int = 0
        while int(s["rotations"][i]) != int(answer[i]) and turns < Rules.orientation_count(str(level["types"][i])):
            expect(World.rotate(s, i), "rotation accepted")
            turns += 1

func _initialize() -> void:
    cleanup()
    var s: Dictionary = World.fresh()
    expect(s["mode"] == "living" and s["round"] == 0, "default living mode")
    expect(World.can_switch_mode(s), "mode can be selected before playing")
    expect(not World.choose(s, 0), "world choice unavailable without solving")
    expect(not World.rotate(s, -1) and not World.rotate(s, 9), "invalid board indices rejected")
    expect(World.rotate(s, 0), "one tap rotates a tile")
    expect(not World.can_switch_mode(s), "comparison mode locked after a first move")
    expect(World.undo(s), "undo restores previous rotation")
    expect(World.can_switch_mode(s), "mode can be selected after undoing to fresh board")
    solve(s)
    expect(bool(s["solved"]), "first stream solved through real rotation actions")
    expect(not World.rotate(s, 0), "solved stream cannot be changed until undo")
    expect(World.undo(s), "solved state may be undone")
    expect(not bool(s["solved"]), "undo clears solved state")
    solve(s)
    expect(World.choose(s, 0), "first consequence commits after solving")
    expect(s["round"] == 1 and s["events"] == [0], "world history advances one step")
    expect(World.garden_counts(s) == [1, 0, 0], "one shade tree remembered")
    expect(not World.choose(s, 1), "choice cannot be replayed without another solution")
    solve(s)
    expect(World.choose(s, 1), "bridge choice committed")
    solve(s)
    expect(World.choose(s, 2), "wildflowers choice committed")
    expect(s["events"] == [0, 1, 2] and s["round"] == 3, "distinct actions persist")
    expect(World.garden_counts(s) == [1, 1, 1], "garden resource counts derived from actions")
    expect(World.consequence(s).find("crossing") >= 0, "combined choices produce second-order consequence")
    expect(World.memory_line(s).find("Fen") >= 0, "specific character remembers last decision")
    expect(World.save(s), "atomic offline save")
    var recovered: Dictionary = World.load_saved()
    expect(recovered["events"] == [0, 1, 2] and recovered["round"] == 3, "history and puzzle persist")
    expect(World.garden_counts(recovered) == [1, 1, 1], "saved history reconstructs resource effects")

    var corrupted: Dictionary = s.duplicate(true)
    corrupted["events"] = [0, 10, 2]
    expect(World.validated(corrupted)["round"] == 0, "invalid events fail closed")
    corrupted = s.duplicate(true)
    corrupted["events"] = [0]
    expect(World.validated(corrupted)["round"] == 0, "forged chapter fails closed")
    corrupted = s.duplicate(true)
    corrupted["solved"] = true
    expect(World.validated(corrupted)["round"] == 0, "forged win fails closed")
    corrupted = s.duplicate(true)
    corrupted["rotations"][0] = 999
    expect(World.validated(corrupted)["round"] == 0, "invalid rotations fail closed")
    for step in range(3, Levels.COUNT):
        solve(recovered)
        expect(World.choose(recovered, step % 3), "chapter %d advances after legal solution" % step)
    expect(int(recovered["round"]) == Levels.COUNT and recovered["rotations"].is_empty(), "all six chapters complete")
    expect(not World.choose(recovered, 0) and not World.rotate(recovered, 0), "finished garden cannot be farmed repeatedly")
    expect(World.validated(recovered)["round"] == Levels.COUNT, "finished state round trips through validator")

    var p: Dictionary = World.fresh("puzzle")
    expect(p["mode"] == "puzzle" and p["events"].is_empty(), "puzzle-only control has no village")
    solve(p)
    expect(not World.choose(p, 0), "control cannot commit living-world choice")
    expect(World.next_puzzle(p), "control advances after solving")
    expect(p["round"] == 1 and p["events"].is_empty(), "control has puzzle progression without artificial story history")

    cleanup()
    print("Evergrove world: %d checks, %d failures" % [checks, failures])
    quit(0 if failures == 0 else 1)

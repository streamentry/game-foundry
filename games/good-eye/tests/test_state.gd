extends SceneTree

const Model = preload("res://scripts/game_state.gd")
const Save = preload("res://scripts/local_store.gd")
var checks := 0
var failures := 0

func expect(ok: bool, description: String) -> void:
    checks += 1
    if not ok:
        failures += 1
        push_error("FAIL: " + description)

func cleanup() -> void:
    for suffix in ["", ".tmp"]:
        if FileAccess.file_exists(Save.SAVE_PATH + suffix):
            DirAccess.remove_absolute(ProjectSettings.globalize_path(Save.SAVE_PATH + suffix))

func _initialize() -> void:
    cleanup()
    var day := "2026-11-21"
    var state: Dictionary = Model.fresh(day, 12345)
    expect(state["mode"] == "practice", "first launch practice")
    expect(Model.current_seed(state) == 12345, "initial practice seed")
    expect(not Model.advance(state, 999), "unplayed shape cannot advance")
    expect(Model.record_cut(state, 48.2, 10.0, 0.04), "practice cut accepted")
    expect(Model.current_cut(state) == [10.0, 0.04], "practice cut placement stored")
    expect(not Model.record_cut(state, 50.0), "practice rescore rejected")
    expect(Model.retry(state), "practice retry")
    expect(Model.current_seed(state) == 12345 and not Model.revealed(state), "retry keeps seed")
    expect(Model.record_cut(state, 51.0), "retry may be scored")
    expect(Model.advance(state, 6789), "practice next shape")
    expect(Model.current_seed(state) == 6789 and not Model.revealed(state), "next seed")

    Model.switch_mode(state, "daily", day)
    expect(Model.current_seed(state) == Model.daily_seed(day, 0), "daily seed deterministic")
    for i in range(Model.DAILY_ROUNDS):
        expect(state["daily_index"] == i, "daily index %d" % i)
        expect(Model.record_cut(state, 49.0 + i * 0.2), "daily records %d" % i)
        expect(not Model.record_cut(state, 50.0), "daily score locked %d" % i)
        expect(not Model.retry(state), "daily retry forbidden %d" % i)
        if i < Model.DAILY_ROUNDS - 1:
            expect(Model.advance(state, 1), "daily advance %d" % i)
    expect(Model.daily_complete(state), "five rounds complete")
    expect(not Model.advance(state, 1), "daily sixth round denied")
    expect(Model.average_daily_score(state) > 95.0, "daily score average")

    expect(Save.write(state), "atomic local save")
    var recovered: Dictionary = Save.read(Model.fresh(day, 2345))
    expect(Model.daily_complete(recovered), "daily progress restored")
    expect(recovered["daily_cuts"].size() == 5, "all daily cut positions restored")
    expect(recovered["practice_seed"] == 6789, "practice preserved in save")

    var next_day: Dictionary = Model.restore(state.duplicate(true), "2026-11-22", 23)
    expect(next_day["daily_percents"].is_empty() and next_day["daily_cuts"].is_empty(), "UTC day rolls over")
    expect(next_day["practice_seed"] == 6789, "day change preserves practice")
    var invalid: Dictionary = state.duplicate(true)
    invalid["daily_index"] = 10
    expect(Model.restore(invalid, day, 777)["practice_seed"] == 777, "corrupted state resets")

    cleanup()
    print("Good Eye state: %d checks, %d failures" % [checks, failures])
    quit(0 if failures == 0 else 1)

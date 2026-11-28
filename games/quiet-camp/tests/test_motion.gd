extends SceneTree
const Factory = preload("res://scripts/level_factory.gd")
const Progress = preload("res://scripts/progress.gd")
const Settings = preload("res://scripts/settings.gd")
const Motion = preload("res://scripts/motion.gd")

var checks: int = 0
var failures: int = 0


func expect(ok: bool, message: String) -> void:
	checks += 1
	if not ok:
		failures += 1
		push_error("FAIL: " + message)


func _initialize() -> void:
	call_deferred("_run")


func _wait(seconds: float) -> void:
	await create_timer(seconds).timeout


func _cleanup() -> void:
	for path in [Progress.FILE_PATH, Progress.FILE_PATH + ".tmp", Settings.FILE_PATH]:
		if FileAccess.file_exists(path):
			DirAccess.remove_absolute(ProjectSettings.globalize_path(path))


func _run() -> void:
	_cleanup()
	var scene: PackedScene = load("res://scenes/main.tscn")
	expect(scene != null, "main scene imports")
	if scene == null:
		quit(1)
		return
	var app = scene.instantiate()
	root.add_child(app)
	await process_frame
	await _wait(0.7)  # let the board intro finish before measuring motion

	app.set_reduced_motion(true)
	expect(app.motion.reduced_motion, "reduced motion turns on in the app")
	expect(Settings.load_reduced_motion(), "reduced motion is saved to settings")

	var first: int = int(Factory.reference_tents(0)[0])
	var cell = app.cells[first]
	app._tap(first)
	expect(app.state["marks"][first] == 1, "tent is placed immediately in reduced motion")
	expect(Motion.running(cell) == 0, "reduced motion starts no tweens on the tapped cell")
	expect(cell.offset_transform_scale == Vector2.ONE, "reduced motion rests at scale one")
	expect(cell.offset_transform_position == Vector2.ZERO, "reduced motion rests at zero offset")

	app.set_reduced_motion(false)
	expect(not app.motion.reduced_motion, "reduced motion turns off in the app")
	expect(not Settings.load_reduced_motion(), "turning reduced motion off is saved")
	var second: int = int(Factory.reference_tents(0)[1])
	var cell2 = app.cells[second]
	app._tap(second)
	expect(app.state["marks"][second] == 1, "tent is placed with motion on")
	expect(Motion.running(cell2) > 0, "motion runs when reduced motion is off")
	await _wait(0.6)
	expect(Motion.running(cell2) == 0, "motion finishes within 0.6 s")
	expect(cell2.offset_transform_scale == Vector2.ONE, "cell returns to rest scale after motion")
	expect(cell2.offset_transform_position == Vector2.ZERO, "cell returns to zero offset after motion")

	app.queue_free()
	await process_frame
	_cleanup()
	print("Quiet Camp motion: %d checks, %d failures" % [checks, failures])
	quit(0 if failures == 0 else 1)

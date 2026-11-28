extends SceneTree
const Motion = preload("res://scripts/motion.gd")
const Progress = preload("res://scripts/progress.gd")
const Rules = preload("res://scripts/rules.gd")
const SETTINGS_PATH: String = "user://settings.json"
var checks: int = 0
var failures: int = 0

func expect(ok: bool, note: String) -> void:
	checks += 1
	if not ok:
		failures += 1
		push_error("FAIL: " + note)

func _initialize() -> void:
	call_deferred("_run")

func _clean() -> void:
	for path in [Progress.FILE_PATH, SETTINGS_PATH]:
		if FileAccess.file_exists(path):
			DirAccess.remove_absolute(ProjectSettings.globalize_path(path))

func _wait(seconds: float) -> void:
	await create_timer(seconds).timeout

func _run() -> void:
	_clean()
	var scene: PackedScene = load("res://scenes/main.tscn")
	var app = scene.instantiate()
	root.add_child(app)
	await process_frame
	await _wait(1.2)  # let the board intro stagger (about 0.46 s at most) finish first
	var original: Array = Rules.blank(app.level)
	var cell_index: int = -1
	for p in range(original.size()):
		if int(original[p]) == 0:
			cell_index = p
			break
	expect(cell_index >= 0, "an editable cell exists")
	var cell: Control = app.cells[cell_index]

	# Reduced motion on: a select and a placement apply instantly, nothing animates.
	app.set_reduced_motion(true)
	expect(app.motion.reduced_motion, "motion follows the reduced-motion setting")
	app._tap(cell_index)
	expect(int(app.state["entries"][cell_index]) == 1, "reduced motion: first tap still assigns route A")
	expect(Motion.running(cell) == 0, "reduced motion: no tween runs on the placed cell")
	expect(cell.offset_transform_scale == Vector2.ONE, "reduced motion: cell is at rest, scale 1")

	# Reduced motion off: a placement animates, then settles back at rest.
	app.set_reduced_motion(false)
	app._tap(cell_index)
	expect(int(app.state["entries"][cell_index]) == 2, "motion on: second tap assigns route B")
	expect(cell.offset_transform_scale != Vector2.ONE, "motion on: pop overshoots right after placement")
	await _wait(0.6)
	expect(Motion.running(cell) == 0, "motion on: tween finished within 0.6 s")
	expect(cell.offset_transform_scale == Vector2.ONE, "motion on: cell back at rest, scale 1")
	expect(cell.modulate.a == 1.0, "motion on: cell fully opaque at rest")

	app.queue_free()
	await process_frame
	_clean()
	print("Motion: %d checks, %d failures" % [checks, failures])
	quit(0 if failures == 0 else 1)

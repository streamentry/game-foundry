extends SceneTree
const Rules = preload("res://scripts/rules.gd")
const Motion = preload("res://scripts/motion.gd")
const Progress = preload("res://scripts/progress.gd")
const Factory = preload("res://scripts/level_factory.gd")
var checks: int = 0
var failures: int = 0

func expect(ok: bool, note: String) -> void:
	checks += 1
	if not ok:
		failures += 1
		push_error("FAIL: " + note)

func _initialize() -> void:
	call_deferred("_run")

func _at_rest(c: Control) -> bool:
	return c.offset_transform_scale.is_equal_approx(Vector2.ONE) \
		and c.offset_transform_position.is_zero_approx() \
		and is_equal_approx(c.modulate.a, 1.0)

func _run() -> void:
	if FileAccess.file_exists(Progress.FILE_PATH):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(Progress.FILE_PATH))
	var scene: PackedScene = load("res://scenes/main.tscn")
	var app = scene.instantiate()
	root.add_child(app)
	await process_frame
	await process_frame

	var original: Array = Rules.blank(app.level)
	var first_editable: int = -1
	for p in range(original.size()):
		if int(original[p]) == 0:
			first_editable = p
			break
	expect(first_editable >= 0, "editable cell exists")
	var cell: Control = app.cells[first_editable]

	# Reduced motion on: select and place must end instantly at rest with no tweens.
	var answer: Array = Factory.reference_solution(0)
	await create_timer(0.7).timeout   # let the intro stagger from level load finish first
	app.motion.reduced_motion = true
	app._tap(first_editable)
	app._enter(int(answer[first_editable]))
	expect(Motion.running(cell) == 0, "reduced motion: no running tweens on placed cell")
	expect(cell.offset_transform_scale == Vector2.ONE, "reduced motion: scale is exactly Vector2.ONE")
	expect(int(app.state["entries"][first_editable]) == int(answer[first_editable]),
		"reduced motion: game state still updates immediately")

	# Reduced motion off: erase, then wait ~0.6 s for the motion to finish and check rest.
	app.motion.reduced_motion = false
	app._enter(0)
	expect(cell.offset_transform_scale != Vector2.ONE or Motion.running(cell) > 0,
		"motion on: pop is animating right after the change")
	await create_timer(0.6).timeout
	expect(Motion.running(cell) == 0, "motion on: all tweens finished within 0.6 s")
	expect(_at_rest(cell), "motion on: cell is back at rest after 0.6 s")
	expect(int(app.state["entries"][first_editable]) == 0, "motion on: erase still applied to state")

	print("Motion: %d checks, %d failures" % [checks, failures])
	app.queue_free()
	await process_frame
	if FileAccess.file_exists(Progress.FILE_PATH):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(Progress.FILE_PATH))
	quit(0 if failures == 0 else 1)

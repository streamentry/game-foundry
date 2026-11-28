extends Control
## Original offline word search: straight horizontal/vertical/diagonal taps.
const Factory=preload("res://scripts/level_factory.gd")
const Rules=preload("res://scripts/rules.gd")
const Progress=preload("res://scripts/progress.gd")
const Motion=preload("res://scripts/motion.gd")
const Capsules=preload("res://scripts/capsule_layer.gd")
const SETTINGS_PATH:String="user://settings.json"
## Palette drawn from the icon: sea teal ground, sand-gold highlight, cream letters.
const BG=Color("#17525F")
const SURFACE=Color("#0F3F4C")
const SURFACE_ALT=Color("#226474")
const INK=Color("#FBF6EA")
const INK_MUTED=Color("#B7DCE4")
const ACCENT=Color("#F2C97D")
const ACCENT_INK=Color("#0C3440")
const SUCCESS=Color("#A7E8C4")
const FOCUS=Color("#A6E3EC")
const CLEAR:=Color(0,0,0,0)
const CELL_SIZE:=Vector2(58,58)
const GAP:=4

var state:Dictionary={}
var level:Dictionary={}
var history:Array=[]
var selected:int=-1
var cells:Array=[]
var word_ends:Array=[]     # per target word: [start cell, end cell] from the level data
var word_cells:Array=[]    # per target word: every cell on its straight line
var shown:Array=[]         # word indices with a capsule on the board
var chip_labels:Array=[]
var motion:=Motion.new()
var capsules
var board:PanelContainer
var grid:GridContainer
var heading:Label
var result:Label
var chips:HFlowContainer
var note:Label
var reduce_check:CheckButton
var next_button:Button
var prev_button:Button
var undo_button:Button

func _ready()->void:
    state=Progress.load_saved()
    motion.reduced_motion=_load_reduced_motion()
    _build_ui()
    capsules.reduced_motion=motion.reduced_motion
    _load_level()

func _label(message:String,px:int,color:Color=INK)->Label:
    var item:=Label.new()
    item.text=message
    item.autowrap_mode=TextServer.AUTOWRAP_WORD_SMART
    item.add_theme_font_size_override("font_size",px)
    item.add_theme_color_override("font_color",color)
    return item

func _box(fill:Color,border:Color,width:int,radius:int=14,shadow:int=0)->StyleBoxFlat:
    var box:=StyleBoxFlat.new()
    box.bg_color=fill
    box.border_color=border
    box.set_border_width_all(width)
    box.set_corner_radius_all(radius)
    box.shadow_size=shadow
    box.shadow_color=Color(0,0,0,0.28)
    box.shadow_offset=Vector2(0,2)
    return box

func _style(item:Button,fill:Color,ink:Color)->void:
    item.add_theme_stylebox_override("normal",_box(fill,CLEAR,0,14,4))
    item.add_theme_stylebox_override("hover",_box(fill.lightened(0.08),CLEAR,0,14,4))
    item.add_theme_stylebox_override("pressed",_box(fill.darkened(0.12),CLEAR,0))
    item.add_theme_stylebox_override("disabled",_box(Color(fill.r,fill.g,fill.b,0.35),CLEAR,0))
    item.add_theme_stylebox_override("focus",_box(CLEAR,FOCUS,3))
    for mode in ["font_color","font_hover_color","font_pressed_color","font_focus_color"]:
        item.add_theme_color_override(mode,ink)
    item.add_theme_color_override("font_disabled_color",Color(ink.r,ink.g,ink.b,0.6))

func _button(message:String,action:Callable,primary:bool=false)->Button:
    var item:=Button.new()
    item.text=message
    item.accessibility_name=message
    item.custom_minimum_size=Vector2(0,54)
    item.size_flags_horizontal=Control.SIZE_EXPAND_FILL
    item.add_theme_font_size_override("font_size",17)
    _style(item,ACCENT if primary else SURFACE_ALT,ACCENT_INK if primary else INK)
    item.pressed.connect(action)
    return item

func _row()->HBoxContainer:
    var row:=HBoxContainer.new()
    row.add_theme_constant_override("separation",9)
    return row

func _build_ui()->void:
    var bg:=ColorRect.new()
    bg.color=BG
    bg.mouse_filter=Control.MOUSE_FILTER_IGNORE
    add_child(bg)
    bg.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
    var scroll:=ScrollContainer.new()
    add_child(scroll)
    scroll.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
    scroll.horizontal_scroll_mode=ScrollContainer.SCROLL_MODE_DISABLED
    var margin:=MarginContainer.new()
    scroll.add_child(margin)
    margin.size_flags_horizontal=Control.SIZE_EXPAND_FILL
    for kv in [["margin_left",14],["margin_right",14],["margin_top",20],["margin_bottom",23]]:
        margin.add_theme_constant_override(kv[0],kv[1])
    var column:=VBoxContainer.new()
    margin.add_child(column)
    column.add_theme_constant_override("separation",12)
    column.add_child(_label("WORD SEARCH SHORES / FIND WHAT IS HIDDEN",12,ACCENT))
    column.add_child(_label("Three words, one little shore.",27))
    column.add_child(_label("Find each listed word horizontally, vertically or diagonally. Tap the first letter and then its last letter; either reading direction works.",15,INK_MUTED))
    heading=_label("",16)
    column.add_child(heading)
    var panel:=_box(SURFACE,CLEAR,0,18,10)
    panel.set_content_margin_all(9.0)
    board=PanelContainer.new()
    board.add_theme_stylebox_override("panel",panel)
    board.size_flags_horizontal=Control.SIZE_SHRINK_CENTER
    column.add_child(board)
    var board_area:=Control.new()
    board_area.custom_minimum_size=Vector2(6*CELL_SIZE.x+5*GAP,6*CELL_SIZE.y+5*GAP)
    board.add_child(board_area)
    capsules=Capsules.new()
    board_area.add_child(capsules)
    capsules.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
    grid=GridContainer.new()
    grid.columns=6
    grid.add_theme_constant_override("h_separation",GAP)
    grid.add_theme_constant_override("v_separation",GAP)
    board_area.add_child(grid)
    grid.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
    chips=HFlowContainer.new()
    chips.add_theme_constant_override("h_separation",14)
    chips.add_theme_constant_override("v_separation",6)
    column.add_child(chips)
    note=_label("Choose a first and last letter. The line will be checked.",15,INK_MUTED)
    column.add_child(note)
    result=_label("",16)
    column.add_child(result)
    reduce_check=CheckButton.new()
    reduce_check.text="Reduce motion"
    reduce_check.accessibility_name="Reduce motion"
    reduce_check.custom_minimum_size=Vector2(0,48)
    reduce_check.add_theme_font_size_override("font_size",16)
    for mode in ["font_color","font_hover_color","font_pressed_color","font_focus_color"]:
        reduce_check.add_theme_color_override(mode,INK)
    reduce_check.button_pressed=motion.reduced_motion
    reduce_check.toggled.connect(_set_reduced_motion)
    column.add_child(reduce_check)
    var actions:=_row()
    undo_button=_button("Undo",_undo)
    actions.add_child(undo_button)
    actions.add_child(_button("Restart",_restart))
    column.add_child(actions)
    var nav:=_row()
    prev_button=_button("Previous",_previous)
    next_button=_button("Next puzzle",_next,true)
    nav.add_child(prev_button)
    nav.add_child(next_button)
    column.add_child(nav)
    column.add_child(_button("Copy challenge",_share))
    column.add_child(_label("Every target has one intended straight-line location. No timers, hidden penalties or account.",13,INK_MUTED))

func _build_chips()->void:
    for child in chips.get_children():
        chips.remove_child(child)
        child.queue_free()
    chip_labels.clear()
    for word in level["words"]:
        var chip:=RichTextLabel.new()
        chip.bbcode_enabled=true
        chip.fit_content=true
        chip.scroll_active=false
        chip.autowrap_mode=TextServer.AUTOWRAP_OFF
        chip.add_theme_font_size_override("normal_font_size",18)
        chip.add_theme_color_override("default_color",INK)
        chip.text="○ "+str(word)
        chips.add_child(chip)
        chip_labels.append(chip)

func _load_level()->void:
    level=Factory.make(int(state["level"]))
    selected=-1
    history.clear()
    word_ends.clear()
    word_cells.clear()
    for word in level["words"]:
        var ends:Array=Rules.match_paths(level,str(word))[0]
        word_ends.append(ends)
        word_cells.append(Rules.path_between(level,int(ends[0]),int(ends[1])))
    for child in grid.get_children():
        grid.remove_child(child)
        child.queue_free()
    cells.clear()
    for id in range(36):
        var item:=Button.new()
        item.text=str(level["letters"])[id]
        item.custom_minimum_size=CELL_SIZE
        item.add_theme_font_size_override("font_size",28)
        item.focus_mode=Control.FOCUS_ALL
        item.pressed.connect(_tap.bind(id))
        grid.add_child(item)
        cells.append(item)
    capsules.cells=cells
    capsules.clear()
    shown.clear()
    _build_chips()
    motion.stagger_in(cells)
    _refresh()

func _tap(index:int)->void:
    motion.press(cells[index])
    if selected<0:
        selected=index
        _refresh()
        return
    var word_index:int=Rules.matched_word(level,selected,index)
    var found_now:int=-1
    if word_index>=0 and int(state["entries"][word_index])==0:
        history.append(state["entries"].duplicate())
        if history.size()>24:history.pop_front()
        if Progress.update(state,word_index,1):
            _save()
            found_now=word_index
            note.text="Found "+str(level["words"][word_index])+"! Choose another word."
            motion.pop(cells[selected])
            motion.pop(cells[index])
            var line:Array=[]
            for cell in word_cells[word_index]:
                line.append(cells[int(cell)])
            motion.sweep(line,ACCENT)
            if Rules.complete(level,state["entries"]):
                motion.sweep(cells,ACCENT)
                motion.celebrate(self,board.get_global_rect().get_center(),[ACCENT,SUCCESS,SURFACE])
        else:
            history.pop_back()
    else:
        note.text="Those endpoints do not identify a new target word. Try another starting letter."
        motion.shake(cells[index])
    selected=-1
    _refresh(found_now)

func _undo()->void:
    if history.is_empty():return
    state["entries"]=history.pop_back()
    selected=-1
    _save()
    _refresh()

func _restart()->void:
    history.clear()
    selected=-1
    state["entries"]=Rules.blank(level)
    _save()
    motion.stagger_in(cells)
    _refresh()

func _next()->void:
    if not Progress.can_advance(state):return
    Progress.set_level(state,int(state["level"])+1)
    _save()
    _load_level()
    motion.slide_swap(board,1)

func _previous()->void:
    if int(state["level"])==0:return
    Progress.set_level(state,int(state["level"])-1)
    _save()
    _load_level()
    motion.slide_swap(board,-1)

func _refresh(animate_word:int=-1)->void:
    heading.text="SHORE %d OF %d  •  %s"%[int(state["level"])+1,Factory.COUNT,str(level["name"])]
    var solved_cells:Dictionary={}
    for i in range(level["words"].size()):
        var word:String=str(level["words"][i])
        var completed:bool=int(state["entries"][i])==1
        chip_labels[i].text=("[color=#F2C97D]✓ [s]%s[/s][/color]"%word) if completed else ("○ "+word)
        if completed:
            for cell in word_cells[i]:
                solved_cells[int(cell)]=true
    for i in range(36):
        var button:Button=cells[i]
        var solved:bool=solved_cells.has(i)
        _style_cell(button,solved,selected==i)
        button.accessibility_name=_cell_name(i,solved)
    _sync_capsules(animate_word)
    result.text=Rules.status(level,state["entries"])
    undo_button.disabled=history.is_empty()
    next_button.disabled=not Progress.can_advance(state)
    prev_button.disabled=int(state["level"])==0

func _cell_box(fill:Color,border:Color,width:int)->StyleBoxFlat:
    var box:=_box(fill,border,width,12)
    return box

func _style_cell(button:Button,solved:bool,current:bool)->void:
    # Cell backgrounds stay transparent so the sand capsule behind them shows through.
    var ring:=FOCUS if current else CLEAR
    var ring_width:=3 if current else 0
    button.add_theme_stylebox_override("normal",_cell_box(Color(1,1,1,0.16) if current else CLEAR,ring,ring_width))
    button.add_theme_stylebox_override("hover",_cell_box(Color(1,1,1,0.10),ring,ring_width))
    button.add_theme_stylebox_override("pressed",_cell_box(Color(1,1,1,0.16),ring,ring_width))
    button.add_theme_stylebox_override("disabled",_cell_box(CLEAR,CLEAR,0))
    button.add_theme_stylebox_override("focus",_cell_box(CLEAR,FOCUS,3))
    var ink:=ACCENT_INK if solved else INK
    for mode in ["font_color","font_hover_color","font_pressed_color","font_focus_color","font_disabled_color"]:
        button.add_theme_color_override(mode,ink)

func _cell_name(index:int,solved:bool)->String:
    var label:="Row %d, column %d, letter %s"%[int(index/6)+1,index%6+1,str(cells[index].text)]
    return label+(", part of a found word" if solved else "")

## Keeps one capsule per found word. Adding a word animates only that word;
## anything removed (undo, restart) rebuilds the capsules at rest.
func _sync_capsules(animate_word:int=-1)->void:
    var wanted:Array=[]
    for i in range(level["words"].size()):
        if int(state["entries"][i])==1:
            wanted.append(i)
    var stale:bool=false
    for s in shown:
        if not wanted.has(s):
            stale=true
    if stale:
        capsules.clear()
        shown.clear()
    for i in wanted:
        if not shown.has(i):
            var ends:Array=word_ends[i]
            capsules.add_word(int(ends[0]),int(ends[1]),i==animate_word)
            shown.append(i)

func _set_reduced_motion(on:bool)->void:
    motion.reduced_motion=on
    capsules.reduced_motion=on
    reduce_check.set_pressed_no_signal(on)
    _save_settings(on)

func _load_reduced_motion()->bool:
    if not FileAccess.file_exists(SETTINGS_PATH):return false
    var file:=FileAccess.open(SETTINGS_PATH,FileAccess.READ)
    if file==null:return false
    var parser:=JSON.new()
    if parser.parse(file.get_as_text())!=OK:return false
    var data:Dictionary=parser.data if typeof(parser.data)==TYPE_DICTIONARY else {}
    return typeof(data.get("reduced_motion",null))==TYPE_BOOL and bool(data["reduced_motion"])

func _save_settings(on:bool)->void:
    var file:=FileAccess.open(SETTINGS_PATH,FileAccess.WRITE)
    if file==null:
        push_warning("Word Search Shores could not save settings.")
        return
    file.store_string(JSON.stringify({"reduced_motion":on}))
    file.close()

func _save()->void:
    if not Progress.save(state):push_warning("Word Search Shores could not save progress.")

func _share()->void:
    var message:String="Word Search Shores: find three words on shore %d"%(int(state["level"])+1)
    if OS.has_feature("web"):
        var base:Variant=JavaScriptBridge.eval("window.location.origin + window.location.pathname",true)
        if typeof(base)==TYPE_STRING and str(base).begins_with("http"):
            message+="\n"+str(base)+"?level="+str(int(state["level"])+1)
    DisplayServer.clipboard_set(message)
    result.text="Challenge text prepared where clipboard access allows."

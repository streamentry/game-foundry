extends Control
## Word Square Studio: arrange three provided 3-letter words so each column
## reads as its row. Rules, levels, saves and solutions are unchanged; this
## file only adds the letter-tile board, theme and motion hooks.
const Factory=preload("res://scripts/level_factory.gd")
const Rules=preload("res://scripts/rules.gd")
const Progress=preload("res://scripts/progress.gd")
const Motion=preload("res://scripts/motion.gd")
const MODE:String="square"
const TITLE:String="Word Square Studio"
const SETTINGS_PATH:String="user://settings.json"
# Palette taken from the app icon: royal purple field, cream tiles, gold diagonal.
const BG:Color=Color("#3B1D6B")
const SURFACE:Color=Color("#2A1252")
const SURFACE_ALT:Color=Color("#4E2A86")
const INK:Color=Color("#FBF5E6")
const INK_MUTED:Color=Color("#DCCFF0")
const TILE:Color=Color("#FBF5E6")
const TILE_EDGE:Color=Color("#D9CFB8")
const TILE_INK:Color=Color("#2E1656")
const ACCENT:Color=Color("#ECB84C")
const ACCENT_HOVER:Color=Color("#F4C863")
const ACCENT_PRESSED:Color=Color("#D39B33")
const ACCENT_INK:Color=Color("#2E1656")
const SUCCESS:Color=Color("#A8E6B8")
const FOCUS:Color=Color("#FFE08A")
const TILE_PX:int=104
const TILE_FONT:int=60

var state:Dictionary={}
var level:Dictionary={}
var history:Array=[]
var motion:=Motion.new()
var reduced_motion:bool=false
var board_grid:GridContainer
var tiles:Array=[]
var tile_labels:Array=[]
var choice_buttons:Array=[]
var shown_words:VBoxContainer
var choices:GridContainer
var heading:Label
var status_label:Label
var guidance:Label
var next_button:Button
var prev_button:Button
var undo_button:Button
var motion_toggle:CheckButton

func _ready()->void:
    motion.owner=self
    reduced_motion=_load_reduced_motion()
    motion.reduced_motion=reduced_motion
    state=Progress.load_saved()
    _build_ui()
    _load_level(0)

func _label(message:String,px:int,color:Color=INK)->Label:
    var item:=Label.new()
    item.text=message
    item.autowrap_mode=TextServer.AUTOWRAP_WORD_SMART
    item.add_theme_font_size_override("font_size",px)
    item.add_theme_color_override("font_color",color)
    return item

func _button_box(fill:Color,edge:Color)->StyleBoxFlat:
    var shape:=StyleBoxFlat.new()
    shape.bg_color=fill
    shape.set_corner_radius_all(14)
    shape.set_border_width_all(1)
    shape.border_color=edge
    shape.shadow_size=4
    shape.shadow_color=Color(0,0,0,0.3)
    shape.shadow_offset=Vector2(0,2)
    return shape

func _style(item:Button,primary:bool=false)->void:
    var edge:=ACCENT_PRESSED if primary else INK_MUTED
    item.add_theme_stylebox_override("normal",_button_box(ACCENT if primary else SURFACE_ALT,edge))
    item.add_theme_stylebox_override("hover",_button_box(ACCENT_HOVER if primary else Color("#5E36A0"),edge))
    item.add_theme_stylebox_override("pressed",_button_box(ACCENT_PRESSED if primary else SURFACE,edge))
    item.add_theme_stylebox_override("disabled",_button_box(SURFACE,Color("#6F5A9A")))
    var focus:=StyleBoxFlat.new()
    focus.bg_color=Color.TRANSPARENT
    focus.border_color=FOCUS
    focus.set_border_width_all(3)
    focus.set_corner_radius_all(14)
    item.add_theme_stylebox_override("focus",focus)
    var ink:=ACCENT_INK if primary else INK
    for mode in ["font_color","font_hover_color","font_pressed_color","font_focus_color"]:
        item.add_theme_color_override(mode,ink)
    item.add_theme_color_override("font_disabled_color",INK_MUTED)

func _button(message:String,callback:Callable,primary:bool=false)->Button:
    var item:=Button.new()
    item.text=message
    item.custom_minimum_size=Vector2(0,58)
    item.size_flags_horizontal=Control.SIZE_EXPAND_FILL
    item.add_theme_font_size_override("font_size",19)
    item.accessibility_name=message
    _style(item,primary)
    if callback.is_valid():
        item.pressed.connect(callback)
    return item

func _row()->HBoxContainer:
    var row:=HBoxContainer.new()
    row.add_theme_constant_override("separation",8)
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
    for token in [["margin_left",20],["margin_right",20],["margin_top",24],["margin_bottom",24]]:
        margin.add_theme_constant_override(token[0],token[1])
    var column:=VBoxContainer.new()
    margin.add_child(column)
    column.add_theme_constant_override("separation",13)
    column.add_child(_label("WORD SQUARE STUDIO / VERTICAL MEETS HORIZONTAL",12,ACCENT))
    column.add_child(_label("Make every letter agree.",28))
    column.add_child(_label("Arrange three different three-letter words from top row to bottom. Every column, read downward, must spell the same word as its corresponding row.",15,INK_MUTED))
    heading=_label("",16)
    column.add_child(heading)
    board_grid=GridContainer.new()
    board_grid.columns=3
    board_grid.add_theme_constant_override("h_separation",10)
    board_grid.add_theme_constant_override("v_separation",10)
    var board_center:=CenterContainer.new()
    column.add_child(board_center)
    board_center.add_child(board_grid)
    for k in 9:
        var tile:=Panel.new()
        tile.custom_minimum_size=Vector2(TILE_PX,TILE_PX)
        tile.mouse_filter=Control.MOUSE_FILTER_IGNORE
        var letter:=Label.new()
        letter.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
        letter.horizontal_alignment=HORIZONTAL_ALIGNMENT_CENTER
        letter.vertical_alignment=VERTICAL_ALIGNMENT_CENTER
        letter.add_theme_font_size_override("font_size",TILE_FONT)
        letter.add_theme_color_override("font_color",TILE_INK)
        letter.mouse_filter=Control.MOUSE_FILTER_IGNORE
        tile.add_child(letter)
        board_grid.add_child(tile)
        tiles.append(tile)
        tile_labels.append(letter)
    shown_words=VBoxContainer.new()
    shown_words.add_theme_constant_override("separation",6)
    column.add_child(shown_words)
    guidance=_label("",15,INK_MUTED)
    column.add_child(guidance)
    choices=GridContainer.new()
    choices.columns=2
    choices.add_theme_constant_override("h_separation",10)
    choices.add_theme_constant_override("v_separation",10)
    column.add_child(choices)
    for i in 3:
        var card:=_button("",Callable(),true)
        card.pressed.connect(_choose.bind(i+1,card))
        choices.add_child(card)
        choice_buttons.append(card)
    status_label=_label("",15)
    column.add_child(status_label)
    column.add_child(_label("Tap the three word cards in order. The rules compare rows and columns, not a hidden answer arrangement.",14,INK_MUTED))
    var commands:=_row()
    undo_button=_button("Undo",_undo)
    commands.add_child(undo_button)
    commands.add_child(_button("Restart",_restart))
    column.add_child(commands)
    var nav:=_row()
    prev_button=_button("Previous",_previous)
    next_button=_button("Next",_next,true)
    nav.add_child(prev_button)
    nav.add_child(next_button)
    column.add_child(nav)
    motion_toggle=CheckButton.new()
    motion_toggle.text="Reduce motion"
    motion_toggle.accessibility_name="Reduce motion"
    motion_toggle.button_pressed=reduced_motion
    motion_toggle.add_theme_font_size_override("font_size",17)
    motion_toggle.add_theme_color_override("font_color",INK)
    motion_toggle.toggled.connect(_set_reduced_motion)
    column.add_child(motion_toggle)
    column.add_child(_button("Copy challenge",_share))

func _load_level(direction:int=0)->void:
    level=Factory.make(int(state["level"]))
    history.clear()
    _refresh()
    if direction==0:
        motion.stagger_in(tiles)
    else:
        motion.slide_swap(board_grid,direction)

func _choose(index:int,source:Control=null)->void:
    var position:int=state["entries"].find(0)
    if position<0 or not Rules.options(level,state["entries"]).has(index):return
    history.append(state["entries"].duplicate())
    if history.size()>32:history.pop_front()
    if not Progress.update(state,position,index):
        history.pop_back()
        motion.shake(board_grid)
        return
    if source!=null:
        motion.press(source)
    for c in 3:
        motion.pop(tiles[position*3+c])
    _save()
    if Rules.complete(level,state["entries"]):
        motion.sweep(tiles,ACCENT)
        motion.celebrate(self,board_grid.get_global_rect().get_center(),[ACCENT,SUCCESS,TILE])
    _refresh()

func _undo()->void:
    if history.is_empty():return
    state["entries"]=history.pop_back()
    _save()
    _refresh()

func _restart()->void:
    history.clear()
    state["entries"]=Rules.blank(level)
    _save()
    _refresh()
    motion.stagger_in(tiles)

func _next()->void:
    if not Progress.can_advance(state):return
    Progress.set_level(state,int(state["level"])+1)
    _save()
    _load_level(1)

func _previous()->void:
    if int(state["level"])==0:return
    Progress.set_level(state,int(state["level"])-1)
    _save()
    _load_level(-1)

func _refresh()->void:
    heading.text="CHALLENGE %d OF %d  •  %s"%[int(state["level"])+1,Factory.COUNT,str(level["name"])]
    for child in shown_words.get_children():
        shown_words.remove_child(child)
        child.queue_free()
    var filled:int=0
    for id in range(3):
        var value:int=int(state["entries"][id])
        if value>0:filled+=1
        var word:String=str(level["words"][value-1]) if value>0 else "___"
        shown_words.add_child(_label("Row %d    %s"%[id+1,word],23))
    guidance.text="Rows = columns.  %d of 3 words placed."%filled
    var solved:bool=Rules.complete(level,state["entries"])
    for r in 3:
        var value:int=int(state["entries"][r])
        var word:String=str(level["words"][value-1]) if value>0 else ""
        for c in 3:
            var k:int=r*3+c
            tile_labels[k].text=word.substr(c,1)
            _set_tile(tiles[k],value>0,solved and r==c)
    var allowed:Array=Rules.options(level,state["entries"])
    for i in 3:
        var card:Button=choice_buttons[i]
        card.visible=allowed.has(i+1)
        card.text=str(level["words"][i])
        card.accessibility_name=card.text
    if allowed.is_empty() and not solved:
        guidance.text+="\nNo legal continuation. Undo your last choice."
    status_label.text=Rules.status(level,state["entries"])
    next_button.disabled=not Progress.can_advance(state)
    prev_button.disabled=int(state["level"])==0
    undo_button.disabled=history.is_empty()

## Cream letter tile, empty outlined cell, or gold when on the solved diagonal.
func _set_tile(tile:Control,filled:bool,gold:bool)->void:
    var box:=StyleBoxFlat.new()
    box.set_corner_radius_all(14)
    box.set_border_width_all(2)
    if not filled:
        box.bg_color=SURFACE_ALT
        box.border_color=INK_MUTED
    else:
        box.bg_color=ACCENT if gold else TILE
        box.border_color=ACCENT_PRESSED if gold else TILE_EDGE
        box.shadow_size=6
        box.shadow_color=Color(0,0,0,0.3)
        box.shadow_offset=Vector2(0,3)
    tile.add_theme_stylebox_override("panel",box)

func _set_reduced_motion(on:bool)->void:
    reduced_motion=on
    motion.reduced_motion=on
    if motion_toggle!=null:
        motion_toggle.set_pressed_no_signal(on)
    _save_reduced_motion()

func _load_reduced_motion()->bool:
    if not FileAccess.file_exists(SETTINGS_PATH):return false
    var file:=FileAccess.open(SETTINGS_PATH,FileAccess.READ)
    if file==null:return false
    var parser:=JSON.new()
    if parser.parse(file.get_as_text())!=OK or typeof(parser.data)!=TYPE_DICTIONARY:return false
    var value:Variant=parser.data.get("reduced_motion",false)
    return typeof(value)==TYPE_BOOL and value==true

func _save_reduced_motion()->void:
    var file:=FileAccess.open(SETTINGS_PATH,FileAccess.WRITE)
    if file==null:
        push_warning(TITLE+" could not save the motion setting.")
        return
    file.store_string(JSON.stringify({"reduced_motion":reduced_motion}))
    file.close()

func _save()->void:
    if not Progress.save(state):push_warning(TITLE+" failed to save progress locally.")

func _share()->void:
    var message:String=TITLE+": can you solve challenge %d?"%(int(state["level"])+1)
    if OS.has_feature("web"):
        var base:Variant=JavaScriptBridge.eval("window.location.origin + window.location.pathname",true)
        if typeof(base)==TYPE_STRING and str(base).begins_with("http"):
            message+="\n"+str(base)+"?level="+str(int(state["level"])+1)
    DisplayServer.clipboard_set(message)
    status_label.text="Challenge text prepared where clipboard access allows."

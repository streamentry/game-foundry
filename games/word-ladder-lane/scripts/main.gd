extends Control
## Word ladder: choose one-letter-step words from the shown cards to reach the goal.
const Factory=preload("res://scripts/level_factory.gd")
const Rules=preload("res://scripts/rules.gd")
const Progress=preload("res://scripts/progress.gd")
const Motion=preload("res://scripts/motion.gd")
const SETTINGS_PATH:String="user://settings.json"
const TITLE:String="Word Ladder Lane"
# Palette matched to icon.png: warm brown ground, wooden rails, cream tiles, gold change.
const GROUND:Color=Color("#66401F")
const RAIL:Color=Color("#C68B4F")
const FRAME:Color=Color("#4E2E15")
const SURFACE:Color=Color("#FBF3E0")
const SURFACE_ALT:Color=Color("#F1E3C4")
const INK:Color=Color("#3B2212")
const INK_MUTED:Color=Color("#6E4A2E")
const ACCENT:Color=Color("#EDB94F")
const ACCENT_INK:Color=Color("#2E1A0B")
const SUCCESS:Color=Color("#2F6B2A")
const SECONDARY:Color=Color("#84592F")
const TILE_SIZE:Vector2=Vector2(64,68)

var state:Dictionary={}
var level:Dictionary={}
var history:Array=[]
var motion:=Motion.new()
var bold:FontVariation
var board_box:PanelContainer
var shown_words:VBoxContainer
var goal_tiles:Array=[]
var ladder_rows:Array=[]
var choices:GridContainer
var choice_buttons:Dictionary={}
var reduce_toggle:CheckButton
var heading:Label
var status_label:Label
var guidance:Label
var next_button:Button
var prev_button:Button
var undo_button:Button

func _ready()->void:
    state=Progress.load_saved()
    bold=FontVariation.new()
    bold.base_font=ThemeDB.fallback_font
    bold.variation_embolden=0.8
    _build_ui()
    _load_settings()
    _load_level()

func _label(message:String,px:int,color:Color=SURFACE,weight_bold:bool=false)->Label:
    var item:=Label.new()
    item.text=message
    item.autowrap_mode=TextServer.AUTOWRAP_WORD_SMART
    item.add_theme_font_size_override("font_size",px)
    item.add_theme_color_override("font_color",color)
    if weight_bold:item.add_theme_font_override("font",bold)
    return item

func _style(item:Button,variant:String)->void:
    var fills:Dictionary={
        "primary":[ACCENT,Color("#F6C864"),Color("#D9A23E"),Color("#CDB98F")],
        "tile":[SURFACE,SURFACE_ALT,Color("#E8D5AE"),Color("#D8CBB0")],
        "secondary":[SECONDARY,Color("#9A6C3C"),Color("#6E4A24"),Color("#6F5A44")],
    }
    var palette:Array=fills[variant]
    var text_color:Color=ACCENT_INK if variant=="primary" else (INK if variant=="tile" else SURFACE)
    var edge:Color=Color("#C9B28A") if variant=="tile" else Color(0,0,0,0)
    var states:Array=["normal","hover","pressed","disabled"]
    for i in range(states.size()):
        var shape:=StyleBoxFlat.new()
        shape.bg_color=palette[i]
        shape.set_corner_radius_all(14)
        shape.set_border_width_all(2 if variant=="tile" else 0)
        shape.border_color=edge
        shape.shadow_size=3 if i==0 else 0
        shape.shadow_color=Color(0,0,0,0.28)
        shape.shadow_offset=Vector2(0,2)
        item.add_theme_stylebox_override(states[i],shape)
    var focus:=StyleBoxFlat.new()
    focus.bg_color=Color(0,0,0,0)
    focus.border_color=INK if variant=="tile" else SURFACE
    focus.set_border_width_all(3)
    focus.set_corner_radius_all(14)
    item.add_theme_stylebox_override("focus",focus)
    for mode in ["font_color","font_hover_color","font_pressed_color","font_disabled_color","font_focus_color"]:
        item.add_theme_color_override(mode,text_color)

func _button(message:String,callback:Callable,variant:String="secondary")->Button:
    var item:=Button.new()
    item.text=message
    item.custom_minimum_size=Vector2(0,58)
    item.size_flags_horizontal=Control.SIZE_EXPAND_FILL
    item.add_theme_font_size_override("font_size",19)
    item.accessibility_name=message
    _style(item,variant)
    item.pressed.connect(callback)
    return item

func _choice_button(word:String,index:int)->Button:
    var item:=_button(word,_choose.bind(index),"tile")
    item.custom_minimum_size=Vector2(0,64)
    item.add_theme_font_size_override("font_size",26)
    item.add_theme_font_override("font",bold)
    return item

func _row()->HBoxContainer:
    var row:=HBoxContainer.new()
    row.add_theme_constant_override("separation",8)
    return row

func _tile(letter:String,changed:bool,empty:bool)->PanelContainer:
    var tile:=PanelContainer.new()
    tile.custom_minimum_size=TILE_SIZE
    tile.size_flags_horizontal=Control.SIZE_EXPAND_FILL
    tile.mouse_filter=Control.MOUSE_FILTER_IGNORE
    var shape:=StyleBoxFlat.new()
    shape.bg_color=ACCENT if changed else (SURFACE_ALT if empty else SURFACE)
    shape.set_corner_radius_all(12)
    shape.set_border_width_all(2 if (changed or empty) else 0)
    shape.border_color=ACCENT_INK if changed else INK_MUTED
    shape.shadow_size=3
    shape.shadow_color=Color(0,0,0,0.25)
    shape.shadow_offset=Vector2(0,2)
    tile.add_theme_stylebox_override("panel",shape)
    var glyph:=_label(letter if not empty else "_",34,ACCENT_INK if changed else (INK_MUTED if empty else INK),true)
    glyph.autowrap_mode=TextServer.AUTOWRAP_OFF
    glyph.horizontal_alignment=HORIZONTAL_ALIGNMENT_CENTER
    glyph.vertical_alignment=VERTICAL_ALIGNMENT_CENTER
    tile.add_child(glyph)
    return tile

## Adds one captioned word row to the ladder and returns its four tiles.
## The letter that differs from `previous` is drawn gold; empty rows show blanks.
func _word_row(caption:String,word:String,previous:String,empty:bool)->Array:
    var row:=HBoxContainer.new()
    row.add_theme_constant_override("separation",6)
    var tag:=_label(caption,13,SURFACE_ALT,true)
    tag.custom_minimum_size=Vector2(56,0)
    tag.autowrap_mode=TextServer.AUTOWRAP_OFF
    tag.vertical_alignment=VERTICAL_ALIGNMENT_CENTER
    row.add_child(tag)
    var tiles:Array=[]
    for i in range(4):
        var letter:String=word.substr(i,1) if not empty else "_"
        var changed:bool=not empty and not previous.is_empty() and previous[i]!=word[i]
        var tile:=_tile(letter,changed,empty)
        row.add_child(tile)
        tiles.append(tile)
    row.accessibility_name="%s %s"%[caption,word if not empty else "not filled yet"]
    shown_words.add_child(row)
    return tiles

func _build_ui()->void:
    var bg:=ColorRect.new()
    bg.color=GROUND
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
    column.add_child(_label("WORD LADDER LANE / ONE LETTER AT A TIME",14,SURFACE_ALT,true))
    column.add_child(_label("A small change makes a journey.",28,SURFACE,true))
    column.add_child(_label("Reach the goal word in the exact number of moves. Each new four-letter word must differ from the last by exactly one letter.",15))
    heading=_label("",16,SURFACE,true)
    column.add_child(heading)
    board_box=PanelContainer.new()
    var frame:=StyleBoxFlat.new()
    frame.bg_color=FRAME
    frame.set_corner_radius_all(16)
    frame.border_color=RAIL
    frame.border_width_left=10
    frame.border_width_right=10
    frame.content_margin_left=14
    frame.content_margin_right=14
    frame.content_margin_top=14
    frame.content_margin_bottom=14
    frame.shadow_size=6
    frame.shadow_color=Color(0,0,0,0.3)
    frame.shadow_offset=Vector2(0,4)
    board_box.add_theme_stylebox_override("panel",frame)
    column.add_child(board_box)
    shown_words=VBoxContainer.new()
    shown_words.add_theme_constant_override("separation",10)
    board_box.add_child(shown_words)
    guidance=_label("",15)
    column.add_child(guidance)
    choices=GridContainer.new()
    choices.columns=2
    choices.add_theme_constant_override("h_separation",10)
    choices.add_theme_constant_override("v_separation",10)
    column.add_child(choices)
    status_label=_label("",16,SURFACE,true)
    column.add_child(status_label)
    column.add_child(_label("Choose only from the displayed word cards. Different valid words may lead to dead ends; Undo is always available.",14,SURFACE_ALT))
    var commands:=_row()
    undo_button=_button("Undo",_undo)
    commands.add_child(undo_button)
    commands.add_child(_button("Restart",_restart))
    column.add_child(commands)
    var nav:=_row()
    prev_button=_button("Previous",_previous)
    next_button=_button("Next",_next,"primary")
    nav.add_child(prev_button)
    nav.add_child(next_button)
    column.add_child(nav)
    column.add_child(_button("Copy challenge",_share))
    reduce_toggle=CheckButton.new()
    reduce_toggle.text="Reduce motion"
    reduce_toggle.custom_minimum_size=Vector2(0,48)
    reduce_toggle.accessibility_name="Reduce motion"
    reduce_toggle.add_theme_font_size_override("font_size",17)
    for mode in ["font_color","font_hover_color","font_pressed_color","font_focus_color"]:
        reduce_toggle.add_theme_color_override(mode,SURFACE)
    reduce_toggle.toggled.connect(_on_reduce_toggled)
    column.add_child(reduce_toggle)

func _load_settings()->void:
    if not FileAccess.file_exists(SETTINGS_PATH):return
    var file:=FileAccess.open(SETTINGS_PATH,FileAccess.READ)
    if file==null:return
    var parser:=JSON.new()
    if parser.parse(file.get_as_text())!=OK:return
    if typeof(parser.data)!=TYPE_DICTIONARY:return
    _set_reduced_motion(bool(parser.data.get("reduced_motion",false)),false)

func _save_settings()->void:
    var file:=FileAccess.open(SETTINGS_PATH,FileAccess.WRITE)
    if file==null:
        push_warning(TITLE+" could not save display settings.")
        return
    file.store_string(JSON.stringify({"reduced_motion":motion.reduced_motion}))
    file.close()

func _set_reduced_motion(on:bool,remember:bool=true)->void:
    motion.reduced_motion=on
    if reduce_toggle.button_pressed!=on:reduce_toggle.set_pressed_no_signal(on)
    if remember:_save_settings()

func _on_reduce_toggled(on:bool)->void:
    _set_reduced_motion(on,true)

func _load_level(direction:int=0)->void:
    level=Factory.make(int(state["level"]))
    history.clear()
    _refresh()
    _stagger_board()
    if direction!=0:motion.slide_swap(board_box,direction)

func _stagger_board()->void:
    var cells:Array=goal_tiles.duplicate()
    for row in ladder_rows:cells.append_array(row)
    cells.append_array(choices.get_children())
    motion.stagger_in(cells)

func _choose(index:int)->void:
    var position:int=state["entries"].find(0)
    if position<0:return
    if not Rules.options(level,state["entries"]).has(index):
        motion.shake(board_box)
        return
    if choice_buttons.has(index):motion.press(choice_buttons[index])
    history.append(state["entries"].duplicate())
    if history.size()>32:history.pop_front()
    if not Progress.update(state,position,index):
        history.pop_back()
        motion.shake(board_box)
        return
    _save()
    _refresh()
    _pop_row(position)
    if Rules.complete(level,state["entries"]):_celebrate()

func _pop_row(position:int)->void:
    # ladder_rows[0] is START, so entry `position` lives in row position+1.
    if position+1<ladder_rows.size():
        for tile in ladder_rows[position+1]:motion.pop(tile)

func _celebrate()->void:
    var cells:Array=[]
    for r in range(ladder_rows.size()-1,-1,-1):cells.append_array(ladder_rows[r])
    motion.sweep(cells,ACCENT)
    motion.celebrate(self,board_box.get_global_rect().get_center(),[ACCENT,SUCCESS,SURFACE])

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
    _stagger_board()

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

func _clear(node:Node)->void:
    for child in node.get_children():
        node.remove_child(child)
        child.queue_free()

func _refresh()->void:
    heading.text="CHALLENGE %d OF %d  •  %s"%[int(state["level"])+1,Factory.COUNT,str(level["name"])]
    _clear(shown_words)
    _clear(choices)
    choice_buttons.clear()
    ladder_rows.clear()
    goal_tiles=_word_row("GOAL",str(level["goal"]),"",false)
    var start:String=str(level["start"])
    ladder_rows.append(_word_row("START",start,"",false))
    var filled:int=0
    var previous:String=start
    for id in range(state["entries"].size()):
        var value:int=int(state["entries"][id])
        if value>0:
            filled+=1
            var word:String=str(level["words"][value-1])
            ladder_rows.append(_word_row("STEP %d"%(id+1),word,previous,false))
            previous=word
        else:
            ladder_rows.append(_word_row("STEP %d"%(id+1),"",previous,true))
    guidance.text="%d/%d moves used. The gold letter is the one that changed."%[filled,int(level["moves"])]
    var allowed:Array=Rules.options(level,state["entries"])
    for id in allowed:
        var label:String=str(level["words"][int(id)-1])
        var button:Button=_choice_button(label,int(id))
        choices.add_child(button)
        choice_buttons[int(id)]=button
    if allowed.is_empty() and not Rules.complete(level,state["entries"]):
        guidance.text+="\nNo legal continuation. Undo your last choice."
    status_label.text=Rules.status(level,state["entries"])
    if Rules.complete(level,state["entries"]):
        status_label.text="Solved! "+status_label.text
    next_button.disabled=not Progress.can_advance(state)
    prev_button.disabled=int(state["level"])==0
    undo_button.disabled=history.is_empty()

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

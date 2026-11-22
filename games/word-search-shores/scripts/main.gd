extends Control
## Original offline word search: straight horizontal/vertical/diagonal taps.
const Factory=preload("res://scripts/level_factory.gd")
const Rules=preload("res://scripts/rules.gd")
const Progress=preload("res://scripts/progress.gd")
const PAPER=Color("#F4F8FA")
const INK=Color("#243F50")
const ACCENT=Color("#296884")
const SOFT=Color("#DDE9F1")
const WHITE=Color("#FFFFFF")

var state:Dictionary={}
var level:Dictionary={}
var history:Array=[]
var selected:int=-1
var cells:Array=[]
var grid:GridContainer
var heading:Label
var result:Label
var targets:Label
var note:Label
var next_button:Button
var prev_button:Button
var undo_button:Button

func _ready()->void:
    state=Progress.load_saved()
    _build_ui()
    _load_level()

func _label(message:String,px:int,color:Color=INK)->Label:
    var item:=Label.new()
    item.text=message
    item.autowrap_mode=TextServer.AUTOWRAP_WORD_SMART
    item.add_theme_font_size_override("font_size",px)
    item.add_theme_color_override("font_color",color)
    return item

func _style(item:Button,bg:Color,fg:Color,active:bool=false)->void:
    var shape:=StyleBoxFlat.new()
    shape.bg_color=bg
    shape.border_color=ACCENT if active else Color("#BFCDD3")
    shape.set_border_width_all(3 if active else 1)
    shape.set_corner_radius_all(8)
    for mode in ["normal","hover","pressed","disabled"]:item.add_theme_stylebox_override(mode,shape)
    var focus:=StyleBoxFlat.new()
    focus.bg_color=Color.TRANSPARENT
    focus.border_color=ACCENT
    focus.set_border_width_all(3)
    focus.set_corner_radius_all(8)
    item.add_theme_stylebox_override("focus",focus)
    for mode in ["font_color","font_hover_color","font_pressed_color","font_disabled_color"]:
        item.add_theme_color_override(mode,fg)

func _button(message:String,action:Callable,primary:bool=false)->Button:
    var item:=Button.new()
    item.text=message
    item.accessibility_name=message
    item.custom_minimum_size=Vector2(0,54)
    item.size_flags_horizontal=Control.SIZE_EXPAND_FILL
    item.add_theme_font_size_override("font_size",17)
    _style(item,ACCENT if primary else SOFT,WHITE if primary else INK)
    item.pressed.connect(action)
    return item

func _row()->HBoxContainer:
    var row:=HBoxContainer.new()
    row.add_theme_constant_override("separation",9)
    return row

func _build_ui()->void:
    var bg:=ColorRect.new()
    bg.color=PAPER
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
    column.add_child(_label("Find each listed word horizontally, vertically or diagonally. Tap the first letter and then its last letter; either reading direction works.",15))
    heading=_label("",16)
    column.add_child(heading)
    var center:=CenterContainer.new()
    column.add_child(center)
    grid=GridContainer.new()
    grid.columns=6
    grid.add_theme_constant_override("h_separation",3)
    grid.add_theme_constant_override("v_separation",3)
    center.add_child(grid)
    targets=_label("",18)
    column.add_child(targets)
    note=_label("Choose a first and last letter. The line will be checked.",15)
    column.add_child(note)
    result=_label("",16)
    column.add_child(result)
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
    column.add_child(_label("Every target has one intended straight-line location. No timers, hidden penalties or account.",13))

func _load_level()->void:
    level=Factory.make(int(state["level"]))
    selected=-1
    history.clear()
    for child in grid.get_children():
        grid.remove_child(child)
        child.queue_free()
    cells.clear()
    for id in range(36):
        var item:=Button.new()
        item.text=str(level["letters"])[id]
        item.custom_minimum_size=Vector2(51,54)
        item.add_theme_font_size_override("font_size",20)
        item.focus_mode=Control.FOCUS_ALL
        item.accessibility_name="Row %d, column %d, letter %s"%[
            int(id/6)+1,id%6+1,item.text]
        item.pressed.connect(_tap.bind(id))
        grid.add_child(item)
        cells.append(item)
    _refresh()

func _tap(index:int)->void:
    if selected<0:
        selected=index
        _refresh()
        return
    var word_index:int=Rules.matched_word(level,selected,index)
    if word_index>=0 and int(state["entries"][word_index])==0:
        history.append(state["entries"].duplicate())
        if history.size()>24:history.pop_front()
        if Progress.update(state,word_index,1):
            _save()
            note.text="Found "+str(level["words"][word_index])+"! Choose another word."
        else:
            history.pop_back()
    else:
        note.text="Those endpoints do not identify a new target word. Try another starting letter."
    selected=-1
    _refresh()

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
    _refresh()

func _next()->void:
    if not Progress.can_advance(state):return
    Progress.set_level(state,int(state["level"])+1)
    _save()
    _load_level()

func _previous()->void:
    if int(state["level"])==0:return
    Progress.set_level(state,int(state["level"])-1)
    _save()
    _load_level()

func _refresh()->void:
    heading.text="SHORE %d OF %d  •  %s"%[int(state["level"])+1,Factory.COUNT,str(level["name"])]
    var highlighted:Dictionary={}
    var found:Array=[]
    for i in range(level["words"].size()):
        var word:String=str(level["words"][i])
        var completed:bool=int(state["entries"][i])==1
        found.append(("✓ " if completed else "○ ")+word)
        if completed:
            for cell in Rules.match_paths(level,word)[0]:
                highlighted[int(cell)]=true
    targets.text="    ".join(PackedStringArray(found))
    for i in range(36):
        var button:Button=cells[i]
        var current:bool=selected==i
        var solved:bool=highlighted.has(i)
        _style(button,ACCENT if solved else (Color("#BFDDE9") if current else WHITE),
            WHITE if solved else INK,current)
    result.text=Rules.status(level,state["entries"])
    undo_button.disabled=history.is_empty()
    next_button.disabled=not Progress.can_advance(state)
    prev_button.disabled=int(state["level"])==0

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

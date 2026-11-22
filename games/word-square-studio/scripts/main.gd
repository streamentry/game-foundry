extends Control
## Word path/word-square choice cards; original accepted words and rules.
const Factory=preload("res://scripts/level_factory.gd")
const Rules=preload("res://scripts/rules.gd")
const Progress=preload("res://scripts/progress.gd")
const MODE:String="square"
const TITLE:String="Word Square Studio"
const PAPER:Color=Color("#F9F3EA")
const INK:Color=Color("#4D3C41")
const ACCENT:Color=Color("#A06B6A")
const SOFT:Color=Color("#F1DFE2")
const WHITE:Color=Color("#FFFFFF")

var state:Dictionary={}
var level:Dictionary={}
var history:Array=[]
var shown_words:VBoxContainer
var choices:GridContainer
var heading:Label
var status_label:Label
var guidance:Label
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

func _style(item:Button,primary:bool=false)->void:
    var shape:=StyleBoxFlat.new()
    shape.bg_color=ACCENT if primary else SOFT
    shape.set_corner_radius_all(11)
    shape.border_color=Color("#A3B7A6")
    shape.set_border_width_all(1)
    for mode in ["normal","hover","pressed","disabled"]:item.add_theme_stylebox_override(mode,shape)
    var focus:=StyleBoxFlat.new()
    focus.bg_color=Color.TRANSPARENT
    focus.border_color=ACCENT
    focus.set_border_width_all(3)
    focus.set_corner_radius_all(11)
    item.add_theme_stylebox_override("focus",focus)
    for mode in ["font_color","font_hover_color","font_pressed_color","font_disabled_color"]:
        item.add_theme_color_override(mode,WHITE if primary else INK)

func _button(message:String,callback:Callable,primary:bool=false)->Button:
    var item:=Button.new()
    item.text=message
    item.custom_minimum_size=Vector2(0,58)
    item.size_flags_horizontal=Control.SIZE_EXPAND_FILL
    item.add_theme_font_size_override("font_size",19)
    item.accessibility_name=message
    _style(item,primary)
    item.pressed.connect(callback)
    return item

func _row()->HBoxContainer:
    var row:=HBoxContainer.new()
    row.add_theme_constant_override("separation",8)
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
    for token in [["margin_left",20],["margin_right",20],["margin_top",24],["margin_bottom",24]]:
        margin.add_theme_constant_override(token[0],token[1])
    var column:=VBoxContainer.new()
    margin.add_child(column)
    column.add_theme_constant_override("separation",13)
    column.add_child(_label("WORD SQUARE STUDIO / VERTICAL MEETS HORIZONTAL",12,ACCENT))
    column.add_child(_label("Make every letter agree.",28))
    column.add_child(_label("Arrange three different three-letter words from top row to bottom. Every column, read downward, must spell the same word as its corresponding row.",15))
    heading=_label("",16)
    column.add_child(heading)
    shown_words=VBoxContainer.new()
    shown_words.add_theme_constant_override("separation",6)
    column.add_child(shown_words)
    guidance=_label("",15)
    column.add_child(guidance)
    choices=GridContainer.new()
    choices.columns=2
    choices.add_theme_constant_override("h_separation",10)
    choices.add_theme_constant_override("v_separation",10)
    column.add_child(choices)
    status_label=_label("",15)
    column.add_child(status_label)
    column.add_child(_label("Tap the three word cards in order. The rules compare rows and columns, not a hidden answer arrangement.",14))
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
    column.add_child(_button("Copy challenge",_share))

func _load_level()->void:
    level=Factory.make(int(state["level"]))
    history.clear()
    _refresh()

func _choose(index:int)->void:
    var position:int=state["entries"].find(0)
    if position<0 or not Rules.options(level,state["entries"]).has(index):return
    history.append(state["entries"].duplicate())
    if history.size()>32:history.pop_front()
    if not Progress.update(state,position,index):
        history.pop_back()
        return
    _save()
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
    heading.text="CHALLENGE %d OF %d  •  %s"%[int(state["level"])+1,Factory.COUNT,str(level["name"])]
    for child in shown_words.get_children():
        shown_words.remove_child(child)
        child.queue_free()
    for child in choices.get_children():
        choices.remove_child(child)
        child.queue_free()
    var filled:int=0
    if MODE=="ladder":
        shown_words.add_child(_label("START  "+str(level["start"]),22,ACCENT))
        for id in range(state["entries"].size()):
            var value:int=int(state["entries"][id])
            if value>0:filled+=1
            var word:String=str(level["words"][value-1]) if value>0 else "____"
            shown_words.add_child(_label("Step %d     %s"%[id+1,word],21))
        guidance.text="GOAL: %s   •   %d/%d moves used"%[
            str(level["goal"]),filled,int(level["moves"])]
    else:
        for id in range(3):
            var value:int=int(state["entries"][id])
            if value>0:filled+=1
            var word:String=str(level["words"][value-1]) if value>0 else "___"
            shown_words.add_child(_label("Row %d    %s"%[id+1,word],23))
        guidance.text="Rows = columns.  %d of 3 words placed."%filled
    var allowed:Array=Rules.options(level,state["entries"])
    for id in allowed:
        var label:String=str(level["words"][int(id)-1])
        var button:Button=_button(label,_choose.bind(int(id)),true)
        choices.add_child(button)
    if allowed.is_empty() and not Rules.complete(level,state["entries"]):
        guidance.text+="\nNo legal continuation. Undo your last choice."
    status_label.text=Rules.status(level,state["entries"])
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

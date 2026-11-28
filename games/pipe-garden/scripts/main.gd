extends Control
## Independent touch-first original grid/movement research prototype.
const Factory=preload("res://scripts/level_factory.gd")
const Rules=preload("res://scripts/rules.gd")
const Progress=preload("res://scripts/progress.gd")
const Motion=preload("res://scripts/motion.gd")
const PipeTile=preload("res://scripts/pipe_tile.gd")
const MODE:String="pipe"
const TITLE:String="Pipe Garden"
const SETTINGS_PATH:String="user://settings.json"
const PAPER:Color=Color("#F3EEDD")
const INK:Color=Color("#1E3A2B")
const ACCENT:Color=Color("#A4572A")
const SOFT:Color=Color("#E6DCC2")
const WHITE:Color=Color("#FBF8EE")
const SUCCESS:Color=Color("#2F6B3A")
const EDGE:Color=Color("#C9BE9F")
const GLOW:Color=Color("#F2C9A0")

var motion:=Motion.new()
var reduce_toggle:CheckButton
var state:Dictionary={}
var level:Dictionary={}
var history:Array=[]
var cells:Array=[]
var grid:GridContainer
var heading:Label
var status_label:Label
var progress_label:Label
var next_button:Button
var prev_button:Button
var undo_button:Button

func _ready()->void:
    state=Progress.load_saved()
    motion.reduced_motion=_load_reduced_motion()
    _build_ui()
    _load_level()

func _load_reduced_motion()->bool:
    if not FileAccess.file_exists(SETTINGS_PATH):return false
    var file:=FileAccess.open(SETTINGS_PATH,FileAccess.READ)
    if file==null:return false
    var parser:=JSON.new()
    if parser.parse(file.get_as_text())!=OK or typeof(parser.data)!=TYPE_DICTIONARY:return false
    return bool(parser.data.get("reduced_motion",false))

func _save_reduced_motion()->void:
    var file:=FileAccess.open(SETTINGS_PATH,FileAccess.WRITE)
    if file==null:
        push_warning(TITLE+" failed to save the reduce-motion setting.")
        return
    file.store_string(JSON.stringify({"reduced_motion":motion.reduced_motion}))
    file.close()

func _on_reduce_motion_toggled(on:bool)->void:
    motion.reduced_motion=on
    _save_reduced_motion()

func _label(value:String,px:int,color:Color=INK)->Label:
    var item:=Label.new()
    item.text=value
    item.autowrap_mode=TextServer.AUTOWRAP_WORD_SMART
    item.add_theme_font_size_override("font_size",px)
    item.add_theme_color_override("font_color",color)
    return item

func _shape(bg:Color,border:Color,border_px:int,shadow:int)->StyleBoxFlat:
    var shape:=StyleBoxFlat.new()
    shape.bg_color=bg
    shape.border_color=border
    shape.set_border_width_all(border_px)
    shape.set_corner_radius_all(12)
    shape.shadow_size=shadow
    shape.shadow_color=Color(0.1,0.2,0.15,0.14)
    shape.shadow_offset=Vector2(0,2 if shadow>0 else 0)
    return shape

func _style(item:Button,bg:Color,ink:Color,active:bool=false)->void:
    var border:=ACCENT if active else EDGE
    var border_px:=3 if active else 1
    var lighter:=bg.lightened(0.04)
    var darker:=bg.darkened(0.06)
    item.add_theme_stylebox_override("normal",_shape(bg,border,border_px,2))
    item.add_theme_stylebox_override("hover",_shape(lighter,border,border_px,2))
    item.add_theme_stylebox_override("pressed",_shape(darker,ACCENT,3,0))
    item.add_theme_stylebox_override("disabled",_shape(SOFT,EDGE,1,0))
    var focus:=StyleBoxFlat.new()
    focus.bg_color=Color.TRANSPARENT
    focus.border_color=INK
    focus.set_border_width_all(3)
    focus.set_corner_radius_all(12)
    item.add_theme_stylebox_override("focus",focus)
    item.add_theme_color_override("font_color",ink)
    item.add_theme_color_override("font_hover_color",ink)
    item.add_theme_color_override("font_pressed_color",ink)
    item.add_theme_color_override("font_focus_color",ink)
    item.add_theme_color_override("font_disabled_color",Color("#4F5E4C"))

func _button(title:String,cb:Callable,primary:bool=false)->Button:
    var button:=Button.new()
    button.text=title
    button.accessibility_name=title
    button.custom_minimum_size=Vector2(0,54)
    button.size_flags_horizontal=Control.SIZE_EXPAND_FILL
    button.add_theme_font_size_override("font_size",17)
    _style(button,ACCENT if primary else SOFT,WHITE if primary else INK)
    button.pressed.connect(cb)
    return button

func _row()->HBoxContainer:
    var row:=HBoxContainer.new()
    row.add_theme_constant_override("separation",8)
    return row

func _build_ui()->void:
    var background:=ColorRect.new()
    background.color=PAPER
    background.mouse_filter=Control.MOUSE_FILTER_IGNORE
    add_child(background)
    background.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
    var scroll:=ScrollContainer.new()
    add_child(scroll)
    scroll.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
    scroll.horizontal_scroll_mode=ScrollContainer.SCROLL_MODE_DISABLED
    var margin:=MarginContainer.new()
    scroll.add_child(margin)
    margin.size_flags_horizontal=Control.SIZE_EXPAND_FILL
    for kv in [["margin_left",16],["margin_right",16],["margin_top",20],["margin_bottom",24]]:
        margin.add_theme_constant_override(kv[0],kv[1])
    var column:=VBoxContainer.new()
    margin.add_child(column)
    column.add_theme_constant_override("separation",12)
    column.add_child(_label("PIPE GARDEN / ONE CONTINUOUS FLOW",12,ACCENT))
    column.add_child(_label("Turn nine pieces into one path.",28))
    column.add_child(_label("Rotate each pipe tile so every open port joins the next. The two end pieces form one continuous route through all nine tiles. No leaks or disconnected loops.",15))
    heading=_label("",16)
    column.add_child(heading)
    var center:=CenterContainer.new()
    column.add_child(center)
    grid=GridContainer.new()
    grid.add_theme_constant_override("h_separation",4)
    grid.add_theme_constant_override("v_separation",4)
    center.add_child(grid)
    progress_label=_label("",15)
    column.add_child(progress_label)
    status_label=_label("",15)
    column.add_child(status_label)
    column.add_child(_label("Tap any tile to rotate. Straight tiles have two orientations; corners and ends have four.",14))
    var actions:=_row()
    undo_button=_button("Undo",_undo)
    actions.add_child(undo_button)
    actions.add_child(_button("Restart",_restart))
    column.add_child(actions)
    reduce_toggle=CheckButton.new()
    reduce_toggle.text="Reduce motion"
    reduce_toggle.accessibility_name="Reduce motion"
    reduce_toggle.custom_minimum_size=Vector2(0,48)
    reduce_toggle.add_theme_font_size_override("font_size",16)
    reduce_toggle.add_theme_color_override("font_color",INK)
    reduce_toggle.add_theme_color_override("font_hover_color",INK)
    reduce_toggle.add_theme_color_override("font_focus_color",INK)
    reduce_toggle.button_pressed=motion.reduced_motion
    reduce_toggle.toggled.connect(_on_reduce_motion_toggled)
    column.add_child(reduce_toggle)
    var nav:=_row()
    prev_button=_button("Previous",_previous)
    next_button=_button("Next",_next,true)
    nav.add_child(prev_button)
    nav.add_child(next_button)
    column.add_child(nav)
    column.add_child(_button("Copy challenge",_share))
    column.add_child(_label("No timer, account, ad SDK or cloud data. Real-phone accessibility testing remains necessary.",13))

func _new_clue(text_value:String)->Button:
    var button:=Button.new()
    button.text=text_value
    button.disabled=true
    button.custom_minimum_size=Vector2(53,53)
    button.add_theme_font_size_override("font_size",13)
    _style(button,SOFT,INK)
    return button

func _load_level()->void:
    level=Factory.make(int(state["level"]))
    history.clear()
    for old in grid.get_children():
        grid.remove_child(old)
        old.queue_free()
    cells.clear()
    var n:int=int(level["size"])
    grid.columns=n+1 if MODE=="nonogram" else n
    if MODE=="nonogram":
        grid.add_child(_new_clue("↘"))
        for col in range(5):
            grid.add_child(_new_clue(" ".join(PackedStringArray(level["cols"][col].map(func(k):return str(k))))))
    for row in range(n):
        if MODE=="nonogram":
            grid.add_child(_new_clue(" ".join(PackedStringArray(level["rows"][row].map(func(k):return str(k))))))
        for col in range(n):
            var id:int=row*n+col
            var item:Button=PipeTile.new() if MODE=="pipe" else Button.new()
            item.custom_minimum_size=Vector2(55,55) if MODE=="nonogram" else (
                Vector2(96,96) if n==3 else Vector2(76,76))
            item.add_theme_font_size_override("font_size",19 if MODE in ["nonogram","akari"] else 25)
            item.focus_mode=Control.FOCUS_ALL
            item.pressed.connect(_tap.bind(id))
            grid.add_child(item)
            cells.append(item)
    _refresh()
    motion.stagger_in(cells)

func _tap(index:int)->void:
    if MODE=="akari" and int(Rules.blank(level)[index])==3:return
    history.append(state["entries"].duplicate())
    if history.size()>60:history.pop_front()
    var success:bool=false
    var was_solved:bool=Rules.complete(level,state["entries"])
    if MODE=="pipe":
        success=Progress.rotate(state,index)
    elif MODE=="sliding":
        success=false
    else:
        var next:int=(int(state["entries"][index])+1)%3
        success=false
    if not success:
        history.pop_back()
        return
    _save()
    _refresh()
    if MODE=="pipe":
        (cells[index] as PipeTile).turn(motion.reduced_motion)
    motion.press(cells[index])
    motion.pop(cells[index])
    if not was_solved and Rules.complete(level,state["entries"]):
        motion.sweep(cells,GLOW)
        motion.celebrate(self,grid.get_global_rect().get_center(),[ACCENT,SUCCESS,SOFT])

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
    motion.stagger_in(cells)

func _next()->void:
    if not Progress.can_advance(state):return
    Progress.set_level(state,int(state["level"])+1)
    _save()
    _load_level()
    motion.slide_swap(grid,1)

func _previous()->void:
    if int(state["level"])==0:return
    Progress.set_level(state,int(state["level"])-1)
    _save()
    _load_level()
    motion.slide_swap(grid,-1)

func _refresh()->void:
    heading.text="PUZZLE %d OF %d  •  %s"%[int(state["level"])+1,Factory.COUNT,str(level["name"])]
    var filled:int=0
    var water:Array=Rules.connected(level,state["entries"]) if MODE=="pipe" else []
    for i in range(cells.size()):
        var item:Button=cells[i]
        var value:int=int(state["entries"][i])
        var tile:String=""
        var background:Color=WHITE
        var foreground:Color=INK
        var fixed:bool=false
        var highlighted:bool=false
        if MODE=="nonogram":
            tile="■" if value==1 else ("×" if value==2 else "·")
            if value==1:
                background=ACCENT
                foreground=WHITE
        elif MODE=="akari":
            fixed=value==3
            if fixed:
                for pair in level["walls"]:
                    if int(pair[0])==i:
                        tile="▣ "+str(pair[1])
                background=INK
                foreground=WHITE
            else:
                tile="☀" if value==1 else ("×" if value==2 else "·")
                if value==1:
                    background=Color("#F4D073")
                elif false:
                    background=SOFT
        elif MODE=="pipe":
            var kind:String=str(level["types"][i])
            (item as PipeTile).set_piece(kind,value,bool(water[i]))
            tile={"E":"end pipe","I":"straight pipe","L":"corner pipe"}.get(kind,"pipe")+(", carries water" if water[i] else ", not connected yet")
            background=SOFT if kind=="E" else WHITE
        elif MODE=="sliding":
            tile=" " if value==0 else str(value)
            if value==0:
                background=SOFT
                fixed=true
            else:
                highlighted=false
                background=WHITE if highlighted else SOFT
                fixed=not highlighted
        if value>0 and not fixed:filled+=1
        item.text="" if MODE=="pipe" else tile
        item.disabled=fixed
        item.accessibility_name="Row %d, column %d, %s"%[
            int(i/int(level["size"]))+1,i%int(level["size"])+1,tile.replace("\n"," ")]
        item.tooltip_text=item.accessibility_name
        _style(item,background,foreground,highlighted)
    if MODE=="nonogram":
        progress_label.text="%d/25 squares marked"%filled
    elif MODE=="akari":
        progress_label.text="%d lamps currently placed"%0
    elif MODE=="sliding":
        progress_label.text="%d moves recorded. Verified minimum: %d"%[history.size(),int(level["minimum"])]
    else:
        progress_label.text="Orient all nine tiles into one route."
    status_label.text=Rules.status(level,state["entries"])
    undo_button.disabled=history.is_empty()
    prev_button.disabled=int(state["level"])==0
    next_button.disabled=not Progress.can_advance(state)

func _save()->void:
    if not Progress.save(state):push_warning(TITLE+" failed to save local progress.")

func _share()->void:
    var challenge:String=TITLE+": play puzzle %d"%(int(state["level"])+1)
    if OS.has_feature("web"):
        var base:Variant=JavaScriptBridge.eval("window.location.origin + window.location.pathname",true)
        if typeof(base)==TYPE_STRING and str(base).begins_with("http"):
            challenge+="\n"+str(base)+"?level="+str(int(state["level"])+1)
    DisplayServer.clipboard_set(challenge)
    status_label.text="Challenge text prepared where clipboard access allows."

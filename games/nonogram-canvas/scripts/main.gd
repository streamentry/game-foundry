extends Control
## Independent touch-first original grid/movement research prototype.
const Factory=preload("res://scripts/level_factory.gd")
const Rules=preload("res://scripts/rules.gd")
const Progress=preload("res://scripts/progress.gd")
const Motion=preload("res://scripts/motion.gd")
const MODE:String="nonogram"
const TITLE:String="Nonogram Canvas"
const SETTINGS_PATH:String="user://settings.json"
const PAPER:Color=Color("#F7F0E1")
const SURFACE:Color=Color("#FFFBF2")
const SOFT:Color=Color("#EAE1CB")
const INK:Color=Color("#27343D")
const INK_MUTED:Color=Color("#5B6670")
const ACCENT:Color=Color("#C4452F")
const ACCENT_HOVER:Color=Color("#B23E29")
const ACCENT_PRESSED:Color=Color("#A83A27")
const WHITE:Color=Color("#FFFFFF")
const SUCCESS:Color=Color("#2F6B5A")
const ERROR:Color=Color("#A32A2A")
const FOCUS:Color=Color("#2B5F8C")
const BORDER:Color=Color("#6F7A83")
const GLOW:Color=Color("#F7C9A6")

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
var reduce_check:CheckButton
var motion:=Motion.new()

func _ready()->void:
    state=Progress.load_saved()
    _build_ui()
    _apply_reduced_motion(_load_settings())
    _load_level()
    motion.stagger_in(cells)

func _label(value:String,px:int,color:Color=INK)->Label:
    var item:=Label.new()
    item.text=value
    item.autowrap_mode=TextServer.AUTOWRAP_WORD_SMART
    item.add_theme_font_size_override("font_size",px)
    item.add_theme_color_override("font_color",color)
    return item

func _shape(bg:Color,active:bool,mode:String,raised:bool,hover:Color,pressed:Color)->StyleBoxFlat:
    var shape:=StyleBoxFlat.new()
    var tone:=bg
    if mode=="hover":
        tone=hover if hover.a>0.0 else bg.lightened(0.06)
    elif mode=="pressed":
        tone=pressed if pressed.a>0.0 else bg.darkened(0.08)
    elif mode=="disabled" and raised:
        tone=PAPER
    shape.bg_color=tone
    var edge:Color=ACCENT if (bg==ACCENT and mode!="disabled") else BORDER
    shape.border_color=FOCUS if active else edge
    shape.set_border_width_all(3 if active else 1)
    shape.set_corner_radius_all(12)
    if raised and mode in ["normal","hover"]:
        shape.shadow_color=Color(0.0,0.0,0.0,0.14)
        shape.shadow_size=3
        shape.shadow_offset=Vector2(0.0,2.0)
    return shape

func _style(item:Button,bg:Color,ink:Color,active:bool=false,raised:bool=false,hover:Color=Color(0,0,0,0),pressed:Color=Color(0,0,0,0))->void:
    for mode in ["normal","hover","pressed","disabled"]:
        item.add_theme_stylebox_override(mode,_shape(bg,active,mode,raised,hover,pressed))
    var focus:=StyleBoxFlat.new()
    focus.bg_color=Color.TRANSPARENT
    focus.border_color=FOCUS
    focus.set_border_width_all(3)
    focus.set_corner_radius_all(12)
    item.add_theme_stylebox_override("focus",focus)
    for mode in ["font_color","font_hover_color","font_pressed_color","font_disabled_color"]:
        item.add_theme_color_override(mode,ink)

func _button(title:String,cb:Callable,primary:bool=false)->Button:
    var button:=Button.new()
    button.text=title
    button.accessibility_name=title
    button.custom_minimum_size=Vector2(0,54)
    button.size_flags_horizontal=Control.SIZE_EXPAND_FILL
    button.add_theme_font_size_override("font_size",17)
    if primary:
        _style(button,ACCENT,WHITE,false,true,ACCENT_HOVER,ACCENT_PRESSED)
    else:
        _style(button,SOFT,INK,false,true)
    button.add_theme_color_override("font_disabled_color",INK_MUTED)
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
    column.add_child(_label("NONOGRAM CANVAS / PICTURE IN NUMBERS",14,INK_MUTED))
    column.add_child(_label("Let the picture appear.",28))
    column.add_child(_label("Each row and column lists consecutive runs of filled squares. Separate runs by at least one empty square. Mark every cell filled or empty.",15))
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
    column.add_child(_label("Tap a square: filled → crossed empty → unknown. All ten clue lines must match.",14))
    var actions:=_row()
    undo_button=_button("Undo",_undo)
    actions.add_child(undo_button)
    actions.add_child(_button("Restart",_restart))
    column.add_child(actions)
    var nav:=_row()
    prev_button=_button("Previous",_previous)
    next_button=_button("Next",_next,true)
    nav.add_child(prev_button)
    nav.add_child(next_button)
    column.add_child(nav)
    column.add_child(_button("Copy challenge",_share))
    reduce_check=CheckButton.new()
    reduce_check.text="Reduce motion"
    reduce_check.accessibility_name="Reduce motion"
    reduce_check.custom_minimum_size=Vector2(0,48)
    reduce_check.add_theme_font_size_override("font_size",17)
    for mode in ["font_color","font_hover_color","font_pressed_color","font_focus_color"]:
        reduce_check.add_theme_color_override(mode,INK)
    reduce_check.toggled.connect(_set_reduced_motion)
    column.add_child(reduce_check)
    column.add_child(_label("No timer, account, ad SDK or cloud data. Real-phone accessibility testing remains necessary.",14))

func _format_clue(values:Array)->String:
    var parts:PackedStringArray=[]
    for value in values:
        parts.append(str(int(value)))
    return " ".join(parts)

func _new_clue(text_value:String)->Button:
    var button:=Button.new()
    button.text=text_value
    button.disabled=true
    button.custom_minimum_size=Vector2(53,53)
    button.add_theme_font_size_override("font_size",15)
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
            grid.add_child(_new_clue(_format_clue(level["cols"][col])))
    for row in range(n):
        if MODE=="nonogram":
            grid.add_child(_new_clue(_format_clue(level["rows"][row])))
        for col in range(n):
            var id:int=row*n+col
            var item:=Button.new()
            item.custom_minimum_size=Vector2(55,55) if MODE=="nonogram" else (
                Vector2(96,96) if n==3 else Vector2(76,76))
            item.add_theme_font_size_override("font_size",22 if MODE=="nonogram" else (19 if MODE=="akari" else 25))
            item.focus_mode=Control.FOCUS_ALL
            item.pressed.connect(_tap.bind(id))
            grid.add_child(item)
            cells.append(item)
    _refresh()

func _tap(index:int)->void:
    if MODE=="akari" and int(Rules.blank(level)[index])==3:return
    history.append(state["entries"].duplicate())
    if history.size()>60:history.pop_front()
    var solved_before:bool=Rules.complete(level,state["entries"])
    var lines_before:Array=_line_states(state["entries"])
    motion.press(cells[index])
    var success:bool=false
    if MODE=="pipe":
        success=false
    elif MODE=="sliding":
        success=false
    else:
        var next:int=(int(state["entries"][index])+1)%3
        success=Progress.update(state,index,next)
    if not success:
        history.pop_back()
        motion.shake(cells[index])
        return
    motion.pop(cells[index])
    _save()
    _refresh()
    _celebrate_changes(solved_before,lines_before)

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

func _pipe_mark(kind:String,orientation:int)->String:
    if kind=="E":return ["↑","→","↓","←"][orientation-1]
    if kind=="I":return ["│","─"][orientation-1]
    if kind=="L":return ["└","┌","┐","┘"][orientation-1]
    return "?"

func _refresh()->void:
    heading.text="PUZZLE %d OF %d  •  %s"%[int(state["level"])+1,Factory.COUNT,str(level["name"])]
    var filled:int=0
    for i in range(cells.size()):
        var item:Button=cells[i]
        var value:int=int(state["entries"][i])
        var tile:String=""
        var background:Color=SURFACE
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
            tile=_pipe_mark(kind,value)+"\n"+("END" if kind=="E" else ("LINE" if kind=="I" else "BEND"))
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
        item.text=tile
        item.disabled=fixed
        item.accessibility_name="Row %d, column %d, %s"%[
            int(i/int(level["size"]))+1,i%int(level["size"])+1,tile.replace("\n"," ")]
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
    status_label.add_theme_color_override("font_color",SUCCESS if Rules.complete(level,state["entries"]) else INK)
    undo_button.disabled=history.is_empty()
    prev_button.disabled=int(state["level"])==0
    next_button.disabled=not Progress.can_advance(state)

func _line_done(entries:Array,index:int,vertical:bool)->bool:
    var seq:Array=[]
    for j in range(5):
        seq.append(int(entries[j*5+index]) if vertical else int(entries[index*5+j]))
    if seq.has(0):return false
    var clue:Array=level["cols"][index] if vertical else level["rows"][index]
    return Rules.runs(seq)==clue

## Ten booleans: rows 0-4, then columns 0-4. True when that line matches its clue.
func _line_states(entries:Array)->Array:
    var out:Array=[]
    for i in range(5):out.append(_line_done(entries,i,false))
    for i in range(5):out.append(_line_done(entries,i,true))
    return out

## Line lock: a row or column that newly matches its clue sweeps its own squares.
## Solving sweeps the whole board and bursts once.
func _celebrate_changes(solved_before:bool,before:Array)->void:
    if Rules.complete(level,state["entries"]):
        if not solved_before:
            motion.celebrate(self,grid.get_global_rect().get_center(),[ACCENT,SUCCESS,SURFACE])
            motion.sweep(cells,GLOW)
        return
    var after:Array=_line_states(state["entries"])
    for k in range(10):
        if bool(after[k]) and not bool(before[k]):
            var line_cells:Array=[]
            for j in range(5):
                line_cells.append(cells[k*5+j] if k<5 else cells[j*5+(k-5)])
            motion.sweep(line_cells,GLOW)

func _load_settings()->bool:
    if not FileAccess.file_exists(SETTINGS_PATH):return false
    var file:=FileAccess.open(SETTINGS_PATH,FileAccess.READ)
    if file==null:return false
    var parser:=JSON.new()
    if parser.parse(file.get_as_text())!=OK:return false
    if typeof(parser.data)!=TYPE_DICTIONARY:return false
    var data:Dictionary=parser.data
    return bool(data.get("reduced_motion",false))

func _save_settings()->bool:
    var file:=FileAccess.open(SETTINGS_PATH,FileAccess.WRITE)
    if file==null:return false
    file.store_string(JSON.stringify({"reduced_motion":motion.reduced_motion}))
    file.close()
    return true

func _apply_reduced_motion(on:bool)->void:
    motion.reduced_motion=on
    reduce_check.set_pressed_no_signal(on)

func _set_reduced_motion(on:bool)->void:
    _apply_reduced_motion(on)
    if not _save_settings():push_warning(TITLE+" failed to save motion setting.")

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

extends Control
## Independent touch-first original grid/movement research prototype.
const Factory=preload("res://scripts/level_factory.gd")
const Rules=preload("res://scripts/rules.gd")
const Progress=preload("res://scripts/progress.gd")
const Motion=preload("res://scripts/motion.gd")
const MODE:String="akari"
const TITLE:String="Akari Evening"
const SETTINGS_PATH:String="user://settings.json"
## Palette drawn from the app icon: night navy, amber tiles, lamp gold. See DESIGN.md section 2.
const BG:Color=Color("#F6F1E3")
const SURFACE:Color=Color("#FFFFFF")
const SURFACE_ALT:Color=Color("#F1DFA6")
const INK:Color=Color("#15034C")
const INK_MUTED:Color=Color("#4A4670")
const ACCENT:Color=Color("#AD7B3C")
const ACCENT_INK:Color=Color("#15034C")
const LAMP:Color=Color("#FCD454")
const SUCCESS:Color=Color("#1F6B45")
const ERROR:Color=Color("#A3262E")
const FOCUS:Color=Color("#15034C")
const DISABLED_BG:Color=Color("#E4DCC7")

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
    motion.reduced_motion=_load_reduced_motion()
    _build_ui()
    _load_level()

func _load_reduced_motion()->bool:
    if not FileAccess.file_exists(SETTINGS_PATH):return false
    var file:=FileAccess.open(SETTINGS_PATH,FileAccess.READ)
    if file==null:return false
    var parser:=JSON.new()
    if parser.parse(file.get_as_text())!=OK:return false
    if typeof(parser.data)!=TYPE_DICTIONARY:return false
    var data:Dictionary=parser.data
    var value:Variant=data.get("reduced_motion",false)
    return typeof(value)==TYPE_BOOL and bool(value)

func _save_reduced_motion(on:bool)->void:
    var file:=FileAccess.open(SETTINGS_PATH,FileAccess.WRITE)
    if file==null:
        push_warning(TITLE+" could not save the reduce-motion setting.")
        return
    file.store_string(JSON.stringify({"reduced_motion":on}))
    file.close()

func _on_reduce_toggled(on:bool)->void:
    motion.reduced_motion=on
    _save_reduced_motion(on)

func _label(value:String,px:int,color:Color=INK)->Label:
    var item:=Label.new()
    item.text=value
    item.autowrap_mode=TextServer.AUTOWRAP_WORD_SMART
    item.add_theme_font_size_override("font_size",px)
    item.add_theme_color_override("font_color",color)
    return item

func _shape(fill:Color,edge:Color,width:int,shadow:int)->StyleBoxFlat:
    var box:=StyleBoxFlat.new()
    box.bg_color=fill
    box.border_color=edge
    box.set_border_width_all(width)
    box.set_corner_radius_all(11)
    if shadow>0:
        box.shadow_size=shadow
        box.shadow_color=Color(INK,0.18)
    return box

func _style(item:Button,bg:Color,ink:Color,active:bool=false,raised:bool=false,disabled_fill:Color=DISABLED_BG)->void:
    var edge:Color=ACCENT if active else Color(INK,0.25)
    var width:int=3 if active else 1
    var shadow:int=3 if raised else 0
    item.add_theme_stylebox_override("normal",_shape(bg,edge,width,shadow))
    item.add_theme_stylebox_override("hover",_shape(bg.lerp(SURFACE,0.08),edge,width,shadow))
    item.add_theme_stylebox_override("pressed",_shape(bg.lerp(INK,0.08),edge,width,0))
    item.add_theme_stylebox_override("disabled",_shape(disabled_fill,edge,width,0))
    var focus:=StyleBoxFlat.new()
    focus.bg_color=Color.TRANSPARENT
    focus.border_color=FOCUS
    focus.set_border_width_all(3)
    focus.set_corner_radius_all(11)
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
    _style(button,ACCENT if primary else SURFACE_ALT,ACCENT_INK if primary else INK,false,true)
    button.pressed.connect(cb)
    return button

func _row()->HBoxContainer:
    var row:=HBoxContainer.new()
    row.add_theme_constant_override("separation",8)
    return row

func _build_ui()->void:
    var background:=ColorRect.new()
    background.color=BG
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
    column.add_child(_label("AKARI EVENING / LAMPS IN THE DARK",12,INK_MUTED))
    column.add_child(_label("Light every open square.",28))
    column.add_child(_label("Place lamps to illuminate all white squares horizontally and vertically. Walls stop light. No two lamps may see each other; numbered walls demand that many adjacent lamps.",15))
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
    column.add_child(_label("Tap a white square: lamp → cross note → blank. Wall numbers are fixed. Cross notes are optional.",14))
    reduce_check=CheckButton.new()
    reduce_check.text="Reduce motion"
    reduce_check.accessibility_name="Reduce motion"
    reduce_check.custom_minimum_size=Vector2(0,48)
    reduce_check.add_theme_font_size_override("font_size",17)
    reduce_check.add_theme_color_override("font_color",INK)
    reduce_check.set_pressed_no_signal(motion.reduced_motion)
    reduce_check.toggled.connect(_on_reduce_toggled)
    column.add_child(reduce_check)
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
    column.add_child(_label("No timer, account, ad SDK or cloud data. Real-phone accessibility testing remains necessary.",13))

func _new_clue(text_value:String)->Button:
    var button:=Button.new()
    button.text=text_value
    button.disabled=true
    button.custom_minimum_size=Vector2(53,53)
    button.add_theme_font_size_override("font_size",13)
    _style(button,SURFACE_ALT,INK)
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
            var item:=Button.new()
            item.custom_minimum_size=Vector2(55,55) if MODE=="nonogram" else (
                Vector2(96,96) if n==3 else Vector2(76,76))
            item.add_theme_font_size_override("font_size",19 if MODE in ["nonogram","akari"] else 25)
            item.focus_mode=Control.FOCUS_ALL
            item.pressed.connect(_tap.bind(id))
            grid.add_child(item)
            cells.append(item)
    _refresh()
    motion.stagger_in(cells)

func _lit_cells()->Array:
    var out:Array=[]
    for p in range(cells.size()):
        if Rules.illuminated(level,state["entries"],p):out.append(p)
    return out

func _newly_lit(before:Array)->Array:
    var out:Array=[]
    for p in _lit_cells():
        if not before.has(p):out.append(cells[p])
    return out

func _tap(index:int)->void:
    if MODE=="akari" and int(Rules.blank(level)[index])==3:return
    var cell:Button=cells[index]
    motion.press(cell)
    history.append(state["entries"].duplicate())
    if history.size()>60:history.pop_front()
    var was_complete:bool=Rules.complete(level,state["entries"])
    var lit_before:Array=_lit_cells()
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
        motion.shake(cell)
        return
    _save()
    _refresh()
    motion.pop(cell)
    if int(state["entries"][index])==1:
        motion.sweep(_newly_lit(lit_before),LAMP)
    if not was_complete and Rules.complete(level,state["entries"]):
        motion.sweep(cells,LAMP)
        motion.celebrate(self,grid.get_global_rect().get_center(),[ACCENT,LAMP,BG])

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
                foreground=SURFACE
        elif MODE=="akari":
            fixed=value==3
            if fixed:
                for pair in level["walls"]:
                    if int(pair[0])==i:
                        tile="▣ "+str(pair[1])
                background=INK
                foreground=SURFACE
            else:
                tile="☀" if value==1 else ("×" if value==2 else "·")
                if value==1:
                    background=LAMP
                elif Rules.illuminated(level,state["entries"],i):
                    background=SURFACE_ALT
        elif MODE=="pipe":
            var kind:String=str(level["types"][i])
            tile=_pipe_mark(kind,value)+"\n"+("END" if kind=="E" else ("LINE" if kind=="I" else "BEND"))
            background=SURFACE_ALT if kind=="E" else SURFACE
        elif MODE=="sliding":
            tile=" " if value==0 else str(value)
            if value==0:
                background=SURFACE_ALT
                fixed=true
            else:
                highlighted=false
                background=SURFACE if highlighted else SURFACE_ALT
                fixed=not highlighted
        if value>0 and not fixed:filled+=1
        item.text=tile
        item.disabled=fixed
        item.accessibility_name="Row %d, column %d, %s"%[
            int(i/int(level["size"]))+1,i%int(level["size"])+1,tile.replace("\n"," ")]
        _style(item,background,foreground,highlighted,false,background)
    if MODE=="nonogram":
        progress_label.text="%d/25 squares marked"%filled
    elif MODE=="akari":
        progress_label.text="%d lamps currently placed"%Rules.lamps(state["entries"]).size()
    elif MODE=="sliding":
        progress_label.text="%d moves recorded. Verified minimum: %d"%[history.size(),int(level["minimum"])]
    else:
        progress_label.text="Orient all nine tiles into one route."
    var status:String=Rules.status(level,state["entries"])
    var done:bool=Rules.complete(level,state["entries"])
    var broken:bool=not Rules.no_collision(level,state["entries"]) or status.begins_with("A numbered")
    status_label.text=("✓ " if done else ("⚠ " if broken else ""))+status
    status_label.add_theme_color_override("font_color",SUCCESS if done else (ERROR if broken else INK))
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

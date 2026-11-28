extends Control
## Independent touch-first original grid/movement research prototype.
const Factory=preload("res://scripts/level_factory.gd")
const Rules=preload("res://scripts/rules.gd")
const Progress=preload("res://scripts/progress.gd")
const Motion=preload("res://scripts/motion.gd")
const MODE:String="sliding"
const TITLE:String="Sliding Tiles"
const SETTINGS_PATH:String="user://settings.json"
const SLIDE_SECONDS:float=0.14
# Warm walnut and wood palette sampled from icon.png (see DESIGN.md).
const BG:Color=Color("#3C2418")
const SURFACE:Color=Color("#54361E")
const SURFACE_ALT:Color=Color("#5C3A22")
const INK:Color=Color("#F6E7C8")
const ACCENT:Color=Color("#E0A458")
const DARK:Color=Color("#3C2418")
const TILE:Color=Color("#EACCA2")
const TILE_INK:Color=Color("#784E2A")
const EDGE:Color=Color("#7A5238")
const FOCUS:Color=Color("#FFD27A")
const SUCCESS:Color=Color("#B5D08A")
const ALERT:Color=Color("#F2977F")
# Legacy names still referenced by the inactive mode branches below.
const PAPER:Color=BG
const SOFT:Color=SURFACE_ALT
const WHITE:Color=TILE

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
var tray:PanelContainer
var reduce_motion:CheckButton
var settings:Dictionary={}
var motion:=Motion.new()

func _ready()->void:
    state=Progress.load_saved()
    settings=_load_settings()
    motion.reduced_motion=bool(settings["reduced_motion"])
    _build_ui()
    _load_level()

func _label(value:String,px:int,color:Color=INK)->Label:
    var item:=Label.new()
    item.text=value
    item.autowrap_mode=TextServer.AUTOWRAP_WORD_SMART
    item.add_theme_font_size_override("font_size",px)
    item.add_theme_color_override("font_color",color)
    return item

func _style(item:Button,bg:Color,ink:Color,active:bool=false)->void:
    var shape:=StyleBoxFlat.new()
    shape.bg_color=bg
    shape.border_color=ACCENT if active else EDGE
    shape.set_border_width_all(3 if active else 1)
    shape.set_corner_radius_all(12)
    var raised:bool=bg!=BG
    shape.shadow_color=Color(0,0,0,0.35)
    shape.shadow_size=4 if raised else 0
    shape.shadow_offset=Vector2(0,2)
    var hover:=shape.duplicate() as StyleBoxFlat
    hover.bg_color=bg.lightened(0.06)
    var pressed:=shape.duplicate() as StyleBoxFlat
    pressed.bg_color=bg.darkened(0.08)
    pressed.shadow_size=1 if raised else 0
    pressed.shadow_offset=Vector2(0,1)
    item.add_theme_stylebox_override("normal",shape)
    item.add_theme_stylebox_override("hover",hover)
    item.add_theme_stylebox_override("pressed",pressed)
    item.add_theme_stylebox_override("disabled",shape)
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
    _style(button,ACCENT if primary else SURFACE_ALT,DARK if primary else INK)
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
    column.add_child(_label("SLIDING TILES / A SIMPLE JOURNEY",12,ACCENT))
    column.add_child(_label("Bring the numbers home.",28))
    column.add_child(_label("Slide a tile into the empty space until the board reads 1 through 8, with the empty space at bottom right. Only orthogonal moves are allowed.",15))
    heading=_label("",16)
    column.add_child(heading)
    var center:=CenterContainer.new()
    column.add_child(center)
    grid=GridContainer.new()
    grid.add_theme_constant_override("h_separation",4)
    grid.add_theme_constant_override("v_separation",4)
    tray=PanelContainer.new()
    tray.add_theme_stylebox_override("panel",_tray_style())
    center.add_child(tray)
    tray.add_child(grid)
    progress_label=_label("",15)
    column.add_child(progress_label)
    status_label=_label("",15)
    column.add_child(status_label)
    column.add_child(_label("Tap only a numbered tile beside the blank. Every starting arrangement is reachable and has a verified shortest solution.",14))
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
    reduce_motion=CheckButton.new()
    reduce_motion.text="Reduce motion"
    reduce_motion.accessibility_name="Reduce motion"
    reduce_motion.custom_minimum_size=Vector2(0,48)
    reduce_motion.add_theme_font_size_override("font_size",17)
    for mode in ["font_color","font_hover_color","font_pressed_color","font_focus_color"]:
        reduce_motion.add_theme_color_override(mode,INK)
    reduce_motion.button_pressed=motion.reduced_motion
    reduce_motion.toggled.connect(_on_reduce_motion_toggled)
    column.add_child(reduce_motion)
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

func _tap(index:int)->void:
    if MODE=="akari" and int(Rules.blank(level)[index])==3:return
    history.append(state["entries"].duplicate())
    if history.size()>60:history.pop_front()
    var success:bool=false
    if MODE=="pipe":
        success=false
    elif MODE=="sliding":
        success=Progress.slide(state,index)
    else:
        var next:int=(int(state["entries"][index])+1)%3
        success=false
    if not success:
        history.pop_back()
        motion.shake(cells[index])
        return
    _save()
    _refresh()
    if MODE=="sliding":
        _animate_move(index,history.back())

func _undo()->void:
    if history.is_empty():return
    var current:Array=state["entries"]
    state["entries"]=history.pop_back()
    _save()
    _refresh()
    if MODE=="sliding":
        _slide_between(state["entries"].find(0),current.find(0))

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
    motion.slide_swap(tray,1)

func _previous()->void:
    if int(state["level"])==0:return
    Progress.set_level(state,int(state["level"])-1)
    _save()
    _load_level()
    motion.slide_swap(tray,-1)

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
            tile=_pipe_mark(kind,value)+"\n"+("END" if kind=="E" else ("LINE" if kind=="I" else "BEND"))
            background=SOFT if kind=="E" else WHITE
        elif MODE=="sliding":
            tile=" " if value==0 else str(value)
            if value==0:
                background=BG
                fixed=true
            else:
                highlighted=Rules.adjacent(i,state["entries"].find(0))
                background=TILE
                foreground=TILE_INK
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

func _tray_style()->StyleBoxFlat:
    var shape:=StyleBoxFlat.new()
    shape.bg_color=SURFACE
    shape.set_corner_radius_all(16)
    shape.set_content_margin_all(12)
    shape.shadow_color=Color(0,0,0,0.3)
    shape.shadow_size=8
    shape.shadow_offset=Vector2(0,4)
    return shape

func _load_settings()->Dictionary:
    var result:Dictionary={"reduced_motion":false}
    if not FileAccess.file_exists(SETTINGS_PATH):return result
    var file:=FileAccess.open(SETTINGS_PATH,FileAccess.READ)
    if file==null:return result
    var parser:=JSON.new()
    if parser.parse(file.get_as_text())!=OK:return result
    if typeof(parser.data)==TYPE_DICTIONARY:
        result["reduced_motion"]=(parser.data as Dictionary).get("reduced_motion",false)==true
    return result

func _save_settings()->void:
    var file:=FileAccess.open(SETTINGS_PATH,FileAccess.WRITE)
    if file==null:
        push_warning(TITLE+" could not save display settings.")
        return
    file.store_string(JSON.stringify({"reduced_motion":motion.reduced_motion}))
    file.close()

func _on_reduce_motion_toggled(on:bool)->void:
    motion.reduced_motion=on
    _save_settings()

func _row_done(entries:Array,row:int)->bool:
    for col in range(3):
        if int(entries[row*3+col])!=int(Rules.GOAL[row*3+col]):return false
    return true

## Visual only: the moved tile starts at its old slot and tweens into the new one.
## Game state already holds the final board; reduced motion snaps at once.
func _slide_between(from_slot:int,to_slot:int)->void:
    if from_slot<0 or to_slot<0 or from_slot==to_slot:return
    var n:int=int(level["size"])
    var pitch:=Vector2(cells[0].custom_minimum_size.x+grid.get_theme_constant("h_separation"),
        cells[0].custom_minimum_size.y+grid.get_theme_constant("v_separation"))
    var from_pos:=Vector2(float(from_slot%n),float(int(from_slot/float(n))))*pitch
    var to_pos:=Vector2(float(to_slot%n),float(int(to_slot/float(n))))*pitch
    _slide_tile(cells[to_slot],from_pos-to_pos)

func _slide_tile(cell:Button,from_offset:Vector2)->void:
    cell.offset_transform_enabled=true
    if cell.has_meta(&"_m_slide"):
        var old:Variant=cell.get_meta(&"_m_slide")
        if old is Tween and (old as Tween).is_valid():(old as Tween).kill()
    if motion.reduced_motion or not cell.is_inside_tree():
        cell.offset_transform_position=Vector2.ZERO
        return
    cell.offset_transform_position=from_offset
    var tw:=cell.create_tween()
    cell.set_meta(&"_m_slide",tw)
    tw.tween_property(cell,"offset_transform_position",Vector2.ZERO,SLIDE_SECONDS) \
        .set_trans(Tween.TRANS_CUBIC).set_ease(Tween.EASE_OUT)

func _animate_move(index:int,prior:Array)->void:
    var entries:Array=state["entries"]
    var dest:int=prior.find(0)
    motion.press(cells[index])
    motion.pop(cells[dest])
    _slide_between(index,dest)
    if Rules.complete(level,entries):
        motion.sweep(cells,FOCUS)
        motion.celebrate(self,tray.get_global_rect().get_center(),[ACCENT,SUCCESS,TILE])
        return
    for row in range(3):
        if not _row_done(prior,row) and _row_done(entries,row):
            motion.sweep([cells[row*3],cells[row*3+1],cells[row*3+2]],FOCUS)

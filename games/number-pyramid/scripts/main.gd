extends Control
## Standalone numeric/letter puzzle: distinct rules and art direction per game.
const Factory=preload("res://scripts/level_factory.gd")
const Rules=preload("res://scripts/rules.gd")
const Progress=preload("res://scripts/progress.gd")
const Motion=preload("res://scripts/motion.gd")
const SETTINGS_PATH:String="user://settings.json"
const MODE:String="pyramid"
const TITLE:String="Number Pyramid"
const PAPER:Color=Color("#F7EDDA")
const SURFACE:Color=Color("#FCEDCC")
const SURFACE_ALT:Color=Color("#EBCFA6")
const INK:Color=Color("#451901")
const INK_MUTED:Color=Color("#6B4A33")
const ACCENT:Color=Color("#A65A2A")
const ACCENT_INK:Color=Color("#FFFFFF")
const SUCCESS:Color=Color("#2F6B3A")
const ERROR:Color=Color("#9E3E40")
const FOCUS:Color=Color("#6E3210")
const GLOW:Color=Color("#F6D9A8")

var motion:=Motion.new()
var reduce_toggle:CheckButton
var board_items:Array=[]
var state:Dictionary={}
var level:Dictionary={}
var history:Array=[]
var selected:int=-1
var cells:Array=[]
var upper_cells:Dictionary={}
var grid:GridContainer
var holder:VBoxContainer
var heading:Label
var feedback:Label
var progress_label:Label
var preview:Label
var next_button:Button
var prev_button:Button
var undo_button:Button

func _ready()->void:
    state=Progress.load_saved()
    _load_settings()
    _build_ui()
    _load_level()

func _load_settings()->void:
    motion.reduced_motion=false
    if not FileAccess.file_exists(SETTINGS_PATH):return
    var file:=FileAccess.open(SETTINGS_PATH,FileAccess.READ)
    if file==null:return
    var parser:=JSON.new()
    if parser.parse(file.get_as_text())!=OK or typeof(parser.data)!=TYPE_DICTIONARY:return
    motion.reduced_motion=bool(parser.data.get("reduced_motion",false))

func _save_settings()->void:
    var file:=FileAccess.open(SETTINGS_PATH,FileAccess.WRITE)
    if file==null:
        push_warning(TITLE+" could not save its display settings.")
        return
    file.store_string(JSON.stringify({"reduced_motion":motion.reduced_motion}))
    file.close()

func set_reduced_motion(on:bool)->void:
    motion.reduced_motion=on
    if reduce_toggle!=null and reduce_toggle.button_pressed!=on:
        reduce_toggle.set_pressed_no_signal(on)
    _save_settings()

func _on_reduce_toggled(on:bool)->void:
    set_reduced_motion(on)

func _label(value:String,px:int,color:Color=INK)->Label:
    var item:=Label.new()
    item.text=value
    item.autowrap_mode=TextServer.AUTOWRAP_WORD_SMART
    item.add_theme_font_size_override("font_size",px)
    item.add_theme_color_override("font_color",color)
    return item

func _tint(background:Color,mode:String)->Color:
    match mode:
        "hover":return background.lerp(Color.WHITE,0.08)
        "pressed":return background.darkened(0.07)
        _:return background

func _style(item:Button,background:Color,color:Color,focused:bool=false)->void:
    for mode in ["normal","hover","pressed","disabled"]:
        var shape:=StyleBoxFlat.new()
        shape.bg_color=_tint(background,mode)
        shape.border_color=ACCENT if focused else Color("#CDB18F")
        shape.set_border_width_all(3 if focused else 1)
        shape.set_corner_radius_all(12)
        var pressed:bool=mode=="pressed"
        shape.shadow_color=Color(0.27,0.1,0.0,0.06 if pressed else 0.16)
        shape.shadow_size=1 if pressed else 4
        shape.shadow_offset=Vector2(0,1 if pressed else 2)
        item.add_theme_stylebox_override(mode,shape)
    item.add_theme_stylebox_override("focus",_focus_ring())
    for mode in ["font_color","font_hover_color","font_pressed_color","font_disabled_color"]:
        item.add_theme_color_override(mode,color)

func _focus_ring()->StyleBoxFlat:
    var ring:=StyleBoxFlat.new()
    ring.bg_color=Color.TRANSPARENT
    ring.border_color=FOCUS
    ring.set_border_width_all(3)
    ring.set_corner_radius_all(12)
    return ring

func _button(value:String,action:Callable,primary:bool=false)->Button:
    var item:=Button.new()
    item.text=value
    item.accessibility_name=value
    item.custom_minimum_size=Vector2(0,54)
    item.size_flags_horizontal=Control.SIZE_EXPAND_FILL
    item.add_theme_font_size_override("font_size",17)
    _style(item,ACCENT if primary else SURFACE_ALT,ACCENT_INK if primary else INK)
    item.pressed.connect(action)
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
    for kv in [["margin_left",16],["margin_right",16],["margin_top",22],["margin_bottom",24]]:
        margin.add_theme_constant_override(kv[0],kv[1])
    var column:=VBoxContainer.new()
    margin.add_child(column)
    column.add_theme_constant_override("separation",12)
    column.add_child(_label("NUMBER PYRAMID / STONES OF SUM",12,INK_MUTED))
    column.add_child(_label("Build from the foundation.",27))
    column.add_child(_label("Every upper stone is the sum of the two below it. Use the scattered printed clues to reconstruct the four bottom numbers (1–9).",15))
    heading=_label("",16)
    column.add_child(heading)
    var centered:=CenterContainer.new()
    column.add_child(centered)
    holder=VBoxContainer.new()
    holder.add_theme_constant_override("separation",7)
    centered.add_child(holder)
    grid=GridContainer.new()
    grid.columns=7 if MODE=="vowel" else 4
    grid.add_theme_constant_override("h_separation",3)
    grid.add_theme_constant_override("v_separation",4)
    holder.add_child(grid)
    preview=_label("",16,INK_MUTED)
    column.add_child(preview)
    progress_label=_label("",15)
    column.add_child(progress_label)
    feedback=_label("",15)
    column.add_child(feedback)
    var keypad:=GridContainer.new()
    keypad.columns=5 if MODE=="vowel" else (3 if MODE=="pyramid" else 4)
    keypad.size_flags_horizontal=Control.SIZE_EXPAND_FILL
    keypad.add_theme_constant_override("h_separation",7)
    keypad.add_theme_constant_override("v_separation",7)
    var labels:Array=["A","E","I","O","U"] if MODE=="vowel" else (
        ["1","2","3","4","5","6","7","8","9"] if MODE=="pyramid" else ["1","2","3","4"])
    for i in range(labels.size()):
        keypad.add_child(_button(str(labels[i]),_enter.bind(i+1),true))
    column.add_child(keypad)
    column.add_child(_label("The top pyramid numbers are calculated from your entries; printed clues stay fixed. There is no timer or penalty.",13))
    reduce_toggle=CheckButton.new()
    reduce_toggle.text="Reduce motion"
    reduce_toggle.accessibility_name="Reduce motion"
    reduce_toggle.custom_minimum_size=Vector2(0,48)
    reduce_toggle.add_theme_font_size_override("font_size",16)
    reduce_toggle.add_theme_color_override("font_color",INK)
    reduce_toggle.add_theme_color_override("font_hover_color",INK)
    reduce_toggle.add_theme_color_override("font_pressed_color",INK)
    reduce_toggle.add_theme_color_override("font_focus_color",INK)
    reduce_toggle.add_theme_stylebox_override("focus",_focus_ring())
    reduce_toggle.set_pressed_no_signal(motion.reduced_motion)
    reduce_toggle.toggled.connect(_on_reduce_toggled)
    column.add_child(reduce_toggle)
    var actions:=_row()
    undo_button=_button("Undo",_undo)
    actions.add_child(undo_button)
    actions.add_child(_button("Erase",_enter.bind(0)))
    actions.add_child(_button("Restart",_restart))
    column.add_child(actions)
    var navigation:=_row()
    prev_button=_button("Previous",_previous)
    next_button=_button("Next",_next,true)
    navigation.add_child(prev_button)
    navigation.add_child(next_button)
    column.add_child(navigation)
    column.add_child(_button("Copy challenge",_share))

func _cell(label:String,size_px:int,action:Callable)->Button:
    var item:=Button.new()
    item.text=label
    item.custom_minimum_size=Vector2(76,76) if MODE!="vowel" else Vector2(43,51)
    item.add_theme_font_size_override("font_size",size_px)
    item.accessibility_name=label
    if action.is_valid():item.pressed.connect(action)
    return item

func _load_level()->void:
    level=Factory.make(int(state["level"]))
    history.clear()
    selected=-1
    for child in holder.get_children():
        holder.remove_child(child)
        child.queue_free()
    grid=GridContainer.new()
    grid.columns=7 if MODE=="vowel" else 4
    grid.add_theme_constant_override("h_separation",3)
    grid.add_theme_constant_override("v_separation",4)
    holder.add_child(grid)
    cells.clear()
    upper_cells.clear()
    board_items.clear()
    if MODE=="renzoku":
        var fixed:Dictionary={}
        for i in range(16):
            var item:Button=_cell("",21,_select.bind(i))
            item.disabled=fixed.has(i)
            grid.add_child(item)
            cells.append(item)
            if selected<0 and not fixed.has(i):selected=i
    elif MODE=="vowel":
        var vowel_index:int=0
        for ch in str(level["pattern"]):
            var item:Button=_cell("",18,Callable())
            if ch=="_":
                item.pressed.connect(_select.bind(vowel_index))
                cells.append(item)
                vowel_index+=1
            else:
                item.disabled=true
                item.text=ch if ch!=" " else " "
                _style(item,Color.TRANSPARENT,INK)
            grid.add_child(item)
        selected=0
    else:
        for r in [[9],[7,8],[4,5,6],[0,1,2,3]]:
            var row:=_row()
            row.alignment=BoxContainer.ALIGNMENT_CENTER
            holder.add_child(row)
            for pos in r:
                var item:Button=_cell("",19,_select.bind(pos))
                item.custom_minimum_size=Vector2(69,65)
                if int(pos)>=4:item.disabled=true
                row.add_child(item)
                upper_cells[int(pos)]=item
                if int(pos)<4:cells.append(item)
                board_items.append(item)
        selected=Rules.blank(level).find(0)
        if selected<0:selected=0
    if board_items.is_empty():
        board_items=cells.duplicate()
    _refresh()
    motion.stagger_in(board_items)

func _select(index:int)->void:
    if MODE=="pyramid" and (index>=4 or Rules.clues(level).has(index)):return
    if MODE=="renzoku" and {}.has(index):return
    if index>=0 and index<cells.size():motion.press(cells[index])
    selected=index
    _refresh()

func _enter(value:int)->void:
    if selected<0 or selected>=state["entries"].size():return
    if int(state["entries"][selected])==value:return
    history.append(state["entries"].duplicate())
    if history.size()>60:history.pop_front()
    var cell:Control=cells[selected] if selected<cells.size() else null
    if not Progress.update(state,selected,value):
        history.pop_back()
        if cell!=null:motion.shake(cell)
        return
    _save()
    _refresh()
    if cell!=null:motion.pop(cell)
    if Rules.complete(level,state["entries"]):
        _celebrate()

func _celebrate()->void:
    # Foundation first, then the wave climbs the pyramid row by row.
    var wave:Array=cells.duplicate()
    for row in [[7,8],[4,5,6],[9]]:
        for pos in row:wave.append(upper_cells[pos])
    motion.sweep(wave,GLOW)
    motion.celebrate(self,holder.get_global_rect().get_center(),[ACCENT,SUCCESS,SURFACE_ALT])

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
    motion.stagger_in(board_items)

func _next()->void:
    if not Progress.can_advance(state):return
    Progress.set_level(state,int(state["level"])+1)
    _save()
    _load_level()
    motion.slide_swap(holder,1)

func _previous()->void:
    if int(state["level"])==0:return
    Progress.set_level(state,int(state["level"])-1)
    _save()
    _load_level()
    motion.slide_swap(holder,-1)

func _refresh()->void:
    heading.text="PUZZLE %d OF %d  •  %s"%[int(state["level"])+1,Factory.COUNT,str(level["name"])]
    var filled:int=0
    for value in state["entries"]:
        if int(value)>0:filled+=1
    if MODE=="renzoku":
        var dots:Dictionary={}
        for pair in level["dots"]:
            dots["%d-%d"%[int(pair[0]),int(pair[1])]]=true
        for i in range(16):
            var item:Button=cells[i]
            var marks:PackedStringArray=[]
            if i%4<3 and dots.has("%d-%d"%[i,i+1]):marks.append("→•")
            if i<12 and dots.has("%d-%d"%[i,i+4]):marks.append("↓•")
            var val:int=int(state["entries"][i])
            item.text=(str(val) if val>0 else "·")+("\n"+" ".join(marks) if not marks.is_empty() else "")
            item.accessibility_name="Row %d column %d, %s%s"%[
                int(i/4)+1,i%4+1,str(val) if val>0 else "empty",", consecutive neighbors "+str(marks)]
            _style(item,SURFACE_ALT if item.disabled else SURFACE,INK,i==selected)
        progress_label.text="%d/16 numbers entered"%filled
        preview.text="Selected row %d, column %d. Dots indicate difference 1."%[int(selected/4)+1,selected%4+1]
    elif MODE=="vowel":
        for i in range(cells.size()):
            var item:Button=cells[i]
            var id:int=int(state["entries"][i])
            item.text="AEIOU"[id-1] if id>0 else "_"
            item.accessibility_name="Missing vowel %d, %s"%[i+1,item.text]
            _style(item,Color("#F5E1BD"),INK,i==selected)
        progress_label.text="%d/%d vowel slots filled"%[filled,level["size"]]
        preview.text=""
    else:
        var all:Array=Rules.values(state["entries"])
        var known:Dictionary=Rules.clues(level)
        for i in range(10):
            var item:Button=upper_cells[i]
            var number:int=int(all[i])
            var visible:bool=true
            if i>=4:
                if i<=6:
                    visible=int(state["entries"][i-4])>0 and int(state["entries"][i-3])>0
                elif i<=8:
                    var a:int=i-7
                    visible=int(state["entries"][a])>0 and int(state["entries"][a+1])>0 and int(state["entries"][a+2])>0
                else:
                    visible=not state["entries"].has(0)
            item.text=str(known[i]) if known.has(i) else (str(number) if visible else "?")
            item.disabled=i>=4 or known.has(i)
            item.accessibility_name="Stone %d, %s%s"%[i,item.text," clue" if known.has(i) else ""]
            _style(item,SURFACE_ALT if known.has(i) else SURFACE,INK,i==selected)
        progress_label.text="%d/4 bottom stones filled"%filled
        preview.text="The pyramid uses adjacent sums; numbers above update as you build."
    feedback.text=Rules.status(level,state["entries"])
    feedback.add_theme_color_override("font_color",SUCCESS if Rules.complete(level,state["entries"]) else INK)
    undo_button.disabled=history.is_empty()
    next_button.disabled=not Progress.can_advance(state)
    prev_button.disabled=int(state["level"])==0

func _save()->void:
    if not Progress.save(state):push_warning(TITLE+" could not save its offline progress.")

func _share()->void:
    var message:String=TITLE+": try puzzle %d"%(int(state["level"])+1)
    if OS.has_feature("web"):
        var base:Variant=JavaScriptBridge.eval("window.location.origin + window.location.pathname",true)
        if typeof(base)==TYPE_STRING and str(base).begins_with("http"):
            message+="\n"+str(base)+"?level="+str(int(state["level"])+1)
    DisplayServer.clipboard_set(message)
    feedback.text="Challenge text prepared (clipboard permission may vary)."

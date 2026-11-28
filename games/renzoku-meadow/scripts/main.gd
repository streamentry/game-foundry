extends Control
## Standalone numeric/letter puzzle: distinct rules and art direction per game.
const Factory=preload("res://scripts/level_factory.gd")
const Rules=preload("res://scripts/rules.gd")
const Progress=preload("res://scripts/progress.gd")
const Motion=preload("res://scripts/motion.gd")
const MODE:String="renzoku"
const TITLE:String="Renzoku Meadow"
const SETTINGS_PATH:String="user://settings.json"
# Tokens (see DESIGN.md): paper/surface/soft from the icon's cream tiles and olive field.
const PAPER:Color=Color("#F6F1DE")
const SURFACE:Color=Color("#FBF6E7")
const SOFT:Color=Color("#E6E9C9")
const INK:Color=Color("#262C24")
const INK_MUTED:Color=Color("#4F5643")
const ACCENT:Color=Color("#5C6B25")
const WHITE:Color=Color("#FFFFFF")
const SUCCESS:Color=Color("#33601F")
const ERROR:Color=Color("#9A3324")
const GOLD:Color=Color("#F2CC45")
const EDGE:Color=Color("#BFC4A2")

var motion:=Motion.new()
var state:Dictionary={}
var level:Dictionary={}
var history:Array=[]
var selected:int=-1
var cells:Array=[]
var upper_cells:Dictionary={}
var grid:GridContainer
var board:Control
var overlay:Control
const BAR_LEN:float=21.0
const BAR_THICK:float=7.0
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
    motion.reduced_motion=_read_reduced_motion()
    _build_ui()
    _load_level()

static func _read_reduced_motion()->bool:
    if not FileAccess.file_exists(SETTINGS_PATH):return false
    var file:=FileAccess.open(SETTINGS_PATH,FileAccess.READ)
    if file==null:return false
    var parser:=JSON.new()
    if parser.parse(file.get_as_text())!=OK or typeof(parser.data)!=TYPE_DICTIONARY:return false
    var flag:Variant=parser.data.get("reduced_motion",false)
    return typeof(flag)==TYPE_BOOL and bool(flag)

func _on_reduce_motion_toggled(on:bool)->void:
    motion.reduced_motion=on
    var file:=FileAccess.open(SETTINGS_PATH,FileAccess.WRITE)
    if file==null:
        push_warning(TITLE+" could not save its motion setting.")
        return
    file.store_string(JSON.stringify({"reduced_motion":on}))
    file.close()

func _label(value:String,px:int,color:Color=INK)->Label:
    var item:=Label.new()
    item.text=value
    item.autowrap_mode=TextServer.AUTOWRAP_WORD_SMART
    item.add_theme_font_size_override("font_size",px)
    item.add_theme_color_override("font_color",color)
    return item

## `focused` here means the cell is the selected one (state, not keyboard focus).
func _style(item:Button,background:Color,color:Color,focused:bool=false)->void:
    var raised:=not item.disabled
    var normal:=_tile(background,ACCENT if focused else EDGE,3 if focused else 1,raised)
    var hover:=_tile(background.lerp(WHITE,0.35) if background!=SURFACE else WHITE,ACCENT,3 if focused else 2,raised)
    var pressed:=_tile(background.darkened(0.08),ACCENT,3,false)
    var disabled:=_tile(background,EDGE if not focused else ACCENT,1,false)
    item.add_theme_stylebox_override("normal",normal)
    item.add_theme_stylebox_override("hover",hover)
    item.add_theme_stylebox_override("pressed",pressed)
    item.add_theme_stylebox_override("disabled",disabled)
    var focus:=StyleBoxFlat.new()
    focus.bg_color=Color.TRANSPARENT
    focus.border_color=INK
    focus.set_border_width_all(3)
    focus.set_corner_radius_all(11)
    focus.shadow_color=GOLD
    focus.shadow_size=2
    item.add_theme_stylebox_override("focus",focus)
    for mode in ["font_color","font_hover_color","font_pressed_color","font_disabled_color"]:
        item.add_theme_color_override(mode,color)

func _tile(background:Color,border:Color,width:int,raised:bool)->StyleBoxFlat:
    var shape:=StyleBoxFlat.new()
    shape.bg_color=background
    shape.border_color=border
    shape.set_border_width_all(width)
    shape.set_corner_radius_all(11)
    if raised:
        shape.shadow_color=Color(0.15,0.17,0.1,0.18)
        shape.shadow_size=3
        shape.shadow_offset=Vector2(0,2)
    return shape

func _button(value:String,action:Callable,primary:bool=false)->Button:
    var item:=Button.new()
    item.text=value
    item.accessibility_name=value
    item.custom_minimum_size=Vector2(0,54)
    item.size_flags_horizontal=Control.SIZE_EXPAND_FILL
    item.add_theme_font_size_override("font_size",17)
    _style(item,ACCENT if primary else SOFT,WHITE if primary else INK)
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
    column.add_child(_label("RENZOKU / THE SPACES BETWEEN",12,ACCENT))
    column.add_child(_label("Every neighbor tells a story.",27))
    column.add_child(_label("Fill 1–4 in every row and column. A marked edge means the two numbers differ by exactly one. An unmarked edge means they do NOT differ by one.",15))
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
    preview=_label("",16,ACCENT)
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
    column.add_child(_label("A gold bar between two squares means their numbers differ by one. No bar means they must not. Tap a square, then choose its digit.",13))
    var reduce:=CheckButton.new()
    reduce.text="Reduce motion"
    reduce.accessibility_name="Reduce motion"
    reduce.custom_minimum_size=Vector2(0,48)
    reduce.add_theme_font_size_override("font_size",16)
    reduce.add_theme_color_override("font_color",INK)
    reduce.button_pressed=motion.reduced_motion
    reduce.toggled.connect(_on_reduce_motion_toggled)
    column.add_child(reduce)
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

func _load_level(direction:int=0)->void:
    level=Factory.make(int(state["level"]))
    history.clear()
    selected=-1
    for child in holder.get_children():
        holder.remove_child(child)
        child.queue_free()
    cells.clear()
    upper_cells.clear()
    board=null
    overlay=null
    if MODE=="renzoku":
        # The board is a fixed square: the grid fills it and the marker overlay sits on top.
        board=Control.new()
        board.custom_minimum_size=Vector2(4*76+3*12,4*76+3*12)
        holder.add_child(board)
        grid=GridContainer.new()
        grid.columns=4
        grid.add_theme_constant_override("h_separation",12)
        grid.add_theme_constant_override("v_separation",12)
        board.add_child(grid)
        grid.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
        overlay=Control.new()
        overlay.mouse_filter=Control.MOUSE_FILTER_IGNORE
        board.add_child(overlay)
        overlay.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
        overlay.draw.connect(_draw_markers)
        overlay.resized.connect(overlay.queue_redraw)
    else:
        grid=GridContainer.new()
        grid.columns=7 if MODE=="vowel" else 4
        grid.add_theme_constant_override("h_separation",3)
        grid.add_theme_constant_override("v_separation",4)
        holder.add_child(grid)
    if MODE=="renzoku":
        var fixed:Dictionary=Rules.givens(level)
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
        selected=Rules.blank(level).find(0)
        if selected<0:selected=0
    # Navigation slides the board in; first load and restart stagger the cells in reading order.
    if direction!=0:motion.slide_swap(holder,direction)
    else:motion.stagger_in(cells)
    _refresh()

func _select(index:int)->void:
    if MODE=="pyramid" and (index>=4 or {}.has(index)):return
    if MODE=="renzoku" and Rules.givens(level).has(index):return
    selected=index
    if index>=0 and index<cells.size():motion.press(cells[index])
    _refresh()

func _enter(value:int)->void:
    if selected<0 or selected>=state["entries"].size():return
    if int(state["entries"][selected])==value:return
    var before:Array=state["entries"].duplicate()
    history.append(before.duplicate())
    if history.size()>60:history.pop_front()
    if not Progress.update(state,selected,value):
        history.pop_back()
        if selected<cells.size():motion.shake(cells[selected])
        return
    _save()
    _animate_entry(before,value)
    _refresh()

## Visual only: the state has already changed. Pop the placed cell, then sweep any
## row/column that just completed, or the whole board when it is solved.
func _animate_entry(before:Array,value:int)->void:
    if selected>=cells.size():return
    if value>0:motion.pop(cells[selected])
    var after:Array=state["entries"]
    var board:Dictionary=level
    if Rules.complete(board,after) and not Rules.complete(board,before):
        motion.sweep(cells,GOLD)
        return
    for group in _groups():
        if _full(after,group) and not _full(before,group):
            var group_cells:Array=[]
            for index in group:group_cells.append(cells[index])
            motion.sweep(group_cells,GOLD)

static func _groups()->Array:
    var out:Array=[]
    for k in range(4):
        out.append([k*4,k*4+1,k*4+2,k*4+3])
        out.append([k,k+4,k+8,k+12])
    return out

static func _full(entries:Array,group:Array)->bool:
    for index in group:
        if int(entries[index])==0:return false
    return true

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
    _load_level(1)

func _previous()->void:
    if int(state["level"])==0:return
    Progress.set_level(state,int(state["level"])-1)
    _save()
    _load_level(-1)

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
            if i%4<3 and dots.has("%d-%d"%[i,i+1]):marks.append("right")
            if i<12 and dots.has("%d-%d"%[i,i+4]):marks.append("below")
            var val:int=int(state["entries"][i])
            # Markers are drawn by the overlay in the gaps; the tile shows only its digit.
            item.text=str(val) if val>0 else ""
            item.accessibility_name="Row %d column %d, %s%s"%[
                int(i/4)+1,i%4+1,str(val) if val>0 else "empty",", difference-one bars to "+(", ".join(marks) if not marks.is_empty() else "none")]
            item.tooltip_text=item.accessibility_name
            _style(item,SOFT if item.disabled else SURFACE,INK,i==selected)
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
        var all:Array=[]
        var known:Dictionary={}
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
            _style(item,SOFT if known.has(i) else WHITE,INK,i==selected)
        progress_label.text="%d/4 bottom stones filled"%filled
        preview.text="The pyramid uses adjacent sums; numbers above update as you build."
    feedback.text=Rules.status(level,state["entries"])
    # Colour supports the text cue; the words alone already carry the meaning.
    var tone:Color=INK
    if Rules.complete(level,state["entries"]):tone=SUCCESS
    elif feedback.text.begins_with("Check"):tone=ERROR
    feedback.add_theme_color_override("font_color",tone)
    if overlay!=null:
        overlay.queue_redraw()
        overlay.queue_redraw.call_deferred()
    undo_button.disabled=history.is_empty()
    next_button.disabled=not Progress.can_advance(state)
    prev_button.disabled=int(state["level"])==0

## Draws one gold bar per dotted edge in the gap between its two tiles (see DESIGN.md §5).
## Satisfied bars are success green, broken ones error red; an undotted pair that is
## consecutive is also drawn as a broken bar. Open edges are gold until both are filled.
func _draw_markers()->void:
    if overlay==null or cells.size()!=16:return
    var origin:Vector2=overlay.get_global_rect().position
    for edge in Rules.edges():
        var a:int=int(edge[0])
        var b:int=int(edge[1])
        var marked:bool=Rules.marked(level,a,b)
        var status:String=Rules.edge_state(level,state["entries"],a,b)
        # Dotted edges always show a bar (gold while open); undotted edges show only a broken rule.
        if not marked and status!="bad":continue
        var fill:Color=GOLD
        if status=="ok":fill=SUCCESS
        elif status=="bad":fill=ERROR
        var ra:Rect2=Rect2(cells[a].get_global_rect().position-origin,cells[a].size)
        var rb:Rect2=Rect2(cells[b].get_global_rect().position-origin,cells[b].size)
        var rect:Rect2
        if b==a+1:
            # Horizontal neighbours: a vertical bar centred in the vertical gap.
            var gap_x:float=(ra.end.x+rb.position.x)*0.5
            rect=Rect2(gap_x-BAR_THICK*0.5,ra.get_center().y-BAR_LEN*0.5,BAR_THICK,BAR_LEN)
        else:
            # Vertical neighbours: a horizontal bar centred in the horizontal gap.
            var gap_y:float=(ra.end.y+rb.position.y)*0.5
            rect=Rect2(ra.get_center().x-BAR_LEN*0.5,gap_y-BAR_THICK*0.5,BAR_LEN,BAR_THICK)
        var shape:=StyleBoxFlat.new()
        shape.bg_color=fill
        shape.set_corner_radius_all(4)
        shape.border_color=fill.darkened(0.45)
        shape.set_border_width_all(1)
        overlay.draw_style_box(shape,rect)

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

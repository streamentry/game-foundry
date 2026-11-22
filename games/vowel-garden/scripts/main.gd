extends Control
## Standalone numeric/letter puzzle: distinct rules and art direction per game.
const Factory=preload("res://scripts/level_factory.gd")
const Rules=preload("res://scripts/rules.gd")
const Progress=preload("res://scripts/progress.gd")
const MODE:String="vowel"
const TITLE:String="Vowel Garden"
const PAPER:Color=Color("#FFF8EE")
const INK:Color=Color("#514331")
const ACCENT:Color=Color("#9A6A45")
const SOFT:Color=Color("#F3E4C9")
const WHITE:Color=Color("#FFFFFF")

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
    _build_ui()
    _load_level()

func _label(value:String,px:int,color:Color=INK)->Label:
    var item:=Label.new()
    item.text=value
    item.autowrap_mode=TextServer.AUTOWRAP_WORD_SMART
    item.add_theme_font_size_override("font_size",px)
    item.add_theme_color_override("font_color",color)
    return item

func _style(item:Button,background:Color,color:Color,focused:bool=false)->void:
    var shape:=StyleBoxFlat.new()
    shape.bg_color=background
    shape.border_color=ACCENT if focused else Color("#BDCCBE")
    shape.set_border_width_all(3 if focused else 1)
    shape.set_corner_radius_all(11)
    for mode in ["normal","hover","pressed","disabled"]:
        item.add_theme_stylebox_override(mode,shape)
    var focus:=StyleBoxFlat.new()
    focus.bg_color=Color.TRANSPARENT
    focus.border_color=ACCENT
    focus.set_border_width_all(3)
    focus.set_corner_radius_all(11)
    item.add_theme_stylebox_override("focus",focus)
    for mode in ["font_color","font_hover_color","font_pressed_color","font_disabled_color"]:
        item.add_theme_color_override(mode,color)

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
    column.add_child(_label("VOWEL GARDEN / WORDS BETWEEN THE LINES",12,ACCENT))
    column.add_child(_label("Find the missing sound.",27))
    column.add_child(_label("Six original phrases have lost their vowels. Tap an underscore, then supply A, E, I, O or U; use the topic as a clue.",15))
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
    column.add_child(_label("Each puzzle has one authored accepted phrase. Other sensible English phrases may fit the consonants.",13))
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
    item.custom_minimum_size=Vector2(76,76) if MODE!="vowel" else Vector2(48,52)
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
    _refresh()

func _select(index:int)->void:
    if MODE=="pyramid" and (index>=4 or Rules.clues(level).has(index)):return
    if MODE=="renzoku" and Rules.givens(level).has(index):return
    selected=index
    _refresh()

func _enter(value:int)->void:
    if selected<0 or selected>=state["entries"].size():return
    if int(state["entries"][selected])==value:return
    history.append(state["entries"].duplicate())
    if history.size()>60:history.pop_front()
    if not Progress.update(state,selected,value):
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
            _style(item,SOFT if item.disabled else WHITE,INK,i==selected)
        progress_label.text="%d/16 numbers entered"%filled
        preview.text="Selected row %d, column %d. Dots indicate difference 1."%[int(selected/4)+1,selected%4+1]
    elif MODE=="vowel":
        for i in range(cells.size()):
            var item:Button=cells[i]
            var id:int=int(state["entries"][i])
            item.text=Rules.VOWELS[id-1] if id>0 else "_"
            item.accessibility_name="Missing vowel %d, %s"%[i+1,item.text]
            _style(item,Color("#F5E1BD"),INK,i==selected)
        progress_label.text="%d/%d vowel slots filled"%[filled,level["size"]]
        preview.text=Rules.reveal(level,state["entries"])
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
            _style(item,SOFT if known.has(i) else WHITE,INK,i==selected)
        progress_label.text="%d/4 bottom stones filled"%filled
        preview.text="The pyramid uses adjacent sums; numbers above update as you build."
    feedback.text=Rules.status(level,state["entries"])
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

extends Control
## Standalone numeric/letter puzzle: distinct rules and art direction per game.
const Factory=preload("res://scripts/level_factory.gd")
const Rules=preload("res://scripts/rules.gd")
const Progress=preload("res://scripts/progress.gd")
const Motion=preload("res://scripts/motion.gd")
const MODE:String="vowel"
const TITLE:String="Vowel Garden"
const SETTINGS_PATH:String="user://settings.json"
const BG:Color=Color("#2A6446")
const SURFACE:Color=Color("#FBF3DF")
const SURFACE_ALT:Color=Color("#1E4D35")
const INK:Color=Color("#1A4329")
const INK_MUTED:Color=Color("#CBE2CC")
const ACCENT:Color=Color("#E9B449")
const SUCCESS:Color=Color("#2F7A4E")
const ERROR:Color=Color("#9B2F22")
const FOCUS:Color=Color("#FFE08A")
const GLOW:Color=Color(1.3,1.18,0.7)
const COLUMN_W:int=382
const TILE_MAX:int=50
const TILE_MIN:int=40

var state:Dictionary={}
var level:Dictionary={}
var history:Array=[]
var selected:int=-1
var cells:Array=[]
var upper_cells:Dictionary={}
var grid:HFlowContainer
var tiles_by_char:Array=[]
var holder:VBoxContainer
var heading:Label
var feedback:Label
var progress_label:Label
var preview:Label
var next_button:Button
var prev_button:Button
var undo_button:Button
var reduce_toggle:CheckButton
var motion:=Motion.new()

func _ready()->void:
    state=Progress.load_saved()
    motion.owner=self
    motion.reduced_motion=_load_reduced_motion()
    _build_ui()
    _load_level()

func _label(value:String,px:int,color:Color=SURFACE)->Label:
    var item:=Label.new()
    item.text=value
    item.autowrap_mode=TextServer.AUTOWRAP_WORD_SMART
    item.add_theme_font_size_override("font_size",px)
    item.add_theme_color_override("font_color",color)
    return item

func _style(item:Button,background:Color,color:Color,focused:bool=false,edge:Color=Color("#4F8A66"),edge_px:int=1)->void:
    var shape:=StyleBoxFlat.new()
    shape.bg_color=background
    shape.border_color=INK if focused else edge
    shape.set_border_width_all(4 if focused else edge_px)
    shape.set_corner_radius_all(11)
    var raised:bool=background.a>0.0
    if raised:
        shape.shadow_size=3
        shape.shadow_color=Color(0.0,0.0,0.0,0.25)
        shape.shadow_offset=Vector2(0.0,2.0)
    var hover:=shape.duplicate() as StyleBoxFlat
    var pressed:=shape.duplicate() as StyleBoxFlat
    var disabled:=shape.duplicate() as StyleBoxFlat
    if raised:
        hover.bg_color=background.lightened(0.06)
        pressed.bg_color=background.darkened(0.08)
        pressed.shadow_size=0
        pressed.shadow_offset=Vector2.ZERO
        disabled.bg_color=background
    item.add_theme_stylebox_override("normal",shape)
    item.add_theme_stylebox_override("hover",hover)
    item.add_theme_stylebox_override("pressed",pressed)
    item.add_theme_stylebox_override("disabled",disabled)
    var focus:=StyleBoxFlat.new()
    focus.bg_color=Color.TRANSPARENT
    focus.border_color=FOCUS
    focus.set_border_width_all(3)
    focus.set_corner_radius_all(11)
    item.add_theme_stylebox_override("focus",focus)
    for mode in ["font_color","font_hover_color","font_pressed_color","font_disabled_color"]:
        item.add_theme_color_override(mode,color)

func _button(value:String,action:Callable,primary:bool=false)->Button:
    var item:=Button.new()
    item.text=value
    item.accessibility_name=value
    item.custom_minimum_size=Vector2(0,56)
    item.size_flags_horizontal=Control.SIZE_EXPAND_FILL
    item.add_theme_font_size_override("font_size",18)
    _style(item,ACCENT if primary else SURFACE_ALT,INK if primary else SURFACE)
    item.pressed.connect(action)
    return item

func _row()->HBoxContainer:
    var row:=HBoxContainer.new()
    row.add_theme_constant_override("separation",8)
    return row

func _build_ui()->void:
    var bg:=ColorRect.new()
    bg.color=BG
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
    column.add_child(_label("VOWEL GARDEN / WORDS BETWEEN THE LINES",12,INK_MUTED))
    column.add_child(_label("Find the missing sound.",27))
    column.add_child(_label("Six original phrases have lost their vowels. Tap an underscore, then supply A, E, I, O or U; use the topic as a clue.",15))
    heading=_label("",16)
    column.add_child(heading)
    holder=VBoxContainer.new()
    holder.add_theme_constant_override("separation",7)
    holder.size_flags_horizontal=Control.SIZE_EXPAND_FILL
    column.add_child(holder)
    grid=_new_grid()
    holder.add_child(grid)
    preview=_label("",14,INK_MUTED)
    column.add_child(preview)
    progress_label=_label("",15)
    column.add_child(progress_label)
    var banner:=PanelContainer.new()
    var banner_shape:=StyleBoxFlat.new()
    banner_shape.bg_color=SURFACE
    banner_shape.set_corner_radius_all(12)
    banner_shape.content_margin_left=14
    banner_shape.content_margin_right=14
    banner_shape.content_margin_top=10
    banner_shape.content_margin_bottom=10
    banner.add_theme_stylebox_override("panel",banner_shape)
    feedback=_label("",16,INK)
    banner.add_child(feedback)
    column.add_child(banner)
    var keypad:=GridContainer.new()
    keypad.columns=5 if MODE=="vowel" else (3 if MODE=="pyramid" else 4)
    keypad.size_flags_horizontal=Control.SIZE_EXPAND_FILL
    keypad.add_theme_constant_override("h_separation",7)
    keypad.add_theme_constant_override("v_separation",7)
    var labels:Array=["A","E","I","O","U"] if MODE=="vowel" else (
        ["1","2","3","4","5","6","7","8","9"] if MODE=="pyramid" else ["1","2","3","4"])
    for i in range(labels.size()):
        var key:=_button(str(labels[i]),_enter.bind(i+1),true)
        key.add_theme_font_size_override("font_size",24)
        keypad.add_child(key)
    column.add_child(keypad)
    column.add_child(_label("Each puzzle has one authored accepted phrase. Other sensible English phrases may fit the consonants.",13,INK_MUTED))
    var actions:=_row()
    undo_button=_button("Undo",_undo)
    actions.add_child(undo_button)
    actions.add_child(_button("Erase",_enter.bind(0)))
    actions.add_child(_button("Restart",_restart))
    column.add_child(actions)
    reduce_toggle=CheckButton.new()
    reduce_toggle.text="Reduce motion"
    reduce_toggle.accessibility_name="Reduce motion"
    reduce_toggle.button_pressed=motion.reduced_motion
    reduce_toggle.custom_minimum_size=Vector2(0,48)
    reduce_toggle.add_theme_font_size_override("font_size",17)
    for mode in ["font_color","font_hover_color","font_pressed_color","font_focus_color"]:
        reduce_toggle.add_theme_color_override(mode,SURFACE)
    reduce_toggle.toggled.connect(_on_reduce_toggled)
    column.add_child(reduce_toggle)
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
    item.custom_minimum_size=Vector2(76,76) if MODE!="vowel" else Vector2(50,60)
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
    grid=_new_grid()
    holder.add_child(grid)
    cells.clear()
    upper_cells.clear()
    tiles_by_char.clear()
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
        var tile:=_tile_size()
        var row:HBoxContainer=null
        for ch in str(level["pattern"]):
            if ch==" ":
                row=null
                tiles_by_char.append(null)
                continue
            if row==null:
                row=HBoxContainer.new()
                row.add_theme_constant_override("separation",3)
                grid.add_child(row)
            var item:Button=_cell("",30,Callable())
            item.custom_minimum_size=tile
            if ch=="_":
                item.pressed.connect(_select.bind(vowel_index))
                cells.append(item)
                vowel_index+=1
            else:
                item.disabled=true
                item.text=ch
                _style(item,SURFACE,INK)
            row.add_child(item)
            tiles_by_char.append(item)
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
    motion.stagger_in(_all_tiles())

func _select(index:int)->void:
    if MODE=="pyramid" and (index>=4 or {}.has(index)):return
    if MODE=="renzoku" and {}.has(index):return
    selected=index
    if MODE=="vowel" and index<cells.size():motion.press(cells[index])
    _refresh()

func _enter(value:int)->void:
    if selected<0 or selected>=state["entries"].size():return
    if int(state["entries"][selected])==value:return
    var before:Array=state["entries"].duplicate()
    history.append(state["entries"].duplicate())
    if history.size()>60:history.pop_front()
    if not Progress.update(state,selected,value):
        history.pop_back()
        _reject(selected)
        return
    _save()
    _refresh()
    _play_entry(before,value)

func _reject(index:int)->void:
    if index<cells.size():motion.shake(cells[index])
    feedback.text="That slot already has a vowel. Tap Erase, then choose another letter."
    feedback.add_theme_color_override("font_color",ERROR)

func _play_entry(before:Array,value:int)->void:
    var slot:Button=cells[selected]
    if value==0:
        motion.press(slot)
        return
    if Rules.complete(level,state["entries"]):
        motion.celebrate(self,grid.get_global_rect().get_center(),[ACCENT,SUCCESS,SURFACE])
        motion.sweep(_all_tiles(),GLOW)
        return
    var swept:bool=false
    for span in _word_spans():
        if not _word_filled(span,before) and _word_filled(span,state["entries"]):
            motion.sweep(_tiles(span),GLOW)
            swept=true
    if swept:return
    motion.pop(slot,1.3)
    _glow(slot)

func _glow(slot:Control)->void:
    if motion.reduced_motion or not slot.is_inside_tree():
        slot.self_modulate=Color.WHITE
        return
    var old:Variant=slot.get_meta("_glow") if slot.has_meta("_glow") else null
    if old is Tween and (old as Tween).is_valid():(old as Tween).kill()
    slot.self_modulate=GLOW
    var tw:=create_tween()
    slot.set_meta("_glow",tw)
    tw.tween_property(slot,"self_modulate",Color.WHITE,0.26).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_OUT)

## Reading-order grid indices of each word (spaces separate words).
func _word_spans()->Array:
    var spans:Array=[]
    var current:Array=[]
    var pattern:String=str(level["pattern"])
    for i in pattern.length():
        if pattern.substr(i,1)==" ":
            if not current.is_empty():spans.append(current)
            current=[]
        else:current.append(i)
    if not current.is_empty():spans.append(current)
    return spans

## True when a word has at least one blank and every blank holds a vowel.
## This marks filled words, not correct ones: the rules only grade the whole phrase.
func _word_filled(span:Array,entries:Array)->bool:
    var pattern:String=str(level["pattern"])
    var slots:int=0
    for i in span:
        if pattern.substr(int(i),1)=="_":
            slots+=1
            if int(entries[pattern.substr(0,int(i)).count("_")])==0:return false
    return slots>0

func _tiles(span:Array)->Array:
    var out:Array=[]
    for i in span:out.append(tiles_by_char[int(i)])
    return out

func _all_tiles()->Array:
    var out:Array=[]
    for t in tiles_by_char:
        if t!=null:out.append(t)
    return out

func _new_grid()->HFlowContainer:
    var flow:=HFlowContainer.new()
    flow.add_theme_constant_override("h_separation",16)
    flow.add_theme_constant_override("v_separation",10)
    flow.alignment=FlowContainer.ALIGNMENT_CENTER
    flow.size_flags_horizontal=Control.SIZE_EXPAND_FILL
    return flow

## Largest tile that lets the longest word fit one row (never below TILE_MIN).
func _tile_size()->Vector2:
    var longest:int=1
    for word in str(level["pattern"]).split(" "):
        longest=maxi(longest,word.length())
    var gaps:int=(longest-1)*3
    var w:int=clampi(floori(float(COLUMN_W-gaps)/float(longest)),TILE_MIN,TILE_MAX)
    return Vector2(w,roundi(w*1.2))

func _on_reduce_toggled(on:bool)->void:
    motion.reduced_motion=on
    _save_reduced_motion(on)

func _load_reduced_motion()->bool:
    if not FileAccess.file_exists(SETTINGS_PATH):return false
    var file:=FileAccess.open(SETTINGS_PATH,FileAccess.READ)
    if file==null:return false
    var parser:=JSON.new()
    if parser.parse(file.get_as_text())!=OK or typeof(parser.data)!=TYPE_DICTIONARY:return false
    return parser.data.get("reduced_motion",false)==true

func _save_reduced_motion(on:bool)->void:
    var file:=FileAccess.open(SETTINGS_PATH,FileAccess.WRITE)
    if file==null:
        push_warning(TITLE+" could not save its motion setting.")
        return
    file.store_string(JSON.stringify({"reduced_motion":on}))
    file.close()

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
    motion.stagger_in(_all_tiles())

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
            item.text=Rules.VOWELS[id-1] if id>0 else "_"
            item.accessibility_name="Missing vowel %d, %s"%[i+1,item.text]
            item.add_theme_font_size_override("font_size",30 if id>0 else 24)
            if id>0:_style(item,ACCENT,INK,i==selected)
            else:_style(item,SURFACE,INK,i==selected,ACCENT,3)
        progress_label.text="%d/%d vowel slots filled"%[filled,level["size"]]
        preview.text=Rules.reveal(level,state["entries"])
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
            _style(item,SURFACE_ALT if known.has(i) else SURFACE,INK,i==selected)
        progress_label.text="%d/4 bottom stones filled"%filled
        preview.text="The pyramid uses adjacent sums; numbers above update as you build."
    feedback.text=Rules.status(level,state["entries"])
    feedback.add_theme_color_override("font_color",INK)
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

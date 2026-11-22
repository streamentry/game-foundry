extends RefCounted
## Choose a chain of four-letter English words from an explicitly shown
## small lexicon. Each step changes exactly one position; no repeated
## word. Reach goal in the specified move budget, no spelling guesses.

static func blank(level:Dictionary)->Array:
    var out:Array=[]
    out.resize(int(level["moves"]))
    out.fill(0)
    return out

static func one_letter(a:String,b:String)->bool:
    if a.length()!=4 or b.length()!=4:return false
    var count:int=0
    for i in range(4):
        if a[i]!=b[i]:count+=1
    return count==1

static func valid_level(level:Dictionary)->bool:
    if int(level.get("moves",0))<2 or int(level.get("moves",0))>6:return false
    if typeof(level.get("words"))!=TYPE_ARRAY or level["words"].size()<level["moves"]:return false
    if str(level["start"]).length()!=4 or str(level["goal"]).length()!=4:return false
    var seen:Dictionary={str(level["start"]):true}
    for word in level["words"]:
        var w:String=str(word)
        if w.length()!=4 or w!=w.to_upper() or seen.has(w):return false
        seen[w]=true
    return level["words"].has(level["goal"])

static func valid_entries(level:Dictionary,raw:Variant)->bool:
    if typeof(raw)!=TYPE_ARRAY or raw.size()!=int(level["moves"]):return false
    var seen:Dictionary={str(level["start"]):true}
    var previous:String=str(level["start"])
    var encountered_blank:bool=false
    for v in raw:
        if typeof(v) not in [TYPE_INT,TYPE_FLOAT]:return false
        var n:float=float(v)
        if not is_finite(n) or floorf(n)!=n or n<0.0 or n>level["words"].size():return false
        if n==0:
            encountered_blank=true
            continue
        if encountered_blank:return false
        var word:String=str(level["words"][int(n)-1])
        if seen.has(word) or not one_letter(previous,word):return false
        seen[word]=true
        previous=word
    return true

static func complete(level:Dictionary,entries:Array)->bool:
    if not valid_entries(level,entries):return false
    if entries.has(0):return false
    return str(level["words"][int(entries[-1])-1])==str(level["goal"])

static func status(level:Dictionary,entries:Array)->String:
    if complete(level,entries):return "A complete word ladder. Every move changes one letter."
    if not valid_entries(level,entries):return "Use unique words, with one letter changed per move."
    var used:int=0
    for item in entries:
        if int(item)!=0:used+=1
    return "%d/%d moves. Reach %s using only shown words."%[used,int(level["moves"]),str(level["goal"])]

static func options(level:Dictionary,entries:Array)->Array:
    var current:String=str(level["start"])
    var seen:Dictionary={current:true}
    for v in entries:
        if int(v)==0:break
        current=str(level["words"][int(v)-1])
        seen[current]=true
    var out:Array=[]
    for i in range(level["words"].size()):
        var w:String=str(level["words"][i])
        if not seen.has(w) and one_letter(current,w):
            out.append(i+1)
    return out

static func count_solutions(level:Dictionary,limit:int=2)->int:
    if not valid_level(level):return 0
    var out:Dictionary={"count":0}
    _search(level,blank(level),0,out,limit)
    return int(out["count"])

static func _search(level:Dictionary,state:Array,depth:int,out:Dictionary,limit:int)->void:
    if int(out["count"])>=limit:return
    if depth==state.size():
        if complete(level,state):out["count"]=int(out["count"])+1
        return
    for candidate in options(level,state):
        state[depth]=candidate
        _search(level,state,depth+1,out,limit)
        state[depth]=0
        if int(out["count"])>=limit:return

extends RefCounted
## A,E,I,O,U fill the missing-vowel positions in a curated original
## phrase. The authored completion is a target, not proof of uniqueness
## across all English expressions.

const VOWELS:String="AEIOU"
const Factory=preload("res://scripts/level_factory.gd")

static func blank(level:Dictionary)->Array:
    var out:Array=[]
    out.resize(int(level["size"]))
    out.fill(0)
    return out

static func valid_level(level:Dictionary)->bool:
    if typeof(level.get("pattern"))!=TYPE_STRING or int(level.get("size",-1))<4:
        return false
    var slots:int=0
    for ch in str(level["pattern"]):
        if ch=="_":slots+=1
        elif ch!=" " and (ch<"A" or ch>"Z"):return false
    return slots==int(level["size"]) and slots<=16

static func valid_entries(level:Dictionary,raw:Variant)->bool:
    if typeof(raw)!=TYPE_ARRAY or raw.size()!=int(level["size"]):return false
    for v in raw:
        if typeof(v) not in [TYPE_INT,TYPE_FLOAT]:return false
        var n:float=float(v)
        if not is_finite(n) or floorf(n)!=n or n<0.0 or n>5.0:return false
    return true

static func reveal(level:Dictionary,entries:Array)->String:
    if not valid_entries(level,entries):return ""
    var out:String=""
    var pos:int=0
    for ch in str(level["pattern"]):
        if ch=="_":
            var id:int=int(entries[pos])
            out+=VOWELS[id-1] if id>0 else "_"
            pos+=1
        else:out+=ch
    return out

static func complete(level:Dictionary,entries:Array)->bool:
    if not valid_entries(level,entries) or entries.has(0):return false
    var answer:Array=[]
    # Independent test uses the canonical authored vowel sequence provided
    # to the game's factory for this named phrase; no external dictionary.
    for item in Factory.FIXTURES:
        if item["pattern"]==level["pattern"] and item["name"]==level["name"]:
            answer=item["answer"]
            break
    return not answer.is_empty() and entries==answer

static func status(level:Dictionary,entries:Array)->String:
    if complete(level,entries):return "The authored phrase is complete. Well done!"
    if not valid_entries(level,entries):return "Use only A E I O U at the missing positions."
    var filled:int=0
    for v in entries:
        if int(v)>0:filled+=1
    return "%d/%d vowels added. The topic is %s."%[filled,int(level["size"]),str(level["theme"])]

static func count_solutions(level:Dictionary,limit:int=2)->int:
    # Count accepted authored sequences, NOT natural-language uniqueness.
    if not valid_level(level):return 0
    for item in Factory.FIXTURES:
        if item["pattern"]==level["pattern"] and item["name"]==level["name"]:
            return 1 if limit>0 else 0
    return 0

extends RefCounted
## Original curated four-letter one-step word ladders with a single
## shortest sequence among the explicitly available word cards.
const COUNT:int=6
const FIXTURES:Array=[{"name":"From Cold to Warm","words":["BOLD","CARD","COLT","CORD","WARD","WARM"],"moves":4,"reference":[4,2,5,6]},{"name":"Head to Tail","words":["HEAL","HEAP","TAIL","TALL","TEAL","TEAR","TELL"],"moves":5,"reference":[1,5,7,4,3]},{"name":"Sand to Need","words":["BAND","BEND","NEED","SEED","SEND"],"moves":3,"reference":[5,4,3]},{"name":"Fire to Love","words":["DIVE","DOVE","FINE","FIVE","LONE","LOVE"],"moves":4,"reference":[4,1,2,6]},{"name":"Books to Polls","words":["COOK","COOL","FOOL","LOOK","POLL","POOL"],"moves":4,"reference":[1,2,6,5]},{"name":"Game to Lane","words":["DATE","GATE","GAVE","LANE","LATE"],"moves":3,"reference":[2,5,4]}]
static func make(index:int)->Dictionary:
    var item:Dictionary=FIXTURES[clampi(index,0,COUNT-1)]
    return {"name":item["name"],"size":item["moves"],"start":["COLD","HEAD","SAND","FIRE","BOOK","GAME"][clampi(index,0,COUNT-1)],
        "goal":["WARM","TAIL","NEED","LOVE","POLL","LANE"][clampi(index,0,COUNT-1)],
        "words":item["words"].duplicate(),"moves":item["moves"]}
static func reference_solution(index:int)->Array:
    return FIXTURES[clampi(index,0,COUNT-1)]["reference"].duplicate()

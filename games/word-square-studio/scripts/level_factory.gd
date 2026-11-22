extends RefCounted
## Original three-letter English word-square arrangements.
const COUNT:int=6
const FIXTURES:Array=[{"name":"Cat in the Corner","words":["CAT","TEN","ARE"],"reference":[1,3,2]},{"name":"A Little Bat","words":["ARE","BAT","TEN"],"reference":[2,1,3]},{"name":"Red Car","words":["CAR","ARE","RED"],"reference":[1,2,3]},{"name":"One Bar","words":["BAR","ARE","RED"],"reference":[1,2,3]},{"name":"Tiny Pig","words":["PIG","GEM","ICE"],"reference":[1,3,2]},{"name":"Sunlit Square","words":["USE","SUN","NET"],"reference":[2,1,3]}]
static func make(index:int)->Dictionary:
    var x:Dictionary=FIXTURES[clampi(index,0,COUNT-1)]
    return {"name":x["name"],"size":3,"words":x["words"].duplicate()}
static func reference_solution(index:int)->Array:
    return FIXTURES[clampi(index,0,COUNT-1)]["reference"].duplicate()

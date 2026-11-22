extends RefCounted
## Eight independently authored, exact-cover-verified number/rectangle boards.
## All dimensions in row, column, height, width order; 0-based coordinates.

const COUNT: int = 8
const LEVELS: Array = [
    {
        "name": "Small beds", "n": 4,
        "clues": [[1,0,4],[1,1,6],[2,1,2],[2,3,4]],
        "solution": [[0,0,4,1],[0,1,2,3],[2,1,2,1],[2,2,2,2]]
    },
    {
        "name": "Along the wall", "n": 4,
        "clues": [[0,1,3],[1,0,3],[1,2,2],[3,2,4],[0,3,4]],
        "solution": [[0,0,1,3],[1,0,3,1],[1,1,1,2],[2,1,2,2],[0,3,4,1]]
    },
    {
        "name": "Quiet courtyard", "n": 5,
        "clues": [[0,1,2],[0,2,3],[2,3,12],[2,4,3],[4,2,5]],
        "solution": [[0,0,1,2],[0,2,1,3],[1,0,3,4],[1,4,3,1],[4,0,1,5]]
    },
    {
        "name": "Tall leaves", "n": 5,
        "clues": [[1,0,2],[3,0,3],[3,1,5],[1,2,3],[3,2,2],[4,4,10]],
        "solution": [[0,0,2,1],[2,0,3,1],[0,1,5,1],[0,2,3,1],[3,2,2,1],[0,3,5,2]]
    },
    {
        "name": "Sunlit patch", "n": 5,
        "clues": [[4,0,5],[0,1,2],[1,2,8],[0,4,2],[2,4,6],[4,3,2]],
        "solution": [[0,0,5,1],[0,1,1,2],[1,1,4,2],[0,3,1,2],[1,3,3,2],[4,3,1,2]]
    },
    {
        "name": "Garden paths", "n": 5,
        "clues": [[0,1,3],[0,3,2],[1,1,2],[1,3,2],[2,3,4],[1,4,2],[3,1,10]],
        "solution": [[0,0,1,3],[0,3,1,2],[1,0,1,2],[1,2,1,2],[2,0,1,4],[1,4,2,1],[3,0,2,5]]
    },
    {
        "name": "Open meadow", "n": 6,
        "clues": [[0,3,6],[1,3,12],[3,0,2],[3,2,2],[3,5,2],[4,2,10],[4,5,2]],
        "solution": [[0,0,1,6],[1,0,2,6],[3,0,1,2],[3,2,1,2],[3,4,1,2],[4,0,2,5],[4,5,2,1]]
    },
    {
        "name": "Last light", "n": 6,
        "clues": [[2,2,15],[2,5,3],[3,1,2],[3,3,2],[3,5,2],[4,2,6],[5,2,4],[5,5,2]],
        "solution": [[0,0,3,5],[0,5,3,1],[3,0,1,2],[3,2,1,2],[3,4,1,2],[4,0,1,6],[5,0,1,4],[5,4,1,2]]
    }
]


static func make(index: int) -> Dictionary:
    var fixture: Dictionary = LEVELS[clampi(index, 0, COUNT - 1)]
    return {
        "name": fixture["name"],
        "size": fixture["n"],
        "clues": fixture["clues"].duplicate(true),
    }


static func solution(index: int) -> Array:
    return LEVELS[clampi(index, 0, COUNT - 1)]["solution"].duplicate(true)

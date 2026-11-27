# I417: Leaves downstream

Run 3, batch 16.

In autumn, five leaves drift down the river, turning slowly as they go and following the river's line from row to row. It adds motion to the still floating leaves of I342. They're part of the endless river: they arrive from upstream and keep going.

- `RiverLifeLayer.Course` is one point per row, the average x of that row's water cells, top to bottom. Leaves interpolate along it. Motion only.
- `RiverMoments.LeavesAfloat(season)`, tested.

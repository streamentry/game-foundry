# I351 — The arrow plays what's waiting

Run 3, batch 3 (P9).

## Player

"I tapped the big arrow to see what happens. It closed home and showed the story page. I wanted to play."

## PM decision

After the story, home's big arrow plays what's waiting: the newest nursery plant, else the newest story thing, still holding its first puzzle. That thing's picture peeks from the arrow's corner, so you know what it will open before you tap. With nothing waiting, it behaves as before. During the story it still continues the story.

## Dev

- Home's default play token works out the waiting thing (bought plants first, then story things) and opens it through `OpenBoughtPlay` or `OpenTreePlay`. It sets the in-hand index, so the solved ring grows in on return (I212).
- The token carries the thing's art as an ornament.

## Result

The most obvious button now does the most obvious thing. Not known: whether players then want to pick a different puzzle. The play marks on the map (I255) are still there for choosing.

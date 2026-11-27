# I396: No boxes at night

Run 3, batch 12.

## Player note (P22, I383 night shot on b320)

At night, a pale square framed the apple-ladder painting. By day it was invisible.

## Cause

25 building paintings (`build-25` to `build-49`) carried a faint haze over their whole canvas: about 8–10% opacity where they should have been fully transparent. On sunny grass it disappears; on the darkened evening ground (I278) it shows as a box.

## Fix

The art itself is fixed. For each painting whose border alpha has a median of 8 or more, alpha is remapped as `max(0, (a − haze) · 255 / (255 − haze))`, with the haze level taken from that image's own border. The border is now fully transparent and painted pixels keep their opacity. Story things, trees and flowers had clean borders and were left alone.

# I436: News that stays

Run 3, batch 20.

## Player note (P28, b325 tour)

The tour's three moment shots (I429) showed the seeds rising (60 → 135 → 155): the four good neighbours, the deeper three and the summer gift were all paid. But **no toast was on screen** in any of them. The picture toasts (I387), the season's gift toast (I388) and the found-a-pairing word (I368) may never have reached a player on home.

## Cause (likely) and fix

`Rebuild()` dismisses any toast (I104), which is right for a toast pointing at something on a page that's being rebuilt, but wrong for news on home: many things call `Rebuild()`, so the news vanished before it could be read.

- A toast with pictures is *news*: it now outlives a page rebuild and stays its 3.6 seconds. Page toasts are dismissed as before.
- The tour now counts `Rule toast` objects at each moment shot (with I433's atmosphere line), so the next tour proves it rather than leaving it to the eye. The result is recorded in I437.

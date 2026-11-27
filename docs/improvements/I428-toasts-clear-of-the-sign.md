# I428: Toasts clear of the sign at large text

Run 3, batch 19 ("moments, seen").

## Player note (P26, b323 at 150% text)

On a family visit, the gift-idea toast covered the "Visiting a garden" sign. The sign grows with the text size, but the toast's top clamp (I398) was a fixed 96 points.

## Fix

The clearance now scales with text size: `96 × max(1, text% / 100)`.

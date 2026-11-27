# I414: Touch the water

Run 3, batch 16.

Tap the river and it answers: a ring opens where you touched, with a soft drop sound. It's a small, harmless thing to do with your finger, the kind of response that makes a world feel real. Tapping land still does what it always did; the water tap also still reaches the usual handler.

- `GardenMapView.TapLand` checks for water and calls `RiverLifeLayer.Ripple(at)` and `WaterTapped`. The app plays the soundscape's water drop (I316) when sound is on.

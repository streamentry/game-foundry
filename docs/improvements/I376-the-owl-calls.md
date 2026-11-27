# I376: The owl calls at night

Run 3, batch 8.

## PM decision

A visitor should be heard as well as seen. Once a grove has brought the owl (I374), and only while a grove still stands, the night soundscape (I316) sometimes carries its call: *hoo... hoo-hoo*, low and soft. It's rare (about 16 seconds between calls, chosen 30% of the time), so it is noticed.

## Dev

- `AmbientVoice.Owl`.
- `Soundscape.VisitorVoice(visitor, season, phase)`.
- `SecondsBetween(Owl) = 16`.
- `GardenSounds.Ambient(Owl)` is synthesised: three notes around 380–410 Hz, each gliding down slightly, with a soft second harmonic and a breath of noise.
- `TickSoundscape` picks it only while `owlInGrove`, which is set when home's map is laid out.
- Tested: heard at night only, never by day; the heron is silent; the owl is rarer than birds.

## Result

There's now a reason to open the garden after dark. The sound switch silences it like every other voice.

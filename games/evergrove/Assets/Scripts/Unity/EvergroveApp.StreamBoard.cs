using System;
using System.Collections;
using System.Globalization;
using Evergrove.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
namespace Evergrove.UnityClient
{
    public sealed partial class EvergroveApp
    {

        // Where you are along the creek: twelve stepping stones (passed, here,
        // ahead) with the stream's name. "Stream n / 12" stays as spoken text.
        private void StreamStones(Puzzle puzzle)
        {
            StreamStones(puzzle, content, true);
        }

        private void StreamStones(Puzzle puzzle, Transform parent, bool named)
        {
            var row = Node(named ? "Chapter" : "Walk so far", parent);
            var group = row.AddComponent<VerticalLayoutGroup>();
            group.childAlignment = TextAnchor.MiddleCenter;
            group.spacing = 4;
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = true;
            group.childForceExpandHeight = false;
            var stones = Node("Stepping stones", row.transform);
            var stoneRow = stones.AddComponent<HorizontalLayoutGroup>();
            stoneRow.reverseArrangement = GameLocalization.IsRtl(GameLocalization.Current);
            // Painted stones sit on one waterline, like a path across the creek (I120).
            bool paintedPath = MosseloraArt.Load(MosseloraArt.StoneHere) != null;
            stoneRow.childAlignment = paintedPath ? TextAnchor.LowerCenter : TextAnchor.MiddleCenter;
            stoneRow.spacing = paintedPath ? 4 : 7;
            stoneRow.childControlWidth = true;
            stoneRow.childControlHeight = true;
            stoneRow.childForceExpandWidth = false;
            stoneRow.childForceExpandHeight = false;
            // The path starts at the spring (I123): water already runs from it to your
            // stone, so even on the first stream "the water reaches here, not beyond"
            // shows before any stone has been walked.
            Sprite spring = paintedPath ? MosseloraArt.Load(MosseloraArt.SpringWaves) : null;
            if (spring != null)
            {
                var source = Node("Spring", stones.transform);
                var sourceLayout = source.AddComponent<LayoutElement>();
                sourceLayout.minWidth = sourceLayout.preferredWidth = 20f;
                sourceLayout.minHeight = sourceLayout.preferredHeight = 20f;
                PathWater(source.transform, stoneRow, true, false);
                var sourceArt = Node("Spring art", source.transform);
                Fill(sourceArt.GetComponent<RectTransform>());
                var sourceImage = sourceArt.AddComponent<Image>();
                sourceImage.sprite = spring;
                sourceImage.preserveAspect = true;
                sourceImage.raycastTarget = false;
            }
            for (int i = 0; i < PuzzleCatalog.Count; i++)
            {
                bool here = i == world.Round;
                bool passed = i < world.Round;
                var stone = Node(passed ? "Stone passed" : here ? "Stone here" : "Stone ahead", stones.transform);
                var layout = stone.AddComponent<LayoutElement>();
                // The stone just passed lights up once on arrival at the next stream.
                if (named && gentleMotion && settledRound != world.Round && i == world.Round - 1)
                    BloomPulse.Add(stone, 0.25f, 0.45f);
                // Painted river stones across the creek (Codex, I120): the ones ahead wear
                // their season, the one you stand on grows a sprout, and the water you
                // have already brought runs under every walked stone up to yours.
                Sprite painted = PathStone(i, here, passed);
                if (painted != null)
                {
                    float width = here ? 34f : 22f;
                    layout.minWidth = layout.preferredWidth = width;
                    layout.minHeight = layout.preferredHeight = Mathf.Round(width / MosseloraArt.Aspect(painted));
                    // A cold-read could not tell walked stones from ahead by the stones
                    // alone; a thin stream under every walked stone, reaching half way
                    // under yours, says "the water got this far" in the game's language.
                    if (passed || (here && (i > 0 || spring != null)))
                        PathWater(stone.transform, stoneRow, !here, here);
                    if (here)
                    {
                        // The old leaf-green ring, now around your stone: one high-contrast
                        // mark the eye finds at once, which the sprout alone did not match.
                        var ring = Node("You are here", stone.transform);
                        var ringRect = ring.GetComponent<RectTransform>();
                        ringRect.anchorMin = ringRect.anchorMax = new Vector2(0.5f, 0.5f);
                        ringRect.sizeDelta = new Vector2(width + 10f, layout.preferredHeight + 10f);
                        ringRect.anchoredPosition = Vector2.zero;
                        var ringImage = ring.AddComponent<Image>();
                        ringImage.sprite = MosseloraArt.MarkerRing();
                        ringImage.type = Image.Type.Sliced;
                        ringImage.pixelsPerUnitMultiplier = MosseloraArt.MarkerRingScale;
                        ringImage.color = Leaf;
                        ringImage.raycastTarget = false;
                    }
                    var art = Node("Stone art", stone.transform);
                    Fill(art.GetComponent<RectTransform>());
                    var artImage = art.AddComponent<Image>();
                    artImage.sprite = painted;
                    artImage.preserveAspect = true;
                    artImage.raycastTarget = false;
                    // Pale frost, moss and the sunlit "here" stone are deepened a little so
                    // every stone keeps at least 3:1 against the paper, as the old dots did
                    // (WCAG 1.4.11, I84): summer 3.9, autumn 4.1, winter 3.4, here 3.2.
                    artImage.color = passed ? Color.white : new Color(0.84f, 0.84f, 0.84f, 1f);
                    continue;
                }
                var image = stone.AddComponent<Image>();
                image.raycastTarget = false;
                float size = here ? 22f : 14f; // was 18 / 11: legible at arm's length (I84)
                layout.minWidth = layout.preferredWidth = size;
                layout.minHeight = layout.preferredHeight = size;
                image.sprite = here ? MosseloraArt.BoldRing() : MosseloraArt.Rounded(4);
                image.type = Image.Type.Simple;
                image.color = here ? Leaf : passed ? MosseloraTheme.WaterChannel : SeasonStone(i);
            }
            if (!named) return;
            string name = GameLocalization.Current == "en" ? puzzle.Name : "";
            // "Rainwater · Wildflower sanctuary" reads as the stream's name beside
            // the painted landmark that roots it, as on the hub (I86); the rooted
            // tile's spoken rule still names the sanctuary.
            int split = name.IndexOf(" · ", StringComparison.Ordinal);
            int[] past = world.Choices;
            Sprite mark = split > 0 && past.Length > 0
                ? ItemArt(past.Length - 1, past[past.Length - 1]) : null;
            // The stream's name is painted on a small wooden sign by the path,
            // part of the scenery rather than a caption (I162).
            Transform nameRow = CenteredRowIn(row.transform, "Stream name row");
            nameRow.GetComponent<HorizontalLayoutGroup>().spacing = 6;
            Transform sign = PaintedSign(nameRow);
            bool plank = sign != null;
            if (plank) nameRow = sign;
            if (mark != null)
            {
                var icon = Node("Rooted by your choice", nameRow);
                var iconLayout = icon.AddComponent<LayoutElement>();
                iconLayout.minWidth = iconLayout.preferredWidth = 26;
                iconLayout.minHeight = iconLayout.preferredHeight = 26;
                var iconImage = icon.AddComponent<Image>();
                iconImage.sprite = mark;
                iconImage.preserveAspect = true;
                iconImage.raycastTarget = false;
            }
            Text title = Label(nameRow, "Stream name",
                name.Length == 0 ? " " : mark != null ? name.Substring(0, split) : name, 16, 0,
                plank ? Ink : Moss, // was 14 (I137); ink on the painted wood (I162)
                TextAnchor.MiddleCenter);
            Emphasize(title);
            if (plank) OneLine(title);
            // Spoken position along the creek, not shown: the stones show it.
            Text spoken = Label(row.transform, "Stream position",
                GameLocalization.Text("stream", world.Round + 1, PuzzleCatalog.Count),
                12, 0, new Color(0f, 0f, 0f, 0f), TextAnchor.MiddleCenter);
            spoken.GetComponent<LayoutElement>().ignoreLayout = true;
        }

        // The thin stream under the path: full width under walked stones and the
        // spring, half way under your stone, overlapping neighbours so it has no seams.
        private void PathWater(Transform holder, HorizontalLayoutGroup row, bool flowsOn, bool endsHere)
        {
            var water = Node("Water so far", holder);
            water.transform.SetAsFirstSibling(); // under the stone
            var waterRect = water.GetComponent<RectTransform>();
            bool rtl = row.reverseArrangement;
            float reachStart = rtl && endsHere ? 0.5f : 0f;
            float reachEnd = !rtl && endsHere ? 0.5f : 1f;
            waterRect.anchorMin = new Vector2(reachStart, 0f);
            waterRect.anchorMax = new Vector2(reachEnd, 0f);
            waterRect.pivot = new Vector2(0.5f, 0f);
            float before = rtl && flowsOn ? row.spacing + 3f : 3f;
            float after = !rtl && flowsOn ? row.spacing + 3f : 3f;
            // 9 pt: a 6 pt stroke was "easy to miss at phone size" in a cold-read (I124).
            waterRect.offsetMin = new Vector2(-before, -4f);
            waterRect.offsetMax = new Vector2(after, 5f);
            var waterImage = water.AddComponent<Image>();
            waterImage.sprite = MosseloraArt.Rounded(3);
            // Opaque, so the overlaps between stones leave no darker seams.
            waterImage.color = Color.Lerp(Paper, MosseloraTheme.WaterChannel, 0.8f);
            waterImage.raycastTarget = false;
        }

        private static Sprite PathStone(int stream, bool here, bool passed)
        {
            if (here) return MosseloraArt.Load(MosseloraArt.StoneHere);
            if (passed) return MosseloraArt.Load(MosseloraArt.StoneWalked);
            int phase = stream >= 9 ? 3 : stream >= 6 ? 2 : stream >= 3 ? 1 : 0;
            switch (GardenSeason.ForStory(phase))
            {
                case Season.Spring: return MosseloraArt.Load("stone-spring") ?? MosseloraArt.Load(MosseloraArt.StoneSummer);
                case Season.Autumn: return MosseloraArt.Load(MosseloraArt.StoneAutumn);
                case Season.Winter: return MosseloraArt.Load(MosseloraArt.StoneWinter);
                default: return MosseloraArt.Load(MosseloraArt.StoneSummer);
            }
        }

        // Stones ahead wear the season they will be walked in (spring mint, summer
        // sage, autumn amber, winter frost): the year ahead is visible, never timed.
        private static Color SeasonStone(int stream)
        {
            int phase = stream >= 9 ? 3 : stream >= 6 ? 2 : stream >= 3 ? 1 : 0;
            switch (GardenSeason.ForStory(phase))
            {
                // Opaque season tones, each at least 3:1 against the paper
                // (WCAG 1.4.11); the old translucent stones measured about 1.6:1 (I84).
                case Season.Spring: return new Color(0.27f, 0.53f, 0.43f, 1f); // spring mint, darker than summer for 3:1
                case Season.Autumn: return new Color(0.72f, 0.47f, 0.20f, 1f);   // autumn 3.3:1
                case Season.Winter: return new Color(0.36f, 0.52f, 0.66f, 1f);   // winter 3.5:1
                default: return new Color(0.40f, 0.55f, 0.35f, 1f);  // summer 3.4:1
            }
        }

        // Thin leaf sprig between sections; purely decorative.
        private void Sprig(float height = 22f)
        {
            Sprite sprig = MosseloraArt.Load(MosseloraArt.Sprig);
            if (sprig == null) return;
            var node = Node("Leaf divider", content);
            Height(node, height);
            var image = node.AddComponent<Image>();
            image.sprite = sprig;
            image.preserveAspect = true;
            image.raycastTarget = false;
            image.color = new Color(1f, 1f, 1f, 0.9f);
        }

        // Full-bleed painting with its own aspect; crops nothing.
        private GameObject Painting(string name, string art, float maxHeight)
        {
            Sprite sprite = MosseloraArt.Load(art);
            if (sprite == null) return null;
            float width = PageWidth() + 34f;
            float height = Mathf.Min(maxHeight, width / MosseloraArt.Aspect(sprite));
            var node = Node(name, content);
            Height(node, height);
            var frame = Node(name + " frame", node.transform);
            var rect = frame.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(-17f, 0f);
            rect.offsetMax = new Vector2(17f, 0f);
            rect.sizeDelta = new Vector2(34f, width / MosseloraArt.Aspect(sprite));
            node.AddComponent<RectMask2D>();
            var image = frame.AddComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            return node;
        }

        // Logical width inside the page margins, from the real safe area.
        private float PageWidth()
        {
            // CanvasScaler.scaleFactor is only the constant-pixel setting; the live
            // ScaleWithScreenSize factor is on the Canvas. Prefer the laid-out page.
            // On the very first build after launch neither the layout nor
            // Canvas.scaleFactor is settled yet (the painted strip came out ~3x
            // too tall on a cold start), so derive the scale from the scaler's
            // own ScaleWithScreenSize formula, which is valid immediately.
            if (content != null && content.rect.width > 1f && Time.frameCount > 2)
                return Mathf.Max(225f, content.rect.width - 34f - columnLeft - columnRight);
            return Mathf.Max(225f, Screen.safeArea.width / LiveCanvasScale() - 34f - columnLeft - columnRight);
        }

        private float LiveCanvasScale()
        {
            if (viewportScaler == null || Screen.width <= 0 || Screen.height <= 0) return 1f;
            Vector2 reference = viewportScaler.referenceResolution;
            float logWidth = Mathf.Log(Screen.width / reference.x, 2f);
            float logHeight = Mathf.Log(Screen.height / reference.y, 2f);
            return Mathf.Max(0.1f, Mathf.Pow(2f, Mathf.Lerp(logWidth, logHeight,
                viewportScaler.matchWidthOrHeight)));
        }

        private GameObject Row(Transform parent, string name)
        {
            var row = Node(name, parent);
            var group = row.AddComponent<HorizontalLayoutGroup>();
            group.reverseArrangement = GameLocalization.IsRtl(GameLocalization.Current);
            group.spacing = 8;
            group.childAlignment = TextAnchor.MiddleCenter;
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = true;
            group.childForceExpandHeight = false;
            MinHeight(row, TextScale.Scale(54));
            return row;
        }
    }
}

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

        // The village's story at a glance: every choice, in order, as its
        // painted icon (no text, so screen readers hear only the journal lines).
        private void ChoiceTimeline(Transform parent)
        {
            int[] chosen = world.Choices;
            if (chosen.Length == 0) return;
            var strip = Node("Choice timeline", parent);
            var grid = strip.AddComponent<GridLayoutGroup>();
            // Each choice stands on the painted stone of the stream it was made in,
            // in its season, so the summary reads as the path walked through the
            // year, not an inventory tally (I161).
            const float cellHeight = 64f;
            grid.cellSize = new Vector2(50f, cellHeight);
            grid.spacing = new Vector2(4f, 6f);
            grid.childAlignment = TextAnchor.MiddleCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = Mathf.Min(6, chosen.Length);
            int rows = (chosen.Length + grid.constraintCount - 1) / grid.constraintCount;
            Height(strip, rows * cellHeight + (rows - 1) * 6f);
            for (int i = 0; i < chosen.Length; i++)
            {
                Sprite icon = ItemArt(i, chosen[i]);
                var cell = Node("Chapter choice", strip.transform);
                Sprite stone = PathStone(i, false, false);
                if (stone != null)
                {
                    var step = Node("Stone", cell.transform);
                    var stepRect = step.GetComponent<RectTransform>();
                    stepRect.anchorMin = new Vector2(0.08f, 0f);
                    stepRect.anchorMax = new Vector2(0.92f, 0.36f);
                    stepRect.offsetMin = stepRect.offsetMax = Vector2.zero;
                    var stepImage = step.AddComponent<Image>();
                    stepImage.sprite = stone;
                    stepImage.preserveAspect = true;
                    stepImage.raycastTarget = false;
                }
                if (icon != null)
                {
                    var picture = Node("Picture", cell.transform);
                    var pictureRect = picture.GetComponent<RectTransform>();
                    pictureRect.anchorMin = new Vector2(0f, stone != null ? 0.2f : 0f);
                    pictureRect.anchorMax = Vector2.one;
                    pictureRect.offsetMin = pictureRect.offsetMax = Vector2.zero;
                    var pictureImage = picture.AddComponent<Image>();
                    pictureImage.sprite = icon;
                    pictureImage.preserveAspect = true;
                    pictureImage.raycastTarget = false;
                }
            }
        }

        // "Orin remembers the bridge you restored." told beside Orin's portrait.
        private void KeeperLine(Transform parent, string name, int recall = -1)
        {
            int[] past = world.Choices;
            int index = recall >= 0 && recall < past.Length ? recall : past.Length - 1;
            Transform memory = CenteredRowIn(parent, name + " row");
            memory.GetComponent<HorizontalLayoutGroup>().spacing = 8;
            Sprite who = MosseloraArt.Portrait(past[index]);
            if (who != null)
            {
                var face = Node("Who remembers", memory);
                var faceLayout = face.AddComponent<LayoutElement>();
                faceLayout.minWidth = faceLayout.preferredWidth = 34;
                faceLayout.minHeight = faceLayout.preferredHeight = 34;
                var faceImage = face.AddComponent<Image>();
                faceImage.sprite = who;
                faceImage.preserveAspect = true;
                faceImage.raycastTarget = false;
            }
            Text line = Label(memory, name, Remembered(past, index), 14, 0, Ink, TextAnchor.MiddleLeft);
            Narrate(line);
            line.GetComponent<LayoutElement>().flexibleWidth = 1f;
        }

        // Same rule as PipeRules.Inspect leaks: a port with no partner
        // (board edge or a neighbor without the facing port) is open.
        private static int OpenPorts(Puzzle puzzle, int[] rotations, int tile)
        {
            int mask = 0;
            for (int d = 0; d < 4; d++)
            {
                if (!PipeRules.HasPort(puzzle.Kinds[tile], rotations[tile], d)) continue;
                int other = PipeRules.Neighbor(tile, d, puzzle.Size);
                if (other < 0 || !PipeRules.HasPort(puzzle.Kinds[other], rotations[other], (d + 2) % 4))
                    mask |= 1 << d;
            }
            return mask;
        }

        private void MeterEnd(Transform row, string art, float size, Color tint)
        {
            Sprite sprite = MosseloraArt.Load(art);
            if (sprite == null) return;
            var node = Node("Water meter " + (art == TreePlay.SeedIcon ? "garden" : "spring"), row);
            var layout = node.AddComponent<LayoutElement>();
            layout.minWidth = layout.preferredWidth = size;
            layout.minHeight = layout.preferredHeight = size;
            var image = node.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.color = tint;
            image.raycastTarget = false;
        }

        // Progress at a glance: a droplet per tile, filled as real water reaches it.
        // The sentence below stays as the exact, spoken statement.
        private void WaterMeter(int tiles, int reached)
        {
            Sprite drop = MosseloraArt.Load(MosseloraArt.Droplet);
            if (drop == null) return;
            Transform row = CenteredRow("Water meter", tiles > 9 ? 3f : 6f);
            MinHeight(row.gameObject, 26);
            waterDrops = new Image[tiles];
            float height = tiles > 9 ? 19f : 23f;
            // I350 (run 3, P7): the row starts at the spring and ends at the sprout, the same
            // marks as the board, so the drops read as water reaching the garden.
            MeterEnd(row, "glyph-waves", height + 4f, MosseloraTheme.WaterAction);
            for (int i = 0; i < tiles; i++)
            {
                var node = Node("Water drop", row);
                var layout = node.AddComponent<LayoutElement>();
                layout.minWidth = layout.preferredWidth = height * MosseloraArt.Aspect(drop);
                layout.minHeight = layout.preferredHeight = height;
                var image = node.AddComponent<Image>();
                image.sprite = drop;
                image.preserveAspect = true;
                image.raycastTarget = false;
                waterDrops[i] = image;
            }
            MeterEnd(row, TreePlay.SeedIcon, height + 4f, Color.white);
            PaintDrops(reached);
        }

        // Water that newly reaches a tile flies down into its drop, so the meter
        // explains itself without a word: each drop is a tile with water (I130).
        private void PaintDropsWithFlights(Puzzle puzzle, PipeInsight insight, float[] arrive = null)
        {
            int reached = world.Solved ? puzzle.CellCount : insight.ConnectedCount;
            int before = 0;
            if (lastFlowing != null) foreach (bool wet in lastFlowing) if (wet) before++;
            bool fly = gentleMotion && waterDrops != null && canvasRoot != null && tileArtwork != null &&
                lastFlowing != null && lastFlowing.Length == puzzle.CellCount && reached > before;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (qaInMemoryOnly) fly = false; // tour screenshots capture settled screens
#endif
            if (!fly) { PaintDrops(reached); return; }
            PaintDrops(before);
            int slot = before, order = 0;
            var fresh = new System.Collections.Generic.List<int>();
            for (int i = 0; i < puzzle.CellCount; i++)
                if (insight.Flowing[i] && !lastFlowing[i]) fresh.Add(i);
            // Nearest the spring first, leaving as each tile fills (I144).
            if (arrive != null) fresh.Sort((a, b) => arrive[a].CompareTo(arrive[b]));
            foreach (int i in fresh)
            {
                if (slot >= reached || slot >= waterDrops.Length) break;
                float wait = arrive != null ? arrive[i] : order * 0.07f;
                StartCoroutine(FlyDrop(tileArtwork[i].rectTransform, slot++, wait));
                order++;
            }
            // Drops not tied to a newly wet tile (a solve fills the rest) fill at once.
            for (; slot < reached && slot < waterDrops.Length; slot++) SetDrop(waterDrops[slot], true);
        }

        // Something arcs from mid-screen into where it's kept (I229 a pear into the
        // purse, I237 a letter into the menu button); its home swells once.
        private IEnumerator FlyInto(RectTransform target, string art, Vector2 size)
        {
            Sprite sprite = MosseloraArt.Load(art);
            if (sprite == null || target == null || canvasRoot == null) yield break;
            for (float wait = 0.35f; wait > 0f; wait -= Time.unscaledDeltaTime) yield return null;
            if (target == null) yield break;
            var node = Node("Arriving " + art, canvasRoot);
            var rect = node.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            var image = node.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
            Vector3 start = canvasRoot.TransformPoint(canvasRoot.rect.center);
            float lift = 90f * canvasRoot.lossyScale.y;
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / 0.8f)
            {
                if (target == null) { Destroy(node); yield break; }
                Vector3 end = target.TransformPoint(target.rect.center);
                float k = Mathf.SmoothStep(0f, 1f, t);
                Vector3 at = Vector3.Lerp(start, end, k);
                at.y += Mathf.Sin(k * Mathf.PI) * lift; // tossed up, then down into its place
                rect.position = at;
                rect.localScale = Vector3.one * Mathf.Lerp(1.2f, 0.6f, k);
                rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(k * Mathf.PI * 2f) * 14f);
                yield return null;
            }
            Destroy(node);
            if (target != null) BloomPulse.Add(target.gameObject, 0f, 0.25f);
        }

        private IEnumerator FlyDrop(RectTransform from, int slot, float startDelay)
        {
            Image[] drops = waterDrops;
            Sprite sprite = MosseloraArt.Load(MosseloraArt.Droplet);
            if (sprite == null || from == null || drops == null || slot >= drops.Length) yield break;
            for (float wait = startDelay; wait > 0f; wait -= Time.unscaledDeltaTime) yield return null;
            if (drops != waterDrops || from == null || drops[slot] == null) yield break;
            var node = Node("Flying drop", canvasRoot);
            var rect = node.GetComponent<RectTransform>();
            rect.sizeDelta = drops[slot].rectTransform.rect.size * 1.3f;
            var image = node.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
            Vector3 start = from.TransformPoint(from.rect.center);
            float lift = 36f * canvasRoot.lossyScale.y;
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / 0.5f)
            {
                if (drops != waterDrops || drops[slot] == null) { Destroy(node); yield break; }
                RectTransform target = drops[slot].rectTransform;
                Vector3 end = target.TransformPoint(target.rect.center);
                float k = Mathf.SmoothStep(0f, 1f, t);
                Vector3 at = Vector3.Lerp(start, end, k);
                at.y += Mathf.Sin(k * Mathf.PI) * lift; // a small arc, like a drop flicked off a leaf
                rect.position = at;
                rect.localScale = Vector3.one * Mathf.Lerp(1.15f, 0.8f, k);
                yield return null;
            }
            Destroy(node);
            if (drops != waterDrops || drops[slot] == null) yield break;
            Puzzle puzzle = world.CurrentPuzzle;
            int now = world.Solved ? puzzle.CellCount : world.Insight.ConnectedCount;
            if (slot < now)
            {
                SetDrop(drops[slot], true);
                BloomPulse.Add(drops[slot].gameObject, 0f, 0.22f);
            }
        }

        // Seconds after the move at which each newly wet tile fills: its step
        // count along connected pipes from the spring, relative to the nearest
        // newly wet tile, 0.07s a step. Null when there is nothing to stagger.
        private float[] WaterArrival(Puzzle puzzle, int[] angles, PipeInsight insight)
        {
            bool animate = gentleMotion && lastFlowing != null && lastFlowing.Length == puzzle.CellCount;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (qaInMemoryOnly) animate = false; // tour screenshots capture settled screens
#endif
            if (!animate) return null;
            int spring = Array.IndexOf(puzzle.Kinds, PipeKind.End);
            if (spring < 0 || !insight.Flowing[spring]) return null;
            int[] steps = new int[puzzle.CellCount];
            for (int i = 0; i < steps.Length; i++) steps[i] = -1;
            var queue = new System.Collections.Generic.Queue<int>();
            steps[spring] = 0;
            queue.Enqueue(spring);
            while (queue.Count > 0)
            {
                int tile = queue.Dequeue();
                for (int d = 0; d < 4; d++)
                {
                    if (!PipeRules.HasPort(puzzle.Kinds[tile], angles[tile], d)) continue;
                    int next = PipeRules.Neighbor(tile, d, puzzle.Size);
                    if (next < 0 || steps[next] >= 0 || !insight.Flowing[next]) continue;
                    if (!PipeRules.HasPort(puzzle.Kinds[next], angles[next], (d + 2) % 4)) continue;
                    steps[next] = steps[tile] + 1;
                    queue.Enqueue(next);
                }
            }
            int nearest = int.MaxValue;
            for (int i = 0; i < steps.Length; i++)
                if (steps[i] >= 0 && !lastFlowing[i]) nearest = Mathf.Min(nearest, steps[i]);
            if (nearest == int.MaxValue) return null;
            var arrive = new float[puzzle.CellCount];
            for (int i = 0; i < steps.Length; i++)
                arrive[i] = steps[i] >= 0 && !lastFlowing[i] ? (steps[i] - nearest) * 0.07f : 0f;
            return arrive;
        }

        // Fills one tile once the running water reaches it, from the board's
        // state at that moment (another move may have come in between).
        private IEnumerator WetLater(int tile, float delay)
        {
            int generation = rebuildGeneration;
            PipeGraphic[] artwork = tileArtwork;
            for (float wait = delay; wait > 0f; wait -= Time.unscaledDeltaTime) yield return null;
            if (generation != rebuildGeneration || artwork != tileArtwork || tileArtwork == null ||
                tile >= tileArtwork.Length || world.Completed) yield break;
            Puzzle puzzle = world.CurrentPuzzle;
            if (tile >= puzzle.CellCount) yield break;
            PaintRefreshedTile(puzzle, world.Insight, world.Rotations, tile, false);
        }

        // The one in-place visual rule for a tile (refresh and the running water).
        private void PaintRefreshedTile(Puzzle puzzle, PipeInsight insight, int[] angles, int i, bool held)
        {
            tileBackgrounds[i].color = MosseloraTheme.TileFill(world.Solved,
                !held && insight.Flowing[i], puzzle.IsAnchored(i), world.HintStep,
                world.HintTile, i, puzzle.Size);
            tileArtwork[i].Configure(puzzle.Kinds[i], angles[i],
                held ? false : insight.Flowing[i], world.Solved, OpenPorts(puzzle, angles, i));
        }

        private void PaintDrops(int reached)
        {
            if (waterDrops == null) return;
            for (int i = 0; i < waterDrops.Length; i++) SetDrop(waterDrops[i], i < reached);
        }

        // A filled drop is the painted droplet; an empty one is its slate outline
        // with a faint fill, so the row reads "still to fill", never "lives
        // left" (I146). The outline keeps I105's >=3:1 contrast on paper.
        private static Sprite dropFull, dropEmpty;

        private static void SetDrop(Image drop, bool wet)
        {
            if (dropFull == null) dropFull = MosseloraArt.Load(MosseloraArt.Droplet);
            if (dropEmpty == null) dropEmpty = MosseloraArt.Load(MosseloraArt.DropletEmpty);
            if (wet || dropEmpty == null)
            {
                if (dropFull != null) drop.sprite = dropFull;
                drop.color = wet ? Color.white : new Color(0.36f, 0.40f, 0.37f, 0.85f);
                return;
            }
            drop.sprite = dropEmpty;
            drop.color = new Color(0.30f, 0.35f, 0.33f, 1f);
        }
    }
}

using Evergrove.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    public sealed partial class EvergroveApp
    {
        // Selection belongs to this view, never to the save. A relaunch returns
        // to the solved stream with all three choices still available.
        private VillageChoicePreview choicePreview;
        private WorldModel choicePreviewSource;
        private int choicePreviewRound = -1;
        private RectTransform choicePreviewPanel;
        private bool revealChoicePreview;
        private bool revealChoices;

        private void PreviewChoice(VillageChoice choice)
        {
            if (settlingChoice) return;
            VillageChoicePreview preview;
            if (!VillageChoicePreview.TryCreate(world, choice, out preview)) return;
            choicePreview = preview;
            choicePreviewSource = world;
            choicePreviewRound = world.Round;
            revealChoicePreview = true;
            Rebuild();
        }

        private void RefreshChoicePreview()
        {
            if (choicePreview == null) return;
            VillageChoicePreview refreshed;
            if (!ReferenceEquals(choicePreviewSource, world) || world.Round != choicePreviewRound ||
                !VillageChoicePreview.TryCreate(world, choicePreview.Choice, out refreshed))
            {
                ClearChoicePreview();
                return;
            }
            choicePreview = refreshed;
        }

        private void ClearChoicePreview()
        {
            choicePreview = null;
            choicePreviewSource = null;
            choicePreviewRound = -1;
            revealChoicePreview = false;
        }

        private void CloseChoicePreview()
        {
            ClearChoicePreview();
            revealChoices = true;
            Rebuild();
        }

        private void ApplyChoicePreview(Transform cards, VillageChoicePreview expected, int generation)
        {
            // Old UI and accessibility callbacks can outlive a rebuild for one
            // frame. They must never commit a different, newly selected card.
            if (generation != rebuildGeneration || !ReferenceEquals(choicePreview, expected)) return;
            RefreshChoicePreview();
            if (choicePreview == null || settlingChoice) return;
            // Keep the same authoritative commit, save, research event and
            // reversible memory screen as ordinary play. Never save WorldAfter.
            SettleChoice(cards, choicePreview.Choice);
        }

        private void BuildChoicePreview(Transform cards)
        {
            if (choicePreview == null) return;
            VillageChoicePreview expected = choicePreview;
            int generation = rebuildGeneration;
            var preview = Card(content, "Choice preview", MosseloraTheme.CardFill);
            choicePreviewPanel = preview.GetComponent<RectTransform>();
            // I394 (run 3, P20): the thing you tapped, by its own name ("Weathervane"), as on its
            // card; "Before you choose" is spoken only, the open card already says it.
            SpokenOnly(Label(preview.transform, "Choice preview heading", GameLocalization.Text("preview_title"),
                16, 0, Moss, TextAnchor.MiddleCenter));
            Text choiceName = Label(preview.transform, "Choice preview name",
                ChoiceName(choicePreview.Choice), 19, 0, Ink, TextAnchor.MiddleCenter);
            Emphasize(choiceName);
            Label(preview.transform, "Choice preview consequence",
                LocalizedVillage.Consequence(choicePreview.WorldAfter),
                17, 0, Ink, TextAnchor.MiddleCenter);

            Puzzle next = choicePreview.NextPuzzle;
            if (next != null)
            {
                var nextStream = Node("Next stream preview", preview.transform);
                var stack = nextStream.AddComponent<VerticalLayoutGroup>();
                stack.spacing = 6;
                stack.childAlignment = TextAnchor.UpperCenter;
                stack.childControlWidth = stack.childControlHeight = true;
                stack.childForceExpandWidth = true;
                stack.childForceExpandHeight = false;
                // I394 (P19): the stepping stones already count the streams; spoken only.
                SpokenOnly(Label(nextStream.transform, "Next stream preview heading",
                    GameLocalization.Text("next") + " · " +
                    GameLocalization.Text("stream", choicePreview.WorldAfter.Round + 1, PuzzleCatalog.Count),
                    15, 0, Ink, TextAnchor.MiddleCenter));
                BuildPreviewAnchor(nextStream.transform, choicePreview);
                Label(nextStream.transform, "Next stream preview rule",
                    LocalizedVillage.EnvironmentalRule(choicePreview.WorldAfter),
                    16, 0, Ink, TextAnchor.MiddleCenter);
            }

            // I348 (run 3, P11): a tick to make it so and a cross to step back, as round
            // tokens like every other decision in the garden. Their names stay spoken.
            Transform decide = CenteredRowIn(preview.transform, "Choice preview decision");
            decide.GetComponent<HorizontalLayoutGroup>().spacing = 28;
            IconButton(decide, "Close choice preview", GameLocalization.Text("close"), MosseloraArt.IconCancel,
                () => {
                    if (generation == rebuildGeneration && ReferenceEquals(choicePreview, expected))
                        CloseChoicePreview();
                }, 64, MosseloraTheme.QuietAction, false, null, false, 0, 0, true);
            IconButton(decide, "Apply garden choice", GameLocalization.Text("preview_apply"), MosseloraArt.IconConfirm,
                () => ApplyChoicePreview(cards, expected, generation), 80, MosseloraTheme.ForestAction,
                false, null, false, 0, 0, true);
        }

        private void BuildPreviewAnchor(Transform parent, VillageChoicePreview preview)
        {
            Puzzle next = preview.NextPuzzle;
            int anchor = next.AnchoredTile;
            if (anchor < 0) return;
            Transform row = CenteredRowIn(parent, "Next stream landmark");
            var tile = Node("Actual protected pipe", row);
            Height(tile, 76);
            var layout = tile.GetComponent<LayoutElement>();
            layout.minWidth = layout.preferredWidth = 76;
            var body = tile.AddComponent<Image>();
            MosseloraArt.Round(body, MosseloraTheme.AnchoredTile, 10);
            body.raycastTarget = false;
            var frame = Node("Landmark frame", tile.transform);
            Fill(frame.GetComponent<RectTransform>());
            frame.AddComponent<LandmarkGraphic>().Configure((int)preview.Choice + 1);
            var channel = Node("Actual channel", tile.transform);
            Fill(channel.GetComponent<RectTransform>());
            PipeInsight insight = preview.WorldAfter.Insight;
            var pipe = channel.AddComponent<PipeGraphic>();
            pipe.Configure(next.Kinds[anchor], next.Initial[anchor], insight.Flowing[anchor], false,
                OpenPorts(next, next.Initial, anchor));
            pipe.raycastTarget = false;
            Sprite mark = MosseloraArt.ChoiceIcon(preview.Choice);
            if (mark != null)
                MosseloraArt.Ornament(tile.transform, "Protected by", mark, new Vector2(0f, 1f),
                    new Vector2(0f, 1f), new Vector2(24f, 24f), new Vector2(1f, -1f));

            string kind = GameLocalization.Text(next.Kinds[anchor] == PipeKind.Corner
                ? "sr_corner" : "sr_straight");
            // I348: "row 1, column 1: corner pipe, dry" is for screen readers; the tile shows it.
            Text position = Label(parent, "Next stream landmark position",
                GameLocalization.Text(insight.Flowing[anchor] ? "sr_tile_wet" : "sr_tile_dry",
                    anchor / next.Size + 1, anchor % next.Size + 1, kind),
                15, 0, new Color(0f, 0f, 0f, 0f), TextAnchor.MiddleCenter);
            position.GetComponent<LayoutElement>().ignoreLayout = true;
        }

        private void RevealChoicePreview()
        {
            // Keep all three cards in view with the preview whenever they fit.
            // On compact screens, the preview remains scrollable and Close
            // returns to the choices without touching the solved board.
            float clearance = ChromeClearance() + RevealMarginPx;
            float panelTop = DepthInContent(choicePreviewPanel, true);
            float panelBottom = DepthInContent(choicePreviewPanel, false);
            float target = panelTop;
            if (decisionHeading != null)
            {
                float choicesTop = DepthInContent(decisionHeading, true);
                if (panelBottom - choicesTop <= scroll.viewport.rect.height - clearance - 8f)
                    target = choicesTop;
            }
            RevealChoiceDepth(target, clearance);
        }

        private void RevealChoices()
        {
            RevealChoiceDepth(DepthInContent(decisionHeading, true), ChromeClearance() + RevealMarginPx);
        }

        private void RevealChoiceDepth(float top, float clearance)
        {
            scroll.StopMovement();
            float distance = content.rect.height - scroll.viewport.rect.height;
            scroll.verticalNormalizedPosition = distance <= 0f ? 1f :
                1f - Mathf.Clamp01((top - clearance) / distance);
        }
    }
}

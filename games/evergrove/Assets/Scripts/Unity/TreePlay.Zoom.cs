using System.Collections.Generic;
using Evergrove.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Checklist 91 (founder, 2026-11-28): a board whose cells are under 48 pt opens whole, as a picture.
    /// One tap zooms in around that spot (BoardZoomRule) so every cell is at least 48 pt; the zoomed board
    /// pans by dragging; "See the whole board" zooms back out. While whole, the cells take no taps (the
    /// picture does), so every live target is at least 48 pt. Only transforms move.
    /// </summary>
    internal sealed partial class TreePlay
    {
        private RectTransform zoomView, zoomBoard;
        private GameObject zoomCatcher, zoomOut;
        private CanvasGroup zoomCells;
        private ScrollRect zoomPan;
        private float zoomScale = 1f;

        internal bool Zoomable { get { return zoomBoard != null; } }
        internal bool Zoomed { get { return zoomBoard != null && zoomBoard.localScale.x > 1.001f; } }
        internal float ZoomScale { get { return zoomScale; } }

        // After the card has settled (KeepCardOnScreen): find the board grid, measure its smallest cell,
        // and wrap it in a zoom view when that cell is under the minimum.
        private void InstallZoom()
        {
            if (cardRect == null || zoomBoard != null) return;
            Canvas.ForceUpdateCanvases();
            Canvas canvas = GetComponentInParent<Canvas>();
            var root = canvas != null ? canvas.rootCanvas.transform as RectTransform : null;
            RectTransform grid = BoardGrid(cardRect, out float smallest, root);
            if (grid == null || !BoardZoomRule.Needed(smallest)) return;
            zoomScale = BoardZoomRule.Scale(smallest);
            Wrap(grid);
        }

        // The child that holds the most live cells (at least nine): that is the board's grid.
        private static RectTransform BoardGrid(RectTransform card, out float smallest, RectTransform root)
        {
            smallest = 0f;
            var counts = new Dictionary<RectTransform, int>();
            foreach (Selectable cell in card.GetComponentsInChildren<Selectable>(false))
            {
                if (!cell.IsInteractable() || !(cell.transform.parent is RectTransform parent)) continue;
                counts.TryGetValue(parent, out int n);
                counts[parent] = n + 1;
            }
            RectTransform best = null;
            int most = 4; // five or more live cells make a board (the sowing beds are seven)
            foreach (KeyValuePair<RectTransform, int> pair in counts)
                if (pair.Value > most) { most = pair.Value; best = pair.Key; }
            if (best == null || root == null) return best;
            smallest = float.MaxValue;
            foreach (Transform child in best)
            {
                var hit = child.GetComponent<Graphic>();
                if (hit == null || !hit.raycastTarget) continue;
                Vector2 size = TouchTargets.TouchSize(hit, root);
                smallest = Mathf.Min(smallest, Mathf.Min(size.x, size.y));
            }
            if (smallest == float.MaxValue) smallest = 0f;
            return best;
        }

        private Vector2 zoomCentre; // where the cells' own centre lies in the grid, unscaled

        // The grid moves into a clipped view that takes its place in the card's layout. The view is as big
        // as the cells really are (some boards place cells outside their container's own rect).
        private void Wrap(RectTransform grid)
        {
            Transform parent = grid.parent;
            int at = grid.GetSiblingIndex();
            Vector2 own = grid.rect.size;
            var view = new GameObject("Board zoom view", typeof(RectTransform));
            view.transform.SetParent(parent, false);
            view.transform.SetSiblingIndex(at);
            zoomView = (RectTransform)view.transform;
            var place = view.AddComponent<LayoutElement>();
            view.AddComponent<RectMask2D>();
            grid.SetParent(zoomView, false);
            var ownPlace = grid.GetComponent<LayoutElement>();
            if (ownPlace != null) ownPlace.ignoreLayout = true;
            grid.anchorMin = grid.anchorMax = grid.pivot = new Vector2(0.5f, 0.5f);
            grid.sizeDelta = own; // the grid keeps its size, so its cells keep their places
            Bounds cells = RectTransformUtility.CalculateRelativeRectTransformBounds(grid);
            zoomCentre = cells.center;
            Vector2 size = new Vector2(Mathf.Max(own.x, cells.size.x), Mathf.Max(own.y, cells.size.y));
            place.minWidth = place.preferredWidth = size.x;
            place.minHeight = place.preferredHeight = size.y;
            grid.anchoredPosition = -zoomCentre;
            zoomBoard = grid;
            zoomCells = grid.gameObject.AddComponent<CanvasGroup>();
            zoomPan = view.AddComponent<ScrollRect>();
            zoomPan.content = grid;
            zoomPan.viewport = zoomView;
            zoomPan.movementType = ScrollRect.MovementType.Clamped;
            zoomPan.inertia = true;
            zoomPan.scrollSensitivity = 0f;
            // The whole board, as a picture: one tap zooms in on that spot.
            zoomCatcher = new GameObject("Tap the board to zoom in", typeof(RectTransform));
            zoomCatcher.transform.SetParent(zoomView, false);
            var catcherRect = (RectTransform)zoomCatcher.transform;
            catcherRect.anchorMin = Vector2.zero;
            catcherRect.anchorMax = Vector2.one;
            catcherRect.offsetMin = catcherRect.offsetMax = Vector2.zero;
            var catcherImage = zoomCatcher.AddComponent<Image>();
            catcherImage.color = new Color(0f, 0f, 0f, 0f);
            zoomCatcher.AddComponent<ZoomTap>().Tapped = local => ZoomIn(local);
            LongPressCaption.Attach(zoomCatcher, "Tap a part of the board to see it larger.", LongPressCaption.DefaultFont);
            // Back out to the whole board: a round token over the view's top-left corner, shown while zoomed.
            Token(zoomView, "See the whole board", MosseloraArt.Load(MosseloraArt.IconBoardSize), 52f,
                MosseloraTheme.ForestAction, ZoomWhole, true);
            zoomOut = zoomView.Find("See the whole board").gameObject;
            var outRect = (RectTransform)zoomOut.transform;
            outRect.GetComponent<LayoutElement>().ignoreLayout = true;
            outRect.anchorMin = outRect.anchorMax = outRect.pivot = new Vector2(0f, 1f);
            outRect.anchoredPosition = new Vector2(6f, -6f);
            outRect.sizeDelta = new Vector2(52f, 52f);
            ZoomWhole();
        }

        // Zoom in so the tapped spot (in the view's local points) comes to the middle.
        private void ZoomIn(Vector2 local)
        {
            if (zoomBoard == null) return;
            Vector2 size = zoomView.rect.size;
            zoomBoard.localScale = new Vector3(zoomScale, zoomScale, 1f);
            zoomBoard.anchoredPosition = new Vector2(
                BoardZoomRule.Offset(local.x, size.x, zoomScale), BoardZoomRule.Offset(local.y, size.y, zoomScale))
                - zoomCentre * zoomScale;
            zoomCells.interactable = zoomCells.blocksRaycasts = true;
            zoomPan.enabled = true;
            zoomCatcher.SetActive(false);
            zoomOut.SetActive(true);
            zoomOut.transform.SetAsLastSibling();
        }

        // The whole board again: cells rest (the picture takes the tap), nothing to pan.
        private void ZoomWhole()
        {
            if (zoomBoard == null) return;
            zoomPan.StopMovement();
            zoomPan.enabled = false;
            zoomBoard.localScale = Vector3.one;
            zoomBoard.anchoredPosition = -zoomCentre;
            zoomCells.interactable = zoomCells.blocksRaycasts = false;
            zoomCatcher.SetActive(true);
            zoomCatcher.transform.SetAsLastSibling();
            zoomOut.SetActive(false);
        }

        /// <summary>Solved: the whole board shows again, and nothing more is taken.</summary>
        private void ZoomDone()
        {
            if (zoomBoard == null) return;
            ZoomWhole();
            zoomCatcher.SetActive(false);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // Tour only: zoom in on the board's centre, or back out.
        internal void QaZoom(bool inward) { if (inward) ZoomIn(Vector2.zero); else ZoomWhole(); }
#endif

        /// <summary>Reports where on the view a tap landed, in the view's local points (centre origin).</summary>
        private sealed class ZoomTap : MonoBehaviour, IPointerClickHandler
        {
            internal System.Action<Vector2> Tapped;

            public void OnPointerClick(PointerEventData eventData)
            {
                var rect = (RectTransform)transform.parent;
                if (Tapped == null || rect == null) return;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, eventData.position,
                        eventData.pressEventCamera, out Vector2 local))
                    Tapped(local - rect.rect.center);
            }
        }
    }
}

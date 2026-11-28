using UnityEngine;
using UnityEngine.EventSystems;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Checklist 19: press and hold any control and its name appears in a gentle
    /// toast above it (<see cref="RuleToast"/>), in place of help pages. The hold
    /// never presses the control: the release after a shown caption is not a click.
    /// Movement beyond <see cref="CancelDistance"/> cancels the hold, and drags are
    /// handed to the nearest ancestor drag handler (the ScrollRect or a page turn)
    /// so lists still scroll under a finger that rests on a button.
    /// The toast fades by alpha only, so it needs no reduced-motion branch.
    /// </summary>
    internal sealed class LongPressCaption : MonoBehaviour,
        IInitializePotentialDragHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler,
        IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public const float HoldSeconds = 0.6f;
        public const float CancelDistance = 12f;

        /// <summary>
        /// Font for static builders (TreePlay.Token, BoardKit.Cell, map items) that have no
        /// font of their own. Set once when the app starts.
        /// </summary>
        public static Font DefaultFont;

        /// <summary>
        /// Raised each time a caption is shown (checklist 5: the hand question waits for the first long-press).
        /// </summary>
        internal static event System.Action Shown;

        private string message;
        private Font font;
        private PointerEventData press;
        private Vector2 origin;
        private float held = -1f;

        /// <summary>
        /// Adds the hold to a control. Returns null (and adds nothing) when there is
        /// no caption to explain, which is what the coverage audit looks for.
        /// </summary>
        public static LongPressCaption Attach(GameObject node, string message, Font font)
        {
            if (node == null || string.IsNullOrWhiteSpace(message)) return null;
            var existing = node.GetComponent<LongPressCaption>();
            if (existing != null) return existing;
            var tip = node.AddComponent<LongPressCaption>();
            tip.message = message;
            tip.font = font;
            return tip;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            press = eventData;
            origin = eventData.position;
            held = 0f;
        }

        public void OnPointerUp(PointerEventData eventData) { Cancel(); }
        public void OnPointerExit(PointerEventData eventData) { Cancel(); }
        private void OnDisable() { Cancel(); }

        private void Update()
        {
            if (held < 0f) return;
            held += Time.unscaledDeltaTime;
            if (held >= HoldSeconds) Show();
        }

        private void Show()
        {
            held = -1f;
            if (font == null) return; // nothing legible to draw with: no toast, and the tap stays a tap
            if (press != null) press.eligibleForClick = false; // the hold asked a question; it did not press
            RuleToast.Show((RectTransform)transform, message, font, MosseloraTheme.Ink, MosseloraTheme.CardFill);
            var shown = Shown;
            if (shown != null) shown();
        }

        private void Cancel()
        {
            held = -1f;
            press = null;
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            Forward(eventData, ExecuteEvents.initializePotentialDrag);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            Cancel();
            Forward(eventData, ExecuteEvents.beginDragHandler);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (held >= 0f && Vector2.Distance(eventData.position, origin) > CancelDistance) Cancel();
            Forward(eventData, ExecuteEvents.dragHandler);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Cancel();
            Forward(eventData, ExecuteEvents.endDragHandler);
        }

        // Drags start on this node (it is the first drag handler under the finger),
        // so they are passed up to whatever would have received them without it.
        private void Forward<T>(PointerEventData eventData, ExecuteEvents.EventFunction<T> function)
            where T : IEventSystemHandler
        {
            Transform parent = transform.parent;
            if (parent != null) ExecuteEvents.ExecuteHierarchy(parent.gameObject, eventData, function);
        }
    }
}

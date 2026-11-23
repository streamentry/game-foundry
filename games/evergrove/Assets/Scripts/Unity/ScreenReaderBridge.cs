using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Accessibility;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// First-pass screen-reader hierarchy for the uGUI puzzle screens (EG-113).
    /// <see cref="Collect"/> walks the generated content in reading order and is
    /// pure UI geometry plus text, so it runs in any build. <see cref="Publish"/>
    /// hands the result to VoiceOver/TalkBack only on iOS/Android while a screen
    /// reader is running; every other platform keeps the previous behavior.
    /// Not device-verified: a human must test with VoiceOver or TalkBack (EG-113).
    /// </summary>
    public static class ScreenReaderBridge
    {
        /// <summary>Spoken label and optional hint for an interactive button.</summary>
        public readonly struct Speech
        {
            public readonly string Label;
            public readonly string Hint;

            public Speech(string label, string hint)
            {
                Label = label;
                Hint = hint;
            }
        }

        /// <summary>One published node, captured on the main thread.</summary>
        public sealed class SemanticNode
        {
            public readonly string Label;
            public readonly string Hint;
            public readonly AccessibilityRole Role;
            public readonly AccessibilityState State;
            public readonly RectTransform Rect;
            public readonly Camera Camera;
            public readonly Action Activate; // null for non-interactive text

            public SemanticNode(string label, string hint, AccessibilityRole role,
                                AccessibilityState state, RectTransform rect, Camera camera,
                                Action activate)
            {
                Label = label;
                Hint = hint;
                Role = role;
                State = state;
                Rect = rect;
                Camera = camera;
                Activate = activate;
            }
        }

        /// <summary>True only where a screen reader is actually running.</summary>
        public static bool Active
        {
            get
            {
#if UNITY_IOS || UNITY_ANDROID
                return AssistiveSupport.isScreenReaderEnabled;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// Reading-order snapshot of the generated screen. Buttons are taken as one
        /// node each (their caption is the label) and are not descended into, so
        /// tile internals such as leak markers are not announced twice.
        /// <paramref name="describe"/> may override a button's label and hint.
        /// </summary>
        public static List<SemanticNode> Collect(RectTransform root, Func<Button, Speech?> describe)
        {
            var nodes = new List<SemanticNode>();
            if (root == null) return nodes;
            Camera camera = CameraFor(root);
            Walk(root, nodes, describe, camera);
            return nodes;
        }

        /// <summary>
        /// Replaces the published hierarchy. No-op unless a screen reader is active,
        /// so desktop, editor and screen-reader-off runs never touch AssistiveSupport.
        /// </summary>
        public static void Publish(List<SemanticNode> nodes)
        {
            if (nodes == null || !Active) return;
            var hierarchy = new AccessibilityHierarchy();
            foreach (SemanticNode item in nodes)
            {
                AccessibilityNode node = hierarchy.AddNode(item.Label, null);
                node.role = item.Role;
                node.state = item.State;
                if (!string.IsNullOrEmpty(item.Hint)) node.hint = item.Hint;
                RectTransform rect = item.Rect;
                Camera camera = item.Camera;
                node.frame = ScreenFrame(rect, camera);
                node.frameGetter = () => ScreenFrame(rect, camera);
                Action activate = item.Activate;
                if (activate != null)
                {
                    node.invoked += () => {
                        if (activate == null) return false;
                        activate();
                        return true;
                    };
                }
            }
            AssistiveSupport.activeHierarchy = hierarchy;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            LogPublished(nodes);
#endif
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        /// <summary>Dev-only: one line listing the published labels for QA inspection.</summary>
        public static void LogForQa(List<SemanticNode> nodes)
        {
            var labels = new List<string>(nodes.Count);
            foreach (SemanticNode item in nodes)
                labels.Add(item.Role + ": " + item.Label +
                    (string.IsNullOrEmpty(item.Hint) ? "" : " [hint: " + item.Hint + "]"));
            Debug.Log("MOSSELORA_SR_NODES " + nodes.Count + " | " + string.Join(" | ", labels));
        }

        /// <summary>Dev-only: reports whether the OS screen reader is on, and why it was asked.</summary>
        public static void LogStatus(string reason)
        {
            Debug.Log("MOSSELORA_SR_STATUS enabled=" + AssistiveSupportEnabled() + " reason=" + reason);
        }

        private static bool AssistiveSupportEnabled()
        {
#if UNITY_IOS || UNITY_ANDROID
            return AssistiveSupport.isScreenReaderEnabled;
#else
            return false;
#endif
        }

        private static void LogPublished(List<SemanticNode> nodes)
        {
            string first = "none";
            if (nodes.Count > 0 && nodes[0].Rect != null)
                first = nodes[0].Label + " frame=" + ScreenFrame(nodes[0].Rect, nodes[0].Camera);
            Debug.Log("MOSSELORA_SR_PUBLISHED " + nodes.Count + " first=" + first);
        }
#endif

        private static void Walk(Transform parent, List<SemanticNode> output,
                                 Func<Button, Speech?> describe, Camera camera)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (!child.gameObject.activeInHierarchy) continue;
                var button = child.GetComponent<Button>();
                if (button != null)
                {
                    output.Add(ButtonNode(button, describe, camera));
                    continue;
                }
                var text = child.GetComponent<Text>();
                var rect = child as RectTransform;
                if (text != null && rect != null && !string.IsNullOrWhiteSpace(text.text))
                {
                    AccessibilityRole role = IsHeading(child.name)
                        ? AccessibilityRole.Header : AccessibilityRole.StaticText;
                    output.Add(new SemanticNode(text.text.Trim(), null, role,
                        AccessibilityState.None, rect, camera, null));
                }
                Walk(child, output, describe, camera);
            }
        }

        private static SemanticNode ButtonNode(Button button, Func<Button, Speech?> describe,
                                               Camera camera)
        {
            string label = CaptionOf(button);
            string hint = null;
            Speech? speech = describe != null ? describe(button) : null;
            if (speech.HasValue)
            {
                label = speech.Value.Label;
                hint = speech.Value.Hint;
            }
            var rect = button.GetComponent<RectTransform>();
            return new SemanticNode(label, hint, AccessibilityRole.Button,
                button.interactable ? AccessibilityState.None : AccessibilityState.Disabled,
                rect, camera, () => {
                    // Same listener path as a pointer tap, so rules and saves are shared.
                    if (button != null && button.interactable) button.onClick.Invoke();
                });
        }

        private static string CaptionOf(Button button)
        {
            Transform caption = button.transform.Find("Caption");
            var text = caption != null ? caption.GetComponent<Text>() : null;
            string value = text != null ? text.text : button.name;
            // Choice captions carry an outcome line after a newline; speak it as a sentence.
            return string.IsNullOrWhiteSpace(value)
                ? button.name
                : value.Replace("\r", "").Replace("\n", ". ").Trim();
        }

        private static bool IsHeading(string name)
        {
            switch (name)
            {
                case "Mosselora wordmark":
                case "Title":
                case "Chapter":
                case "Choose title":
                case "Ending title":
                case "Moment title":
                case "Journal title":
                    return true;
                default:
                    return false;
            }
        }

        private static Camera CameraFor(RectTransform rect)
        {
            Canvas canvas = rect.GetComponentInParent<Canvas>();
            if (canvas == null) return null;
            canvas = canvas.rootCanvas;
            return canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        }

        // Unity screen coordinates (origin bottom-left, pixels) of a UI rect.
        private static Rect ScreenFrame(RectTransform rect, Camera camera)
        {
            if (rect == null) return Rect.zero;
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            Vector2 min = Vector2.positiveInfinity;
            Vector2 max = Vector2.negativeInfinity;
            foreach (Vector3 corner in corners)
            {
                Vector2 point = RectTransformUtility.WorldToScreenPoint(camera, corner);
                min = Vector2.Min(min, point);
                max = Vector2.Max(max, point);
            }
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }
    }
}

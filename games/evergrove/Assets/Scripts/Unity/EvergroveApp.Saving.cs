using System;
using System.Globalization;
using Evergrove.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Item 96: saves are journaled (WorldStore), and the player can see that a save was kept
    /// (a quiet "Saved" mark on the hub) and can send the garden file to the system share sheet
    /// (iOS "Save to Files", Android Files or Drive). Nothing here leaves the device by itself.
    /// </summary>
    public sealed partial class EvergroveApp
    {
        // Bursts of saves share one mark: a new one appears at most every 1.5 s.
        private const float SavedMarkMinGap = 1.5f;
        // Motion on: hold, then fade with alpha only. Motion reduced: appear, stay, disappear.
        private const float SavedMarkHold = 1.3f;
        private const float SavedMarkFade = 1.2f;
        private const float SavedMarkStillHold = 1.5f;

        private float lastSavedMarkAt = -999f;
        private GameObject savedMark;


        // Called by SaveWorld after a successful write only. Shows the mark on the hub, and
        // only there: not while a puzzle card, a sheet, the quiet garden or the menu is open.
        private void NoteSaved()
        {
            if (!HubIsOnTop()) return;
            float now = Time.unscaledTime;
            if (now - lastSavedMarkAt < SavedMarkMinGap) return;
            lastSavedMarkAt = now;
            if (savedMark != null) Destroy(savedMark);
            savedMark = BuildSavedMark(titleScreen.transform);
        }

        private bool HubIsOnTop()
        {
            return titleScreen != null && titleScreen.activeInHierarchy
                && openCard == null && familySheet == null && letterCard == null
                && quietLayer == null && !menuOpen;
        }

        // Top-left, under the menu button, in the calm corner the hub leaves free. The mark
        // does not block taps and never takes focus.
        private GameObject BuildSavedMark(Transform hub)
        {
            GameObject node = Node("Saved mark", hub);
            var rect = node.GetComponent<RectTransform>();
            Rect safe = Screen.safeArea;
            float w = Mathf.Max(1, Screen.width), h = Mathf.Max(1, Screen.height);
            rect.anchorMin = rect.anchorMax = new Vector2(safe.xMin / w, safe.yMax / h);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(0f, 40f);
            rect.anchoredPosition = new Vector2(12f, ChromeTop - 66f);

            var plate = node.AddComponent<Image>();
            Color paper = MosseloraTheme.CardFill;
            MosseloraArt.Round(plate, new Color(paper.r, paper.g, paper.b, 0.94f), 20);
            plate.raycastTarget = false;
            var fit = node.AddComponent<ContentSizeFitter>();
            fit.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            var row = node.AddComponent<HorizontalLayoutGroup>();
            row.padding = new RectOffset(10, 14, 6, 6);
            row.spacing = 6f;
            row.childAlignment = TextAnchor.MiddleCenter;
            row.childControlWidth = true;
            row.childControlHeight = true;
            row.childForceExpandWidth = false;
            row.childForceExpandHeight = false;

            GameObject leaf = Node("Saved sprig", node.transform);
            var leafLayout = leaf.AddComponent<LayoutElement>();
            leafLayout.preferredWidth = leafLayout.preferredHeight = 24f;
            var leafImage = leaf.AddComponent<Image>();
            leafImage.sprite = MosseloraArt.Load(MosseloraArt.Sprig);
            leafImage.preserveAspect = true;
            leafImage.raycastTarget = false;

            GameObject words = Node("Saved words", node.transform);
            var label = words.AddComponent<Text>();
            label.font = font;
            label.fontSize = TextScale.Scale(15);
            label.color = Ink;
            label.alignment = TextAnchor.MiddleLeft;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false;
            label.text = GameLocalization.Text("saved");

            var group = node.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            group.interactable = false;
            var fade = node.AddComponent<SavedMarkTimer>();
            fade.Group = group;
            fade.Motion = gentleMotion; // gentleMotion true = motion on (the Motion toggle)
            return node;
        }

        // Drives the mark's life with unscaled time. Alpha only; no layout is animated.
        private sealed class SavedMarkTimer : MonoBehaviour
        {
            public CanvasGroup Group;
            public bool Motion;
            private float elapsed;

            private void Update()
            {
                elapsed += Time.unscaledDeltaTime;
                if (!Motion)
                {
                    if (elapsed >= SavedMarkStillHold) Destroy(gameObject);
                    return;
                }
                if (elapsed > SavedMarkHold && Group != null)
                    Group.alpha = Mathf.Clamp01(1f - (elapsed - SavedMarkHold) / SavedMarkFade);
                if (elapsed >= SavedMarkHold + SavedMarkFade) Destroy(gameObject);
            }
        }

        // "Save garden file" sits beside "Copy garden file" and uses the same button helper.
        private void BuildSaveGardenFileButton(Transform files)
        {
            IconButton(files, "Save garden file", GameLocalization.Text("garden_save"),
                MosseloraArt.IconCopyFile, SaveGardenFile, 78, MosseloraTheme.WaterAction,
                false, null, true);
        }

        // Opens the platform share sheet with the same text the copy button uses. When no sheet
        // can open (editor, desktop, failure), it falls back to the clipboard copy and its notice.
        private void SaveGardenFile()
        {
            string stamp = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);
            string text = GardenTransfer.Export(world, stamp);
            gardenRestoreConfirm = false;
            pendingGardenRestore = null;
            if (!NativeShare.Text(text, "Mosselora garden"))
            {
                GUIUtility.systemCopyBuffer = text;
                SetGardenNotice("garden_copied", false);
            }
            Rebuild();
        }
    }
}

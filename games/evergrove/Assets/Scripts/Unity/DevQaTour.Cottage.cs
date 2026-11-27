#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    public sealed partial class DevQaTour
    {
        // I299: the cottage's summary, what's waiting at a glance (and who left lanterns).
        // Named for these shots only, so the title line shows. When the card's lines are more
        // than fit (sideways, 150% text) they scroll inside it (I299 fit); page down so the
        // tour shows every line.
        private IEnumerator CottageShots()
        {
            app.QaGardenName("Willow Bend");
            app.OpenCottage();
            for (int f = 0; f < 6; f++) yield return null;
            yield return new WaitForSecondsRealtime(0.3f);
            yield return new WaitForEndOfFrame();
            SavePng("17j5-cottage.png");
            for (int page = 2; page <= 6 && PageCottage(); page++)
            {
                for (int f = 0; f < 3; f++) yield return null;
                yield return new WaitForEndOfFrame();
                SavePng("17j5-cottage-" + page + ".png");
            }
            app.CloseCottage();
            app.QaGardenName("");
        }

        // Scrolls the cottage card's lines down by most of a view; false when it doesn't
        // scroll or is already at the end.
        private static bool PageCottage()
        {
            var card = GameObject.Find("Cottage paper");
            var scroll = card != null ? card.GetComponent<ScrollRect>() : null;
            if (scroll == null || !scroll.vertical || scroll.content == null || scroll.viewport == null) return false;
            Canvas.ForceUpdateCanvases();
            float distance = scroll.content.rect.height - scroll.viewport.rect.height;
            if (distance <= 0.5f || scroll.verticalNormalizedPosition <= 0.001f) return false;
            float step = scroll.viewport.rect.height * 0.8f / distance;
            scroll.StopMovement();
            scroll.verticalNormalizedPosition = Mathf.Max(0f, scroll.verticalNormalizedPosition - step);
            return true;
        }
    }
}
#endif

using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// I374: the visitors good neighbours bring, drawn from soft shapes like the cat (I335):
    /// 0 a heron standing in the water, now and then dipping to fish; 1 a hedgehog snuffling at
    /// a flower's foot; 2 an owl up in a grove tree, blinking. Still when motion is off.
    /// </summary>
    internal sealed class GardenCreature : MonoBehaviour
    {
        public int Kind;
        private RectTransform moving;
        private RectTransform[] lids;
        private Vector2 home;
        private float clock;

        private void Start()
        {
            clock = Random.Range(0f, 6f);
            if (Kind == 0) Heron();
            else if (Kind == 1) Hedgehog();
            else if (Kind == 2) Owl();
            else Kingfisher();
            if (moving != null) home = moving.anchoredPosition;
        }

        private void Heron()
        {
            Color grey = new Color(0.66f, 0.71f, 0.76f, 1f), dark = new Color(0.36f, 0.40f, 0.46f, 1f);
            Color beak = new Color(0.93f, 0.70f, 0.28f, 1f);
            Part("Ripple", new Vector2(30f, 6f), new Vector2(0f, -18f), 0f, new Color(1f, 1f, 1f, 0.45f), 3);
            Part("Leg", new Vector2(2f, 14f), new Vector2(-3f, -11f), 0f, dark, 1);
            Part("Leg", new Vector2(2f, 14f), new Vector2(3f, -11f), 0f, dark, 1);
            Part("Body", new Vector2(22f, 12f), new Vector2(0f, 0f), 20f, grey, 6);
            Part("Wing", new Vector2(15f, 7f), new Vector2(3f, -1f), 20f, dark, 4);
            moving = Part("Neck and head", new Vector2(4f, 4f), new Vector2(-8f, 6f), 0f, new Color(0f, 0f, 0f, 0f), 1);
            Part("Neck", new Vector2(3f, 14f), new Vector2(0f, 6f), -10f, grey, 1, moving);
            Part("Head", new Vector2(8f, 6f), new Vector2(-1f, 13f), 0f, grey, 3, moving);
            Part("Crest", new Vector2(7f, 1.5f), new Vector2(4f, 15f), 15f, dark, 1, moving);
            Part("Beak", new Vector2(9f, 2f), new Vector2(-8f, 12f), 0f, beak, 1, moving);
        }

        private void Hedgehog()
        {
            Color spines = new Color(0.42f, 0.32f, 0.22f, 1f), tips = new Color(0.62f, 0.50f, 0.36f, 1f);
            Color face = new Color(0.80f, 0.66f, 0.50f, 1f);
            moving = Part("Hedgehog", new Vector2(4f, 4f), Vector2.zero, 0f, new Color(0f, 0f, 0f, 0f), 1);
            Part("Body", new Vector2(24f, 14f), new Vector2(2f, 0f), 0f, spines, 7, moving);
            for (int i = 0; i < 5; i++)
                Part("Spines", new Vector2(4f, 8f), new Vector2(-4f + i * 4f, 6f), -30f + i * 15f, tips, 2, moving);
            Part("Face", new Vector2(11f, 8f), new Vector2(-10f, -2f), 0f, face, 4, moving);
            Part("Nose", new Vector2(3f, 3f), new Vector2(-15f, -2f), 0f, new Color(0.15f, 0.12f, 0.1f, 1f), 1, moving);
            Part("Eye", new Vector2(2f, 2f), new Vector2(-10f, 0f), 0f, new Color(0.15f, 0.12f, 0.1f, 1f), 1, moving);
        }

        // I382: small and bright, perched on a reed over the water; now and then it bobs.
        private void Kingfisher()
        {
            Color blue = new Color(0.13f, 0.56f, 0.80f, 1f), deep = new Color(0.08f, 0.33f, 0.55f, 1f);
            Color orange = new Color(0.93f, 0.52f, 0.20f, 1f);
            Part("Reed", new Vector2(2f, 22f), new Vector2(2f, -10f), 6f, new Color(0.45f, 0.55f, 0.30f, 1f), 1);
            Part("Ripple", new Vector2(18f, 4f), new Vector2(2f, -20f), 0f, new Color(1f, 1f, 1f, 0.4f), 2);
            moving = Part("Kingfisher", new Vector2(4f, 4f), new Vector2(0f, 2f), 0f, new Color(0f, 0f, 0f, 0f), 1);
            Part("Tail", new Vector2(8f, 3.5f), new Vector2(7f, -3f), -25f, deep, 1, moving);
            Part("Back", new Vector2(13f, 8f), new Vector2(2f, 1f), -10f, blue, 4, moving);
            Part("Breast", new Vector2(9f, 6f), new Vector2(-1f, -2f), 0f, orange, 3, moving);
            Part("Head", new Vector2(8f, 7f), new Vector2(-4f, 5f), 0f, blue, 3, moving);
            Part("Cheek", new Vector2(3f, 2.5f), new Vector2(-5f, 4f), 0f, orange, 1, moving);
            Part("Beak", new Vector2(8f, 1.8f), new Vector2(-11f, 5f), 0f, new Color(0.15f, 0.12f, 0.1f, 1f), 1, moving);
        }

        private void Owl()
        {
            Color brown = new Color(0.52f, 0.40f, 0.27f, 1f), disc = new Color(0.86f, 0.78f, 0.62f, 1f);
            Color eye = new Color(0.98f, 0.86f, 0.40f, 1f), pupil = new Color(0.12f, 0.10f, 0.08f, 1f);
            Part("Ear", new Vector2(5f, 7f), new Vector2(-7f, 13f), 20f, brown, 2);
            Part("Ear", new Vector2(5f, 7f), new Vector2(7f, 13f), -20f, brown, 2);
            Part("Body", new Vector2(22f, 26f), new Vector2(0f, 0f), 0f, brown, 10);
            Part("Breast", new Vector2(13f, 12f), new Vector2(0f, -5f), 0f, new Color(0.70f, 0.58f, 0.42f, 1f), 6);
            Part("Face", new Vector2(19f, 12f), new Vector2(0f, 6f), 0f, disc, 6);
            Part("Eye", new Vector2(7f, 7f), new Vector2(-4.5f, 6f), 0f, eye, 3);
            Part("Eye", new Vector2(7f, 7f), new Vector2(4.5f, 6f), 0f, eye, 3);
            Part("Pupil", new Vector2(3f, 3f), new Vector2(-4.5f, 6f), 0f, pupil, 1);
            Part("Pupil", new Vector2(3f, 3f), new Vector2(4.5f, 6f), 0f, pupil, 1);
            Part("Beak", new Vector2(2.5f, 4f), new Vector2(0f, 2f), 0f, new Color(0.80f, 0.62f, 0.30f, 1f), 1);
            lids = new[]
            {
                Part("Lid", new Vector2(7.5f, 7.5f), new Vector2(-4.5f, 6f), 0f, brown, 3),
                Part("Lid", new Vector2(7.5f, 7.5f), new Vector2(4.5f, 6f), 0f, brown, 3),
            };
            foreach (RectTransform lid in lids) lid.localScale = new Vector3(1f, 0f, 1f);
        }

        private RectTransform Part(string name, Vector2 size, Vector2 at, float angle, Color colour, int radius,
            Transform parent = null)
        {
            var node = new GameObject(name, typeof(RectTransform));
            node.transform.SetParent(parent != null ? parent : transform, false);
            var rect = (RectTransform)node.transform;
            rect.sizeDelta = size;
            rect.anchoredPosition = at;
            rect.localRotation = Quaternion.Euler(0f, 0f, angle);
            var image = node.AddComponent<Image>();
            image.sprite = MosseloraArt.Rounded(radius);
            image.color = colour;
            image.raycastTarget = false;
            return rect;
        }

        private void Update()
        {
            if (!GardenMapView.FirefliesMove) return;
            clock += Time.unscaledDeltaTime;
            if (Kind == 0 && moving != null)
            {
                // Still for a long while, then a quick dip to the water and back.
                float t = clock % 7f;
                float dip = t > 6f ? Mathf.Sin((t - 6f) * Mathf.PI) : 0f;
                moving.anchoredPosition = home + new Vector2(-3f * dip, -9f * dip);
                moving.localRotation = Quaternion.Euler(0f, 0f, 35f * dip);
            }
            else if (Kind == 3 && moving != null)
            {
                // A quick bob every few seconds, then still.
                float t = clock % 3.2f;
                moving.anchoredPosition = home + new Vector2(0f, t < 0.3f ? -2.5f * Mathf.Sin(t / 0.3f * Mathf.PI) : 0f);
            }
            else if (Kind == 1 && moving != null)
            {
                // Snuffling: a slow shuffle back and forth, a small nod.
                moving.anchoredPosition = home + new Vector2(4f * Mathf.Sin(clock * 0.5f), 0.8f * Mathf.Abs(Mathf.Sin(clock * 3f)));
            }
            else if (lids != null)
            {
                // A slow blink every few seconds.
                float t = clock % 4.5f;
                float shut = t > 4.3f ? Mathf.Sin((t - 4.3f) / 0.2f * Mathf.PI) : 0f;
                foreach (RectTransform lid in lids) lid.localScale = new Vector3(1f, shut, 1f);
            }
        }
    }
}

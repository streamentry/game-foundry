using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Keeps every word of a short label whole (I196): uGUI breaks a word that is
    /// wider than its box mid-word ("Weathervan / e"). When the longest word does
    /// not fit, the font steps down, never below <see cref="minimum"/>.
    /// </summary>
    [RequireComponent(typeof(Text))]
    public sealed class WholeWordFit : MonoBehaviour
    {
        private Text target;
        private int largest;
        private int minimum;
        private int appliedWidth = -1;

        public static void Attach(Text text, int minimumSize)
        {
            if (text == null) return;
            WholeWordFit fit = text.GetComponent<WholeWordFit>() ?? text.gameObject.AddComponent<WholeWordFit>();
            fit.target = text;
            fit.largest = text.fontSize;
            fit.minimum = Mathf.Min(minimumSize, text.fontSize);
            fit.appliedWidth = -1;
            fit.Refit();
        }

        private void OnRectTransformDimensionsChange()
        {
            Refit();
        }

        private void Refit()
        {
            if (target == null) return;
            int width = Mathf.RoundToInt(target.rectTransform.rect.width);
            if (width <= 0 || width == appliedWidth) return;
            appliedWidth = width;
            int size = largest;
            while (size > minimum && LongestWord(size) > width) size--;
            if (target.fontSize != size) target.fontSize = size;
        }

        private float LongestWord(int size)
        {
            TextGenerationSettings settings = target.GetGenerationSettings(Vector2.zero);
            settings.horizontalOverflow = HorizontalWrapMode.Overflow;
            settings.fontSize = size;
            var generator = new TextGenerator();
            float longest = 0f;
            foreach (string word in (target.text ?? "").Split(' ', '\n'))
                if (word.Length > 0)
                    longest = Mathf.Max(longest, generator.GetPreferredWidth(word, settings) / target.pixelsPerUnit);
            return longest;
        }
    }
}

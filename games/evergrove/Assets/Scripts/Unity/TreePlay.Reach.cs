using System.Collections;
using Evergrove.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Vision run 20261128-1502, idea 5: the water board says how far the water reaches, for a screen reader
    /// ("Water reaches 5 of 9 pipes."). One hidden caption under the card, reused on every paint. English only.
    /// </summary>
    internal sealed partial class TreePlay
    {
        private void SpeakReach(PipeInsight insight)
        {
            if (GameLocalization.Current != "en" || cardRect == null || uiFont == null) return;
            Transform caption = cardRect.Find("Water reach");
            if (caption == null)
            {
                Hidden(cardRect, "", uiFont);
                caption = cardRect.GetChild(cardRect.childCount - 1);
                caption.name = "Water reach";
            }
            Text text = caption.GetComponent<Text>();
            if (text != null) text.text = WaterReachWords.For(insight, puzzle.CellCount);
        }
    }
}

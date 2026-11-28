using Evergrove.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Vision run 20261128-0927, idea 5: when turns pass, the take-turns token also says whose turn it is
    /// to a screen reader ("Second player's turn, of three."). One hidden caption per token, reused on every
    /// turn, never a new node per turn. Nothing visible changes.
    /// </summary>
    internal sealed partial class TreePlay
    {
        private void SpeakTurn(Transform token)
        {
            string words = TakeTurns.Spoken(turn, players);
            Transform caption = token.Find("Turn caption");
            if (caption == null)
            {
                if (uiFont == null) return; // the card isn't built yet; the next PaintTurn makes it
                Hidden(token, words, uiFont);
                caption = token.GetChild(token.childCount - 1);
                caption.name = "Turn caption";
            }
            Text text = caption.GetComponent<Text>();
            if (text != null) text.text = words;
        }
    }
}

#if UNITY_EDITOR || DEVELOPMENT_BUILD
using Evergrove.Core;
using UnityEngine;

namespace Evergrove.UnityClient
{
    public sealed partial class EvergroveApp
    {
        // Dev QA seam for the hand tour (DevQaTour.Hand.cs). Sets the hand and whether it was asked, on this device;
        // the tour rebuilds the hub afterwards so the new mirror shows.
        internal Hand QaHand { get { return hand; } }
        internal bool QaHandAsked { get { return HandAsked; } }

        internal void QaSetHand(Hand value, bool asked)
        {
            SetHand(value);
            PlayerPrefs.SetInt(Handedness.AskedKey, asked ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
#endif

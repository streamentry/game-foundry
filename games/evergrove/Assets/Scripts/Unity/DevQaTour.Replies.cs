#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using Evergrove.Core;
using UnityEngine;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Checklist 81, tour shots: a neighbour's letter with the three reply chips, a chip chosen, and the
    /// neighbour's echo the next day. Part of the dev QA tour (DevQaTour.cs); the same guard applies.
    /// </summary>
    public sealed partial class DevQaTour
    {
        private IEnumerator ReplyShots()
        {
            // The tour's own living garden; the hook keeps a note from Mira if the newest letter is not a neighbour's.
            app.QaSetDay(0);
            app.QaSetClock(13);
            app.QaShowHub(true);
            for (int f = 0; f < 6; f++) yield return null;
            app.QaOpenNeighbourLetter();
            yield return new WaitForSecondsRealtime(1.5f); // the seal lifts, then the paper rises
            yield return new WaitForEndOfFrame();
            SavePng("21-reply-chips.png");
            app.QaChooseReply(1); // "Tell me more"
            for (int f = 0; f < 2; f++) yield return null;
            yield return new WaitForEndOfFrame();
            app.QaCloseLetter();
            app.QaSetDay(1); // the next day: Mira answers the reply
            app.QaShowHub(true);
            app.QaTalk(0);
            for (int f = 0; f < 6; f++) yield return null;
            yield return new WaitForEndOfFrame();
            SavePng("21-reply-echo.png");
            app.QaTalk(-1);
            app.QaShowHub(false);
            app.QaSetDay(0);
        }
    }
}
#endif

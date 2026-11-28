#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using Evergrove.Core;
using UnityEngine;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Checklist 38, tour shots: a stream path thing (story chapter 4) and a plots thing (chapter 6)
    /// of the tour's garden are solved on their own cards; the cause line is shot 0.6 s and 1.5 s after
    /// the solve and timed (MOSSELORA_CHAIN). Then the hub shows the day's watered mark. Part of the dev
    /// QA tour (DevQaTour.cs); the same guard applies.
    /// </summary>
    public sealed partial class DevQaTour
    {
        private IEnumerator ChainShots()
        {
            yield return ChainBoard(4, PuzzleKind.StreamPath, "27-chain-stream");
            yield return ChainBoard(6, PuzzleKind.Plots, "27-chain-plots");
            app.QaShowHub(true);
            for (int f = 0; f < 10; f++) yield return null;
            yield return new WaitForSecondsRealtime(0.5f);
            yield return new WaitForEndOfFrame();
            SavePng("27-chain-watered.png");
            app.QaShowHub(false);
        }

        private IEnumerator ChainBoard(int item, PuzzleKind kind, string shot)
        {
            app.QaOpenTreePlay(item);
            for (int f = 0; f < 8; f++) yield return null;
            yield return new WaitForSecondsRealtime(0.4f);
            yield return new WaitForEndOfFrame();
            app.QaSolveTreePlay();
            yield return new WaitForSecondsRealtime(0.6f);
            yield return new WaitForEndOfFrame();
            SavePng(shot + "-0.6s.png");
            yield return new WaitForSecondsRealtime(0.9f);
            yield return new WaitForEndOfFrame();
            SavePng(shot + "-1.5s.png");
            // The line's first visible frame, measured from the solve; a plots thing with no nursery shows none.
            float limit = Time.unscaledTime + 2f;
            while (ChainCause.ShownAt < ChainCause.SolvedAt && Time.unscaledTime < limit) yield return null;
            if (ChainCause.ShownAt >= ChainCause.SolvedAt)
                Debug.Log("MOSSELORA_CHAIN " + kind + " shown " + (ChainCause.ShownAt - ChainCause.SolvedAt).ToString("0.00"));
            else
                Debug.Log("MOSSELORA_CHAIN " + kind + " none");
            app.QaEndTreePlay();
            for (int f = 0; f < 4; f++) yield return null;
        }
    }
}
#endif

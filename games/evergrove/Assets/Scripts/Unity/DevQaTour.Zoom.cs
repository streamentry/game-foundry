#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using Evergrove.Core;
using UnityEngine;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Checklist 91, the zoom: the densest boards open whole (cells resting, the picture takes the tap),
    /// then zoom in on the centre, where every cell is at least 48 pt. The touch scan runs at each shot
    /// (DevQaTour.Touch.cs), so both states are measured. Part of the dev QA tour; the same guard applies.
    /// </summary>
    public sealed partial class DevQaTour
    {
        private IEnumerator ZoomShots()
        {
            foreach (PuzzleKind kind in new[] { PuzzleKind.Doorstep, PuzzleKind.Sudoku, PuzzleKind.Nonogram })
            {
                string name = kind.ToString().ToLowerInvariant();
                app.QaOpenKind(kind, 2);
                for (int f = 0; f < 8; f++) yield return null;
                yield return new WaitForSecondsRealtime(0.4f);
                yield return new WaitForEndOfFrame();
                Debug.Log("MOSSELORA_ZOOM " + name + " " + app.QaZoomState());
                SavePng("28-zoom-" + name + "-whole.png");
                app.QaZoomTreePlay(true);
                for (int f = 0; f < 4; f++) yield return null;
                yield return new WaitForEndOfFrame();
                Debug.Log("MOSSELORA_ZOOM " + name + " " + app.QaZoomState());
                SavePng("28-zoom-" + name + "-in.png");
                app.QaZoomTreePlay(false);
                for (int f = 0; f < 2; f++) yield return null;
                app.QaEndTreePlay();
                for (int f = 0; f < 4; f++) yield return null;
            }
        }
    }
}
#endif

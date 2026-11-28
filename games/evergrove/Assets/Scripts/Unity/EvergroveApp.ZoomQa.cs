#if UNITY_EDITOR || DEVELOPMENT_BUILD
namespace Evergrove.UnityClient
{
    /// <summary>Checklist 91 tour hooks: zoom the open board in or out, and read its zoom.</summary>
    public sealed partial class EvergroveApp
    {
        internal void QaZoomTreePlay(bool inward) { if (qaTreePlay != null) qaTreePlay.QaZoom(inward); }

        internal string QaZoomState()
        {
            if (qaTreePlay == null) return "no card";
            return qaTreePlay.Zoomable ? "zoomable scale " + qaTreePlay.ZoomScale.ToString("0.00") + (qaTreePlay.Zoomed ? " in" : " whole")
                : "not needed";
        }
    }
}
#endif

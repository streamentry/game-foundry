#if UNITY_EDITOR || DEVELOPMENT_BUILD
namespace Evergrove.UnityClient
{
    public sealed partial class EvergroveApp
    {
        // Checklist 24 tour: redraw the hub's map without closing it, so the neighbours are placed
        // again at the current clock and any moved spot walks (EvergroveApp.NeighbourWalk.cs).
        // RefreshMaps rebuilds the hub when it is open; Rebuild would only redraw the page beneath.
        internal void QaRebuildHub()
        {
            qaShowHub = true; // the hub is built only when the tour asks for it (BuildTitleScreen)
            RefreshMaps();
            qaShowHub = false;
        }
    }
}
#endif

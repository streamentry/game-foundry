namespace Evergrove.UnityClient
{
    /// <summary>Checklist 94 (founder, 2026-11-28): a downward swipe on the puzzle card closes it, as a sheet does.</summary>
    internal sealed partial class TreePlay
    {
        // Checklist 94: the puzzle card closes on a downward swipe, with the same action as its close token.
        private void AttachSwipeClose()
        {
            SwipeToClose.Attach(cardRect, Close, gentle);
        }
    }
}

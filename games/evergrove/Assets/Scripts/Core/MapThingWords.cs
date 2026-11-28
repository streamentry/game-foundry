namespace Evergrove.Core
{
    /// <summary>
    /// What a grown thing on the garden map says to a screen reader: its own caption when it has
    /// one, else its family label ("On the map" alone says nothing about what it is). Pure rules;
    /// Unity puts the words on a hidden child named "Caption".
    /// </summary>
    public static class MapThingWords
    {
        /// <summary>
        /// The trimmed caption if it has words, else the trimmed family label if it has words,
        /// else "". Never throws; the inputs are not changed.
        /// </summary>
        public static string Spoken(string caption, string familyLabel)
        {
            if (!string.IsNullOrWhiteSpace(caption)) return caption.Trim();
            if (!string.IsNullOrWhiteSpace(familyLabel)) return familyLabel.Trim();
            return "";
        }
    }
}

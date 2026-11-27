using System.Collections.Generic;
using Evergrove.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    public sealed partial class EvergroveApp
    {
        /// <summary>
        /// The cottage table (I228, I328): the fruit in the basket and the jars in the pantry, drawn as one small
        /// painted object each, up to the display cap in <see cref="FamilyTable"/>. Hold any object to hear its
        /// caption, which states the true count. Senders are not recorded, so no name is given.
        /// </summary>
        private void CottageTableRow(Transform parent)
        {
            foreach (FamilyTableItem item in FamilyTable.Items(world))
            {
                Transform row = CenteredRowIn(parent, "Table " + item.Kind);
                var layout = row.GetComponent<HorizontalLayoutGroup>();
                layout.spacing = 4;
                Sprite art = MosseloraArt.Load(item.Kind == FamilyTableKind.Jam ? "food-jam" : "food-pear");
                for (int i = 0; i < item.Shown; i++)
                {
                    GameObject piece = Node("Table object", row);
                    var size = piece.AddComponent<LayoutElement>();
                    size.preferredWidth = size.preferredHeight = size.minWidth = size.minHeight = 30f;
                    var image = piece.AddComponent<Image>();
                    image.sprite = art;
                    image.preserveAspect = true;
                    LongPressCaption.Attach(piece, item.Words, font);
                }
            }
        }
    }
}

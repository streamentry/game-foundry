using System;
using Evergrove.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// I215: family parcels, over your garden. Two round tokens switch between
    /// sending (your name, one gift, a few words, then the share sheet) and
    /// opening (paste what a relative sent: their name, their letter, the gift).
    /// The rules live in Core (FamilyParcel, WorldModel.SendParcel/Open); this
    /// only gathers choices, shows results in plain words and reports back.
    /// English only for now, like the other new screens.
    /// </summary>
    internal sealed class FamilySheet : MonoBehaviour
    {
        internal sealed class Hooks
        {
            public Func<int> Seeds;
            public Func<int> Fruit;                // fruit picked from your trees (I230)
            public Func<int, bool> RowOpen;
            public Func<int[]> OwnedKinds;          // nursery kinds you have bought, newest first
            public Func<string, string, ParcelGift, int, string, int, ParcelSendResult> Send; // I308: the last int is a blessing
            public Action<int, int, int, string> PlayBoard;  // I309: kind, tier, seed, from whom
            public Func<string, ParcelOpenResult> Open;
            public Action<string> Share;           // native share sheet, else the clipboard
            public Func<string> Paste;              // the clipboard
            public Action Postcard;
            public Func<string, bool> Visit;        // a relative's garden file, read-only
            public Action Closed;
            public Func<string> Name;
            public Action<string> SaveName;
        }

        private static readonly int[] SeedChoices = { 10, 50, 100 };
        private const int LetterOnly = -1, CuttingBase = 1000, FruitBase = 2000; // pick codes: seed amount, 1000+kind, 2000+fruit, or -1
        private const int Lantern = -2;          // I310: leave a lantern
        public const string LanternArt = "build-96"; // the catalogue's own paper lanterns
        private int blessing;                    // I308: 0, or 1..6 into FamilyParcel.Blessings
        private ParcelOpenResult boardWaiting;   // I309: an opened board, ready to play
        private const int FruitPerBasket = 5;

        private Hooks hooks;
        private Font font;
        private bool sending = true;
        private int pick = 10;
        private string letter = "";
        private string outcome = "";
        private Transform body;
        private RectTransform card;
        private float width;

        public static FamilySheet Open(RectTransform host, Font font, Hooks hooks)
        {
            var root = new GameObject("Family", typeof(RectTransform));
            root.transform.SetParent(host, false);
            var rect = (RectTransform)root.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            root.AddComponent<Image>().color = new Color(0.08f, 0.14f, 0.11f, 0.38f);
            var sheet = root.AddComponent<FamilySheet>();
            sheet.hooks = hooks;
            sheet.font = font;
            sheet.Build(rect);
            return sheet;
        }

        private void Build(RectTransform root)
        {
            float screenWidth = root.rect.width > 1f ? root.rect.width : 390f;
            float screenHeight = root.rect.height > 1f ? root.rect.height : 844f;
            // Sideways and short (I258): sending lays out in two columns instead of shrinking
            // the words below a comfortable size.
            wide = sending && screenWidth > screenHeight * 1.2f && screenHeight < 520f;
            width = wide ? Mathf.Min(screenWidth - 40f, 760f) : Mathf.Min(screenWidth - 28f, 440f);
            var node = new GameObject("Family card", typeof(RectTransform));
            node.transform.SetParent(root, false);
            card = (RectTransform)node.transform;
            card.anchorMin = card.anchorMax = new Vector2(0.5f, 0.5f);
            card.sizeDelta = new Vector2(width, 0f);
            var image = node.AddComponent<Image>();
            MosseloraArt.Round(image, MosseloraTheme.CardFill, 26);
            MosseloraArt.Lift(image, 4f, 0.22f);
            var column = node.AddComponent<VerticalLayoutGroup>();
            column.padding = new RectOffset(18, 18, 16, 18);
            column.spacing = 12;
            column.childAlignment = TextAnchor.UpperCenter;
            column.childControlWidth = column.childControlHeight = true;
            column.childForceExpandWidth = true;
            column.childForceExpandHeight = false;
            node.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Top: close, send / open, the postcard, and the seeds you have.
            Transform top = Row(node.transform, "Family top", 60f, 8f);
            TreePlay.Token(top, "Close family", MosseloraArt.Load(MosseloraArt.IconCancel), 52f,
                MosseloraTheme.QuietAction, Close, true);
            TreePlay.Token(top, "Send a parcel", MosseloraArt.Load(MosseloraArt.IconCopyFile), 56f,
                sending ? MosseloraTheme.ForestAction : MosseloraTheme.QuietAction, () => Switch(true), true);
            TreePlay.Token(top, "Open a parcel", MosseloraArt.Load(MosseloraArt.IconRestoreFile), 56f,
                sending ? MosseloraTheme.QuietAction : MosseloraTheme.ForestAction, () => Switch(false), true);
            TreePlay.Token(top, "Send a garden postcard", MosseloraArt.Load(MosseloraArt.IconChat), 52f,
                MosseloraTheme.QuietAction, () => hooks.Postcard?.Invoke(), true);
            TreePlay.Token(top, "Visit their garden", MosseloraArt.Load(MosseloraArt.IconHome), 52f,
                MosseloraTheme.QuietAction, DoVisit, true);

            var bodyNode = new GameObject("Family body", typeof(RectTransform));
            bodyNode.transform.SetParent(node.transform, false);
            rightColumn = null;
            if (wide)
            {
                var pair = bodyNode.AddComponent<HorizontalLayoutGroup>();
                pair.spacing = 18;
                pair.childAlignment = TextAnchor.UpperCenter;
                pair.childControlWidth = pair.childControlHeight = true;
                pair.childForceExpandWidth = true;
                pair.childForceExpandHeight = false;
                body = Column(bodyNode.transform, "Gifts side");
                rightColumn = Column(bodyNode.transform, "Letter side");
            }
            else
            {
                var bodyColumn = bodyNode.AddComponent<VerticalLayoutGroup>();
                bodyColumn.spacing = 12;
                bodyColumn.childAlignment = TextAnchor.UpperCenter;
                bodyColumn.childControlWidth = bodyColumn.childControlHeight = true;
                bodyColumn.childForceExpandWidth = true;
                bodyColumn.childForceExpandHeight = false;
                body = bodyNode.transform;
            }
            if (sending) BuildSend(); else BuildOpen();
        }

        private bool wide;
        private Transform rightColumn;

        private static Transform Column(Transform parent, string name)
        {
            var node = new GameObject(name, typeof(RectTransform));
            node.transform.SetParent(parent, false);
            node.AddComponent<LayoutElement>().flexibleWidth = 1f;
            var column = node.AddComponent<VerticalLayoutGroup>();
            column.spacing = 12;
            column.childAlignment = TextAnchor.UpperCenter;
            column.childControlWidth = column.childControlHeight = true;
            column.childForceExpandWidth = true;
            column.childForceExpandHeight = false;
            return node.transform;
        }

        // Sideways the card can be taller than the screen: once laid out, it shrinks
        // to fit (never below 75%), so the send token is always on screen (I215).
        private void LateUpdate()
        {
            if (card == null) return;
            var root = (RectTransform)transform;
            float room = root.rect.height - 24f, tall = card.rect.height;
            float scale = tall > room && tall > 1f ? Mathf.Max(0.75f, room / tall) : 1f;
            if (Mathf.Abs(card.localScale.x - scale) > 0.001f) card.localScale = new Vector3(scale, scale, 1f);
        }

        private void Switch(bool send)
        {
            sending = send;
            outcome = "";
            Rebuild();
        }

        private void Rebuild()
        {
            foreach (Transform child in transform) Destroy(child.gameObject);
            Build((RectTransform)transform);
        }

        // ---- Sending
        private void BuildSend()
        {
            InputField name = Field(body, "Your name", hooks.Name() ?? "", 48f, false, FamilyParcel.MaxNameLength);
            Transform pickRow = Row(body, "Pick row", 44f, 10f);
            Words(pickRow, "Pick one gift", 15, MosseloraTheme.EarthInk);
            TreePlay.Pill(pickRow, hooks.Seeds().ToString(), font, out _);
            Transform gifts = Row(body, "Gifts", 82f, 10f);
            foreach (int amount in SeedChoices)
                GiftToken(gifts, amount, MosseloraArt.Load(TreePlay.SeedIcon), amount.ToString(),
                    hooks.Seeds() >= amount);
            GiftToken(gifts, LetterOnly, MosseloraArt.Load(MosseloraArt.IconChat), "", true);
            // A basket of fruit from your trees (I230), then a cutting of a plant you
            // have grown from the nursery (it costs its price).
            int[] owned = hooks.OwnedKinds();
            int fruit = hooks.Fruit != null ? hooks.Fruit() : 0;
            {
                Transform cuttings = Row(body, "Lantern, fruit and cuttings", 82f, 10f);
                GiftToken(cuttings, Lantern, MosseloraArt.Load(LanternArt), "", true); // I310
                if (fruit > 0)
                {
                    int basket = Math.Min(FruitPerBasket, fruit);
                    GiftToken(cuttings, FruitBase + basket, MosseloraArt.Load("food-basket"), basket.ToString(), true);
                }
                for (int i = 0; i < owned.Length && i < (fruit > 0 ? 2 : 3); i++)
                    GiftToken(cuttings, CuttingBase + owned[i], MosseloraArt.Load(Nursery.Item(owned[i]).Art),
                        Nursery.Price(owned[i]).ToString(), hooks.Seeds() >= Nursery.Price(owned[i]));
            }
            // I308 (idea 41, after red envelopes): seeds or fruit may go in a festive envelope with
            // a wish, picked from a few, never typed. One token turns through the wishes.
            bool wrappable = pick != LetterOnly && pick != Lantern && (pick < CuttingBase || pick >= FruitBase);
            if (!wrappable) blessing = 0;
            else
            {
                Transform wrap = Row(body, "Festive envelope", 60f, 10f);
                TreePlay.Token(wrap, blessing == 0 ? "Wrap it with a wish" : "Wish: " + FamilyParcel.Blessings[blessing - 1],
                    MosseloraArt.Load("letter-sealed"), 54f, blessing == 0 ? MosseloraTheme.QuietAction : Festive,
                    () => { blessing = (blessing + 1) % (FamilyParcel.MaxBlessing + 1); Rebuild(); }, true);
                Words(wrap, blessing == 0 ? "No envelope" : "“" + FamilyParcel.Blessings[blessing - 1] + "”", 16,
                    MosseloraTheme.EarthInk);
            }
            if (rightColumn != null) body = rightColumn; // sideways: the letter has its own column
            InputField words = Field(body, "A few words for them (optional)", letter, wide ? 150f : 104f, true,
                FamilyParcel.MaxLetterLength);
            words.onValueChanged.AddListener(v => letter = v ?? "");
            Transform go = Row(body, "Send row", 76f, 12f);
            TreePlay.Token(go, "Send it", MosseloraArt.Load(MosseloraArt.IconContinue), 72f,
                MosseloraTheme.WaterAction, () => DoSend(name.text), true);
            if (outcome.Length > 0) Words(body, outcome, 16, MosseloraTheme.EarthInk);
        }

        private void GiftToken(Transform row, int code, Sprite art, string price, bool can)
        {
            bool chosen = pick == code;
            TreePlay.Token(row, code == LetterOnly ? "Just a letter" : code == Lantern ? "Leave a lantern" : code >= FruitBase ? (code - FruitBase) + " fruit"
                    : code >= CuttingBase ? "A cutting of " + Nursery.Item(code - CuttingBase).Name : code + " seeds",
                art, 66f, chosen ? MosseloraTheme.ForestAction : MosseloraTheme.QuietAction,
                () => { if (can) { pick = code; Rebuild(); } }, true);
            Transform token = row.GetChild(row.childCount - 1);
            Transform icon = token.Find("Icon");
            if (icon != null)
            {
                ((RectTransform)icon).sizeDelta = new Vector2(46f, 46f);
                icon.GetComponent<Image>().color = can ? Color.white : new Color(1f, 1f, 1f, 0.35f);
            }
            if (price.Length > 0)
            {
                Image chip = MosseloraArt.Ornament(token, "Price", MosseloraArt.Rounded(10), new Vector2(0.5f, 0f),
                    new Vector2(0.5f, 0.5f), new Vector2(44f, 20f), new Vector2(0f, 2f));
                if (chip != null)
                {
                    chip.preserveAspect = false;
                    chip.color = can ? new Color(0.98f, 0.93f, 0.74f, 1f) : new Color(0.86f, 0.85f, 0.82f, 0.9f);
                    Text value = Words(chip.transform, price, 13, MosseloraTheme.EarthInk);
                    value.GetComponent<LayoutElement>().ignoreLayout = true;
                    var valueRect = value.rectTransform;
                    valueRect.anchorMin = Vector2.zero;
                    valueRect.anchorMax = Vector2.one;
                    valueRect.offsetMin = valueRect.offsetMax = Vector2.zero;
                }
            }
        }

        private void DoSend(string name)
        {
            string from = (name ?? "").Trim();
            if (!FamilyParcel.ValidName(from)) { outcome = "Write your name first, so they know who it's from."; Rebuild(); return; }
            hooks.SaveName(from);
            ParcelGift gift = pick == LetterOnly ? ParcelGift.None : pick == Lantern ? ParcelGift.Lantern
                : pick >= FruitBase ? ParcelGift.Fruit : pick >= CuttingBase ? ParcelGift.Cutting : ParcelGift.Seeds;
            int value = pick == Lantern ? 0 : pick >= FruitBase ? pick - FruitBase : pick >= CuttingBase ? pick - CuttingBase : pick;
            int wish = gift == ParcelGift.Seeds || gift == ParcelGift.Fruit ? blessing : 0;
            string nonce = Guid.NewGuid().ToString("N").Substring(0, 16);
            ParcelSendResult sent = hooks.Send(from, letter.Trim(), gift, value, nonce, wish);
            if (!sent.Ok) { outcome = Explain(sent.Error); Rebuild(); return; }
            string what = gift == ParcelGift.Seeds ? value + " seeds"
                : gift == ParcelGift.Fruit ? "a basket of " + value + " fruit from their trees"
                : gift == ParcelGift.Cutting ? "a cutting of " + Nursery.Item(value).Name
                : gift == ParcelGift.Lantern ? "a lantern for their garden" : "a letter";
            if (wish > 0) what += " in a festive envelope";
            blessing = 0;
            hooks.Share(FamilyParcel.Wrap(sent.Text, from + " sent you " + what + " in Mosselora. Open it in the game: Family, then Open."));
            letter = "";
            outcome = "Sent. Share it with them however you like to talk.";
            Rebuild();
        }

        // ---- Opening
        private void BuildOpen()
        {
            Words(body, "Copy the message they sent you, then tap here.", 16, MosseloraTheme.EarthInk);
            Transform go = Row(body, "Open row", 92f, 12f);
            TreePlay.Token(go, "Paste and open", MosseloraArt.Load(MosseloraArt.IconRestoreFile), 84f,
                MosseloraTheme.WaterAction, DoOpen, true);
            if (outcome.Length > 0) Words(body, outcome, 17, MosseloraTheme.EarthInk);
            if (boardWaiting != null && hooks.PlayBoard != null)
            {
                ParcelOpenResult board = boardWaiting;
                Transform play = Row(body, "Play their board", 84f, 12f);
                TreePlay.Token(play, "Play the board they sent", MosseloraArt.Load(MosseloraArt.IconContinue), 76f,
                    MosseloraTheme.ForestAction, () => {
                        boardWaiting = null;
                        hooks.PlayBoard(board.BoardKind, board.BoardTier, board.BoardSeed, board.Contents.From);
                    }, true);
            }
        }

        private static readonly Color Festive = new Color(0.78f, 0.24f, 0.20f, 1f); // envelope red

        private void DoOpen()
        {
            string raw = FamilyParcel.Unwrap(hooks.Paste());
            if (raw == null) { outcome = "There's no parcel in what you copied. Copy their whole message and try again."; Rebuild(); return; }
            ParcelOpenResult opened = hooks.Open(raw);
            if (!opened.Ok) { outcome = Explain(opened.Error); Rebuild(); return; }
            ParcelContents c = opened.Contents;
            string gift = c.Gift == ParcelGift.Seeds ? opened.SeedsGained + " seeds are in your purse."
                : c.Gift == ParcelGift.Cutting ? "A cutting of " + Nursery.Item(c.Kind).Name + " is waiting in your basket."
                : c.Gift == ParcelGift.Fruit ? (opened.FruitGained > 0 ? opened.FruitGained + " fruit, picked from their trees, are in your basket."
                    : "Your fruit basket is already full.")
                : c.Gift == ParcelGift.Help ? c.From + " solved a puzzle in your garden for you. Look for the bloom."
                : c.Gift == ParcelGift.Lantern ? c.From + " left a lantern by your cottage. It's lit in the evenings."
                : c.Gift == ParcelGift.Board ? "The very board " + c.From + " solved. Play it when you like; it pays nothing."
                : "";
            if (c.Blessing > 0) gift = "A festive envelope: “" + FamilyParcel.Blessings[c.Blessing - 1] + "”.\n" + gift;
            boardWaiting = c.Gift == ParcelGift.Board && opened.BoardKind >= 0 ? opened : null;
            outcome = "From " + c.From + "." + (c.Letter.Length > 0 ? "\n“" + c.Letter + "”" : "") +
                (gift.Length > 0 ? "\n" + gift : "");
            Rebuild();
        }

        // Visit: their garden file (Settings → Copy garden file on their phone).
        private void DoVisit()
        {
            string text = hooks.Paste();
            if (string.IsNullOrWhiteSpace(text) || hooks.Visit == null || !hooks.Visit(text.Trim()))
            {
                sending = false;
                outcome = "To visit, ask them for their garden file (Menu, Settings, Copy garden file), copy it, then tap the house.";
                Rebuild();
            }
        }

        /// <summary>Begin a letter (I235, "write back"): the send view with these words started.</summary>
        internal void Prefill(string words)
        {
            sending = true;
            outcome = "";
            pick = LetterOnly;
            letter = words ?? "";
            Rebuild();
        }

        // Tour only: send with this name, or open this text, by the same paths as taps.
        internal void QaSend(string name, int code, string words) { pick = code; letter = words; DoSend(name); }
        internal void QaOpen() { sending = false; DoOpen(); }
        internal void QaVisit() { DoVisit(); }

        private static string Explain(ParcelError error)
        {
            switch (error)
            {
                case ParcelError.AlreadyOpened: return "You've already opened this parcel.";
                case ParcelError.AlreadyUsed: return "This parcel came from your own garden.";
                case ParcelError.NotEnoughSeeds: return "You don't have enough seeds for that gift yet.";
                case ParcelError.BasketFull: return "Your basket is full. Plant a gift first, then open this one.";
                case ParcelError.CuttingNotAvailable: return "That plant isn't open in your nursery yet.";
                case ParcelError.NotEnoughFruit: return "You don't have that much fruit yet. Tend your trees to pick more.";
                case ParcelError.NotInThisGarden: return "This help is for something your garden hasn't grown yet.";
                case ParcelError.ChecksumMismatch:
                case ParcelError.Malformed:
                case ParcelError.Invalid: return "This parcel was changed on the way and can't be opened. Ask them to send it again.";
                case ParcelError.UnsupportedFutureVersion: return "This parcel needs a newer Mosselora. Update the game, then try again.";
                default: return "That didn't work. Copy their whole message and try again.";
            }
        }

        private void Close()
        {
            Action closed = hooks.Closed;
            Destroy(gameObject);
            closed?.Invoke();
        }

        // ---- builders
        private static Transform Row(Transform parent, string name, float height, float spacing)
        {
            var row = new GameObject(name, typeof(RectTransform));
            row.transform.SetParent(parent, false);
            row.AddComponent<LayoutElement>().preferredHeight = height;
            var group = row.AddComponent<HorizontalLayoutGroup>();
            group.spacing = spacing;
            group.childAlignment = TextAnchor.MiddleCenter;
            group.childControlWidth = group.childControlHeight = true;
            group.childForceExpandWidth = group.childForceExpandHeight = false;
            return row.transform;
        }

        private Text Words(Transform parent, string words, int size, Color color)
        {
            var node = new GameObject("Words", typeof(RectTransform));
            node.transform.SetParent(parent, false);
            node.AddComponent<LayoutElement>();
            var text = node.AddComponent<Text>();
            text.font = font;
            text.fontSize = Mathf.RoundToInt(size * TextScale.Percent / 100f);
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            text.text = words;
            return text;
        }

        private InputField Field(Transform parent, string hint, string value, float height, bool multi, int limit)
        {
            var node = new GameObject(hint, typeof(RectTransform));
            node.transform.SetParent(parent, false);
            node.AddComponent<LayoutElement>().preferredHeight = height;
            var back = node.AddComponent<Image>();
            MosseloraArt.Round(back, Color.white, 14);
            MosseloraArt.Edge(node.transform, MosseloraTheme.CardEdge, 14);
            var field = node.AddComponent<InputField>();
            Text shown = Inner(node.transform, "Text", value, MosseloraTheme.EarthInk);
            Text placeholder = Inner(node.transform, "Placeholder", hint, new Color(0.45f, 0.5f, 0.45f, 0.9f));
            placeholder.fontStyle = FontStyle.Italic;
            field.targetGraphic = back;
            field.textComponent = shown;
            field.placeholder = placeholder;
            field.lineType = multi ? InputField.LineType.MultiLineNewline : InputField.LineType.SingleLine;
            field.characterLimit = limit;
            field.text = value;
            return field;
        }

        private Text Inner(Transform parent, string name, string words, Color color)
        {
            var node = new GameObject(name, typeof(RectTransform));
            node.transform.SetParent(parent, false);
            var rect = (RectTransform)node.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(12f, 8f);
            rect.offsetMax = new Vector2(-12f, -8f);
            var text = node.AddComponent<Text>();
            text.font = font;
            text.fontSize = Mathf.RoundToInt(18 * TextScale.Percent / 100f);
            text.color = color;
            text.supportRichText = false;
            text.alignment = TextAnchor.UpperLeft;
            text.raycastTarget = false;
            text.text = words;
            return text;
        }
    }
}

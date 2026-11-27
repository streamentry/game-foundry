using System;

namespace Evergrove.Core
{
    /// <summary>
    /// I213: family parcels. Gifts MOVE seeds and never create them: sending subtracts from the
    /// sender, opening adds to the recipient (capped at MaxSeeds). A parcel opens once per garden,
    /// and a garden cannot open a parcel it sent. Only a Living garden sends or opens.
    /// </summary>
    public sealed partial class WorldModel
    {
        public const int MaxGifts = 50;
        public const int MaxLetters = 20;
        public const int MaxParcelIds = 200;
        /// <summary>At most one entry per grown thing, so this only needs to cover the catalogue.</summary>
        public const int MaxHelped = 50;

        /// <summary>Grown things a relative helped with (I226), oldest first; <see cref="HelpedFrom"/> holds who.</summary>
        public int[] Helped { get { return (int[])state.helped.Clone(); } }
        public string[] HelpedFrom { get { return (string[])state.helpedFrom.Clone(); } }
        public bool IsHelped(int item) { return Array.IndexOf(state.helped, item) >= 0; }
        /// <summary>Who helped with grown thing <paramref name="item"/>, or null.</summary>
        public string HelpedBy(int item)
        {
            int at = Array.IndexOf(state.helped, item);
            return at < 0 ? null : state.helpedFrom[at];
        }

        // ---- I228: fruit. Picked only by tending trees (see SolveTree/SolveBought), never by time.
        public const int MaxFruit = 99;
        public const int MaxFruitPerParcel = 10;
        /// <summary>Neighbours who can be given fruit: Mira 0, Orin 1, Fen 2.</summary>
        public const int Neighbours = 3;
        private const int MaxFruitGiven = 100000;

        /// <summary>Fruit in the garden, 0..<see cref="MaxFruit"/>.</summary>
        public int Fruit { get { return state.fruit; } }
        /// <summary>
        /// How many a neighbour was given. A private memory so they can speak truthfully:
        /// never show it as a score, a meter or a goal.
        /// </summary>
        public int FruitGivenTo(int villager)
        {
            return villager < 0 || villager >= Neighbours ? 0 : state.fruitGiven[villager];
        }
        /// <summary>The neighbour who got the latest fruit, or -1.</summary>
        public int LastFruitTo { get { return state.lastFruitTo; } }

        /// <summary>Gives one fruit to a neighbour (0 Mira, 1 Orin, 2 Fen). Needs a Living garden and some fruit.</summary>
        public bool GiveFruit(int villager)
        {
            if (Mode != PlayMode.Living || villager < 0 || villager >= Neighbours || state.fruit < 1) return false;
            int[] given = (int[])state.fruitGiven.Clone();
            given[villager] = Math.Min(MaxFruitGiven, given[villager] + 1);
            state.fruitGiven = given;
            state.fruit--;
            state.lastFruitTo = villager;
            return true;
        }

        /// <summary>One fruit from tending a tree, up to the cap (extra is simply not picked).</summary>
        private void PickFruit()
        {
            if (state.fruit < MaxFruit) state.fruit++;
        }

        /// <summary>Cuttings waiting in the basket, as nursery kinds, oldest first.</summary>
        public int[] Gifts { get { return (int[])state.gifts.Clone(); } }
        /// <summary>The newest opened letters, oldest first; <see cref="LetterFrom"/> holds who wrote each.</summary>
        public string[] Letters { get { return (string[])state.letters.Clone(); } }
        public string[] LetterFrom { get { return (string[])state.letterFrom.Clone(); } }

        public bool HasSeenParcel(string id) { return id != null && Array.IndexOf(state.parcelIds, id) >= 0; }

        /// <summary>
        /// Makes a parcel and pays for it, all or nothing. <paramref name="value"/> is the seed amount
        /// (1..500) for Seeds, the nursery kind for Cutting, or the grown thing to help with for Help; ignored for None.
        /// Help, like a letter, costs nothing. A cutting costs its
        /// nursery price and needs that kind's row open; it removes no plant. A letter costs nothing.
        /// </summary>
        public ParcelSendResult SendParcel(string from, string letter, ParcelGift gift, int value, string nonce)
        {
            var result = new ParcelSendResult();
            if (Mode != PlayMode.Living) return Fail(result, ParcelError.NotLiving);
            string text;
            ParcelContents contents;
            ParcelError error;
            if (!FamilyParcel.TryCreate(from, letter, gift, value, nonce, out text, out contents, out error))
                return Fail(result, error);
            if (HasSeenParcel(contents.Id)) return Fail(result, ParcelError.AlreadyUsed);
            int cost = 0;
            if (gift == ParcelGift.Seeds) cost = contents.Amount;
            else if (gift == ParcelGift.Cutting)
            {
                if (!RowOpen(Nursery.Row(contents.Kind))) return Fail(result, ParcelError.CuttingNotAvailable);
                cost = Nursery.Price(contents.Kind);
            }
            if (state.seeds < cost) return Fail(result, ParcelError.NotEnoughSeeds);
            if (gift == ParcelGift.Fruit && state.fruit < contents.Amount) return Fail(result, ParcelError.NotEnoughFruit);
            state.seeds -= cost;
            if (gift == ParcelGift.Fruit) state.fruit -= contents.Amount;
            RememberParcel(contents.Id);
            result.Ok = true;
            result.Error = ParcelError.None;
            result.Text = text;
            result.Id = contents.Id;
            result.Cost = cost;
            return result;
        }

        /// <summary>
        /// Opens pasted parcel text. Refusals change nothing. Seeds are added (capped at MaxSeeds);
        /// a cutting goes to the basket (see <see cref="PlantGift"/>); a letter is kept.
        /// </summary>
        public ParcelOpenResult Open(string text)
        {
            var result = new ParcelOpenResult();
            if (Mode != PlayMode.Living) return Refuse(result, ParcelError.NotLiving);
            ParcelContents contents;
            ParcelError error;
            if (!FamilyParcel.TryRead(text, out contents, out error)) return Refuse(result, error);
            if (HasSeenParcel(contents.Id)) return Refuse(result, ParcelError.AlreadyOpened);
            if (contents.Gift == ParcelGift.Cutting && state.gifts.Length >= MaxGifts) return Refuse(result, ParcelError.BasketFull);
            if (contents.Gift == ParcelGift.Help && contents.Kind >= state.choices.Length) return Refuse(result, ParcelError.NotInThisGarden);

            if (contents.Gift == ParcelGift.Seeds)
            {
                int gained = Math.Min(contents.Amount, MaxSeeds - state.seeds);
                state.seeds += gained;
                result.SeedsGained = gained;
            }
            else if (contents.Gift == ParcelGift.Fruit)
            {
                int gained = Math.Min(contents.Amount, MaxFruit - state.fruit);
                state.fruit += gained;
                result.FruitGained = gained;
            }
            else if (contents.Gift == ParcelGift.Cutting)
            {
                state.gifts = Append(state.gifts, contents.Kind);
                result.GiftIndex = state.gifts.Length - 1;
            }
            else if (contents.Gift == ParcelGift.Help)
            {
                RecordHelp(contents.Kind, contents.From);
                result.HelpedItem = contents.Kind;
            }
            if (contents.Letter.Length > 0)
            {
                state.letters = AppendCapped(state.letters, contents.Letter, MaxLetters);
                state.letterFrom = AppendCapped(state.letterFrom, contents.From, MaxLetters);
            }
            RememberParcel(contents.Id);
            result.Ok = true;
            result.Error = ParcelError.None;
            result.Contents = contents;
            return result;
        }

        /// <summary>Moves basket gift <paramref name="i"/> onto a map cell as a bought plant with its own puzzle. Free.</summary>
        public bool PlantGift(int i, int cell)
        {
            if (Mode != PlayMode.Living || i < 0 || i >= state.gifts.Length ||
                cell < 0 || cell >= MaxMapCell || BoughtCount >= MaxBought)
                return false;
            int kind = state.gifts[i];
            var rest = new int[state.gifts.Length - 1];
            Array.Copy(state.gifts, 0, rest, 0, i);
            Array.Copy(state.gifts, i + 1, rest, i, rest.Length - i);
            int[] bought = new int[state.bought.Length + 2];
            Array.Copy(state.bought, bought, state.bought.Length);
            bought[bought.Length - 2] = kind;
            bought[bought.Length - 1] = cell;
            state.gifts = rest;
            state.bought = bought;
            return true;
        }

        /// <summary>One entry per item: helping again replaces the sender and moves it to newest.</summary>
        private void RecordHelp(int item, string from)
        {
            int at = Array.IndexOf(state.helped, item);
            int others = state.helped.Length - (at < 0 ? 0 : 1);
            int keep = Math.Min(others, MaxHelped - 1);
            var items = new int[keep + 1];
            var names = new string[keep + 1];
            int skip = others - keep; // oldest entries beyond the cap
            int n = 0;
            for (int i = 0; i < state.helped.Length; i++)
            {
                if (i == at) continue;
                if (skip > 0) { skip--; continue; }
                items[n] = state.helped[i];
                names[n] = state.helpedFrom[i];
                n++;
            }
            items[n] = item;
            names[n] = from;
            state.helped = items;
            state.helpedFrom = names;
        }

        private void RememberParcel(string id)
        {
            state.parcelIds = AppendCapped(state.parcelIds, id, MaxParcelIds);
        }

        private static ParcelSendResult Fail(ParcelSendResult result, ParcelError error)
        {
            result.Error = error;
            return result;
        }

        private static ParcelOpenResult Refuse(ParcelOpenResult result, ParcelError error)
        {
            result.Error = error;
            return result;
        }

        private static int[] Append(int[] items, int value)
        {
            var next = new int[items.Length + 1];
            Array.Copy(items, next, items.Length);
            next[items.Length] = value;
            return next;
        }

        /// <summary>Appends and drops the oldest entries beyond <paramref name="cap"/>.</summary>
        private static string[] AppendCapped(string[] items, string value, int cap)
        {
            int keep = Math.Min(items.Length, cap - 1);
            var next = new string[keep + 1];
            Array.Copy(items, items.Length - keep, next, 0, keep);
            next[keep] = value;
            return next;
        }

        /// <summary>
        /// Fail-closed validation of the fruit fields (I228); a missing or empty count array means "absent in
        /// an older save". Returns the counts normalised to exactly three.
        /// </summary>
        private static bool ReadFruit(WorldSnapshot source, out int[] fruitGiven)
        {
            fruitGiven = null;
            int[] given = source.fruitGiven ?? Array.Empty<int>();
            if (source.fruit < 0 || source.fruit > MaxFruit || (given.Length != 0 && given.Length != Neighbours) ||
                source.lastFruitTo < -1 || source.lastFruitTo >= Neighbours)
                return false;
            var counts = new int[Neighbours];
            int total = 0;
            for (int i = 0; i < given.Length; i++)
            {
                if (given[i] < 0 || given[i] > MaxFruitGiven) return false;
                counts[i] = given[i];
                total += given[i];
            }
            // Truthful memory: the latest recipient must have been given something, and any gift names one.
            if (total == 0 ? source.lastFruitTo != -1 : (source.lastFruitTo == -1 || counts[source.lastFruitTo] == 0))
                return false;
            if (source.mode != "living" && (source.fruit != 0 || total != 0)) return false;
            fruitGiven = counts;
            return true;
        }

        /// <summary>Fail-closed validation of the help fields (I226); null arrays mean "absent in an older save".</summary>
        private static bool ReadHelped(WorldSnapshot source, out int[] helped, out string[] helpedFrom)
        {
            helped = source.helped ?? Array.Empty<int>();
            helpedFrom = source.helpedFrom ?? Array.Empty<string>();
            if (helped.Length != helpedFrom.Length || helped.Length > MaxHelped ||
                (helped.Length != 0 && source.mode != "living"))
                return false;
            for (int i = 0; i < helped.Length; i++)
                if (helped[i] < 0 || helped[i] >= source.choices.Length ||
                    Array.IndexOf(helped, helped[i]) != i || !FamilyParcel.ValidName(helpedFrom[i]))
                    return false;
            helped = (int[])helped.Clone();
            helpedFrom = (string[])helpedFrom.Clone();
            return true;
        }

        /// <summary>Fail-closed validation of the family fields; null arrays mean "absent in an older save".</summary>
        private static bool ReadFamily(WorldSnapshot source, out int[] gifts, out string[] parcelIds,
                                       out string[] letters, out string[] letterFrom)
        {
            gifts = source.gifts ?? Array.Empty<int>();
            parcelIds = source.parcelIds ?? Array.Empty<string>();
            letters = source.letters ?? Array.Empty<string>();
            letterFrom = source.letterFrom ?? Array.Empty<string>();
            if (gifts.Length > MaxGifts || parcelIds.Length > MaxParcelIds ||
                letters.Length > MaxLetters || letters.Length != letterFrom.Length ||
                (source.mode != "living" &&
                 (gifts.Length != 0 || parcelIds.Length != 0 || letters.Length != 0)))
                return false;
            for (int i = 0; i < gifts.Length; i++)
                if (gifts[i] < 0 || gifts[i] >= Nursery.Kinds) return false;
            for (int i = 0; i < parcelIds.Length; i++)
                if (!FamilyParcel.IsParcelId(parcelIds[i]) || Array.IndexOf(parcelIds, parcelIds[i]) != i) return false;
            for (int i = 0; i < letters.Length; i++)
                if (letters[i] == null || letters[i].Length == 0 || !FamilyParcel.ValidLetter(letters[i]) ||
                    !FamilyParcel.ValidName(letterFrom[i]))
                    return false;
            gifts = (int[])gifts.Clone();
            parcelIds = (string[])parcelIds.Clone();
            letters = (string[])letters.Clone();
            letterFrom = (string[])letterFrom.Clone();
            return true;
        }
    }
}

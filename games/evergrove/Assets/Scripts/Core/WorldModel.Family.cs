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

        /// <summary>Cuttings waiting in the basket, as nursery kinds, oldest first.</summary>
        public int[] Gifts { get { return (int[])state.gifts.Clone(); } }
        /// <summary>The newest opened letters, oldest first; <see cref="LetterFrom"/> holds who wrote each.</summary>
        public string[] Letters { get { return (string[])state.letters.Clone(); } }
        public string[] LetterFrom { get { return (string[])state.letterFrom.Clone(); } }

        public bool HasSeenParcel(string id) { return id != null && Array.IndexOf(state.parcelIds, id) >= 0; }

        /// <summary>
        /// Makes a parcel and pays for it, all or nothing. <paramref name="value"/> is the seed amount
        /// (1..500) for Seeds or the nursery kind for Cutting; ignored for None. A cutting costs its
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
            state.seeds -= cost;
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

            if (contents.Gift == ParcelGift.Seeds)
            {
                int gained = Math.Min(contents.Amount, MaxSeeds - state.seeds);
                state.seeds += gained;
                result.SeedsGained = gained;
            }
            else if (contents.Gift == ParcelGift.Cutting)
            {
                state.gifts = Append(state.gifts, contents.Kind);
                result.GiftIndex = state.gifts.Length - 1;
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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Evergrove.Core
{
    public enum ParcelGift { None = 0, Seeds = 1, Cutting = 2 }

    public enum ParcelError
    {
        None = 0,
        Empty,
        TooLarge,
        Malformed,
        WrongFormat,
        UnsupportedFutureVersion,
        ChecksumMismatch,
        /// <summary>Well-formed text whose contents break the rules (forged amount, bad name, wrong id...).</summary>
        Invalid,
        AlreadyOpened,
        /// <summary>Sending: a parcel with the same contents and nonce was already sent from this garden.</summary>
        AlreadyUsed,
        NotLiving,
        NotEnoughSeeds,
        CuttingNotAvailable,
        BasketFull
    }

    public sealed class ParcelContents
    {
        public string Id;
        public string Nonce;
        public string From;
        public string Letter = "";
        public ParcelGift Gift;
        public int Amount;
        public int Kind = -1;
    }

    public sealed class ParcelSendResult
    {
        public bool Ok;
        public ParcelError Error;
        public string Text;
        public string Id;
        public int Cost;
    }

    public sealed class ParcelOpenResult
    {
        public bool Ok;
        public ParcelError Error;
        public ParcelContents Contents;
        /// <summary>Seeds that actually arrived (less than sent only when the garden hit its seed cap).</summary>
        public int SeedsGained;
        /// <summary>Basket index of a received cutting, else -1.</summary>
        public int GiftIndex = -1;
    }

    /// <summary>
    /// A family parcel: one gift (seeds, a cutting, or nothing but a letter) from one garden to
    /// another, as a small checksummed JSON text shared through any messaging app. Pure C#, no
    /// clock, no randomness: the caller supplies a nonce. The checksum and id catch accidental
    /// damage and forged ids; they are NOT tamper-proofing (anyone can recompute them), so the
    /// rules (amount cap, strict shapes) and the garden's economy checks do the real work.
    /// Reading is as defensive as the garden file: size cap, strict keys, depth limit, fail closed.
    /// </summary>
    public static class FamilyParcel
    {
        public const string FormatName = "mosselora-parcel";
        public const int CurrentFormatVersion = 1;
        public const int MaxParcelBytes = 4096;
        public const int MaxNameLength = 24;
        public const int MaxLetterLength = 280;
        public const int MaxLetterLines = 6;
        public const int MaxSeedsPerParcel = 500;
        public const int MaxNonceLength = 32;
        public const int IdLength = 24;
        private const string NameAndLetterPunctuation = ".,'’!?-:;()&\"";

        private static readonly HashSet<string> EnvelopeKeys = new HashSet<string> { "format", "formatVersion", "checksumSha256", "parcel" };
        private static readonly HashSet<string> ParcelKeys = new HashSet<string> { "id", "nonce", "from", "letter", "gift", "amount", "kind" };

        /// <summary>Builds a parcel after normalising (trim, CRLF to LF) and validating its contents.</summary>
        public static bool TryCreate(string from, string letter, ParcelGift gift, int value, string nonce,
                                     out string text, out ParcelContents contents, out ParcelError error)
        {
            text = null;
            contents = null;
            error = ParcelError.Invalid;
            var c = new ParcelContents {
                From = from == null ? null : from.Trim(),
                Letter = letter == null ? "" : letter.Replace("\r\n", "\n").Trim(),
                Gift = gift,
                Amount = gift == ParcelGift.Seeds ? value : 0,
                Kind = gift == ParcelGift.Cutting ? value : -1,
                Nonce = nonce
            };
            if (!ValidNonce(c.Nonce) || !ValidShape(c)) return false;
            c.Id = MakeId(c);
            text = BuildText(c);
            contents = c;
            error = ParcelError.None;
            return true;
        }

        /// <summary>Reads pasted text. Checks run in order: size, JSON, identity, version, keys, checksum, rules.</summary>
        /// <summary>Marks the start of a wrapped parcel token in shared text.</summary>
        public const string TokenMark = "MOSSELORA-PARCEL:";

        /// <summary>
        /// The text actually shared (I214): a friendly line, then one token with no
        /// quotes, braces or spaces (URL-safe base64 of the parcel), so messaging
        /// apps can't "smarten" or wrap it, and it survives being forwarded.
        /// </summary>
        public static string Wrap(string parcel, string friendlyLine)
        {
            if (parcel == null) throw new ArgumentNullException(nameof(parcel));
            string token = Convert.ToBase64String(Encoding.UTF8.GetBytes(parcel))
                .TrimEnd('=').Replace('+', '-').Replace('/', '_');
            string line = (friendlyLine ?? "").Replace('"', '\'').Trim();
            return (line.Length > 0 ? line + "\n" : "") + TokenMark + token;
        }

        /// <summary>The parcel inside any pasted text, or null. A raw parcel passes through.</summary>
        public static string Unwrap(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            string trimmed = text.Trim();
            if (trimmed.StartsWith("{", StringComparison.Ordinal)) return trimmed;
            int at = text.IndexOf(TokenMark, StringComparison.Ordinal);
            if (at < 0) return null;
            int start = at + TokenMark.Length, end = start;
            while (end < text.Length && (char.IsLetterOrDigit(text[end]) || text[end] == '-' || text[end] == '_') &&
                   text[end] < 128) end++;
            int length = end - start;
            if (length == 0 || length > MaxParcelBytes * 2) return null;
            string b64 = text.Substring(start, length).Replace('-', '+').Replace('_', '/');
            if (b64.Length % 4 == 1) return null; // never a valid base64 length: cut off
            b64 = b64.PadRight(b64.Length + (4 - b64.Length % 4) % 4, '=');
            try { return Encoding.UTF8.GetString(Convert.FromBase64String(b64)); }
            catch (FormatException) { return null; }
        }

        public static bool TryRead(string text, out ParcelContents contents, out ParcelError error)
        {
            contents = null;
            error = ParcelError.None;
            if (text == null || text.Trim().Length == 0) { error = ParcelError.Empty; return false; }
            if (Encoding.UTF8.GetByteCount(text) > MaxParcelBytes) { error = ParcelError.TooLarge; return false; }

            object root;
            var envelope = GardenTransfer.Parser.TryParse(text.Trim(), out root) ? root as Dictionary<string, object> : null;
            if (envelope == null) { error = ParcelError.Malformed; return false; }

            object formatValue;
            if (!envelope.TryGetValue("format", out formatValue) || formatValue as string != FormatName)
            { error = ParcelError.WrongFormat; return false; }
            object versionValue;
            int version;
            if (!envelope.TryGetValue("formatVersion", out versionValue) || !GardenTransfer.AsInt(versionValue, out version) || version < 1)
            { error = ParcelError.WrongFormat; return false; }
            if (version > CurrentFormatVersion) { error = ParcelError.UnsupportedFutureVersion; return false; }

            object checksumValue, parcelValue;
            var fields = envelope.TryGetValue("parcel", out parcelValue) ? parcelValue as Dictionary<string, object> : null;
            string checksum = envelope.TryGetValue("checksumSha256", out checksumValue) ? checksumValue as string : null;
            ParcelContents parsed;
            if (checksum == null || fields == null || !OnlyKeys(envelope, EnvelopeKeys) || !OnlyKeys(fields, ParcelKeys) ||
                !TryReadFields(fields, out parsed))
            { error = ParcelError.Malformed; return false; }

            if (!string.Equals(GardenTransfer.Sha256Hex(ParcelJson(parsed, true)), checksum, StringComparison.Ordinal))
            { error = ParcelError.ChecksumMismatch; return false; }
            if (!ValidNonce(parsed.Nonce) || !ValidShape(parsed) || !IsParcelId(parsed.Id) ||
                !string.Equals(parsed.Id, MakeId(parsed), StringComparison.Ordinal))
            { error = ParcelError.Invalid; return false; }
            contents = parsed;
            return true;
        }

        /// <summary>Deterministic id: first 24 hex characters of SHA-256 over the contents and nonce.</summary>
        internal static string MakeId(ParcelContents c)
        {
            return GardenTransfer.Sha256Hex("mosselora-parcel-id|" + ParcelJson(c, false)).Substring(0, IdLength);
        }

        /// <summary>Writes the envelope without checking the rules. Tests use it to forge; TryCreate validates first.</summary>
        internal static string BuildText(ParcelContents c)
        {
            return "{\"format\":" + GardenTransfer.Quote(FormatName) +
                ",\"formatVersion\":" + GardenTransfer.Int(CurrentFormatVersion) +
                ",\"checksumSha256\":\"" + GardenTransfer.Sha256Hex(ParcelJson(c, true)) + "\"" +
                ",\"parcel\":" + ParcelJson(c, true) + "}";
        }

        public static bool IsParcelId(string id)
        {
            if (id == null || id.Length != IdLength) return false;
            foreach (char ch in id)
                if (!((ch >= '0' && ch <= '9') || (ch >= 'a' && ch <= 'f'))) return false;
            return true;
        }

        public static bool ValidName(string name)
        {
            return name != null && name.Length >= 1 && name.Length <= MaxNameLength && name == name.Trim() && AllowedChars(name, false);
        }

        /// <summary>A kept or sent letter: at most 280 characters and 6 lines, trimmed, no control characters.</summary>
        public static bool ValidLetter(string letter)
        {
            if (letter == null || letter.Length > MaxLetterLength || letter != letter.Trim() || !AllowedChars(letter, true))
                return false;
            int lines = 1;
            foreach (char ch in letter) if (ch == '\n') lines++;
            return lines <= MaxLetterLines;
        }

        public static bool ValidNonce(string nonce)
        {
            if (nonce == null || nonce.Length < 1 || nonce.Length > MaxNonceLength) return false;
            foreach (char ch in nonce)
                if (!((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9') || ch == '-' || ch == '_'))
                    return false;
            return true;
        }

        private static bool ValidShape(ParcelContents c)
        {
            if (!ValidName(c.From) || !ValidLetter(c.Letter)) return false;
            switch (c.Gift)
            {
                case ParcelGift.None:
                    return c.Amount == 0 && c.Kind == -1 && c.Letter.Length > 0;
                case ParcelGift.Seeds:
                    return c.Amount >= 1 && c.Amount <= MaxSeedsPerParcel && c.Kind == -1;
                case ParcelGift.Cutting:
                    return c.Amount == 0 && c.Kind >= 0 && c.Kind < Nursery.Kinds;
                default:
                    return false;
            }
        }

        private static bool AllowedChars(string text, bool allowNewline)
        {
            foreach (char ch in text)
            {
                if (ch == ' ' || (allowNewline && ch == '\n') || NameAndLetterPunctuation.IndexOf(ch) >= 0) continue;
                if (char.IsLetterOrDigit(ch)) continue;
                UnicodeCategory category = char.GetUnicodeCategory(ch);
                if (category == UnicodeCategory.NonSpacingMark || category == UnicodeCategory.SpacingCombiningMark) continue;
                return false;
            }
            return true;
        }

        private static bool OnlyKeys(Dictionary<string, object> fields, HashSet<string> allowed)
        {
            if (fields.Count != allowed.Count) return false;
            foreach (string key in fields.Keys)
                if (!allowed.Contains(key)) return false;
            return true;
        }

        private static bool TryReadFields(Dictionary<string, object> fields, out ParcelContents c)
        {
            c = new ParcelContents();
            string gift;
            int amount, kind;
            c.Id = fields["id"] as string;
            c.Nonce = fields["nonce"] as string;
            c.From = fields["from"] as string;
            c.Letter = fields["letter"] as string;
            gift = fields["gift"] as string;
            if (c.Id == null || c.Nonce == null || c.From == null || c.Letter == null || gift == null ||
                !GardenTransfer.AsInt(fields["amount"], out amount) || !GardenTransfer.AsInt(fields["kind"], out kind))
                return false;
            c.Amount = amount;
            c.Kind = kind;
            switch (gift)
            {
                case "none": c.Gift = ParcelGift.None; break;
                case "seeds": c.Gift = ParcelGift.Seeds; break;
                case "cutting": c.Gift = ParcelGift.Cutting; break;
                default: c.Gift = (ParcelGift)(-1); break; // fails ValidShape, so the parcel is Invalid
            }
            return true;
        }

        private static string GiftName(ParcelGift gift)
        {
            switch (gift)
            {
                case ParcelGift.Seeds: return "seeds";
                case ParcelGift.Cutting: return "cutting";
                case ParcelGift.None: return "none";
                default: return "unknown";
            }
        }

        private static string ParcelJson(ParcelContents c, bool includeId)
        {
            return "{" + (includeId ? "\"id\":" + GardenTransfer.Quote(c.Id) + "," : "") +
                "\"nonce\":" + GardenTransfer.Quote(c.Nonce) +
                ",\"from\":" + GardenTransfer.Quote(c.From) +
                ",\"letter\":" + GardenTransfer.Quote(c.Letter) +
                ",\"gift\":" + GardenTransfer.Quote(GiftName(c.Gift)) +
                ",\"amount\":" + GardenTransfer.Int(c.Amount) +
                ",\"kind\":" + GardenTransfer.Int(c.Kind) + "}";
        }
    }
}

using System;
using System.IO;

namespace Evergrove.Core
{
    /// <summary>
    /// Best-effort offline journal for a single-process local save.
    /// Never rotate an invalid primary over the last valid backup.
    /// The validated staging file remains available if publication fails.
    /// This is NOT a guarantee of power-loss atomicity or durable fsync on every OS.
    /// </summary>
    public static class SaveJournal
    {
        private static bool ReadValid(string path, Func<string, bool> accepts, out string data)
        {
            data = null;
            try
            {
                if (!File.Exists(path)) return false;
                string candidate = File.ReadAllText(path);
                if (!accepts(candidate)) return false;
                data = candidate;
                return true;
            }
            catch (Exception)
            {
                // A malformed or temporarily unreadable slot must never win recovery.
                return false;
            }
        }

        /// <summary>Load validated primary, then backup, then staged save.</summary>
        public static bool TryLoad(string primary, Func<string, bool> accepts,
                                   out string payload, out string origin)
        {
            if (string.IsNullOrEmpty(primary)) throw new ArgumentException("Save path is required.", nameof(primary));
            if (accepts == null) throw new ArgumentNullException(nameof(accepts));
            if (ReadValid(primary, accepts, out payload))
            {
                origin = "primary";
                return true;
            }
            if (ReadValid(primary + ".bak", accepts, out payload))
            {
                origin = "backup";
                return true;
            }
            if (ReadValid(primary + ".tmp", accepts, out payload))
            {
                origin = "temporary";
                return true;
            }
            payload = null;
            origin = null;
            return false;
        }

        /// <summary>
        /// Only replace the primary after a fully validated staging write and
        /// a validated primary has been copied to backup. On a failed copy
        /// the original primary or backup (or complete staging file) survives.
        /// A caller must block saves after an unrecoverable load until the
        /// player explicitly accepts a reset; this class cannot infer intent.
        /// </summary>
        public static bool Commit(string primary, string payload, Func<string, bool> accepts)
        {
            if (string.IsNullOrEmpty(primary)) throw new ArgumentException("Save path is required.", nameof(primary));
            if (accepts == null) throw new ArgumentNullException(nameof(accepts));
            if (payload == null || !accepts(payload)) return false;

            string directory = Path.GetDirectoryName(primary);
            if (string.IsNullOrEmpty(directory))
                throw new ArgumentException("An absolute save path is required.", nameof(primary));
            Directory.CreateDirectory(directory);
            string staged = primary + ".tmp";
            string backup = primary + ".bak";

            File.WriteAllText(staged, payload);
            string validated;
            if (!ReadValid(staged, accepts, out validated) ||
                !string.Equals(validated, payload, StringComparison.Ordinal))
                return false;

            // Do not let a corrupted old primary replace the only valid backup.
            // If backup copy fails, publication has not started and the primary
            // is still intact. The new candidate remains staged.
            string old;
            if (ReadValid(primary, accepts, out old))
            {
                File.Copy(primary, backup, true);
                if (!ReadValid(backup, accepts, out old))
                    return false;
            }

            if (File.Exists(primary))
                File.Copy(staged, primary, true);
            else
                File.Move(staged, primary);

            if (!ReadValid(primary, accepts, out validated) ||
                !string.Equals(validated, payload, StringComparison.Ordinal))
                return false;

            // A successful primary write must not be reported as failed just
            // because optional cleanup cannot remove a stale stage.
            try { if (File.Exists(staged)) File.Delete(staged); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            return true;
        }

        public static bool HasAnySlot(string primary)
        {
            return File.Exists(primary) || File.Exists(primary + ".bak") ||
                   File.Exists(primary + ".tmp");
        }
    }
}

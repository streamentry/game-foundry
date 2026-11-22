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

            // Recovery corner case: if .tmp is the ONLY valid save left, a
            // normal staging write would destroy it before a second good copy
            // exists. Preserve that last good staged snapshot in .bak FIRST.
            // Also fail closed when existing files are all unreadable; callers
            // must never turn data corruption into silent world replacement.
            string oldPrimary, oldBackup, oldStaged;
            bool primaryValid = ReadValid(primary, accepts, out oldPrimary);
            bool backupValid = ReadValid(backup, accepts, out oldBackup);
            bool stagedValid = ReadValid(staged, accepts, out oldStaged);
            if (!primaryValid && !backupValid)
            {
                if (!stagedValid && HasAnySlot(primary)) return false;
                if (stagedValid)
                {
                    File.Copy(staged, backup, true);
                    string preserved;
                    if (!ReadValid(backup, accepts, out preserved) ||
                        !string.Equals(preserved, oldStaged, StringComparison.Ordinal))
                        return false;
                }
            }

            File.WriteAllText(staged, payload);
            string validated;
            if (!ReadValid(staged, accepts, out validated) ||
                !string.Equals(validated, payload, StringComparison.Ordinal))
                return false;

            // A validated primary can be rotated to backup only after a
            // validated staging write exists. A corrupt primary must never
            // overwrite the last good backup.
            if (primaryValid)
            {
                File.Copy(primary, backup, true);
                string preserved;
                if (!ReadValid(backup, accepts, out preserved) ||
                    !string.Equals(preserved, oldPrimary, StringComparison.Ordinal))
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

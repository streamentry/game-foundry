using System;
using System.Collections;
using System.Globalization;
using Evergrove.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
namespace Evergrove.UnityClient
{
    public sealed partial class EvergroveApp
    {

        private void BuildGardenFileSection()
        {
            var card = Card(content, "Garden file", MosseloraTheme.CardFill);
            BuildSaveTrustLine(card.transform); // EvergroveApp.Saving.cs (checklist 9): first line, above every button and prompt
            GameObject files = Row(card.transform, "Garden file actions");
            IconButton(files.transform, "Copy garden file", GameLocalization.Text("garden_copy"),
                MosseloraArt.IconCopyFile, CopyGardenFile, 78, MosseloraTheme.ForestAction,
                false, null, true);
            BuildSaveGardenFileButton(files.transform); // EvergroveApp.Saving.cs (item 96)
            if (!WorldStore.RecoveryBlocked)
            {
                if (!gardenRestoreConfirm)
                    IconButton(files.transform, "Restore garden file", GameLocalization.Text("garden_restore"),
                        MosseloraArt.IconRestoreFile, ReadGardenFile, 78, MosseloraTheme.WaterAction,
                        false, null, true);
                else
                {
                    Label(card.transform, "Garden replace warning", GameLocalization.Text("garden_replace"),
                        13, 40, Ink);
                    if (pendingGardenRestore != null)
                    {
                        string[] compare = GardenCompare.Lines(world, pendingGardenRestore);
                        for (int i = 0; i < compare.Length; i++)
                            Label(card.transform, "Garden restore compare " + i, compare[i], 13, 40, Ink);
                    }
                    // I219: restoring is what the player came to do, not an erasure: the button
                    // names the restore, in the restore's own colour; the warning above stays.
                    IconButton(card.transform, "Confirm garden restore", GameLocalization.Text("garden_restore"),
                        MosseloraArt.IconRestoreFile, ConfirmGardenRestore, 54, MosseloraTheme.WaterAction, true);
                    IconButton(card.transform, "Cancel garden restore", GameLocalization.Text("keep"),
                        MosseloraArt.IconCancel, CancelGardenRestore, 52, MosseloraTheme.QuietAction);
                }
            }
            if (gardenNoticeKey.Length > 0)
                Label(card.transform, "Garden file notice", GameLocalization.Text(gardenNoticeKey),
                    13, 40, gardenNoticeIsError ? MosseloraTheme.EarthInk : Moss);
        }

        private void SetGardenNotice(string key, bool isError)
        {
            gardenNoticeKey = key ?? "";
            gardenNoticeIsError = isError;
        }

        private void CopyGardenFile()
        {
            string stamp = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);
            GUIUtility.systemCopyBuffer = GardenTransfer.ExportForPeople(world, stamp);
            gardenRestoreConfirm = false;
            pendingGardenRestore = null;
            SetGardenNotice("garden_copied", false);
            Rebuild();
        }

        private void ReadGardenFile()
        {
            WorldModel parsed;
            GardenImportError error;
            if (!GardenTransfer.TryImport(GUIUtility.systemCopyBuffer ?? "", out parsed, out error))
            {
                gardenRestoreConfirm = false;
                pendingGardenRestore = null;
                SetGardenNotice(GardenErrorKey(error), true);
                Rebuild();
                return;
            }
            // Nothing is replaced until the second, explicit confirmation.
            pendingGardenRestore = parsed;
            gardenRestoreConfirm = true;
            SetGardenNotice("", false);
            Rebuild();
        }

        private void ConfirmGardenRestore()
        {
            if (pendingGardenRestore == null || WorldStore.RecoveryBlocked)
            {
                CancelGardenRestore();
                return;
            }
            world = pendingGardenRestore;
            pendingGardenRestore = null;
            gardenRestoreConfirm = false;
            SetGardenNotice("", false);
            speakingWith = -1;
            showingMemory = false;
            showJournal = false;
            confirmReset = false;
            SaveWorld(); // same persistence path as every other progress change
            Rebuild(true);
        }

        private void CancelGardenRestore()
        {
            gardenRestoreConfirm = false;
            pendingGardenRestore = null;
            Rebuild();
        }

        private static string GardenErrorKey(GardenImportError error)
        {
            switch (error)
            {
                case GardenImportError.Empty: return "garden_err_empty";
                case GardenImportError.WrongFormat: return "garden_err_not_garden";
                case GardenImportError.UnsupportedFutureVersion: return "garden_err_newer";
                case GardenImportError.TooLarge: return "garden_err_too_large";
                default: return "garden_err_damaged"; // Malformed, ChecksumMismatch, InvalidSnapshot
            }
        }

        private void ConfirmedReset()
        {
            if (!WorldStore.Clear())
            {
                // Never announce a new saved garden when any old slot could
                // not be cleared; keep the in-memory world and warn instead.
                saveFailed = true;
                Rebuild();
                return;
            }
            // A new garden meets the next twelve trees, flowers and builds (I172),
            // and remembers which garden it is in its own save (I178).
            gardenNumber = Mathf.Max(gardenNumber, world != null ? world.Garden : 0) + 1;
            world = WorldModel.New(PlayMode.Living, gardenNumber);
            mapInHand = -1;
            bool persist = true;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            persist = !qaInMemoryOnly;
#endif
            if (persist)
            {
                PlayerPrefs.SetInt(GardenNumberKey, gardenNumber);
                PlayerPrefs.Save();
            }
            saveFailed = false;
            confirmReset = false;
            showJournal = false;
            showingMemory = false;
            speakingWith = -1;
            Rebuild(true);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void RecordReturnDiaryOpen()
        {
            if (returnDiary.RecordOpen(DateTime.Now.Date)) SaveReturnDiary();
        }

        private void SaveReturnDiary()
        {
            PlayerPrefs.SetString(ReturnDiaryKey, returnDiary.Serialize());
            PlayerPrefs.Save();
        }

        private void ClearReturnDiary()
        {
            returnDiary.Delete();
            returnDiaryCopied = false;
            PlayerPrefs.DeleteKey(ReturnDiaryKey);
            PlayerPrefs.Save();
        }

        private void ResearchPause(bool paused)
        {
            if (!paused) RecordReturnDiaryOpen();
        }
#endif
    }
}

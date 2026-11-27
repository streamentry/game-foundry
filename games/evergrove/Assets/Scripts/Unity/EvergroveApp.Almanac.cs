using System;
using System.Collections.Generic;
using System.Globalization;
using Evergrove.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    public sealed partial class EvergroveApp
    {
        // ---- v1 #36: the ways the player has solved, named once found (device-only record).
        private void TechniqueRow(Transform page)
        {
            var names = new List<string>();
            foreach (SudokuTechnique.Technique technique in new[] { SudokuTechnique.Technique.NakedSingle, SudokuTechnique.Technique.HiddenSingle })
                if (SudokuBoard.TechniqueLog.Seen(technique)) names.Add(SudokuTechnique.Name(technique));
            if (names.Count == 0) { RecipeRow(page); return; }
            Text line = Label(page, "Ways you've solved", "Ways you've solved: " + string.Join(", ", names.ToArray()),
                18, 0, Moss, TextAnchor.MiddleLeft);
            Emphasize(line);
            RecipeRow(page);
        }

        // ---- Item 75: the recipes found by cooking, once any is found (nothing is shown before that).
        private void RecipeRow(Transform page)
        {
            if (world == null) return;
            string[] names = Recipes.Found(world);
            if (names.Length == 0) return;
            Text line = Label(page, "Recipes found", "Recipes found: " + string.Join(", ", names), 18, 0, Moss, TextAnchor.MiddleLeft);
            Emphasize(line);
        }

        // ---- v1 #58: the weekly diary. The week-start save is kept per garden on the device.
        // Living gardens only. Called from AlmanacPage.
        private void WeeklyDiaryRow(Transform page)
        {
            if (world == null || world.Mode != PlayMode.Living) return;
            DiaryPage diary;
            if (!TryWeeklyDiary(out diary)) return;
            var paper = Card(page, "Weekly diary", MosseloraTheme.CardFill);
            Text title = Label(paper.transform, "Diary title", diary.Title, 20, 0, Ink, TextAnchor.MiddleLeft);
            Emphasize(title);
            foreach (string text in diary.Lines)
                Label(paper.transform, "Diary line", text, 17, 0, Ink, TextAnchor.MiddleLeft);
        }

        // Builds the current ISO week's page from the stored week-start save.
        // - Same week and the save reads back: the page is the difference from that start.
        // - A new week: today's save becomes the start, so the page is the quiet week.
        // - Same week but the save cannot be read back: it is reset to now and no page is shown.
        private bool TryWeeklyDiary(out DiaryPage diary)
        {
            diary = null;
            int week = WeeklyBoard.IsoWeekOf(LocalDayStamp());
            string id = world.Garden.ToString(CultureInfo.InvariantCulture);
            string keyName = "evergrove:week:" + id + ":key";
            string snapName = "evergrove:week:" + id + ":snapshot";
            try
            {
                bool sameWeek = PlayerPrefs.GetInt(keyName, 0) == week;
                WorldModel start;
                GardenImportError error;
                if (sameWeek && GardenTransfer.TryImport(PlayerPrefs.GetString(snapName, ""), out start, out error)
                    && start.Garden == world.Garden)
                {
                    diary = WeeklyDiary.Write(start.Export(), world.Export(), week % 100, week / 100);
                    return true;
                }
                StoreWeekStart(keyName, snapName, week);
                if (sameWeek) return false;
                diary = WeeklyDiary.Write(world.Export(), world.Export(), week % 100, week / 100);
                return true;
            }
            catch (Exception)
            {
                // The diary is an extra page: if it cannot be written, the almanac simply shows none.
                return false;
            }
        }

        // Takes the week-start save the first time the hub opens in a new ISO week, so the diary counts
        // the whole week, not just what happened after the almanac was first opened. Living gardens only.
        private void EnsureWeekStart()
        {
            if (world == null || world.Mode != PlayMode.Living) return;
            try
            {
                int week = WeeklyBoard.IsoWeekOf(LocalDayStamp());
                string id = world.Garden.ToString(CultureInfo.InvariantCulture);
                string keyName = "evergrove:week:" + id + ":key";
                if (PlayerPrefs.GetInt(keyName, 0) != week)
                    StoreWeekStart(keyName, "evergrove:week:" + id + ":snapshot", week);
            }
            catch (Exception)
            {
                // The diary is an extra page; failing to store its start never affects play.
            }
        }

        private void StoreWeekStart(string keyName, string snapName, int week)
        {
            PlayerPrefs.SetString(snapName, GardenTransfer.Export(world, DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)));
            PlayerPrefs.SetInt(keyName, week);
            PlayerPrefs.Save();
        }

        // ---- v1 #52: a quiet line on the cottage card, shown once the garden has been played on a day.
        private void CottageDaysLine(Transform parent, int days)
        {
            if (days <= 0) return;
            CottageLine(parent, MosseloraArt.Load(MosseloraArt.Sprout),
                "Days in the garden: " + days.ToString(CultureInfo.InvariantCulture));
        }
    }
}

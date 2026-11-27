using System;
using System.Globalization;
using Evergrove.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    public sealed partial class EvergroveApp
    {
        // ---- Checklist 87: dates to remember. The player marks real dates; the neighbours mark them on the day.
        // PRIVACY: the list is kept only in device-local PlayerPrefs (ChosenDates.ListKey). It is never put in
        // the garden save, the garden file, family parcels, postcards, notifications or any share text, and
        // this file references none of those.
        private GameObject datesSheet;
        private ChosenDateKind draftKind = ChosenDateKind.Birthday;
        private int draftMonth = 1;
        private int draftDay = 1;
        private string draftLabel = "";
        private string datesNote = ""; // why the last action was refused, or that a date was added

        // The menu entry. Dates are device-only, so they are offered in every garden.
        private void BuildDatesEntry()
        {
            if (world == null) return;
            Button(content, "Dates to remember", "Dates to remember", () => { SetMenu(false); OpenDatesSheet(); });
        }

        internal void OpenDatesSheet()
        {
            if (titleScreen == null) return;
            DateTime today = DateTime.Now;
            draftMonth = today.Month;
            draftDay = today.Day;
            draftKind = ChosenDateKind.Birthday;
            draftLabel = "";
            datesNote = "";
            BuildDatesSheet();
        }

        internal void CloseDatesSheet() { RetireDates(false); }

        private void RetireDates(bool replacing)
        {
            RetireSheet(datesSheet, "Dates sheet", replacing);
            datesSheet = null;
            semanticsDirty = true;
        }

        private void BuildDatesSheet()
        {
            if (titleScreen == null) return;
            bool replacing = datesSheet != null;
            RetireDates(replacing);
            datesSheet = BuildSheet(titleScreen.transform, "Dates sheet", CloseDatesSheet,
                out RectTransform sheetContent, out ScrollRect sheetScroll);
            SheetClose(sheetContent, "Close dates", CloseDatesSheet);
            Text title = Label(sheetContent, "Dates title", "Dates to remember", 26, 0, Ink, TextAnchor.MiddleCenter);
            Emphasize(title);
            Label(sheetContent, "Dates hint", "Add a birthday, an anniversary or another day. The neighbours mark it on that day, once.",
                18, 0, Ink, TextAnchor.MiddleCenter);
            Label(sheetContent, "Dates privacy", "These dates stay on this phone. They are never sent anywhere.",
                18, 0, Ink, TextAnchor.MiddleCenter);

            ChosenDate[] dates = LoadChosenDates();
            for (int i = 0; i < dates.Length; i++) DateRow(sheetContent, dates, i);
            if (dates.Length < ChosenDates.MaxDates) DraftControls(sheetContent);
            else Label(sheetContent, "Dates full", "Six dates is the most. Remove one to add another.",
                18, 0, Ink, TextAnchor.MiddleCenter);
            if (datesNote.Length > 0) Label(sheetContent, "Dates note", datesNote, 18, 0, Ink, TextAnchor.MiddleCenter);

            SheetMotion.Show(datesSheet, SheetEdge.For("Dates sheet"), ReducedMotion, null, replacing);
            datesSheet.transform.SetAsLastSibling();
            FitSheet(datesSheet, "Dates sheet panel"); // BuildSheet names it "<name> panel"
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(sheetContent);
            FitSheetToPages(datesSheet, sheetContent);
            if (sheetScroll != null) sheetScroll.verticalNormalizedPosition = 1f;
            semanticsDirty = true;
        }

        // One saved date, with a remove button.
        private void DateRow(Transform parent, ChosenDate[] dates, int index)
        {
            ChosenDate date = dates[index];
            Text words = Label(parent, "Date " + (index + 1), DateWords(date), 18, 0, Ink, TextAnchor.MiddleLeft);
            words.GetComponent<LayoutElement>().flexibleWidth = 1f;
            Button(parent, "Remove date " + (index + 1), "Remove " + DateWords(date), () => RemoveChosenDate(index), 48, false);
        }

        // The add form: kind, month and day steppers, an optional label, then the add button.
        private void DraftControls(Transform parent)
        {
            Button(parent, "Date kind", "Kind: " + KindWords(draftKind), () => { draftKind = NextKind(draftKind); BuildDatesSheet(); }, 48, false);
            Button(parent, "Earlier month", "Earlier month", () => { draftMonth = draftMonth == 1 ? 12 : draftMonth - 1; ClampDraftDay(); BuildDatesSheet(); }, 48, false);
            Label(parent, "Draft month", "Month: " + MonthName(draftMonth), 18, 0, Ink, TextAnchor.MiddleCenter);
            Button(parent, "Later month", "Later month", () => { draftMonth = draftMonth == 12 ? 1 : draftMonth + 1; ClampDraftDay(); BuildDatesSheet(); }, 48, false);
            Button(parent, "Earlier day", "Earlier day", () => { draftDay = draftDay == 1 ? DaysIn(draftMonth) : draftDay - 1; BuildDatesSheet(); }, 48, false);
            Label(parent, "Draft day", "Day: " + draftDay, 18, 0, Ink, TextAnchor.MiddleCenter);
            Button(parent, "Later day", "Later day", () => { draftDay = draftDay >= DaysIn(draftMonth) ? 1 : draftDay + 1; BuildDatesSheet(); }, 48, false);
            InputField label = NameField(parent, draftLabel, "Optional label, like Nan's");
            label.onValueChanged.AddListener(text => draftLabel = text); // kept across rebuilds
            Button(parent, "Add this date", "Add this date", AddChosenDate, 52, true);
        }

        private void AddChosenDate()
        {
            string clean;
            if (!ChosenDates.CleanLabel(draftLabel, out clean))
            {
                datesNote = "Keep the label to 24 letters or fewer, with no < or > and no line breaks.";
                BuildDatesSheet();
                return;
            }
            ChosenDate date;
            if (!ChosenDates.TryMake(draftMonth, draftDay, draftKind, clean, out date))
            {
                datesNote = "That day does not exist. Pick another.";
                BuildDatesSheet();
                return;
            }
            ChosenDate[] next;
            if (!ChosenDates.TryAdd(LoadChosenDates(), date, out next))
            {
                datesNote = "That day is already marked, or six dates is the most. Remove one first.";
                BuildDatesSheet();
                return;
            }
            SaveChosenDates(next);
            draftLabel = "";
            datesNote = "Added. The neighbours will mark it on that day.";
            BuildDatesSheet();
        }

        private void RemoveChosenDate(int index)
        {
            ChosenDate[] next;
            if (ChosenDates.TryRemoveAt(LoadChosenDates(), index, out next)) SaveChosenDates(next);
            datesNote = "Removed.";
            BuildDatesSheet();
        }

        // Fail-closed: a stored string that does not decode reads as no dates (it is never repaired silently).
        private static ChosenDate[] LoadChosenDates()
        {
            ChosenDate[] dates;
            return ChosenDates.TryDecode(PlayerPrefs.GetString(ChosenDates.ListKey, ""), out dates) ? dates : new ChosenDate[0];
        }

        private static void SaveChosenDates(ChosenDate[] dates)
        {
            PlayerPrefs.SetString(ChosenDates.ListKey, ChosenDates.Encode(dates));
            PlayerPrefs.Save();
        }

        // Called once per hub visit, from NotePlayedToday. Shows at most one neighbour's mark per day.
        private void ShowChosenDateMark()
        {
            if (titleScreen == null) return;
            int today = LocalDayStamp();
            if (PlayerPrefs.GetInt(ChosenDates.ShownKey, 0) == today) return;
            string line = ChosenDates.MarkFor(today, LoadChosenDates());
            if (line == null) return;
            PlayerPrefs.SetInt(ChosenDates.ShownKey, today);
            PlayerPrefs.Save();
            RuleToast.Show((RectTransform)titleScreen.transform, line, font, Ink, MosseloraTheme.CardFill);
        }

        private void ClampDraftDay()
        {
            if (draftDay > DaysIn(draftMonth)) draftDay = DaysIn(draftMonth);
        }

        // Feb has 29 days here: a Feb 29 date is a real date, marked on Feb 28 in a year without one.
        private static int DaysIn(int month)
        {
            return DateTime.DaysInMonth(2000, month);
        }

        private static string MonthName(int month)
        {
            return CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(month);
        }

        private static string KindWords(ChosenDateKind kind)
        {
            switch (kind)
            {
                case ChosenDateKind.Birthday: return "Birthday";
                case ChosenDateKind.Anniversary: return "Anniversary";
                default: return "Another day";
            }
        }

        private static ChosenDateKind NextKind(ChosenDateKind kind)
        {
            switch (kind)
            {
                case ChosenDateKind.Birthday: return ChosenDateKind.Anniversary;
                case ChosenDateKind.Anniversary: return ChosenDateKind.Other;
                default: return ChosenDateKind.Birthday;
            }
        }

        private static string DateWords(ChosenDate date)
        {
            string when = date.Day + " " + MonthName(date.Month);
            string kind = KindWords(date.Kind);
            return date.Label.Length > 0 ? kind + ", " + when + ": " + date.Label : kind + ", " + when;
        }
    }
}

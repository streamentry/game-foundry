using System;
using System.Collections.Generic;
using System.IO;
using Evergrove.Core;

/// <summary>
/// Smart companion SC06: locales. A locale with no bank keeps LocalizedVillage exactly as before
/// (the companion is silent); a reviewed bank speaks only its own words, never English sentences,
/// and only with slots that need no translation.
/// </summary>
internal static class CompanionLocaleTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Companion locale test failed: " + message);
    }

    public static int Run()
    {
        assertions = 0;
        NoBankMeansUnchanged();
        AReviewedBankSpeaks();
        return assertions;
    }

    // Every shipped language except English has no bank today: the companion stays silent there,
    // so NeighborSays falls through to LocalizedVillage.Speak, exactly as before SC01.
    private static void NoBankMeansUnchanged()
    {
        string root = Root();
        string locales = Path.Combine(root, "games/evergrove/Assets/Resources/Evergrove/Locales");
        string lines = Path.Combine(root, "games/evergrove/Assets/Resources/Evergrove/Lines");
        var world = CompanionFactsTests.Grown(9);
        world.SolveTree(0);
        var context = new CompanionContext(new DateTime(2026, 11, 27), weather: Weather.Rainy, lightOn: true, phase: DayPhase.Dusk);
        IReadOnlyList<Fact> facts = CompanionFacts.For(world, context);
        Expect(facts.Count > 0, "the garden has things to say in English");
        int checkedLocales = 0;
        foreach (string file in Directory.GetFiles(locales, "*.json"))
        {
            string locale = Path.GetFileNameWithoutExtension(file);
            if (locale == CompanionLocale.English) continue;
            string bankFile = Path.Combine(lines, locale + ".json");
            LineBank bank = File.Exists(bankFile) ? LineBank.Parse(File.ReadAllText(bankFile)) : LineBank.Empty;
            if (!bank.IsEmpty) continue; // a reviewed bank: covered below
            Expect(!CompanionLocale.Speaks(locale, bank), locale + ": no bank, the companion is silent");
            foreach (Fact f in facts)
                for (int v = 0; v < 3; v++)
                {
                    Expect(!CompanionLocale.CanSay(locale, bank, f, v), locale + ": nothing to say in " + f);
                    Expect(CompanionLocale.Words(locale, bank, f, v, world.Garden, context.DayNumber, null, -1, out int variant) == null && variant == -1,
                        locale + ": never an English sentence");
                }
            Expect(LocalizedVillage.Speak(world, 0) != null, locale + ": the story's line is still there");
            checkedLocales++;
        }
        Expect(checkedLocales >= 29, "every non-English language checked: " + checkedLocales);
        Expect(CompanionLocale.Speaks("en", LineBank.Empty), "English always speaks (today's sentences)");
    }

    private static void AReviewedBankSpeaks()
    {
        LineBank fr = LineBank.Parse("{" +
            "\"FruitGiven\": {\"0\": [\"Merci pour les fruits.\"]}," +
            "\"LetterWaiting\": {\"1\": [\"Une lettre de {name} vous attend.\"]}," +
            "\"NewestThing\": {\"0\": [\"Votre {thing} {where}.\"]}," +
            "\"WeatherToday.Rainy\": {\"2\": [\"Il pleut.\"]}}");
        Expect(CompanionLocale.Speaks("fr", fr), "a reviewed bank speaks");
        var none = new Dictionary<string, string>();
        Fact fruit = new Fact(FactKind.FruitGiven, 0, "fruit:0:1", none, 15, false, false, 0);
        Fact letter = new Fact(FactKind.LetterWaiting, 1, "letter:Anh:1", new Dictionary<string, string> { { "name", "Anh" } }, 50, true, false, 0);
        Fact newest = new Fact(FactKind.NewestThing, 0, "newest:3:1", new Dictionary<string, string> { { "thing", "quince tree" }, { "where", "by the creek" } }, 0, false, false, 0);
        Fact rain = new Fact(FactKind.WeatherToday, -1, "weather:Rainy", new Dictionary<string, string> { { "weather", "Rainy" } }, 5, false, false, 0);
        Fact snow = new Fact(FactKind.WeatherToday, -1, "weather:Snowy", new Dictionary<string, string> { { "weather", "Snowy" } }, 5, false, false, 0);
        Expect(CompanionLocale.Words("fr", fr, fruit, 0, 1, 1, null, -1, out _) == "Merci pour les fruits.", "French words for fruit");
        Expect(CompanionLocale.Words("fr", fr, letter, 1, 1, 1, null, -1, out _) == "Une lettre de Anh vous attend.", "a typed name needs no translation");
        Expect(!CompanionLocale.CanSay("fr", fr, newest, 0) && CompanionLocale.Words("fr", fr, newest, 0, 1, 1, null, -1, out _) == null,
            "an English item name would be wrong in French: not said");
        Expect(CompanionLocale.CanSay("fr", fr, rain, 2) && !CompanionLocale.CanSay("fr", fr, snow, 2), "only what the bank has");
        Expect(!CompanionLocale.CanSay("fr", fr, fruit, 1), "only the voices the bank has");
        Expect(CompanionLocale.Words("en", LineBank.Empty, fruit, 0, 1, 1, null, -1, out int v) == NeighbourNotice.Thanked(0) && v == -1,
            "English without a bank: today's sentence");
        Expect(CompanionLocale.IsLocalized("name") && !CompanionLocale.IsLocalized("thing"), "which slots travel");
    }

    private static string Root()
    {
        string dir = AppContext.BaseDirectory;
        while (dir != null && !File.Exists(Path.Combine(dir, "games/evergrove/docs/specs/companion-line-rules.json")))
            dir = Path.GetDirectoryName(dir);
        if (dir == null) throw new Exception("repository root not found");
        return dir;
    }
}

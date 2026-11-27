using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Evergrove.Core;

/// <summary>
/// Checklist 8, no sign-up wall: family features are an offer from day 3 (or at once with family activity),
/// every family entry point is gated, no first-run path touches a network or an account, and a day-1 solo
/// garden never needs any family state.
/// </summary>
internal static class NoSignupWallTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("NoSignupWallTests failed: " + message);
    }

    public static int Run()
    {
        OfferRules();
        ActivityRules();
        FamilyEntryPointsGated();
        NoNetworkOrAccountOnFirstRun();
        DayOneSoloNeedsNoFamily();
        return assertions;
    }

    // ---- Core: when the offer is visible.

    private static void OfferRules()
    {
        Expect(!FamilyOffer.Visible(0, false), "day 0 with no family activity shows no family features");
        Expect(!FamilyOffer.Visible(1, false), "day 1 with no family activity shows no family features");
        Expect(!FamilyOffer.Visible(2, false), "day 2 with no family activity shows no family features");
        Expect(FamilyOffer.Visible(3, false), "day 3 shows the family offer");
        Expect(FamilyOffer.Visible(40, false), "later days keep the offer");
        Expect(FamilyOffer.Visible(0, true) && FamilyOffer.Visible(1, true), "family activity shows the offer at once");
        Expect(FamilyOffer.Line.Contains("It's never needed to play."), "the offer says family is never needed to play");
        Expect(FamilyOffer.OfferedKey == "evergrove:family-offer-shown", "the once-flag key is stable");
    }

    // ---- Core: what counts as family activity, read through the public API.

    private static void ActivityRules()
    {
        WorldModel fresh = WorldModel.New(PlayMode.Living);
        Expect(!FamilyOffer.HasActivity(fresh), "a fresh garden has no family activity");
        Expect(!FamilyOffer.HasActivity(WorldModel.New(PlayMode.PuzzleOnly)), "a puzzle-only garden has no family activity");

        Expect(fresh.KeepNote("Mira", "The river is running well this morning."), "a neighbour note is kept");
        Expect(!FamilyOffer.HasActivity(fresh), "a neighbour's note is not family activity");
        Expect(!FamilyOffer.Visible(1, FamilyOffer.HasActivity(fresh)), "a neighbour's note does not open family on day 1");

        WorldModel sender = WorldModel.New(PlayMode.Living);
        ParcelSendResult sent = sender.SendParcel("Nan", "Hello from the coast.", ParcelGift.None, 0, "nan-1");
        Expect(sent.Ok && sent.Text != null, "a letter parcel is made offline (" + sent.Error + ")");

        WorldModel receiver = WorldModel.New(PlayMode.Living);
        Expect(receiver.Open(sent.Text).Ok, "a pasted family parcel opens offline");
        Expect(FamilyOffer.HasActivity(receiver), "a family letter is family activity");
        Expect(FamilyOffer.Visible(1, FamilyOffer.HasActivity(receiver)), "a garden with a family letter sees family on day 1");
    }

    // ---- Source audit: every family entry point is gated by FamilyOffer.Visible through FamilyShown().

    private static void FamilyEntryPointsGated()
    {
        string root = FindGameRoot();
        string app = UnityAppSource.ReadAll();

        // The gate itself.
        Expect(app.Contains("private bool FamilyShown()"), "EvergroveApp defines FamilyShown");
        Expect(app.Contains("FamilyOffer.Visible(world.DaysPlayed, FamilyOffer.HasActivity(world))"),
            "FamilyShown reads FamilyOffer.Visible with days played and family activity");

        // 1. The family sheet opens only through OpenFamily, which is gated. Canary: the sheet is created once.
        int openDef = app.IndexOf("private void OpenFamily()", StringComparison.Ordinal);
        Expect(openDef >= 0, "OpenFamily exists");
        int openGuard = app.IndexOf("if (!FamilyShown()) return;", openDef, StringComparison.Ordinal);
        int sheetOpen = app.IndexOf("familySheet = FamilySheet.Open(", openDef, StringComparison.Ordinal);
        Expect(openGuard > openDef && sheetOpen > openGuard, "OpenFamily checks FamilyShown before it opens the sheet");
        Expect(CountOf(app, "FamilySheet.Open(") == 1, "the family sheet is opened from one place only");

        // 2. Fan menu "Family" token (title hub, living gardens).
        Expect(Gated(app, "tokens.Add(FanToken(column.transform, \"Family\"", "if (living && FamilyShown())"),
            "fan menu Family token is gated");

        // 3. Journal tab "Family in the garden".
        Expect(Gated(app, "IconButton(tabs, \"Family in the garden\"", "if (FamilyShown())"),
            "journal family tab is gated");

        // 4. Send a solved board to family (the board's own card).
        Expect(Gated(app, "card.SendBoard = () => SendBoardToFamily(", "world.Mode == PlayMode.Living && FamilyShown())"),
            "send-board-to-family is gated");

        // 5. Memory shelf: help-from-family and lantern keepsakes are removed before the shelf is laid out.
        Expect(app.Contains("if (!FamilyShown()) shelf = System.Array.FindAll(shelf, k => !FamilyOffer.IsFamilyKeepsake(k.Art));"),
            "memory shelf hides family keepsakes until family is offered");

        // 6. Hub offer: once, on this device, when it first becomes visible.
        Expect(app.Contains("OfferFamilyOnce();"), "hub calls the one-time family offer");
        Expect(app.Contains("PlayerPrefs.GetInt(FamilyOffer.OfferedKey, 0) == 1") &&
               app.Contains("RuleToast.Show((RectTransform)titleScreen.transform, FamilyOffer.Line"),
            "the family offer shows once, as a gentle line on the hub");

        // Every remaining OpenFamily caller sits behind the sheet gate: callers are the fan token,
        // the letter write-back, the board send fallback and the dev tour. None opens family on its own.
        Expect(CountOf(app, "OpenFamily();") >= 4, "OpenFamily callers are the known family entry points");
    }

    // ---- Source audit: no network, sign-in or account on any non-dev Unity path.

    private static readonly string[] BannedOnFirstRun =
    {
        "UnityWebRequest", "WorldClient", "HttpClient", "WebSocket",
        "sign in", "sign-in", "signin", "account", "login", "log in",
    };

    private static void NoNetworkOrAccountOnFirstRun()
    {
        string unity = Path.Combine(FindGameRoot(), "Assets/Scripts/Unity");
        int scanned = 0;
        foreach (string file in Directory.GetFiles(unity, "*.cs", SearchOption.AllDirectories))
        {
            string name = Path.GetFileName(file);
            if (name == "WorldClient.cs" || name.StartsWith("Dev", StringComparison.Ordinal)) continue;
            if (file.Contains(Path.DirectorySeparatorChar + "Dev" + Path.DirectorySeparatorChar)) continue;
            string code = ShippingCode(File.ReadAllText(file));
            scanned++;
            foreach (string word in BannedOnFirstRun)
                Expect(code.IndexOf(word, StringComparison.OrdinalIgnoreCase) < 0,
                    name + " mentions '" + word + "' on a non-dev path");
        }
        Expect(scanned > 20, "the network audit scanned the Unity sources (" + scanned + " files)");
    }

    // Keeps only code that ships: drops development-only #if regions and // comments.
    private static string ShippingCode(string text)
    {
        var stack = new List<int[]>(); // [0] = 1 for a development-only branch, [1] = 1 inside #else/#elif
        var sb = new StringBuilder();
        foreach (string raw in text.Replace("\r", "").Split('\n'))
        {
            string line = raw.Trim();
            if (line.StartsWith("#if", StringComparison.Ordinal))
            {
                bool dev = (line.Contains("DEVELOPMENT_BUILD") || line.Contains("UNITY_EDITOR")) &&
                           !line.Contains("!") && !line.Contains("UNITY_IOS") && !line.Contains("UNITY_ANDROID");
                stack.Add(new[] { dev ? 1 : 0, 0 });
                continue;
            }
            if (line.StartsWith("#elif", StringComparison.Ordinal) || line.StartsWith("#else", StringComparison.Ordinal))
            {
                if (stack.Count > 0) stack[stack.Count - 1][1] = 1;
                continue;
            }
            if (line.StartsWith("#endif", StringComparison.Ordinal))
            {
                if (stack.Count > 0) stack.RemoveAt(stack.Count - 1);
                continue;
            }
            bool devRegion = stack.Exists(e => e[0] == 1 && e[1] == 0);
            if (devRegion) continue;
            sb.Append(Regex.Replace(raw, "//.*$", "")).Append('\n');
        }
        return sb.ToString();
    }

    // ---- Day 1, solo, offline: solve, choose, buy and plant, with no family state at all.

    private static void DayOneSoloNeedsNoFamily()
    {
        WorldModel garden = WorldModel.New(PlayMode.Living);
        Expect(garden.MarkPlayed(20261128), "day 1 is counted");
        Expect(!FamilyOffer.Visible(garden.DaysPlayed, FamilyOffer.HasActivity(garden)), "day 1 shows no family offer");

        for (int chapter = 0; chapter < 6; chapter++)
        {
            Puzzle board = garden.CurrentPuzzle;
            for (int i = 0; i < board.CellCount; i++)
                while (garden.Rotations[i] != board.Solution[i]) garden.Rotate(i);
            Expect(garden.Choose((VillageChoice)(chapter % 3)), "chapter " + (chapter + 1) + " is solved and its village choice is made");
        }
        for (int item = 0; item < 6; item++) garden.SolveTree(item);
        Expect(garden.Seeds >= Nursery.Price(0), "solo play earns enough seeds for the nursery");
        Expect(garden.Buy(0, 10), "a nursery plant is bought and planted with no family state");
        Expect(garden.PlantingCount == 0 && garden.BoughtCount == 1, "the bought plant sits in the garden");

        Expect(garden.Letters.Length == 0 && garden.Helped.Length == 0 && garden.Lanterns.Length == 0,
            "day 1 solo holds no family letters, help or lanterns");
        Expect(garden.Export().parcelIds.Length == 0, "day 1 solo has sent and opened no parcels");
        Expect(!FamilyOffer.HasActivity(garden), "day 1 solo has no family activity");

        Expect(garden.MarkPlayed(20261129), "day 2 is counted");
        Expect(!FamilyOffer.Visible(garden.DaysPlayed, FamilyOffer.HasActivity(garden)), "day 2 shows no family offer");
        Expect(garden.MarkPlayed(20261130), "day 3 is counted");
        Expect(FamilyOffer.Visible(garden.DaysPlayed, FamilyOffer.HasActivity(garden)), "day 3 shows the family offer");
    }

    // ---- Helpers.

    // True when the gate text appears in the 200 characters just before the entry point's first use.
    private static bool Gated(string source, string token, string gate)
    {
        int at = source.IndexOf(token, StringComparison.Ordinal);
        if (at < 0) return false;
        int from = Math.Max(0, at - 200);
        return source.Substring(from, at - from).Contains(gate);
    }

    private static int CountOf(string source, string token)
    {
        int count = 0, at = 0;
        while ((at = source.IndexOf(token, at, StringComparison.Ordinal)) >= 0) { count++; at += token.Length; }
        return count;
    }

    private static string Read(string root, string relative)
    {
        return File.ReadAllText(Path.Combine(root, relative));
    }

    // Walks up from the working directory and the test binary until it finds the Evergrove project.
    private static string FindGameRoot()
    {
        foreach (string start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            string dir = Path.GetFullPath(start);
            while (dir != null)
            {
                string game = Path.Combine(dir, "games", "evergrove");
                if (File.Exists(Path.Combine(game, "Assets", "Scripts", "Unity", "EvergroveApp.cs"))) return game;
                if (File.Exists(Path.Combine(dir, "Assets", "Scripts", "Unity", "EvergroveApp.cs"))) return dir;
                dir = Directory.GetParent(dir) == null ? null : Directory.GetParent(dir).FullName;
            }
        }
        throw new Exception("NoSignupWallTests failed: the Evergrove Unity sources were not found");
    }
}

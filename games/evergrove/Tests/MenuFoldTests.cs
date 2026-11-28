using System;
using System.IO;

// Checklist 11, one menu button: the hub at rest shows at most two controls (the menu button and the
// play token). The letterbox, the basket and the seed book live in the corner fan; the seed count is a
// read-only label. The count itself is taken in the running tour (DevQaTour.Rest.cs); these audits pin
// the wiring that makes it true.
internal static class MenuFoldTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("MenuFoldTests failed: " + message);
    }

    public static int Run()
    {
        assertions = 0;
        FanHoldsTheMovedControls();
        FanTokensAreRoundAndSized();
        BasketOpensFromTheFanOnly();
        LetterboxIsNoLongerAHomeButton();
        LetterArrivalFliesIntoTheMenuButton();
        UnreadLetterMarksTheMenuButton();
        SeedPurseIsReadOnly();
        RestingBasketControlIsGone();
        HubAtRestHasOneCornerButton();
        TourCountsRestingControls();
        IdleFadeNoLongerNamesTheLetterbox();
        RegisteredInTheSuite();
        return assertions;
    }

    private static void FanHoldsTheMovedControls()
    {
        string menu = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Menu.cs");
        int look = menu.IndexOf("\"Just look at the garden\"", StringComparison.Ordinal);
        int letters = menu.IndexOf("FanToken(column.transform, \"Letters\"", StringComparison.Ordinal);
        int basket = menu.IndexOf("FanToken(column.transform, \"Basket\"", StringComparison.Ordinal);
        int seedBook = menu.IndexOf("FanToken(column.transform, \"Seed book\"", StringComparison.Ordinal);
        int settings = menu.IndexOf("FanToken(column.transform, \"Settings\"", StringComparison.Ordinal);
        int arrange = menu.IndexOf("tokens = ArrangeFan(tokens);", StringComparison.Ordinal);
        Expect(letters > look && basket > letters && seedBook > basket && settings > seedBook,
            "the fan adds Letters, Basket and Seed book after the existing tokens and before Settings");
        Expect(arrange > settings, "the new tokens are built before ArrangeFan orders the fan");
        Expect(menu.Contains("if (living && titleScreen != null && world.Letters.Length > 0)"),
            "the Letters token exists only in a living garden with a letter to open");
        Expect(menu.Contains("if (memories && titleScreen != null)"),
            "the Basket and Seed book tokens need a living garden with something grown, as the purse and basket did");
        Expect(menu.Contains("\"letterbox\""), "the Letters token wears the painted letterbox art");
        Expect(menu.Contains("\"food-basket\"") && menu.Contains("TreePlay.SeedIcon"),
            "the Basket and Seed book tokens use the basket and seed art already on home");
    }

    private static void FanTokensAreRoundAndSized()
    {
        string chrome = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Chrome.cs");
        Expect(chrome.Contains("IconButton(parent, name, caption, icon, () => { RememberFanToken(name); action(); }, 64,"),
            "every fan token is a 64 pt round token");
        Expect(chrome.Contains("size.minHeight = size.preferredHeight = 64f;"), "fan tokens keep their 64 pt height in the layout");
    }

    private static void BasketOpensFromTheFanOnly()
    {
        string menu = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Menu.cs");
        string map = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.MapActions.cs");
        int basket = menu.IndexOf("FanToken(column.transform, \"Basket\"", StringComparison.Ordinal);
        string tokenAction = menu.Substring(basket, Math.Min(400, menu.Length - basket));
        Expect(tokenAction.Contains("CloseFan();") && tokenAction.Contains("basketOpen = true;") && tokenAction.Contains("RefreshMaps();"),
            "the Basket token closes the fan, opens the tray and refreshes home");
        Expect(map.Contains("if (!basketOpen && heldPlanting < 0) return;"),
            "the basket corner is built only while the tray is open or a planting is held");
        Expect(map.Contains("\"Close basket\"") && map.Contains("}, 72, MosseloraTheme.ForestAction,"),
            "the open tray keeps a 72 pt close control");
        Expect(map.Contains("private void BasketFanCue(Button basket)") && menu.Contains("BasketFanCue(basket.GetComponent<Button>());"),
            "the basket's newest-thing peek and gift ribbon moved onto the fan's Basket token");
    }

    private static void LetterboxIsNoLongerAHomeButton()
    {
        string letters = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Letters.cs");
        Expect(!letters.Contains("Node(\"Letterbox\""), "home builds no Letterbox node");
        Expect(!letters.Contains("LetterBox()") && !letters.Contains("letterboxRect"), "LetterBox and letterboxRect are gone");
        Expect(!letters.Contains("button.onClick.AddListener(() => ShowLetter(0));"), "no letterbox tap is wired");
        string hub = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Hub.cs");
        Expect(!hub.Contains("LetterBox();"), "the hub no longer calls LetterBox");
        Expect(ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Menu.cs").Contains("ShowLetter(0);"),
            "the fan's Letters token opens the newest letter");
    }

    private static void LetterArrivalFliesIntoTheMenuButton()
    {
        string letters = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Letters.cs");
        string hub = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Hub.cs");
        Expect(letters.Contains("FlyInto(menu.GetComponent<RectTransform>(), \"letter-sealed\""),
            "a letter that arrives flies its sealed sprite into the menu button");
        Expect(!letters.Contains("FlyInto(rect, \"letter-sealed\""), "no arrival flies into a letterbox rect any more");
        Expect(hub.Contains("MenuLetterCue(menu);"), "HubChrome hands the menu button to the letter cue");
        Expect(letters.Contains("if (letterArrived)") && letters.Contains("Haptics.Arrived();"),
            "the arrival keeps its haptic");
    }

    private static void UnreadLetterMarksTheMenuButton()
    {
        string letters = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Letters.cs");
        Expect(letters.Contains("if (PlayerPrefs.GetInt(LettersReadKey, 0) < letters.Length)") &&
               letters.Contains("\"Has a new letter\""),
            "while any letter is unread, the menu button wears a dot");
        Expect(letters.Contains("new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(18f, 18f)"),
            "the dot sits at the menu button's top right, 18 pt");
    }

    private static void SeedPurseIsReadOnly()
    {
        string notes = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.GardenNotes.cs");
        string economy = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Economy.cs");
        Expect(!notes.Contains("SeedPurseTap") && !economy.Contains("SeedPurseTap"), "SeedPurseTap is removed");
        Expect(notes.Contains("body.raycastTarget = false;") && notes.Contains("count.raycastTarget = false;"),
            "the purse and its count take no raycasts");
        Expect(notes.Contains("SeedPurseSpeech(purse);"), "the purse keeps its spoken count");
        int speech = economy.IndexOf("private void SeedPurseSpeech(", StringComparison.Ordinal);
        int sheet = economy.IndexOf("internal void OpenSeedBook()", StringComparison.Ordinal);
        Expect(speech > 0 && sheet > speech, "SeedPurseSpeech sits before the seed book");
        string speechBody = economy.Substring(speech, sheet - speech);
        Expect(!speechBody.Contains("AddComponent<Button>") && !speechBody.Contains("LongPressCaption") && !speechBody.Contains("OpenSeedBook"),
            "the purse's spoken label adds no button, no hold caption and no seed book tap");
        Expect(speechBody.Contains("\"Seeds you have, \" + world.Seeds + \".\""), "the spoken name still gives the count");
    }

    private static void RestingBasketControlIsGone()
    {
        string map = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.MapActions.cs");
        Expect(!map.Contains("\"Plant more\"") && !map.Contains("Plant more"), "no resting Plant more toggle");
        Expect(!map.Contains("if (!basketOpen && !holding)"), "the closed-basket branch moved to the fan token");
    }

    private static void HubAtRestHasOneCornerButton()
    {
        string hub = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Hub.cs");
        int start = hub.IndexOf("private void HubChrome()", StringComparison.Ordinal);
        int end = hub.IndexOf("// \"Rainwater\"", start, StringComparison.Ordinal);
        Expect(start > 0 && end > start, "HubChrome is found");
        string chrome = hub.Substring(start, end - start);
        Expect(CountOf(chrome, "IconButton(") == 1 && chrome.Contains("\"Open menu\""),
            "the hub's chrome builds exactly one button: the menu");
        Expect(hub.Contains("if (world.Mode == PlayMode.Living) BasketControls(titleScreen.transform, stack.transform);"),
            "the basket corner is only asked for; it builds nothing while closed");
    }

    private static void TourCountsRestingControls()
    {
        string rest = ReadEvergrove("Assets/Scripts/Unity/DevQaTour.Rest.cs");
        Expect(rest.StartsWith("#if UNITY_EDITOR || DEVELOPMENT_BUILD"), "the rest tour is dev-only");
        Expect(rest.Contains("public sealed partial class DevQaTour") && rest.Contains("private IEnumerator RestShots()"),
            "RestShots is a DevQaTour partial");
        Expect(rest.Contains("app.QaShowHub(true);") && rest.Contains("WaitForSecondsRealtime(0.5f)"), "the hub is shown and settled first");
        Expect(rest.Contains("Debug.Log(\"MOSSELORA_REST controls \""), "the count is logged as MOSSELORA_REST controls");
        Expect(rest.Contains("\"Title screen\"") && rest.Contains("\"Village window\"") && rest.Contains("\"Your village\""),
            "the count is taken under the hub and leaves out the garden's map and neighbours");
        Expect(rest.Contains("\"Day token\"") && rest.Contains("\"Dew\"") && rest.Contains("\"Lantern\""),
            "the dew and lantern tokens are garden content, not controls");
        Expect(rest.Contains("SavePng(\"26-hub-at-rest.png\")") && rest.Contains("SavePng(\"26-fan-folded.png\")"),
            "the tour saves the hub at rest and the fan");
        string main = ReadEvergrove("Assets/Scripts/Unity/DevQaTour.cs");
        Expect(main.Contains("yield return RestShots(); // checklist 11 (DevQaTour.Rest.cs)"), "the tour runs RestShots");
        Expect(main.Split('\n').Length - 1 <= 1177, "DevQaTour.cs stays within its 1177-line budget");
    }

    private static void IdleFadeNoLongerNamesTheLetterbox()
    {
        string idle = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Idle.cs");
        Expect(idle.Contains("{ \"Hub chrome\", \"Seeds you have\", \"Basket corner\" }"), "idle fade names the chrome that still exists");
        Expect(!idle.Contains("\"Letterbox\""), "idle fade no longer names the letterbox");
    }

    private static void RegisteredInTheSuite()
    {
        Expect(ReadEvergrove("Tests/Program.cs").Contains("MenuFoldTests.Run()"), "MenuFoldTests is registered");
    }

    private static int CountOf(string text, string part)
    {
        int count = 0, at = 0;
        while ((at = text.IndexOf(part, at, StringComparison.Ordinal)) >= 0) { count++; at += part.Length; }
        return count;
    }

    private static string ReadEvergrove(string relative)
    {
        string dir = AppContext.BaseDirectory;
        while (dir != null && !File.Exists(Path.Combine(dir, relative)))
            dir = Path.GetDirectoryName(dir);
        if (dir == null) throw new Exception(relative + " not found");
        return File.ReadAllText(Path.Combine(dir, relative));
    }
}

using System;
using Evergrove.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    public sealed partial class EvergroveApp
    {
        // ---- Items 74 and 79: the stall (sell fruit and jam, trade with neighbours) and the
        // deeper weekly boards (item 35). Sheets follow the seed book pattern: built fresh on each
        // open or change, a dimmed veil, a parchment panel, and a close button in the header.
        private GameObject marketSheet;
        private GameObject weeklySheet;
        private string marketNote = ""; // the last sale or trade's gentle line, shown until the sheet closes

        private static readonly string[] VillagerNames = { "Mira", "Orin", "Fen" }; // as in Core GiveFruit
        private static readonly string[] SeasonWords = { "spring", "summer", "autumn", "winter" };

        // ---- The stall

        // The menu's two entries for the stall (v1 #74, #79) and this week's deeper boards (v1 #35).
        // Called once from the menu build, after the postcard; Living gardens only.
        private void BuildStallEntries()
        {
            if (world == null || world.Mode != PlayMode.Living) return;
            Button(content, "Open the stall", "The stall", () => { SetMenu(false); OpenMarketSheet(); });
            bool anyWeekly = false;
            foreach (PuzzleKind kind in (PuzzleKind[])Enum.GetValues(typeof(PuzzleKind)))
                if (WeeklyBoardOffered(kind)) { anyWeekly = true; break; }
            if (anyWeekly)
                Button(content, "Open deeper boards", "This week's deeper boards", () => { SetMenu(false); OpenWeeklyBoardSheet(); });
        }

        internal void OpenMarketSheet()
        {
            if (titleScreen == null || world == null || world.Mode != PlayMode.Living) return;
            marketNote = "";
            BuildMarketSheet();
        }

        internal void CloseMarketSheet() { RetireMarket(false); }

        private void RetireMarket(bool replacing)
        {
            RetireSheet(marketSheet, "The stall", replacing);
            marketSheet = null;
            semanticsDirty = true;
        }

        // Rebuilt after every sale or trade, so the counts and the day's allowance are current.
        private void BuildMarketSheet()
        {
            if (titleScreen == null || world == null) return;
            bool replacing = marketSheet != null;
            RetireMarket(replacing);
            marketSheet = BuildSheet(titleScreen.transform, "The stall", CloseMarketSheet,
                out RectTransform content, out ScrollRect scroll);
            SheetClose(content, "Close the stall", CloseMarketSheet);
            Text title = Label(content, "Stall title", "The stall", 26, 0, Ink, TextAnchor.MiddleCenter);
            Emphasize(title);

            int day = LocalDayStamp();
            Season season = GardenSeason.For(world);
            string seasonWord = SeasonWords[(int)season];

            Text fruitLine = Label(content, "Stall fruit", "Fruit: " + world.Fruit + " in the basket. Sells for " +
                Market.FruitPrice(season) + " seeds each this " + seasonWord + ".", 18, 0, Ink, TextAnchor.MiddleLeft);
            fruitLine.GetComponent<LayoutElement>().flexibleWidth = 1f;
            Label(content, "Stall fruit allowance", "You can sell " + FruitLeftToday(day) + " more fruit today.",
                16, 0, Ink, TextAnchor.MiddleLeft).GetComponent<LayoutElement>().flexibleWidth = 1f;
            Button(content, "Sell one fruit", "Sell one fruit", () => SellAtStall(Good.Fruit), 52, true);

            Text jamLine = Label(content, "Stall jam", "Jam: " + world.Jam + " jars. Sells for " +
                Market.JamPrice(season) + " seeds each this " + seasonWord + ".", 18, 0, Ink, TextAnchor.MiddleLeft);
            jamLine.GetComponent<LayoutElement>().flexibleWidth = 1f;
            Label(content, "Stall jam allowance", "You can sell " + JamLeftToday(day) + " more jars today.",
                16, 0, Ink, TextAnchor.MiddleLeft).GetComponent<LayoutElement>().flexibleWidth = 1f;
            Button(content, "Sell one jar of jam", "Sell one jar", () => SellAtStall(Good.Jam), 52, true);

            // Item 75: the kitchen. Each recipe is cooked from goods the garden holds; the first cook finds it.
            Label(content, "Kitchen title", "Kitchen", 20, 0, Ink, TextAnchor.MiddleLeft);
            for (int id = 0; id < Recipes.Count; id++)
            {
                int recipe = id;
                string name = Recipes.Name(recipe);
                Text method = Label(content, "Recipe " + name, name + ": " + Recipes.Method(recipe), 18, 0, Ink, TextAnchor.MiddleLeft);
                method.GetComponent<LayoutElement>().flexibleWidth = 1f;
                if (world.CanCookRecipe(recipe))
                    Button(content, "Cook " + name, "Cook " + name.ToLowerInvariant(), () => CookAtStall(recipe), 52, false);
            }

            // Item 76: the larder. Shelves are bought here with seeds; each one adds room for fruit and jam.
            Label(content, "Larder title", "Larder", 20, 0, Ink, TextAnchor.MiddleLeft);
            Text larder = Label(content, "Larder line", "Shelves: " + world.LarderShelves + " of " + WorldModel.MaxShelves +
                ". Holds " + world.FruitCapacity + " fruit, " + world.JamCapacity + " jars.", 18, 0, Ink, TextAnchor.MiddleLeft);
            larder.GetComponent<LayoutElement>().flexibleWidth = 1f;
            int shelfPrice = world.NextShelfPrice;
            if (shelfPrice > 0)
            {
                string shelfLine = world.Seeds >= shelfPrice
                    ? "Add a shelf (" + shelfPrice + " seeds)"
                    : "The next shelf costs " + shelfPrice + " seeds.";
                if (world.Seeds >= shelfPrice)
                    Button(content, "Add a shelf", shelfLine, BuyShelfAtStall, 52, false);
                else
                    Label(content, "Next shelf price", shelfLine, 16, 0, Ink, TextAnchor.MiddleLeft).GetComponent<LayoutElement>().flexibleWidth = 1f;
            }

            Label(content, "Stall trades title", "Neighbours", 20, 0, Ink, TextAnchor.MiddleLeft);
            for (int i = 0; i < Market.TradeCount; i++)
            {
                int index = i;
                NeighbourTrade trade = Market.TradeAt(index);
                string villager = VillagerNames[trade.Villager];
                Text offer = Label(content, "Trade offer " + index, villager + ": " + GoodPhrase(trade.GiveAmount, trade.Give) +
                    " for " + GoodPhrase(trade.GetAmount, trade.Get) + ".", 18, 0, Ink, TextAnchor.MiddleLeft);
                offer.GetComponent<LayoutElement>().flexibleWidth = 1f;
                Button(content, "Trade with " + villager, "Trade with " + villager, () => TradeAtStall(index), 52, false);
            }

            if (marketNote.Length > 0)
            {
                Text note = Label(content, "Stall note", marketNote, 18, 0, Ink, TextAnchor.MiddleLeft);
                note.GetComponent<LayoutElement>().flexibleWidth = 1f;
            }

            SheetMotion.Show(marketSheet, SheetEdge.For("The stall"), ReducedMotion, null, replacing);
            marketSheet.transform.SetAsLastSibling(); // over the garden
            FitSheet(marketSheet, "The stall panel");
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            FitSheetToPages(marketSheet, content);
            if (scroll != null) scroll.verticalNormalizedPosition = 1f;
            semanticsDirty = true;
        }

        // Cooks one recipe. The first cook of a recipe says so; a refusal changes nothing.
        private void CookAtStall(int recipe)
        {
            bool firstTime = !world.HasFound(recipe);
            if (world.CookRecipe(recipe))
            {
                SaveWorld();
                PlayChime();
                marketNote = firstTime ? "New recipe found: " + Recipes.Name(recipe) + ". It is in the almanac."
                    : "Cooked " + Recipes.Name(recipe).ToLowerInvariant() + ".";
            }
            else
            {
                marketNote = "There is not enough fruit for that yet, or the pantry is full.";
            }
            BuildMarketSheet();
        }

        // Buys the next larder shelf. A refusal changes nothing and says why.
        private void BuyShelfAtStall()
        {
            if (world.BuyShelf())
            {
                SaveWorld();
                PlayChime();
                marketNote = "A new shelf is up in the larder.";
            }
            else if (world.Mode != PlayMode.Living) marketNote = "The stall is not open in this garden yet.";
            else if (world.LarderShelves >= WorldModel.MaxShelves) marketNote = "The larder has all its shelves.";
            else marketNote = "Not enough seeds for a shelf yet.";
            BuildMarketSheet();
        }

        private int FruitLeftToday(int day)
        {
            return Math.Max(0, Market.MaxFruitSoldPerDay - world.FruitSoldOn(day));
        }

        private int JamLeftToday(int day)
        {
            return Math.Max(0, Market.MaxJamSoldPerDay - world.JamSoldOn(day));
        }

        // Sells one unit to the stall. A success pays seeds; a refusal changes nothing and says why.
        private void SellAtStall(Good good)
        {
            int day = LocalDayStamp();
            Season season = GardenSeason.For(world);
            int earned = good == Good.Fruit ? world.SellFruit(1, season, day) : world.SellJam(1, season, day);
            if (earned > 0)
            {
                SaveWorld();
                PlayChime();
                marketNote = "Sold for " + earned + " seeds.";
            }
            else
            {
                marketNote = StallRefusal(good, day, season);
            }
            BuildMarketSheet();
        }

        // Maps each refusal case in WorldModel.SellFruit / SellJam to a gentle line. Checked in the same
        // order as the Core guard, so the line matches the reason the sale was refused.
        private string StallRefusal(Good good, int day, Season season)
        {
            bool fruit = good == Good.Fruit;
            if (world.Mode != PlayMode.Living) return "The stall is not open in this garden yet.";
            if ((fruit ? FruitLeftToday(day) : JamLeftToday(day)) < 1)
                return fruit ? "The stall has sold enough fruit for today." : "The stall has sold enough jam for today.";
            if ((fruit ? world.Fruit : world.Jam) < 1) return "Nothing to sell yet.";
            int earned = fruit ? Market.FruitPrice(season) : Market.JamPrice(season);
            if ((long)world.Seeds + earned > WorldModel.MaxSeeds) return "The purse is full for now.";
            return "The stall could not make that sale.";
        }

        // Makes one neighbour trade. A refusal changes nothing; the line says which rule stopped it.
        private void TradeAtStall(int index)
        {
            int day = LocalDayStamp();
            NeighbourTrade trade = Market.TradeAt(index);
            if (world.Trade(index, day))
            {
                SaveWorld();
                PlayChime();
                marketNote = "Traded with " + VillagerNames[trade.Villager] + ". Thank you.";
            }
            else
            {
                marketNote = TradeRefusal(trade, day);
            }
            BuildMarketSheet();
        }

        // The same order as WorldModel.Trade: the day's allowance, then the goods, then the room to receive.
        private string TradeRefusal(NeighbourTrade trade, int day)
        {
            if (world.Mode != PlayMode.Living) return "The stall is not open in this garden yet.";
            if (world.TradesOn(day) >= Market.MaxTradesPerDay) return "The neighbours have traded enough for today.";
            int held = trade.Give == Good.Fruit ? world.Fruit : trade.Give == Good.Jam ? world.Jam : world.Seeds;
            if (held < trade.GiveAmount) return "Not enough to trade yet.";
            return "There is no room for that yet.";
        }

        // Plain words for an amount of a good: "2 fruit", "1 jar of jam", "4 seeds".
        private static string GoodPhrase(int amount, Good good)
        {
            if (good == Good.Fruit) return amount + " fruit";
            if (good == Good.Jam) return amount == 1 ? "1 jar of jam" : amount + " jars of jam";
            return amount + " seeds";
        }

        // ---- Item 35: the deeper weekly boards

        // Offered only in a Living garden, for a kind with a proven single-solution weekly board,
        // once the garden has any first solve (there is no per-kind solve record to check).
        internal bool WeeklyBoardOffered(PuzzleKind kind)
        {
            return world != null && world.Mode == PlayMode.Living && WeeklyBoard.HasWeekly(kind) && world.PuzzlesSolved > 0;
        }

        // Opens this week's board of a kind. It pays nothing and changes nothing, so the solve callback only returns 0.
        internal void OpenWeeklyBoard(PuzzleKind kind)
        {
            if (titleScreen == null || !WeeklyBoardOffered(kind)) return;
            int seed = WeeklyBoard.SeedFor(world.Garden, kind, WeeklyBoard.IsoWeekOf(LocalDayStamp()));
            CloseWeeklyBoardSheet();
            OpenThingPuzzle(seed, WeeklyBoard.Tier, kind, MosseloraArt.Load("build-05"), "This week's board", 0,
                Mathf.Abs(seed % 97), () => 0, ownBoard: false);
        }

        // Lists the offered kinds, one button each, under the header that explains the board.
        internal void OpenWeeklyBoardSheet()
        {
            if (titleScreen == null || world == null || world.Mode != PlayMode.Living) return;
            CloseWeeklyBoardSheet();
            weeklySheet = BuildSheet(titleScreen.transform, "Deeper boards", CloseWeeklyBoardSheet,
                out RectTransform content, out ScrollRect scroll);
            SheetClose(content, "Close deeper boards", CloseWeeklyBoardSheet);
            Text title = Label(content, "Deeper boards title", "Deeper boards", 26, 0, Ink, TextAnchor.MiddleCenter);
            Emphasize(title);
            Text header = Label(content, "Deeper boards header", "This week's deeper board. No timer, and it pays nothing.",
                18, 0, Ink, TextAnchor.MiddleLeft);
            header.GetComponent<LayoutElement>().flexibleWidth = 1f;

            bool any = false;
            foreach (PuzzleKind kind in Enum.GetValues(typeof(PuzzleKind)))
            {
                if (!WeeklyBoardOffered(kind)) continue;
                any = true;
                string name = WeeklyKindName(kind);
                Button(content, "Deeper " + name, name, () => OpenWeeklyBoard(kind), 52, true);
            }
            if (!any)
            {
                Text none = Label(content, "Deeper boards none", "No deeper boards yet. Solve a puzzle first and they will appear here.",
                    18, 0, Ink, TextAnchor.MiddleLeft);
                none.GetComponent<LayoutElement>().flexibleWidth = 1f;
            }

            SheetMotion.Show(weeklySheet, SheetEdge.For("Deeper boards"), ReducedMotion);
            weeklySheet.transform.SetAsLastSibling();
            FitSheet(weeklySheet, "Deeper boards panel");
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            FitSheetToPages(weeklySheet, content);
            if (scroll != null) scroll.verticalNormalizedPosition = 1f;
            semanticsDirty = true;
        }

        internal void CloseWeeklyBoardSheet()
        {
            RetireSheet(weeklySheet, "Deeper boards", false);
            weeklySheet = null;
            semanticsDirty = true;
        }

        private static string WeeklyKindName(PuzzleKind kind)
        {
            switch (kind)
            {
                case PuzzleKind.Water: return "Water";
                case PuzzleKind.Sudoku: return "Sudoku";
                case PuzzleKind.Plots: return "Plots";
                case PuzzleKind.StreamPath: return "Stream";
                case PuzzleKind.Sums: return "Sums";
                case PuzzleKind.Doorstep: return "Doorstep";
                default: return kind.ToString();
            }
        }
    }
}

using System;
using System.IO;
using Evergrove.Core;

// Checklist 91 (founder, 2026-11-28): dense boards zoom so every live cell is at least 48 pt, and
// sideways the puzzle card splits into board and a full-size control panel. Pure rules here; the wiring
// in TreePlay's partials, the tour's zoom shots and the reachable-only scan by source audit.
internal static class BoardZoomTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("BoardZoomTests failed: " + message);
    }

    public static int Run()
    {
        assertions = 0;
        NeededOnlyUnderTheMinimum();
        ScaleBringsEveryCellToTheMinimum();
        OffsetCentresTheTapAndStaysOnTheBoard();
        ZoomIsWiredIntoTheCard();
        SidewaysSplitsTheCard();
        LandscapeNeverShrinksControls();
        TourMeasuresBothStates();
        ScanCountsOnlyReachableTargets();
        return assertions;
    }

    private static void NeededOnlyUnderTheMinimum()
    {
        Expect(BoardZoomRule.Needed(44f) && BoardZoomRule.Needed(18f), "cells under 48 pt need the zoom");
        Expect(!BoardZoomRule.Needed(48f) && !BoardZoomRule.Needed(60f), "cells of 48 pt or more do not");
        Expect(!BoardZoomRule.Needed(0f) && !BoardZoomRule.Needed(-3f) && !BoardZoomRule.Needed(float.NaN), "no real size, no zoom");
    }

    private static void ScaleBringsEveryCellToTheMinimum()
    {
        for (float cell = 12f; cell < 48f; cell += 0.5f)
        {
            float scale = BoardZoomRule.Scale(cell);
            Expect(scale >= 1f && scale <= BoardZoomRule.MaxScale, "scale within 1.." + BoardZoomRule.MaxScale + " for " + cell);
            Expect(cell * scale >= BoardZoomRule.MinTouch || scale == BoardZoomRule.MaxScale,
                cell + " pt zoomed is at least 48 pt (or the most the zoom gives)");
        }
        // The real boards measured in the batch 23 scan all reach the minimum.
        foreach (float measured in new[] { 44f, 42f, 47f, 18f, 21f, 23f, 15f, 25f, 26f, 27f })
            Expect(measured * BoardZoomRule.Scale(measured) >= 48f, measured + " pt (measured) reaches 48 pt zoomed");
        Expect(BoardZoomRule.Scale(60f) == 1f, "no zoom when none is needed");
    }

    private static void OffsetCentresTheTapAndStaysOnTheBoard()
    {
        const float size = 300f, scale = 2f;
        Expect(Math.Abs(BoardZoomRule.Offset(0f, size, scale)) < 0.001f, "a tap at the centre keeps the board centred");
        Expect(Math.Abs(BoardZoomRule.Offset(50f, size, scale) + 100f) < 0.001f, "a tap 50 pt right moves the board 100 pt left");
        float limit = size * (scale - 1f) / 2f;
        Expect(Math.Abs(BoardZoomRule.Offset(150f, size, scale) + limit) < 0.001f &&
               Math.Abs(BoardZoomRule.Offset(-150f, size, scale) - limit) < 0.001f, "a tap at the edge never shows past the board");
        Expect(BoardZoomRule.Offset(40f, size, 1f) == 0f && BoardZoomRule.Offset(40f, 0f, 2f) == 0f &&
               BoardZoomRule.Offset(float.NaN, size, 2f) == 0f, "no zoom or bad input: no offset");
    }

    private static void ZoomIsWiredIntoTheCard()
    {
        string tree = Read("Assets/Scripts/Unity/TreePlay.cs");
        string zoom = Read("Assets/Scripts/Unity/TreePlay.Zoom.cs");
        Expect(tree.Contains("internal sealed partial class TreePlay"), "TreePlay is partial, so its new parts live in their own files");
        Expect(tree.Contains("InstallZoom(); // v1 #91") && tree.Contains("solved = true; ZoomDone();"),
            "the zoom installs once the card settles and lets go when the board is solved");
        Expect(zoom.Contains("BoardZoomRule.Needed(smallest)") && zoom.Contains("BoardZoomRule.Scale(smallest)"), "the zoom follows the pure rule");
        Expect(zoom.Contains("zoomCells.interactable = zoomCells.blocksRaycasts = false;") &&
               zoom.Contains("zoomCells.interactable = zoomCells.blocksRaycasts = true;"),
            "whole, the cells take no taps (the picture does); zoomed, they do");
        Expect(zoom.Contains("\"See the whole board\"") && zoom.Contains("52f"), "a 52 pt token zooms back out");
        Expect(zoom.Contains("view.AddComponent<RectMask2D>()") && zoom.Contains("zoomPan.movementType = ScrollRect.MovementType.Clamped"),
            "the zoomed board is clipped to its place and pans within it");
    }

    private static void SidewaysSplitsTheCard()
    {
        string tree = Read("Assets/Scripts/Unity/TreePlay.cs");
        string side = Read("Assets/Scripts/Unity/TreePlay.Sideways.cs");
        string chain = Read("Assets/Scripts/Unity/ChainCause.cs");
        Expect(tree.Contains("SplitSideways(root); // v1 #91"), "the card splits once settled");
        Expect(tree.Contains("float width = Sideways(root) ? SidewaysBoardWidth(root) :") &&
               tree.Contains("if (Sideways(root)) screenHeight += TopRowSpace;"), "sideways, the board is built for the room it really has");
        Expect(side.Contains("\"Card controls\"") && side.Contains("\"Board side\"") && side.Contains("SidePanelWidth = 260f"),
            "board left, a 260 pt control panel right");
        Expect(side.Contains("name == \"Tree puzzle top\"") && side.Contains("lower.Contains(\"tray\")") && side.Contains("\"Undo and redo\""),
            "the top row, trays and undo go to the panel");
        Expect(chain.Contains("TreePlay.ControlsOf(card)"), "the chain line, added at the solve, joins the panel too");
    }

    // Batch 28 tour findings, fixed: the zoom view is as big as the cells really are (the pieces board
    // places cells outside its container), the sowing board's seven beds count as a board, the sideways
    // panel hands its lowest controls to the board side rather than shrinking, and the family sheet
    // scrolls its body instead of scaling its 52-56 pt controls down.
    private static void LandscapeNeverShrinksControls()
    {
        string zoom = Read("Assets/Scripts/Unity/TreePlay.Zoom.cs");
        string side = Read("Assets/Scripts/Unity/TreePlay.Sideways.cs");
        string family = Read("Assets/Scripts/Unity/FamilySheet.cs");
        Expect(zoom.Contains("RectTransformUtility.CalculateRelativeRectTransformBounds(grid)") &&
               zoom.Contains("zoomBoard.anchoredPosition = -zoomCentre;"), "the zoom view frames the cells' real bounds");
        Expect(zoom.Contains("int most = 4;"), "five or more live cells make a board");
        Expect(zoom.Contains("zoomView.anchoredPosition = grid.anchoredPosition;") && zoom.Contains("zoomView.sizeDelta = grid.sizeDelta;") &&
               zoom.Contains("LayoutRebuilder.ForceRebuildLayoutImmediate(grid);"),
            "the view takes the grid's own place (beds have no layout group) and measures cells after they are laid out");
        Expect(side.Contains("Balance((RectTransform)boardSide, (RectTransform)panel,") &&
               side.Contains("if (part == null || part.name == \"Tree puzzle top\") continue;"),
            "the panel's lowest controls move under the board when the panel is too tall; the top row stays");
        Expect(family.Contains("\"Family scroll\"") && family.Contains("bodyScroll.vertical = true;") &&
               !family.Contains("Mathf.Max(0.75f, room / tall)"), "the family sheet scrolls; it no longer shrinks to 75%");
    }

    private static void TourMeasuresBothStates()
    {
        string tour = Read("Assets/Scripts/Unity/DevQaTour.Zoom.cs");
        Expect(tour.Contains("PuzzleKind.Doorstep") && tour.Contains("PuzzleKind.Sudoku") && tour.Contains("PuzzleKind.Nonogram"),
            "the densest boards are toured");
        Expect(tour.Contains("-whole.png") && tour.Contains("-in.png") && tour.Contains("MOSSELORA_ZOOM"), "whole and zoomed shots, logged");
        Expect(Read("Assets/Scripts/Unity/DevQaTour.cs").Contains("yield return ZoomShots();"), "the tour runs the zoom shots");
    }

    private static void ScanCountsOnlyReachableTargets()
    {
        string scan = Read("Assets/Scripts/Unity/DevQaTour.Touch.cs");
        Expect(scan.Contains("if (!Reachable(small)) { touchCovered++; continue; }") && scan.Contains("events.RaycastAll(probe, hits);"),
            "a target counts only if a tap at its centre reaches it");
        Expect(scan.Contains("\" covered=\" + touchCovered"), "covered targets are still counted in the report");
    }

    private static string Read(string relative)
    {
        string dir = AppContext.BaseDirectory;
        while (dir != null && !File.Exists(Path.Combine(dir, relative)))
            dir = Path.GetDirectoryName(dir);
        if (dir == null) throw new Exception(relative + " not found");
        return File.ReadAllText(Path.Combine(dir, relative));
    }
}

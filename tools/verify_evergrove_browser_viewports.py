#!/usr/bin/env python3
"""Offline Chromium viewport and first-interaction QA for the browser RESEARCH slice.

This intentionally loads the checked-in HTML/CSS/JS as strings, so no server,
external assets or network calls are necessary. It cannot certify the Unity
player, Safari/VoiceOver, Android TalkBack, perceived beauty or human delight.
"""
from pathlib import Path

from playwright.sync_api import sync_playwright

ROOT = Path(__file__).resolve().parents[1]
SLICE = ROOT / "games" / "evergrove" / "research" / "first-minute"
OUT = ROOT / "artifacts" / "evergrove-first-minute"
VIEWPORTS = ((320, 568), (375, 667), (430, 932))


def require(ok: bool, detail: str) -> None:
    if not ok:
        raise AssertionError(detail)


def run() -> None:
    html = (SLICE / "index.html").read_text(encoding="utf-8")
    style = (SLICE / "style.css").read_text(encoding="utf-8")
    code = (SLICE / "game.js").read_text(encoding="utf-8")
    # set_content() is about:blank. Strip only two relative asset tags,
    # then inject the *exact* files separately; never fetch a remote copy.
    html = html.replace('<link rel="stylesheet" href="style.css">', "")
    html = html.replace('<script src="game.js" defer></script>', "")
    OUT.mkdir(parents=True, exist_ok=True)

    with sync_playwright() as playwright:
        browser = playwright.chromium.launch(headless=True)
        try:
            for width, height in VIEWPORTS:
                context = browser.new_context(
                    viewport={"width": width, "height": height},
                    is_mobile=True, has_touch=True, device_scale_factor=2,
                    reduced_motion="reduce",
                )
                page = context.new_page()
                errors = []
                page.on("pageerror", lambda error: errors.append(str(error)))
                try:
                    page.set_content(html, wait_until="domcontentloaded")
                    page.add_style_tag(content=style)
                    page.add_script_tag(content=code)
                    page.wait_for_function(
                        "() => document.querySelectorAll('#board button').length === 9"
                    )
                    first = page.locator("#board button").first
                    rect = first.bounding_box()
                    require(rect is not None, "Missing first tile")
                    require(rect["width"] >= 68 and rect["height"] >= 68,
                            f"{width}x{height}: first tile below minimum size: {rect}")
                    require(
                        rect["y"] + min(64, rect["height"] * 0.7) <= height,
                        f"{width}x{height}: first tile is below initial fold: {rect}",
                    )
                    require(
                        page.evaluate(
                            "() => document.documentElement.scrollWidth <= window.innerWidth + 1"
                        ),
                        f"{width}x{height}: horizontal overflow",
                    )
                    require(page.evaluate("() => window.scrollY") < 1,
                            "Initial render moved the page without player input")
                    require(page.locator("#water-count").inner_text().startswith("4 of 9"),
                            "Initial fixture is not the real First Bend board")
                    require(page.locator(".brand").inner_text().endswith("MOSSELORA"),
                            "Mosselora branding is absent from the playable page")
                    require(page.locator(".tile-alert").count() > 0 and
                            page.locator(".tile-alert").first.inner_text() == "○",
                            "Loose ends need a gentle visible shape, not red alarm punctuation")
                    require(page.locator("#world-status").inner_text() == "A PLACE WAITING FOR WATER",
                            "New garden must not claim already restored water")
                    page.screenshot(path=str(OUT / f"{width}x{height}-first.png"))

                    page.locator("#board button").nth(6).click()
                    require(page.locator("#water-count").inner_text().startswith("5 of 9"),
                            "First clockwise move did not connect the next pipe")
                    page.locator("#undo").click()
                    require(page.locator("#water-count").inner_text().startswith("4 of 9"),
                            "Undo did not retract the water")

                    # A genuine restart must remain in copied participant notes.
                    page.locator("#restart").click()
                    for index, turns in ((4, 1), (5, 1), (6, 2), (8, 2)):
                        for _ in range(turns):
                            page.locator("#board button").nth(index).click()
                    page.wait_for_function(
                        "() => document.querySelector('#water-count').textContent.startsWith('9 of 9')"
                    )
                    require(page.locator("#choices").is_visible(),
                            "Solved board did not reveal choice buttons")
                    require(page.locator("#world-status").inner_text() == "THE STREAM HAS RETURNED",
                            "Completed creek still has a stale 'waiting for water' caption")
                    page.wait_for_function(
                        "() => document.activeElement && document.activeElement.id === 'world'"
                    )
                    page.screenshot(path=str(OUT / f"{width}x{height}-restored.png"))

                    page.locator('[data-choice="tree"]').click()
                    require(page.locator("#world-tree").is_visible(),
                            "Tree consequence not shown after player choice")
                    require(page.locator("#world-status").inner_text() ==
                            "A PLACE CHANGED BY YOUR CHOICE",
                            "Choice art has a stale world caption")
                    require("shade tree" in
                            (page.locator("#creek").get_attribute("aria-label") or ""),
                            "Accessible illustration did not describe the new tree")
                    require("Mira" in page.locator("#memory-text").inner_text(),
                            "Missing an authored consequence memory")
                    require(page.locator("#restart").is_disabled(),
                            "Accidental reset still enabled after memory beat")
                    require(page.evaluate(
                        "() => document.activeElement && document.activeElement.id === 'world'"
                    ), "Focus did not return to the changed village")
                    page.screenshot(path=str(OUT / f"{width}x{height}-choice.png"))

                    # Force the no-clipboard branch in this isolated test
                    # to prove notes stay inspectable without permissions.
                    page.evaluate(
                        "() => Object.defineProperty(navigator, 'clipboard', "
                        "{ configurable: true, value: undefined })"
                    )
                    page.locator("#copy-notes").click()
                    page.wait_for_function(
                        "() => !document.querySelector('#notes-fallback').hidden"
                    )
                    report = page.locator("#notes-fallback").input_value()
                    require("restarts=1" in report, "Reset attempt lost in notes")
                    require("rotations_all_attempts=7" in report,
                            "Failed first attempt missing from notes")
                    require("undos_all_attempts=1" in report,
                            "Undo missing from notes")
                    require(not errors, f"{width}x{height}: browser script errors: {errors}")
                    print(f"PASS: {width}x{height} first tile, touch, Undo, "
                          "full creek, choice/focus, no script errors")
                except Exception:
                    page.screenshot(
                        path=str(OUT / f"{width}x{height}-failure.png"),
                        full_page=True,
                    )
                    raise
                finally:
                    context.close()
        finally:
            browser.close()


if __name__ == "__main__":
    run()

/* Tests the independent browser slice against the exact Unity chapter-one
 * fixture and game rules. No DOM or device screenshots are implied.
 */
const test = require("node:test");
const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");
const { FIXTURE, CHOICES, inspect, Game, hasPort, neighbor } = require("./game.js");

function solve(game) {
  for (let i = 0; i < 9; i++) {
    let safety = 0;
    while (game.rotations[i] !== FIXTURE.solution[i]) {
      assert.equal(game.rotate(i), true, "legal rotation on unsolved fixture");
      assert.ok(++safety <= 4, "one tile must solve within a full rotation cycle");
    }
  }
}
test("one original Unity fixture, never invented web puzzle content", () => {
  const unity = fs.readFileSync(path.join(__dirname, "../../Assets/Scripts/Core/PipeRules.cs"), "utf8");
  const match = unity.match(/new Puzzle\("First Bend", "([EIL]+)",\s*new\[\]\s*\{([^}]+)\},\s*new\[\]\s*\{([^}]+)\}\)/);
  assert.ok(match, "the original Unity chapter-one authoring format changed");
  const array = s => s.split(",").map(x => Number(x.trim()));
  assert.equal(match[1], FIXTURE.layout);
  assert.deepEqual(array(match[2]), FIXTURE.initial);
  assert.deepEqual(array(match[3]), FIXTURE.solution);
  assert.equal(FIXTURE.layout.length, 9);
});
test("pipes use the same four clockwise directions as Unity", () => {
  assert.equal(hasPort("E", 1, 0), true);
  assert.equal(hasPort("E", 4, 3), true);
  assert.equal(hasPort("E", 2, 3), false);
  assert.equal(hasPort("I", 1, 0), true);
  assert.equal(hasPort("I", 1, 2), true);
  assert.equal(hasPort("I", 2, 1), true);
  assert.equal(hasPort("I", 2, 3), true);
  assert.equal(hasPort("L", 1, 0), true);
  assert.equal(hasPort("L", 1, 1), true);
  assert.equal(hasPort("L", 4, 3), true);
  assert.equal(hasPort("L", 4, 0), true);
  assert.equal(neighbor(2, 1), -1);
  assert.equal(neighbor(2, 2), 5);
  assert.equal(neighbor(4, 0), 1);
  assert.equal(neighbor(4, 3), 3);
});
test("spring-water reach is honest: first tap advances, undo retracts", () => {
  const game = new Game();
  assert.deepEqual(game.rotations, FIXTURE.initial);
  assert.equal(game.insight.connectedCount, 4);
  assert.equal(game.insight.solved, false);
  assert.equal(game.insight.creekReach, 3 / 8);
  assert.equal(game.rotate(6), true);
  assert.equal(game.insight.connectedCount, 5, "one clockwise tile rotation reaches another pipe");
  assert.equal(game.insight.creekReach, 4 / 8);
  assert.equal(game.undo(), true);
  assert.equal(game.insight.connectedCount, 4);
  assert.equal(game.undo(), false, "one-step undo only");
  assert.deepEqual(game.rotations, FIXTURE.initial);
});
test("full leak-free route is the only way to restore the creek", () => {
  const game = new Game();
  assert.equal(game.choose("tree"), false, "not possible before the real solution");
  solve(game);
  assert.deepEqual(game.rotations, FIXTURE.solution);
  assert.equal(game.insight.connectedCount, 9);
  assert.equal(game.insight.leakCount, 0);
  assert.equal(game.insight.creekReach, 1);
  assert.equal(game.insight.solved, true);
  assert.equal(game.rotate(0), false, "no accidental changes after completion");
  assert.equal(game.choose("tree"), true);
  assert.equal(game.choice, "tree");
  assert.equal(game.choose("bridge"), false, "one decision, not multiple outcomes");
  game.reset();
  assert.deepEqual(game.rotations, FIXTURE.initial);
  assert.equal(game.choice, null);
});
test("all three consequences are real, mutually exclusive and choice-gated", () => {
  for (const choice of Object.keys(CHOICES)) {
    const game = new Game();
    assert.equal(game.choose(choice), false);
    solve(game);
    assert.equal(game.choose(choice), true);
    assert.ok(CHOICES[choice].result.trim().length >= 45, "choice has meaningful written consequence");
    assert.equal(game.choose(choice), false);
  }
  assert.equal(new Game().choose("fake"), false);
});
test("three-stage hints reveal a row, then tile, then align exactly one", () => {
  const game = new Game();
  assert.equal(game.hint(), 1);
  const target = game.hintTile;
  assert.ok(target >= 0 && target < 9);
  const before = game.rotations.slice();
  assert.deepEqual(game.rotations, before, "first hint does not alter puzzle");
  assert.equal(game.hint(), 2);
  assert.deepEqual(game.rotations, before, "second hint does not alter puzzle");
  assert.equal(game.hint(), 3);
  assert.equal(game.rotations[target], FIXTURE.solution[target]);
  assert.equal(game.undos, 0);
  assert.equal(game.undo(), true);
  assert.deepEqual(game.rotations, before, "help remains reversible");
  assert.equal(game.hints, 3);
});
test("invalid rotation states fail explicitly, rather than silently solving", () => {
  assert.throws(() => inspect([]));
  assert.throws(() => inspect([1, 2, 3, 4, 5, 6, 7, 8, 9]));
  assert.throws(() => inspect(FIXTURE.initial.map(String)));
  assert.equal(new Game().rotate(-1), false);
  assert.equal(new Game().rotate(9), false);
});
test("first playable board precedes decorative world in DOM and keyboard order", () => {
  const html = fs.readFileSync(path.join(__dirname, "index.html"), "utf8");
  const style = fs.readFileSync(path.join(__dirname, "style.css"), "utf8");
  const app = fs.readFileSync(path.join(__dirname, "game.js"), "utf8");
  const board = html.indexOf('id="puzzle-panel"');
  const world = html.indexOf('id="world"');
  const choices = html.indexOf('id="choices"');
  assert.ok(board >= 0 && board < world && world < choices,
    "a first mover must not need to scroll past a decorative hero");
  assert.match(html, /id="world"[^>]*tabindex="-1"/);
  assert.match(html, /id="world-status"/);
  assert.match(app, /node\("world-status"\)\.textContent = model\.choice/);
  assert.match(app, /node\("creek"\)\.setAttribute\("aria-label", sceneDescription\)/);
  assert.match(app, /if \(becameSolved\) requestAnimationFrame/);
  assert.match(app, /world\.scrollIntoView/);
  assert.match(app, /prefers-reduced-motion: reduce/);
  assert.match(app, /node\("restart"\)\.disabled = !!model\.choice/,
    "protect the memory state from an accidental reset");
  assert.match(app, /toggleAttribute\("hidden", model\.choice !== key\)/,
    "SVG world changes must remove the hidden attribute, not set an expando");
  assert.match(style, /max-width: 430px\) and \(max-height: 680px/);
});

test("facilitator notes preserve attempts before restart and disclose that fact", () => {
  const app = fs.readFileSync(path.join(__dirname, "game.js"), "utf8");
  for (const text of [
    "totalRotations++", "totalHints++", "totalUndos++", "totalRestarts++",
    '"rotations_all_attempts="', '"hints_all_attempts="',
    '"undos_all_attempts="', '"restarts="',
  ]) assert.ok(app.includes(text), "missing truthful session accounting: " + text);
  assert.match(app, /Only explicit Replay begins a new measurement window/);
  assert.ok(app.indexOf("totalRestarts++") < app.indexOf('node("replay").addEventListener'),
    "Restart must not reset observation history as though the attempt succeeded");
});

test("research slice cannot collect or transmit private player data", () => {
  const script = fs.readFileSync(path.join(__dirname, "game.js"), "utf8");
  const html = fs.readFileSync(path.join(__dirname, "index.html"), "utf8");
  const css = fs.readFileSync(path.join(__dirname, "style.css"), "utf8");
  assert.doesNotMatch(script, /\bfetch\s*\(|XMLHttpRequest|sendBeacon|localStorage|sessionStorage|WebSocket/);
  assert.doesNotMatch(html, /https?:\/\/|<form\b/i, "no external assets or auto-upload forms");
  assert.match(html, /id="board"/);
  assert.match(html, /id="choices"/);
  assert.match(html, /id="memory"/);
  assert.match(html, /data-choice="tree"/);
  assert.match(html, /data-choice="bridge"/);
  assert.match(html, /data-choice="flowers"/);
  assert.match(html, /role="status" aria-live="polite"/);
  assert.match(css, /prefers-reduced-motion:\s*reduce/);
  assert.match(css, /:focus-visible/);
});

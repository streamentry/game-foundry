/* Mosselora first-minute research slice: exact Unity chapter-one fixture.
 * No framework, telemetry, account, persistence, external assets or network.
 * Browser prototype only. A real Unity/device QA pass is still mandatory.
 */
(function (root) {
  "use strict";
  const FIXTURE = Object.freeze({
    name: "First Bend",
    layout: "LIEIELLIL",
    initial: Object.freeze([2, 2, 4, 1, 1, 2, 3, 2, 2]),
    solution: Object.freeze([2, 2, 4, 1, 2, 3, 1, 2, 4]),
    size: 3
  });
  const DIRECTIONS = Object.freeze([[-1, 0], [0, 1], [1, 0], [0, -1]]);
  const CHOICES = Object.freeze({
    tree: Object.freeze({
      name: "Plant a shade tree",
      person: "Mira",
      result: "A young tree takes root by the cottage. Mira promises to watch over it."
    }),
    bridge: Object.freeze({
      name: "Build a crossing",
      person: "Orin",
      result: "A footbridge makes the two banks feel close. Orin can visit again."
    }),
    flowers: Object.freeze({
      name: "Grow a wildflower meadow",
      person: "Fen",
      result: "The first flowers find their place beside the creek. Fen leaves a little space for butterflies."
    })
  });
  function orientations(kind) { return kind === "I" ? 2 : 4; }
  function hasPort(kind, rotation, direction) {
    if (rotation < 1 || rotation > orientations(kind) || direction < 0 || direction > 3)
      return false;
    const turn = rotation - 1;
    if (kind === "E") return direction === turn;
    if (kind === "I") return turn === 0 ? direction === 0 || direction === 2
                                              : direction === 1 || direction === 3;
    return direction === turn || direction === (turn + 1) % 4;
  }
  function neighbor(tile, direction) {
    const row = Math.floor(tile / FIXTURE.size), column = tile % FIXTURE.size;
    const [dr, dc] = DIRECTIONS[direction];
    const nextRow = row + dr, nextColumn = column + dc;
    return nextRow >= 0 && nextRow < FIXTURE.size &&
           nextColumn >= 0 && nextColumn < FIXTURE.size
      ? nextRow * FIXTURE.size + nextColumn : -1;
  }
  function inspect(rotations) {
    if (!Array.isArray(rotations) || rotations.length !== FIXTURE.initial.length ||
      rotations.some((turn, i) => !Number.isInteger(turn) ||
        turn < 1 || turn > orientations(FIXTURE.layout[i])))
      throw new Error("Invalid first-stream rotations");
    const leaking = Array(9).fill(false), flowing = Array(9).fill(false);
    let leaks = 0;
    for (let i = 0; i < 9; i++) for (let d = 0; d < 4; d++) {
      const other = neighbor(i, d), here = hasPort(FIXTURE.layout[i], rotations[i], d);
      if (other < 0) {
        if (here) { leaks++; leaking[i] = true; }
      } else if (i < other) {
        const there = hasPort(FIXTURE.layout[other], rotations[other], (d + 2) % 4);
        if (here !== there) {
          leaks++;
          leaking[i] = true;
          leaking[other] = true;
        }
      }
    }
    const source = FIXTURE.layout.indexOf("E");
    const pending = [source];
    let count = 0;
    while (pending.length) {
      const i = pending.pop();
      if (flowing[i]) continue;
      flowing[i] = true;
      count++;
      for (let d = 0; d < 4; d++) {
        const other = neighbor(i, d);
        if (other >= 0 && !flowing[other] &&
          hasPort(FIXTURE.layout[i], rotations[i], d) &&
          hasPort(FIXTURE.layout[other], rotations[other], (d + 2) % 4))
          pending.push(other);
      }
    }
    return { flowing, leaking, connectedCount: count, leakCount: leaks,
      solved: count === 9 && leaks === 0,
      creekReach: Math.max(0, (count - 1) / 8) };
  }
  class Game {
    constructor() { this.reset(); }
    reset() {
      this.rotations = FIXTURE.initial.slice();
      this.previous = null;
      this.hintStep = 0;
      this.hintTile = -1;
      this.choice = null;
      this.turns = 0;
      this.hints = 0;
      this.undos = 0;
    }
    get insight() { return inspect(this.rotations); }
    rotate(index) {
      if (!Number.isInteger(index) || index < 0 || index >= 9 ||
          this.choice || this.insight.solved) return false;
      this.previous = this.rotations.slice();
      this.rotations[index] = this.rotations[index] % orientations(FIXTURE.layout[index]) + 1;
      this.hintStep = 0; this.hintTile = -1;
      this.turns++;
      return true;
    }
    undo() {
      if (!this.previous || this.choice) return false;
      this.rotations = this.previous;
      this.previous = null;
      this.hintStep = 0; this.hintTile = -1;
      this.undos++;
      return true;
    }
    target() {
      const flowing = this.insight.flowing;
      let fallback = -1;
      for (let i = 0; i < 9; i++) {
        if (this.rotations[i] === FIXTURE.solution[i]) continue;
        if (fallback === -1) fallback = i;
        if (flowing[i] || DIRECTIONS.some((_, dir) => {
          const j = neighbor(i, dir);
          return j >= 0 && flowing[j];
        })) return i;
      }
      return fallback;
    }
    hint() {
      if (this.insight.solved || this.choice) return 0;
      if (this.hintStep === 0) {
        this.hintTile = this.target();
        if (this.hintTile < 0) return 0;
        this.hintStep = 1; this.hints++; return 1;
      }
      if (this.hintStep === 1) {
        this.hintStep = 2; this.hints++; return 2;
      }
      if (this.hintTile < 0) return 0;
      this.previous = this.rotations.slice();
      this.rotations[this.hintTile] = FIXTURE.solution[this.hintTile];
      this.hintStep = 0; this.hintTile = -1;
      this.hints++; return 3;
    }
    choose(key) {
      if (!Object.prototype.hasOwnProperty.call(CHOICES, key) ||
          this.choice || !this.insight.solved) return false;
      this.choice = key;
      return true;
    }
  }
  const API = { FIXTURE, CHOICES, DIRECTIONS, orientations, hasPort, neighbor, inspect, Game };
  if (typeof module !== "undefined" && module.exports) module.exports = API;
  if (typeof document === "undefined") return;
  root.EvergrovePreview = API;
  const node = id => document.getElementById(id);
  const model = new Game();
  const board = node("board"), tileButtons = [];
  let origin = typeof performance !== "undefined" ? performance.now() : 0;
  let firstTurn = null, firstSolved = null;
  let previousSolved = false;
  // Never hide a failed attempt from formative notes just because somebody
  // restarted the board. Only explicit Replay begins a new measurement window.
  let totalRotations = 0, totalHints = 0, totalUndos = 0, totalRestarts = 0;

  function revealWorld() {
    const world = node("world");
    // The active tile becomes disabled on solve; give keyboard/screen-reader
    // focus a visible, descriptive landing point rather than a dead control.
    world.focus({ preventScroll: true });
    const reduceMotion = window.matchMedia &&
      window.matchMedia("(prefers-reduced-motion: reduce)").matches;
    world.scrollIntoView({ behavior: reduceMotion ? "auto" : "smooth", block: "start" });
  }

  function tileIcon(i) {
    const dirs = [];
    for (let d = 0; d < 4; d++)
      if (hasPort(FIXTURE.layout[i], model.rotations[i], d)) {
        const [dr, dc] = DIRECTIONS[d];
        dirs.push('<path d="M50 50 L' + (50 + 44 * dc) + ' ' +
          (50 + 44 * dr) + '" />');
      }
    return '<svg viewBox="0 0 100 100" aria-hidden="true" focusable="false">' +
      '<g fill="none" stroke="currentColor" stroke-width="18" stroke-linecap="round">' +
      dirs.join("") + '</g><circle cx="50" cy="50" r="10" fill="currentColor" /></svg>';
  }
  for (let i = 0; i < 9; i++) {
    const button = document.createElement("button");
    button.type = "button";
    button.className = "tile";
    button.addEventListener("click", () => {
      if (!model.rotate(i)) return;
      if (firstTurn === null) firstTurn = performance.now() - origin;
      totalRotations++;
      render();
    });
    board.appendChild(button);
    tileButtons.push(button);
  }
  function render() {
    const insight = model.insight;
    const solved = insight.solved;
    const becameSolved = solved && !previousSolved;
    previousSolved = solved;
    tileButtons.forEach((button, i) => {
      const isSource = i === 2, isGarden = i === 4;
      const label = isSource ? "spring" : isGarden ? "garden" : "pipe";
      const water = insight.flowing[i] ? "flowing" : "dry";
      button.className = "tile " + (insight.flowing[i] ? "tile--wet" : "tile--dry") +
        (insight.leaking[i] ? " tile--leak" : "");
      button.disabled = solved;
      button.setAttribute("aria-label", "Row " + (Math.floor(i / 3) + 1) +
        ", column " + (i % 3 + 1) + ", " + label + ", " + water +
        (insight.leaking[i] ? ", unmatched connection" : "") +
        (solved ? "" : ". Tap or press Enter to turn."));
      button.innerHTML = tileIcon(i) +
        (isSource ? '<span class="tile-mark" aria-hidden="true">≋</span>' :
         isGarden ? '<span class="tile-mark tile-mark--sprout" aria-hidden="true">✿</span>' : '') +
        (insight.leaking[i] ? '<span class="tile-alert" aria-hidden="true">○</span>' : '');
    });
    node("water-count").textContent = insight.connectedCount + " of 9";
    node("creek-front").style.strokeDashoffset = String(100 * (1 - insight.creekReach));
    node("undo").disabled = !model.previous || !!model.choice;
    node("hint").disabled = solved || !!model.choice;
    node("restart").disabled = !!model.choice; // use deliberate Replay after the memory beat
    node("hint").textContent = ["A gentle hint", "Show me the tile", "Align one pipe"][model.hintStep];
    const description = model.hintStep === 1 ? "Notice row " +
      (Math.floor(model.hintTile / 3) + 1) + "." :
      model.hintStep === 2 ? "Look at row " + (Math.floor(model.hintTile / 3) + 1) +
      ", column " + (model.hintTile % 3 + 1) + "." : "";
    node("hint-detail").textContent = description;
    node("message").textContent = model.choice ? "This place remembers your choice." :
      solved ? "The stream has found its way home." :
      insight.connectedCount === 9 ? "The water reaches every tile. Find the loose ends." :
      "Turn the channels to guide water from the spring to the garden.";
    // Do not tell the player a restored river is still 'waiting for water'.
    // Visible copy and the SVG's accessible name must reflect exactly the
    // world state shown. This is particularly important after keyboard focus
    // moves to the changed scene on completion/choice.
    const sceneLabels = {
      tree: "A shade tree now stands by the cottage beside the restored creek.",
      bridge: "A wooden footbridge now crosses the restored creek.",
      flowers: "Wildflowers and a butterfly now brighten the restored creek."
    };
    const sceneDescription = model.choice ? sceneLabels[model.choice] :
      solved ? "The village creek now flows fully past the cottage." :
      "A cottage beside a partly dry creek; the stream is not yet restored.";
    node("world-status").textContent = model.choice ?
      "A PLACE CHANGED BY YOUR CHOICE" :
      solved ? "THE STREAM HAS RETURNED" : "A PLACE WAITING FOR WATER";
    node("world").setAttribute("aria-label", sceneDescription);
    node("creek").setAttribute("aria-label", sceneDescription);
    node("choices").hidden = !solved || !!model.choice;
    node("memory").hidden = !model.choice;
    node("puzzle-panel").classList.toggle("puzzle--complete", solved);
    node("creek").classList.toggle("creek--restored", solved);
    ["tree", "bridge", "flowers"].forEach(key => {
      // SVG <g> is an SVGElement, NOT an HTMLElement: setting .hidden=false
      // only adds a JS expando on some browsers while the HTML hidden
      // ATTRIBUTE remains, leaving the chosen world change invisible.
      node("world-" + key).toggleAttribute("hidden", model.choice !== key);
    });
    if (model.choice) {
      const detail = CHOICES[model.choice];
      node("memory-title").textContent = detail.name + ".";
      node("memory-text").textContent = detail.result;
    }
    if (solved && firstSolved === null) firstSolved = performance.now() - origin;
    // The chapter is about restoring the creek, not staring at a solved grid.
    // Scroll only for genuine state transitions, never on initial page load.
    if (becameSolved) requestAnimationFrame(() => {
      if (model.insight.solved && !model.choice) revealWorld();
    });
  }
  node("undo").addEventListener("click", () => {
    if (model.undo()) { totalUndos++; render(); }
  });
  node("hint").addEventListener("click", () => {
    if (model.hint()) { totalHints++; render(); }
  });
  node("restart").addEventListener("click", () => {
    totalRestarts++;
    model.reset();
    render();
    node("puzzle-title").focus({ preventScroll: true });
  });
  document.querySelectorAll("[data-choice]").forEach(button => {
    button.addEventListener("click", () => {
      if (!model.choose(button.dataset.choice)) return;
      render();
      // Show what actually changed before asking somebody to read prose.
      revealWorld();
    });
  });
  node("replay").addEventListener("click", () => {
    model.reset();
    origin = performance.now();
    firstTurn = null; firstSolved = null;
    totalRotations = totalHints = totalUndos = totalRestarts = 0;
    node("notes-fallback").hidden = true;
    node("copy-notes").textContent = "Copy local session notes (optional)";
    render();
    node("puzzle-title").focus();
  });
  node("copy-notes").addEventListener("click", async () => {
    const report = [
      "MOSSELORA FIRST-MINUTE RESEARCH PROTOTYPE",
      "No identity collected. No automatic recording or upload.",
      "condition=storybook-first-stream",
      "board=First Bend (identical Unity 3x3 fixture)",
      "first_turn_seconds=" + (firstTurn === null ? "not_observed" : (firstTurn / 1000).toFixed(1)),
      "first_solve_seconds=" + (firstSolved === null ? "not_observed" : (firstSolved / 1000).toFixed(1)),
      "rotations_all_attempts=" + totalRotations,
      "hints_all_attempts=" + totalHints,
      "undos_all_attempts=" + totalUndos,
      "restarts=" + totalRestarts,
      "choice=" + (model.choice || "none"),
      "NOTE: Browser interaction is not Unity performance or mobile accessibility evidence."
    ].join("\n");
    try {
      if (!navigator.clipboard || !navigator.clipboard.writeText) throw new Error("clipboard unavailable");
      await navigator.clipboard.writeText(report);
      node("copy-notes").textContent = "Notes copied";
    } catch {
      node("notes-fallback").hidden = false;
      node("notes-fallback").value = report;
      node("notes-fallback").focus();
      node("notes-fallback").select();
    }
  });
  render();
})(typeof window === "undefined" ? globalThis : window);

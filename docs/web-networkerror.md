# Fixing a Godot Web playtest NetworkError

## What we verified

- Game Foundry exports complete stand-alone **Godot 4.7.2, single-threaded** Web builds in GitHub Actions. A successful build job proves that files were produced; it does **not** prove they were hosted.
- The GitHub repository currently has **GitHub Pages disabled**. Workflow artifacts are **downloadable ZIPs**, not URLs that serve the game.
- Godot Web requires HTTP(S) delivery of both `index.wasm` and `index.pck` alongside `index.html` and `index.js`. Browsers generally do not permit a `file://` HTML page to fetch adjacent binary files.
- In the revised export workflows, a `serve.py` launcher is bundled; build CI verifies file magic, sizes, SHA-256 when present, MIME types, HTTP response codes and *complete* delivery of the four assets. This is an HTTP-level test, **not an actual graphical browser test**.

## Run the downloaded build correctly

1. Open the **successful GitHub Actions run** for the game and download the appropriate `<game>-web-playtest` ZIP under **Artifacts** (not the repository's source-code ZIP).
2. Extract the ZIP **fully** into a normal writable directory. Verify that `index.html`, `index.js`, `index.wasm`, and `index.pck` are next to each other.
3. Open Terminal in this exact directory and run:

   ```bash
   python3 serve.py --check
   python3 serve.py --smoke
   python3 serve.py
   ```

   Older artifacts generated before this launcher was added may not contain `serve.py`. For those, run `python3 -m http.server 8000 --bind 127.0.0.1` instead.
4. Open **http://127.0.0.1:8000/** in the *same computer's* Chrome, Firefox, or another WebGL2-capable browser. Keep Terminal running until finished.

For another port, use `python3 serve.py --port 8010`. The URL on `127.0.0.1` cannot be opened on a friend's phone: it is local to one computer.

## What the error tells you

| Symptom / evidence | Likely issue | Next action |
| --- | --- | --- |
| Address starts with `file://` | Browser security blocks required `fetch()` calls | Start the local HTTP launcher above |
| Browser shows a GitHub Actions or ZIP URL, not the game's canvas | Artifact is not hosted | Extract and serve all build files |
| `http://127.0.0.1:8000/` refuses to connect | Server was not started, stopped or using another port | Inspect Terminal and try `--port 8010` |
| Network tab shows `index.wasm` or `index.pck` with **404** | Wrong directory or missing companion file | Run `python3 serve.py --check` and re-extract the ZIP |
| Network tab shows **blocked / CORS / mixed content** | Assets are loaded across origins, or an HTTP resource is included in HTTPS | Keep assets on the same origin over HTTPS; remove cross-domain redirects |
| Network tab shows **200** on the assets but application fails with a WebGL error | GPU, WebGL2, browser, device or runtime issue | Try a recent desktop Chromium/Firefox and inspect the **Console** tab |
| Browser intermittently shows an older build | Browser cache or stale service worker from another deployed game | Hard reload; clear site-specific cache/service worker if present |
| The bundled `--smoke` passes but browser still fails | The HTTP pathway works, **not** necessarily browser WebAssembly execution | Capture the first failing network request, browser version and console error |

Only **threaded** Godot Web exports require cross-origin isolation/SharedArrayBuffer headers. These are **single-threaded** builds; avoid adding COOP/COEP headers as a speculative fix for an unrelated `NetworkError`.

## Why new artifacts are better

Prior workflows checked that export files existed, but did not confirm that a browser-like HTTP client could GET them. All current Web workflows now call the same stdlib-only launcher in `--smoke` mode before uploading, checking actual bytes (not just a green "export" label). A real browser and phone QA pass is still required before release.

**If the error persists:** provide the exact page URL you opened and the first failed request or console line. That is enough to distinguish server/file delivery from WebGL/WASM runtime defects without guessing.

References: [Godot official Web export guidance](https://docs.godotengine.org/en/stable/tutorials/export/exporting_for_web.html), [playtest instructions](playtest.md).

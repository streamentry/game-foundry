# Mosselora website

Static Astro site deployed as a Cloudflare Worker Static Assets project. The source lives inside `games/evergrove/site`; Evergrove remains the internal Unity project name.

## Local use

Requires Node.js 22.23.4 or newer. `.nvmrc` selects 24.22.2.

```sh
npm install
npm run dev
npm run test
npm run build
npm run preview
npm run deploy:preview
```

`npm run test` exercises the existing one-puzzle browser research slice. That slice is clearly labeled as a research prototype, not the complete mobile game or a native build. The marketing pages contain no analytics, account flow, form submission, or external font dependency.

## Cloudflare Worker deployment

The static build goes to `dist/` and Wrangler deploys that directory as Worker assets. Node 24.22.3 built the site and its 10 local tests passed on 2026-11-24. Production Worker `mosselora-site` is live at `https://mosselora.com/` (version `f8c23470-8134-4aee-adbf-6a6a3411f2ea`); `/`, `/support/`, `/privacy/`, `/preview/` and the promotional key-art route returned HTTP 200. The support page lists `hi@mosselora.com`. The separate public preview Worker remains `https://mosselora-site-preview.4rqnf2gvxf.workers.dev` (version `300c2c2c-df33-400f-a655-f945e10a0362`).

The `deploy:preview` script uses `wrangler.preview.jsonc` and updates only `mosselora-site-preview.workers.dev`; it has no custom-domain route. The owner explicitly approved the production route on 2026-11-24. `wrangler.jsonc` maps `mosselora.com/*` to `mosselora-site`; deploy future production changes with `npm run deploy -- --strict`.

The separate Worker API is in `../backend/`. It is a limited `WATER_TREE` experiment, not cloud save for the mobile game. Keep it detached from game state until authentication, quota controls, privacy review, and player evidence pass their gates.

# Mosselora website

Static Astro site deployed as a Cloudflare Worker Static Assets project. The source lives inside `games/evergrove/site`; Evergrove remains the internal Unity project name.

## Local use

Requires Node.js 22.12 or newer.

```sh
npm install
npm run dev
npm run test
npm run build
npm run preview
```

`npm run test` exercises the existing one-puzzle browser research slice. That slice is clearly labeled as a research prototype, not the complete mobile game or a native build. The marketing pages contain no analytics, account flow, form submission, or external font dependency.

## Cloudflare Worker deployment

The static build goes to `dist/` and Wrangler deploys that directory as Worker assets. The target custom domain is `mosselora.com`; Wrangler requires an authenticated Cloudflare account with an active zone for the domain. Do not create or change DNS records manually if the zone is unavailable. `npm run deploy` publishes the static site and configures the custom domain from `wrangler.jsonc`.

The separate Worker API is in `../backend/`. It is a limited `WATER_TREE` experiment, not cloud save for the mobile game. Keep it detached from game state until authentication, quota controls, privacy review, and player evidence pass their gates.

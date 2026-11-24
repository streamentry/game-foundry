# Mosselora World API on Cloudflare Workers

**Runtime:** TypeScript on Cloudflare Workers with SQLite-backed Durable Objects. `Evergrove` is the internal source name; Mosselora is the public working title. The current API remains a **prototype** and is not connected to the game's save data or default boot scene.

This corrects the original in-memory, unauthenticated global-world sketch:
- SQLite-backed Durable Object storage persists between evictions.
- Each 32-byte random capability maps to its own isolated Durable Object using SHA-256, not `idFromName("global-evergrove")`.
- Only one demo action, `WATER_TREE`, is accepted with a UUID operation key, capped tree stage and optimistic revision. Each mutation is wrapped in a Durable Object storage transaction.
- Duplicate IDs are remembered for the last **256 accepted operations**; older retries may no longer be deduplicated (revision checks still apply).
- Requests are authenticated with `Authorization: Bearer <base64url32bytes>`. This is a *bearer-capability prototype*, **not a production identity system**.
- The Worker applies an approximate per-IP limit of 60 read/write units per minute at each Cloudflare location; writes consume five units. This is basic abuse resistance, not a global spending cap or user authentication.
- Exact `ALLOWED_ORIGIN` supports browser clients; CORS is not authentication. Native Unity clients normally send no Origin header.
- DELETE /api/world clears storage for the capability-controlled world. Recovery/deletion/compliance behavior requires further review.

### Local usage

Requires Node.js 22.16+ for the repository's configured scripts. Local test and development do not deploy or change Cloudflare resources:

```bash
cd games/evergrove/backend
npm install
npm run types
npm test
npm run dev
npm run deploy:dry
```

The Unity reference client is `Assets/Scripts/Unity/WorldClient.cs`, disabled by default. Do not attach it to the game or enter the Worker URL until player evidence, account security, privacy disclosures, quotas and the Cloudflare deployment are reviewed.

The deployed Worker is `mosselora-world-api` at `https://api.mosselora.com` (Cloudflare version `8be216ba-4032-4ec6-8c6c-4a9281fb1b27`, checked 2026-11-23). `/healthz` returned HTTP 200 with `status: prototype` and `gameplaySync: false`; unauthenticated `GET /api/world` returned HTTP 401 `CAPABILITY_REQUIRED`. The world endpoint below remains a limited tree-growth experiment. It does not sync puzzle progress, saved choices, chapters or village state and must not be described as production cloud save.

### API

- `GET /api/world` → `{ "status": "ok", "world": { "schemaVersion": 1, "revision": 0, "treeStage": 0, "lastWatered": 0 } }`
- `POST /api/world` JSON `{ "type": "WATER_TREE", "operationId": "<UUID>", "expectedRevision": 0 }` → `{ "status": "applied", "world": { ... } }`
- `409` for stale revision (latest state included); `422` invalid action or mature tree; `DELETE` clears the demo state.

**Critical scope boundary:** this API does **not** synchronize Mosselora's twelve authored puzzle chapters, NPC memory, local choice history or a multiplayer world. It accepts a client-generated bearer capability and has no account system or strict global quota. CORS is not authentication. Do not connect it to the public game or claim cloud-save support until authentication, abuse limits, cost controls, privacy disclosures and device tests are reviewed. PlayerPrefs is not secure secret storage. The production website and the game remain usable without this API.

References: [Durable Object Storage](https://developers.cloudflare.com/durable-objects/best-practices/access-durable-objects-storage/), [SQLite namespace migrations](https://developers.cloudflare.com/changelog/post/2026-07-09-restrict-new-kv-backed-namespaces/).

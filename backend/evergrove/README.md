# Cloudflare Workers + Durable Objects — optional Evergrove sync experiment

**Runtime:** TypeScript (Cloudflare Workers), while the **game client is Unity/C#**. Cloudflare Workers do not natively execute Unity C# code. This preview API **has not been deployed**.

This corrects the original in-memory, unauthenticated global-world sketch:
- SQLite-backed Durable Object storage persists between evictions.
- Each 32-byte random capability maps to its own isolated Durable Object using SHA-256, not `idFromName("global-evergrove")`.
- Only one demo action, `WATER_TREE`, is accepted with a UUID operation key, capped tree stage and optimistic revision. Each mutation is wrapped in a Durable Object storage transaction.
- Duplicate IDs are remembered for the last **256 accepted operations**; older retries may no longer be deduplicated (revision checks still apply).
- Requests are authenticated with `Authorization: Bearer <base64url32bytes>`. This is a *bearer-capability prototype*, **not a production identity system**.
- Exact `ALLOWED_ORIGIN` supports browser clients; CORS is not authentication. Native Unity clients normally send no Origin header.
- DELETE /api/world clears storage for the capability-controlled world. Recovery/deletion/compliance behavior requires further review.

### Local usage

Requires Node.js 22.16+ and a Cloudflare account for any deployment:

```bash
cd backend/evergrove
npm install
npm test
npx wrangler dev
```

The Unity client script is `games/evergrove/Assets/Scripts/Unity/WorldClient.cs`, disabled by default. Attach it to a GameObject and provide the HTTPS Worker URL **only after the offline experiment has validated the world layer**.

### API

- `GET /api/world` → `{ "status": "ok", "world": { "schemaVersion": 1, "revision": 0, "treeStage": 0, "lastWatered": 0 } }`
- `POST /api/world` JSON `{ "type": "WATER_TREE", "operationId": "<UUID>", "expectedRevision": 0 }` → `{ "status": "applied", "world": { ... } }`
- `409` for stale revision (latest state included); `422` invalid action or mature tree; `DELETE` clears the demo state.

**Critical scope boundary:** This API does **not** yet synchronize Evergrove's six puzzles, NPC memory, local choice history, or future multiuser world. It should not be connected automatically or described as a complete scalable game backend. Token storage in Unity PlayerPrefs is unsuitable for production secret security; use authenticated, recoverable accounts and OS-secure storage, plus rate limits, quota controls and abuse prevention before public release. No live Cloudflare account is modified by this branch.

References: [Durable Object Storage](https://developers.cloudflare.com/durable-objects/best-practices/access-durable-objects-storage/), [SQLite namespace migrations](https://developers.cloudflare.com/changelog/post/2026-07-09-restrict-new-kv-backed-namespaces/).

import { DurableObject } from "cloudflare:workers";
import { applyWater, initialWorld, parseAction, publicWorld, type World } from "./model";

const PATH = "/api/world";
const MAX_BODY = 2048;
const CAPABILITY = /^[A-Za-z0-9_-]{43}$/;

function json(value: unknown, status = 200): Response {
  return Response.json(value, { status, headers: {
    "Cache-Control": "no-store",
    "X-Robots-Tag": "noindex, nofollow"
  } });
}

export class WorldState extends DurableObject<Env> {
  async fetch(request: Request): Promise<Response> {
    if (request.method === "DELETE") {
      await this.ctx.storage.deleteAll();
      return json({ deleted: true });
    }
    if (request.method === "GET") {
      const world = (await this.ctx.storage.get<World>("world")) ?? initialWorld();
      return json({ status: "ok", world: publicWorld(world) });
    }
    if (request.method !== "POST") return json({ error: "METHOD_NOT_ALLOWED" }, 405);

    let input: unknown;
    try {
      const body = await request.text();
      if (new TextEncoder().encode(body).byteLength > MAX_BODY) return json({ error: "PAYLOAD_TOO_LARGE" }, 413);
      input = JSON.parse(body);
    } catch {
      return json({ error: "INVALID_JSON" }, 400);
    }
    const action = parseAction(input);
    if (action === null) return json({ error: "INVALID_ACTION" }, 422);

    // Atomic transaction: two concurrent devices cannot silently drop one
    // another's updates by reading the same prior revision.
    const result = await this.ctx.storage.transaction(async (transaction) => {
      const world = (await transaction.get<World>("world")) ?? initialWorld();
      const outcome = applyWater(world, action, Date.now());
      if (outcome.status === "applied") await transaction.put("world", outcome.world);
      return outcome;
    });
    if (result.status === "conflict") return json({ error: result.error, world: publicWorld(result.world) }, 409);
    if (result.status === "complete") return json({ error: result.error, world: publicWorld(result.world) }, 422);
    return json({ status: result.status, world: publicWorld(result.world) });
  }
}

async function keyForCapability(token: string): Promise<string> {
  const bytes = new TextEncoder().encode(token);
  const digest = new Uint8Array(await crypto.subtle.digest("SHA-256", bytes));
  return [...digest].map((b) => b.toString(16).padStart(2, "0")).join("");
}

function withCors(reply: Response, origin: string | null, env: Env): Response {
  const headers = new Headers(reply.headers);
  headers.set("Vary", "Origin");
  headers.set("Cache-Control", "no-store");
  if (origin !== null && env.ALLOWED_ORIGIN === origin) {
    headers.set("Access-Control-Allow-Origin", origin);
    headers.set("Access-Control-Allow-Headers", "Authorization, Content-Type");
    headers.set("Access-Control-Allow-Methods", "GET, POST, DELETE, OPTIONS");
    headers.set("Access-Control-Max-Age", "600");
  }
  return new Response(reply.body, { status: reply.status, headers });
}

async function handleRequest(request: Request, env: Env): Promise<Response> {
  const url = new URL(request.url);
  const origin = request.headers.get("Origin");
  if (url.pathname === "/healthz" && request.method === "GET")
    return json({ service: "mosselora-world-api", status: "prototype", gameplaySync: false });
  if (url.pathname !== PATH) return json({ error: "NOT_FOUND" }, 404);
  if (origin !== null && origin !== env.ALLOWED_ORIGIN) return json({ error: "ORIGIN_DENIED" }, 403);
  if (request.method === "OPTIONS")
    return origin === null ? json({ error: "ORIGIN_REQUIRED" }, 403) : withCors(new Response(null, { status: 204 }), origin, env);
  if (!["GET", "POST", "DELETE"].includes(request.method)) return json({ error: "METHOD_NOT_ALLOWED" }, 405);
  const authorization = request.headers.get("Authorization") ?? "";
  if (!authorization.startsWith("Bearer ") || !CAPABILITY.test(authorization.slice(7)))
    return json({ error: "CAPABILITY_REQUIRED" }, 401);
  if (request.method === "POST" && request.headers.get("Content-Type")?.split(";")[0].trim() !== "application/json")
    return json({ error: "JSON_REQUIRED" }, 415);

  let body: string | undefined;
  if (request.method === "POST") {
    body = await request.text();
    if (new TextEncoder().encode(body).byteLength > MAX_BODY) return json({ error: "PAYLOAD_TOO_LARGE" }, 413);
  }
  // Per-capability private DO; never store or log the bearer token in the object.
  const key = await keyForCapability(authorization.slice(7));
  const object = env.WORLD.get(env.WORLD.idFromName(key));
  const proxy = new Request("https://mosselora.internal/state", {
    method: request.method,
    headers: body !== undefined ? { "Content-Type": "application/json" } : undefined,
    body
  });
  const reply = await object.fetch(proxy);
  return withCors(reply, origin, env);
}

export default {
  async fetch(request: Request, env: Env): Promise<Response> {
    try {
      return await handleRequest(request, env);
    } catch (error) {
      console.error(JSON.stringify({
        event: "mosselora_api_error",
        errorType: error instanceof Error ? error.name : "UnknownError"
      }));
      return withCors(json({ error: "INTERNAL_ERROR" }, 500), request.headers.get("Origin"), env);
    }
  }
} satisfies ExportedHandler<Env>;

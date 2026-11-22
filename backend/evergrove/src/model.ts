/** Pure, deterministic model for the OPTIONAL Cloudflare Durable Object tree demo. */
export interface World {
  schemaVersion: 1;
  revision: number;
  treeStage: number;
  lastWatered: number;
  recentOperations: string[];
}
export interface WaterAction {
  type: "WATER_TREE";
  operationId: string;
  expectedRevision: number;
}
export type Result =
  | { status: "applied" | "duplicate"; world: World }
  | { status: "conflict" | "complete"; world: World; error: string };
const UUID = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-8][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;
const DEDUP_WINDOW = 256;

export function initialWorld(): World {
  return { schemaVersion: 1, revision: 0, treeStage: 0, lastWatered: 0, recentOperations: [] };
}
export function publicWorld(world: World): Omit<World, "recentOperations"> {
  return { schemaVersion: 1, revision: world.revision, treeStage: world.treeStage, lastWatered: world.lastWatered };
}
export function parseAction(raw: unknown): WaterAction | null {
  if (typeof raw !== "object" || raw === null || Array.isArray(raw)) return null;
  const value = raw as Record<string, unknown>;
  if (value.type !== "WATER_TREE" || typeof value.operationId !== "string" || !UUID.test(value.operationId)) return null;
  if (typeof value.expectedRevision !== "number" || !Number.isSafeInteger(value.expectedRevision) || value.expectedRevision < 0) return null;
  if (Object.keys(value).some((key) => !["type", "operationId", "expectedRevision"].includes(key))) return null;
  return { type: "WATER_TREE", operationId: value.operationId.toLowerCase(), expectedRevision: value.expectedRevision };
}
export function applyWater(world: World, action: WaterAction, now: number): Result {
  if (world.recentOperations.includes(action.operationId)) return { status: "duplicate", world };
  if (action.expectedRevision !== world.revision) return { status: "conflict", world, error: "REVISION_CONFLICT" };
  if (world.treeStage >= 3) return { status: "complete", world, error: "TREE_FULLY_GROWN" };
  if (!Number.isSafeInteger(now) || now < 0 || world.revision >= Number.MAX_SAFE_INTEGER)
    throw new Error("Invalid clock or revision");
  return {
    status: "applied",
    world: {
      schemaVersion: 1,
      revision: world.revision + 1,
      treeStage: world.treeStage + 1,
      lastWatered: now,
      recentOperations: [...world.recentOperations, action.operationId].slice(-DEDUP_WINDOW)
    }
  };
}

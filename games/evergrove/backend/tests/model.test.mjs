import test from "node:test";
import assert from "node:assert/strict";
import { initialWorld, publicWorld, parseAction, applyWater } from "../src/model.ts";
const ids = [
  "ae89e05f-b19e-48bd-a1fd-e6b08419115c",
  "58746b22-bce5-4492-bcbb-11e0ba8c58d8",
  "8c1998e5-94b8-4d0d-8e8d-9706d36f2a1f",
  "728e4570-1491-4c9f-a5cb-683e79cf75ff"
];
const act = (i, revision) => ({ type: "WATER_TREE", operationId: ids[i], expectedRevision: revision });

test("fresh world has bounded, nonpublic operation history", () => {
  assert.deepEqual(publicWorld(initialWorld()), { schemaVersion: 1, revision: 0, treeStage: 0, lastWatered: 0 });
  assert.deepEqual(initialWorld().recentOperations, []);
});
test("invalid actions reject unknown fields, bad revisions and invalid IDs", () => {
  assert.equal(parseAction({ type: "HACK", operationId: ids[0], expectedRevision: 0 }), null);
  assert.equal(parseAction({ type: "WATER_TREE", operationId: "bad", expectedRevision: 0 }), null);
  assert.equal(parseAction({ ...act(0, 0), role: "admin" }), null);
  assert.equal(parseAction(act(0, -1)), null);
  assert.deepEqual(parseAction(act(0, 0)), act(0, 0));
});
test("three actions increment once each, then cap growth", () => {
  let state = initialWorld();
  for (let i = 0; i < 3; i++) {
    const result = applyWater(state, act(i, i), 100 + i);
    assert.equal(result.status, "applied");
    state = result.world;
    assert.equal(state.revision, i + 1);
    assert.equal(state.treeStage, i + 1);
  }
  assert.equal(applyWater(state, act(3, 3), 103).status, "complete");
});
test("same operation remains idempotent on retries", () => {
  const first = applyWater(initialWorld(), act(0, 0), 100);
  assert.equal(applyWater(first.world, act(0, 0), 101).status, "duplicate");
  const next = applyWater(first.world, act(1, 1), 102);
  assert.equal(applyWater(next.world, act(0, 0), 104).status, "duplicate");
});
test("stale revision gets conflict, never a silent overwrite", () => {
  const first = applyWater(initialWorld(), act(0, 0), 100);
  const stale = applyWater(first.world, act(1, 0), 101);
  assert.equal(stale.status, "conflict");
  assert.equal(first.world.treeStage, 1);
});

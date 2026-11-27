# I284 — A note really arrives

## Goal
- So far only a log line showed what would be sent. See a real notification,
  from the real plugin, arrive.

## What happened
- A development-only launch option (`SIMCTL_CHILD_MosseloraNoteTest=1`) turns
  notes on and queues a note 75 seconds out. It's the garden's real note, or a
  clearly marked test line when nothing is waiting. It never exists in release
  builds.
- On the simulator:
  1. The iOS permission prompt appeared after notes were turned on ("Mosselora
     Playtest would like to send you notifications"). Allowed.
  2. The simulator was locked.
  3. 75 seconds later the note appeared on the lock screen with Mosselora's
     icon: `I284-note-delivered.png`.
- Two of my own bugs, found on the way:
  - Leaving the game ran the real scheduler, which found nothing waiting in a
    new garden and correctly cleared the queue, test note included.
  - Unity reports a resume right after launch, and `Opened()` cleared the queue
    again.

  Both are right for players; the dev test is now exempt from both.

## Result
- The plugin, framework linking (I275), permission flow and scheduling are
  verified on the iOS simulator.
- Not verified: a physical iPhone, the soft ask leading to the prompt in a
  normal (non-test) flow, and the tap-to-open path (I283) from a real tap.

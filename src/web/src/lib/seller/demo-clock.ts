// Deterministic demo clock — the single "now" anchor for all relative-time
// rendering on the seller pages.
//
// Canonical instant: 2026-04-08T16:45:00-07:00 (America/Los_Angeles, PDT),
// which is 2026-04-08T23:45:00Z in UTC. The plan's parenthetical wrote this as
// "00:45Z", but 16:45 − (−07:00) = 23:45Z; the seeded order ages ("2h", "5h",
// "7h", "1d", …) only line up with the hi-fi mock at 23:45Z. Swap to a live
// clock when real buyer sessions exist.
export const DEMO_NOW = new Date("2026-04-08T23:45:00Z");

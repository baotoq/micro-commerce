# QA — Seller Journey

Manual test cases and execution evidence for the seller-journey feature (11 routes).

## Structure

```
qa/
  README.md                     — this file
  TEMPLATE.md                   — test-case template
  test-cases/
    TC-S01-seller-overview.md
    TC-S02-seller-listings.md
    TC-S03-seller-analytics.md
    TC-S04-seller-apply.md
    TC-S05-seller-onboard.md
    TC-S06-seller-welcome.md
    TC-S07-seller-listings-new.md
    TC-S08-seller-first-order.md
    TC-S09-seller-pack-ship.md
    TC-S10-seller-first-month.md
    TC-S11-seller-payouts.md
  evidence/
    screenshots/                — PNG per test case (named <case-id>.png)
    console-logs/               — browser console output per test case
```

## Routes under test

| ID     | Route                       | Batch    | Worker   |
|--------|-----------------------------|----------|----------|
| TC-S01 | /seller                     | A        | worker-1 |
| TC-S02 | /seller/listings            | A        | worker-1 |
| TC-S03 | /seller/analytics           | A        | worker-1 |
| TC-S04 | /seller/apply               | A        | worker-1 |
| TC-S05 | /seller/onboard             | B        | worker-2 |
| TC-S06 | /seller/welcome             | B        | worker-2 |
| TC-S07 | /seller/listings/new        | B        | worker-2 |
| TC-S08 | /seller/first-order         | B        | worker-2 |
| TC-S09 | /seller/pack-ship           | C        | worker-3 |
| TC-S10 | /seller/first-month         | C        | worker-3 |
| TC-S11 | /seller/payouts             | C        | worker-3 |

## Result legend

| Mark    | Meaning                                              |
|---------|------------------------------------------------------|
| `[x]`   | PASS — assertion verified; screenshot in evidence/   |
| `[!]`   | FAIL — assertion failed; screenshot + console log    |
| `[~]`   | BLOCKED — cannot be tested (reason noted)            |

## Brand constants

- Brand name: **Micro Commerce** (logotype on marketing pages: **micro.**)
- Shop owner: **Alex**

## Dev server

Running on `http://localhost:3000`. Do not manage server lifecycle from QA scripts.

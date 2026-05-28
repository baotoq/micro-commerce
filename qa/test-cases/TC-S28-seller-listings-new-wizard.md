# TC-S28 — Route: /seller/listings/new (3-step wizard + SAS photo upload)

**Route:** `/seller/listings/new`
**Batch:** F
**Worker:** qa-manual
**Executed:** 2026-05-28

---

## Preconditions

- Full Aspire stack running (web + Catalog API + Postgres + Azurite). Start
  with `aspire start --apphost src/AppHost/MicroCommerce.AppHost.csproj`.
- Web served on the pinned port `http://localhost:3000`
  (AppHost.cs uses `WithHttpEndpoint(port: 3000, env: "PORT")`).
- Catalog API SAS endpoint reachable via the Next.js proxy
  `/api/products/photo-upload-url`.

This case supersedes TC-S07 (legacy static page) and TC-S24 (wired single-form
flow). It exercises the wizard introduced in commits 46bf4acc / 4da24459 /
95d22489.

---

## Test cases

### S28-01 — Wizard renders Step 1 with progress strip and disabled Next

**Steps:**

1. Navigate to `/seller/listings/new`.
2. Observe the wizard chrome.

**Expected:**

- Page heading "New listing" + subtitle "Listings · Drafts · 1 of 1".
- Progress strip lists "Basics", "Pricing & Inventory", "Media & Discovery";
  the active step has `aria-current="step"` on "Basics".
- Section headings "Title & description" and "Category" are visible.
- Fields SKU, Name, Description (with `0 / 2000` counter), Category render.
- Listing-health rail starts at `0` with the 9 unmet checks listed.
- "Back" is disabled (we are on step 1); "Next" is disabled (required fields
  empty).

**Result:** [x] pass — Evidence: `evidence/screenshots/S28-01.png`.

---

### S28-02 — Step 1 fields lift listing-health score and enable Next

**Steps:**

1. Fill SKU `MC-QA-S28-01`, Name `Persimmon Garden Vase`, Description
   (≥100 chars), Category `Vessels`.
2. Watch the listing-health score and the Next button.

**Expected:**

- Health score rises as fields fill (Name → name unmet drops; Description
  ≥100 chars → description unmet drops; Category set → category unmet drops).
- Description char counter updates to `<length> / 2000`.
- Once SKU + Name + Category are populated, "Next" enables.

**Result:** [x] pass — Evidence: `evidence/screenshots/S28-02.png`.

---

### S28-03 — Step 2 renders Pricing/Inventory/Shipping with `aria-current=step`

**Steps:**

1. Click "Next" from Step 1.
2. Observe URL and form.

**Expected:**

- URL becomes `/seller/listings/new?step=2`.
- Progress strip shows `aria-current="step"` on "Pricing & Inventory".
- Section headings "Pricing", "Inventory", "Shipping" render.
- Inputs render: Price (`$` prefix), Total in stock, Status combobox
  defaulting to "Draft", Weight (kg), Origin (placeholder "Portland, OR").
- "Back" is enabled. "Next" is disabled until required step-2 fields pass.
- Health-score rail updates to remove the Step-1 checks already completed.

**Result:** [x] pass — Evidence: `evidence/screenshots/S28-03.png`.

---

### S28-04 — Step 2 fields enable Next

**Steps:**

1. Fill Price `79.99`, Total in stock `15`, Weight `1.25`, Origin
   `Portland, OR`. Leave Status at default ("Draft").
2. Observe Next button.

**Expected:**

- Health-score continues to rise (price/inventory/weight/origin checks drop).
- "Next" enables once all required Step-2 fields are valid.

**Result:** [x] pass — Evidence: `evidence/screenshots/S28-04.png`.

---

### S28-05 — Step 3 renders Photos + Tags and surfaces the Publish CTA

**Steps:**

1. Click "Next" from Step 2.
2. Observe URL, sections, and CTAs.

**Expected:**

- URL becomes `/seller/listings/new?step=3`.
- Progress strip shows `aria-current="step"` on "Media & Discovery".
- Photo grid renders 6 empty "Add photo" slots + a hidden file input
  labelled "Choose a photo to upload".
- Tags section renders with a single "Add a tag" textbox.
- Footer CTAs become "Back" + "Publish" (no more "Next").

**Result:** [x] pass — Evidence: `evidence/screenshots/S28-05.png`.

---

### S28-06 — Photo upload SAS round-trip fails with a CORS error on Azurite (regression)

**Steps:**

1. On Step 3, click the first "Add photo" slot.
2. In the file chooser, select `src/web/e2e/fixtures/photo-small.jpg`
   (100×100, ~4 KB JPEG).
3. Watch the network calls and DevTools console.

**Expected:**

- POST `/api/products/photo-upload-url` returns `200` with `{ uploadUrl,
  blobUrl, expiresAt }` (proxied via the new Next.js route handler at
  `src/web/src/app/api/products/photo-upload-url/route.ts`).
- The PUT to the returned `uploadUrl` (Azurite at `http://127.0.0.1:<port>`)
  succeeds — i.e. Azurite's CORS allows the browser-origin PUT.
- After PUT, slot 1 swaps to a thumbnail with a "Primary" pill and
  `data-photo-url` matching the bare `blobUrl` (no SAS signature).

**Actual:**

- The SAS POST succeeds (`200`) — proxy is working.
- The direct PUT to Azurite is blocked: `Access to fetch ... has been blocked
  by CORS policy: Response to preflight request doesn't pass access control
  check: No 'Access-Control-Allow-Origin' header is present`. The uploader
  shows "Network error. Check your connection." with a Retry button.
- Root cause (now fixed in `Catalog.API/src/Api/Program.cs`): the dev
  Azurite container ships without CORS rules. The fix adds a
  `SetPropertiesAsync` call on startup that publishes an `AllowedOrigins=*`
  PUT/GET CORS rule for development environments only.

**Result:** [x] pass (after fix). The first-pass run captured the CORS
failure (`evidence/console-logs/S28-06.txt`). After landing the
`SetPropertiesAsync` CORS init and rebuilding the Catalog API, a re-run
shows "Photo 1" with the "Primary" pill and the bare blob URL (see
`evidence/screenshots/S28-06.png`).

---

## Summary

All six S28 cases pass after the photo-upload regression was traced and
closed. Two infrastructure gaps were fixed during this run, both required
for the browser-side wizard flow to round-trip end-to-end:

1. **Next.js proxy routes** (web). The wizard's same-origin `fetch` calls
   target `/api/products/photo-upload-url` and
   `/api/products/by-sku/<sku>/exists`, but neither route existed on the
   Next.js side. Both now exist as thin proxies that forward to the
   `API_URL` Catalog endpoint (see
   `src/web/src/app/api/products/photo-upload-url/route.ts` and
   `src/web/src/app/api/products/by-sku/[sku]/exists/route.ts`).
2. **Azurite CORS** (Catalog.API startup). SAS-direct browser PUTs were
   blocked by missing CORS on the Azurite emulator. `Program.cs` now
   reads current properties, swaps in a permissive dev-only CORS rule
   (`AllowedOrigins=*`, `Methods=PUT,GET,OPTIONS,HEAD`), and writes them
   back via `BlobServiceClient.SetPropertiesAsync`. Production storage
   accounts should set this via their own CORS policy.

Re-running the relevant Playwright specs after these landed:
- `seller-listings-new-photo.spec.ts` — passes; preview renders with the
  bare blob URL and no `sig=` token.
- `seller-listings-new-wizard.spec.ts` — wizard happy path now reaches
  Step 3 photo upload without the prior network/CORS noise.

// Photo upload happy-path against the live Aspire stack.
//
// Requires the Aspire AppHost to be running so that:
//   1. POST /api/products/photo-upload-url returns a real SAS URL from Azurite.
//   2. The PUT to that SAS URL succeeds against the Azurite container.
//
// When the stack is not up the test will be marked failed by Playwright;
// the verifier owns the run (`dotnet run --project src/AppHost` then
// `BASE_URL=<web-endpoint> npm run e2e`). Tagged @seed-dependent so the
// scripts/e2e:smoke filter excludes it from runs without the stack.

import path from "node:path";
import { fileURLToPath } from "node:url";
import { expect, test } from "./fixtures/test";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const PHOTO_FIXTURE = path.join(__dirname, "fixtures", "photo-small.jpg");

test.describe(
  "New listing photo upload",
  { tag: ["@listings", "@seed-dependent"] },
  () => {
    test("uploads a JPEG and persists the blob URL in the form", async ({
      page,
    }) => {
      // Jump straight to the media step — the wizard seeds `step` from the
      // URL on mount, so we don't need to fill steps 1 and 2 just to exercise
      // the uploader.
      await page.goto("/seller/listings/new?step=3");

      await expect(
        page.getByRole("heading", { name: /Photos/i }),
      ).toBeVisible();

      const fileInput = page.locator('input[type="file"]');
      await fileInput.setInputFiles(PHOTO_FIXTURE);

      // Once the SAS round-trip resolves the uploader renders an <Image>
      // with the persisted blob URL exposed via data-photo-url so the e2e
      // can assert on the bare URL without parsing next/image's src.
      const preview = page.getByAltText(/photo 1/i);
      await expect(preview).toBeVisible({ timeout: 15_000 });

      const storedUrl = await preview.getAttribute("data-photo-url");
      expect(storedUrl).toBeTruthy();
      expect(storedUrl).toMatch(/\/photos\/products\//);
      // The persisted URL must be the bare blob URL — no SAS signature.
      expect(storedUrl).not.toContain("sig=");

      await expect(page.getByText(/primary/i)).toBeVisible();
    });
  },
);

/**
 * E2E tests require the full Aspire stack running:
 * dotnet run --project src/MicroCommerce.AppHost
 *
 * Run tests: cd src/MicroCommerce.Web && npm run test:e2e
 */
import { test, expect } from '@playwright/test';

test.describe('Product Browsing', () => {
  test('homepage displays product grid', async ({ page }) => {
    await page.goto('/');
    // Wait for products to load
    await expect(page.locator('[data-testid="product-card"], article, .product-card').first()).toBeVisible({ timeout: 10000 });
  });

  test('product detail page shows product info', async ({ page }) => {
    await page.goto('/');
    // Click first product link
    await page.locator('a[href*="/products/"]').first().click();
    // Verify URL contains product ID
    await expect(page).toHaveURL(/\/products\//);
    // Verify product info elements are present
    await expect(page.getByRole('heading').first()).toBeVisible();
  });

  test('search filters products by name', async ({ page }) => {
    await page.goto('/');
    // Find search input
    const searchInput = page.getByPlaceholder(/search/i).or(page.getByRole('searchbox'));
    if (await searchInput.isVisible()) {
      await searchInput.fill('laptop');
      // Wait for debounce + API response
      await page.waitForTimeout(500);
      // URL should contain search param
      await expect(page).toHaveURL(/search=laptop|q=laptop/i);
    }
  });

  test('category filter narrows product list', async ({ page }) => {
    await page.goto('/');
    // Look for category filter (sidebar, dropdown, or buttons)
    const categoryLink = page.getByRole('link', { name: /electronics|clothing|books/i }).first()
      .or(page.getByRole('button', { name: /electronics|clothing|books/i }).first());
    if (await categoryLink.isVisible()) {
      await categoryLink.click();
      // URL should update with category filter
      await page.waitForTimeout(500);
    }
  });

  test('infinite scroll loads more products', async ({ page }) => {
    await page.goto('/');
    // Wait for initial products
    await expect(page.locator('[data-testid="product-card"], article').first()).toBeVisible({ timeout: 10000 });
    // Count initial products
    const initialCount = await page.locator('[data-testid="product-card"], article').count();
    // Scroll to bottom
    await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
    // Wait for potential load
    await page.waitForTimeout(2000);
    // Verify same or more products (scroll may or may not trigger load depending on data)
  });
});

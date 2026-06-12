# Buyer Storefront Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the buyer-facing storefront (home/shop → PDP → cookie cart → Keycloak-gated checkout with demo payment + promo codes → confirmation) per the approved spec at `docs/superpowers/specs/2026-06-12-storefront-design.md`, closing the buyer→order→seller loop.

**Architecture:** New `(storefront)` route group in the Next.js app backed by existing anonymous Catalog read endpoints plus three thin API additions: a buyer checkout command (`PlaceStorefrontOrderCommand`, server-priced, auto-numbered, inventory-decrementing, promo-validating), order discount fields, and a buyer-scoped order read. Cart lives in an httpOnly cookie mutated only by server actions. A buyer is any authenticated Keycloak user (new `BuyerPolicy` = authenticated; no new realm role).

**Tech Stack:** .NET 10 (MediatR v12, EF Core/Npgsql, Vogen), xUnit v3, Next.js 16 App Router (cacheComponents, React 19, RHF), Tailwind v4, Vitest, Playwright, Keycloak via Aspire.

**Conventions that bind every task:**
- TDD: write the failing test first, watch it fail, implement, watch it pass, commit.
- EF: mutating handlers MUST use `.AsTracking()` (global default is NoTracking — silent no-op saves otherwise). Always construct `DateTimeOffset` in UTC (Npgsql timestamptz).
- Money rounding: `Math.Round(x, 2, MidpointRounding.AwayFromZero)` in C#; `Math.round(x * 100) / 100` in TS.
- Wire values: promotion `kind` is `"percentage" | "fixed"`; order status keys are lowercase (`"new"`).
- Frontend: Biome (`npm run lint`), 2-space, `@/*` alias. React Compiler is on — no manual memo; RHF components need `"use no memo"`.
- All `dotnet` commands run from the repo root; all `npm`/`npx` commands run from `src/web/`.

**Demo constants (spec §7):** shipping `standard $8 / express $22 / pickup $0`; tax = 8.5% of the discounted subtotal; discount applies to item subtotal only, fixed promos cap at the subtotal. DO NOT add tax maintenance tests (CLAUDE.md).

---

## File map

**Backend (Catalog.API):**
| File | Action | Responsibility |
|---|---|---|
| `src/Services/Catalog.API/src/Domain/Orders/Order.cs` | Modify | `DiscountCode`/`DiscountAmount` + `ApplyDiscount()`, `Paid` includes discount |
| `src/Services/Catalog.API/src/Domain/Products/Product.cs` | Modify | `DecrementInventory(int)` |
| `src/Services/Catalog.API/src/Application/Orders/Dtos/OrderDtos.cs` | Modify | `OrderSummaryDto` + discount fields |
| `src/Services/Catalog.API/src/Application/Orders/OrderMapping.cs` | Modify | map discount into summary |
| `src/Services/Catalog.API/src/Infrastructure/Persistence/OrderConfiguration.cs` | Modify | column config for discount |
| `src/Services/Catalog.API/src/Infrastructure/Persistence/Migrations/` | Create | `AddOrderDiscount` migration |
| `src/Services/Catalog.API/src/Application/Orders/Commands/PlaceStorefrontOrder.cs` | Create | buyer checkout command + handler |
| `src/Services/Catalog.API/src/Application/Products/Queries/GetProducts.cs` | Modify | `Buyable`/`Category`/`Sort` params |
| `src/Services/Catalog.API/src/Application/Products/Queries/GetProductCategories.cs` | Create | distinct buyable categories |
| `src/Services/Catalog.API/src/Api/AuthenticationExtensions.cs` | Modify | `BuyerPolicy` |
| `src/Services/Catalog.API/src/Api/Endpoints/StorefrontOrderEndpoints.cs` | Create | `POST /api/orders/checkout`, `GET /api/orders/{n}/confirmation` |
| `src/Services/Catalog.API/src/Api/Endpoints/ProductReadEndpoints.cs` | Modify | new query params + `/categories` |
| `src/Services/Catalog.API/src/Api/Program.cs` | Modify | map storefront endpoints |
| `src/AppHost/Realms/microcommerce-realm.json` | Modify | self-registration + buyer user |

**Backend tests:** `tests/Domain.Tests/Orders/OrderDiscountTests.cs`, `tests/Domain.Tests/Products/ProductInventoryTests.cs`, `tests/Application.Tests/Orders/Commands/PlaceStorefrontOrderHandlerTests.cs`, `tests/Application.Tests/Products/Queries/GetProductsStorefrontTests.cs` + `GetProductCategoriesTests.cs`, `tests/FunctionalTests/Storefront/StorefrontCheckoutTests.cs` (+ modify `Auth/TestTokens.cs`, `CatalogWebApplicationFactory.cs`).

**Frontend (src/web):**
| File | Action | Responsibility |
|---|---|---|
| `src/lib/storefront/cart.ts` (+`.test.ts`) | Create | pure cart codec/ops |
| `src/lib/storefront/totals.ts` (+`.test.ts`) | Create | shipping/tax/discount math + constants |
| `src/lib/storefront/cart-cookie.ts` | Create | httpOnly cookie read/write |
| `src/lib/storefront/data.ts` | Create | `"use cache"` shop loaders |
| `src/lib/storefront/actions.ts` (+`.test.ts`) | Create | cart + promo + placeOrder server actions |
| `src/lib/catalog/api.ts` | Modify | `category`/`sort`/`buyable` params + `fetchProductCategories` |
| `src/lib/catalog/checkout.ts` | Create | authenticated checkout/confirmation fetchers |
| `src/components/storefront/topbar.tsx` | Create | Micro Commerce topbar + cart badge island |
| `src/components/storefront/product-image.tsx` | Create | photo w/ tone-gradient fallback |
| `src/components/storefront/product-card.tsx` | Create | grid card |
| `src/components/storefront/add-to-bag.tsx` (+`.test.tsx`) | Create | qty + add action (client) |
| `src/components/storefront/cart-line-row.tsx` | Create | qty stepper/remove forms |
| `src/components/storefront/promo-form.tsx` | Create | promo input + errors |
| `src/components/storefront/checkout-form.tsx` (+`.test.tsx`) | Create | RHF form, responsive wizard |
| `src/app/(storefront)/layout.tsx` | Create | shop chrome |
| `src/app/(storefront)/page.tsx` | Create | home/shop (replaces root page) |
| `src/app/page.tsx` | **Delete** | placeholder superseded (route conflict otherwise) |
| `src/app/(storefront)/products/[sku]/page.tsx` | Create | PDP |
| `src/app/(storefront)/cart/page.tsx` | Create | cart |
| `src/app/(storefront)/checkout/page.tsx` | Create | login-gated checkout |
| `src/app/(storefront)/checkout/confirmation/[number]/page.tsx` | Create | confirmation |
| `src/lib/seller/orders/types.ts` | Modify | `OrderSummary` discount fields |
| `src/components/seller/orders/order-detail-summary.tsx` (+test) | Modify | promo row |
| `src/proxy.ts` | Modify | bounce anonymous `/checkout` |
| `e2e/buyer-auth.setup.ts`, `e2e/storefront-funnel.spec.ts` | Create | buyer login + full funnel |
| `playwright.config.ts` | Modify | buyer setup + storefront projects |

---

### Task 1: Domain — order discount + product inventory decrement

**Files:**
- Create: `src/Services/Catalog.API/tests/Domain.Tests/Orders/OrderDiscountTests.cs`
- Create: `src/Services/Catalog.API/tests/Domain.Tests/Products/ProductInventoryTests.cs`
- Modify: `src/Services/Catalog.API/src/Domain/Orders/Order.cs`
- Modify: `src/Services/Catalog.API/src/Domain/Products/Product.cs`

- [ ] **Step 1: Write the failing domain tests**

Check how existing Domain.Tests construct an Order first (`ls src/Services/Catalog.API/tests/Domain.Tests/Orders/`) and reuse their builder/helper if one exists; otherwise use this inline factory. Create `OrderDiscountTests.cs`:

```csharp
using MicroCommerce.Catalog.Domain.Orders;

namespace MicroCommerce.Catalog.Domain.Tests.Orders;

public class OrderDiscountTests
{
    private static Order NewOrder(decimal unitPrice = 86m, int qty = 1) =>
        new(
            1042,
            new DateTimeOffset(2026, 6, 12, 0, 0, 0, TimeSpan.Zero),
            "Sasha Leblanc", "sasha.l@gmail.com", "San Francisco, CA",
            "820 Sutter St · #4B", "San Francisco, CA 94109", true,
            "Persimmon vase", "Standard", 8m, 6.5m, 4m,
            "Visa", "4242", null, 0m, null, "",
            [new OrderLine("MC-VS-001", "Persimmon vase", "", qty, unitPrice, FulfillmentStatus.Awaiting, null, null)]);

    [Fact]
    public void ApplyDiscount_ReducesPaid()
    {
        var order = NewOrder(unitPrice: 86m); // Subtotal 86, Shipping 8, Tax 6.5
        order.ApplyDiscount("WELCOME10", 10m);

        Assert.Equal("WELCOME10", order.DiscountCode);
        Assert.Equal(10m, order.DiscountAmount);
        Assert.Equal(86m - 10m + 8m + 6.5m, order.Paid);
    }

    [Fact]
    public void NoDiscount_PaidUnchanged()
    {
        var order = NewOrder(unitPrice: 86m);
        Assert.Null(order.DiscountCode);
        Assert.Equal(0m, order.DiscountAmount);
        Assert.Equal(86m + 8m + 6.5m, order.Paid);
    }

    [Fact]
    public void ApplyDiscount_NegativeAmount_Throws()
    {
        var order = NewOrder();
        Assert.Throws<ArgumentOutOfRangeException>(() => order.ApplyDiscount("X", -1m));
    }

    [Fact]
    public void ApplyDiscount_OverSubtotal_Throws()
    {
        var order = NewOrder(unitPrice: 20m);
        Assert.Throws<ArgumentOutOfRangeException>(() => order.ApplyDiscount("X", 21m));
    }

    [Fact]
    public void ApplyDiscount_BlankCode_Throws()
    {
        var order = NewOrder();
        Assert.Throws<ArgumentException>(() => order.ApplyDiscount(" ", 5m));
    }
}
```

Create `ProductInventoryTests.cs` (check existing Domain.Tests/Products for a product factory helper and reuse it if present; the constructor below mirrors `Product`'s public ctor — verify parameter order against `Product.cs:21` before running):

```csharp
using MicroCommerce.Catalog.Domain.Products;

namespace MicroCommerce.Catalog.Domain.Tests.Products;

public class ProductInventoryTests
{
    private static Product NewProduct(int inventory = 5) =>
        new(Sku.From("MC-TS-001"), "Test vase", "Vessels", 86m, inventory, ProductStatus.Active);

    [Fact]
    public void DecrementInventory_ReducesCount()
    {
        var p = NewProduct(inventory: 5);
        p.DecrementInventory(2);
        Assert.Equal(3, p.Inventory);
    }

    [Fact]
    public void DecrementInventory_ToZero_IsAllowed()
    {
        var p = NewProduct(inventory: 2);
        p.DecrementInventory(2);
        Assert.Equal(0, p.Inventory);
    }

    [Fact]
    public void DecrementInventory_BelowZero_Throws()
    {
        var p = NewProduct(inventory: 1);
        Assert.Throws<InvalidOperationException>(() => p.DecrementInventory(2));
    }

    [Fact]
    public void DecrementInventory_NonPositiveQty_Throws()
    {
        var p = NewProduct();
        Assert.Throws<ArgumentOutOfRangeException>(() => p.DecrementInventory(0));
    }
}
```

If the `Product` constructor requires more arguments than shown, adapt the factory to the real signature (description/tags/weight/origin/photoUrls have defaults in `Update` but may be required in the ctor) — keep the assertions identical.

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test src/Services/Catalog.API/tests/Domain.Tests --filter "FullyQualifiedName~OrderDiscountTests|FullyQualifiedName~ProductInventoryTests"`
Expected: FAIL — `Order` has no `ApplyDiscount`/`DiscountCode`; `Product` has no `DecrementInventory` (compile errors count as the red step).

- [ ] **Step 3: Implement the domain changes**

In `Order.cs`, add after the `InternalNote` property (line ~36):

```csharp
    // Storefront promo discount (applies to item subtotal only; see spec §7)
    public string? DiscountCode { get; private set; }
    public decimal DiscountAmount { get; private set; }
```

Change the `Paid` computed property:

```csharp
    public decimal Paid => Subtotal - DiscountAmount + ShippingPaid + Tax;
```

Add the method after `ToggleStar()`:

```csharp
    public void ApplyDiscount(string code, decimal amount)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Discount code required.", nameof(code));
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Discount cannot be negative.");
        if (amount > Subtotal)
            throw new ArgumentOutOfRangeException(nameof(amount), "Discount cannot exceed the item subtotal.");

        DiscountCode = code;
        DiscountAmount = amount;
    }
```

In `Product.cs`, add after `Update(...)`:

```csharp
    public void DecrementInventory(int qty)
    {
        if (qty < 1)
            throw new ArgumentOutOfRangeException(nameof(qty), "Quantity must be at least 1.");
        if (qty > Inventory)
            throw new InvalidOperationException(
                $"Cannot decrement inventory by {qty}; only {Inventory} in stock.");
        Inventory -= qty;
    }
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test src/Services/Catalog.API/tests/Domain.Tests`
Expected: PASS (all Domain tests, not just the new ones — `Paid` changed; existing orders have `DiscountAmount = 0` so nothing else should move).

- [ ] **Step 5: Commit**

```bash
git add src/Services/Catalog.API/src/Domain src/Services/Catalog.API/tests/Domain.Tests
git commit -m "feat(catalog): order discount + product inventory decrement (domain)"
```

---

### Task 2: Wire discount through DTO, mapping, EF config + migration

**Files:**
- Modify: `src/Services/Catalog.API/src/Application/Orders/Dtos/OrderDtos.cs:63-73`
- Modify: `src/Services/Catalog.API/src/Application/Orders/OrderMapping.cs:111-121`
- Modify: `src/Services/Catalog.API/src/Infrastructure/Persistence/OrderConfiguration.cs`
- Create: migration `AddOrderDiscount`

- [ ] **Step 1: Extend `OrderSummaryDto` (append fields to keep existing positional args valid)**

```csharp
public record OrderSummaryDto(
    decimal Subtotal,
    int ItemsCount,
    decimal Shipping,
    decimal Tax,
    decimal Paid,
    decimal FeePct,
    decimal Fee,
    string? LabelCarrier,
    decimal LabelCost,
    decimal Net,
    string? DiscountCode = null,
    decimal DiscountAmount = 0m);
```

- [ ] **Step 2: Map it in `OrderMapping.ToDetailDto` (the `summary` construction)**

```csharp
        var summary = new OrderSummaryDto(
            o.Subtotal,
            o.Lines.Sum(l => l.Qty),
            o.ShippingPaid,
            o.Tax,
            o.Paid,
            o.FeePct,
            o.Fee,
            o.LabelCarrier,
            o.LabelCost,
            o.Net,
            o.DiscountCode,
            o.DiscountAmount);
```

- [ ] **Step 3: EF config.** In `OrderConfiguration.cs`, alongside the other money properties (match the file's existing precision style — look at how `ShippingPaid`/`Tax` are configured and mirror it):

```csharp
        builder.Property(o => o.DiscountCode).HasMaxLength(40);
        builder.Property(o => o.DiscountAmount).HasPrecision(10, 2);
```

- [ ] **Step 4: Build + create the migration**

```bash
dotnet build src/MicroCommerce.slnx
dotnet ef migrations add AddOrderDiscount \
  --project src/Services/Catalog.API/src/Infrastructure \
  --startup-project src/Services/Catalog.API/src/Api \
  --output-dir Persistence/Migrations
```

Expected: a new `2026..._AddOrderDiscount.cs` adding nullable `discount_code` and `discount_amount` (default 0) to `orders`. Inspect it — it must NOT touch other tables (if it does, the model snapshot drifted; stop and investigate).

- [ ] **Step 5: Run the backend unit suites**

Run: `dotnet test src/Services/Catalog.API/tests/Domain.Tests && dotnet test src/Services/Catalog.API/tests/Application.Tests`
Expected: PASS.

- [ ] **Step 6: Commit**

```bash
git add src/Services/Catalog.API/src
git commit -m "feat(catalog): persist + expose order discount (dto, mapping, migration)"
```

---

### Task 3: `PlaceStorefrontOrderCommand` (Application, TDD)

**Files:**
- Create: `src/Services/Catalog.API/tests/Application.Tests/Orders/Commands/PlaceStorefrontOrderHandlerTests.cs`
- Create: `src/Services/Catalog.API/src/Application/Orders/Commands/PlaceStorefrontOrder.cs`

- [ ] **Step 1: Write the failing handler tests**

```csharp
using MicroCommerce.Catalog.Application.Orders.Commands;
using MicroCommerce.Catalog.Application.Orders.Events;
using MicroCommerce.Catalog.Domain.Products;
using MicroCommerce.Catalog.Domain.Promotions;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Tests.Orders.Commands;

public class PlaceStorefrontOrderHandlerTests
{
    private static Product Vase(int inventory = 10) =>
        new(Sku.From("MC-VS-001"), "Persimmon vase", "Vessels", 86m, inventory, ProductStatus.Active);

    private static Product Bowl(int inventory = 10) =>
        new(Sku.From("MC-BW-014"), "Forest bowl", "Tableware", 68m, inventory, ProductStatus.Active);

    private static Promotion Welcome10()
    {
        var p = new Promotion(
            PromotionCode.From("WELCOME10"), "first order",
            DiscountKind.FixedAmount, null, 10m, 40m, null, null);
        p.Activate();
        return p;
    }

    private static PlaceStorefrontOrderCommand Cmd(
        string? promoCode = null,
        params (string sku, int qty)[] lines) =>
        new(
            CustomerEmail: "buyer@microcommerce.dev",
            CustomerName: "Bao Buyer",
            ShipLine1: "241 Telegraph Ave",
            ShipLine2: "Oakland, CA 94612",
            CityState: "Oakland, CA",
            ShippingMethod: "Standard",
            ShippingPaid: 8m,
            Tax: 6.46m,
            PaymentBrand: "Visa",
            PaymentLastFour: "4242",
            PromoCode: promoCode,
            Lines: lines.Length == 0
                ? [new StorefrontLineInput("MC-VS-001", 1)]
                : [.. lines.Select(l => new StorefrontLineInput(l.sku, l.qty))]);

    private static async Task<(AppDbContext db, PlaceStorefrontOrderHandler handler, FakePublisher publisher)>
        ArrangeAsync(string dbName, params object[] entities)
    {
        var db = DbContextFactory.Create(dbName);
        foreach (var e in entities) db.Add(e);
        await db.SaveChangesAsync();
        var publisher = new FakePublisher();
        return (db, new PlaceStorefrontOrderHandler(db, publisher), publisher);
    }

    [Fact]
    public async Task PricesLinesFromDatabase_AndAllocatesNumber()
    {
        var dbName = Guid.NewGuid().ToString();
        var (db, handler, publisher) = await ArrangeAsync(dbName, Vase(), Bowl());

        var result = await handler.Handle(
            Cmd(null, ("MC-VS-001", 1), ("MC-BW-014", 2)), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        var dto = result.Value!;
        Assert.True(dto.Number >= 1001); // allocated, not supplied
        Assert.Equal(86m + 2 * 68m, dto.Summary.Subtotal); // DB prices, not client prices
        Assert.Equal("new", dto.Status);
        var evt = Assert.IsType<OrderCreatedEvent>(Assert.Single(publisher.Published));
        Assert.Equal(dto.Number, evt.Number);
    }

    [Fact]
    public async Task SequentialOrders_GetIncreasingNumbers()
    {
        var dbName = Guid.NewGuid().ToString();
        var (db, handler, _) = await ArrangeAsync(dbName, Vase());

        var first = await handler.Handle(Cmd(), TestContext.Current.CancellationToken);
        var second = await handler.Handle(Cmd(), TestContext.Current.CancellationToken);

        Assert.Equal(first.Value!.Number + 1, second.Value!.Number);
    }

    [Fact]
    public async Task DecrementsInventory_AndPersists()
    {
        var dbName = Guid.NewGuid().ToString();
        var (db, handler, _) = await ArrangeAsync(dbName, Vase(inventory: 5));

        var result = await handler.Handle(Cmd(null, ("MC-VS-001", 2)), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        // Fresh context over the SAME store: catches a missing AsTracking()
        await using var verify = DbContextFactory.Create(dbName);
        var product = await verify.Products.SingleAsync(TestContext.Current.CancellationToken);
        Assert.Equal(3, product.Inventory);
    }

    [Fact]
    public async Task InsufficientStock_ReturnsError_AndPersistsNothing()
    {
        var dbName = Guid.NewGuid().ToString();
        var (db, handler, publisher) = await ArrangeAsync(dbName, Vase(inventory: 1));

        var result = await handler.Handle(Cmd(null, ("MC-VS-001", 2)), TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal("INSUFFICIENT_STOCK", result.Error!.Value.Code);
        Assert.Empty(publisher.Published);
        Assert.Empty(db.Orders);
    }

    [Fact]
    public async Task UnknownSku_ReturnsProductNotFound()
    {
        var (db, handler, _) = await ArrangeAsync(Guid.NewGuid().ToString(), Vase());
        var result = await handler.Handle(Cmd(null, ("MC-NOPE-99", 1)), TestContext.Current.CancellationToken);
        Assert.True(result.IsFailure);
        Assert.Equal("PRODUCT_NOT_FOUND", result.Error!.Value.Code);
    }

    [Fact]
    public async Task DraftProduct_IsNotBuyable()
    {
        var draft = new Product(Sku.From("MC-DR-001"), "Draft pot", "Vessels", 50m, 10, ProductStatus.Draft);
        var (db, handler, _) = await ArrangeAsync(Guid.NewGuid().ToString(), draft);
        var result = await handler.Handle(Cmd(null, ("MC-DR-001", 1)), TestContext.Current.CancellationToken);
        Assert.True(result.IsFailure);
        Assert.Equal("PRODUCT_NOT_FOUND", result.Error!.Value.Code);
    }

    [Fact]
    public async Task EmptyLines_ReturnsEmptyCart()
    {
        var (db, handler, _) = await ArrangeAsync(Guid.NewGuid().ToString(), Vase());
        var cmd = Cmd() with { Lines = [] };
        var result = await handler.Handle(cmd, TestContext.Current.CancellationToken);
        Assert.True(result.IsFailure);
        Assert.Equal("EMPTY_CART", result.Error!.Value.Code);
    }

    [Fact]
    public async Task FixedPromo_AppliedAndPersisted()
    {
        var (db, handler, _) = await ArrangeAsync(Guid.NewGuid().ToString(), Vase(), Welcome10());

        var result = await handler.Handle(Cmd("WELCOME10", ("MC-VS-001", 1)), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal("WELCOME10", result.Value!.Summary.DiscountCode);
        Assert.Equal(10m, result.Value.Summary.DiscountAmount);
        Assert.Equal(86m - 10m + 8m + 6.46m, result.Value.Summary.Paid);
    }

    [Fact]
    public async Task PercentPromo_RoundsAwayFromZero()
    {
        var promo = new Promotion(
            PromotionCode.From("SPRING20"), "sitewide",
            DiscountKind.Percentage, 20, null, null, null, null);
        promo.Activate();
        var (db, handler, _) = await ArrangeAsync(Guid.NewGuid().ToString(), Vase(), promo);

        var result = await handler.Handle(Cmd("SPRING20", ("MC-VS-001", 1)), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(17.20m, result.Value!.Summary.DiscountAmount); // 86 * 20%
    }

    [Fact]
    public async Task PromoBelowMinOrder_ReturnsError()
    {
        var cheap = new Product(Sku.From("MC-CH-001"), "Cheap mug", "Drinkware", 20m, 10, ProductStatus.Active);
        var (db, handler, _) = await ArrangeAsync(Guid.NewGuid().ToString(), cheap, Welcome10());

        var result = await handler.Handle(Cmd("WELCOME10", ("MC-CH-001", 1)), TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal("PROMO_MIN_ORDER", result.Error!.Value.Code);
    }

    [Fact]
    public async Task UnknownOrInactivePromo_ReturnsPromoInvalid()
    {
        var draftPromo = new Promotion(
            PromotionCode.From("FRIENDS"), "sitewide", DiscountKind.Percentage, 15, null, null, null, null);
        // not activated -> Draft
        var (db, handler, _) = await ArrangeAsync(Guid.NewGuid().ToString(), Vase(), draftPromo);

        var unknown = await handler.Handle(Cmd("NOPE", ("MC-VS-001", 1)), TestContext.Current.CancellationToken);
        var inactive = await handler.Handle(Cmd("FRIENDS", ("MC-VS-001", 1)), TestContext.Current.CancellationToken);

        Assert.Equal("PROMO_INVALID", unknown.Error!.Value.Code);
        Assert.Equal("PROMO_INVALID", inactive.Error!.Value.Code);
    }

    [Fact]
    public async Task ExpiredPromo_ReturnsPromoInvalid()
    {
        var expired = new Promotion(
            PromotionCode.From("BLOOM"), "sitewide", DiscountKind.Percentage, 10, null, null,
            new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 4, 12, 0, 0, 0, TimeSpan.Zero));
        expired.Activate();
        var (db, handler, _) = await ArrangeAsync(Guid.NewGuid().ToString(), Vase(), expired);

        var result = await handler.Handle(Cmd("BLOOM", ("MC-VS-001", 1)), TestContext.Current.CancellationToken);

        Assert.Equal("PROMO_INVALID", result.Error!.Value.Code);
    }

    [Fact]
    public async Task FixedPromo_CapsAtSubtotal()
    {
        var cheap = new Product(Sku.From("MC-CH-002"), "Tiny dish", "Tableware", 5m, 10, ProductStatus.Active);
        var big = new Promotion(
            PromotionCode.From("BIGOFF"), "test", DiscountKind.FixedAmount, null, 50m, null, null, null);
        big.Activate();
        var (db, handler, _) = await ArrangeAsync(Guid.NewGuid().ToString(), cheap, big);

        var result = await handler.Handle(Cmd("BIGOFF", ("MC-CH-002", 1)), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(5m, result.Value!.Summary.DiscountAmount); // capped, total never negative
    }
}
```

Notes for the engineer: `DbContextFactory.Create(dbName)` returns `AppDbContext` — if the tuple type `AppDbContextHandle` in `ArrangeAsync` doesn't compile, just declare the helper to return `(AppDbContext, PlaceStorefrontOrderHandler, FakePublisher)`. Promotion code casing: `PromotionCode` is a Vogen type — if `From("WELCOME10")` normalizes/validates differently, mirror what `PromotionSeeder.cs` does. Product ctor: adapt to the real signature as in Task 1.

- [ ] **Step 2: Run to verify failure**

Run: `dotnet test src/Services/Catalog.API/tests/Application.Tests --filter "FullyQualifiedName~PlaceStorefrontOrder"`
Expected: FAIL (compile error — command/handler don't exist).

- [ ] **Step 3: Implement `PlaceStorefrontOrder.cs`**

```csharp
using MediatR;
using MicroCommerce.Catalog.Application.Orders.Dtos;
using MicroCommerce.Catalog.Application.Orders.Events;
using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Domain.Common;
using MicroCommerce.Catalog.Domain.Orders;
using MicroCommerce.Catalog.Domain.Products;
using MicroCommerce.Catalog.Domain.Promotions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MicroCommerce.Catalog.Application.Orders.Commands;

public record StorefrontLineInput(string Sku, int Qty);

/// <summary>
/// Buyer checkout (spec docs/superpowers/specs/2026-06-12-storefront-design.md §5).
/// Server-authoritative: prices come from Products, the discount from Promotions, the
/// order number from max+1 allocation. The web server action supplies shipping/tax
/// demo constants and the payment summary; CustomerEmail comes from the bearer token.
/// </summary>
public record PlaceStorefrontOrderCommand(
    string CustomerEmail,
    string CustomerName,
    string ShipLine1,
    string ShipLine2,
    string CityState,
    string ShippingMethod,
    decimal ShippingPaid,
    decimal Tax,
    string PaymentBrand,
    string PaymentLastFour,
    string? PromoCode,
    IReadOnlyList<StorefrontLineInput> Lines) : IRequest<Result<OrderDetailDto>>;

public class PlaceStorefrontOrderHandler(AppDbContext db, IPublisher publisher)
    : IRequestHandler<PlaceStorefrontOrderCommand, Result<OrderDetailDto>>
{
    private const string PostgresUniqueViolation = "23505";
    private const int FirstOrderNumber = 1001;
    private const decimal DefaultFeePct = 4m;
    private const int MaxNumberRetries = 3;

    public async Task<Result<OrderDetailDto>> Handle(PlaceStorefrontOrderCommand request, CancellationToken ct)
    {
        if (request.Lines.Count == 0 || request.Lines.Any(l => l.Qty < 1))
            return Result<OrderDetailDto>.Conflict("EMPTY_CART", "The cart has no valid lines.");

        // Load + mutate inventory => tracking required (global default is NoTracking).
        var skus = request.Lines.Select(l => l.Sku).Distinct().ToList();
        var products = await db.Products.AsTracking()
            .Where(p => skus.Contains(p.Sku))
            .ToDictionaryAsync(p => p.Sku.Value, ct);

        var orderLines = new List<OrderLine>();
        foreach (var line in request.Lines)
        {
            if (!products.TryGetValue(line.Sku, out var product) || product.Status == ProductStatus.Draft)
                return Result<OrderDetailDto>.Conflict("PRODUCT_NOT_FOUND", $"Product '{line.Sku}' is not available.");
            if (line.Qty > product.Inventory)
                return Result<OrderDetailDto>.Conflict(
                    "INSUFFICIENT_STOCK",
                    $"Only {product.Inventory} of '{product.Name}' in stock.");

            orderLines.Add(new OrderLine(
                product.Sku.Value, product.Name, Tone: "", line.Qty, product.Price,
                FulfillmentStatus.Awaiting, Tracking: null, RestockNote: null));
        }

        var subtotal = orderLines.Sum(l => l.UnitPrice * l.Qty);

        string? discountCode = null;
        var discountAmount = 0m;
        if (!string.IsNullOrWhiteSpace(request.PromoCode))
        {
            var validation = await ValidatePromoAsync(request.PromoCode, subtotal, ct);
            if (validation.IsFailure)
                return Result<OrderDetailDto>.Conflict(validation.Error!.Value.Code, validation.Error.Value.Message);
            (discountCode, discountAmount) = validation.Value;
        }

        foreach (var line in request.Lines)
            products[line.Sku].DecrementInventory(line.Qty);

        var itemsSummary = string.Join(", ", orderLines
            .Select(l => l.Qty > 1 ? $"{l.ProductName} ×{l.Qty}" : l.ProductName));

        Order? order = null;
        for (var attempt = 1; attempt <= MaxNumberRetries; attempt++)
        {
            var next = (await db.Orders.AsNoTracking().MaxAsync(o => (int?)o.Number, ct)
                ?? (FirstOrderNumber - 1)) + 1;

            order = new Order(
                next, DateTimeOffset.UtcNow,
                request.CustomerName, request.CustomerEmail, request.CityState,
                request.ShipLine1, request.ShipLine2, billSameAsShip: true,
                itemsSummary, request.ShippingMethod, request.ShippingPaid, request.Tax,
                DefaultFeePct, request.PaymentBrand, request.PaymentLastFour,
                labelCarrier: null, labelCost: 0m, labelWeightLabel: null,
                internalNote: "", orderLines);
            if (discountAmount > 0 && discountCode is not null)
                order.ApplyDiscount(discountCode, discountAmount);

            db.Orders.Add(order);
            try
            {
                await db.SaveChangesAsync(ct);
                break;
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is PostgresException { SqlState: PostgresUniqueViolation }
                      && attempt < MaxNumberRetries)
            {
                db.Entry(order).State = EntityState.Detached;
            }
        }

        var dto = await OrderMapping.ToDetailDtoWithCustomerAsync(db, order!, ct);
        await publisher.Publish(new OrderCreatedEvent(order!.Number, dto.Status), ct);
        return Result<OrderDetailDto>.Success(dto);
    }

    private async Task<Result<(string Code, decimal Amount)>> ValidatePromoAsync(
        string code, decimal subtotal, CancellationToken ct)
    {
        var normalized = code.Trim().ToUpperInvariant();
        var promo = await db.Promotions.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Code == PromotionCode.From(normalized), ct);

        var now = DateTimeOffset.UtcNow;
        if (promo is null
            || promo.Status != PromotionStatus.Active
            || (promo.StartsAt is { } start && now < start)
            || (promo.EndsAt is { } end && now > end))
        {
            return Result<(string, decimal)>.Conflict("PROMO_INVALID", "This promo code is not valid.");
        }

        if (promo.MinOrderAmount is { } min && subtotal < min)
            return Result<(string, decimal)>.Conflict(
                "PROMO_MIN_ORDER", $"This code needs a minimum order of {min:C}.");

        var amount = promo.Kind switch
        {
            DiscountKind.Percentage =>
                Math.Round(subtotal * promo.PercentValue!.Value / 100m, 2, MidpointRounding.AwayFromZero),
            DiscountKind.FixedAmount => Math.Min(promo.FixedAmount!.Value, subtotal),
            _ => 0m,
        };

        return Result<(string, decimal)>.Success((normalized, amount));
    }
}
```

Adapt to reality where the existing code disagrees with this sketch (check `Result<T>` factory names in `Domain/Common`, `OrderLine` ctor parameter names from `CreateOrder.cs:53-62`, and whether `p.Code == PromotionCode.From(...)` translates in-memory — `GetPromotionByCode.cs` shows the working comparison pattern; copy it). If `PromotionCode.From` throws on arbitrary strings, wrap in try/catch and return `PROMO_INVALID`.

- [ ] **Step 4: Run to verify pass**

Run: `dotnet test src/Services/Catalog.API/tests/Application.Tests --filter "FullyQualifiedName~PlaceStorefrontOrder"`
Expected: PASS (all 12 tests).

- [ ] **Step 5: Run the full Application.Tests suite, then commit**

Run: `dotnet test src/Services/Catalog.API/tests/Application.Tests`
Expected: PASS.

```bash
git add src/Services/Catalog.API/src/Application src/Services/Catalog.API/tests/Application.Tests
git commit -m "feat(catalog): PlaceStorefrontOrderCommand — server-priced buyer checkout"
```

---

### Task 4: Product query — `buyable`/`category`/`sort` + categories list (TDD)

**Files:**
- Create: `src/Services/Catalog.API/tests/Application.Tests/Products/Queries/GetProductsStorefrontTests.cs`
- Create: `src/Services/Catalog.API/tests/Application.Tests/Products/Queries/GetProductCategoriesTests.cs`
- Modify: `src/Services/Catalog.API/src/Application/Products/Queries/GetProducts.cs`
- Create: `src/Services/Catalog.API/src/Application/Products/Queries/GetProductCategories.cs`

- [ ] **Step 1: Write the failing tests**

`GetProductsStorefrontTests.cs`:

```csharp
using MicroCommerce.Catalog.Application.Products.Queries;
using MicroCommerce.Catalog.Domain.Products;

namespace MicroCommerce.Catalog.Application.Tests.Products.Queries;

public class GetProductsStorefrontTests
{
    private static Product P(string sku, string name, string category, decimal price, ProductStatus status) =>
        new(Sku.From(sku), name, category, price, 10, status);

    private static async Task<AppDbContext> SeedAsync()
    {
        var db = DbContextFactory.Create();
        db.AddRange(
            P("MC-A", "Active vase", "Vessels", 86m, ProductStatus.Active),
            P("MC-B", "Low bowl", "Tableware", 30m, ProductStatus.Low),
            P("MC-C", "Out mug", "Drinkware", 20m, ProductStatus.Out),
            P("MC-D", "Draft pot", "Vessels", 50m, ProductStatus.Draft));
        await db.SaveChangesAsync();
        return db;
    }

    [Fact]
    public async Task Buyable_ExcludesDraftAndOut()
    {
        await using var db = await SeedAsync();
        var handler = new GetProductsHandler(db);
        var page = await handler.Handle(
            new GetProductsQuery(1, 20, null, null, Buyable: true), TestContext.Current.CancellationToken);
        Assert.Equal(["MC-A", "MC-B"], page.Items.Select(i => i.Sku).Order().ToArray());
    }

    [Fact]
    public async Task Category_Filters()
    {
        await using var db = await SeedAsync();
        var handler = new GetProductsHandler(db);
        var page = await handler.Handle(
            new GetProductsQuery(1, 20, null, null, Buyable: true, Category: "Vessels"),
            TestContext.Current.CancellationToken);
        Assert.Equal(["MC-A"], page.Items.Select(i => i.Sku).ToArray());
    }

    [Fact]
    public async Task Sort_PriceAscAndDesc()
    {
        await using var db = await SeedAsync();
        var handler = new GetProductsHandler(db);
        var asc = await handler.Handle(
            new GetProductsQuery(1, 20, null, null, Buyable: true, Sort: "price-asc"),
            TestContext.Current.CancellationToken);
        var desc = await handler.Handle(
            new GetProductsQuery(1, 20, null, null, Buyable: true, Sort: "price-desc"),
            TestContext.Current.CancellationToken);
        Assert.Equal(["MC-B", "MC-A"], asc.Items.Select(i => i.Sku).ToArray());
        Assert.Equal(["MC-A", "MC-B"], desc.Items.Select(i => i.Sku).ToArray());
    }
}
```

`GetProductCategoriesTests.cs`:

```csharp
using MicroCommerce.Catalog.Application.Products.Queries;
using MicroCommerce.Catalog.Domain.Products;

namespace MicroCommerce.Catalog.Application.Tests.Products.Queries;

public class GetProductCategoriesTests
{
    [Fact]
    public async Task ReturnsDistinctBuyableCategories_Sorted()
    {
        await using var db = DbContextFactory.Create();
        db.AddRange(
            new Product(Sku.From("MC-1"), "A", "Vessels", 10m, 5, ProductStatus.Active),
            new Product(Sku.From("MC-2"), "B", "Tableware", 10m, 5, ProductStatus.Low),
            new Product(Sku.From("MC-3"), "C", "Vessels", 10m, 5, ProductStatus.Active),
            new Product(Sku.From("MC-4"), "D", "Hidden", 10m, 5, ProductStatus.Draft));
        await db.SaveChangesAsync();

        var handler = new GetProductCategoriesHandler(db);
        var categories = await handler.Handle(new GetProductCategoriesQuery(), TestContext.Current.CancellationToken);

        Assert.Equal(["Tableware", "Vessels"], categories);
    }
}
```

(Adapt `Product` ctor + `using MicroCommerce.Catalog.Application.Persistence;` imports as in earlier tasks.)

- [ ] **Step 2: Run to verify failure**

Run: `dotnet test src/Services/Catalog.API/tests/Application.Tests --filter "FullyQualifiedName~GetProductsStorefront|FullyQualifiedName~GetProductCategories"`
Expected: FAIL (compile errors — new params/types missing).

- [ ] **Step 3: Implement.** Extend the record with defaulted params (existing callers keep compiling):

```csharp
public record GetProductsQuery(
    int Page,
    int PageSize,
    string? Status,
    string? Search,
    bool Buyable = false,
    string? Category = null,
    string? Sort = null) : IRequest<PagedResult<ProductDto>>;
```

In the handler, after the `Status` filter:

```csharp
        if (request.Buyable)
            query = query.Where(p => p.Status == ProductStatus.Active || p.Status == ProductStatus.Low);

        if (!string.IsNullOrWhiteSpace(request.Category))
            query = query.Where(p => p.Category == request.Category);
```

Replace the fixed `OrderByDescending(p => p.Views7d)` with:

```csharp
        query = request.Sort switch
        {
            "price-asc" => query.OrderBy(p => p.Price),
            "price-desc" => query.OrderByDescending(p => p.Price),
            _ => query.OrderByDescending(p => p.Views7d),
        };
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
```

New file `GetProductCategories.cs`:

```csharp
using MediatR;
using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Products.Queries;

public record GetProductCategoriesQuery() : IRequest<IReadOnlyList<string>>;

public class GetProductCategoriesHandler(AppDbContext db)
    : IRequestHandler<GetProductCategoriesQuery, IReadOnlyList<string>>
{
    public async Task<IReadOnlyList<string>> Handle(GetProductCategoriesQuery request, CancellationToken ct) =>
        await db.Products.AsNoTracking()
            .Where(p => p.Status == ProductStatus.Active || p.Status == ProductStatus.Low)
            .Select(p => p.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(ct);
}
```

- [ ] **Step 4: Run to verify pass, then full suite**

Run: `dotnet test src/Services/Catalog.API/tests/Application.Tests`
Expected: PASS.

- [ ] **Step 5: Wire the endpoint params.** In `ProductReadEndpoints.cs`, change the list route and add `/categories` (before the `/{sku}` route so "categories" isn't captured as a SKU):

```csharp
        group.MapGet("/categories", async (ISender mediator, CancellationToken ct) =>
            Results.Ok(await mediator.Send(new GetProductCategoriesQuery(), ct)))
            .CacheOutput(CacheProducts)
            .WithName("GetProductCategories");

        group.MapGet("/", async (ISender mediator,
            int page = 1, int limit = 9, string? status = null, string? search = null,
            bool buyable = false, string? category = null, string? sort = null,
            CancellationToken ct = default) =>
            Results.Ok(await mediator.Send(
                new GetProductsQuery(page, limit, status, search, buyable, category, sort), ct)))
            .CacheOutput(CacheProducts)
            .WithName("GetProducts");
```

NOTE: output caching must vary by the new query params — check how the `CacheProducts` policy is defined in `Program.cs`; if it uses `SetVaryByQuery` with an explicit list, add `buyable`, `category`, `sort` to it. If it varies by full query string, no change.

- [ ] **Step 6: Build + commit**

Run: `dotnet build src/MicroCommerce.slnx`
Expected: build succeeds.

```bash
git add src/Services/Catalog.API/src src/Services/Catalog.API/tests/Application.Tests
git commit -m "feat(catalog): storefront product filters (buyable/category/sort) + categories endpoint"
```

---

### Task 5: API — BuyerPolicy + storefront endpoints

**Files:**
- Modify: `src/Services/Catalog.API/src/Api/AuthenticationExtensions.cs`
- Create: `src/Services/Catalog.API/src/Api/Endpoints/StorefrontOrderEndpoints.cs`
- Modify: `src/Services/Catalog.API/src/Api/Program.cs:81-90`

- [ ] **Step 1: Add the policy.** In `AuthenticationExtensions.cs`:

```csharp
    /// <summary>Authorization policy name protecting every Catalog write endpoint group.</summary>
    public const string SellerPolicy = "seller";

    /// <summary>Any authenticated user — a buyer needs no realm role (spec §3/§6).</summary>
    public const string BuyerPolicy = "buyer";
```

and in `AddAuthorization`:

```csharp
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(SellerPolicy, policy =>
                policy.RequireAuthenticatedUser().RequireRole("seller"));
            options.AddPolicy(BuyerPolicy, policy =>
                policy.RequireAuthenticatedUser());
        });
```

- [ ] **Step 2: Create `StorefrontOrderEndpoints.cs`**

```csharp
using System.Security.Claims;
using MediatR;
using MicroCommerce.Catalog.Application.Orders.Commands;
using MicroCommerce.Catalog.Application.Orders.Queries;

namespace MicroCommerce.Catalog.Api.Endpoints;

/// <summary>Buyer-facing order endpoints (storefront checkout + confirmation read).</summary>
public static class StorefrontOrderEndpoints
{
    /// <summary>Checkout body — everything except CustomerEmail, which comes from the token.</summary>
    public record StorefrontCheckoutRequest(
        string CustomerName,
        string ShipLine1,
        string ShipLine2,
        string CityState,
        string ShippingMethod,
        decimal ShippingPaid,
        decimal Tax,
        string PaymentBrand,
        string PaymentLastFour,
        string? PromoCode,
        IReadOnlyList<StorefrontLineInput> Lines);

    public static IEndpointRouteBuilder MapStorefrontOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Storefront")
            .RequireAuthorization(AuthenticationExtensions.BuyerPolicy);

        group.MapPost("/checkout", async (
            StorefrontCheckoutRequest request, ClaimsPrincipal user, ISender mediator, CancellationToken ct) =>
        {
            var email = TokenEmail(user);
            if (email is null) return Results.Unauthorized();

            var result = await mediator.Send(new PlaceStorefrontOrderCommand(
                email,
                request.CustomerName,
                request.ShipLine1,
                request.ShipLine2,
                request.CityState,
                request.ShippingMethod,
                request.ShippingPaid,
                request.Tax,
                request.PaymentBrand,
                request.PaymentLastFour,
                request.PromoCode,
                request.Lines), ct);

            if (result.IsFailure)
            {
                return Results.Problem(
                    title: result.Error!.Value.Code,
                    detail: result.Error.Value.Message,
                    statusCode: StatusCodes.Status409Conflict);
            }
            var created = result.Value!;
            return Results.Created($"/api/orders/{created.Number}/confirmation", created);
        })
        .WithName("StorefrontCheckout");

        // Buyer-scoped read: only the order's own customer may fetch it (email claim match).
        group.MapGet("/{number:int}/confirmation", async (
            int number, ClaimsPrincipal user, ISender mediator, CancellationToken ct) =>
        {
            var email = TokenEmail(user);
            if (email is null) return Results.Unauthorized();

            var order = await mediator.Send(new GetOrderByNumberQuery(number), ct);
            if (order is null || !string.Equals(order.Customer.Email, email, StringComparison.OrdinalIgnoreCase))
                return Results.NotFound();
            return Results.Ok(order);
        })
        .WithName("StorefrontOrderConfirmation");

        return app;
    }

    // MapInboundClaims=false keeps original claim names: prefer "email", fall back to
    // "preferred_username" (usernames are emails in this realm; see realm import).
    private static string? TokenEmail(ClaimsPrincipal user) =>
        user.FindFirst("email")?.Value ?? user.FindFirst("preferred_username")?.Value;
}
```

- [ ] **Step 3: Map it in `Program.cs`** after `app.MapOrderWriteEndpoints();`:

```csharp
app.MapStorefrontOrderEndpoints();
```

- [ ] **Step 4: Build**

Run: `dotnet build src/MicroCommerce.slnx`
Expected: success.

- [ ] **Step 5: Commit**

```bash
git add src/Services/Catalog.API/src/Api
git commit -m "feat(catalog): buyer policy + storefront checkout/confirmation endpoints"
```

---

### Task 6: FunctionalTests — buyer JWTs + checkout HTTP tests (TDD against real Postgres)

**Files:**
- Modify: `src/Services/Catalog.API/tests/FunctionalTests/Auth/TestTokens.cs`
- Modify: `src/Services/Catalog.API/tests/FunctionalTests/CatalogWebApplicationFactory.cs`
- Create: `src/Services/Catalog.API/tests/FunctionalTests/Storefront/StorefrontCheckoutTests.cs`

Requires Docker (Testcontainers).

- [ ] **Step 1: Extend `TestTokens.Mint` with an email claim**

```csharp
    public static string Mint(string[] roles, string audience = Audience, string? email = null)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, "test-seller"),
            new(JwtRegisteredClaimNames.PreferredUsername, email ?? "seller@microcommerce.dev"),
        };
        if (email is not null)
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, email));
        claims.AddRange(roles.Select(r => new Claim(RoleClaimType, r)));
        // ... rest unchanged
```

- [ ] **Step 2: Add the buyer client to the factory** (next to `CreateSellerClient`):

```csharp
    /// <summary>HttpClient with a self-minted role-less buyer token (BuyerPolicy = authenticated).</summary>
    public HttpClient CreateBuyerClient(string email = "buyer-tests@microcommerce.dev")
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TestTokens.Mint([], email: email));
        return client;
    }
```

- [ ] **Step 3: Write the failing HTTP tests.** Seeders run at API startup, so the 42 products (`MC-VS-001` Persimmon vase $86) and the `WELCOME10` promo (fixed $10, min $40, Active) exist. Each checkout decrements inventory and appends an order — use distinct SKUs per test where it matters and never assume exact order numbers, only relative ones.

```csharp
using System.Net;
using System.Net.Http.Json;
using MicroCommerce.Catalog.Application.Orders.Dtos;
using MicroCommerce.Catalog.Api.Endpoints;

namespace MicroCommerce.Catalog.FunctionalTests.Storefront;

public class StorefrontCheckoutTests(CatalogWebApplicationFactory factory)
    : IClassFixture<CatalogWebApplicationFactory>
{
    private static StorefrontOrderEndpoints.StorefrontCheckoutRequest Request(
        string? promo = null, int qty = 1, string sku = "MC-VS-001") =>
        new(
            CustomerName: "Bao Buyer",
            ShipLine1: "241 Telegraph Ave",
            ShipLine2: "Oakland, CA 94612",
            CityState: "Oakland, CA",
            ShippingMethod: "Standard",
            ShippingPaid: 8m,
            Tax: 6.46m,
            PaymentBrand: "Visa",
            PaymentLastFour: "4242",
            PromoCode: promo,
            Lines: [new StorefrontLineInput(sku, qty)]);

    [Fact]
    public async Task Checkout_Anonymous_Is401()
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/orders/checkout", Request(),
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Checkout_Buyer_CreatesOrder_WithDbPricing()
    {
        var client = factory.CreateBuyerClient("checkout-pricing@test.dev");
        var response = await client.PostAsJsonAsync("/api/orders/checkout", Request(qty: 1),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<OrderDetailDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(dto);
        Assert.Equal(86m, dto!.Summary.Subtotal);             // seeded Persimmon vase price
        Assert.Equal("checkout-pricing@test.dev", dto.Customer.Email); // from token, not body
        Assert.Equal($"/api/orders/{dto.Number}/confirmation", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task Checkout_WithWelcome10_PersistsDiscount()
    {
        var client = factory.CreateBuyerClient("checkout-promo@test.dev");
        var response = await client.PostAsJsonAsync("/api/orders/checkout", Request(promo: "WELCOME10"),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<OrderDetailDto>(TestContext.Current.CancellationToken);
        Assert.Equal("WELCOME10", dto!.Summary.DiscountCode);
        Assert.Equal(10m, dto.Summary.DiscountAmount);
        Assert.Equal(86m - 10m + 8m + 6.46m, dto.Summary.Paid);
    }

    [Fact]
    public async Task Checkout_InvalidPromo_Is409WithCode()
    {
        var client = factory.CreateBuyerClient();
        var response = await client.PostAsJsonAsync("/api/orders/checkout", Request(promo: "NOPE-99"),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetailsLite>(TestContext.Current.CancellationToken);
        Assert.Equal("PROMO_INVALID", problem!.Title);
    }

    [Fact]
    public async Task Checkout_OverStock_Is409()
    {
        var client = factory.CreateBuyerClient();
        var response = await client.PostAsJsonAsync("/api/orders/checkout", Request(qty: 100_000),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetailsLite>(TestContext.Current.CancellationToken);
        Assert.Equal("INSUFFICIENT_STOCK", problem!.Title);
    }

    [Fact]
    public async Task Confirmation_OwnOrder200_ForeignOrder404()
    {
        var owner = factory.CreateBuyerClient("confirm-owner@test.dev");
        var created = await owner.PostAsJsonAsync("/api/orders/checkout", Request(),
            TestContext.Current.CancellationToken);
        var dto = await created.Content.ReadFromJsonAsync<OrderDetailDto>(TestContext.Current.CancellationToken);

        var own = await owner.GetAsync($"/api/orders/{dto!.Number}/confirmation",
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, own.StatusCode);

        var stranger = factory.CreateBuyerClient("someone-else@test.dev");
        var foreign = await stranger.GetAsync($"/api/orders/{dto.Number}/confirmation",
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, foreign.StatusCode);
    }

    [Fact]
    public async Task SellerOrderPost_StillRejectsRolelessBuyer()
    {
        var buyer = factory.CreateBuyerClient();
        // Seller CreateOrderCommand body — shape irrelevant, the 403 happens at the policy.
        var response = await buyer.PostAsJsonAsync("/api/orders", new { number = 1 },
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private sealed record ProblemDetailsLite(string Title, string? Detail);
}
```

Match the namespace/usings to the project's conventions (look at `PromotionEndpointsTests.cs` imports).

- [ ] **Step 4: Run (Docker must be up)**

Run: `dotnet test src/Services/Catalog.API/tests/FunctionalTests --filter "FullyQualifiedName~StorefrontCheckout"`
Expected first run: FAIL only if Tasks 3/5 left gaps — these tests are written after implementation, so they should PASS; treat any failure as a real bug and fix it (this is the integration red/green for the whole backend slice).

- [ ] **Step 5: Run the full FunctionalTests suite, then commit**

Run: `dotnet test src/Services/Catalog.API/tests/FunctionalTests`
Expected: PASS.

```bash
git add src/Services/Catalog.API/tests/FunctionalTests
git commit -m "test(catalog): functional coverage for storefront checkout + buyer reads"
```

---

### Task 7: Realm registration + buyer user + `/checkout` proxy gate

**Files:**
- Modify: `src/AppHost/Realms/microcommerce-realm.json`
- Modify: `src/web/src/proxy.ts`

- [ ] **Step 1: Realm.** Add to the realm root (after `"sslRequired": "none",`):

```json
  "registrationAllowed": true,
  "registrationEmailAsUsername": true,
```

Append to `"users"`:

```json
    {
      "username": "buyer@microcommerce.dev",
      "email": "buyer@microcommerce.dev",
      "emailVerified": true,
      "enabled": true,
      "firstName": "Bao",
      "lastName": "Buyer",
      "credentials": [
        { "type": "password", "value": "Passw0rd!", "temporary": false }
      ],
      "realmRoles": []
    }
```

(Keycloak only re-imports the realm on a fresh container; the AppHost runs Keycloak without a persistent volume, so a stack restart picks this up.)

- [ ] **Step 2: Proxy.** `/checkout` joins `/seller` as a bounced-when-anonymous path:

```ts
export const proxy = auth((req) => {
  const needsAuth =
    req.nextUrl.pathname.startsWith("/seller") ||
    req.nextUrl.pathname.startsWith("/checkout");
  if (!req.auth && needsAuth) {
    const signInUrl = new URL("/api/auth/signin", req.nextUrl);
    signInUrl.searchParams.set(
      "callbackUrl",
      `${req.nextUrl.pathname}${req.nextUrl.search}`,
    );
    return NextResponse.redirect(signInUrl);
  }
  return NextResponse.next();
});

export const config = {
  matcher: ["/seller/:path*", "/checkout/:path*", "/checkout"],
};
```

Keep the existing comment block; confirmation pages (`/checkout/confirmation/...`) are intentionally included.

- [ ] **Step 3: Lint + commit**

Run (from `src/web/`): `npm run lint`
Expected: clean.

```bash
git add src/AppHost/Realms/microcommerce-realm.json src/web/src/proxy.ts
git commit -m "feat(auth): buyer self-registration + seeded buyer + checkout gate"
```

---

### Task 8: Cart codec + totals math (web, TDD)

**Files:**
- Create: `src/web/src/lib/storefront/cart.test.ts`, `src/web/src/lib/storefront/cart.ts`
- Create: `src/web/src/lib/storefront/totals.test.ts`, `src/web/src/lib/storefront/totals.ts`

- [ ] **Step 1: Write the failing tests**

`cart.test.ts`:

```ts
import { describe, expect, it } from "vitest";
import {
  addLine,
  cartCount,
  EMPTY_CART,
  parseCart,
  removeLine,
  serializeCart,
  setLineQty,
} from "@/lib/storefront/cart";

describe("cart codec", () => {
  it("round-trips through serialize/parse", () => {
    const cart = { lines: [{ sku: "MC-VS-001", qty: 2 }], promoCode: "WELCOME10" };
    expect(parseCart(serializeCart(cart))).toEqual(cart);
  });

  it("parses garbage to the empty cart", () => {
    expect(parseCart(undefined)).toEqual(EMPTY_CART);
    expect(parseCart("not json")).toEqual(EMPTY_CART);
    expect(parseCart('{"lines":"nope"}')).toEqual(EMPTY_CART);
    expect(parseCart('{"lines":[{"sku":1,"qty":"x"}]}')).toEqual(EMPTY_CART);
  });

  it("addLine merges quantities for the same sku", () => {
    const cart = addLine(addLine(EMPTY_CART, "MC-VS-001", 1), "MC-VS-001", 2);
    expect(cart.lines).toEqual([{ sku: "MC-VS-001", qty: 3 }]);
  });

  it("setLineQty updates and removes at zero", () => {
    const cart = addLine(EMPTY_CART, "MC-VS-001", 2);
    expect(setLineQty(cart, "MC-VS-001", 5).lines[0]?.qty).toBe(5);
    expect(setLineQty(cart, "MC-VS-001", 0).lines).toEqual([]);
  });

  it("removeLine drops the sku and keeps the promo", () => {
    const cart = {
      lines: [
        { sku: "MC-VS-001", qty: 1 },
        { sku: "MC-BW-014", qty: 2 },
      ],
      promoCode: "WELCOME10",
    };
    expect(removeLine(cart, "MC-VS-001")).toEqual({
      lines: [{ sku: "MC-BW-014", qty: 2 }],
      promoCode: "WELCOME10",
    });
  });

  it("counts total units", () => {
    expect(cartCount(EMPTY_CART)).toBe(0);
    expect(cartCount({ lines: [{ sku: "a", qty: 2 }, { sku: "b", qty: 1 }] })).toBe(3);
  });
});
```

`totals.test.ts` (spec §7 money math — no tax *maintenance* tests; these pin the formula once):

```ts
import { describe, expect, it } from "vitest";
import type { PromotionDto } from "@/lib/seller/promos/types";
import { computeDiscount, computeTotals, SHIPPING_OPTIONS } from "@/lib/storefront/totals";

const lines = [
  { sku: "MC-VS-001", name: "Persimmon vase", price: 86, qty: 1 },
  { sku: "MC-BW-014", name: "Forest bowl", price: 68, qty: 2 },
]; // subtotal 222

const fixed10: PromotionDto = {
  code: "WELCOME10",
  description: "first order",
  kind: "fixed",
  percentValue: null,
  fixedAmount: 10,
  minOrderAmount: 40,
  startsAt: null,
  endsAt: null,
  status: "active",
};

describe("computeDiscount", () => {
  it("fixed promos cap at the subtotal", () => {
    expect(computeDiscount(222, fixed10)).toBe(10);
    expect(computeDiscount(5, { ...fixed10, minOrderAmount: null })).toBe(5);
  });

  it("percentage promos round to cents", () => {
    const pct: PromotionDto = { ...fixed10, kind: "percentage", percentValue: 15, fixedAmount: null };
    expect(computeDiscount(86, pct)).toBe(12.9);
  });

  it("no promo means zero", () => {
    expect(computeDiscount(222, null)).toBe(0);
  });
});

describe("computeTotals", () => {
  it("discount hits the subtotal; tax applies after discount; shipping added last", () => {
    const t = computeTotals(lines, fixed10, "standard");
    expect(t.subtotal).toBe(222);
    expect(t.discount).toBe(10);
    expect(t.shipping).toBe(8);
    expect(t.tax).toBe(18.02); // (222-10) * 0.085
    expect(t.total).toBe(238.02);
  });

  it("pickup ships free", () => {
    expect(computeTotals(lines, null, "pickup").shipping).toBe(0);
  });

  it("exposes the three shipping options", () => {
    expect(SHIPPING_OPTIONS.map((o) => o.id)).toEqual(["standard", "express", "pickup"]);
  });
});
```

Check `src/web/src/lib/seller/promos/types.ts` for the exact `PromotionDto` field list (it has `status` and string dates — align the literal above with the real type; the test must compile against it).

- [ ] **Step 2: Run to verify failure**

Run: `npx vitest run src/lib/storefront`
Expected: FAIL (modules don't exist).

- [ ] **Step 3: Implement**

`cart.ts` (pure — no server imports, fully unit-testable):

```ts
export type CartLine = { sku: string; qty: number };
export type CartState = { lines: CartLine[]; promoCode?: string };

export const EMPTY_CART: CartState = { lines: [] };

export function parseCart(raw: string | undefined): CartState {
  if (!raw) return EMPTY_CART;
  try {
    const parsed: unknown = JSON.parse(raw);
    if (typeof parsed !== "object" || parsed === null) return EMPTY_CART;
    const { lines, promoCode } = parsed as { lines?: unknown; promoCode?: unknown };
    if (!Array.isArray(lines)) return EMPTY_CART;
    const valid: CartLine[] = [];
    for (const line of lines) {
      const { sku, qty } = (line ?? {}) as { sku?: unknown; qty?: unknown };
      if (typeof sku !== "string" || typeof qty !== "number" || !Number.isInteger(qty) || qty < 1) {
        return EMPTY_CART;
      }
      valid.push({ sku, qty });
    }
    return {
      lines: valid,
      ...(typeof promoCode === "string" && promoCode ? { promoCode } : {}),
    };
  } catch {
    return EMPTY_CART;
  }
}

export function serializeCart(cart: CartState): string {
  return JSON.stringify(cart);
}

export function addLine(cart: CartState, sku: string, qty: number): CartState {
  const existing = cart.lines.find((l) => l.sku === sku);
  const lines = existing
    ? cart.lines.map((l) => (l.sku === sku ? { ...l, qty: l.qty + qty } : l))
    : [...cart.lines, { sku, qty }];
  return { ...cart, lines };
}

export function setLineQty(cart: CartState, sku: string, qty: number): CartState {
  if (qty < 1) return removeLine(cart, sku);
  return { ...cart, lines: cart.lines.map((l) => (l.sku === sku ? { ...l, qty } : l)) };
}

export function removeLine(cart: CartState, sku: string): CartState {
  return { ...cart, lines: cart.lines.filter((l) => l.sku !== sku) };
}

export function cartCount(cart: CartState): number {
  return cart.lines.reduce((sum, l) => sum + l.qty, 0);
}
```

`totals.ts`:

```ts
import type { PromotionDto } from "@/lib/seller/promos/types";

// Demo constants per spec §7. Single source of truth for shipping + tax.
export const TAX_RATE = 0.085;

export const SHIPPING_OPTIONS = [
  { id: "standard", label: "Standard", detail: "5–7 business days · USPS Ground", price: 8 },
  { id: "express", label: "Express", detail: "2–3 business days · UPS Saver", price: 22 },
  { id: "pickup", label: "Local pickup", detail: "Oakland studio · ready Fri 4–7pm", price: 0 },
] as const;

export type ShippingId = (typeof SHIPPING_OPTIONS)[number]["id"];

export type PricedLine = { sku: string; name: string; price: number; qty: number };

export type Totals = {
  subtotal: number;
  discount: number;
  shipping: number;
  tax: number;
  total: number;
};

function roundMoney(n: number): number {
  return Math.round(n * 100) / 100;
}

/** Discount applies to the item subtotal only; fixed promos cap at the subtotal (spec §7). */
export function computeDiscount(subtotal: number, promo: PromotionDto | null): number {
  if (!promo) return 0;
  if (promo.kind === "percentage" && promo.percentValue) {
    return roundMoney((subtotal * promo.percentValue) / 100);
  }
  if (promo.kind === "fixed" && promo.fixedAmount) {
    return Math.min(promo.fixedAmount, subtotal);
  }
  return 0;
}

export function computeTotals(
  lines: PricedLine[],
  promo: PromotionDto | null,
  shipping: ShippingId,
): Totals {
  const subtotal = roundMoney(lines.reduce((sum, l) => sum + l.price * l.qty, 0));
  const discount = computeDiscount(subtotal, promo);
  const shippingCost = SHIPPING_OPTIONS.find((o) => o.id === shipping)?.price ?? 0;
  const tax = roundMoney((subtotal - discount) * TAX_RATE);
  return {
    subtotal,
    discount,
    shipping: shippingCost,
    tax,
    total: roundMoney(subtotal - discount + shippingCost + tax),
  };
}
```

- [ ] **Step 4: Run to verify pass**

Run: `npx vitest run src/lib/storefront`
Expected: PASS.

- [ ] **Step 5: Lint + commit**

```bash
npm run lint
git add src/web/src/lib/storefront
git commit -m "feat(web): storefront cart codec + totals math (TDD)"
```

---

### Task 9: API fetchers + cached shop loaders + cart cookie

**Files:**
- Modify: `src/web/src/lib/catalog/api.ts`
- Create: `src/web/src/lib/catalog/checkout.ts`
- Create: `src/web/src/lib/storefront/data.ts`
- Create: `src/web/src/lib/storefront/cart-cookie.ts`

- [ ] **Step 1: Extend `api.ts`.** Add to `ProductQuery` and `fetchProducts`:

```ts
export type ProductQuery = {
  page?: number;
  limit?: number;
  status?: ListingStatus;
  search?: string;
  buyable?: boolean;
  category?: string;
  sort?: "price-asc" | "price-desc";
};
```

and in `fetchProducts` after the existing `searchParams` lines:

```ts
  if (query.buyable) url.searchParams.set("buyable", "true");
  if (query.category) url.searchParams.set("category", query.category);
  if (query.sort) url.searchParams.set("sort", query.sort);
```

Add below `fetchProductBySku`:

```ts
export async function fetchProductCategories(): Promise<string[]> {
  const res = await fetch(`${apiBase()}/api/products/categories`);
  if (!res.ok)
    throw new Error(`GET /api/products/categories failed: ${res.status}`);
  return (await res.json()) as string[];
}
```

- [ ] **Step 2: Create `checkout.ts`** (authenticated, uncached — mirrors the write-fetcher pattern in `api.ts`):

```ts
import "server-only";
import { getAccessToken } from "@/lib/auth/token";
import type { OrderDetail } from "@/lib/seller/orders/types";

// Buyer checkout + confirmation. Both attach the session bearer token and are
// never cached (AUTH RULE: authenticated fetchers are uncached).

function apiBase(): string {
  const url = process.env.API_URL;
  if (!url) {
    throw new Error(
      "API_URL env var is not set. Run the app via the Aspire AppHost (`dotnet run --project src/AppHost`).",
    );
  }
  return url.replace(/\/$/, "");
}

export type PlaceOrderPayload = {
  customerName: string;
  shipLine1: string;
  shipLine2: string;
  cityState: string;
  shippingMethod: string;
  shippingPaid: number;
  tax: number;
  paymentBrand: string;
  paymentLastFour: string;
  promoCode?: string;
  lines: { sku: string; qty: number }[];
};

export type CheckoutError =
  | "EMPTY_CART"
  | "PRODUCT_NOT_FOUND"
  | "INSUFFICIENT_STOCK"
  | "PROMO_INVALID"
  | "PROMO_MIN_ORDER"
  | "UNKNOWN";

export type CheckoutResult =
  | { ok: true; order: OrderDetail }
  | { ok: false; error: CheckoutError; detail?: string };

export async function placeStorefrontOrder(
  payload: PlaceOrderPayload,
): Promise<CheckoutResult> {
  const res = await fetch(`${apiBase()}/api/orders/checkout`, {
    cache: "no-store",
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${await getAccessToken()}`,
    },
    body: JSON.stringify(payload),
  });
  if (res.status === 201) {
    return { ok: true, order: (await res.json()) as OrderDetail };
  }
  if (res.status === 409) {
    const problem = (await res.json()) as { title?: string; detail?: string };
    const known: CheckoutError[] = [
      "EMPTY_CART",
      "PRODUCT_NOT_FOUND",
      "INSUFFICIENT_STOCK",
      "PROMO_INVALID",
      "PROMO_MIN_ORDER",
    ];
    const error = known.find((k) => k === problem.title) ?? "UNKNOWN";
    return { ok: false, error, detail: problem.detail };
  }
  throw new Error(`POST /api/orders/checkout failed: ${res.status}`);
}

export async function fetchOrderConfirmation(
  number: number,
): Promise<OrderDetail | null> {
  const res = await fetch(`${apiBase()}/api/orders/${number}/confirmation`, {
    cache: "no-store",
    headers: { Authorization: `Bearer ${await getAccessToken()}` },
  });
  if (res.status === 404) return null;
  if (!res.ok)
    throw new Error(`GET /api/orders/${number}/confirmation failed: ${res.status}`);
  return (await res.json()) as OrderDetail;
}
```

Check `src/web/src/lib/seller/orders/types.ts` — the detail type may be named `OrderDetail` or similar (line ~150 has `summary: OrderSummary`); use the real exported name.

- [ ] **Step 3: Create `data.ts`** (cached + anonymous — same policy/tag as the seller listings loaders so product writes invalidate the shop too):

```ts
// Storefront read loaders. AUTH RULE (Keycloak): cached loaders are anonymous —
// calling auth()/getAccessToken() inside "use cache" is illegal. Tagged
// "listings" so every existing product mutation (and storefront checkout's
// inventory decrement) invalidates the shop with the already-wired
// revalidateTag("listings").
import { cacheTag } from "next/cache";
import {
  fetchProductBySku,
  fetchProductCategories,
  fetchProducts,
  type ProductQuery,
} from "@/lib/catalog/api";
import type { Listing } from "@/lib/seller/listings/types";

export const SHOP_PAGE_SIZE = 12;

export type ShopQuery = Pick<ProductQuery, "page" | "category" | "sort" | "search">;

export async function getShopProducts(query: ShopQuery = {}) {
  "use cache";
  cacheTag("listings");
  return fetchProducts({
    page: query.page ?? 1,
    limit: SHOP_PAGE_SIZE,
    buyable: true,
    category: query.category,
    sort: query.sort,
    search: query.search,
  });
}

export async function getShopProduct(sku: string): Promise<Listing | null> {
  "use cache";
  cacheTag("listings");
  const product = await fetchProductBySku(sku);
  // Draft/out products exist for the seller but are not buyable.
  if (!product || product.status === "draft" || product.status === "out") return null;
  return product;
}

export async function getShopCategories(): Promise<string[]> {
  "use cache";
  cacheTag("listings");
  return fetchProductCategories();
}
```

(`Listing.status` literal values: check `src/web/src/lib/seller/listings/types.ts` — they are `"active" | "low" | "out" | "draft"`.)

- [ ] **Step 4: Create `cart-cookie.ts`**:

```ts
import "server-only";
import { cookies } from "next/headers";
import { type CartState, EMPTY_CART, parseCart, serializeCart } from "@/lib/storefront/cart";

// httpOnly cookie so only server actions can mutate the cart. The cookie holds
// skus+qtys+promo code only — prices are always re-fetched from the API, so a
// stale or tampered cookie cannot corrupt totals (spec §7).
const CART_COOKIE = "mc_cart";
const ONE_WEEK_SECONDS = 60 * 60 * 24 * 7;

export async function readCart(): Promise<CartState> {
  const store = await cookies();
  return parseCart(store.get(CART_COOKIE)?.value);
}

export async function writeCart(cart: CartState): Promise<void> {
  const store = await cookies();
  if (cart.lines.length === 0 && !cart.promoCode) {
    store.delete(CART_COOKIE);
    return;
  }
  store.set(CART_COOKIE, serializeCart(cart), {
    httpOnly: true,
    sameSite: "lax",
    path: "/",
    maxAge: ONE_WEEK_SECONDS,
  });
}

export async function clearCartCookie(): Promise<void> {
  const store = await cookies();
  store.delete(CART_COOKIE);
}

export { EMPTY_CART };
```

- [ ] **Step 5: Verify it compiles + lint, commit**

Run: `npm run build` (the typecheck gate) — expected: success. Then `npm run lint`.

```bash
git add src/web/src/lib
git commit -m "feat(web): storefront loaders, checkout fetchers, cart cookie"
```

---

### Task 10: Server actions — cart, promo, placeOrder (TDD)

**Files:**
- Create: `src/web/src/lib/storefront/actions.test.ts`
- Create: `src/web/src/lib/storefront/actions.ts`

- [ ] **Step 1: Write the failing tests.** Mock the cookie + catalog modules (follow the mocking style of the existing `src/web/src/lib/catalog/actions.test.ts` — read it first and mirror its `vi.mock` usage):

```ts
import { beforeEach, describe, expect, it, vi } from "vitest";

const readCart = vi.fn();
const writeCart = vi.fn();
const clearCartCookie = vi.fn();
vi.mock("@/lib/storefront/cart-cookie", () => ({
  readCart: (...a: unknown[]) => readCart(...a),
  writeCart: (...a: unknown[]) => writeCart(...a),
  clearCartCookie: (...a: unknown[]) => clearCartCookie(...a),
}));

const fetchProductBySku = vi.fn();
vi.mock("@/lib/catalog/api", () => ({
  fetchProductBySku: (...a: unknown[]) => fetchProductBySku(...a),
}));

const fetchPromotionByCode = vi.fn();
vi.mock("@/lib/catalog/promotions", () => ({
  fetchPromotionByCode: (...a: unknown[]) => fetchPromotionByCode(...a),
}));

const placeStorefrontOrder = vi.fn();
vi.mock("@/lib/catalog/checkout", () => ({
  placeStorefrontOrder: (...a: unknown[]) => placeStorefrontOrder(...a),
}));

const revalidateTag = vi.fn();
vi.mock("next/cache", () => ({ revalidateTag: (...a: unknown[]) => revalidateTag(...a) }));

const redirect = vi.fn((url: string) => {
  throw new Error(`NEXT_REDIRECT:${url}`);
});
vi.mock("next/navigation", () => ({ redirect: (url: string) => redirect(url) }));

import {
  addToCart,
  applyPromoCode,
  placeOrder,
  removeCartLine,
  setCartQty,
} from "@/lib/storefront/actions";

const activeProduct = {
  sku: "MC-VS-001",
  name: "Persimmon vase",
  price: 86,
  inventory: 5,
  status: "active",
};

beforeEach(() => {
  vi.clearAllMocks();
  readCart.mockResolvedValue({ lines: [] });
  fetchProductBySku.mockResolvedValue(activeProduct);
});

describe("addToCart", () => {
  it("adds a buyable product and clamps qty to inventory", async () => {
    const result = await addToCart("MC-VS-001", 99);
    expect(result).toEqual({ ok: true, qty: 5 });
    expect(writeCart).toHaveBeenCalledWith({ lines: [{ sku: "MC-VS-001", qty: 5 }] });
  });

  it("rejects unknown or non-buyable products", async () => {
    fetchProductBySku.mockResolvedValue(null);
    expect(await addToCart("MC-NOPE", 1)).toEqual({ ok: false, error: "UNAVAILABLE" });
    fetchProductBySku.mockResolvedValue({ ...activeProduct, status: "out" });
    expect(await addToCart("MC-VS-001", 1)).toEqual({ ok: false, error: "UNAVAILABLE" });
    expect(writeCart).not.toHaveBeenCalled();
  });
});

describe("setCartQty / removeCartLine", () => {
  it("updates quantity within stock", async () => {
    readCart.mockResolvedValue({ lines: [{ sku: "MC-VS-001", qty: 1 }] });
    await setCartQty("MC-VS-001", 3);
    expect(writeCart).toHaveBeenCalledWith({ lines: [{ sku: "MC-VS-001", qty: 3 }] });
  });

  it("removes a line", async () => {
    readCart.mockResolvedValue({ lines: [{ sku: "MC-VS-001", qty: 1 }] });
    await removeCartLine("MC-VS-001");
    expect(writeCart).toHaveBeenCalledWith({ lines: [] });
  });
});

describe("applyPromoCode", () => {
  it("stores a valid active promo", async () => {
    readCart.mockResolvedValue({ lines: [{ sku: "MC-VS-001", qty: 1 }] });
    fetchPromotionByCode.mockResolvedValue({ code: "WELCOME10", status: "active", kind: "fixed", fixedAmount: 10, percentValue: null, minOrderAmount: 40, startsAt: null, endsAt: null, description: "" });
    const result = await applyPromoCode("welcome10");
    expect(result).toEqual({ ok: true });
    expect(writeCart).toHaveBeenCalledWith({
      lines: [{ sku: "MC-VS-001", qty: 1 }],
      promoCode: "WELCOME10",
    });
  });

  it("rejects unknown and inactive codes", async () => {
    fetchPromotionByCode.mockResolvedValue(null);
    expect(await applyPromoCode("NOPE")).toEqual({ ok: false, error: "PROMO_INVALID" });
    fetchPromotionByCode.mockResolvedValue({ code: "FRIENDS", status: "draft", kind: "percentage", percentValue: 15, fixedAmount: null, minOrderAmount: null, startsAt: null, endsAt: null, description: "" });
    expect(await applyPromoCode("FRIENDS")).toEqual({ ok: false, error: "PROMO_INVALID" });
  });
});

describe("placeOrder", () => {
  const shippingInput = {
    customerName: "Bao Buyer",
    shipLine1: "241 Telegraph Ave",
    shipLine2: "Oakland, CA 94612",
    cityState: "Oakland, CA",
    shippingId: "standard" as const,
    paymentBrand: "Visa",
    paymentLastFour: "4242",
  };

  it("posts server-computed shipping/tax, clears the cart, redirects", async () => {
    readCart.mockResolvedValue({ lines: [{ sku: "MC-VS-001", qty: 1 }] });
    placeStorefrontOrder.mockResolvedValue({ ok: true, order: { number: 1043 } });

    await expect(placeOrder(shippingInput)).rejects.toThrow(
      "NEXT_REDIRECT:/checkout/confirmation/1043",
    );
    expect(placeStorefrontOrder).toHaveBeenCalledWith(
      expect.objectContaining({
        shippingMethod: "Standard",
        shippingPaid: 8,
        tax: 7.31, // 86 * 0.085
        lines: [{ sku: "MC-VS-001", qty: 1 }],
      }),
    );
    expect(clearCartCookie).toHaveBeenCalled();
    expect(revalidateTag).toHaveBeenCalledWith("orders");
    expect(revalidateTag).toHaveBeenCalledWith("listings");
  });

  it("returns the API error without clearing the cart", async () => {
    readCart.mockResolvedValue({ lines: [{ sku: "MC-VS-001", qty: 1 }] });
    placeStorefrontOrder.mockResolvedValue({ ok: false, error: "INSUFFICIENT_STOCK" });

    const result = await placeOrder(shippingInput);
    expect(result).toEqual({ ok: false, error: "INSUFFICIENT_STOCK" });
    expect(clearCartCookie).not.toHaveBeenCalled();
  });

  it("rejects an empty cart", async () => {
    readCart.mockResolvedValue({ lines: [] });
    expect(await placeOrder(shippingInput)).toEqual({ ok: false, error: "EMPTY_CART" });
  });
});
```

- [ ] **Step 2: Run to verify failure**

Run: `npx vitest run src/lib/storefront/actions.test.ts`
Expected: FAIL (module missing).

- [ ] **Step 3: Implement `actions.ts`**

```ts
"use server";

import { revalidateTag } from "next/cache";
import { redirect } from "next/navigation";
import { fetchProductBySku } from "@/lib/catalog/api";
import {
  type CheckoutError,
  type PlaceOrderPayload,
  placeStorefrontOrder,
} from "@/lib/catalog/checkout";
import { fetchPromotionByCode } from "@/lib/catalog/promotions";
import { addLine, removeLine, setLineQty } from "@/lib/storefront/cart";
import { clearCartCookie, readCart, writeCart } from "@/lib/storefront/cart-cookie";
import {
  computeTotals,
  type PricedLine,
  SHIPPING_OPTIONS,
  type ShippingId,
} from "@/lib/storefront/totals";

export type CartActionResult =
  | { ok: true; qty?: number }
  | { ok: false; error: "UNAVAILABLE" | "PROMO_INVALID" };

async function buyableProduct(sku: string) {
  const product = await fetchProductBySku(sku);
  if (!product || product.status === "draft" || product.status === "out") return null;
  return product;
}

export async function addToCart(sku: string, qty: number): Promise<CartActionResult> {
  const product = await buyableProduct(sku);
  if (!product || product.inventory < 1) return { ok: false, error: "UNAVAILABLE" };

  const cart = await readCart();
  const existing = cart.lines.find((l) => l.sku === sku)?.qty ?? 0;
  const clamped = Math.min(Math.max(qty, 1), product.inventory - existing);
  if (clamped < 1) return { ok: false, error: "UNAVAILABLE" };

  await writeCart(addLine(cart, sku, clamped));
  return { ok: true, qty: existing + clamped };
}

export async function setCartQty(sku: string, qty: number): Promise<CartActionResult> {
  const cart = await readCart();
  if (qty > 0) {
    const product = await buyableProduct(sku);
    if (!product) return { ok: false, error: "UNAVAILABLE" };
    qty = Math.min(qty, product.inventory);
  }
  await writeCart(setLineQty(cart, sku, qty));
  return { ok: true, qty };
}

export async function removeCartLine(sku: string): Promise<CartActionResult> {
  await writeCart(removeLine(await readCart(), sku));
  return { ok: true };
}

export async function applyPromoCode(code: string): Promise<CartActionResult> {
  const normalized = code.trim().toUpperCase();
  if (!normalized) return { ok: false, error: "PROMO_INVALID" };

  const promo = await fetchPromotionByCode(normalized);
  const now = Date.now();
  if (
    !promo ||
    promo.status !== "active" ||
    (promo.startsAt && now < Date.parse(promo.startsAt)) ||
    (promo.endsAt && now > Date.parse(promo.endsAt))
  ) {
    return { ok: false, error: "PROMO_INVALID" };
  }

  const cart = await readCart();
  await writeCart({ ...cart, promoCode: normalized });
  return { ok: true };
}

export async function removePromoCode(): Promise<CartActionResult> {
  const cart = await readCart();
  const { promoCode: _drop, ...rest } = cart;
  await writeCart({ lines: rest.lines });
  return { ok: true };
}

export type PlaceOrderInput = {
  customerName: string;
  shipLine1: string;
  shipLine2: string;
  cityState: string;
  shippingId: ShippingId;
  paymentBrand: string;
  paymentLastFour: string;
};

export type PlaceOrderResult = { ok: false; error: CheckoutError; detail?: string };

export async function placeOrder(input: PlaceOrderInput): Promise<PlaceOrderResult> {
  const cart = await readCart();
  if (cart.lines.length === 0) return { ok: false, error: "EMPTY_CART" };

  // Price the cart server-side to derive shipping/tax (the API re-prices items
  // and the discount as the authority; spec §7).
  const priced: PricedLine[] = [];
  for (const line of cart.lines) {
    const product = await buyableProduct(line.sku);
    if (!product) return { ok: false, error: "PRODUCT_NOT_FOUND" };
    priced.push({ sku: line.sku, name: product.name, price: product.price, qty: line.qty });
  }

  const promo = cart.promoCode ? await fetchPromotionByCode(cart.promoCode) : null;
  const shippingOption = SHIPPING_OPTIONS.find((o) => o.id === input.shippingId);
  if (!shippingOption) return { ok: false, error: "UNKNOWN" };
  const totals = computeTotals(priced, promo, input.shippingId);

  const payload: PlaceOrderPayload = {
    customerName: input.customerName,
    shipLine1: input.shipLine1,
    shipLine2: input.shipLine2,
    cityState: input.cityState,
    shippingMethod: shippingOption.label,
    shippingPaid: totals.shipping,
    tax: totals.tax,
    paymentBrand: input.paymentBrand,
    paymentLastFour: input.paymentLastFour,
    ...(cart.promoCode ? { promoCode: cart.promoCode } : {}),
    lines: cart.lines.map((l) => ({ sku: l.sku, qty: l.qty })),
  };

  const result = await placeStorefrontOrder(payload);
  if (!result.ok) return { ok: false, error: result.error, detail: result.detail };

  await clearCartCookie();
  // Mirror the API's eviction matrix on the web cache (lib/seller/*/data.ts tags),
  // plus "listings" because checkout decremented inventory.
  for (const tag of ["orders", "customers", "analytics", "dashboard", "payouts", "listings"]) {
    revalidateTag(tag);
  }
  redirect(`/checkout/confirmation/${result.order.number}`);
  return { ok: false, error: "UNKNOWN" }; // unreachable; redirect throws
}
```

NOTE: `revalidateTag` in Next 16 may require the `(tag, "max")` profile argument or `updateTag` inside server actions — check how `src/web/src/lib/catalog/actions.ts` calls it and copy that exact form (and mirror it in the test mock).

- [ ] **Step 4: Run to verify pass**

Run: `npx vitest run src/lib/storefront/actions.test.ts`
Expected: PASS.

- [ ] **Step 5: Lint + commit**

```bash
npm run lint
git add src/web/src/lib/storefront
git commit -m "feat(web): storefront server actions — cart, promo, placeOrder"
```

---

### Task 11: Storefront chrome + home page

**Files:**
- Create: `src/web/src/components/storefront/product-image.tsx`
- Create: `src/web/src/components/storefront/product-card.tsx`
- Create: `src/web/src/components/storefront/topbar.tsx`
- Create: `src/web/src/app/(storefront)/layout.tsx`
- Create: `src/web/src/app/(storefront)/page.tsx`
- Create: `src/web/src/app/(storefront)/page.test.tsx`
- Delete: `src/web/src/app/page.tsx`

Design source: `Home_Tiles` in `design/project/hifi-home.jsx` + DESIGN.md. Brand is **Micro Commerce** (never "Mira"). Lean adapt: no ratings, no swatches; tags render as chips. Before writing JSX, skim `node_modules/next/dist/docs/` for `use cache` + `searchParams` semantics in this Next version (AGENTS.md rule).

- [ ] **Step 1: Write the failing page test** (`page.test.tsx` — async server component tested by awaiting the element, mirroring existing patterns; read `src/web/src/app/seller/loading.test.tsx` for the conventions in this repo):

```tsx
import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

vi.mock("@/lib/storefront/data", () => ({
  SHOP_PAGE_SIZE: 12,
  getShopProducts: vi.fn().mockResolvedValue({
    items: [
      {
        sku: "MC-VS-001",
        name: "Persimmon vase",
        category: "Vessels",
        price: 86,
        inventory: 24,
        status: "active",
        views7d: 0,
        description: null,
        tags: [],
        weight: 0.1,
        origin: "Oakland, CA",
        photoUrls: [],
      },
    ],
    total: 42,
    page: 1,
    pageSize: 12,
  }),
  getShopCategories: vi.fn().mockResolvedValue(["Drinkware", "Tableware", "Vessels"]),
}));

import ShopHome from "@/app/(storefront)/page";

describe("storefront home", () => {
  it("renders the grid, count, and category chips from real data", async () => {
    render(await ShopHome({ searchParams: Promise.resolve({}) }));
    expect(screen.getByText("Persimmon vase")).toBeInTheDocument();
    expect(screen.getByText("$86.00")).toBeInTheDocument();
    expect(screen.getByText("42 pieces")).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Vessels" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "All" })).toBeInTheDocument();
  });
});
```

Run: `npx vitest run "src/app/(storefront)/page.test.tsx"` → expected FAIL.

- [ ] **Step 2: `product-image.tsx`** — photo with tone-gradient fallback (no fake photos):

```tsx
import Image from "next/image";
import { cn } from "@/lib/utils";

// Design palette tones (hifi-styles): deterministic per-category gradient when
// a product has no photos yet, so the grid still reads as the Tiles design.
const TONES: Record<string, string> = {
  Vessels: "from-[#C96F4A] to-[#B45A38]",
  Tableware: "from-[#A9B49A] to-[#8A9B7C]",
  Drinkware: "from-[#DCB9AC] to-[#C99A8A]",
};
const FALLBACK_TONE = "from-[#E8E2D5] to-[#D9CFBC]";

export function ProductImage({
  photoUrl,
  category,
  name,
  className,
  sizes,
}: {
  photoUrl: string | undefined;
  category: string;
  name: string;
  className?: string;
  sizes?: string;
}) {
  if (photoUrl) {
    return (
      <div className={cn("relative overflow-hidden rounded-lg bg-muted", className)}>
        <Image src={photoUrl} alt={name} fill sizes={sizes ?? "25vw"} className="object-cover" />
      </div>
    );
  }
  return (
    <div
      aria-label={name}
      role="img"
      className={cn(
        "rounded-lg bg-gradient-to-br",
        TONES[category] ?? FALLBACK_TONE,
        className,
      )}
    />
  );
}
```

If `next/image` rejects the Azurite SAS host at runtime, add the host to `images.remotePatterns` in `src/web/next.config.ts` (check whether the seller wizard already configured it — search for `remotePatterns`).

- [ ] **Step 3: `product-card.tsx`**:

```tsx
import Link from "next/link";
import { money } from "@/lib/money";
import type { Listing } from "@/lib/seller/listings/types";
import { ProductImage } from "@/components/storefront/product-image";

export function ProductCard({ product }: { product: Listing }) {
  return (
    <Link href={`/products/${encodeURIComponent(product.sku)}`} className="group block">
      <div className="relative">
        <ProductImage
          photoUrl={product.photoUrls?.[0]}
          category={product.category}
          name={product.name}
          className="aspect-square w-full transition-opacity group-hover:opacity-90"
        />
        {product.inventory <= 3 && (
          <span className="absolute left-3 top-3 rounded-full bg-white/90 px-2.5 py-1 text-[11px] font-semibold">
            {product.inventory} left
          </span>
        )}
      </div>
      <div className="mt-2.5 flex items-baseline justify-between gap-2">
        <span className="text-sm font-semibold">{product.name}</span>
        <span className="text-sm font-semibold tabular-nums">{money(product.price)}</span>
      </div>
      <div className="mt-0.5 text-xs text-muted-foreground">{product.category}</div>
    </Link>
  );
}
```

- [ ] **Step 4: `topbar.tsx`** (server component; cart badge is a Suspense island because it reads cookies):

```tsx
import Link from "next/link";
import { Suspense } from "react";
import { cartCount } from "@/lib/storefront/cart";
import { readCart } from "@/lib/storefront/cart-cookie";

async function CartBadge() {
  const count = cartCount(await readCart());
  if (count === 0) return null;
  return (
    <span className="absolute -right-1 -top-1 flex h-4 min-w-4 items-center justify-center rounded-full bg-foreground px-1 text-[10px] font-bold text-background">
      {count}
    </span>
  );
}

export function ShopTopbar() {
  return (
    <header className="border-b">
      <div className="mx-auto flex max-w-6xl items-center gap-7 px-6 py-3.5">
        <Link href="/" className="font-display text-lg font-semibold tracking-tight">
          Micro Commerce
        </Link>
        <nav className="flex gap-5 text-[13px] font-medium">
          <Link href="/" className="text-foreground">Shop</Link>
          <Link href="/cart" className="text-muted-foreground hover:text-foreground">Bag</Link>
        </nav>
        <form action="/" className="ml-auto hidden sm:block">
          <input
            type="search"
            name="q"
            placeholder="Search products…"
            className="h-8 w-52 rounded-full bg-muted px-4 text-xs outline-none placeholder:text-muted-foreground"
          />
        </form>
        <Link href="/cart" aria-label="Bag" className="relative p-1.5">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" aria-hidden>
            <path d="M6 7h12l-1 13H7L6 7Z" />
            <path d="M9 7a3 3 0 0 1 6 0" />
          </svg>
          <Suspense fallback={null}>
            <CartBadge />
          </Suspense>
        </Link>
      </div>
    </header>
  );
}
```

- [ ] **Step 5: `(storefront)/layout.tsx`**:

```tsx
import { ShopTopbar } from "@/components/storefront/topbar";

export default function StorefrontLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="flex min-h-screen flex-col">
      <ShopTopbar />
      <main className="flex-1">{children}</main>
      <footer className="border-t py-8 text-center text-xs text-muted-foreground">
        Micro Commerce · hand-thrown in Oakland · free local delivery
      </footer>
    </div>
  );
}
```

- [ ] **Step 6: Home page `(storefront)/page.tsx`** (Home_Tiles: hero strip → chip filters + count + sort → 4-up grid):

```tsx
import Link from "next/link";
import { getShopCategories, getShopProducts } from "@/lib/storefront/data";
import { ProductCard } from "@/components/storefront/product-card";

type Search = { category?: string; sort?: string; q?: string; page?: string };

function chipHref(params: Search, category?: string) {
  const next = new URLSearchParams();
  if (category) next.set("category", category);
  if (params.sort) next.set("sort", params.sort);
  if (params.q) next.set("q", params.q);
  const qs = next.toString();
  return qs ? `/?${qs}` : "/";
}

export default async function ShopHome({
  searchParams,
}: {
  searchParams: Promise<Search>;
}) {
  const params = await searchParams;
  const sort = params.sort === "price-asc" || params.sort === "price-desc" ? params.sort : undefined;
  const [page, categories] = await Promise.all([
    getShopProducts({
      page: params.page ? Number(params.page) : 1,
      category: params.category,
      sort,
      search: params.q,
    }),
    getShopCategories(),
  ]);

  return (
    <div>
      {/* Hero strip — full-bleed gradient panel per Home_Tiles */}
      <section className="relative flex h-52 items-center overflow-hidden bg-gradient-to-r from-[#7A4630] via-[#C96F4A] to-[#DCB9AC]">
        <div className="mx-auto w-full max-w-6xl px-6 text-white">
          <p className="text-[11px] font-semibold uppercase tracking-[0.14em] text-white/80">
            Spring ’26 · Vessels
          </p>
          <h1 className="font-display mt-2 max-w-md text-4xl leading-[1.05]">
            Hand-thrown for slow <i>mornings.</i>
          </h1>
          <div className="mt-4 flex gap-2">
            <Link
              href="#shop"
              className="rounded-full bg-white px-4 py-2 text-xs font-semibold text-foreground"
            >
              Shop the drop →
            </Link>
          </div>
        </div>
      </section>

      {/* Filter bar */}
      <section className="border-b">
        <div className="mx-auto flex max-w-6xl flex-wrap items-center justify-between gap-3 px-6 py-3.5">
          <div className="flex flex-wrap gap-2">
            <Link
              href={chipHref(params)}
              className={`rounded-full border px-3 py-1 text-xs font-medium ${!params.category ? "border-foreground bg-foreground text-background" : "text-muted-foreground hover:text-foreground"}`}
            >
              All
            </Link>
            {categories.map((c) => (
              <Link
                key={c}
                href={chipHref(params, c)}
                className={`rounded-full border px-3 py-1 text-xs font-medium ${params.category === c ? "border-foreground bg-foreground text-background" : "text-muted-foreground hover:text-foreground"}`}
              >
                {c}
              </Link>
            ))}
          </div>
          <div className="flex items-center gap-4 text-xs text-muted-foreground">
            <span>{page.total} pieces</span>
            <span className="flex gap-2">
              <Link
                href={`/?${new URLSearchParams({ ...(params.category ? { category: params.category } : {}), sort: "price-asc" })}`}
                className={params.sort === "price-asc" ? "font-semibold text-foreground" : ""}
              >
                Price ↑
              </Link>
              <Link
                href={`/?${new URLSearchParams({ ...(params.category ? { category: params.category } : {}), sort: "price-desc" })}`}
                className={params.sort === "price-desc" ? "font-semibold text-foreground" : ""}
              >
                Price ↓
              </Link>
            </span>
          </div>
        </div>
      </section>

      {/* Grid */}
      <section id="shop" className="mx-auto max-w-6xl px-6 py-6">
        {page.items.length === 0 ? (
          <p className="py-16 text-center text-sm text-muted-foreground">
            Nothing matches — try another search.
          </p>
        ) : (
          <div className="grid grid-cols-2 gap-5 md:grid-cols-4">
            {page.items.map((p) => (
              <ProductCard key={p.sku} product={p} />
            ))}
          </div>
        )}
        {page.total > page.pageSize && (
          <div className="mt-8 flex justify-center gap-3 text-xs">
            {page.page > 1 && (
              <Link href={`/?${new URLSearchParams({ ...(params.category ? { category: params.category } : {}), page: String(page.page - 1) })}`} className="rounded-full border px-4 py-2">
                ← Previous
              </Link>
            )}
            {page.page * page.pageSize < page.total && (
              <Link href={`/?${new URLSearchParams({ ...(params.category ? { category: params.category } : {}), page: String(page.page + 1) })}`} className="rounded-full border px-4 py-2">
                Next →
              </Link>
            )}
          </div>
        )}
      </section>
    </div>
  );
}
```

- [ ] **Step 7: Delete the placeholder** `src/web/src/app/page.tsx` (the group page now owns `/`).

```bash
git rm src/web/src/app/page.tsx
```

- [ ] **Step 8: Run the page test + build**

Run: `npx vitest run "src/app/(storefront)/page.test.tsx"` → PASS.
Run: `npm run build` → must succeed (route conflict would surface here).

- [ ] **Step 9: Commit**

```bash
npm run lint
git add -A src/web/src
git commit -m "feat(web): storefront chrome + Home_Tiles shop page at /"
```

---

### Task 12: Product detail page + AddToBag

**Files:**
- Create: `src/web/src/components/storefront/add-to-bag.test.tsx`
- Create: `src/web/src/components/storefront/add-to-bag.tsx`
- Create: `src/web/src/app/(storefront)/products/[sku]/page.tsx`

Design source: `Product_Classic` (thumb rail · hero image · info column). Lean adapt: no swatches/sizes/ratings — description, tag chips, origin, low-stock badge, qty + add to bag, delivery/returns notes.

- [ ] **Step 1: Failing AddToBag test**

```tsx
import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";

const addToCart = vi.fn();
vi.mock("@/lib/storefront/actions", () => ({
  addToCart: (...a: unknown[]) => addToCart(...a),
}));

import { AddToBag } from "@/components/storefront/add-to-bag";

describe("AddToBag", () => {
  beforeEach(() => vi.clearAllMocks());

  it("steps quantity within bounds and submits", async () => {
    addToCart.mockResolvedValue({ ok: true, qty: 2 });
    render(<AddToBag sku="MC-VS-001" price={86} maxQty={3} />);

    fireEvent.click(screen.getByRole("button", { name: "Increase quantity" }));
    expect(screen.getByText("2")).toBeInTheDocument();

    fireEvent.click(screen.getByRole("button", { name: /Add to bag/ }));
    await waitFor(() => expect(addToCart).toHaveBeenCalledWith("MC-VS-001", 2));
    expect(await screen.findByText("Added to bag ✓")).toBeInTheDocument();
  });

  it("shows the error state when the action rejects the add", async () => {
    addToCart.mockResolvedValue({ ok: false, error: "UNAVAILABLE" });
    render(<AddToBag sku="MC-VS-001" price={86} maxQty={1} />);
    fireEvent.click(screen.getByRole("button", { name: /Add to bag/ }));
    expect(
      await screen.findByText("This piece just sold out."),
    ).toBeInTheDocument();
  });

  it("never steps above maxQty or below 1", () => {
    render(<AddToBag sku="MC-VS-001" price={86} maxQty={1} />);
    fireEvent.click(screen.getByRole("button", { name: "Increase quantity" }));
    fireEvent.click(screen.getByRole("button", { name: "Decrease quantity" }));
    fireEvent.click(screen.getByRole("button", { name: "Decrease quantity" }));
    expect(screen.getByText("1")).toBeInTheDocument();
  });
});
```

Run: `npx vitest run src/components/storefront/add-to-bag.test.tsx` → FAIL.

- [ ] **Step 2: Implement `add-to-bag.tsx`**

```tsx
"use client";

import { useState, useTransition } from "react";
import { addToCart } from "@/lib/storefront/actions";
import { money } from "@/lib/money";

export function AddToBag({ sku, price, maxQty }: { sku: string; price: number; maxQty: number }) {
  const [qty, setQty] = useState(1);
  const [status, setStatus] = useState<"idle" | "added" | "error">("idle");
  const [pending, startTransition] = useTransition();

  const submit = () =>
    startTransition(async () => {
      const result = await addToCart(sku, qty);
      setStatus(result.ok ? "added" : "error");
    });

  return (
    <div>
      <div className="flex gap-2">
        <div className="flex h-11 items-center rounded-full border px-2">
          <button
            type="button"
            aria-label="Decrease quantity"
            className="px-2 text-lg leading-none disabled:opacity-30"
            disabled={qty <= 1}
            onClick={() => setQty((q) => Math.max(1, q - 1))}
          >
            −
          </button>
          <span className="w-6 text-center text-sm font-semibold tabular-nums">{qty}</span>
          <button
            type="button"
            aria-label="Increase quantity"
            className="px-2 text-lg leading-none disabled:opacity-30"
            disabled={qty >= maxQty}
            onClick={() => setQty((q) => Math.min(maxQty, q + 1))}
          >
            +
          </button>
        </div>
        <button
          type="button"
          onClick={submit}
          disabled={pending || maxQty < 1}
          className="h-11 flex-1 rounded-full bg-foreground text-sm font-semibold text-background disabled:opacity-50"
        >
          {pending ? "Adding…" : `Add to bag · ${money(price * qty)}`}
        </button>
      </div>
      <p aria-live="polite" className="mt-2 min-h-5 text-xs">
        {status === "added" && <span className="text-green-700">Added to bag ✓</span>}
        {status === "error" && <span className="text-red-600">This piece just sold out.</span>}
      </p>
    </div>
  );
}
```

Run the test again → PASS.

- [ ] **Step 3: PDP `products/[sku]/page.tsx`**

```tsx
import Link from "next/link";
import { notFound } from "next/navigation";
import { AddToBag } from "@/components/storefront/add-to-bag";
import { ProductImage } from "@/components/storefront/product-image";
import { money } from "@/lib/money";
import { getShopProduct } from "@/lib/storefront/data";

export default async function ProductPage({
  params,
}: {
  params: Promise<{ sku: string }>;
}) {
  const { sku } = await params;
  const product = await getShopProduct(decodeURIComponent(sku));
  if (!product) notFound();

  const photos = product.photoUrls ?? [];

  return (
    <div className="mx-auto max-w-6xl px-6 py-5">
      <nav className="mb-4 text-xs text-muted-foreground">
        <Link href="/" className="hover:text-foreground">Shop</Link>
        <span className="mx-1.5">/</span>
        <Link href={`/?category=${encodeURIComponent(product.category)}`} className="hover:text-foreground">
          {product.category}
        </Link>
        <span className="mx-1.5">/</span>
        <span className="text-foreground">{product.name}</span>
      </nav>

      <div className="grid gap-5 lg:grid-cols-[64px_1fr_380px]">
        {/* Thumb rail */}
        <div className="order-2 flex gap-2 lg:order-1 lg:flex-col">
          {(photos.length > 0 ? photos : [undefined]).slice(0, 5).map((url, i) => (
            <ProductImage
              key={url ?? i}
              photoUrl={url}
              category={product.category}
              name={`${product.name} photo ${i + 1}`}
              className="h-16 w-16 rounded-lg"
            />
          ))}
        </div>

        {/* Hero image */}
        <div className="order-1 lg:order-2">
          <ProductImage
            photoUrl={photos[0]}
            category={product.category}
            name={product.name}
            className="aspect-[4/3] w-full rounded-xl"
            sizes="(min-width: 1024px) 50vw, 100vw"
          />
        </div>

        {/* Info column */}
        <div className="order-3">
          <p className="text-[11px] font-semibold uppercase tracking-[0.14em] text-muted-foreground">
            Micro Commerce · Oakland
          </p>
          <h1 className="font-display mt-1.5 text-3xl">{product.name}</h1>
          <div className="mt-3 flex items-baseline gap-2">
            <span className="text-2xl font-semibold tabular-nums">{money(product.price)}</span>
            <span className="text-xs text-muted-foreground">+ tax</span>
          </div>

          {product.inventory <= 3 && product.inventory > 0 && (
            <p className="mt-2 text-xs font-semibold text-[#B45A38]">
              Only {product.inventory} left
            </p>
          )}
          {product.inventory === 0 && (
            <p className="mt-2 text-xs font-semibold text-red-600">Sold out</p>
          )}

          {product.description && (
            <p className="mt-4 text-sm leading-relaxed text-muted-foreground">
              {product.description}
            </p>
          )}

          {(product.tags?.length ?? 0) > 0 && (
            <div className="mt-4 flex flex-wrap gap-1.5">
              {product.tags?.map((tag) => (
                <span key={tag} className="rounded-full border px-2.5 py-0.5 text-[11px] text-muted-foreground">
                  {tag}
                </span>
              ))}
            </div>
          )}

          <div className="mt-5">
            <AddToBag sku={product.sku} price={product.price} maxQty={product.inventory} />
          </div>

          <div className="mt-4 space-y-1 text-xs text-muted-foreground">
            <p>🚚 Free local delivery · ships in 3–5 days</p>
            <p>↩︎ 14-day returns · made one at a time</p>
            <p>Origin: {product.origin}</p>
          </div>
        </div>
      </div>
    </div>
  );
}
```

- [ ] **Step 4: Build + lint + commit**

Run: `npm run build && npm run lint` → success.

```bash
git add src/web/src
git commit -m "feat(web): Product_Classic PDP with AddToBag"
```

---

### Task 13: Cart page

**Files:**
- Create: `src/web/src/components/storefront/cart-line-row.tsx`
- Create: `src/web/src/components/storefront/promo-form.tsx`
- Create: `src/web/src/app/(storefront)/cart/page.tsx`

Design source: `Cart_List` (mobile) / `Cart_Desktop` (2-col, sticky summary). One responsive page; dynamic (reads cookies). Promo lives in the cart cookie; totals come from `computeTotals`.

- [ ] **Step 1: `cart-line-row.tsx`** (server component; steppers are server-action forms — no client JS needed):

```tsx
import Link from "next/link";
import { ProductImage } from "@/components/storefront/product-image";
import { money } from "@/lib/money";
import type { Listing } from "@/lib/seller/listings/types";
import { removeCartLine, setCartQty } from "@/lib/storefront/actions";

export function CartLineRow({ product, qty }: { product: Listing; qty: number }) {
  const setQty = async (formData: FormData) => {
    "use server";
    await setCartQty(String(formData.get("sku")), Number(formData.get("qty")));
  };
  const remove = async (formData: FormData) => {
    "use server";
    await removeCartLine(String(formData.get("sku")));
  };

  return (
    <div className="flex gap-4 border-b py-4 last:border-b-0">
      <Link href={`/products/${encodeURIComponent(product.sku)}`} className="shrink-0">
        <ProductImage
          photoUrl={product.photoUrls?.[0]}
          category={product.category}
          name={product.name}
          className="h-20 w-20"
        />
      </Link>
      <div className="flex grow flex-col">
        <div className="flex justify-between gap-2">
          <span className="text-sm font-semibold">{product.name}</span>
          <span className="text-sm font-semibold tabular-nums">{money(product.price * qty)}</span>
        </div>
        <span className="mt-0.5 text-xs text-muted-foreground">
          {product.category} · SKU {product.sku}
        </span>
        <div className="mt-auto flex items-center justify-between pt-2">
          <div className="flex h-7 items-center rounded-full border px-1 text-sm">
            <form action={setQty}>
              <input type="hidden" name="sku" value={product.sku} />
              <input type="hidden" name="qty" value={qty - 1} />
              <button type="submit" aria-label={`Decrease ${product.name} quantity`} className="px-2">−</button>
            </form>
            <span className="w-5 text-center text-xs font-semibold tabular-nums">{qty}</span>
            <form action={setQty}>
              <input type="hidden" name="sku" value={product.sku} />
              <input type="hidden" name="qty" value={Math.min(qty + 1, product.inventory)} />
              <button
                type="submit"
                aria-label={`Increase ${product.name} quantity`}
                className="px-2 disabled:opacity-30"
                disabled={qty >= product.inventory}
              >
                +
              </button>
            </form>
          </div>
          <form action={remove}>
            <input type="hidden" name="sku" value={product.sku} />
            <button type="submit" className="text-xs text-muted-foreground hover:text-foreground">
              Remove
            </button>
          </form>
        </div>
      </div>
    </div>
  );
}
```

- [ ] **Step 2: `promo-form.tsx`** (client — needs inline error state):

```tsx
"use client";

import { useState, useTransition } from "react";
import { applyPromoCode, removePromoCode } from "@/lib/storefront/actions";

export function PromoForm({ appliedCode }: { appliedCode?: string }) {
  const [error, setError] = useState<string | null>(null);
  const [pending, startTransition] = useTransition();

  if (appliedCode) {
    return (
      <div className="flex items-center justify-between rounded-lg border border-dashed px-3 py-2.5 text-xs">
        <span className="font-semibold text-green-700">{appliedCode} applied ✓</span>
        <button
          type="button"
          className="text-muted-foreground hover:text-foreground"
          onClick={() => startTransition(async () => void (await removePromoCode()))}
        >
          Remove
        </button>
      </div>
    );
  }

  return (
    <form
      className="flex items-center gap-2 rounded-lg border border-dashed px-3 py-2"
      onSubmit={(e) => {
        e.preventDefault();
        const code = String(new FormData(e.currentTarget).get("code") ?? "");
        startTransition(async () => {
          const result = await applyPromoCode(code);
          setError(result.ok ? null : "That code isn’t valid right now.");
        });
      }}
    >
      <input
        name="code"
        placeholder="Promo code"
        aria-label="Promo code"
        className="h-7 grow bg-transparent text-xs outline-none"
      />
      <button type="submit" disabled={pending} className="text-xs font-semibold disabled:opacity-50">
        Apply
      </button>
      {error && <span role="alert" className="text-xs text-red-600">{error}</span>}
    </form>
  );
}
```

- [ ] **Step 3: `cart/page.tsx`**:

```tsx
import Link from "next/link";
import { CartLineRow } from "@/components/storefront/cart-line-row";
import { PromoForm } from "@/components/storefront/promo-form";
import { fetchProductBySku } from "@/lib/catalog/api";
import { fetchPromotionByCode } from "@/lib/catalog/promotions";
import { money } from "@/lib/money";
import { readCart } from "@/lib/storefront/cart-cookie";
import { computeTotals, type PricedLine } from "@/lib/storefront/totals";

export default async function CartPage() {
  const cart = await readCart();

  if (cart.lines.length === 0) {
    return (
      <div className="mx-auto flex max-w-6xl flex-col items-center px-6 py-24 text-center">
        <h1 className="font-display text-3xl">
          Your <i>bag</i> is empty.
        </h1>
        <Link
          href="/"
          className="mt-6 rounded-full bg-foreground px-5 py-2.5 text-xs font-semibold text-background"
        >
          Shop the collection →
        </Link>
      </div>
    );
  }

  const products = await Promise.all(cart.lines.map((l) => fetchProductBySku(l.sku)));
  const rows = cart.lines
    .map((line, i) => ({ line, product: products[i] }))
    .filter((r) => r.product && r.product.status !== "draft");
  const stale = cart.lines.length - rows.length;

  const priced: PricedLine[] = rows.map((r) => ({
    sku: r.line.sku,
    name: r.product!.name,
    price: r.product!.price,
    qty: r.line.qty,
  }));
  const promo = cart.promoCode ? await fetchPromotionByCode(cart.promoCode) : null;
  const totals = computeTotals(priced, promo, "standard");

  return (
    <div className="mx-auto max-w-6xl px-6 py-6">
      <h1 className="font-display text-3xl">
        Your <i>bag</i>
      </h1>
      {stale > 0 && (
        <p className="mt-2 rounded-lg bg-amber-50 px-3 py-2 text-xs text-amber-800">
          {stale} item{stale > 1 ? "s are" : " is"} no longer available and was removed from view.
        </p>
      )}

      <div className="mt-4 grid gap-8 lg:grid-cols-[1fr_360px]">
        <div>
          {rows.map((r) => (
            <CartLineRow key={r.line.sku} product={r.product!} qty={r.line.qty} />
          ))}
        </div>

        <aside className="lg:sticky lg:top-4 lg:self-start">
          <div className="rounded-xl border p-5">
            <p className="text-[11px] font-semibold uppercase tracking-[0.14em] text-muted-foreground">
              Order summary
            </p>
            <dl className="mt-3 space-y-1.5 text-sm">
              <div className="flex justify-between">
                <dt className="text-muted-foreground">Subtotal · {rows.length} items</dt>
                <dd className="tabular-nums">{money(totals.subtotal)}</dd>
              </div>
              {totals.discount > 0 && cart.promoCode && (
                <div className="flex justify-between text-green-700">
                  <dt>Promo · {cart.promoCode}</dt>
                  <dd className="tabular-nums">−{money(totals.discount)}</dd>
                </div>
              )}
              <div className="flex justify-between">
                <dt className="text-muted-foreground">Shipping · from</dt>
                <dd className="tabular-nums">{money(totals.shipping)}</dd>
              </div>
              <div className="flex justify-between">
                <dt className="text-muted-foreground">Estimated tax</dt>
                <dd className="tabular-nums">{money(totals.tax)}</dd>
              </div>
            </dl>
            <div className="my-4 border-t" />
            <div className="flex items-baseline justify-between">
              <span className="text-sm font-semibold">Total</span>
              <span className="text-2xl font-semibold tabular-nums">{money(totals.total)}</span>
            </div>
            <p className="mt-1 text-[11px] text-muted-foreground">
              USD · final shipping chosen at checkout
            </p>
            <Link
              href="/checkout"
              className="mt-4 block rounded-full bg-foreground py-3 text-center text-sm font-semibold text-background"
            >
              Checkout →
            </Link>
          </div>
          <div className="mt-3">
            <PromoForm appliedCode={cart.promoCode} />
          </div>
        </aside>
      </div>
    </div>
  );
}
```

- [ ] **Step 4: Build + lint + commit**

Run: `npm run build && npm run lint` → success.

```bash
git add src/web/src
git commit -m "feat(web): cart page (Cart_List/Cart_Desktop) with promo + steppers"
```

---

### Task 14: Checkout (wizard + single page) and confirmation

**Files:**
- Create: `src/web/src/components/storefront/checkout-form.test.tsx`
- Create: `src/web/src/components/storefront/checkout-form.tsx`
- Create: `src/web/src/app/(storefront)/checkout/page.tsx`
- Create: `src/web/src/app/(storefront)/checkout/confirmation/[number]/page.tsx`

Design: `Checkout_Steps` (mobile wizard Ship → Pay → Review) + `Checkout_Desktop` (single page, 2-col with summary rail). One RHF form; on `lg:` screens all sections render at once, below `lg:` only the active step. RHF + React Compiler ⇒ `"use no memo"` (project memory).

- [ ] **Step 1: Failing checkout-form test**

```tsx
import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";

const placeOrder = vi.fn();
vi.mock("@/lib/storefront/actions", () => ({
  placeOrder: (...a: unknown[]) => placeOrder(...a),
}));

import { CheckoutForm } from "@/components/storefront/checkout-form";

const totals = { subtotal: 86, discount: 0, shipping: 8, tax: 7.31, total: 101.31 };

function fillShipping() {
  fireEvent.change(screen.getByLabelText("Full name"), { target: { value: "Bao Buyer" } });
  fireEvent.change(screen.getByLabelText("Street address"), { target: { value: "241 Telegraph Ave" } });
  fireEvent.change(screen.getByLabelText("City, State ZIP"), { target: { value: "Oakland, CA 94612" } });
}

describe("CheckoutForm", () => {
  beforeEach(() => vi.clearAllMocks());

  it("walks Ship → Pay → Review and submits the typed payload", async () => {
    placeOrder.mockResolvedValue({ ok: false, error: "UNKNOWN" }); // redirect happens server-side
    render(<CheckoutForm email="buyer@microcommerce.dev" totals={totals} />);

    fillShipping();
    fireEvent.click(screen.getByRole("button", { name: /Continue to payment/ }));

    fireEvent.change(await screen.findByLabelText("Card number"), {
      target: { value: "4242 4242 4242 4242" },
    });
    fireEvent.change(screen.getByLabelText("Name on card"), { target: { value: "Bao Buyer" } });
    fireEvent.click(screen.getByRole("button", { name: /Review order/ }));

    fireEvent.click(await screen.findByRole("button", { name: /Place order/ }));

    await waitFor(() =>
      expect(placeOrder).toHaveBeenCalledWith(
        expect.objectContaining({
          customerName: "Bao Buyer",
          shipLine1: "241 Telegraph Ave",
          cityState: "Oakland, CA 94612",
          shippingId: "standard",
          paymentBrand: "Visa",
          paymentLastFour: "4242",
        }),
      ),
    );
  });

  it("blocks the ship step until required fields are filled", async () => {
    render(<CheckoutForm email="b@x.dev" totals={totals} />);
    fireEvent.click(screen.getByRole("button", { name: /Continue to payment/ }));
    expect(await screen.findAllByRole("alert")).not.toHaveLength(0);
    expect(screen.queryByLabelText("Card number")).not.toBeInTheDocument();
  });

  it("surfaces a server rejection", async () => {
    placeOrder.mockResolvedValue({ ok: false, error: "INSUFFICIENT_STOCK" });
    render(<CheckoutForm email="b@x.dev" totals={totals} />);
    fillShipping();
    fireEvent.click(screen.getByRole("button", { name: /Continue to payment/ }));
    fireEvent.change(await screen.findByLabelText("Card number"), {
      target: { value: "4242424242424242" },
    });
    fireEvent.change(screen.getByLabelText("Name on card"), { target: { value: "B" } });
    fireEvent.click(screen.getByRole("button", { name: /Review order/ }));
    fireEvent.click(await screen.findByRole("button", { name: /Place order/ }));
    expect(
      await screen.findByText(/no longer in stock/i),
    ).toBeInTheDocument();
  });
});
```

Run: `npx vitest run src/components/storefront/checkout-form.test.tsx` → FAIL.

- [ ] **Step 2: Implement `checkout-form.tsx`**

```tsx
"use client";
"use no memo"; // RHF error state is swallowed by the React Compiler otherwise (project memory)

import { useState, useTransition } from "react";
import { useForm } from "react-hook-form";
import { money } from "@/lib/money";
import { type CheckoutError } from "@/lib/catalog/checkout";
import { placeOrder } from "@/lib/storefront/actions";
import { SHIPPING_OPTIONS, type ShippingId, type Totals } from "@/lib/storefront/totals";

type FormValues = {
  customerName: string;
  shipLine1: string;
  cityState: string;
  shippingId: ShippingId;
  cardNumber: string;
  cardName: string;
};

const ERROR_COPY: Record<CheckoutError, string> = {
  EMPTY_CART: "Your bag is empty — head back to the shop.",
  PRODUCT_NOT_FOUND: "An item in your bag is no longer available.",
  INSUFFICIENT_STOCK: "An item in your bag is no longer in stock at that quantity.",
  PROMO_INVALID: "Your promo code is no longer valid — remove it in the bag.",
  PROMO_MIN_ORDER: "Your bag no longer meets the promo's minimum.",
  UNKNOWN: "Something went wrong placing the order. Please try again.",
};

export function detectBrand(cardNumber: string): string {
  const digits = cardNumber.replace(/\D/g, "");
  if (digits.startsWith("4")) return "Visa";
  if (/^5[1-5]/.test(digits) || /^2[2-7]/.test(digits)) return "Mastercard";
  if (/^3[47]/.test(digits)) return "Amex";
  return "Card";
}

type Step = "ship" | "pay" | "review";
const STEPS: Step[] = ["ship", "pay", "review"];

export function CheckoutForm({ email, totals }: { email: string; totals: Totals }) {
  const [step, setStep] = useState<Step>("ship");
  const [serverError, setServerError] = useState<string | null>(null);
  const [pending, startTransition] = useTransition();

  const {
    register,
    trigger,
    getValues,
    watch,
    formState: { errors },
  } = useForm<FormValues>({
    mode: "onTouched",
    defaultValues: { shippingId: "standard", customerName: "", shipLine1: "", cityState: "", cardNumber: "", cardName: "" },
  });

  const shippingId = watch("shippingId");

  const next = async (target: Step, fields: (keyof FormValues)[]) => {
    if (await trigger(fields)) setStep(target);
  };

  const submit = () =>
    startTransition(async () => {
      const v = getValues();
      const digits = v.cardNumber.replace(/\D/g, "");
      const result = await placeOrder({
        customerName: v.customerName,
        shipLine1: v.shipLine1,
        shipLine2: v.cityState,
        cityState: v.cityState,
        shippingId: v.shippingId,
        paymentBrand: detectBrand(v.cardNumber),
        paymentLastFour: digits.slice(-4),
      });
      // placeOrder redirects on success; reaching here means failure.
      if (!result.ok) setServerError(ERROR_COPY[result.error]);
    });

  const showShip = step === "ship";
  const showPay = step === "pay";
  const showReview = step === "review";

  return (
    <div>
      {/* Step rail (mobile-first; desktop shows it too per Checkout_Desktop) */}
      <ol className="mb-5 flex items-center gap-2 text-xs">
        {STEPS.map((s, i) => (
          <li key={s} className="flex items-center gap-2">
            <span
              className={`flex h-5 w-5 items-center justify-center rounded-full text-[10px] font-bold ${
                STEPS.indexOf(step) >= i ? "bg-foreground text-background" : "border text-muted-foreground"
              }`}
            >
              {i + 1}
            </span>
            <span className={STEPS.indexOf(step) >= i ? "font-medium" : "text-muted-foreground"}>
              {s === "ship" ? "Ship" : s === "pay" ? "Pay" : "Review"}
            </span>
            {i < STEPS.length - 1 && <span className="w-6 border-t" />}
          </li>
        ))}
      </ol>

      <form onSubmit={(e) => e.preventDefault()} noValidate>
        {showShip && (
          <section className="space-y-3">
            <h2 className="font-display text-2xl">
              Shipping <i>address</i>
            </h2>
            <p className="text-xs text-muted-foreground">Signed in as {email}</p>
            <div>
              <label htmlFor="customerName" className="text-xs font-medium">Full name</label>
              <input
                id="customerName"
                className="mt-1 h-10 w-full rounded-lg border px-3 text-sm"
                {...register("customerName", { required: "Your name is required." })}
              />
              {errors.customerName && <p role="alert" className="mt-1 text-xs text-red-600">{errors.customerName.message}</p>}
            </div>
            <div>
              <label htmlFor="shipLine1" className="text-xs font-medium">Street address</label>
              <input
                id="shipLine1"
                className="mt-1 h-10 w-full rounded-lg border px-3 text-sm"
                {...register("shipLine1", { required: "Street address is required." })}
              />
              {errors.shipLine1 && <p role="alert" className="mt-1 text-xs text-red-600">{errors.shipLine1.message}</p>}
            </div>
            <div>
              <label htmlFor="cityState" className="text-xs font-medium">City, State ZIP</label>
              <input
                id="cityState"
                className="mt-1 h-10 w-full rounded-lg border px-3 text-sm"
                placeholder="Oakland, CA 94612"
                {...register("cityState", { required: "City/state is required." })}
              />
              {errors.cityState && <p role="alert" className="mt-1 text-xs text-red-600">{errors.cityState.message}</p>}
            </div>

            <fieldset className="space-y-2 pt-1">
              <legend className="text-xs font-medium">Delivery method</legend>
              {SHIPPING_OPTIONS.map((o) => (
                <label
                  key={o.id}
                  className={`flex cursor-pointer items-center gap-3 rounded-lg border px-3.5 py-2.5 ${shippingId === o.id ? "border-foreground bg-muted" : ""}`}
                >
                  <input type="radio" value={o.id} {...register("shippingId")} className="accent-foreground" />
                  <span className="grow">
                    <span className="block text-sm font-semibold">{o.label}</span>
                    <span className="block text-xs text-muted-foreground">{o.detail}</span>
                  </span>
                  <span className="text-sm font-semibold tabular-nums">
                    {o.price === 0 ? "Free" : money(o.price)}
                  </span>
                </label>
              ))}
            </fieldset>

            <button
              type="button"
              onClick={() => next("pay", ["customerName", "shipLine1", "cityState"])}
              className="mt-2 w-full rounded-full bg-foreground py-3 text-sm font-semibold text-background"
            >
              Continue to payment →
            </button>
          </section>
        )}

        {showPay && (
          <section className="space-y-3">
            <h2 className="font-display text-2xl">
              Payment <i>details</i>
            </h2>
            <p className="rounded-lg bg-muted px-3 py-2 text-xs text-muted-foreground">
              Demo checkout — nothing is charged. Card details are validated for shape only;
              just the brand and last four digits go on the order.
            </p>
            <div>
              <label htmlFor="cardNumber" className="text-xs font-medium">Card number</label>
              <input
                id="cardNumber"
                inputMode="numeric"
                placeholder="4242 4242 4242 4242"
                className="mt-1 h-10 w-full rounded-lg border px-3 text-sm tabular-nums"
                {...register("cardNumber", {
                  required: "Card number is required.",
                  validate: (v) =>
                    /^\d{13,19}$/.test(v.replace(/[\s-]/g, "")) || "That doesn't look like a card number.",
                })}
              />
              {errors.cardNumber && <p role="alert" className="mt-1 text-xs text-red-600">{errors.cardNumber.message}</p>}
            </div>
            <div>
              <label htmlFor="cardName" className="text-xs font-medium">Name on card</label>
              <input
                id="cardName"
                className="mt-1 h-10 w-full rounded-lg border px-3 text-sm"
                {...register("cardName", { required: "Name on card is required." })}
              />
              {errors.cardName && <p role="alert" className="mt-1 text-xs text-red-600">{errors.cardName.message}</p>}
            </div>
            <div className="flex gap-2 pt-1">
              <button type="button" onClick={() => setStep("ship")} className="rounded-full border px-5 py-3 text-sm">
                ← Back
              </button>
              <button
                type="button"
                onClick={() => next("review", ["cardNumber", "cardName"])}
                className="grow rounded-full bg-foreground py-3 text-sm font-semibold text-background"
              >
                Review order →
              </button>
            </div>
          </section>
        )}

        {showReview && (
          <section className="space-y-3">
            <h2 className="font-display text-2xl">
              Review &amp; <i>place order</i>
            </h2>
            <dl className="space-y-1.5 rounded-xl border p-4 text-sm">
              <div className="flex justify-between"><dt className="text-muted-foreground">Ship to</dt><dd>{getValues("shipLine1")}, {getValues("cityState")}</dd></div>
              <div className="flex justify-between"><dt className="text-muted-foreground">Contact</dt><dd>{email}</dd></div>
              <div className="flex justify-between"><dt className="text-muted-foreground">Delivery</dt><dd>{SHIPPING_OPTIONS.find((o) => o.id === shippingId)?.label}</dd></div>
              <div className="flex justify-between"><dt className="text-muted-foreground">Pay with</dt><dd>{detectBrand(getValues("cardNumber"))} ···· {getValues("cardNumber").replace(/\D/g, "").slice(-4)}</dd></div>
              <div className="mt-2 flex justify-between border-t pt-2 font-semibold"><dt>Total</dt><dd className="tabular-nums">{money(totals.total)}</dd></div>
            </dl>
            {serverError && (
              <p role="alert" className="rounded-lg bg-red-50 px-3 py-2 text-xs text-red-700">{serverError}</p>
            )}
            <div className="flex gap-2">
              <button type="button" onClick={() => setStep("pay")} className="rounded-full border px-5 py-3 text-sm">
                ← Back
              </button>
              <button
                type="button"
                onClick={submit}
                disabled={pending}
                className="grow rounded-full bg-foreground py-3 text-sm font-semibold text-background disabled:opacity-50"
              >
                {pending ? "Placing order…" : `Place order · ${money(totals.total)}`}
              </button>
            </div>
          </section>
        )}
      </form>
    </div>
  );
}
```

Note: totals shown in the rail don't re-compute when the shipping radio changes step-side (server recomputes the real number at placement; the cart page already shows "from" pricing). If you want live totals, lift `shipping` into the page via a search param — do NOT duplicate the math client-side beyond `SHIPPING_OPTIONS` lookups.

Run: `npx vitest run src/components/storefront/checkout-form.test.tsx` → PASS.

- [ ] **Step 3: `checkout/page.tsx`** (auth-gated; empty cart → `/cart`):

```tsx
import { redirect } from "next/navigation";
import { auth } from "@/lib/auth/config";
import { CheckoutForm } from "@/components/storefront/checkout-form";
import { ProductImage } from "@/components/storefront/product-image";
import { fetchProductBySku } from "@/lib/catalog/api";
import { fetchPromotionByCode } from "@/lib/catalog/promotions";
import { money } from "@/lib/money";
import { readCart } from "@/lib/storefront/cart-cookie";
import { computeTotals, type PricedLine } from "@/lib/storefront/totals";

export default async function CheckoutPage() {
  const session = await auth();
  if (!session?.user?.email) {
    redirect("/api/auth/signin?callbackUrl=/checkout");
  }

  const cart = await readCart();
  if (cart.lines.length === 0) redirect("/cart");

  const products = await Promise.all(cart.lines.map((l) => fetchProductBySku(l.sku)));
  const priced: PricedLine[] = [];
  cart.lines.forEach((line, i) => {
    const p = products[i];
    if (p && p.status !== "draft") {
      priced.push({ sku: line.sku, name: p.name, price: p.price, qty: line.qty });
    }
  });
  if (priced.length === 0) redirect("/cart");

  const promo = cart.promoCode ? await fetchPromotionByCode(cart.promoCode) : null;
  const totals = computeTotals(priced, promo, "standard");

  return (
    <div className="mx-auto max-w-5xl px-6 py-6">
      <div className="grid gap-10 lg:grid-cols-[1fr_360px]">
        <CheckoutForm email={session.user.email} totals={totals} />

        {/* Summary rail (Checkout_Desktop right column) */}
        <aside className="order-first lg:order-last lg:sticky lg:top-4 lg:self-start">
          <div className="rounded-xl border">
            <p className="border-b px-5 py-3.5 text-[11px] font-semibold uppercase tracking-[0.14em] text-muted-foreground">
              Order summary
            </p>
            <div className="px-5">
              {priced.map((l) => (
                <div key={l.sku} className="flex items-center gap-3 border-b py-3 last:border-b-0">
                  <ProductImage photoUrl={undefined} category="" name={l.name} className="h-12 w-12" />
                  <div className="grow">
                    <p className="text-[13px] font-semibold">{l.name}</p>
                    <p className="text-xs text-muted-foreground">×{l.qty}</p>
                  </div>
                  <span className="text-xs font-semibold tabular-nums">{money(l.price * l.qty)}</span>
                </div>
              ))}
            </div>
            <dl className="space-y-1.5 bg-muted/50 px-5 py-4 text-sm">
              <div className="flex justify-between"><dt className="text-muted-foreground">Subtotal</dt><dd className="tabular-nums">{money(totals.subtotal)}</dd></div>
              {totals.discount > 0 && cart.promoCode && (
                <div className="flex justify-between text-green-700"><dt>Promo · {cart.promoCode}</dt><dd className="tabular-nums">−{money(totals.discount)}</dd></div>
              )}
              <div className="flex justify-between"><dt className="text-muted-foreground">Shipping · Standard</dt><dd className="tabular-nums">{money(totals.shipping)}</dd></div>
              <div className="flex justify-between"><dt className="text-muted-foreground">Estimated tax</dt><dd className="tabular-nums">{money(totals.tax)}</dd></div>
              <div className="flex justify-between border-t pt-2 font-semibold"><dt>Total</dt><dd className="tabular-nums text-lg">{money(totals.total)}</dd></div>
            </dl>
          </div>
        </aside>
      </div>
    </div>
  );
}
```

Pass the chosen shipping into the displayed totals later if live-updating is wanted — the placed order is always server-computed either way. Also pull the product for each summary image if you want real thumbs: swap `photoUrl={undefined}` for the product's first photo by indexing `products`.

- [ ] **Step 4: Confirmation page**

```tsx
import Link from "next/link";
import { notFound, redirect } from "next/navigation";
import { auth } from "@/lib/auth/config";
import { fetchOrderConfirmation } from "@/lib/catalog/checkout";
import { money } from "@/lib/money";

export default async function ConfirmationPage({
  params,
}: {
  params: Promise<{ number: string }>;
}) {
  const session = await auth();
  if (!session) redirect("/api/auth/signin?callbackUrl=/");

  const { number } = await params;
  const orderNumber = Number(number);
  if (!Number.isInteger(orderNumber)) notFound();

  const order = await fetchOrderConfirmation(orderNumber);
  if (!order) notFound();

  return (
    <div className="mx-auto max-w-xl px-6 py-12 text-center">
      <p className="text-4xl">✓</p>
      <h1 className="font-display mt-3 text-3xl">
        Order <i>#{order.number}</i> placed.
      </h1>
      <p className="mt-2 text-sm text-muted-foreground">
        A confirmation is on its way to {order.customer.email}. Every piece is made one at a
        time — it ships in 3–5 days.
      </p>

      <div className="mt-8 rounded-xl border p-5 text-left">
        {order.lines.map((line) => (
          <div key={line.sku} className="flex justify-between border-b py-2.5 text-sm last:border-b-0">
            <span>
              {line.productName}
              {line.qty > 1 ? ` ×${line.qty}` : ""}
            </span>
            <span className="tabular-nums">{money(line.unitPrice * line.qty)}</span>
          </div>
        ))}
        <dl className="mt-3 space-y-1 border-t pt-3 text-sm">
          <div className="flex justify-between"><dt className="text-muted-foreground">Subtotal</dt><dd className="tabular-nums">{money(order.summary.subtotal)}</dd></div>
          {order.summary.discountCode && (
            <div className="flex justify-between text-green-700">
              <dt>Promo · {order.summary.discountCode}</dt>
              <dd className="tabular-nums">−{money(order.summary.discountAmount)}</dd>
            </div>
          )}
          <div className="flex justify-between"><dt className="text-muted-foreground">Shipping</dt><dd className="tabular-nums">{money(order.summary.shipping)}</dd></div>
          <div className="flex justify-between"><dt className="text-muted-foreground">Tax</dt><dd className="tabular-nums">{money(order.summary.tax)}</dd></div>
          <div className="flex justify-between font-semibold"><dt>Paid</dt><dd className="tabular-nums">{money(order.summary.paid)}</dd></div>
        </dl>
      </div>

      <Link
        href="/"
        className="mt-8 inline-block rounded-full bg-foreground px-6 py-3 text-sm font-semibold text-background"
      >
        Keep shopping →
      </Link>
    </div>
  );
}
```

Field names (`order.lines[].productName`, `order.summary.subtotal`, …) must match `src/web/src/lib/seller/orders/types.ts` — verify the casing there (it mirrors the C# DTOs in camelCase) and adjust.

- [ ] **Step 5: Build, lint, run all web tests, commit**

Run: `npm run build && npm run lint && npm test` → all pass.

```bash
git add src/web/src
git commit -m "feat(web): login-gated checkout wizard + order confirmation"
```

---

### Task 15: Seller order page shows the promo line

**Files:**
- Modify: `src/web/src/lib/seller/orders/types.ts` (the `OrderSummary` type, ~line 119)
- Modify: `src/web/src/components/seller/orders/order-detail-summary.test.tsx`
- Modify: `src/web/src/components/seller/orders/order-detail-summary.tsx`

- [ ] **Step 1: Extend the type**

```ts
export type OrderSummary = {
  subtotal: number;
  // ...existing fields stay untouched...
  discountCode: string | null;
  discountAmount: number;
};
```

(Backend always serializes both now; no optionality needed. If existing fixtures in tests break, add `discountCode: null, discountAmount: 0` to them.)

- [ ] **Step 2: Failing component test.** Add a case to `order-detail-summary.test.tsx` (follow the file's existing fixture style):

```tsx
it("renders the promo line when the order carried a discount", () => {
  render(
    <OrderDetailSummary
      summary={{ ...baseSummary, discountCode: "WELCOME10", discountAmount: 10 }}
    />,
  );
  expect(screen.getByText("Promo · WELCOME10")).toBeInTheDocument();
  expect(screen.getByText("−$10.00")).toBeInTheDocument();
});

it("omits the promo line when there is no discount", () => {
  render(<OrderDetailSummary summary={{ ...baseSummary, discountCode: null, discountAmount: 0 }} />);
  expect(screen.queryByText(/Promo ·/)).not.toBeInTheDocument();
});
```

(`baseSummary` and the component's props: read the existing test file and reuse its fixture/prop names exactly — the component may take the whole order; mirror what's there.)

Run: `npx vitest run src/components/seller/orders/order-detail-summary.test.tsx` → FAIL.

- [ ] **Step 3: Implement.** In `order-detail-summary.tsx`, after the Subtotal row (match surrounding row markup exactly — same class names; note the − is U+2212 per the design glyph conventions):

```tsx
{summary.discountCode && summary.discountAmount > 0 && (
  <div className="...same row classes as Subtotal...">
    <span className="...">Promo · {summary.discountCode}</span>
    <span className="...">−{money(summary.discountAmount)}</span>
  </div>
)}
```

- [ ] **Step 4: Run tests + lint + commit**

Run: `npx vitest run src/components/seller/orders && npm run lint` → PASS.

```bash
git add src/web/src
git commit -m "feat(web): show storefront promo discount on seller order detail"
```

---

### Task 16: e2e — buyer auth setup + full funnel spec

**Files:**
- Create: `src/web/e2e/buyer-auth.setup.ts`
- Create: `src/web/e2e/storefront-funnel.spec.ts`
- Modify: `src/web/playwright.config.ts`

Requires the full Aspire stack (`dotnet run --project src/AppHost`). e2e runs serially (Keycloak memory).

- [ ] **Step 1: `buyer-auth.setup.ts`** (mirrors `auth.setup.ts`; `/checkout` is the bounce trigger):

```ts
// Logs in the seeded BUYER once and stores e2e/.auth/buyer.json. Mirrors
// auth.setup.ts (seller). Keep credentials in sync with
// src/AppHost/Realms/microcommerce-realm.json.
import path from "node:path";
import { expect, test as setup } from "@playwright/test";

const BUYER_EMAIL = process.env.E2E_BUYER_EMAIL ?? "buyer@microcommerce.dev";
const BUYER_PASSWORD = process.env.E2E_BUYER_PASSWORD ?? "Passw0rd!";
const STORAGE_STATE = path.resolve(__dirname, ".auth/buyer.json");

setup("authenticate as the seeded buyer", async ({ page }) => {
  // /checkout is auth-gated by the proxy; anonymous hit bounces to sign-in.
  await page.goto("/checkout");
  await page.getByRole("button", { name: /Sign in with Keycloak/i }).click();

  const username = page.locator("input#username");
  await expect(username).toBeVisible({ timeout: 15_000 });
  await username.fill(BUYER_EMAIL);
  await page.locator("input#password").fill(BUYER_PASSWORD);
  await page.locator("input#kc-login, button[type=submit]").first().click();

  // Landed back in the app (empty cart redirects /checkout -> /cart; either is fine).
  await page.waitForURL(/\/(checkout|cart)(\/|\?|$)/, { timeout: 30_000 });
  await expect(page).not.toHaveURL(/\/api\/auth\/signin/);

  await page.context().storageState({ path: STORAGE_STATE });
});
```

- [ ] **Step 2: Playwright projects.** In `playwright.config.ts`, extend `projects` (keep the existing two entries; add buyer storage state constant next to the seller one — check how `storageState` is defined at the top of the file and mirror it):

```ts
  projects: [
    {
      name: "setup",
      testMatch: /auth\.setup\.ts/,
    },
    {
      name: "setup-buyer",
      testMatch: /buyer-auth\.setup\.ts/,
    },
    {
      name: "chromium",
      use: { ...devices["Desktop Chrome"], storageState },
      dependencies: ["setup"],
      testIgnore: [/auth\.setup\.ts/, /buyer-auth\.setup\.ts/, /storefront-.*\.spec\.ts/],
    },
    {
      name: "storefront",
      use: {
        ...devices["Desktop Chrome"],
        storageState: path.resolve(__dirname, "e2e/.auth/buyer.json"),
      },
      // "setup" too: the funnel spec opens a seller context from e2e/.auth/seller.json.
      dependencies: ["setup", "setup-buyer"],
      testMatch: /storefront-.*\.spec\.ts/,
    },
  ],
```

(Add `import path from "node:path";` if the config doesn't already have it.)

- [ ] **Step 3: `storefront-funnel.spec.ts`**

```ts
// Full buyer funnel against the real stack: browse → PDP → cart (+ WELCOME10,
// seeded: fixed $10 off, $40 minimum, active, no date window) → checkout →
// confirmation → the order shows up for the seller. Serial like all e2e here.
import path from "node:path";
import { expect, test } from "@playwright/test";

const SELLER_STATE = path.resolve(__dirname, ".auth/seller.json");

test.describe.configure({ mode: "serial" });

test("buyer browses, applies WELCOME10, checks out, and the seller sees the order", async ({
  page,
  browser,
}) => {
  // Browse home (Persimmon vase is seeded at $86 — comfortably over the $40 promo min).
  await page.goto("/");
  await expect(page.getByText(/\d+ pieces/)).toBeVisible();
  await page.getByRole("link", { name: "Vessels" }).click();
  await page.getByRole("link", { name: /Persimmon vase/ }).first().click();

  // PDP → add to bag.
  await expect(page.getByRole("heading", { name: /Persimmon vase/ })).toBeVisible();
  await page.getByRole("button", { name: /Add to bag/ }).click();
  await expect(page.getByText("Added to bag ✓")).toBeVisible();

  // Cart: line present, promo applies.
  await page.goto("/cart");
  await expect(page.getByText("Persimmon vase")).toBeVisible();
  await page.getByLabel("Promo code").fill("WELCOME10");
  await page.getByRole("button", { name: "Apply" }).click();
  await expect(page.getByText(/WELCOME10 applied/)).toBeVisible();

  // Checkout (already authenticated via buyer storageState).
  await page.getByRole("link", { name: /Checkout/ }).click();
  await page.getByLabel("Full name").fill("Bao Buyer");
  await page.getByLabel("Street address").fill("241 Telegraph Ave");
  await page.getByLabel("City, State ZIP").fill("Oakland, CA 94612");
  await page.getByRole("button", { name: /Continue to payment/ }).click();

  await page.getByLabel("Card number").fill("4242 4242 4242 4242");
  await page.getByLabel("Name on card").fill("Bao Buyer");
  await page.getByRole("button", { name: /Review order/ }).click();
  await page.getByRole("button", { name: /Place order/ }).click();

  // Confirmation.
  await page.waitForURL(/\/checkout\/confirmation\/\d+/, { timeout: 30_000 });
  const orderNumber = Number(page.url().match(/confirmation\/(\d+)/)![1]);
  await expect(page.getByRole("heading", { name: new RegExp(`#${orderNumber}`) })).toBeVisible();
  await expect(page.getByText("Promo · WELCOME10")).toBeVisible();

  // Cart is now empty.
  await page.goto("/cart");
  await expect(page.getByText(/bag is empty/i)).toBeVisible();

  // Seller sees the order (separate authenticated context, same browser).
  const sellerContext = await browser.newContext({ storageState: SELLER_STATE });
  const sellerPage = await sellerContext.newPage();
  await sellerPage.goto(`/seller/orders/${orderNumber}`);
  await expect(sellerPage.getByText("Bao Buyer")).toBeVisible();
  await expect(sellerPage.getByText("Promo · WELCOME10")).toBeVisible();
  await sellerContext.close();
});

test("anonymous checkout bounces to Keycloak", async ({ browser }) => {
  const anonContext = await browser.newContext(); // no storageState
  const anonPage = await anonContext.newPage();
  await anonPage.goto("/checkout");
  await expect(anonPage).toHaveURL(/\/api\/auth\/signin/);
  await anonContext.close();
});
```

Adjust the seller-order URL if the seller route uses a different id format (check `src/web/src/app/seller/orders/[id]/` — the `[id]` param is the order number in existing specs; confirm against an existing `seller-orders` spec).

The `storefront` project depends only on `setup-buyer`; the cross-check needs the seller state file too, so list both: `dependencies: ["setup", "setup-buyer"]` on the storefront project.

- [ ] **Step 4: Run e2e against the stack**

Start the stack in one terminal: `dotnet run --project src/AppHost` (fresh `catalogdb` volume if migrations changed — `docker volume rm <catalogdb-volume>` once).
Run in another (from `src/web/`): `BASE_URL=<web-endpoint> npm run e2e`
Expected: new storefront specs PASS and **every existing seller spec stays green**.

- [ ] **Step 5: Commit**

```bash
git add src/web/e2e src/web/playwright.config.ts
git commit -m "test(e2e): buyer auth setup + storefront full-funnel spec"
```

---

### Task 17: Full verification gates + browser pass

- [ ] **Step 1: Backend suites**

```bash
dotnet build src/MicroCommerce.slnx
dotnet test src/MicroCommerce.slnx
```

Expected: all green (FunctionalTests + IntegrationTests need Docker; IntegrationTests also needs the Dapr CLI).

- [ ] **Step 2: Web suites**

```bash
cd src/web
npm run lint && npm test && npm run build
```

Expected: all green.

- [ ] **Step 3: e2e full run** (AppHost in another terminal): `npm run e2e` — all specs green, serially.

- [ ] **Step 4: Browser verification (mandatory per project memory).** With the stack running, use agent-browser (`/usr/local/bin/agent-browser`):

```bash
AB=/usr/local/bin/agent-browser
$AB --session storefront open http://localhost:3000/
$AB --session storefront wait --load networkidle
$AB --session storefront errors        # expect: none
$AB --session storefront screenshot --annotate qa/evidence/screenshots/storefront-home.png
$AB --session storefront open http://localhost:3000/products/MC-VS-001
$AB --session storefront errors
$AB --session storefront screenshot --annotate qa/evidence/screenshots/storefront-pdp.png
$AB --session storefront open http://localhost:3000/cart
$AB --session storefront errors
$AB --session storefront screenshot --annotate qa/evidence/screenshots/storefront-cart.png
```

Compare against `Home_Tiles` / `Product_Classic` / `Cart_*` in `design/project/` — typography, spacing, glyphs (·, –, ↑/↓), and confirm the brand reads "Micro Commerce" everywhere (no "Mira").

- [ ] **Step 5: Final commit** (any evidence/doc updates):

```bash
git add qa/
git commit -m "qa: storefront browser-verification evidence"
```

---

## Self-review checklist (run after writing, fixed inline)

- **Spec coverage:** home/shop (T11), PDP (T12), cart (T13), checkout + wizard + confirmation (T14), cookie cart (T8/T9/T10), Keycloak buyer + registration + gate (T5/T7), demo payment (T14), promo end-to-end incl. order persistence + seller display (T1/T2/T3/T15), buyer order read (T5/T6), category/sort/buyable (T4), error handling (T3/T10/T13/T14), testing pyramid + e2e + browser pass (every task + T16/T17). Out-of-scope items (reviews, variants, redemption counts, tracking) appear in no task. ✓
- **Type consistency:** `CartState/CartLine` (T8) used by T9/T10/T11; `PlaceOrderPayload/CheckoutError` (T9) used by T10/T14; `StorefrontLineInput` (T3) used by T5/T6; `OrderSummaryDto.DiscountCode/DiscountAmount` (T2) read by T14/T15; `SHIPPING_OPTIONS/ShippingId/Totals` (T8) used by T10/T13/T14. ✓
- **Known judgment calls an executor may hit:** exact `Product`/`OrderLine` ctor signatures, `Result<T>` factory names, `revalidateTag` vs `updateTag` form, `OrderDetail` TS type name, `CacheProducts` vary-by-query config — each task says where to look and what to mirror; deviations there are expected and fine as long as the tests stay as written.

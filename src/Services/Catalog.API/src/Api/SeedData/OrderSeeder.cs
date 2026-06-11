using System.Reflection;
using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Api.SeedData;

public static class OrderSeeder
{
    // Idempotent by Number: load existing order numbers into a HashSet and only insert
    // missing ones. All DateTimeOffset values are FIXED literals anchored so relative-time
    // labels match exactly at DEMO_NOW = 2026-04-08T16:45:00-07:00. NO Random, NO UtcNow.
    //
    // The Order aggregate exposes status changes only through legal transition methods
    // (Pack/Ship/Deliver/Cancel/RequestRefund), several of which also mutate line state
    // (Ship marks every line Shipped) or block New+Refund combinations. Seeding restores
    // arbitrary *persisted* states with line-level fidelity (e.g. #1042's split fulfillment),
    // so we set Status/Starred/Refund through their private setters via reflection — the same
    // mechanism EF uses to rehydrate these aggregates from the database.
    public static async Task SeedAsync(AppDbContext db, CancellationToken ct = default)
    {
        var existing = (await db.Orders.Select(o => o.Number).ToListAsync(ct)).ToHashSet();

        var seeds = BuildSeeds();

        var added = false;
        foreach (var spec in seeds)
        {
            if (existing.Contains(spec.Number)) continue;
            db.Orders.Add(spec.Build());
            added = true;
        }

        if (added) await db.SaveChangesAsync(ct);
    }

    private static readonly TimeSpan Pt = TimeSpan.FromHours(-7);

    private sealed record LineSpec(
        string Sku,
        string ProductName,
        string Tone,
        int Qty,
        decimal UnitPrice,
        FulfillmentStatus Fulfillment,
        string? Tracking = null,
        string? RestockNote = null);

    private sealed record TimelineSpec(
        string Icon,
        string Title,
        string Sub,
        DateTimeOffset OccurredAt,
        string? Tone,
        bool Highlight);

    private sealed record RefundItemSpec(string Sku, int Qty, decimal Amount, bool Selected, decimal? PartialAmount);

    private sealed record RefundSpec(string Reason, string RestockChoice, decimal Total, IReadOnlyList<RefundItemSpec> Items);

    private sealed record OrderSpec(
        int Number,
        DateTimeOffset PlacedAt,
        OrderStatus Status,
        bool Starred,
        string CustomerName,
        string CustomerEmail,
        string CityState,
        string ShipLine1,
        string ShipLine2,
        bool BillSameAsShip,
        string ItemsSummary,
        string ShippingMethod,
        decimal ShippingPaid,
        decimal Tax,
        decimal FeePct,
        string PaymentBrand,
        string PaymentLastFour,
        string? LabelCarrier,
        decimal LabelCost,
        string? LabelWeightLabel,
        string InternalNote,
        IReadOnlyList<LineSpec> Lines,
        RefundSpec? Refund = null,
        IReadOnlyList<TimelineSpec>? Timeline = null)
    {
        public Order Build()
        {
            var lines = Lines
                .Select(l => new OrderLine(l.Sku, l.ProductName, l.Tone, l.Qty, l.UnitPrice, l.Fulfillment, l.Tracking, l.RestockNote))
                .ToList();

            var order = new Order(
                Number, PlacedAt, CustomerName, CustomerEmail, CityState, ShipLine1, ShipLine2,
                BillSameAsShip, ItemsSummary, ShippingMethod, ShippingPaid, Tax, FeePct,
                PaymentBrand, PaymentLastFour, LabelCarrier, LabelCost, LabelWeightLabel,
                InternalNote, lines);

            SetStatus(order, Status);
            if (Starred) SetStarred(order, true);

            if (Refund is not null)
            {
                var draft = new RefundDraft(
                    Refund.Reason,
                    Refund.RestockChoice,
                    Refund.Total,
                    Refund.Items.Select(i => new RefundDraftItem(i.Sku, i.Qty, i.Amount, i.Selected, i.PartialAmount)).ToList());
                SetRefund(order, draft);
            }

            if (Timeline is not null)
                foreach (var t in Timeline)
                    order.AppendTimeline(new OrderTimelineEntry(t.Icon, t.Title, t.Sub, t.OccurredAt, t.Tone, t.Highlight));

            return order;
        }
    }

    private static readonly PropertyInfo StatusProp =
        typeof(Order).GetProperty(nameof(Order.Status))!;
    private static readonly PropertyInfo StarredProp =
        typeof(Order).GetProperty(nameof(Order.Starred))!;
    private static readonly PropertyInfo RefundProp =
        typeof(Order).GetProperty(nameof(Order.Refund))!;

    private static void SetStatus(Order o, OrderStatus status) => StatusProp.SetValue(o, status);
    private static void SetStarred(Order o, bool starred) => StarredProp.SetValue(o, starred);
    private static void SetRefund(Order o, RefundDraft draft) => RefundProp.SetValue(o, draft);

    private static List<OrderSpec> BuildSeeds()
    {
        var seeds = new List<OrderSpec>();

        // === The 10 inbox orders (#1042–#1033), verbatim from plan 2.1 SeedData. ===

        var placed1042 = new DateTimeOffset(2026, 4, 8, 14, 14, 0, Pt);
        seeds.Add(new OrderSpec(
            Number: 1042,
            PlacedAt: placed1042,
            Status: OrderStatus.New,
            Starred: true,
            CustomerName: "Sasha Leblanc",
            CustomerEmail: "sasha.l@gmail.com",
            CityState: "San Francisco, CA",
            ShipLine1: "820 Sutter St · #4B",
            ShipLine2: "San Francisco, CA 94109",
            BillSameAsShip: true,
            ItemsSummary: "Persimmon vase, Ash budstem",
            ShippingMethod: "USPS Priority",
            ShippingPaid: 0m,
            Tax: 0m,
            FeePct: 4m,
            PaymentBrand: "Visa",
            PaymentLastFour: "4421",
            LabelCarrier: "USPS",
            LabelCost: 9.84m,
            LabelWeightLabel: "1lb 4oz",
            InternalNote: "Held until budstem restocks Tue. Sasha OK with split.",
            Lines:
            [
                new LineSpec("MC-VS-001", "Persimmon vase", "clay", 1, 86.00m, FulfillmentStatus.Shipped, "USPS · 9405 5036 9930 0124 2317", null),
                new LineSpec("MC-AB-002", "Ash budstem", "rust", 1, 66.00m, FulfillmentStatus.Awaiting, null, "back in stock Tue"),
            ],
            Refund: new RefundSpec(
                "Item arrived chipped",
                "Damage",
                30.00m,
                [
                    new RefundItemSpec("MC-VS-001", 1, 86.00m, false, null),
                    new RefundItemSpec("MC-AB-002", 1, 66.00m, true, 30.00m),
                ]),
            Timeline:
            [
                new TimelineSpec("check", "Order placed", "2 items · $152.00 paid via Visa · 4421", placed1042.AddHours(-2), null, true),
                new TimelineSpec("box", "Persimmon vase packed", "Box S · 1lb 4oz", placed1042.AddHours(-1), null, false),
                new TimelineSpec("truck", "Persimmon vase shipped", "USPS Priority · 1–3 days", placed1042.AddMinutes(-52), null, false),
                new TimelineSpec("chat", "Note from Sasha", "\"No rush on the budstem — ship together if it's faster!\"", placed1042.AddMinutes(-14), null, false),
                new TimelineSpec("info", "Ash budstem oversold", "Restock arrives Tue · auto-fulfill on", placed1042.AddMinutes(-8), "warn", false),
            ]));

        seeds.Add(Inbox(
            number: 1041,
            placedAt: new DateTimeOffset(2026, 4, 8, 11, 8, 0, Pt),
            status: OrderStatus.New,
            customerName: "Dev Patel",
            customerEmail: "dev.p@gmail.com",
            cityState: "Brooklyn, NY",
            itemsSummary: "Forest bowl, lg.",
            shippingMethod: "USPS Ground",
            lines: [new LineSpec("MC-BW-014", "Forest bowl", "sage", 1, 64.00m, FulfillmentStatus.Awaiting)]));

        seeds.Add(Inbox(
            number: 1040,
            placedAt: new DateTimeOffset(2026, 4, 8, 9, 41, 0, Pt),
            status: OrderStatus.New,
            customerName: "Luz Moreno",
            customerEmail: "luz.m@gmail.com",
            cityState: "Portland, OR",
            itemsSummary: "Cream tumbler set",
            shippingMethod: "USPS Ground",
            lines: [new LineSpec("MC-TB-007", "Cream tumbler", "cream", 1, 48.00m, FulfillmentStatus.Awaiting)]));

        seeds.Add(Inbox(
            number: 1039,
            placedAt: new DateTimeOffset(2026, 4, 7, 14, 30, 0, Pt),
            status: OrderStatus.Packed,
            customerName: "Kai Zhang",
            customerEmail: "kai.z@gmail.com",
            cityState: "Seattle, WA",
            itemsSummary: "Indigo carafe",
            shippingMethod: "UPS Ground",
            lines: [new LineSpec("MC-CR-003", "Indigo carafe", "indigo", 1, 110.00m, FulfillmentStatus.Awaiting)]));

        seeds.Add(Inbox(
            number: 1038,
            placedAt: new DateTimeOffset(2026, 4, 6, 10, 0, 0, Pt),
            status: OrderStatus.Shipped,
            customerName: "Jordan Ellis",
            customerEmail: "jordan.e@gmail.com",
            cityState: "Austin, TX",
            itemsSummary: "Soft hand vessel +2",
            shippingMethod: "USPS Priority",
            lines:
            [
                new LineSpec("MC-VS-001", "Persimmon vase", "clay", 2, 86.00m, FulfillmentStatus.Shipped, "USPS · 9405 1038"),
                new LineSpec("MC-TB-007", "Cream tumbler", "cream", 1, 46.00m, FulfillmentStatus.Shipped, "USPS · 9405 1038"),
            ]));

        seeds.Add(Inbox(
            number: 1037,
            placedAt: new DateTimeOffset(2026, 4, 5, 9, 0, 0, Pt),
            status: OrderStatus.Shipped,
            customerName: "Anna Vogel",
            customerEmail: "anna.v@gmail.com",
            cityState: "Vancouver, BC",
            itemsSummary: "Ceremony bowl",
            shippingMethod: "USPS Intl",
            lines: [new LineSpec("MC-BW-014", "Ceremony bowl", "sage", 1, 142.00m, FulfillmentStatus.Shipped, "USPS · 9405 1037")]));

        seeds.Add(Inbox(
            number: 1036,
            placedAt: new DateTimeOffset(2026, 4, 4, 11, 0, 0, Pt),
            status: OrderStatus.RefundRequested,
            customerName: "Marco Rivera",
            customerEmail: "marco.r@gmail.com",
            cityState: "Mexico City, MX",
            itemsSummary: "Field cup × 4",
            shippingMethod: "USPS Intl",
            lines: [new LineSpec("MC-MG-041", "Field cup", "rust", 4, 22.00m, FulfillmentStatus.Shipped, "USPS · 9405 1036")],
            refund: new RefundSpec(
                "Two cups cracked in transit",
                "Damage",
                44.00m,
                [new RefundItemSpec("MC-MG-041", 2, 44.00m, true, 44.00m)])));

        seeds.Add(Inbox(
            number: 1035,
            placedAt: new DateTimeOffset(2026, 4, 3, 8, 0, 0, Pt),
            status: OrderStatus.Delivered,
            customerName: "Priya Singh",
            customerEmail: "priya.s@gmail.com",
            cityState: "Chicago, IL",
            itemsSummary: "Storm bowl",
            shippingMethod: "USPS Ground",
            lines: [new LineSpec("MC-BW-027", "Storm bowl", "smoke", 1, 58.00m, FulfillmentStatus.Shipped, "USPS · 9405 1035")]));

        seeds.Add(Inbox(
            number: 1034,
            placedAt: new DateTimeOffset(2026, 4, 2, 15, 0, 0, Pt),
            status: OrderStatus.Delivered,
            customerName: "Sam Park",
            customerEmail: "sam.p@gmail.com",
            cityState: "Oakland, CA",
            itemsSummary: "Earth tumbler ×2",
            shippingMethod: "Local pickup",
            lines: [new LineSpec("MC-TB-019", "Earth tumbler", "earth", 2, 28.00m, FulfillmentStatus.Shipped, "Local pickup")]));

        seeds.Add(Inbox(
            number: 1033,
            placedAt: new DateTimeOffset(2026, 4, 1, 9, 0, 0, Pt),
            status: OrderStatus.Cancelled,
            customerName: "Yuki Tanaka",
            customerEmail: "yuki.t@gmail.com",
            cityState: "Los Angeles, CA",
            itemsSummary: "Linen vase wrap",
            shippingMethod: "USPS First",
            lines: [new LineSpec("MC-VS-015", "Linen vase wrap", "linen", 1, 18.00m, FulfillmentStatus.Awaiting)]));

        // === 36 filler orders #1032–#997 (fixed literals; no Random). ===
        // Status distribution to satisfy D2 #1 (total 46):
        //   New 3 (1042/1041/1040), Packed 2 (1039 + 1 filler), Shipped 18 (1038/1037 + 16 filler),
        //   Delivered 21 (1035/1034 + 19 filler), RefundRequested 1 (1036), Cancelled 1 (1033).
        // Persimmon vase (MC-VS-001) is weighted to lead units/revenue for analytics top-products.

        // +1 Packed (Apr 7)
        seeds.Add(Filler(1032, new DateTimeOffset(2026, 4, 7, 9, 15, 0, Pt), OrderStatus.Packed,
            "Nora Beck", "nora.b@gmail.com", "Denver, CO", "USPS Ground",
            [new LineSpec("MC-VS-001", "Persimmon vase", "clay", 1, 86.00m, FulfillmentStatus.Awaiting)]));

        // +16 Shipped (Apr 4 → Mar 23), Persimmon-heavy.
        var shippedFillers = new (int num, DateTimeOffset at, string name, string email, string city, LineSpec[] lines)[]
        {
            (1031, new DateTimeOffset(2026, 4, 4, 13, 0, 0, Pt), "Owen Reed", "owen.r@gmail.com", "Boston, MA", [new LineSpec("MC-VS-001", "Persimmon vase", "clay", 2, 86.00m, FulfillmentStatus.Shipped, "USPS · 1031")]),
            (1030, new DateTimeOffset(2026, 4, 3, 16, 0, 0, Pt), "Mia Foster", "mia.f@gmail.com", "Phoenix, AZ", [new LineSpec("MC-VS-001", "Persimmon vase", "clay", 1, 86.00m, FulfillmentStatus.Shipped, "USPS · 1030")]),
            (1029, new DateTimeOffset(2026, 4, 2, 11, 30, 0, Pt), "Leo Martin", "leo.m@gmail.com", "Miami, FL", [new LineSpec("MC-BW-014", "Forest bowl", "sage", 1, 64.00m, FulfillmentStatus.Shipped, "USPS · 1029")]),
            (1028, new DateTimeOffset(2026, 4, 1, 10, 0, 0, Pt), "Ella Cruz", "ella.c@gmail.com", "Dallas, TX", [new LineSpec("MC-VS-001", "Persimmon vase", "clay", 1, 86.00m, FulfillmentStatus.Shipped, "USPS · 1028")]),
            (1027, new DateTimeOffset(2026, 3, 31, 14, 0, 0, Pt), "Noah Webb", "noah.w@gmail.com", "Atlanta, GA", [new LineSpec("MC-TB-007", "Cream tumbler", "cream", 1, 48.00m, FulfillmentStatus.Shipped, "USPS · 1027")]),
            (1026, new DateTimeOffset(2026, 3, 30, 9, 0, 0, Pt), "Ava Hart", "ava.h@gmail.com", "Nashville, TN", [new LineSpec("MC-VS-001", "Persimmon vase", "clay", 2, 86.00m, FulfillmentStatus.Shipped, "USPS · 1026")]),
            (1025, new DateTimeOffset(2026, 3, 29, 12, 0, 0, Pt), "Liam Shaw", "liam.s@gmail.com", "Detroit, MI", [new LineSpec("MC-CR-003", "Indigo carafe", "indigo", 1, 110.00m, FulfillmentStatus.Shipped, "USPS · 1025")]),
            (1024, new DateTimeOffset(2026, 3, 28, 15, 0, 0, Pt), "Zoe Ray", "zoe.r@gmail.com", "Houston, TX", [new LineSpec("MC-VS-001", "Persimmon vase", "clay", 1, 86.00m, FulfillmentStatus.Shipped, "USPS · 1024")]),
            (1023, new DateTimeOffset(2026, 3, 27, 11, 0, 0, Pt), "Eli Quinn", "eli.q@gmail.com", "San Diego, CA", [new LineSpec("MC-BW-027", "Storm bowl", "smoke", 1, 58.00m, FulfillmentStatus.Shipped, "USPS · 1023")]),
            (1022, new DateTimeOffset(2026, 3, 26, 13, 30, 0, Pt), "Ivy Lane", "ivy.l@gmail.com", "Sacramento, CA", [new LineSpec("MC-VS-001", "Persimmon vase", "clay", 2, 86.00m, FulfillmentStatus.Shipped, "USPS · 1022")]),
            (1021, new DateTimeOffset(2026, 3, 26, 9, 0, 0, Pt), "Max Frost", "max.f@gmail.com", "Minneapolis, MN", [new LineSpec("MC-TB-019", "Earth tumbler", "earth", 2, 28.00m, FulfillmentStatus.Shipped, "USPS · 1021")]),
            (1020, new DateTimeOffset(2026, 3, 25, 16, 0, 0, Pt), "Ruby Vale", "ruby.v@gmail.com", "Salt Lake City, UT", [new LineSpec("MC-VS-001", "Persimmon vase", "clay", 1, 86.00m, FulfillmentStatus.Shipped, "USPS · 1020")]),
            (1019, new DateTimeOffset(2026, 3, 25, 10, 0, 0, Pt), "Cole Wynn", "cole.w@gmail.com", "Tampa, FL", [new LineSpec("MC-MG-041", "Field cup", "rust", 3, 22.00m, FulfillmentStatus.Shipped, "USPS · 1019")]),
            (1018, new DateTimeOffset(2026, 3, 24, 14, 0, 0, Pt), "Lena Pope", "lena.p@gmail.com", "Charlotte, NC", [new LineSpec("MC-VS-001", "Persimmon vase", "clay", 1, 86.00m, FulfillmentStatus.Shipped, "USPS · 1018")]),
            (1017, new DateTimeOffset(2026, 3, 24, 9, 0, 0, Pt), "Jude Kerr", "jude.k@gmail.com", "Columbus, OH", [new LineSpec("MC-BW-014", "Forest bowl", "sage", 1, 64.00m, FulfillmentStatus.Shipped, "USPS · 1017")]),
            (1016, new DateTimeOffset(2026, 3, 23, 12, 0, 0, Pt), "Tess Glen", "tess.g@gmail.com", "Raleigh, NC", [new LineSpec("MC-VS-001", "Persimmon vase", "clay", 1, 86.00m, FulfillmentStatus.Shipped, "USPS · 1016")]),
        };
        foreach (var f in shippedFillers)
            seeds.Add(Filler(f.num, f.at, OrderStatus.Shipped, f.name, f.email, f.city, "USPS Ground", f.lines));

        // +19 Delivered (Mar 22 → Mar 12), including 2 prior Sasha Leblanc orders ($66 each = $132)
        // so her #1042 detail shows "3rd order · $284 lifetime" (132 + 152).
        var deliveredFillers = new (int num, DateTimeOffset at, string name, string email, string city, LineSpec[] lines)[]
        {
            // Two prior Sasha orders (delivered, non-cancelled) -> lifetime spend $132.
            (1015, new DateTimeOffset(2026, 3, 22, 10, 0, 0, Pt), "Sasha Leblanc", "sasha.l@gmail.com", "San Francisco, CA", [new LineSpec("MC-AB-002", "Ash budstem", "rust", 1, 66.00m, FulfillmentStatus.Shipped, "USPS · 1015")]),
            (1014, new DateTimeOffset(2026, 3, 21, 11, 0, 0, Pt), "Sasha Leblanc", "sasha.l@gmail.com", "San Francisco, CA", [new LineSpec("MC-VS-001", "Persimmon vase", "clay", 1, 66.00m, FulfillmentStatus.Shipped, "USPS · 1014")]),
            (1013, new DateTimeOffset(2026, 3, 21, 9, 0, 0, Pt), "Iris Down", "iris.d@gmail.com", "Portland, ME", [new LineSpec("MC-VS-001", "Persimmon vase", "clay", 2, 86.00m, FulfillmentStatus.Shipped, "USPS · 1013")]),
            (1012, new DateTimeOffset(2026, 3, 20, 14, 0, 0, Pt), "Gabe Lowe", "gabe.l@gmail.com", "Albuquerque, NM", [new LineSpec("MC-VS-001", "Persimmon vase", "clay", 1, 86.00m, FulfillmentStatus.Shipped, "USPS · 1012")]),
            (1011, new DateTimeOffset(2026, 3, 19, 10, 0, 0, Pt), "Nina Roe", "nina.r@gmail.com", "Tucson, AZ", [new LineSpec("MC-TB-007", "Cream tumbler", "cream", 1, 48.00m, FulfillmentStatus.Shipped, "USPS · 1011")]),
            (1010, new DateTimeOffset(2026, 3, 19, 8, 0, 0, Pt), "Theo Pace", "theo.p@gmail.com", "Omaha, NE", [new LineSpec("MC-VS-001", "Persimmon vase", "clay", 1, 86.00m, FulfillmentStatus.Shipped, "USPS · 1010")]),
            (1009, new DateTimeOffset(2026, 3, 18, 13, 0, 0, Pt), "Cleo Sims", "cleo.s@gmail.com", "Kansas City, MO", [new LineSpec("MC-BW-027", "Storm bowl", "smoke", 1, 58.00m, FulfillmentStatus.Shipped, "USPS · 1009")]),
            (1008, new DateTimeOffset(2026, 3, 18, 9, 0, 0, Pt), "Maya Hill", "maya.h@gmail.com", "Boise, ID", [new LineSpec("MC-VS-001", "Persimmon vase", "clay", 2, 86.00m, FulfillmentStatus.Shipped, "USPS · 1008")]),
            (1007, new DateTimeOffset(2026, 3, 17, 12, 0, 0, Pt), "Finn Ash", "finn.a@gmail.com", "Spokane, WA", [new LineSpec("MC-CR-003", "Indigo carafe", "indigo", 1, 110.00m, FulfillmentStatus.Shipped, "USPS · 1007")]),
            (1006, new DateTimeOffset(2026, 3, 17, 8, 30, 0, Pt), "Remy Cole", "remy.c@gmail.com", "Reno, NV", [new LineSpec("MC-VS-001", "Persimmon vase", "clay", 1, 86.00m, FulfillmentStatus.Shipped, "USPS · 1006")]),
            (1005, new DateTimeOffset(2026, 3, 16, 14, 0, 0, Pt), "Ada Ven", "ada.v@gmail.com", "Madison, WI", [new LineSpec("MC-MG-041", "Field cup", "rust", 2, 22.00m, FulfillmentStatus.Shipped, "USPS · 1005")]),
            (1004, new DateTimeOffset(2026, 3, 16, 9, 0, 0, Pt), "Beau Ng", "beau.n@gmail.com", "Richmond, VA", [new LineSpec("MC-VS-001", "Persimmon vase", "clay", 1, 86.00m, FulfillmentStatus.Shipped, "USPS · 1004")]),
            (1003, new DateTimeOffset(2026, 3, 15, 13, 0, 0, Pt), "Cora Day", "cora.d@gmail.com", "Buffalo, NY", [new LineSpec("MC-TB-019", "Earth tumbler", "earth", 2, 28.00m, FulfillmentStatus.Shipped, "USPS · 1003")]),
            (1002, new DateTimeOffset(2026, 3, 15, 9, 0, 0, Pt), "Wren Fox", "wren.f@gmail.com", "Des Moines, IA", [new LineSpec("MC-VS-001", "Persimmon vase", "clay", 1, 86.00m, FulfillmentStatus.Shipped, "USPS · 1002")]),
            (1001, new DateTimeOffset(2026, 3, 14, 14, 0, 0, Pt), "Soren Bly", "soren.b@gmail.com", "El Paso, TX", [new LineSpec("MC-BW-014", "Forest bowl", "sage", 1, 64.00m, FulfillmentStatus.Shipped, "USPS · 1001")]),
            (1000, new DateTimeOffset(2026, 3, 14, 9, 0, 0, Pt), "Hana Lim", "hana.l@gmail.com", "Honolulu, HI", [new LineSpec("MC-VS-001", "Persimmon vase", "clay", 1, 86.00m, FulfillmentStatus.Shipped, "USPS · 1000")]),
            (999, new DateTimeOffset(2026, 3, 13, 13, 0, 0, Pt), "Otto Vey", "otto.v@gmail.com", "Anchorage, AK", [new LineSpec("MC-VS-001", "Persimmon vase", "clay", 2, 86.00m, FulfillmentStatus.Shipped, "USPS · 0999")]),
            (998, new DateTimeOffset(2026, 3, 12, 12, 0, 0, Pt), "Esme Roe", "esme.r@gmail.com", "Fargo, ND", [new LineSpec("MC-VS-001", "Persimmon vase", "clay", 1, 86.00m, FulfillmentStatus.Shipped, "USPS · 0998")]),
            (997, new DateTimeOffset(2026, 3, 12, 9, 0, 0, Pt), "Jonah Ek", "jonah.e@gmail.com", "Billings, MT", [new LineSpec("MC-VS-001", "Persimmon vase", "clay", 1, 86.00m, FulfillmentStatus.Shipped, "USPS · 0997")]),
        };
        foreach (var f in deliveredFillers)
            seeds.Add(Filler(f.num, f.at, OrderStatus.Delivered, f.name, f.email, f.city, "USPS Ground", f.lines));

        return seeds;
    }

    // Inbox-row helper: full address fidelity for the 10 named inbox orders.
    private static OrderSpec Inbox(
        int number,
        DateTimeOffset placedAt,
        OrderStatus status,
        string customerName,
        string customerEmail,
        string cityState,
        string itemsSummary,
        string shippingMethod,
        IReadOnlyList<LineSpec> lines,
        RefundSpec? refund = null) =>
        new(
            Number: number,
            PlacedAt: placedAt,
            Status: status,
            Starred: false,
            CustomerName: customerName,
            CustomerEmail: customerEmail,
            CityState: cityState,
            ShipLine1: $"{number} Market St",
            ShipLine2: cityState,
            BillSameAsShip: true,
            ItemsSummary: itemsSummary,
            ShippingMethod: shippingMethod,
            ShippingPaid: 0m,
            Tax: 0m,
            FeePct: 4m,
            PaymentBrand: "Visa",
            PaymentLastFour: "4421",
            LabelCarrier: status is OrderStatus.New ? null : "USPS",
            LabelCost: 0m,
            LabelWeightLabel: status is OrderStatus.New ? null : "1lb",
            InternalNote: "",
            Lines: lines,
            Refund: refund);

    // Filler helper: minimal-but-valid synthetic orders for volume/analytics.
    private static OrderSpec Filler(
        int number,
        DateTimeOffset placedAt,
        OrderStatus status,
        string customerName,
        string customerEmail,
        string cityState,
        string shippingMethod,
        IReadOnlyList<LineSpec> lines) =>
        new(
            Number: number,
            PlacedAt: placedAt,
            Status: status,
            Starred: false,
            CustomerName: customerName,
            CustomerEmail: customerEmail,
            CityState: cityState,
            ShipLine1: $"{number} Main St",
            ShipLine2: cityState,
            BillSameAsShip: true,
            ItemsSummary: lines[0].ProductName + (lines.Count > 1 ? $" +{lines.Count - 1}" : ""),
            ShippingMethod: shippingMethod,
            ShippingPaid: 0m,
            Tax: 0m,
            FeePct: 4m,
            PaymentBrand: "Visa",
            PaymentLastFour: "4421",
            LabelCarrier: status is OrderStatus.Cancelled ? null : "USPS",
            LabelCost: 0m,
            LabelWeightLabel: status is OrderStatus.Cancelled ? null : "1lb",
            InternalNote: "",
            Lines: lines);
}

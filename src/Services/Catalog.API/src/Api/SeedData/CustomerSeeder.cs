using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Domain.Customers;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Api.SeedData;

public static class CustomerSeeder
{
    // Idempotent: inserts any seed customer whose Email is not already in the DB.
    // The six emails mirror the recurring inbox-order customers so the /seller/customers
    // list and per-order customer panels resolve real tags + cities. Lifetime spend/order
    // count are NOT seeded here — they derive at query time by grouping db.Orders on
    // CustomerEmail (no cross-domain FK, see plan D3).
    //
    // CreatedAt uses a FIXED literal anchored before DEMO_NOW (2026-04-08T16:45:00-07:00).
    // NO Random, NO DateTimeOffset.UtcNow — deterministic demo data (see plan D1).
    public static async Task SeedAsync(AppDbContext db, CancellationToken ct = default)
    {
        var existing = (await db.Customers.ToListAsync(ct))
            .Select(c => c.Email.Value)
            .ToHashSet();

        var createdAt = new DateTimeOffset(2026, 1, 15, 9, 0, 0, TimeSpan.FromHours(-7));

        var seeds = new List<(string email, string name, string city, string[] tags)>
        {
            ("sasha.l@gmail.com", "Sasha Leblanc", "San Francisco, CA", ["VIP", "Repeat buyer"]),
            ("dev.p@gmail.com", "Dev Patel", "Brooklyn, NY", []),
            ("luz.m@gmail.com", "Luz Moreno", "Portland, OR", []),
            ("kai.z@gmail.com", "Kai Zhang", "Seattle, WA", ["Repeat buyer"]),
            ("jordan.e@gmail.com", "Jordan Ellis", "Austin, TX", []),
            // Filler customers matching the remaining seeded inbox-order emails.
            ("anna.v@gmail.com", "Anna Vogel", "Vancouver, BC", []),
            ("marco.r@gmail.com", "Marco Rivera", "Mexico City, MX", []),
            ("priya.s@gmail.com", "Priya Singh", "Chicago, IL", []),
            ("sam.p@gmail.com", "Sam Park", "Oakland, CA", []),
            ("yuki.t@gmail.com", "Yuki Tanaka", "Los Angeles, CA", []),
        };

        var added = false;
        foreach (var s in seeds)
        {
            // Email.From normalizes to lower-invariant, matching the unique-index key.
            var email = Email.From(s.email);
            if (existing.Contains(email.Value)) continue;

            var customer = new Customer(s.name, email, s.city, createdAt);
            foreach (var tag in s.tags)
                customer.AddTag(tag);

            db.Customers.Add(customer);
            added = true;
        }

        if (added) await db.SaveChangesAsync(ct);
    }
}

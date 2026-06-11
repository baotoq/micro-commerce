using MicroCommerce.Catalog.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroCommerce.Catalog.Infrastructure.Persistence;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders", t => t.HasCheckConstraint(
            "ck_orders_money_non_negative",
            "shipping_paid >= 0 AND tax >= 0 AND label_cost >= 0"));

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => OrderId.From(value));

        builder.Property(o => o.Number)
            .HasColumnName("number")
            .IsRequired();
        builder.HasIndex(o => o.Number).IsUnique();

        builder.Property(o => o.PlacedAt).HasColumnName("placed_at").IsRequired();

        builder.Property(o => o.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(o => o.Starred).HasColumnName("starred").IsRequired();

        // Customer snapshot
        builder.Property(o => o.CustomerName).HasColumnName("customer_name").HasMaxLength(200).IsRequired();
        builder.Property(o => o.CustomerEmail).HasColumnName("customer_email").HasMaxLength(200).IsRequired();
        builder.Property(o => o.CityState).HasColumnName("city_state").HasMaxLength(120).IsRequired();
        builder.Property(o => o.ShipLine1).HasColumnName("ship_line1").HasMaxLength(200);
        builder.Property(o => o.ShipLine2).HasColumnName("ship_line2").HasMaxLength(200);
        builder.Property(o => o.BillSameAsShip).HasColumnName("bill_same_as_ship").IsRequired();

        // Money / meta
        builder.Property(o => o.ItemsSummary).HasColumnName("items_summary").HasMaxLength(200);
        builder.Property(o => o.ShippingMethod).HasColumnName("shipping_method").HasMaxLength(60);
        builder.Property(o => o.ShippingPaid).HasColumnName("shipping_paid").HasPrecision(18, 2);
        builder.Property(o => o.Tax).HasColumnName("tax").HasPrecision(18, 2);
        builder.Property(o => o.FeePct).HasColumnName("fee_pct").HasPrecision(18, 2);
        builder.Property(o => o.PaymentBrand).HasColumnName("payment_brand").HasMaxLength(40);
        builder.Property(o => o.PaymentLastFour).HasColumnName("payment_last_four").HasMaxLength(8);
        builder.Property(o => o.LabelCarrier).HasColumnName("label_carrier").HasMaxLength(40);
        builder.Property(o => o.LabelCost).HasColumnName("label_cost").HasPrecision(18, 2);
        builder.Property(o => o.LabelWeightLabel).HasColumnName("label_weight_label").HasMaxLength(40);

        builder.Property(o => o.InternalNote).HasColumnName("internal_note").HasColumnType("text");

        // Computed properties are not persisted.
        builder.Ignore(o => o.Subtotal);
        builder.Ignore(o => o.Paid);
        builder.Ignore(o => o.Fee);
        builder.Ignore(o => o.Net);

        // Owned collection: order lines, ordinal-keyed (R3).
        builder.OwnsMany(o => o.Lines, b =>
        {
            b.ToTable("order_lines");
            b.WithOwner().HasForeignKey("order_id");
            b.Property<int>("ordinal").ValueGeneratedOnAdd();
            b.HasKey("order_id", "ordinal");

            b.Property(l => l.Sku).HasColumnName("sku").HasMaxLength(50).IsRequired();
            b.Property(l => l.ProductName).HasColumnName("product_name").HasMaxLength(200).IsRequired();
            b.Property(l => l.Tone).HasColumnName("tone").HasMaxLength(40);
            b.Property(l => l.Qty).HasColumnName("qty").IsRequired();
            b.Property(l => l.UnitPrice).HasColumnName("unit_price").HasPrecision(18, 2);
            b.Property(l => l.FulfillmentStatus)
                .HasColumnName("fulfillment_status")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();
            b.Property(l => l.Tracking).HasColumnName("tracking").HasMaxLength(120);
            b.Property(l => l.RestockNote).HasColumnName("restock_note").HasMaxLength(200);
        });

        // Owned collection: timeline events, ordinal-keyed (R3).
        builder.OwnsMany(o => o.Timeline, b =>
        {
            b.ToTable("order_timeline_events");
            b.WithOwner().HasForeignKey("order_id");
            b.Property<int>("ordinal").ValueGeneratedOnAdd();
            b.HasKey("order_id", "ordinal");

            b.Property(t => t.Icon).HasColumnName("icon").HasMaxLength(20).IsRequired();
            b.Property(t => t.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
            b.Property(t => t.Sub).HasColumnName("sub").HasMaxLength(400);
            b.Property(t => t.OccurredAt).HasColumnName("occurred_at").IsRequired();
            b.Property(t => t.Tone).HasColumnName("tone").HasMaxLength(20);
            b.Property(t => t.Highlight).HasColumnName("highlight").IsRequired();
        });

        // Owned single: refund draft, which itself owns a collection of items (R3/R4).
        builder.OwnsOne(o => o.Refund, b =>
        {
            b.ToTable("order_refunds");
            b.WithOwner().HasForeignKey("order_id");
            b.Property<int>("id").ValueGeneratedOnAdd();
            b.HasKey("order_id");

            b.Property(r => r.Reason).HasColumnName("reason").HasMaxLength(200).IsRequired();
            b.Property(r => r.RestockChoice).HasColumnName("restock_choice").HasMaxLength(20).IsRequired();
            b.Property(r => r.Total).HasColumnName("total").HasPrecision(18, 2);

            b.OwnsMany(r => r.Items, ib =>
            {
                ib.ToTable("order_refund_items");
                ib.WithOwner().HasForeignKey("order_id");
                ib.Property<int>("ordinal").ValueGeneratedOnAdd();
                ib.HasKey("order_id", "ordinal");

                ib.Property(i => i.Sku).HasColumnName("sku").HasMaxLength(50).IsRequired();
                ib.Property(i => i.Qty).HasColumnName("qty").IsRequired();
                ib.Property(i => i.Amount).HasColumnName("amount").HasPrecision(18, 2);
                ib.Property(i => i.Selected).HasColumnName("selected").IsRequired();
                ib.Property(i => i.PartialAmount).HasColumnName("partial_amount").HasPrecision(18, 2);
            });
        });
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MicroCommerce.ApiService.Features.Ordering.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConventionsDryConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Orders_OrderId",
                schema: "ordering",
                table: "OrderItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Orders",
                schema: "ordering",
                table: "Orders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CheckoutSagas",
                schema: "ordering",
                table: "CheckoutSagas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderItems",
                schema: "ordering",
                table: "OrderItems");

            migrationBuilder.RenameTable(
                name: "Orders",
                schema: "ordering",
                newName: "orders",
                newSchema: "ordering");

            migrationBuilder.RenameTable(
                name: "OrderItems",
                schema: "ordering",
                newName: "order_items",
                newSchema: "ordering");

            migrationBuilder.RenameColumn(
                name: "Total",
                schema: "ordering",
                table: "orders",
                newName: "total");

            migrationBuilder.RenameColumn(
                name: "Tax",
                schema: "ordering",
                table: "orders",
                newName: "tax");

            migrationBuilder.RenameColumn(
                name: "Subtotal",
                schema: "ordering",
                table: "orders",
                newName: "subtotal");

            migrationBuilder.RenameColumn(
                name: "Status",
                schema: "ordering",
                table: "orders",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "ordering",
                table: "orders",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ShippingCost",
                schema: "ordering",
                table: "orders",
                newName: "shipping_cost");

            migrationBuilder.RenameColumn(
                name: "PaidAt",
                schema: "ordering",
                table: "orders",
                newName: "paid_at");

            migrationBuilder.RenameColumn(
                name: "OrderNumber",
                schema: "ordering",
                table: "orders",
                newName: "order_number");

            migrationBuilder.RenameColumn(
                name: "FailureReason",
                schema: "ordering",
                table: "orders",
                newName: "failure_reason");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "ordering",
                table: "orders",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "BuyerId",
                schema: "ordering",
                table: "orders",
                newName: "buyer_id");

            migrationBuilder.RenameColumn(
                name: "BuyerEmail",
                schema: "ordering",
                table: "orders",
                newName: "buyer_email");

            migrationBuilder.RenameColumn(
                name: "ShippingAddress_ZipCode",
                schema: "ordering",
                table: "orders",
                newName: "shipping_address_ZipCode");

            migrationBuilder.RenameColumn(
                name: "ShippingAddress_Street",
                schema: "ordering",
                table: "orders",
                newName: "shipping_address_Street");

            migrationBuilder.RenameColumn(
                name: "ShippingAddress_State",
                schema: "ordering",
                table: "orders",
                newName: "shipping_address_State");

            migrationBuilder.RenameColumn(
                name: "ShippingAddress_Name",
                schema: "ordering",
                table: "orders",
                newName: "shipping_address_Name");

            migrationBuilder.RenameColumn(
                name: "ShippingAddress_Email",
                schema: "ordering",
                table: "orders",
                newName: "shipping_address_Email");

            migrationBuilder.RenameColumn(
                name: "ShippingAddress_City",
                schema: "ordering",
                table: "orders",
                newName: "shipping_address_City");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_Status",
                schema: "ordering",
                table: "orders",
                newName: "ix_orders_status");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_OrderNumber",
                schema: "ordering",
                table: "orders",
                newName: "ix_orders_order_number");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_CreatedAt",
                schema: "ordering",
                table: "orders",
                newName: "ix_orders_created_at");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_BuyerId",
                schema: "ordering",
                table: "orders",
                newName: "ix_orders_buyer_id");

            migrationBuilder.RenameColumn(
                name: "SubmittedAt",
                schema: "ordering",
                table: "CheckoutSagas",
                newName: "submitted_at");

            migrationBuilder.RenameColumn(
                name: "ReservationIdsJson",
                schema: "ordering",
                table: "CheckoutSagas",
                newName: "reservation_ids_json");

            migrationBuilder.RenameColumn(
                name: "OrderId",
                schema: "ordering",
                table: "CheckoutSagas",
                newName: "order_id");

            migrationBuilder.RenameColumn(
                name: "FailureReason",
                schema: "ordering",
                table: "CheckoutSagas",
                newName: "failure_reason");

            migrationBuilder.RenameColumn(
                name: "CurrentState",
                schema: "ordering",
                table: "CheckoutSagas",
                newName: "current_state");

            migrationBuilder.RenameColumn(
                name: "BuyerId",
                schema: "ordering",
                table: "CheckoutSagas",
                newName: "buyer_id");

            migrationBuilder.RenameColumn(
                name: "BuyerEmail",
                schema: "ordering",
                table: "CheckoutSagas",
                newName: "buyer_email");

            migrationBuilder.RenameColumn(
                name: "CorrelationId",
                schema: "ordering",
                table: "CheckoutSagas",
                newName: "correlation_id");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                schema: "ordering",
                table: "order_items",
                newName: "quantity");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "ordering",
                table: "order_items",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UnitPrice",
                schema: "ordering",
                table: "order_items",
                newName: "unit_price");

            migrationBuilder.RenameColumn(
                name: "ProductName",
                schema: "ordering",
                table: "order_items",
                newName: "product_name");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                schema: "ordering",
                table: "order_items",
                newName: "product_id");

            migrationBuilder.RenameColumn(
                name: "OrderId",
                schema: "ordering",
                table: "order_items",
                newName: "order_id");

            migrationBuilder.RenameColumn(
                name: "LineTotal",
                schema: "ordering",
                table: "order_items",
                newName: "line_total");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                schema: "ordering",
                table: "order_items",
                newName: "image_url");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItems_OrderId",
                schema: "ordering",
                table: "order_items",
                newName: "ix_order_items_order_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_orders",
                schema: "ordering",
                table: "orders",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_checkout_sagas",
                schema: "ordering",
                table: "CheckoutSagas",
                column: "correlation_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_order_items",
                schema: "ordering",
                table: "order_items",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_order_items_orders_order_id",
                schema: "ordering",
                table: "order_items",
                column: "order_id",
                principalSchema: "ordering",
                principalTable: "orders",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_order_items_orders_order_id",
                schema: "ordering",
                table: "order_items");

            migrationBuilder.DropPrimaryKey(
                name: "pk_orders",
                schema: "ordering",
                table: "orders");

            migrationBuilder.DropPrimaryKey(
                name: "pk_checkout_sagas",
                schema: "ordering",
                table: "CheckoutSagas");

            migrationBuilder.DropPrimaryKey(
                name: "pk_order_items",
                schema: "ordering",
                table: "order_items");

            migrationBuilder.RenameTable(
                name: "orders",
                schema: "ordering",
                newName: "Orders",
                newSchema: "ordering");

            migrationBuilder.RenameTable(
                name: "order_items",
                schema: "ordering",
                newName: "OrderItems",
                newSchema: "ordering");

            migrationBuilder.RenameColumn(
                name: "total",
                schema: "ordering",
                table: "Orders",
                newName: "Total");

            migrationBuilder.RenameColumn(
                name: "tax",
                schema: "ordering",
                table: "Orders",
                newName: "Tax");

            migrationBuilder.RenameColumn(
                name: "subtotal",
                schema: "ordering",
                table: "Orders",
                newName: "Subtotal");

            migrationBuilder.RenameColumn(
                name: "status",
                schema: "ordering",
                table: "Orders",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "ordering",
                table: "Orders",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "shipping_cost",
                schema: "ordering",
                table: "Orders",
                newName: "ShippingCost");

            migrationBuilder.RenameColumn(
                name: "paid_at",
                schema: "ordering",
                table: "Orders",
                newName: "PaidAt");

            migrationBuilder.RenameColumn(
                name: "order_number",
                schema: "ordering",
                table: "Orders",
                newName: "OrderNumber");

            migrationBuilder.RenameColumn(
                name: "failure_reason",
                schema: "ordering",
                table: "Orders",
                newName: "FailureReason");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "ordering",
                table: "Orders",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "buyer_id",
                schema: "ordering",
                table: "Orders",
                newName: "BuyerId");

            migrationBuilder.RenameColumn(
                name: "buyer_email",
                schema: "ordering",
                table: "Orders",
                newName: "BuyerEmail");

            migrationBuilder.RenameColumn(
                name: "shipping_address_ZipCode",
                schema: "ordering",
                table: "Orders",
                newName: "ShippingAddress_ZipCode");

            migrationBuilder.RenameColumn(
                name: "shipping_address_Street",
                schema: "ordering",
                table: "Orders",
                newName: "ShippingAddress_Street");

            migrationBuilder.RenameColumn(
                name: "shipping_address_State",
                schema: "ordering",
                table: "Orders",
                newName: "ShippingAddress_State");

            migrationBuilder.RenameColumn(
                name: "shipping_address_Name",
                schema: "ordering",
                table: "Orders",
                newName: "ShippingAddress_Name");

            migrationBuilder.RenameColumn(
                name: "shipping_address_Email",
                schema: "ordering",
                table: "Orders",
                newName: "ShippingAddress_Email");

            migrationBuilder.RenameColumn(
                name: "shipping_address_City",
                schema: "ordering",
                table: "Orders",
                newName: "ShippingAddress_City");

            migrationBuilder.RenameIndex(
                name: "ix_orders_status",
                schema: "ordering",
                table: "Orders",
                newName: "IX_Orders_Status");

            migrationBuilder.RenameIndex(
                name: "ix_orders_order_number",
                schema: "ordering",
                table: "Orders",
                newName: "IX_Orders_OrderNumber");

            migrationBuilder.RenameIndex(
                name: "ix_orders_created_at",
                schema: "ordering",
                table: "Orders",
                newName: "IX_Orders_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_orders_buyer_id",
                schema: "ordering",
                table: "Orders",
                newName: "IX_Orders_BuyerId");

            migrationBuilder.RenameColumn(
                name: "submitted_at",
                schema: "ordering",
                table: "CheckoutSagas",
                newName: "SubmittedAt");

            migrationBuilder.RenameColumn(
                name: "reservation_ids_json",
                schema: "ordering",
                table: "CheckoutSagas",
                newName: "ReservationIdsJson");

            migrationBuilder.RenameColumn(
                name: "order_id",
                schema: "ordering",
                table: "CheckoutSagas",
                newName: "OrderId");

            migrationBuilder.RenameColumn(
                name: "failure_reason",
                schema: "ordering",
                table: "CheckoutSagas",
                newName: "FailureReason");

            migrationBuilder.RenameColumn(
                name: "current_state",
                schema: "ordering",
                table: "CheckoutSagas",
                newName: "CurrentState");

            migrationBuilder.RenameColumn(
                name: "buyer_id",
                schema: "ordering",
                table: "CheckoutSagas",
                newName: "BuyerId");

            migrationBuilder.RenameColumn(
                name: "buyer_email",
                schema: "ordering",
                table: "CheckoutSagas",
                newName: "BuyerEmail");

            migrationBuilder.RenameColumn(
                name: "correlation_id",
                schema: "ordering",
                table: "CheckoutSagas",
                newName: "CorrelationId");

            migrationBuilder.RenameColumn(
                name: "quantity",
                schema: "ordering",
                table: "OrderItems",
                newName: "Quantity");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "ordering",
                table: "OrderItems",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "unit_price",
                schema: "ordering",
                table: "OrderItems",
                newName: "UnitPrice");

            migrationBuilder.RenameColumn(
                name: "product_name",
                schema: "ordering",
                table: "OrderItems",
                newName: "ProductName");

            migrationBuilder.RenameColumn(
                name: "product_id",
                schema: "ordering",
                table: "OrderItems",
                newName: "ProductId");

            migrationBuilder.RenameColumn(
                name: "order_id",
                schema: "ordering",
                table: "OrderItems",
                newName: "OrderId");

            migrationBuilder.RenameColumn(
                name: "line_total",
                schema: "ordering",
                table: "OrderItems",
                newName: "LineTotal");

            migrationBuilder.RenameColumn(
                name: "image_url",
                schema: "ordering",
                table: "OrderItems",
                newName: "ImageUrl");

            migrationBuilder.RenameIndex(
                name: "ix_order_items_order_id",
                schema: "ordering",
                table: "OrderItems",
                newName: "IX_OrderItems_OrderId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Orders",
                schema: "ordering",
                table: "Orders",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CheckoutSagas",
                schema: "ordering",
                table: "CheckoutSagas",
                column: "CorrelationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderItems",
                schema: "ordering",
                table: "OrderItems",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Orders_OrderId",
                schema: "ordering",
                table: "OrderItems",
                column: "OrderId",
                principalSchema: "ordering",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

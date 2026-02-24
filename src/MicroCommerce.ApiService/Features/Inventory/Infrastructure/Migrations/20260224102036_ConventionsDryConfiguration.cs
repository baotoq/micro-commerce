using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MicroCommerce.ApiService.Features.Inventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConventionsDryConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockReservations_StockItems_StockItemId",
                schema: "inventory",
                table: "StockReservations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StockReservations",
                schema: "inventory",
                table: "StockReservations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StockItems",
                schema: "inventory",
                table: "StockItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StockAdjustments",
                schema: "inventory",
                table: "StockAdjustments");

            migrationBuilder.RenameTable(
                name: "StockReservations",
                schema: "inventory",
                newName: "stock_reservations",
                newSchema: "inventory");

            migrationBuilder.RenameTable(
                name: "StockItems",
                schema: "inventory",
                newName: "stock_items",
                newSchema: "inventory");

            migrationBuilder.RenameTable(
                name: "StockAdjustments",
                schema: "inventory",
                newName: "stock_adjustments",
                newSchema: "inventory");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                schema: "inventory",
                table: "stock_reservations",
                newName: "quantity");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "inventory",
                table: "stock_reservations",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "StockItemId",
                schema: "inventory",
                table: "stock_reservations",
                newName: "stock_item_id");

            migrationBuilder.RenameColumn(
                name: "ExpiresAt",
                schema: "inventory",
                table: "stock_reservations",
                newName: "expires_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "inventory",
                table: "stock_reservations",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_StockReservations_StockItemId",
                schema: "inventory",
                table: "stock_reservations",
                newName: "ix_stock_reservations_stock_item_id");

            migrationBuilder.RenameIndex(
                name: "IX_StockReservations_ExpiresAt",
                schema: "inventory",
                table: "stock_reservations",
                newName: "ix_stock_reservations_expires_at");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "inventory",
                table: "stock_items",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "QuantityOnHand",
                schema: "inventory",
                table: "stock_items",
                newName: "quantity_on_hand");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                schema: "inventory",
                table: "stock_items",
                newName: "product_id");

            migrationBuilder.RenameIndex(
                name: "IX_StockItems_ProductId",
                schema: "inventory",
                table: "stock_items",
                newName: "ix_stock_items_product_id");

            migrationBuilder.RenameColumn(
                name: "Reason",
                schema: "inventory",
                table: "stock_adjustments",
                newName: "reason");

            migrationBuilder.RenameColumn(
                name: "Adjustment",
                schema: "inventory",
                table: "stock_adjustments",
                newName: "adjustment");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "inventory",
                table: "stock_adjustments",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "StockItemId",
                schema: "inventory",
                table: "stock_adjustments",
                newName: "stock_item_id");

            migrationBuilder.RenameColumn(
                name: "QuantityAfter",
                schema: "inventory",
                table: "stock_adjustments",
                newName: "quantity_after");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "inventory",
                table: "stock_adjustments",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "AdjustedBy",
                schema: "inventory",
                table: "stock_adjustments",
                newName: "adjusted_by");

            migrationBuilder.RenameIndex(
                name: "IX_StockAdjustments_StockItemId",
                schema: "inventory",
                table: "stock_adjustments",
                newName: "ix_stock_adjustments_stock_item_id");

            migrationBuilder.RenameIndex(
                name: "IX_StockAdjustments_CreatedAt",
                schema: "inventory",
                table: "stock_adjustments",
                newName: "ix_stock_adjustments_created_at");

            migrationBuilder.AddPrimaryKey(
                name: "pk_stock_reservations",
                schema: "inventory",
                table: "stock_reservations",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_stock_items",
                schema: "inventory",
                table: "stock_items",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_stock_adjustments",
                schema: "inventory",
                table: "stock_adjustments",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_stock_reservations_stock_items_stock_item_id",
                schema: "inventory",
                table: "stock_reservations",
                column: "stock_item_id",
                principalSchema: "inventory",
                principalTable: "stock_items",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_stock_reservations_stock_items_stock_item_id",
                schema: "inventory",
                table: "stock_reservations");

            migrationBuilder.DropPrimaryKey(
                name: "pk_stock_reservations",
                schema: "inventory",
                table: "stock_reservations");

            migrationBuilder.DropPrimaryKey(
                name: "pk_stock_items",
                schema: "inventory",
                table: "stock_items");

            migrationBuilder.DropPrimaryKey(
                name: "pk_stock_adjustments",
                schema: "inventory",
                table: "stock_adjustments");

            migrationBuilder.RenameTable(
                name: "stock_reservations",
                schema: "inventory",
                newName: "StockReservations",
                newSchema: "inventory");

            migrationBuilder.RenameTable(
                name: "stock_items",
                schema: "inventory",
                newName: "StockItems",
                newSchema: "inventory");

            migrationBuilder.RenameTable(
                name: "stock_adjustments",
                schema: "inventory",
                newName: "StockAdjustments",
                newSchema: "inventory");

            migrationBuilder.RenameColumn(
                name: "quantity",
                schema: "inventory",
                table: "StockReservations",
                newName: "Quantity");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "inventory",
                table: "StockReservations",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "stock_item_id",
                schema: "inventory",
                table: "StockReservations",
                newName: "StockItemId");

            migrationBuilder.RenameColumn(
                name: "expires_at",
                schema: "inventory",
                table: "StockReservations",
                newName: "ExpiresAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "inventory",
                table: "StockReservations",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_stock_reservations_stock_item_id",
                schema: "inventory",
                table: "StockReservations",
                newName: "IX_StockReservations_StockItemId");

            migrationBuilder.RenameIndex(
                name: "ix_stock_reservations_expires_at",
                schema: "inventory",
                table: "StockReservations",
                newName: "IX_StockReservations_ExpiresAt");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "inventory",
                table: "StockItems",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "quantity_on_hand",
                schema: "inventory",
                table: "StockItems",
                newName: "QuantityOnHand");

            migrationBuilder.RenameColumn(
                name: "product_id",
                schema: "inventory",
                table: "StockItems",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "ix_stock_items_product_id",
                schema: "inventory",
                table: "StockItems",
                newName: "IX_StockItems_ProductId");

            migrationBuilder.RenameColumn(
                name: "reason",
                schema: "inventory",
                table: "StockAdjustments",
                newName: "Reason");

            migrationBuilder.RenameColumn(
                name: "adjustment",
                schema: "inventory",
                table: "StockAdjustments",
                newName: "Adjustment");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "inventory",
                table: "StockAdjustments",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "stock_item_id",
                schema: "inventory",
                table: "StockAdjustments",
                newName: "StockItemId");

            migrationBuilder.RenameColumn(
                name: "quantity_after",
                schema: "inventory",
                table: "StockAdjustments",
                newName: "QuantityAfter");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "inventory",
                table: "StockAdjustments",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "adjusted_by",
                schema: "inventory",
                table: "StockAdjustments",
                newName: "AdjustedBy");

            migrationBuilder.RenameIndex(
                name: "ix_stock_adjustments_stock_item_id",
                schema: "inventory",
                table: "StockAdjustments",
                newName: "IX_StockAdjustments_StockItemId");

            migrationBuilder.RenameIndex(
                name: "ix_stock_adjustments_created_at",
                schema: "inventory",
                table: "StockAdjustments",
                newName: "IX_StockAdjustments_CreatedAt");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StockReservations",
                schema: "inventory",
                table: "StockReservations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StockItems",
                schema: "inventory",
                table: "StockItems",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StockAdjustments",
                schema: "inventory",
                table: "StockAdjustments",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StockReservations_StockItems_StockItemId",
                schema: "inventory",
                table: "StockReservations",
                column: "StockItemId",
                principalSchema: "inventory",
                principalTable: "StockItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

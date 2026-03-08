using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MicroCommerce.ApiService.Features.Ordering.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCouponFieldsToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "coupon_code",
                schema: "ordering",
                table: "orders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "discount_amount",
                schema: "ordering",
                table: "orders",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "coupon_code",
                schema: "ordering",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "discount_amount",
                schema: "ordering",
                table: "orders");
        }
    }
}

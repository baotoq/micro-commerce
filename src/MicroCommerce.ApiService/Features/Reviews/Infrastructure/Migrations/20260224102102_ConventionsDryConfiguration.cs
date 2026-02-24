using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MicroCommerce.ApiService.Features.Reviews.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConventionsDryConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Reviews",
                schema: "reviews",
                table: "Reviews");

            migrationBuilder.RenameTable(
                name: "Reviews",
                schema: "reviews",
                newName: "reviews",
                newSchema: "reviews");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "reviews",
                table: "reviews",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                schema: "reviews",
                table: "reviews",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                schema: "reviews",
                table: "reviews",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                schema: "reviews",
                table: "reviews",
                newName: "product_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "reviews",
                table: "reviews",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_Reviews_UserId_ProductId",
                schema: "reviews",
                table: "reviews",
                newName: "ix_reviews_user_id_product_id");

            migrationBuilder.RenameIndex(
                name: "IX_Reviews_ProductId",
                schema: "reviews",
                table: "reviews",
                newName: "ix_reviews_product_id");

            migrationBuilder.RenameIndex(
                name: "IX_Reviews_CreatedAt",
                schema: "reviews",
                table: "reviews",
                newName: "ix_reviews_created_at");

            migrationBuilder.AddPrimaryKey(
                name: "pk_reviews",
                schema: "reviews",
                table: "reviews",
                column: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_reviews",
                schema: "reviews",
                table: "reviews");

            migrationBuilder.RenameTable(
                name: "reviews",
                schema: "reviews",
                newName: "Reviews",
                newSchema: "reviews");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "reviews",
                table: "Reviews",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                schema: "reviews",
                table: "Reviews",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                schema: "reviews",
                table: "Reviews",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "product_id",
                schema: "reviews",
                table: "Reviews",
                newName: "ProductId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "reviews",
                table: "Reviews",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_reviews_user_id_product_id",
                schema: "reviews",
                table: "Reviews",
                newName: "IX_Reviews_UserId_ProductId");

            migrationBuilder.RenameIndex(
                name: "ix_reviews_product_id",
                schema: "reviews",
                table: "Reviews",
                newName: "IX_Reviews_ProductId");

            migrationBuilder.RenameIndex(
                name: "ix_reviews_created_at",
                schema: "reviews",
                table: "Reviews",
                newName: "IX_Reviews_CreatedAt");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Reviews",
                schema: "reviews",
                table: "Reviews",
                column: "Id");
        }
    }
}

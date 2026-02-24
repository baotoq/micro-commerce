using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MicroCommerce.ApiService.Features.Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConventionsDryConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_categories_CategoryId",
                schema: "catalog",
                table: "Products");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Products",
                schema: "catalog",
                table: "Products");

            migrationBuilder.DropPrimaryKey(
                name: "PK_categories",
                schema: "catalog",
                table: "categories");

            migrationBuilder.RenameTable(
                name: "Products",
                schema: "catalog",
                newName: "products",
                newSchema: "catalog");

            migrationBuilder.RenameColumn(
                name: "Status",
                schema: "catalog",
                table: "products",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Sku",
                schema: "catalog",
                table: "products",
                newName: "sku");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "catalog",
                table: "products",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                schema: "catalog",
                table: "products",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "catalog",
                table: "products",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                schema: "catalog",
                table: "products",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "ReviewCount",
                schema: "catalog",
                table: "products",
                newName: "review_count");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                schema: "catalog",
                table: "products",
                newName: "image_url");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "catalog",
                table: "products",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                schema: "catalog",
                table: "products",
                newName: "category_id");

            migrationBuilder.RenameColumn(
                name: "AverageRating",
                schema: "catalog",
                table: "products",
                newName: "average_rating");

            migrationBuilder.RenameIndex(
                name: "IX_Products_Status",
                schema: "catalog",
                table: "products",
                newName: "ix_products_status");

            migrationBuilder.RenameIndex(
                name: "IX_Products_Sku",
                schema: "catalog",
                table: "products",
                newName: "ix_products_sku");

            migrationBuilder.RenameIndex(
                name: "IX_Products_CategoryId",
                schema: "catalog",
                table: "products",
                newName: "ix_products_category_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_products",
                schema: "catalog",
                table: "products",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_categories",
                schema: "catalog",
                table: "categories",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_products_categories_category_id",
                schema: "catalog",
                table: "products",
                column: "category_id",
                principalSchema: "catalog",
                principalTable: "categories",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_products_categories_category_id",
                schema: "catalog",
                table: "products");

            migrationBuilder.DropPrimaryKey(
                name: "pk_products",
                schema: "catalog",
                table: "products");

            migrationBuilder.DropPrimaryKey(
                name: "pk_categories",
                schema: "catalog",
                table: "categories");

            migrationBuilder.RenameTable(
                name: "products",
                schema: "catalog",
                newName: "Products",
                newSchema: "catalog");

            migrationBuilder.RenameColumn(
                name: "status",
                schema: "catalog",
                table: "Products",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "sku",
                schema: "catalog",
                table: "Products",
                newName: "Sku");

            migrationBuilder.RenameColumn(
                name: "name",
                schema: "catalog",
                table: "Products",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                schema: "catalog",
                table: "Products",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "catalog",
                table: "Products",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                schema: "catalog",
                table: "Products",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "review_count",
                schema: "catalog",
                table: "Products",
                newName: "ReviewCount");

            migrationBuilder.RenameColumn(
                name: "image_url",
                schema: "catalog",
                table: "Products",
                newName: "ImageUrl");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "catalog",
                table: "Products",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "category_id",
                schema: "catalog",
                table: "Products",
                newName: "CategoryId");

            migrationBuilder.RenameColumn(
                name: "average_rating",
                schema: "catalog",
                table: "Products",
                newName: "AverageRating");

            migrationBuilder.RenameIndex(
                name: "ix_products_status",
                schema: "catalog",
                table: "Products",
                newName: "IX_Products_Status");

            migrationBuilder.RenameIndex(
                name: "ix_products_sku",
                schema: "catalog",
                table: "Products",
                newName: "IX_Products_Sku");

            migrationBuilder.RenameIndex(
                name: "ix_products_category_id",
                schema: "catalog",
                table: "Products",
                newName: "IX_Products_CategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Products",
                schema: "catalog",
                table: "Products",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_categories",
                schema: "catalog",
                table: "categories",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_categories_CategoryId",
                schema: "catalog",
                table: "Products",
                column: "CategoryId",
                principalSchema: "catalog",
                principalTable: "categories",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

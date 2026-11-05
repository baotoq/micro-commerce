using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace MicroCommerce.ApiService.Features.Catalog.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "products",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    options = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    search_vector = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: true)
                        .Annotation("Npgsql:TsVectorConfig", "english")
                        .Annotation("Npgsql:TsVectorProperties", new[] { "name", "description" })
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products", x => x.id);
                    table.ForeignKey(
                        name: "FK_products_categories_category_id",
                        column: x => x.category_id,
                        principalSchema: "catalog",
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "product_images",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false),
                    format = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    uploaded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_images", x => x.id);
                    table.ForeignKey(
                        name: "FK_product_images_products_product_id",
                        column: x => x.product_id,
                        principalSchema: "catalog",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "variants",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false),
                    option_values = table.Column<List<string>>(type: "text[]", nullable: false),
                    price = table.Column<long>(type: "bigint", nullable: false),
                    stock = table.Column<int>(type: "integer", nullable: false),
                    weight_grams = table.Column<int>(type: "integer", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_variants", x => x.id);
                    table.ForeignKey(
                        name: "FK_variants_products_product_id",
                        column: x => x.product_id,
                        principalSchema: "catalog",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_product_images_product_id",
                schema: "catalog",
                table: "product_images",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_products_category_id",
                schema: "catalog",
                table: "products",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_products_merchant_id_created_at",
                schema: "catalog",
                table: "products",
                columns: new[] { "merchant_id", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_products_search_vector",
                schema: "catalog",
                table: "products",
                column: "search_vector")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "ix_products_status_created_at",
                schema: "catalog",
                table: "products",
                columns: new[] { "status", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_variants_product_id",
                schema: "catalog",
                table: "variants",
                column: "product_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "product_images",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "variants",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "products",
                schema: "catalog");
        }
    }
}

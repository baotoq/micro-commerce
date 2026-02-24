using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MicroCommerce.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class ConventionsDryConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_UserProfiles_UserProfileId",
                schema: "profiles",
                table: "Addresses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Addresses",
                schema: "profiles",
                table: "Addresses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserProfiles",
                schema: "profiles",
                table: "UserProfiles");

            migrationBuilder.RenameTable(
                name: "UserProfiles",
                schema: "profiles",
                newName: "user_profiles",
                newSchema: "profiles");

            migrationBuilder.RenameColumn(
                name: "Street",
                schema: "profiles",
                table: "Addresses",
                newName: "street");

            migrationBuilder.RenameColumn(
                name: "State",
                schema: "profiles",
                table: "Addresses",
                newName: "state");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "profiles",
                table: "Addresses",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Country",
                schema: "profiles",
                table: "Addresses",
                newName: "country");

            migrationBuilder.RenameColumn(
                name: "City",
                schema: "profiles",
                table: "Addresses",
                newName: "city");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "profiles",
                table: "Addresses",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ZipCode",
                schema: "profiles",
                table: "Addresses",
                newName: "zip_code");

            migrationBuilder.RenameColumn(
                name: "UserProfileId",
                schema: "profiles",
                table: "Addresses",
                newName: "user_profile_id");

            migrationBuilder.RenameColumn(
                name: "IsDefault",
                schema: "profiles",
                table: "Addresses",
                newName: "is_default");

            migrationBuilder.RenameIndex(
                name: "IX_Addresses_UserProfileId",
                schema: "profiles",
                table: "Addresses",
                newName: "ix_addresses_user_profile_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "profiles",
                table: "user_profiles",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                schema: "profiles",
                table: "user_profiles",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                schema: "profiles",
                table: "user_profiles",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "profiles",
                table: "user_profiles",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "AvatarUrl",
                schema: "profiles",
                table: "user_profiles",
                newName: "avatar_url");

            migrationBuilder.RenameIndex(
                name: "IX_UserProfiles_UserId",
                schema: "profiles",
                table: "user_profiles",
                newName: "ix_user_profiles_user_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_addresses",
                schema: "profiles",
                table: "Addresses",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_user_profiles",
                schema: "profiles",
                table: "user_profiles",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_addresses_user_profiles_user_profile_id",
                schema: "profiles",
                table: "Addresses",
                column: "user_profile_id",
                principalSchema: "profiles",
                principalTable: "user_profiles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_addresses_user_profiles_user_profile_id",
                schema: "profiles",
                table: "Addresses");

            migrationBuilder.DropPrimaryKey(
                name: "pk_addresses",
                schema: "profiles",
                table: "Addresses");

            migrationBuilder.DropPrimaryKey(
                name: "pk_user_profiles",
                schema: "profiles",
                table: "user_profiles");

            migrationBuilder.RenameTable(
                name: "user_profiles",
                schema: "profiles",
                newName: "UserProfiles",
                newSchema: "profiles");

            migrationBuilder.RenameColumn(
                name: "street",
                schema: "profiles",
                table: "Addresses",
                newName: "Street");

            migrationBuilder.RenameColumn(
                name: "state",
                schema: "profiles",
                table: "Addresses",
                newName: "State");

            migrationBuilder.RenameColumn(
                name: "name",
                schema: "profiles",
                table: "Addresses",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "country",
                schema: "profiles",
                table: "Addresses",
                newName: "Country");

            migrationBuilder.RenameColumn(
                name: "city",
                schema: "profiles",
                table: "Addresses",
                newName: "City");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "profiles",
                table: "Addresses",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "zip_code",
                schema: "profiles",
                table: "Addresses",
                newName: "ZipCode");

            migrationBuilder.RenameColumn(
                name: "user_profile_id",
                schema: "profiles",
                table: "Addresses",
                newName: "UserProfileId");

            migrationBuilder.RenameColumn(
                name: "is_default",
                schema: "profiles",
                table: "Addresses",
                newName: "IsDefault");

            migrationBuilder.RenameIndex(
                name: "ix_addresses_user_profile_id",
                schema: "profiles",
                table: "Addresses",
                newName: "IX_Addresses_UserProfileId");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "profiles",
                table: "UserProfiles",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                schema: "profiles",
                table: "UserProfiles",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                schema: "profiles",
                table: "UserProfiles",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "profiles",
                table: "UserProfiles",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "avatar_url",
                schema: "profiles",
                table: "UserProfiles",
                newName: "AvatarUrl");

            migrationBuilder.RenameIndex(
                name: "ix_user_profiles_user_id",
                schema: "profiles",
                table: "UserProfiles",
                newName: "IX_UserProfiles_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Addresses",
                schema: "profiles",
                table: "Addresses",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserProfiles",
                schema: "profiles",
                table: "UserProfiles",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_UserProfiles_UserProfileId",
                schema: "profiles",
                table: "Addresses",
                column: "UserProfileId",
                principalSchema: "profiles",
                principalTable: "UserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderAndSlugToCategoryAndStore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StoreOrder",
                table: "Stores",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "StoreSlug",
                table: "Stores",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CategoryOrder",
                table: "Categories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CategorySlug",
                table: "Categories",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Stores_StoreOrder",
                table: "Stores",
                column: "StoreOrder",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stores_StoreSlug",
                table: "Stores",
                column: "StoreSlug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_CategoryOrder",
                table: "Categories",
                column: "CategoryOrder",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_CategorySlug",
                table: "Categories",
                column: "CategorySlug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Stores_StoreOrder",
                table: "Stores");

            migrationBuilder.DropIndex(
                name: "IX_Stores_StoreSlug",
                table: "Stores");

            migrationBuilder.DropIndex(
                name: "IX_Categories_CategoryOrder",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_CategorySlug",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "StoreOrder",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "StoreSlug",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "CategoryOrder",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "CategorySlug",
                table: "Categories");
        }
    }
}

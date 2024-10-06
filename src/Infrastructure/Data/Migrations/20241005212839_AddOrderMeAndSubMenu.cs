using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderMeAndSubMenu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SubMenuOrder",
                table: "SubMenus",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SubMenuSlug",
                table: "SubMenus",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MenuOrder",
                table: "Menus",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SubMenus_SubMenuSlug",
                table: "SubMenus",
                column: "SubMenuSlug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SubMenus_SubMenuSlug",
                table: "SubMenus");

            migrationBuilder.DropColumn(
                name: "SubMenuOrder",
                table: "SubMenus");

            migrationBuilder.DropColumn(
                name: "SubMenuSlug",
                table: "SubMenus");

            migrationBuilder.DropColumn(
                name: "MenuOrder",
                table: "Menus");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRenameFranchise : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Franchise_franchiseId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Series_Franchise_franchiseId",
                table: "Series");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Franchise",
                table: "Franchise");

            migrationBuilder.RenameTable(
                name: "Franchise",
                newName: "Franchises");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Franchises",
                table: "Franchises",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Franchises_franchiseId",
                table: "Products",
                column: "franchiseId",
                principalTable: "Franchises",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Series_Franchises_franchiseId",
                table: "Series",
                column: "franchiseId",
                principalTable: "Franchises",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Franchises_franchiseId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Series_Franchises_franchiseId",
                table: "Series");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Franchises",
                table: "Franchises");

            migrationBuilder.RenameTable(
                name: "Franchises",
                newName: "Franchise");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Franchise",
                table: "Franchise",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Franchise_franchiseId",
                table: "Products",
                column: "franchiseId",
                principalTable: "Franchise",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Series_Franchise_franchiseId",
                table: "Series",
                column: "franchiseId",
                principalTable: "Franchise",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

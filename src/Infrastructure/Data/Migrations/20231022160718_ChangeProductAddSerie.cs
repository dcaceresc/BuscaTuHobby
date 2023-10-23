using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeProductAddSerie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "serieId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_serieId",
                table: "Products",
                column: "serieId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Series_serieId",
                table: "Products",
                column: "serieId",
                principalTable: "Series",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Series_serieId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_serieId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "serieId",
                table: "Products");
        }
    }
}

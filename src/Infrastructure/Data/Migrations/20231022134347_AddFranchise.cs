using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFranchise : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Series_serieId",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "serieId",
                table: "Products",
                newName: "franchiseId");

            migrationBuilder.RenameIndex(
                name: "IX_Products_serieId",
                table: "Products",
                newName: "IX_Products_franchiseId");

            migrationBuilder.AddColumn<int>(
                name: "franchiseId",
                table: "Series",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "size",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "targetAge",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Franchise",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Franchise", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Series_franchiseId",
                table: "Series",
                column: "franchiseId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Franchise_franchiseId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Series_Franchise_franchiseId",
                table: "Series");

            migrationBuilder.DropTable(
                name: "Franchise");

            migrationBuilder.DropIndex(
                name: "IX_Series_franchiseId",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "franchiseId",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "size",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "targetAge",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "franchiseId",
                table: "Products",
                newName: "serieId");

            migrationBuilder.RenameIndex(
                name: "IX_Products_franchiseId",
                table: "Products",
                newName: "IX_Products_serieId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Series_serieId",
                table: "Products",
                column: "serieId",
                principalTable: "Series",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeGunplaToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FavoriteGunplas_Gunplas_gunplaId",
                table: "FavoriteGunplas");

            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_Gunplas_gunplaId",
                table: "Inventories");

            migrationBuilder.DropForeignKey(
                name: "FK_Photos_Gunplas_gunplaId",
                table: "Photos");

            migrationBuilder.DropTable(
                name: "Gunplas");

            migrationBuilder.RenameColumn(
                name: "gunplaId",
                table: "Photos",
                newName: "productId");

            migrationBuilder.RenameIndex(
                name: "IX_Photos_gunplaId",
                table: "Photos",
                newName: "IX_Photos_productId");

            migrationBuilder.RenameColumn(
                name: "gunplaId",
                table: "Inventories",
                newName: "productId");

            migrationBuilder.RenameIndex(
                name: "IX_Inventories_gunplaId",
                table: "Inventories",
                newName: "IX_Inventories_productId");

            migrationBuilder.RenameColumn(
                name: "gunplaId",
                table: "FavoriteGunplas",
                newName: "productId");

            migrationBuilder.RenameIndex(
                name: "IX_FavoriteGunplas_gunplaId",
                table: "FavoriteGunplas",
                newName: "IX_FavoriteGunplas_productId");

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    gradeId = table.Column<int>(type: "int", nullable: false),
                    scaleId = table.Column<int>(type: "int", nullable: false),
                    manufacturerId = table.Column<int>(type: "int", nullable: false),
                    serieId = table.Column<int>(type: "int", nullable: false),
                    hasBase = table.Column<bool>(type: "bit", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    releaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    actve = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.id);
                    table.ForeignKey(
                        name: "FK_Products_Grades_gradeId",
                        column: x => x.gradeId,
                        principalTable: "Grades",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Products_Manufacturers_manufacturerId",
                        column: x => x.manufacturerId,
                        principalTable: "Manufacturers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Products_Scales_scaleId",
                        column: x => x.scaleId,
                        principalTable: "Scales",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Products_Series_serieId",
                        column: x => x.serieId,
                        principalTable: "Series",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_gradeId",
                table: "Products",
                column: "gradeId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_manufacturerId",
                table: "Products",
                column: "manufacturerId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_scaleId",
                table: "Products",
                column: "scaleId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_serieId",
                table: "Products",
                column: "serieId");

            migrationBuilder.AddForeignKey(
                name: "FK_FavoriteGunplas_Products_productId",
                table: "FavoriteGunplas",
                column: "productId",
                principalTable: "Products",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_Products_productId",
                table: "Inventories",
                column: "productId",
                principalTable: "Products",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Photos_Products_productId",
                table: "Photos",
                column: "productId",
                principalTable: "Products",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FavoriteGunplas_Products_productId",
                table: "FavoriteGunplas");

            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_Products_productId",
                table: "Inventories");

            migrationBuilder.DropForeignKey(
                name: "FK_Photos_Products_productId",
                table: "Photos");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.RenameColumn(
                name: "productId",
                table: "Photos",
                newName: "gunplaId");

            migrationBuilder.RenameIndex(
                name: "IX_Photos_productId",
                table: "Photos",
                newName: "IX_Photos_gunplaId");

            migrationBuilder.RenameColumn(
                name: "productId",
                table: "Inventories",
                newName: "gunplaId");

            migrationBuilder.RenameIndex(
                name: "IX_Inventories_productId",
                table: "Inventories",
                newName: "IX_Inventories_gunplaId");

            migrationBuilder.RenameColumn(
                name: "productId",
                table: "FavoriteGunplas",
                newName: "gunplaId");

            migrationBuilder.RenameIndex(
                name: "IX_FavoriteGunplas_productId",
                table: "FavoriteGunplas",
                newName: "IX_FavoriteGunplas_gunplaId");

            migrationBuilder.CreateTable(
                name: "Gunplas",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    gradeId = table.Column<int>(type: "int", nullable: false),
                    manufacturerId = table.Column<int>(type: "int", nullable: false),
                    scaleId = table.Column<int>(type: "int", nullable: false),
                    serieId = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    actve = table.Column<bool>(type: "bit", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    hasBase = table.Column<bool>(type: "bit", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    releaseDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gunplas", x => x.id);
                    table.ForeignKey(
                        name: "FK_Gunplas_Grades_gradeId",
                        column: x => x.gradeId,
                        principalTable: "Grades",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Gunplas_Manufacturers_manufacturerId",
                        column: x => x.manufacturerId,
                        principalTable: "Manufacturers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Gunplas_Scales_scaleId",
                        column: x => x.scaleId,
                        principalTable: "Scales",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Gunplas_Series_serieId",
                        column: x => x.serieId,
                        principalTable: "Series",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Gunplas_gradeId",
                table: "Gunplas",
                column: "gradeId");

            migrationBuilder.CreateIndex(
                name: "IX_Gunplas_manufacturerId",
                table: "Gunplas",
                column: "manufacturerId");

            migrationBuilder.CreateIndex(
                name: "IX_Gunplas_scaleId",
                table: "Gunplas",
                column: "scaleId");

            migrationBuilder.CreateIndex(
                name: "IX_Gunplas_serieId",
                table: "Gunplas",
                column: "serieId");

            migrationBuilder.AddForeignKey(
                name: "FK_FavoriteGunplas_Gunplas_gunplaId",
                table: "FavoriteGunplas",
                column: "gunplaId",
                principalTable: "Gunplas",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_Gunplas_gunplaId",
                table: "Inventories",
                column: "gunplaId",
                principalTable: "Gunplas",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Photos_Gunplas_gunplaId",
                table: "Photos",
                column: "gunplaId",
                principalTable: "Gunplas",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeGradeScaleAndAddSubCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FavoriteGunplas_Favorites_favoriteId",
                table: "FavoriteGunplas");

            migrationBuilder.DropForeignKey(
                name: "FK_FavoriteGunplas_Products_productId",
                table: "FavoriteGunplas");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Grades_gradeId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "Grades");

            migrationBuilder.DropIndex(
                name: "IX_Products_gradeId",
                table: "Products");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FavoriteGunplas",
                table: "FavoriteGunplas");

            migrationBuilder.DropColumn(
                name: "acronym",
                table: "Scales");

            migrationBuilder.DropColumn(
                name: "gradeId",
                table: "Products");

            migrationBuilder.RenameTable(
                name: "FavoriteGunplas",
                newName: "FavoriteProducts");

            migrationBuilder.RenameColumn(
                name: "actve",
                table: "Products",
                newName: "active");

            migrationBuilder.RenameIndex(
                name: "IX_FavoriteGunplas_productId",
                table: "FavoriteProducts",
                newName: "IX_FavoriteProducts_productId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FavoriteProducts",
                table: "FavoriteProducts",
                columns: new[] { "favoriteId", "productId" });

            migrationBuilder.CreateTable(
                name: "Categories",
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
                    table.PrimaryKey("PK_Categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "SubCategories",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    categoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubCategories", x => x.id);
                    table.ForeignKey(
                        name: "FK_SubCategories_Categories_categoryId",
                        column: x => x.categoryId,
                        principalTable: "Categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubCategoryProducts",
                columns: table => new
                {
                    productId = table.Column<int>(type: "int", nullable: false),
                    subCategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubCategoryProducts", x => new { x.subCategoryId, x.productId });
                    table.ForeignKey(
                        name: "FK_SubCategoryProducts_Products_productId",
                        column: x => x.productId,
                        principalTable: "Products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubCategoryProducts_SubCategories_subCategoryId",
                        column: x => x.subCategoryId,
                        principalTable: "SubCategories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubCategories_categoryId",
                table: "SubCategories",
                column: "categoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SubCategoryProducts_productId",
                table: "SubCategoryProducts",
                column: "productId");

            migrationBuilder.AddForeignKey(
                name: "FK_FavoriteProducts_Favorites_favoriteId",
                table: "FavoriteProducts",
                column: "favoriteId",
                principalTable: "Favorites",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FavoriteProducts_Products_productId",
                table: "FavoriteProducts",
                column: "productId",
                principalTable: "Products",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FavoriteProducts_Favorites_favoriteId",
                table: "FavoriteProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_FavoriteProducts_Products_productId",
                table: "FavoriteProducts");

            migrationBuilder.DropTable(
                name: "SubCategoryProducts");

            migrationBuilder.DropTable(
                name: "SubCategories");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FavoriteProducts",
                table: "FavoriteProducts");

            migrationBuilder.RenameTable(
                name: "FavoriteProducts",
                newName: "FavoriteGunplas");

            migrationBuilder.RenameColumn(
                name: "active",
                table: "Products",
                newName: "actve");

            migrationBuilder.RenameIndex(
                name: "IX_FavoriteProducts_productId",
                table: "FavoriteGunplas",
                newName: "IX_FavoriteGunplas_productId");

            migrationBuilder.AddColumn<string>(
                name: "acronym",
                table: "Scales",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "gradeId",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_FavoriteGunplas",
                table: "FavoriteGunplas",
                columns: new[] { "favoriteId", "productId" });

            migrationBuilder.CreateTable(
                name: "Grades",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    acronym = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grades", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_gradeId",
                table: "Products",
                column: "gradeId");

            migrationBuilder.AddForeignKey(
                name: "FK_FavoriteGunplas_Favorites_favoriteId",
                table: "FavoriteGunplas",
                column: "favoriteId",
                principalTable: "Favorites",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FavoriteGunplas_Products_productId",
                table: "FavoriteGunplas",
                column: "productId",
                principalTable: "Products",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Grades_gradeId",
                table: "Products",
                column: "gradeId",
                principalTable: "Grades",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

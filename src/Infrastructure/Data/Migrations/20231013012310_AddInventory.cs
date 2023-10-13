using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GunplaPrices");

            migrationBuilder.DropTable(
                name: "Sales");

            migrationBuilder.AddColumn<string>(
                name: "acronym",
                table: "Scales",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Inventories",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    storeId = table.Column<int>(type: "int", nullable: false),
                    gunplaId = table.Column<int>(type: "int", nullable: false),
                    price = table.Column<int>(type: "int", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventories", x => x.id);
                    table.ForeignKey(
                        name: "FK_Inventories_Gunplas_gunplaId",
                        column: x => x.gunplaId,
                        principalTable: "Gunplas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Inventories_Stores_storeId",
                        column: x => x.storeId,
                        principalTable: "Stores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_gunplaId",
                table: "Inventories",
                column: "gunplaId");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_storeId",
                table: "Inventories",
                column: "storeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Inventories");

            migrationBuilder.DropColumn(
                name: "acronym",
                table: "Scales");

            migrationBuilder.CreateTable(
                name: "GunplaPrices",
                columns: table => new
                {
                    gunplaId = table.Column<int>(type: "int", nullable: false),
                    storeId = table.Column<int>(type: "int", nullable: false),
                    price = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GunplaPrices", x => new { x.gunplaId, x.storeId });
                    table.ForeignKey(
                        name: "FK_GunplaPrices_Gunplas_gunplaId",
                        column: x => x.gunplaId,
                        principalTable: "Gunplas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GunplaPrices_Stores_storeId",
                        column: x => x.storeId,
                        principalTable: "Stores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sales",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    gunplaId = table.Column<int>(type: "int", nullable: false),
                    storeId = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    price = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sales", x => x.id);
                    table.ForeignKey(
                        name: "FK_Sales_Gunplas_gunplaId",
                        column: x => x.gunplaId,
                        principalTable: "Gunplas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Sales_Stores_storeId",
                        column: x => x.storeId,
                        principalTable: "Stores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GunplaPrices_storeId",
                table: "GunplaPrices",
                column: "storeId");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_gunplaId",
                table: "Sales",
                column: "gunplaId");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_storeId",
                table: "Sales",
                column: "storeId");
        }
    }
}

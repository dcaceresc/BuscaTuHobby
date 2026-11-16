using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations;

// Prepared manually without dotnet-ef. Review generated SQL before applying to an isolated SQL Server.
public partial class InventoryPriceHistory : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "InventoryPriceHistories",
            columns: table => new
            {
                PriceHistoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                InventoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Price = table.Column<int>(type: "int", nullable: false),
                OriginalPrice = table.Column<int>(type: "int", nullable: false),
                DiscountPercentage = table.Column<int>(type: "int", nullable: false),
                Availability = table.Column<bool>(type: "bit", nullable: true),
                ObservedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                IsBaseline = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_InventoryPriceHistories", x => x.PriceHistoryId);
                table.ForeignKey(
                    name: "FK_InventoryPriceHistories_Inventories_InventoryId",
                    column: x => x.InventoryId,
                    principalTable: "Inventories",
                    principalColumn: "InventoryId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_InventoryPriceHistories_InventoryId_ObservedAtUtc",
            table: "InventoryPriceHistories",
            columns: ["InventoryId", "ObservedAtUtc"]);

        // One baseline point per active offer, copied from its current values at the migration instant.
        // The NOT EXISTS guard keeps the statement harmless if the generated script is re-run.
        migrationBuilder.Sql("""
            INSERT INTO [InventoryPriceHistories]
                ([PriceHistoryId], [InventoryId], [Price], [OriginalPrice], [DiscountPercentage], [Availability], [ObservedAtUtc], [IsBaseline])
            SELECT NEWID(), i.[InventoryId], i.[Price], i.[OriginalPrice], i.[DiscountPercentage], i.[Availability], SYSUTCDATETIME(), 1
            FROM [Inventories] AS i
            WHERE i.[IsActive] = 1
              AND NOT EXISTS (
                  SELECT 1
                  FROM [InventoryPriceHistories] AS h
                  WHERE h.[InventoryId] = i.[InventoryId] AND h.[IsBaseline] = 1
              );
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Dropping the table removes the seeded baseline rows and the series index with it.
        migrationBuilder.DropTable(name: "InventoryPriceHistories");
    }
}

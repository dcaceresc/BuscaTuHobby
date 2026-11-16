using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations;

// Preparada manualmente sin dotnet-ef. Revisar el SQL generado antes de aplicarla a un SQL Server aislado.
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

        // Un punto de línea base por oferta activa, copiado de sus valores actuales en el instante de la migración.
        // La guarda NOT EXISTS mantiene la sentencia inofensiva si el script generado se vuelve a ejecutar.
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
        // Eliminar la tabla borra las filas de línea base sembradas y, con ellas, el índice de la serie.
        migrationBuilder.DropTable(name: "InventoryPriceHistories");
    }
}

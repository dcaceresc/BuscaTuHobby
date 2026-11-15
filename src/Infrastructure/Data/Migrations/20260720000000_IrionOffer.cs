using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations;

// Prepared manually without dotnet-ef. Review generated SQL before applying to an isolated SQL Server.
public partial class IrionOffer : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Fail before any schema changes; administrators must resolve duplicates explicitly.
        migrationBuilder.Sql("""
            IF EXISTS (
                SELECT [StoreId], [ProductId]
                FROM [Inventories]
                GROUP BY [StoreId], [ProductId]
                HAVING COUNT(*) > 1
            )
                THROW 51000, N'Existen inventarios duplicados por tienda y producto. Resuélvalos antes de aplicar la migración.', 1;
            """);

        migrationBuilder.AddColumn<string>(name: "SourceUrl", table: "Inventories", type: "nvarchar(2048)", maxLength: 2048, nullable: true);
        migrationBuilder.AddColumn<string>(name: "ExternalVariantId", table: "Inventories", type: "nvarchar(128)", maxLength: 128, nullable: true);
        migrationBuilder.AddColumn<bool>(name: "Availability", table: "Inventories", type: "bit", nullable: true);
        migrationBuilder.AddColumn<DateTime>(name: "ObservedAtUtc", table: "Inventories", type: "datetime2", nullable: true);
        migrationBuilder.AddColumn<byte[]>(name: "RowVersion", table: "Inventories", type: "rowversion", rowVersion: true, nullable: false);
        migrationBuilder.DropIndex(name: "IX_Inventories_StoreId", table: "Inventories");
        migrationBuilder.CreateIndex(name: "IX_Inventories_StoreId_ProductId", table: "Inventories", columns: ["StoreId", "ProductId"], unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_Inventories_StoreId_ProductId", table: "Inventories");
        migrationBuilder.CreateIndex(name: "IX_Inventories_StoreId", table: "Inventories", column: "StoreId");
        migrationBuilder.DropColumn(name: "SourceUrl", table: "Inventories");
        migrationBuilder.DropColumn(name: "ExternalVariantId", table: "Inventories");
        migrationBuilder.DropColumn(name: "Availability", table: "Inventories");
        migrationBuilder.DropColumn(name: "ObservedAtUtc", table: "Inventories");
        migrationBuilder.DropColumn(name: "RowVersion", table: "Inventories");
    }
}

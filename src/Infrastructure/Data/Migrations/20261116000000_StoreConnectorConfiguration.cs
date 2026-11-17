using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations;

// Preparada manualmente sin dotnet-ef. Revisar el SQL generado antes de aplicarla a un SQL Server aislado.
public partial class StoreConnectorConfiguration : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Nullable a propósito: una tienda sin conector no participa en la ingesta automática, y las filas que
        // ya existían no pierden nada por no tener plataforma declarada.
        migrationBuilder.AddColumn<string>(
            name: "ConnectorKey",
            table: "Stores",
            type: "nvarchar(32)",
            maxLength: 32,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ConnectorSettings",
            table: "Stores",
            type: "nvarchar(4000)",
            maxLength: 4000,
            nullable: true);

        // La importación manual de irion.cl debe seguir funcionando en instalaciones que ya tienen su fila, sin
        // que nadie edite filas a mano. El resto de tiendas conserva la ingesta apagada hasta que se configure.
        migrationBuilder.Sql(
            "UPDATE [Stores] SET [ConnectorKey] = 'shopify', [ConnectorSettings] = '{\"currency\":\"CLP\"}' " +
            "WHERE [ConnectorKey] IS NULL AND [StoreWebSite] LIKE '%irion.cl%';");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ConnectorSettings",
            table: "Stores");

        migrationBuilder.DropColumn(
            name: "ConnectorKey",
            table: "Stores");
    }
}

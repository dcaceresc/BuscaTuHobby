using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations;

// Preparada manualmente sin dotnet-ef. Revisar el SQL generado antes de aplicarla a un SQL Server aislado.
//
// Decisión de índice, registrada aquí porque es una decisión de esquema y no de código: el GTIN NO lleva índice
// único. El mismo código de barras puede aparecer legítimamente en más de una fila del catálogo (la misma caja
// publicada por dos tiendas con títulos distintos, o repetida dentro de una misma tienda). Una restricción única
// convertiría esa duplicidad real en un fallo de escritura de la ingesta; en cambio la ingesta empareja por GTIN y,
// cuando las referencias no son concluyentes, aparca el elemento para revisión humana. Por eso tampoco se crea un
// índice no único: hoy no hay ninguna consulta que lo necesite y la ingesta carga el catálogo una vez por ejecución.
public partial class ProductGtin : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Nullable y aditivo: null significa "la tienda no informó código de barras", que es el estado de todas las
        // filas anteriores a esta columna. Sin valor predeterminado para no inventar un identificador.
        migrationBuilder.AddColumn<string>(
            name: "Gtin",
            table: "Products",
            type: "nvarchar(14)",
            maxLength: 14,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Gtin",
            table: "Products");
    }
}

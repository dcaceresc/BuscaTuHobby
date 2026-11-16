using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations;

// Preparada manualmente sin dotnet-ef. Revisar el SQL generado antes de aplicarla a un SQL Server aislado.
public partial class OfferConditionAndPaymentMethod : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // La condición de una oferta es un entero con el mismo orden que publica la API: 0 nueva, 1 refabricada,
        // 2 usada. El valor predeterminado deja como "nueva" a toda fila que ya existía, porque no hay
        // información histórica de condición y esa es la suposición menos sorprendente.
        migrationBuilder.AddColumn<int>(
            name: "Condition",
            table: "Inventories",
            type: "int",
            nullable: false,
            defaultValue: 0);

        // La etiqueta del medio de pago es opcional: null significa que la oferta no informa uno.
        migrationBuilder.AddColumn<string>(
            name: "PaymentMethodLabel",
            table: "Inventories",
            type: "nvarchar(64)",
            maxLength: 64,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "PaymentMethodLabel",
            table: "Inventories");

        migrationBuilder.DropColumn(
            name: "Condition",
            table: "Inventories");
    }
}

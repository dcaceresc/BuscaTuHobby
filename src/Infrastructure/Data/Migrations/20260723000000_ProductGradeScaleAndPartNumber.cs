using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations;

// Preparada manualmente sin dotnet-ef. Revisar el SQL generado antes de aplicarla a un SQL Server aislado.
public partial class ProductGradeScaleAndPartNumber : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // El grado de la maqueta es un entero con el mismo orden que publica la API: 0 sin grado, 1 HG, 2 MG,
        // 3 RG, 4 PG, 5 SD, 6 otro. El valor predeterminado deja como "sin grado" a toda fila que ya existía,
        // porque no hay información histórica de grado y esa es la suposición menos sorprendente.
        migrationBuilder.AddColumn<int>(
            name: "Grade",
            table: "Products",
            type: "int",
            nullable: false,
            defaultValue: 0);

        // La escala y el número de parte son opcionales: null significa que el producto no los informa.
        migrationBuilder.AddColumn<string>(
            name: "Scale",
            table: "Products",
            type: "nvarchar(16)",
            maxLength: 16,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "PartNumber",
            table: "Products",
            type: "nvarchar(64)",
            maxLength: 64,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "PartNumber",
            table: "Products");

        migrationBuilder.DropColumn(
            name: "Scale",
            table: "Products");

        migrationBuilder.DropColumn(
            name: "Grade",
            table: "Products");
    }
}

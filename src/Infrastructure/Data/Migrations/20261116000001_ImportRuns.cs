using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations;

// Preparada manualmente sin dotnet-ef. Revisar el SQL generado antes de aplicarla a un SQL Server aislado.
public partial class ImportRuns : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // La ejecución guarda con qué presupuesto arrancó y qué encontró. Sin esta fila no se puede auditar una
        // ingesta ni decidir qué quedó pendiente cuando el proceso se corta a mitad.
        migrationBuilder.CreateTable(
            name: "ImportRuns",
            columns: table => new
            {
                ImportRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                StartedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                FinishedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                Status = table.Column<int>(type: "int", nullable: false),
                RequestBudget = table.Column<int>(type: "int", nullable: false),
                DiscoveredCount = table.Column<int>(type: "int", nullable: false),
                StoredCount = table.Column<int>(type: "int", nullable: false),
                SkippedCount = table.Column<int>(type: "int", nullable: false),
                FailedCount = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ImportRuns", x => x.ImportRunId);

                // Restrict y no Cascade: borrar una tienda no puede borrar la auditoría de sus ingestas.
                table.ForeignKey(
                    name: "FK_ImportRuns_Stores_StoreId",
                    column: x => x.StoreId,
                    principalTable: "Stores",
                    principalColumn: "StoreId",
                    onDelete: ReferentialAction.Restrict);
            });

        // Un elemento por URL y ejecución. El estado es el número del enumerado y el error es nulo cuando no hubo
        // fallo ni descarte, para que una fila guardada no arrastre texto que ya no aplica.
        migrationBuilder.CreateTable(
            name: "ImportItems",
            columns: table => new
            {
                ImportItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                ImportRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SourceUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                State = table.Column<int>(type: "int", nullable: false),
                HttpStatus = table.Column<int>(type: "int", nullable: true),
                ErrorMessage = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                AttemptedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ImportItems", x => x.ImportItemId);

                table.ForeignKey(
                    name: "FK_ImportItems_ImportRuns_ImportRunId",
                    column: x => x.ImportRunId,
                    principalTable: "ImportRuns",
                    principalColumn: "ImportRunId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ImportRuns_StoreId",
            table: "ImportRuns",
            column: "StoreId");

        // El índice único es la garantía de que reanudar o repetir el recorrido no duplique una URL en la ejecución.
        migrationBuilder.CreateIndex(
            name: "IX_ImportItems_ImportRunId_SourceUrl",
            table: "ImportItems",
            columns: ["ImportRunId", "SourceUrl"],
            unique: true);

        // Reanudación: resolver los pendientes de una ejecución sin recorrer la tabla completa.
        migrationBuilder.CreateIndex(
            name: "IX_ImportItems_ImportRunId_State",
            table: "ImportItems",
            columns: ["ImportRunId", "State"]);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Los elementos primero: son los que declaran la clave foránea hacia la ejecución.
        migrationBuilder.DropTable(name: "ImportItems");

        migrationBuilder.DropTable(name: "ImportRuns");
    }
}

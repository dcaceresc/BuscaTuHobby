using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class DeleteUniverse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Series_Universes_universeId",
                table: "Series");

            migrationBuilder.DropTable(
                name: "Universes");

            migrationBuilder.DropIndex(
                name: "IX_Series_universeId",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "universeId",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "acronym",
                table: "Manufacturers");

            migrationBuilder.AddColumn<bool>(
                name: "active",
                table: "SubCategories",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "active",
                table: "SubCategories");

            migrationBuilder.AddColumn<int>(
                name: "universeId",
                table: "Series",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "acronym",
                table: "Manufacturers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Universes",
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
                    table.PrimaryKey("PK_Universes", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Series_universeId",
                table: "Series",
                column: "universeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Series_Universes_universeId",
                table: "Series",
                column: "universeId",
                principalTable: "Universes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

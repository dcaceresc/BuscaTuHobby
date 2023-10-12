using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "acronym",
                table: "Universes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "active",
                table: "Universes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "active",
                table: "Stores",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "active",
                table: "Series",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "active",
                table: "Scales",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "acronym",
                table: "Manufacturers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "active",
                table: "Manufacturers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "actve",
                table: "Gunplas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "active",
                table: "Grades",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "acronym",
                table: "Universes");

            migrationBuilder.DropColumn(
                name: "active",
                table: "Universes");

            migrationBuilder.DropColumn(
                name: "active",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "active",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "active",
                table: "Scales");

            migrationBuilder.DropColumn(
                name: "acronym",
                table: "Manufacturers");

            migrationBuilder.DropColumn(
                name: "active",
                table: "Manufacturers");

            migrationBuilder.DropColumn(
                name: "actve",
                table: "Gunplas");

            migrationBuilder.DropColumn(
                name: "active",
                table: "Grades");
        }
    }
}

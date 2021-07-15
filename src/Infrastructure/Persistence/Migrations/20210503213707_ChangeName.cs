using Microsoft.EntityFrameworkCore.Migrations;

namespace Infrastructure.Persistence.Migrations
{
    public partial class ChangeName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Photo_Gunplas_GunplaId",
                table: "Photo");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Photo",
                table: "Photo");

            migrationBuilder.RenameTable(
                name: "Photo",
                newName: "Photos");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "Universes",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "Series",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "Scales",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "Manufacturers",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "Gunplas",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "Siglas",
                table: "Grades",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "Grades",
                newName: "Acronym");

            migrationBuilder.RenameIndex(
                name: "IX_Photo_GunplaId",
                table: "Photos",
                newName: "IX_Photos_GunplaId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Photos",
                table: "Photos",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Photos_Gunplas_GunplaId",
                table: "Photos",
                column: "GunplaId",
                principalTable: "Gunplas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Photos_Gunplas_GunplaId",
                table: "Photos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Photos",
                table: "Photos");

            migrationBuilder.RenameTable(
                name: "Photos",
                newName: "Photo");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Universes",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Series",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Scales",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Manufacturers",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Gunplas",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Grades",
                newName: "Siglas");

            migrationBuilder.RenameColumn(
                name: "Acronym",
                table: "Grades",
                newName: "Nombre");

            migrationBuilder.RenameIndex(
                name: "IX_Photos_GunplaId",
                table: "Photo",
                newName: "IX_Photo_GunplaId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Photo",
                table: "Photo",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Photo_Gunplas_GunplaId",
                table: "Photo",
                column: "GunplaId",
                principalTable: "Gunplas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

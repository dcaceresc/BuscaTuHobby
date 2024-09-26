using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeCityForCommune : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoresAddresses_Cities_CityId",
                table: "StoresAddresses");

            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.RenameColumn(
                name: "CityId",
                table: "StoresAddresses",
                newName: "CommuneId");

            migrationBuilder.RenameIndex(
                name: "IX_StoresAddresses_CityId",
                table: "StoresAddresses",
                newName: "IX_StoresAddresses_CommuneId");

            migrationBuilder.AddColumn<int>(
                name: "RegionOrder",
                table: "Regions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Communes",
                columns: table => new
                {
                    CommuneId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    CommuneName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RegionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Communes", x => x.CommuneId);
                    table.ForeignKey(
                        name: "FK_Communes_Regions_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Regions",
                        principalColumn: "RegionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Communes_CommuneName",
                table: "Communes",
                column: "CommuneName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Communes_RegionId",
                table: "Communes",
                column: "RegionId");

            migrationBuilder.AddForeignKey(
                name: "FK_StoresAddresses_Communes_CommuneId",
                table: "StoresAddresses",
                column: "CommuneId",
                principalTable: "Communes",
                principalColumn: "CommuneId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoresAddresses_Communes_CommuneId",
                table: "StoresAddresses");

            migrationBuilder.DropTable(
                name: "Communes");

            migrationBuilder.DropColumn(
                name: "RegionOrder",
                table: "Regions");

            migrationBuilder.RenameColumn(
                name: "CommuneId",
                table: "StoresAddresses",
                newName: "CityId");

            migrationBuilder.RenameIndex(
                name: "IX_StoresAddresses_CommuneId",
                table: "StoresAddresses",
                newName: "IX_StoresAddresses_CityId");

            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    CityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    RegionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.CityId);
                    table.ForeignKey(
                        name: "FK_Cities_Regions_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Regions",
                        principalColumn: "RegionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cities_CityName",
                table: "Cities",
                column: "CityName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cities_RegionId",
                table: "Cities",
                column: "RegionId");

            migrationBuilder.AddForeignKey(
                name: "FK_StoresAddresses_Cities_CityId",
                table: "StoresAddresses",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "CityId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

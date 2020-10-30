using Microsoft.EntityFrameworkCore.Migrations;

namespace BoschApi.Entities.Migrations
{
    public partial class CrowdDensityLevelUpdated : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Date",
                table: "CrowdDensityLevels",
                newName: "Timestamp");

            migrationBuilder.AlterColumn<int>(
                name: "Level",
                table: "CrowdDensityLevels",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "CrowdDensityLevels",
                newName: "Date");

            migrationBuilder.AlterColumn<string>(
                name: "Level",
                table: "CrowdDensityLevels",
                nullable: true,
                oldClrType: typeof(int));
        }
    }
}

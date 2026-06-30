using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ucms.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMeasurementUnitFkToWorkType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_WorkTypes_MeasurementUnitId",
                table: "WorkTypes",
                column: "MeasurementUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkTypes_MeasurementUnits_MeasurementUnitId",
                table: "WorkTypes",
                column: "MeasurementUnitId",
                principalTable: "MeasurementUnits",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkTypes_MeasurementUnits_MeasurementUnitId",
                table: "WorkTypes");

            migrationBuilder.DropIndex(
                name: "IX_WorkTypes_MeasurementUnitId",
                table: "WorkTypes");
        }
    }
}

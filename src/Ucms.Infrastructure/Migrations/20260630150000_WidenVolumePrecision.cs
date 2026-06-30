using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ucms.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class WidenVolumePrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Volume",
                table: "EstimateItems",
                type: "numeric(28,12)",
                precision: 28,
                scale: 12,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "Volume",
                table: "WorkLogs",
                type: "numeric(28,12)",
                precision: 28,
                scale: 12,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "Volume",
                table: "ClientActItems",
                type: "numeric(28,12)",
                precision: 28,
                scale: 12,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Volume",
                table: "EstimateItems",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(28,12)",
                oldPrecision: 28,
                oldScale: 12);

            migrationBuilder.AlterColumn<decimal>(
                name: "Volume",
                table: "WorkLogs",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(28,12)",
                oldPrecision: 28,
                oldScale: 12);

            migrationBuilder.AlterColumn<decimal>(
                name: "Volume",
                table: "ClientActItems",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(28,12)",
                oldPrecision: 28,
                oldScale: 12);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ucms.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPartnerNameToCashTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PartnerName",
                table: "CashTransactions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PartnerName",
                table: "CashTransactions");
        }
    }
}

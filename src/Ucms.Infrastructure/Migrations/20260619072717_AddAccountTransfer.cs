using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ucms.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountTransfer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountTransfers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Commission = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TransferredBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Note = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountTransfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountTransfers_CashAccounts_FromAccountId",
                        column: x => x.FromAccountId,
                        principalTable: "CashAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccountTransfers_CashAccounts_ToAccountId",
                        column: x => x.ToAccountId,
                        principalTable: "CashAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccountTransfers_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountTransfers_Date",
                table: "AccountTransfers",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_AccountTransfers_FromAccountId",
                table: "AccountTransfers",
                column: "FromAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountTransfers_OrganizationId",
                table: "AccountTransfers",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountTransfers_ToAccountId",
                table: "AccountTransfers",
                column: "ToAccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountTransfers");
        }
    }
}

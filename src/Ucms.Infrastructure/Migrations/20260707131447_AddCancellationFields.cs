using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ucms.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCancellationFields : Migration
    {
        private static readonly string[] Tables =
        [
            "BrigadePayments",
            "ClientPayments",
            "Salaries",
            "ProjectExpenses",
            "AccountTransfers",
            "CashTransactions",
        ];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var table in Tables)
            {
                migrationBuilder.AddColumn<bool>(
                    name: "IsCancelled",
                    table: table,
                    type: "boolean",
                    nullable: false,
                    defaultValue: false);

                migrationBuilder.AddColumn<DateTimeOffset>(
                    name: "CancelledAt",
                    table: table,
                    type: "timestamp with time zone",
                    nullable: true);

                migrationBuilder.AddColumn<Guid>(
                    name: "CancelledBy",
                    table: table,
                    type: "uuid",
                    nullable: true);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var table in Tables)
            {
                migrationBuilder.DropColumn(name: "IsCancelled",  table: table);
                migrationBuilder.DropColumn(name: "CancelledAt",  table: table);
                migrationBuilder.DropColumn(name: "CancelledBy",  table: table);
            }
        }
    }
}

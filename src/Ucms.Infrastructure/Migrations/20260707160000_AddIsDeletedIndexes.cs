namespace Ucms.Infrastructure.Migrations;

using Microsoft.EntityFrameworkCore.Migrations;

/// <summary>
/// IDeletable barcha entity jadvallariga "IsDeleted" ustuni bo'yicha index qo'shadi.
/// PostgreSQL CONCURRENTLY — jadval lock bo'lmaydi, production-da xavfsiz.
/// </summary>
public partial class AddIsDeletedIndexes : Migration
{
    private static readonly (string Table, string Schema)[] Tables =
    [
        ("AccountTransfers",           "public"),
        ("Brigades",                   "public"),
        ("CashAccounts",               "public"),
        ("CashTransactions",           "public"),
        ("Customers",                  "public"),
        ("Employees",                  "public"),
        ("Estimates",                  "public"),
        ("Incomes",                    "public"),
        ("IncomeItems",                "public"),
        ("IncomeOutcomes",             "public"),
        ("Manufacturers",              "public"),
        ("MeasurementUnits",           "public"),
        ("Organizations",              "public"),
        ("OrganizationMeasurementUnits", "public"),
        ("Outcomes",                   "public"),
        ("OutcomeItems",               "public"),
        ("Products",                   "public"),
        ("Projects",                   "public"),
        ("ProjectExpenses",            "public"),
        ("Salaries",                   "public"),
        ("Skus",                       "public"),
        ("Stocks",                     "public"),
        ("StockBalanceRegisters",      "public"),
        ("StockDemands",               "public"),
        ("StockDemandItems",           "public"),
        ("StockSkus",                  "public"),
        ("Suppliers",                  "public"),
        ("WorkTypes",                  "public"),
        ("Users",                      "Identity"),
    ];

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // CONCURRENTLY — jadvalni lock qilmaydi, production-da xavfsiz
        // IF NOT EXISTS — idempotent
        migrationBuilder.Sql("SET statement_timeout = '0';");

        foreach (var (table, schema) in Tables)
        {
            var indexName = $"IX_{table}_IsDeleted";
            var fullTable = schema == "public"
                ? $"\"{table}\""
                : $"\"{schema}\".\"{table}\"";

            migrationBuilder.Sql(
                $"CREATE INDEX CONCURRENTLY IF NOT EXISTS \"{indexName}\" " +
                $"ON {fullTable} (\"IsDeleted\");",
                suppressTransaction: true);   // CONCURRENTLY tranzaksiya tashqarisida ishlaydi
        }
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        foreach (var (table, _) in Tables)
        {
            var indexName = $"IX_{table}_IsDeleted";
            migrationBuilder.Sql(
                $"DROP INDEX CONCURRENTLY IF EXISTS \"{indexName}\";",
                suppressTransaction: true);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ucms.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLocalizationFromSku : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Skus");

            migrationBuilder.DropColumn(
                name: "NameEn",
                table: "Skus");

            migrationBuilder.DropColumn(
                name: "NameKa",
                table: "Skus");

            migrationBuilder.DropColumn(
                name: "NameRu",
                table: "Skus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Skus",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "Skus",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameKa",
                table: "Skus",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameRu",
                table: "Skus",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}

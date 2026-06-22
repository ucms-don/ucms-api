using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ucms.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDescriptionAndMaterialPriceToEstimateItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "EstimateItems");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "EstimateItems",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaterialUnitPrice",
                table: "EstimateItems",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkTypeId",
                table: "EstimateItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WorkTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    NameRu = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    NameKa = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EstimateItems_WorkTypeId",
                table: "EstimateItems",
                column: "WorkTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkTypes_Id",
                table: "WorkTypes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EstimateItems_WorkTypes_WorkTypeId",
                table: "EstimateItems",
                column: "WorkTypeId",
                principalTable: "WorkTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EstimateItems_WorkTypes_WorkTypeId",
                table: "EstimateItems");

            migrationBuilder.DropTable(
                name: "WorkTypes");

            migrationBuilder.DropIndex(
                name: "IX_EstimateItems_WorkTypeId",
                table: "EstimateItems");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "EstimateItems");

            migrationBuilder.DropColumn(
                name: "MaterialUnitPrice",
                table: "EstimateItems");

            migrationBuilder.DropColumn(
                name: "WorkTypeId",
                table: "EstimateItems");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "EstimateItems",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ucms.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEstimateBlocks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. EstimateSections.BlockId → EstimateId (Estimates ga to'g'ridan-to'g'ri)
            migrationBuilder.DropForeignKey(
                name: "FK_EstimateSections_EstimateBlocks_BlockId",
                table: "EstimateSections");

            migrationBuilder.DropIndex(
                name: "IX_EstimateSections_BlockId",
                table: "EstimateSections");

            // Data migration: BlockId qiymatlarini EstimateBlocks.EstimateId ga o'zgartirish
            migrationBuilder.Sql(
                """
                UPDATE "EstimateSections" s
                SET "BlockId" = b."EstimateId"
                FROM "EstimateBlocks" b
                WHERE s."BlockId" = b."Id";
                """);

            migrationBuilder.RenameColumn(
                name: "BlockId",
                table: "EstimateSections",
                newName: "EstimateId");

            migrationBuilder.CreateIndex(
                name: "IX_EstimateSections_EstimateId",
                table: "EstimateSections",
                column: "EstimateId");

            migrationBuilder.AddForeignKey(
                name: "FK_EstimateSections_Estimates_EstimateId",
                table: "EstimateSections",
                column: "EstimateId",
                principalTable: "Estimates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // 2. EstimateBlocks jadvalini o'chirish
            migrationBuilder.DropTable(name: "EstimateBlocks");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EstimateBlocks",
                columns: table => new
                {
                    Id         = table.Column<Guid>(type: "uuid", nullable: false),
                    EstimateId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name       = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Order      = table.Column<int>(type: "integer", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstimateBlocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EstimateBlocks_Estimates_EstimateId",
                        column: x => x.EstimateId,
                        principalTable: "Estimates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EstimateBlocks_EstimateId",
                table: "EstimateBlocks",
                column: "EstimateId");

            migrationBuilder.CreateIndex(
                name: "IX_EstimateBlocks_Id",
                table: "EstimateBlocks",
                column: "Id");

            migrationBuilder.DropForeignKey(
                name: "FK_EstimateSections_Estimates_EstimateId",
                table: "EstimateSections");

            migrationBuilder.DropIndex(
                name: "IX_EstimateSections_EstimateId",
                table: "EstimateSections");

            migrationBuilder.RenameColumn(
                name: "EstimateId",
                table: "EstimateSections",
                newName: "BlockId");

            migrationBuilder.CreateIndex(
                name: "IX_EstimateSections_BlockId",
                table: "EstimateSections",
                column: "BlockId");

            migrationBuilder.AddForeignKey(
                name: "FK_EstimateSections_EstimateBlocks_BlockId",
                table: "EstimateSections",
                column: "BlockId",
                principalTable: "EstimateBlocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

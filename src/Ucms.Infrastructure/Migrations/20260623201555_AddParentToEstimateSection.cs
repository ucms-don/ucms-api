using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ucms.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddParentToEstimateSection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ParentId",
                table: "EstimateSections",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EstimateSections_ParentId",
                table: "EstimateSections",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_EstimateSections_EstimateSections_ParentId",
                table: "EstimateSections",
                column: "ParentId",
                principalTable: "EstimateSections",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EstimateSections_EstimateSections_ParentId",
                table: "EstimateSections");

            migrationBuilder.DropIndex(
                name: "IX_EstimateSections_ParentId",
                table: "EstimateSections");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "EstimateSections");
        }
    }
}

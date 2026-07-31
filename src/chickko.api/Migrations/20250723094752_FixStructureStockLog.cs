using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chickko.api.Migrations
{
    /// <inheritdoc />
    public partial class FixStructureStockLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockLog_StockCategory_StockCategoryID",
                table: "StockLog");

            migrationBuilder.DropForeignKey(
                name: "FK_StockLog_StockLocation_StockLocationID",
                table: "StockLog");

            migrationBuilder.DropForeignKey(
                name: "FK_StockLog_StockUnitType_StockUnitTypeID",
                table: "StockLog");

            migrationBuilder.DropIndex(
                name: "IX_StockLog_StockCategoryID",
                table: "StockLog");

            migrationBuilder.DropIndex(
                name: "IX_StockLog_StockLocationID",
                table: "StockLog");

            migrationBuilder.DropIndex(
                name: "IX_StockLog_StockUnitTypeID",
                table: "StockLog");

            migrationBuilder.DropColumn(
                name: "StockCategoryID",
                table: "StockLog");

            migrationBuilder.DropColumn(
                name: "StockLocationID",
                table: "StockLog");

            migrationBuilder.DropColumn(
                name: "StockUnitTypeID",
                table: "StockLog");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StockCategoryID",
                table: "StockLog",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StockLocationID",
                table: "StockLog",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StockUnitTypeID",
                table: "StockLog",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_StockLog_StockCategoryID",
                table: "StockLog",
                column: "StockCategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_StockLog_StockLocationID",
                table: "StockLog",
                column: "StockLocationID");

            migrationBuilder.CreateIndex(
                name: "IX_StockLog_StockUnitTypeID",
                table: "StockLog",
                column: "StockUnitTypeID");

            migrationBuilder.AddForeignKey(
                name: "FK_StockLog_StockCategory_StockCategoryID",
                table: "StockLog",
                column: "StockCategoryID",
                principalTable: "StockCategory",
                principalColumn: "StockCategoryID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StockLog_StockLocation_StockLocationID",
                table: "StockLog",
                column: "StockLocationID",
                principalTable: "StockLocation",
                principalColumn: "StockLocationID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StockLog_StockUnitType_StockUnitTypeID",
                table: "StockLog",
                column: "StockUnitTypeID",
                principalTable: "StockUnitType",
                principalColumn: "StockUnitTypeID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

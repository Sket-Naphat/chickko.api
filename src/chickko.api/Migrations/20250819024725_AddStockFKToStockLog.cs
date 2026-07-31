using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chickko.api.Migrations
{
    /// <inheritdoc />
    public partial class AddStockFKToStockLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stock_StockLog_RecentStockLogId",
                table: "Stock");

            migrationBuilder.DropIndex(
                name: "IX_Stock_RecentStockLogId",
                table: "Stock");

            migrationBuilder.CreateIndex(
                name: "IX_StockLog_StockId",
                table: "StockLog",
                column: "StockId");

            migrationBuilder.AddForeignKey(
                name: "FK_StockLog_Stock_StockId",
                table: "StockLog",
                column: "StockId",
                principalTable: "Stock",
                principalColumn: "StockId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockLog_Stock_StockId",
                table: "StockLog");

            migrationBuilder.DropIndex(
                name: "IX_StockLog_StockId",
                table: "StockLog");

            migrationBuilder.CreateIndex(
                name: "IX_Stock_RecentStockLogId",
                table: "Stock",
                column: "RecentStockLogId");

            migrationBuilder.AddForeignKey(
                name: "FK_Stock_StockLog_RecentStockLogId",
                table: "Stock",
                column: "RecentStockLogId",
                principalTable: "StockLog",
                principalColumn: "StockLogId");
        }
    }
}

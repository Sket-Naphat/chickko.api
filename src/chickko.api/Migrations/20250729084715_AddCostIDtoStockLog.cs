using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chickko.api.Migrations
{
    /// <inheritdoc />
    public partial class AddCostIDtoStockLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CostId",
                table: "StockLog",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_StockLog_CostId",
                table: "StockLog",
                column: "CostId");

            migrationBuilder.AddForeignKey(
                name: "FK_StockLog_Cost_CostId",
                table: "StockLog",
                column: "CostId",
                principalTable: "Cost",
                principalColumn: "CostId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockLog_Cost_CostId",
                table: "StockLog");

            migrationBuilder.DropIndex(
                name: "IX_StockLog_CostId",
                table: "StockLog");

            migrationBuilder.DropColumn(
                name: "CostId",
                table: "StockLog");
        }
    }
}

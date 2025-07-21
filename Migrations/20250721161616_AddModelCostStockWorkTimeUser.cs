using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace chickko.api.Migrations
{
    /// <inheritdoc />
    public partial class AddModelCostStockWorkTimeUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockLogs_Stocks_StockId",
                table: "StockLogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Stocks",
                table: "Stocks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StockLogs",
                table: "StockLogs");

            migrationBuilder.RenameTable(
                name: "Stocks",
                newName: "Stock");

            migrationBuilder.RenameTable(
                name: "StockLogs",
                newName: "StockLog");

            migrationBuilder.RenameIndex(
                name: "IX_StockLogs_StockId",
                table: "StockLog",
                newName: "IX_StockLog_StockId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Stock",
                table: "Stock",
                column: "StockId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StockLog",
                table: "StockLog",
                column: "StockLogId");

            migrationBuilder.CreateTable(
                name: "Supplier",
                columns: table => new
                {
                    SupplyId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SupplyName = table.Column<string>(type: "text", nullable: false),
                    SupplyContact = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Supplier", x => x.SupplyId);
                });

            migrationBuilder.CreateTable(
                name: "WorkTime",
                columns: table => new
                {
                    WorkTimeID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkDate = table.Column<DateOnly>(type: "date", nullable: false),
                    TimeClockIn = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    TimeClockOut = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    ClockInLocation = table.Column<string>(type: "text", nullable: false),
                    TotalWirkTime = table.Column<int>(type: "integer", nullable: false),
                    wage = table.Column<int>(type: "integer", nullable: false),
                    bonus = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<int>(type: "integer", nullable: false),
                    IsPurchese = table.Column<bool>(type: "boolean", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    Remark = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkTime", x => x.WorkTimeID);
                });

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

            migrationBuilder.DropTable(
                name: "Supplier");

            migrationBuilder.DropTable(
                name: "WorkTime");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StockLog",
                table: "StockLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Stock",
                table: "Stock");

            migrationBuilder.RenameTable(
                name: "StockLog",
                newName: "StockLogs");

            migrationBuilder.RenameTable(
                name: "Stock",
                newName: "Stocks");

            migrationBuilder.RenameIndex(
                name: "IX_StockLog_StockId",
                table: "StockLogs",
                newName: "IX_StockLogs_StockId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StockLogs",
                table: "StockLogs",
                column: "StockLogId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Stocks",
                table: "Stocks",
                column: "StockId");

            migrationBuilder.AddForeignKey(
                name: "FK_StockLogs_Stocks_StockId",
                table: "StockLogs",
                column: "StockId",
                principalTable: "Stocks",
                principalColumn: "StockId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

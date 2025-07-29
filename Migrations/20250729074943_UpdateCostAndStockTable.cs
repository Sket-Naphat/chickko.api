using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chickko.api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCostAndStockTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockLog_Supplier_SupplierSupplyId",
                table: "StockLog");

            migrationBuilder.RenameColumn(
                name: "SupplyId",
                table: "Supplier",
                newName: "SupplyID");

            migrationBuilder.RenameColumn(
                name: "SupplyId",
                table: "StockLog",
                newName: "SupplyID");

            migrationBuilder.RenameColumn(
                name: "SupplierSupplyId",
                table: "StockLog",
                newName: "SupplierSupplyID");

            migrationBuilder.RenameIndex(
                name: "IX_StockLog_SupplierSupplyId",
                table: "StockLog",
                newName: "IX_StockLog_SupplierSupplyID");

            migrationBuilder.AddColumn<DateOnly>(
                name: "PurcheseDate",
                table: "Cost",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<TimeOnly>(
                name: "PurcheseTime",
                table: "Cost",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddForeignKey(
                name: "FK_StockLog_Supplier_SupplierSupplyID",
                table: "StockLog",
                column: "SupplierSupplyID",
                principalTable: "Supplier",
                principalColumn: "SupplyID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockLog_Supplier_SupplierSupplyID",
                table: "StockLog");

            migrationBuilder.DropColumn(
                name: "PurcheseDate",
                table: "Cost");

            migrationBuilder.DropColumn(
                name: "PurcheseTime",
                table: "Cost");

            migrationBuilder.RenameColumn(
                name: "SupplyID",
                table: "Supplier",
                newName: "SupplyId");

            migrationBuilder.RenameColumn(
                name: "SupplyID",
                table: "StockLog",
                newName: "SupplyId");

            migrationBuilder.RenameColumn(
                name: "SupplierSupplyID",
                table: "StockLog",
                newName: "SupplierSupplyId");

            migrationBuilder.RenameIndex(
                name: "IX_StockLog_SupplierSupplyID",
                table: "StockLog",
                newName: "IX_StockLog_SupplierSupplyId");

            migrationBuilder.AddForeignKey(
                name: "FK_StockLog_Supplier_SupplierSupplyId",
                table: "StockLog",
                column: "SupplierSupplyId",
                principalTable: "Supplier",
                principalColumn: "SupplyId");
        }
    }
}

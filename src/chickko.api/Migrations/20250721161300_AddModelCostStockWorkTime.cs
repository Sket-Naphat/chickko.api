using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chickko.api.Migrations
{
    /// <inheritdoc />
    public partial class AddModelCostStockWorkTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RequiredQuantity",
                table: "Stocks",
                newName: "TotalQTY");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Stocks",
                newName: "StockId");

            migrationBuilder.RenameColumn(
                name: "StockInQuantity",
                table: "StockLogs",
                newName: "TotalQTY");

            migrationBuilder.RenameColumn(
                name: "RemainingQuantity",
                table: "StockLogs",
                newName: "SupplyId");

            migrationBuilder.RenameColumn(
                name: "QuantityToPurchase",
                table: "StockLogs",
                newName: "StockInQTY");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "StockLogs",
                newName: "StockLogId");

            migrationBuilder.AddColumn<string>(
                name: "Contact",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "RequiredQTY",
                table: "Stocks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StockInQTY",
                table: "Stocks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateOnly>(
                name: "UpdateDate",
                table: "Stocks",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<TimeOnly>(
                name: "UpdateTime",
                table: "Stocks",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<int>(
                name: "DipQTY",
                table: "StockLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsPurchase",
                table: "StockLogs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Price",
                table: "StockLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PurcheseQTY",
                table: "StockLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Remark",
                table: "StockLogs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "RequiredQTY",
                table: "StockLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "StockInTime",
                table: "StockLogs",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "CostTime",
                table: "Cost",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0),
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "CostDate",
                table: "Cost",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1),
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "UpdateDate",
                table: "Cost",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<TimeOnly>(
                name: "UpdateTime",
                table: "Cost",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<bool>(
                name: "isFinish",
                table: "Cost",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Contact",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RequiredQTY",
                table: "Stocks");

            migrationBuilder.DropColumn(
                name: "StockInQTY",
                table: "Stocks");

            migrationBuilder.DropColumn(
                name: "UpdateDate",
                table: "Stocks");

            migrationBuilder.DropColumn(
                name: "UpdateTime",
                table: "Stocks");

            migrationBuilder.DropColumn(
                name: "DipQTY",
                table: "StockLogs");

            migrationBuilder.DropColumn(
                name: "IsPurchase",
                table: "StockLogs");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "StockLogs");

            migrationBuilder.DropColumn(
                name: "PurcheseQTY",
                table: "StockLogs");

            migrationBuilder.DropColumn(
                name: "Remark",
                table: "StockLogs");

            migrationBuilder.DropColumn(
                name: "RequiredQTY",
                table: "StockLogs");

            migrationBuilder.DropColumn(
                name: "StockInTime",
                table: "StockLogs");

            migrationBuilder.DropColumn(
                name: "UpdateDate",
                table: "Cost");

            migrationBuilder.DropColumn(
                name: "UpdateTime",
                table: "Cost");

            migrationBuilder.DropColumn(
                name: "isFinish",
                table: "Cost");

            migrationBuilder.RenameColumn(
                name: "TotalQTY",
                table: "Stocks",
                newName: "RequiredQuantity");

            migrationBuilder.RenameColumn(
                name: "StockId",
                table: "Stocks",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "TotalQTY",
                table: "StockLogs",
                newName: "StockInQuantity");

            migrationBuilder.RenameColumn(
                name: "SupplyId",
                table: "StockLogs",
                newName: "RemainingQuantity");

            migrationBuilder.RenameColumn(
                name: "StockInQTY",
                table: "StockLogs",
                newName: "QuantityToPurchase");

            migrationBuilder.RenameColumn(
                name: "StockLogId",
                table: "StockLogs",
                newName: "Id");

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "CostTime",
                table: "Cost",
                type: "time without time zone",
                nullable: true,
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "CostDate",
                table: "Cost",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date");
        }
    }
}

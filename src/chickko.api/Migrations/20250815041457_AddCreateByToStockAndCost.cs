using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chickko.api.Migrations
{
    /// <inheritdoc />
    public partial class AddCreateByToStockAndCost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Users",
                newName: "UserId");

            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "StockLog",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CreateBy",
                table: "StockLog",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateOnly>(
                name: "CreateDate",
                table: "StockLog",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<TimeOnly>(
                name: "CreateTime",
                table: "StockLog",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<int>(
                name: "UpdateBy",
                table: "StockLog",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateOnly>(
                name: "UpdateDate",
                table: "StockLog",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<TimeOnly>(
                name: "UpdateTime",
                table: "StockLog",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<int>(
                name: "CreateBy",
                table: "Cost",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateOnly>(
                name: "CreateDate",
                table: "Cost",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<TimeOnly>(
                name: "CreateTime",
                table: "Cost",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Cost",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "UpdateBy",
                table: "Cost",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Active",
                table: "StockLog");

            migrationBuilder.DropColumn(
                name: "CreateBy",
                table: "StockLog");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "StockLog");

            migrationBuilder.DropColumn(
                name: "CreateTime",
                table: "StockLog");

            migrationBuilder.DropColumn(
                name: "UpdateBy",
                table: "StockLog");

            migrationBuilder.DropColumn(
                name: "UpdateDate",
                table: "StockLog");

            migrationBuilder.DropColumn(
                name: "UpdateTime",
                table: "StockLog");

            migrationBuilder.DropColumn(
                name: "CreateBy",
                table: "Cost");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "Cost");

            migrationBuilder.DropColumn(
                name: "CreateTime",
                table: "Cost");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Cost");

            migrationBuilder.DropColumn(
                name: "UpdateBy",
                table: "Cost");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Users",
                newName: "Id");
        }
    }
}

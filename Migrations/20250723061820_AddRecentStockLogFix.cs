using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace chickko.api.Migrations
{
    /// <inheritdoc />
    public partial class AddRecentStockLogFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockLog_Stock_StockId",
                table: "StockLog");

            migrationBuilder.DropIndex(
                name: "IX_StockLog_StockId",
                table: "StockLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ErrorLog",
                table: "ErrorLog");

            migrationBuilder.DropColumn(
                name: "ErrorDate",
                table: "ErrorLog");

            migrationBuilder.DropColumn(
                name: "ErrorTime",
                table: "ErrorLog");

            migrationBuilder.RenameTable(
                name: "ErrorLog",
                newName: "ErrorLogs");

            migrationBuilder.RenameColumn(
                name: "ErrorMassage",
                table: "ErrorLogs",
                newName: "StackTrace");

            migrationBuilder.RenameColumn(
                name: "ErrorFile",
                table: "ErrorLogs",
                newName: "Source");

            migrationBuilder.RenameColumn(
                name: "LogId",
                table: "ErrorLogs",
                newName: "Id");

            migrationBuilder.AddColumn<int>(
                name: "StockLogTypeID",
                table: "StockLog",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "Stock",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RecentStockLogId",
                table: "Stock",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ErrorLogs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "ErrorLogs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Method",
                table: "ErrorLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Path",
                table: "ErrorLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ErrorLogs",
                table: "ErrorLogs",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "StockLogType",
                columns: table => new
                {
                    StockLogTypeID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StockLogTypeName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockLogType", x => x.StockLogTypeID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StockLog_StockLogTypeID",
                table: "StockLog",
                column: "StockLogTypeID");

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

            migrationBuilder.AddForeignKey(
                name: "FK_StockLog_StockLogType_StockLogTypeID",
                table: "StockLog",
                column: "StockLogTypeID",
                principalTable: "StockLogType",
                principalColumn: "StockLogTypeID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stock_StockLog_RecentStockLogId",
                table: "Stock");

            migrationBuilder.DropForeignKey(
                name: "FK_StockLog_StockLogType_StockLogTypeID",
                table: "StockLog");

            migrationBuilder.DropTable(
                name: "StockLogType");

            migrationBuilder.DropIndex(
                name: "IX_StockLog_StockLogTypeID",
                table: "StockLog");

            migrationBuilder.DropIndex(
                name: "IX_Stock_RecentStockLogId",
                table: "Stock");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ErrorLogs",
                table: "ErrorLogs");

            migrationBuilder.DropColumn(
                name: "StockLogTypeID",
                table: "StockLog");

            migrationBuilder.DropColumn(
                name: "Active",
                table: "Stock");

            migrationBuilder.DropColumn(
                name: "RecentStockLogId",
                table: "Stock");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ErrorLogs");

            migrationBuilder.DropColumn(
                name: "Message",
                table: "ErrorLogs");

            migrationBuilder.DropColumn(
                name: "Method",
                table: "ErrorLogs");

            migrationBuilder.DropColumn(
                name: "Path",
                table: "ErrorLogs");

            migrationBuilder.RenameTable(
                name: "ErrorLogs",
                newName: "ErrorLog");

            migrationBuilder.RenameColumn(
                name: "StackTrace",
                table: "ErrorLog",
                newName: "ErrorMassage");

            migrationBuilder.RenameColumn(
                name: "Source",
                table: "ErrorLog",
                newName: "ErrorFile");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ErrorLog",
                newName: "LogId");

            migrationBuilder.AddColumn<DateOnly>(
                name: "ErrorDate",
                table: "ErrorLog",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "ErrorTime",
                table: "ErrorLog",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ErrorLog",
                table: "ErrorLog",
                column: "LogId");

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
    }
}

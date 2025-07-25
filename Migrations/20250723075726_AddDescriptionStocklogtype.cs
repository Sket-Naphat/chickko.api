﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chickko.api.Migrations
{
    /// <inheritdoc />
    public partial class AddDescriptionStocklogtype : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "StockLogType",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "StockLogType");
        }
    }
}

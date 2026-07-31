using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chickko.api.Migrations
{
    /// <inheritdoc />
    public partial class renamePurchase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
            name: "IsPurchese",
            schema: "public",
            table: "Cost",
            newName: "IsPurchase");
            migrationBuilder.RenameColumn(
            name: "PurcheseQTY",
            schema: "public",
            table: "StockLog",
            newName: "PurchaseQTY");
            migrationBuilder.RenameColumn(
            name: "PurcheseDate",
            schema: "public",
            table: "Cost",
            newName: "PurchaseDate");
            migrationBuilder.RenameColumn(
            name: "PurcheseTime",
            schema: "public",
            table: "Cost",
            newName: "PurchaseTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}

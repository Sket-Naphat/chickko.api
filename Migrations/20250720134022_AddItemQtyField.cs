using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chickko.api.Migrations
{
    /// <inheritdoc />
    public partial class AddItemQtyField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ItemQTY",
                table: "OrderHeaders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ToppingQTY",
                table: "OrderDetails",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ItemQTY",
                table: "OrderHeaders");

            migrationBuilder.DropColumn(
                name: "ToppingQTY",
                table: "OrderDetails");
        }
    }
}

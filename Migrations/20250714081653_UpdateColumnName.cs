using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chickko.api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateColumnName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IdInFirestore",
                table: "Menus",
                newName: "MenuIdInFirestore");

            migrationBuilder.AddColumn<string>(
                name: "MenuIdInFirestore",
                table: "OrderDetails",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MenuIdInFirestore",
                table: "OrderDetails");

            migrationBuilder.RenameColumn(
                name: "MenuIdInFirestore",
                table: "Menus",
                newName: "IdInFirestore");
        }
    }
}

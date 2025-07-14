using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chickko.api.Migrations
{
    /// <inheritdoc />
    public partial class addStockAndMenuCodeFromFirebase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdInFirestore",
                table: "Menus",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdInFirestore",
                table: "Menus");
        }
    }
}

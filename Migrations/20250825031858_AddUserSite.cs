using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chickko.api.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Site",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Site",
                table: "Users");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chickko.api.Migrations
{
    /// <inheritdoc />
    public partial class AddCostIDToWageTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CostID",
                table: "Worktime",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Worktime_CostID",
                table: "Worktime",
                column: "CostID");

            migrationBuilder.AddForeignKey(
                name: "FK_Worktime_Cost_CostID",
                table: "Worktime",
                column: "CostID",
                principalTable: "Cost",
                principalColumn: "CostId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Worktime_Cost_CostID",
                table: "Worktime");

            migrationBuilder.DropIndex(
                name: "IX_Worktime_CostID",
                table: "Worktime");

            migrationBuilder.DropColumn(
                name: "CostID",
                table: "Worktime");
        }
    }
}

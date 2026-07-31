using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chickko.api.Migrations
{
    /// <inheritdoc />
    public partial class ChangeNameCost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cost_CostCategory_CostCategoryID",
                table: "cost");

            migrationBuilder.DropPrimaryKey(
                name: "PK_cost",
                table: "cost");

            migrationBuilder.RenameTable(
                name: "cost",
                newName: "Cost");

            migrationBuilder.RenameIndex(
                name: "IX_cost_CostCategoryID",
                table: "Cost",
                newName: "IX_Cost_CostCategoryID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Cost",
                table: "Cost",
                column: "CostId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cost_CostCategory_CostCategoryID",
                table: "Cost",
                column: "CostCategoryID",
                principalTable: "CostCategory",
                principalColumn: "CostCategoryID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cost_CostCategory_CostCategoryID",
                table: "Cost");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Cost",
                table: "Cost");

            migrationBuilder.RenameTable(
                name: "Cost",
                newName: "cost");

            migrationBuilder.RenameIndex(
                name: "IX_Cost_CostCategoryID",
                table: "cost",
                newName: "IX_cost_CostCategoryID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_cost",
                table: "cost",
                column: "CostId");

            migrationBuilder.AddForeignKey(
                name: "FK_cost_CostCategory_CostCategoryID",
                table: "cost",
                column: "CostCategoryID",
                principalTable: "CostCategory",
                principalColumn: "CostCategoryID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

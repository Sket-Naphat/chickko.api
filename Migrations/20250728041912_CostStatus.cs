using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace chickko.api.Migrations
{
    /// <inheritdoc />
    public partial class CostStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "isFinish",
                table: "Cost",
                newName: "IsPurchese");

            migrationBuilder.AddColumn<int>(
                name: "CostStatusID",
                table: "Cost",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CostStatus",
                columns: table => new
                {
                    CostStatusID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CostStatusName = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CostStatus", x => x.CostStatusID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cost_CostStatusID",
                table: "Cost",
                column: "CostStatusID");

            migrationBuilder.AddForeignKey(
                name: "FK_Cost_CostStatus_CostStatusID",
                table: "Cost",
                column: "CostStatusID",
                principalTable: "CostStatus",
                principalColumn: "CostStatusID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cost_CostStatus_CostStatusID",
                table: "Cost");

            migrationBuilder.DropTable(
                name: "CostStatus");

            migrationBuilder.DropIndex(
                name: "IX_Cost_CostStatusID",
                table: "Cost");

            migrationBuilder.DropColumn(
                name: "CostStatusID",
                table: "Cost");

            migrationBuilder.RenameColumn(
                name: "IsPurchese",
                table: "Cost",
                newName: "isFinish");
        }
    }
}

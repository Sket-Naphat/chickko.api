using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace chickko.api.Migrations
{
    /// <inheritdoc />
    public partial class AddCostPurchaseType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CostPurchaseTypeID",
                table: "Cost",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CostPurchaseType",
                columns: table => new
                {
                    CostPurchaseTypeID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CostPurchaseTypeName = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CostPurchaseType", x => x.CostPurchaseTypeID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cost_CostPurchaseTypeID",
                table: "Cost",
                column: "CostPurchaseTypeID");

            migrationBuilder.AddForeignKey(
                name: "FK_Cost_CostPurchaseType_CostPurchaseTypeID",
                table: "Cost",
                column: "CostPurchaseTypeID",
                principalTable: "CostPurchaseType",
                principalColumn: "CostPurchaseTypeID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cost_CostPurchaseType_CostPurchaseTypeID",
                table: "Cost");

            migrationBuilder.DropTable(
                name: "CostPurchaseType");

            migrationBuilder.DropIndex(
                name: "IX_Cost_CostPurchaseTypeID",
                table: "Cost");

            migrationBuilder.DropColumn(
                name: "CostPurchaseTypeID",
                table: "Cost");
        }
    }
}

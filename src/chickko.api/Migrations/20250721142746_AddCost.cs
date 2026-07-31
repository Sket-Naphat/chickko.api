using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace chickko.api.Migrations
{
    /// <inheritdoc />
    public partial class AddCost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CostCategory",
                columns: table => new
                {
                    CostCategoryID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CostCategoryName = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CostCategory", x => x.CostCategoryID);
                });

            migrationBuilder.CreateTable(
                name: "cost",
                columns: table => new
                {
                    CostId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CostCategoryID = table.Column<int>(type: "integer", nullable: false),
                    CostPrice = table.Column<int>(type: "integer", nullable: false),
                    CostDescription = table.Column<string>(type: "text", nullable: false),
                    CostDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CostTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cost", x => x.CostId);
                    table.ForeignKey(
                        name: "FK_cost_CostCategory_CostCategoryID",
                        column: x => x.CostCategoryID,
                        principalTable: "CostCategory",
                        principalColumn: "CostCategoryID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cost_CostCategoryID",
                table: "cost",
                column: "CostCategoryID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cost");

            migrationBuilder.DropTable(
                name: "CostCategory");
        }
    }
}

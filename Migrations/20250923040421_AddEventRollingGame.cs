using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace chickko.api.Migrations
{
    /// <inheritdoc />
    public partial class AddEventRollingGame : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RollingRewards",
                columns: table => new
                {
                    RollingRewardId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RewardName = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Probability = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RollingRewards", x => x.RollingRewardId);
                });

            migrationBuilder.CreateTable(
                name: "RollingResults",
                columns: table => new
                {
                    RollingResultId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderFirstStoreID = table.Column<string>(type: "text", nullable: true),
                    RewardID = table.Column<int>(type: "integer", nullable: false),
                    CostPrice = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RollingResults", x => x.RollingResultId);
                    table.ForeignKey(
                        name: "FK_RollingResults_RollingRewards_RewardID",
                        column: x => x.RewardID,
                        principalTable: "RollingRewards",
                        principalColumn: "RollingRewardId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RollingResults_RewardID",
                table: "RollingResults",
                column: "RewardID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RollingResults");

            migrationBuilder.DropTable(
                name: "RollingRewards");
        }
    }
}

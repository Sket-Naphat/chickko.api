using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chickko.api.Migrations
{
    /// <inheritdoc />
    public partial class RenameEventRollingGame : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RollingResults_RollingRewards_RewardID",
                table: "RollingResults");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RollingRewards",
                table: "RollingRewards");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RollingResults",
                table: "RollingResults");

            migrationBuilder.RenameTable(
                name: "RollingRewards",
                newName: "EventRollingRewards");

            migrationBuilder.RenameTable(
                name: "RollingResults",
                newName: "EventRollingResults");

            migrationBuilder.RenameIndex(
                name: "IX_RollingResults_RewardID",
                table: "EventRollingResults",
                newName: "IX_EventRollingResults_RewardID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventRollingRewards",
                table: "EventRollingRewards",
                column: "RollingRewardId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventRollingResults",
                table: "EventRollingResults",
                column: "RollingResultId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventRollingResults_EventRollingRewards_RewardID",
                table: "EventRollingResults",
                column: "RewardID",
                principalTable: "EventRollingRewards",
                principalColumn: "RollingRewardId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventRollingResults_EventRollingRewards_RewardID",
                table: "EventRollingResults");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EventRollingRewards",
                table: "EventRollingRewards");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EventRollingResults",
                table: "EventRollingResults");

            migrationBuilder.RenameTable(
                name: "EventRollingRewards",
                newName: "RollingRewards");

            migrationBuilder.RenameTable(
                name: "EventRollingResults",
                newName: "RollingResults");

            migrationBuilder.RenameIndex(
                name: "IX_EventRollingResults_RewardID",
                table: "RollingResults",
                newName: "IX_RollingResults_RewardID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RollingRewards",
                table: "RollingRewards",
                column: "RollingRewardId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RollingResults",
                table: "RollingResults",
                column: "RollingResultId");

            migrationBuilder.AddForeignKey(
                name: "FK_RollingResults_RollingRewards_RewardID",
                table: "RollingResults",
                column: "RewardID",
                principalTable: "RollingRewards",
                principalColumn: "RollingRewardId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

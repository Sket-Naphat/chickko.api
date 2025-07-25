using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chickko.api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateResent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkTime_Users_EmployeeID",
                table: "WorkTime");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkTime",
                table: "WorkTime");

            migrationBuilder.RenameTable(
                name: "WorkTime",
                newName: "Worktime");

            migrationBuilder.RenameColumn(
                name: "TotalWorkTime",
                table: "Worktime",
                newName: "TotalWorktime");

            migrationBuilder.RenameColumn(
                name: "WorkTimeID",
                table: "Worktime",
                newName: "WorktimeID");

            migrationBuilder.RenameIndex(
                name: "IX_WorkTime_EmployeeID",
                table: "Worktime",
                newName: "IX_Worktime_EmployeeID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Worktime",
                table: "Worktime",
                column: "WorktimeID");

            migrationBuilder.AddForeignKey(
                name: "FK_Worktime_Users_EmployeeID",
                table: "Worktime",
                column: "EmployeeID",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Worktime_Users_EmployeeID",
                table: "Worktime");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Worktime",
                table: "Worktime");

            migrationBuilder.RenameTable(
                name: "Worktime",
                newName: "WorkTime");

            migrationBuilder.RenameColumn(
                name: "TotalWorktime",
                table: "WorkTime",
                newName: "TotalWorkTime");

            migrationBuilder.RenameColumn(
                name: "WorktimeID",
                table: "WorkTime",
                newName: "WorkTimeID");

            migrationBuilder.RenameIndex(
                name: "IX_Worktime_EmployeeID",
                table: "WorkTime",
                newName: "IX_WorkTime_EmployeeID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkTime",
                table: "WorkTime",
                column: "WorkTimeID");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkTime_Users_EmployeeID",
                table: "WorkTime",
                column: "EmployeeID",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

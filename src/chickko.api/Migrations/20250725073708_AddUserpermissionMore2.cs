using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace chickko.api.Migrations
{
    /// <inheritdoc />
    public partial class AddUserpermissionMore2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "bonus",
                table: "WorkTime",
                newName: "Bonus");

            migrationBuilder.RenameColumn(
                name: "wage",
                table: "WorkTime",
                newName: "EmployeeID");

            migrationBuilder.AlterColumn<double>(
                name: "Bonus",
                table: "WorkTime",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<double>(
                name: "TotalWorkTime",
                table: "WorkTime",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "TimeClockOut",
                table: "WorkTime",
                type: "time without time zone",
                nullable: true,
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone");

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "TimeClockIn",
                table: "WorkTime",
                type: "time without time zone",
                nullable: true,
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone");

            migrationBuilder.AlterColumn<double>(
                name: "Price",
                table: "WorkTime",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<DateOnly>(
                name: "UpdateDate",
                table: "WorkTime",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<TimeOnly>(
                name: "UpdateTime",
                table: "WorkTime",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<double>(
                name: "WageCost",
                table: "WorkTime",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "UserPermistionID",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "UserPermistion",
                columns: table => new
                {
                    UserPermistionID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserPermistionName = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    WageCost = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPermistion", x => x.UserPermistionID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkTime_EmployeeID",
                table: "WorkTime",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserPermistionID",
                table: "Users",
                column: "UserPermistionID");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_UserPermistion_UserPermistionID",
                table: "Users",
                column: "UserPermistionID",
                principalTable: "UserPermistion",
                principalColumn: "UserPermistionID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkTime_Users_EmployeeID",
                table: "WorkTime",
                column: "EmployeeID",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_UserPermistion_UserPermistionID",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkTime_Users_EmployeeID",
                table: "WorkTime");

            migrationBuilder.DropTable(
                name: "UserPermistion");

            migrationBuilder.DropIndex(
                name: "IX_WorkTime_EmployeeID",
                table: "WorkTime");

            migrationBuilder.DropIndex(
                name: "IX_Users_UserPermistionID",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UpdateDate",
                table: "WorkTime");

            migrationBuilder.DropColumn(
                name: "UpdateTime",
                table: "WorkTime");

            migrationBuilder.DropColumn(
                name: "WageCost",
                table: "WorkTime");

            migrationBuilder.DropColumn(
                name: "UserPermistionID",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "Bonus",
                table: "WorkTime",
                newName: "bonus");

            migrationBuilder.RenameColumn(
                name: "EmployeeID",
                table: "WorkTime",
                newName: "wage");

            migrationBuilder.AlterColumn<int>(
                name: "TotalWorkTime",
                table: "WorkTime",
                type: "integer",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "TimeClockOut",
                table: "WorkTime",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0),
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "TimeClockIn",
                table: "WorkTime",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0),
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Price",
                table: "WorkTime",
                type: "integer",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<int>(
                name: "bonus",
                table: "WorkTime",
                type: "integer",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");
        }
    }
}

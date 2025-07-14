using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace chickko.api.Migrations
{
    /// <inheritdoc />
    public partial class Tableandupdatetable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderHeaders_LocationOrders_LocationOrderId",
                table: "OrderHeaders");

            migrationBuilder.DropTable(
                name: "LocationOrders");

            migrationBuilder.DropIndex(
                name: "IX_OrderHeaders_LocationOrderId",
                table: "OrderHeaders");

            migrationBuilder.DropColumn(
                name: "LocationOrderId",
                table: "OrderHeaders");

            migrationBuilder.AddColumn<string>(
                name: "IdInFirestore",
                table: "OrderHeaders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TableID",
                table: "OrderHeaders",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Tables",
                columns: table => new
                {
                    TableID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TableName = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tables", x => x.TableID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderHeaders_TableID",
                table: "OrderHeaders",
                column: "TableID");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderHeaders_Tables_TableID",
                table: "OrderHeaders",
                column: "TableID",
                principalTable: "Tables",
                principalColumn: "TableID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderHeaders_Tables_TableID",
                table: "OrderHeaders");

            migrationBuilder.DropTable(
                name: "Tables");

            migrationBuilder.DropIndex(
                name: "IX_OrderHeaders_TableID",
                table: "OrderHeaders");

            migrationBuilder.DropColumn(
                name: "IdInFirestore",
                table: "OrderHeaders");

            migrationBuilder.DropColumn(
                name: "TableID",
                table: "OrderHeaders");

            migrationBuilder.AddColumn<int>(
                name: "LocationOrderId",
                table: "OrderHeaders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "LocationOrders",
                columns: table => new
                {
                    LocationOrderId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Description = table.Column<string>(type: "text", nullable: true),
                    LocationName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationOrders", x => x.LocationOrderId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderHeaders_LocationOrderId",
                table: "OrderHeaders",
                column: "LocationOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderHeaders_LocationOrders_LocationOrderId",
                table: "OrderHeaders",
                column: "LocationOrderId",
                principalTable: "LocationOrders",
                principalColumn: "LocationOrderId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

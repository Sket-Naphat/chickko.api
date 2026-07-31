using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace chickko.api.Migrations
{
    /// <inheritdoc />
    public partial class AddFKStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StockCategoryID",
                table: "StockLog",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StockLocationID",
                table: "StockLog",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StockUnitTypeID",
                table: "StockLog",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SupplierSupplyId",
                table: "StockLog",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StockCategoryID",
                table: "Stock",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StockLocationID",
                table: "Stock",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StockUnitTypeID",
                table: "Stock",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "StockCategory",
                columns: table => new
                {
                    StockCategoryID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StockCategoryName = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockCategory", x => x.StockCategoryID);
                });

            migrationBuilder.CreateTable(
                name: "StockLocation",
                columns: table => new
                {
                    StockLocationID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StockLocationName = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockLocation", x => x.StockLocationID);
                });

            migrationBuilder.CreateTable(
                name: "StockUnitType",
                columns: table => new
                {
                    StockUnitTypeID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StockUnitTypeName = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockUnitType", x => x.StockUnitTypeID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StockLog_StockCategoryID",
                table: "StockLog",
                column: "StockCategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_StockLog_StockLocationID",
                table: "StockLog",
                column: "StockLocationID");

            migrationBuilder.CreateIndex(
                name: "IX_StockLog_StockUnitTypeID",
                table: "StockLog",
                column: "StockUnitTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_StockLog_SupplierSupplyId",
                table: "StockLog",
                column: "SupplierSupplyId");

            migrationBuilder.CreateIndex(
                name: "IX_Stock_StockCategoryID",
                table: "Stock",
                column: "StockCategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_Stock_StockLocationID",
                table: "Stock",
                column: "StockLocationID");

            migrationBuilder.CreateIndex(
                name: "IX_Stock_StockUnitTypeID",
                table: "Stock",
                column: "StockUnitTypeID");

            migrationBuilder.AddForeignKey(
                name: "FK_Stock_StockCategory_StockCategoryID",
                table: "Stock",
                column: "StockCategoryID",
                principalTable: "StockCategory",
                principalColumn: "StockCategoryID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Stock_StockLocation_StockLocationID",
                table: "Stock",
                column: "StockLocationID",
                principalTable: "StockLocation",
                principalColumn: "StockLocationID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Stock_StockUnitType_StockUnitTypeID",
                table: "Stock",
                column: "StockUnitTypeID",
                principalTable: "StockUnitType",
                principalColumn: "StockUnitTypeID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StockLog_StockCategory_StockCategoryID",
                table: "StockLog",
                column: "StockCategoryID",
                principalTable: "StockCategory",
                principalColumn: "StockCategoryID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StockLog_StockLocation_StockLocationID",
                table: "StockLog",
                column: "StockLocationID",
                principalTable: "StockLocation",
                principalColumn: "StockLocationID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StockLog_StockUnitType_StockUnitTypeID",
                table: "StockLog",
                column: "StockUnitTypeID",
                principalTable: "StockUnitType",
                principalColumn: "StockUnitTypeID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StockLog_Supplier_SupplierSupplyId",
                table: "StockLog",
                column: "SupplierSupplyId",
                principalTable: "Supplier",
                principalColumn: "SupplyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stock_StockCategory_StockCategoryID",
                table: "Stock");

            migrationBuilder.DropForeignKey(
                name: "FK_Stock_StockLocation_StockLocationID",
                table: "Stock");

            migrationBuilder.DropForeignKey(
                name: "FK_Stock_StockUnitType_StockUnitTypeID",
                table: "Stock");

            migrationBuilder.DropForeignKey(
                name: "FK_StockLog_StockCategory_StockCategoryID",
                table: "StockLog");

            migrationBuilder.DropForeignKey(
                name: "FK_StockLog_StockLocation_StockLocationID",
                table: "StockLog");

            migrationBuilder.DropForeignKey(
                name: "FK_StockLog_StockUnitType_StockUnitTypeID",
                table: "StockLog");

            migrationBuilder.DropForeignKey(
                name: "FK_StockLog_Supplier_SupplierSupplyId",
                table: "StockLog");

            migrationBuilder.DropTable(
                name: "StockCategory");

            migrationBuilder.DropTable(
                name: "StockLocation");

            migrationBuilder.DropTable(
                name: "StockUnitType");

            migrationBuilder.DropIndex(
                name: "IX_StockLog_StockCategoryID",
                table: "StockLog");

            migrationBuilder.DropIndex(
                name: "IX_StockLog_StockLocationID",
                table: "StockLog");

            migrationBuilder.DropIndex(
                name: "IX_StockLog_StockUnitTypeID",
                table: "StockLog");

            migrationBuilder.DropIndex(
                name: "IX_StockLog_SupplierSupplyId",
                table: "StockLog");

            migrationBuilder.DropIndex(
                name: "IX_Stock_StockCategoryID",
                table: "Stock");

            migrationBuilder.DropIndex(
                name: "IX_Stock_StockLocationID",
                table: "Stock");

            migrationBuilder.DropIndex(
                name: "IX_Stock_StockUnitTypeID",
                table: "Stock");

            migrationBuilder.DropColumn(
                name: "StockCategoryID",
                table: "StockLog");

            migrationBuilder.DropColumn(
                name: "StockLocationID",
                table: "StockLog");

            migrationBuilder.DropColumn(
                name: "StockUnitTypeID",
                table: "StockLog");

            migrationBuilder.DropColumn(
                name: "SupplierSupplyId",
                table: "StockLog");

            migrationBuilder.DropColumn(
                name: "StockCategoryID",
                table: "Stock");

            migrationBuilder.DropColumn(
                name: "StockLocationID",
                table: "Stock");

            migrationBuilder.DropColumn(
                name: "StockUnitTypeID",
                table: "Stock");
        }
    }
}

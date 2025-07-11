using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace chickko.api.Migrations
{
    /// <inheritdoc />
    public partial class OrdersModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrderDetailId",
                table: "Menus",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DischargeTypes",
                columns: table => new
                {
                    DischargeTypeId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DischargeName = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DischargeTypes", x => x.DischargeTypeId);
                });

            migrationBuilder.CreateTable(
                name: "Discounts",
                columns: table => new
                {
                    DiscountID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DiscountName = table.Column<string>(type: "text", nullable: false),
                    DiscountPercentage = table.Column<decimal>(type: "numeric", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Discounts", x => x.DiscountID);
                });

            migrationBuilder.CreateTable(
                name: "LocationOrders",
                columns: table => new
                {
                    LocationOrderId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LocationName = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationOrders", x => x.LocationOrderId);
                });

            migrationBuilder.CreateTable(
                name: "Ordertypes",
                columns: table => new
                {
                    OrderTypeId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderTypeName = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ordertypes", x => x.OrderTypeId);
                });

            migrationBuilder.CreateTable(
                name: "OrderHeaders",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LocationOrderId = table.Column<int>(type: "integer", nullable: false),
                    OrderDate = table.Column<DateOnly>(type: "date", nullable: false),
                    OrderTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    OrderTypeId = table.Column<int>(type: "integer", nullable: false),
                    DischargeTypeId = table.Column<int>(type: "integer", nullable: false),
                    DischargeTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    IsDischarge = table.Column<bool>(type: "boolean", nullable: false),
                    FinishOrderTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    IsFinishOrder = table.Column<bool>(type: "boolean", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    OrderRemark = table.Column<string>(type: "text", nullable: false),
                    DiscountID = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderHeaders", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_OrderHeaders_DischargeTypes_DischargeTypeId",
                        column: x => x.DischargeTypeId,
                        principalTable: "DischargeTypes",
                        principalColumn: "DischargeTypeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderHeaders_Discounts_DiscountID",
                        column: x => x.DiscountID,
                        principalTable: "Discounts",
                        principalColumn: "DiscountID");
                    table.ForeignKey(
                        name: "FK_OrderHeaders_LocationOrders_LocationOrderId",
                        column: x => x.LocationOrderId,
                        principalTable: "LocationOrders",
                        principalColumn: "LocationOrderId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderHeaders_Ordertypes_OrderTypeId",
                        column: x => x.OrderTypeId,
                        principalTable: "Ordertypes",
                        principalColumn: "OrderTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderDetails",
                columns: table => new
                {
                    OrderDetailId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    OrderHeaderOrderId = table.Column<int>(type: "integer", nullable: false),
                    MenuId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    IsDone = table.Column<bool>(type: "boolean", nullable: false),
                    IsDischarge = table.Column<bool>(type: "boolean", nullable: false),
                    Remark = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDetails", x => x.OrderDetailId);
                    table.ForeignKey(
                        name: "FK_OrderDetails_Menus_MenuId",
                        column: x => x.MenuId,
                        principalTable: "Menus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderDetails_OrderHeaders_OrderHeaderOrderId",
                        column: x => x.OrderHeaderOrderId,
                        principalTable: "OrderHeaders",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Menus_OrderDetailId",
                table: "Menus",
                column: "OrderDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_MenuId",
                table: "OrderDetails",
                column: "MenuId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_OrderHeaderOrderId",
                table: "OrderDetails",
                column: "OrderHeaderOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderHeaders_DischargeTypeId",
                table: "OrderHeaders",
                column: "DischargeTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderHeaders_DiscountID",
                table: "OrderHeaders",
                column: "DiscountID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderHeaders_LocationOrderId",
                table: "OrderHeaders",
                column: "LocationOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderHeaders_OrderTypeId",
                table: "OrderHeaders",
                column: "OrderTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Menus_OrderDetails_OrderDetailId",
                table: "Menus",
                column: "OrderDetailId",
                principalTable: "OrderDetails",
                principalColumn: "OrderDetailId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Menus_OrderDetails_OrderDetailId",
                table: "Menus");

            migrationBuilder.DropTable(
                name: "OrderDetails");

            migrationBuilder.DropTable(
                name: "OrderHeaders");

            migrationBuilder.DropTable(
                name: "DischargeTypes");

            migrationBuilder.DropTable(
                name: "Discounts");

            migrationBuilder.DropTable(
                name: "LocationOrders");

            migrationBuilder.DropTable(
                name: "Ordertypes");

            migrationBuilder.DropIndex(
                name: "IX_Menus_OrderDetailId",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "OrderDetailId",
                table: "Menus");
        }
    }
}

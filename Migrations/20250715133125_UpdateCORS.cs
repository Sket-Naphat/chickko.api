using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace chickko.api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCORS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Menus_OrderDetails_OrderDetailId",
                table: "Menus");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_Menus_MenuId",
                table: "OrderDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_OrderHeaders_OrderHeaderOrderId",
                table: "OrderDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderHeaders_DischargeTypes_DischargeTypeId",
                table: "OrderHeaders");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderHeaders_Discounts_DiscountID",
                table: "OrderHeaders");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderHeaders_Ordertypes_OrderTypeId",
                table: "OrderHeaders");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderHeaders_Tables_TableID",
                table: "OrderHeaders");

            migrationBuilder.DropIndex(
                name: "IX_OrderDetails_OrderHeaderOrderId",
                table: "OrderDetails");

            migrationBuilder.DropIndex(
                name: "IX_Menus_OrderDetailId",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "OrderHeaderOrderId",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "OrderDetailId",
                table: "Menus");

            migrationBuilder.CreateTable(
                name: "OrderDetailToppings",
                columns: table => new
                {
                    OrderDetailToppingId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderDetailId = table.Column<int>(type: "integer", nullable: false),
                    MenuId = table.Column<int>(type: "integer", nullable: false),
                    ToppingPrice = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDetailToppings", x => x.OrderDetailToppingId);
                    table.ForeignKey(
                        name: "FK_OrderDetailToppings_Menus_MenuId",
                        column: x => x.MenuId,
                        principalTable: "Menus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderDetailToppings_OrderDetails_OrderDetailId",
                        column: x => x.OrderDetailId,
                        principalTable: "OrderDetails",
                        principalColumn: "OrderDetailId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_OrderId",
                table: "OrderDetails",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetailToppings_MenuId",
                table: "OrderDetailToppings",
                column: "MenuId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetailToppings_OrderDetailId",
                table: "OrderDetailToppings",
                column: "OrderDetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_Menus_MenuId",
                table: "OrderDetails",
                column: "MenuId",
                principalTable: "Menus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_OrderHeaders_OrderId",
                table: "OrderDetails",
                column: "OrderId",
                principalTable: "OrderHeaders",
                principalColumn: "OrderId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderHeaders_DischargeTypes_DischargeTypeId",
                table: "OrderHeaders",
                column: "DischargeTypeId",
                principalTable: "DischargeTypes",
                principalColumn: "DischargeTypeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderHeaders_Discounts_DiscountID",
                table: "OrderHeaders",
                column: "DiscountID",
                principalTable: "Discounts",
                principalColumn: "DiscountID",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderHeaders_Ordertypes_OrderTypeId",
                table: "OrderHeaders",
                column: "OrderTypeId",
                principalTable: "Ordertypes",
                principalColumn: "OrderTypeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderHeaders_Tables_TableID",
                table: "OrderHeaders",
                column: "TableID",
                principalTable: "Tables",
                principalColumn: "TableID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_Menus_MenuId",
                table: "OrderDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_OrderHeaders_OrderId",
                table: "OrderDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderHeaders_DischargeTypes_DischargeTypeId",
                table: "OrderHeaders");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderHeaders_Discounts_DiscountID",
                table: "OrderHeaders");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderHeaders_Ordertypes_OrderTypeId",
                table: "OrderHeaders");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderHeaders_Tables_TableID",
                table: "OrderHeaders");

            migrationBuilder.DropTable(
                name: "OrderDetailToppings");

            migrationBuilder.DropIndex(
                name: "IX_OrderDetails_OrderId",
                table: "OrderDetails");

            migrationBuilder.AddColumn<int>(
                name: "OrderHeaderOrderId",
                table: "OrderDetails",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrderDetailId",
                table: "Menus",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_OrderHeaderOrderId",
                table: "OrderDetails",
                column: "OrderHeaderOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Menus_OrderDetailId",
                table: "Menus",
                column: "OrderDetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_Menus_OrderDetails_OrderDetailId",
                table: "Menus",
                column: "OrderDetailId",
                principalTable: "OrderDetails",
                principalColumn: "OrderDetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_Menus_MenuId",
                table: "OrderDetails",
                column: "MenuId",
                principalTable: "Menus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_OrderHeaders_OrderHeaderOrderId",
                table: "OrderDetails",
                column: "OrderHeaderOrderId",
                principalTable: "OrderHeaders",
                principalColumn: "OrderId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderHeaders_DischargeTypes_DischargeTypeId",
                table: "OrderHeaders",
                column: "DischargeTypeId",
                principalTable: "DischargeTypes",
                principalColumn: "DischargeTypeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderHeaders_Discounts_DiscountID",
                table: "OrderHeaders",
                column: "DiscountID",
                principalTable: "Discounts",
                principalColumn: "DiscountID");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderHeaders_Ordertypes_OrderTypeId",
                table: "OrderHeaders",
                column: "OrderTypeId",
                principalTable: "Ordertypes",
                principalColumn: "OrderTypeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderHeaders_Tables_TableID",
                table: "OrderHeaders",
                column: "TableID",
                principalTable: "Tables",
                principalColumn: "TableID");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Customo.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Allergens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Allergens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableNumber = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MenuItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuItems_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderAllergies",
                columns: table => new
                {
                    AllergiesId = table.Column<int>(type: "int", nullable: false),
                    OrdersId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderAllergies", x => new { x.AllergiesId, x.OrdersId });
                    table.ForeignKey(
                        name: "FK_OrderAllergies_Allergens_AllergiesId",
                        column: x => x.AllergiesId,
                        principalTable: "Allergens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderAllergies_Orders_OrdersId",
                        column: x => x.OrdersId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MenuItemAllergens",
                columns: table => new
                {
                    MenuItemId = table.Column<int>(type: "int", nullable: false),
                    AllergenId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItemAllergens", x => new { x.MenuItemId, x.AllergenId });
                    table.ForeignKey(
                        name: "FK_MenuItemAllergens_Allergens_AllergenId",
                        column: x => x.AllergenId,
                        principalTable: "Allergens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MenuItemAllergens_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MenuItemOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MenuItemId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PriceDelta = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItemOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuItemOptions_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    MenuItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    HasAllergyConflict = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderItemOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderItemId = table.Column<int>(type: "int", nullable: false),
                    OptionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PriceDelta = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItemOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItemOptions_OrderItems_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "OrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Allergens",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Peanuts" },
                    { 2, "Tree nuts" },
                    { 3, "Milk" },
                    { 4, "Egg" },
                    { 5, "Wheat" },
                    { 6, "Soy" },
                    { 7, "Fish" },
                    { 8, "Shrimp" },
                    { 9, "Shellfish" },
                    { 10, "Sesame" }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Mains" },
                    { 2, "Sides" },
                    { 3, "Desserts" },
                    { 4, "Drinks" }
                });

            migrationBuilder.InsertData(
                table: "MenuItems",
                columns: new[] { "Id", "CategoryId", "Name", "Price" },
                values: new object[,]
                {
                    { 1, 1, "Chicken Adobo", 150m },
                    { 2, 1, "Pancit Canton", 120m },
                    { 3, 1, "Kare-Kare", 250m },
                    { 4, 1, "Pork Sisig", 180m },
                    { 5, 1, "Sinigang na Baboy", 200m },
                    { 6, 1, "Crispy Pata", 450m },
                    { 7, 2, "Lumpiang Shanghai", 100m },
                    { 8, 2, "Plain Rice", 30m },
                    { 9, 3, "Halo-Halo", 110m },
                    { 10, 3, "Leche Flan", 80m },
                    { 11, 4, "Iced Tea", 60m },
                    { 12, 4, "Mango Shake", 90m }
                });

            migrationBuilder.InsertData(
                table: "MenuItemAllergens",
                columns: new[] { "AllergenId", "MenuItemId" },
                values: new object[,]
                {
                    { 6, 1 },
                    { 4, 2 },
                    { 5, 2 },
                    { 8, 2 },
                    { 1, 3 },
                    { 8, 3 },
                    { 4, 4 },
                    { 6, 4 },
                    { 7, 5 },
                    { 6, 6 },
                    { 4, 7 },
                    { 5, 7 },
                    { 1, 9 },
                    { 3, 9 },
                    { 3, 10 },
                    { 4, 10 },
                    { 3, 12 }
                });

            migrationBuilder.InsertData(
                table: "MenuItemOptions",
                columns: new[] { "Id", "MenuItemId", "Name", "PriceDelta", "Type" },
                values: new object[,]
                {
                    { 1, 1, "Extra rice", 20m, "Extra" },
                    { 2, 1, "Egg", 15m, "Extra" },
                    { 3, 1, "Steamed rice", 0m, "Swap" },
                    { 4, 1, "Garlic rice", 10m, "Swap" },
                    { 5, 2, "Extra veggies", 20m, "Extra" },
                    { 6, 3, "Extra bagoong", 10m, "Extra" },
                    { 7, 4, "Extra rice", 20m, "Extra" },
                    { 8, 4, "Egg", 15m, "Extra" },
                    { 9, 5, "Extra rice", 20m, "Extra" },
                    { 10, 6, "Extra sauce", 15m, "Extra" },
                    { 11, 7, "Sweet chili", 10m, "Extra" },
                    { 12, 9, "Ice cream", 30m, "Extra" },
                    { 13, 9, "Leche flan", 25m, "Extra" },
                    { 14, 11, "Regular", 0m, "Size" },
                    { 15, 11, "Medium", 10m, "Size" },
                    { 16, 11, "Large", 20m, "Size" },
                    { 17, 12, "Regular", 0m, "Size" },
                    { 18, 12, "Medium", 10m, "Size" },
                    { 19, 12, "Large", 20m, "Size" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemAllergens_AllergenId",
                table: "MenuItemAllergens",
                column: "AllergenId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemOptions_MenuItemId",
                table: "MenuItemOptions",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_CategoryId",
                table: "MenuItems",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderAllergies_OrdersId",
                table: "OrderAllergies",
                column: "OrdersId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemOptions_OrderItemId",
                table: "OrderItemOptions",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_MenuItemId",
                table: "OrderItems",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MenuItemAllergens");

            migrationBuilder.DropTable(
                name: "MenuItemOptions");

            migrationBuilder.DropTable(
                name: "OrderAllergies");

            migrationBuilder.DropTable(
                name: "OrderItemOptions");

            migrationBuilder.DropTable(
                name: "Allergens");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "MenuItems");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}

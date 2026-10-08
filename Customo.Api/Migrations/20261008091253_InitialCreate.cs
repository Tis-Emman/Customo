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
                    TotalAmount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tables",
                columns: table => new
                {
                    Number = table.Column<int>(type: "int", nullable: false),
                    ClaimToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OccupiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tables", x => x.Number);
                });

            migrationBuilder.CreateTable(
                name: "Ingredients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AllergenId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ingredients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ingredients_Allergens_AllergenId",
                        column: x => x.AllergenId,
                        principalTable: "Allergens",
                        principalColumn: "Id");
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
                name: "MenuItemIngredients",
                columns: table => new
                {
                    MenuItemId = table.Column<int>(type: "int", nullable: false),
                    IngredientId = table.Column<int>(type: "int", nullable: false),
                    IsRemovable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItemIngredients", x => new { x.MenuItemId, x.IngredientId });
                    table.ForeignKey(
                        name: "FK_MenuItemIngredients_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MenuItemIngredients_MenuItems_MenuItemId",
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
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                table: "Ingredients",
                columns: new[] { "Id", "AllergenId", "Name" },
                values: new object[,]
                {
                    { 1, null, "Chicken" },
                    { 3, null, "Vinegar" },
                    { 4, null, "Garlic" },
                    { 5, null, "Bay leaf" },
                    { 6, null, "Black pepper" },
                    { 10, null, "Cabbage" },
                    { 11, null, "Carrots" },
                    { 12, null, "Beef" },
                    { 15, null, "Eggplant" },
                    { 16, null, "String beans" },
                    { 17, null, "Bok choy" },
                    { 18, null, "Pork" },
                    { 19, null, "Onions" },
                    { 20, null, "Chili" },
                    { 21, null, "Calamansi" },
                    { 22, null, "Tamarind broth" },
                    { 24, null, "Radish" },
                    { 25, null, "Kangkong" },
                    { 26, null, "Tomato" },
                    { 28, null, "Rice" },
                    { 29, null, "Shaved ice" },
                    { 31, null, "Sweet beans" },
                    { 32, null, "Banana" },
                    { 36, null, "Caramel" },
                    { 37, null, "Tea" },
                    { 38, null, "Sugar syrup" },
                    { 39, null, "Ice" },
                    { 40, null, "Lemon" },
                    { 41, null, "Mango" },
                    { 42, null, "Sugar" },
                    { 43, null, "Pork leg" }
                });

            migrationBuilder.InsertData(
                table: "Tables",
                columns: new[] { "Number", "ClaimToken", "OccupiedAt" },
                values: new object[,]
                {
                    { 1, null, null },
                    { 2, null, null },
                    { 3, null, null },
                    { 4, null, null },
                    { 5, null, null },
                    { 6, null, null },
                    { 7, null, null },
                    { 8, null, null },
                    { 9, null, null },
                    { 10, null, null },
                    { 11, null, null },
                    { 12, null, null }
                });

            migrationBuilder.InsertData(
                table: "Ingredients",
                columns: new[] { "Id", "AllergenId", "Name" },
                values: new object[,]
                {
                    { 2, 6, "Soy sauce" },
                    { 7, 5, "Canton noodles" },
                    { 8, 8, "Shrimp" },
                    { 9, 4, "Egg" },
                    { 13, 1, "Peanut sauce" },
                    { 14, 8, "Shrimp paste" },
                    { 23, 7, "Fish sauce" },
                    { 27, 5, "Lumpia wrapper" },
                    { 30, 3, "Milk" },
                    { 33, 1, "Peanut topping" },
                    { 34, 3, "Condensed milk" },
                    { 35, 4, "Egg yolks" }
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
                table: "MenuItemIngredients",
                columns: new[] { "IngredientId", "MenuItemId", "IsRemovable" },
                values: new object[,]
                {
                    { 1, 1, false },
                    { 2, 1, false },
                    { 3, 1, false },
                    { 4, 1, true },
                    { 5, 1, true },
                    { 6, 1, true },
                    { 1, 2, true },
                    { 4, 2, true },
                    { 7, 2, false },
                    { 8, 2, true },
                    { 9, 2, true },
                    { 10, 2, true },
                    { 11, 2, true },
                    { 12, 3, false },
                    { 13, 3, false },
                    { 14, 3, true },
                    { 15, 3, true },
                    { 16, 3, true },
                    { 17, 3, true },
                    { 2, 4, false },
                    { 9, 4, true },
                    { 18, 4, false },
                    { 19, 4, true },
                    { 20, 4, true },
                    { 21, 4, true },
                    { 18, 5, false },
                    { 22, 5, false },
                    { 23, 5, true },
                    { 24, 5, true },
                    { 25, 5, true },
                    { 26, 5, true },
                    { 2, 6, true },
                    { 4, 6, true },
                    { 43, 6, false },
                    { 9, 7, false },
                    { 11, 7, true },
                    { 18, 7, false },
                    { 19, 7, true },
                    { 27, 7, false },
                    { 28, 8, false },
                    { 29, 9, false },
                    { 30, 9, true },
                    { 31, 9, true },
                    { 32, 9, true },
                    { 33, 9, true },
                    { 34, 10, false },
                    { 35, 10, false },
                    { 36, 10, false },
                    { 37, 11, false },
                    { 38, 11, true },
                    { 39, 11, true },
                    { 40, 11, true },
                    { 30, 12, true },
                    { 39, 12, true },
                    { 41, 12, false },
                    { 42, 12, true }
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
                name: "IX_Ingredients_AllergenId",
                table: "Ingredients",
                column: "AllergenId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemIngredients_IngredientId",
                table: "MenuItemIngredients",
                column: "IngredientId");

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
                name: "MenuItemIngredients");

            migrationBuilder.DropTable(
                name: "MenuItemOptions");

            migrationBuilder.DropTable(
                name: "OrderAllergies");

            migrationBuilder.DropTable(
                name: "OrderItemOptions");

            migrationBuilder.DropTable(
                name: "Tables");

            migrationBuilder.DropTable(
                name: "Ingredients");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Allergens");

            migrationBuilder.DropTable(
                name: "MenuItems");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}

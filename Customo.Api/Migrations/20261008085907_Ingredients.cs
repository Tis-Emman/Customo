using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Customo.Api.Migrations
{
    /// <inheritdoc />
    public partial class Ingredients : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MenuItemAllergens");

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "OrderItemOptions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

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

            migrationBuilder.InsertData(
                table: "Ingredients",
                columns: new[] { "Id", "AllergenId", "Name" },
                values: new object[,]
                {
                    { 1, null, "Chicken" },
                    { 2, 6, "Soy sauce" },
                    { 3, null, "Vinegar" },
                    { 4, null, "Garlic" },
                    { 5, null, "Bay leaf" },
                    { 6, null, "Black pepper" },
                    { 7, 5, "Canton noodles" },
                    { 8, 8, "Shrimp" },
                    { 9, 4, "Egg" },
                    { 10, null, "Cabbage" },
                    { 11, null, "Carrots" },
                    { 12, null, "Beef" },
                    { 13, 1, "Peanut sauce" },
                    { 14, 8, "Shrimp paste" },
                    { 15, null, "Eggplant" },
                    { 16, null, "String beans" },
                    { 17, null, "Bok choy" },
                    { 18, null, "Pork" },
                    { 19, null, "Onions" },
                    { 20, null, "Chili" },
                    { 21, null, "Calamansi" },
                    { 22, null, "Tamarind broth" },
                    { 23, 7, "Fish sauce" },
                    { 24, null, "Radish" },
                    { 25, null, "Kangkong" },
                    { 26, null, "Tomato" },
                    { 27, 5, "Lumpia wrapper" },
                    { 28, null, "Rice" },
                    { 29, null, "Shaved ice" },
                    { 30, 3, "Milk" },
                    { 31, null, "Sweet beans" },
                    { 32, null, "Banana" },
                    { 33, 1, "Peanut topping" },
                    { 34, 3, "Condensed milk" },
                    { 35, 4, "Egg yolks" },
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

            migrationBuilder.CreateIndex(
                name: "IX_Ingredients_AllergenId",
                table: "Ingredients",
                column: "AllergenId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemIngredients_IngredientId",
                table: "MenuItemIngredients",
                column: "IngredientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MenuItemIngredients");

            migrationBuilder.DropTable(
                name: "Ingredients");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "OrderItemOptions");

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

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemAllergens_AllergenId",
                table: "MenuItemAllergens",
                column: "AllergenId");
        }
    }
}

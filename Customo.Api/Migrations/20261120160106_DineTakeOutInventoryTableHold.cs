using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Customo.Api.Migrations
{
    /// <inheritdoc />
    public partial class DineTakeOutInventoryTableHold : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasOrdered",
                table: "Tables",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "TableNumber",
                table: "Orders",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Orders",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "DineIn");

            migrationBuilder.AddColumn<decimal>(
                name: "Quantity",
                table: "MenuItemIngredients",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 1m);   // existing recipes: one portion per serving

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "Ingredients",
                type: "bit",
                nullable: false,
                defaultValue: true); // existing ingredients start available

            migrationBuilder.AddColumn<decimal>(
                name: "LowAt",
                table: "Ingredients",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Stock",
                table: "Ingredients",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "Ingredients",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "portions");

            migrationBuilder.CreateTable(
                name: "StockMovements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IngredientId = table.Column<int>(type: "int", nullable: false),
                    Change = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockMovements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockMovements_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StockMovements_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id");
                });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "IsAvailable", "LowAt", "Stock", "Unit" },
                values: new object[] { true, null, null, "portions" });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "IsAvailable", "LowAt", "Stock", "Unit" },
                values: new object[] { true, null, null, "portions" });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "IsAvailable", "LowAt", "Stock", "Unit" },
                values: new object[] { true, null, null, "portions" });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "IsAvailable", "LowAt", "Stock", "Unit" },
                values: new object[] { true, null, null, "portions" });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "IsAvailable", "LowAt", "Stock", "Unit" },
                values: new object[] { true, null, null, "portions" });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "IsAvailable", "LowAt", "Stock", "Unit" },
                values: new object[] { true, null, null, "portions" });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "IsAvailable", "LowAt", "Stock", "Unit" },
                values: new object[] { true, null, null, "portions" });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "IsAvailable", "LowAt", "Stock", "Unit" },
                values: new object[] { true, null, null, "portions" });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "IsAvailable", "LowAt", "Stock", "Unit" },
                values: new object[] { true, null, null, "portions" });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "IsAvailable", "LowAt", "Stock", "Unit" },
                values: new object[] { true, null, null, "portions" });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "IsAvailable", "LowAt", "Stock", "Unit" },
                values: new object[] { true, null, null, "portions" });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "IsAvailable", "LowAt", "Stock", "Unit" },
                values: new object[] { true, null, null, "portions" });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "IsAvailable", "LowAt", "Stock", "Unit" },
                values: new object[] { true, null, null, "portions" });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "IsAvailable", "LowAt", "Stock", "Unit" },
                values: new object[] { true, null, null, "portions" });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "IsAvailable", "LowAt", "Stock", "Unit" },
                values: new object[] { true, null, null, "portions" });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "IsAvailable", "LowAt", "Stock", "Unit" },
                values: new object[] { true, null, null, "portions" });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "IsAvailable", "LowAt", "Stock", "Unit" },
                values: new object[] { true, null, null, "portions" });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "IsAvailable", "LowAt", "Stock", "Unit" },
                values: new object[] { true, null, null, "portions" });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "IsAvailable", "LowAt", "Stock", "Unit" },
                values: new object[] { true, null, null, "portions" });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "IsAvailable", "LowAt", "Stock", "Unit" },
                values: new object[] { true, null, null, "portions" });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "IsAvailable", "LowAt", "Stock", "Unit" },
                values: new object[] { true, null, null, "portions" });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "IsAvailable", "LowAt", "Stock", "Unit" },
                values: new object[] { true, null, null, "portions" });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "IsAvailable", "LowAt", "Stock", "Unit" },
                values: new object[] { true, null, null, "portions" });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "IsAvailable", "LowAt", "Stock", "Unit" },
                values: new object[] { true, null, null, "portions" });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 25,
                columns: new[] { "IsAvailable", "LowAt", "Stock", "Unit" },
                values: new object[] { true, null, null, "portions" });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "IsAvailable", "LowAt", "Stock", "Unit" },
                values: new object[] { true, null, null, "portions" });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "IsAvailable", "LowAt", "Stock", "Unit" },
                values: new object[] { true, null, null, "portions" });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "IsAvailable", "LowAt", "Stock", "Unit" },
                values: new object[] { true, null, null, "portions" });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "IsAvailable", "LowAt", "Stock", "Unit" },
                values: new object[] { true, null, null, "portions" });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "IsAvailable", "LowAt", "Stock", "Unit" },
                values: new object[] { true, null, null, "portions" });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "IsAvailable", "LowAt", "Stock", "Unit" },
                values: new object[] { true, null, null, "portions" });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "IsAvailable", "LowAt", "Stock", "Unit" },
                values: new object[] { true, null, null, "portions" });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 33,
                columns: new[] { "IsAvailable", "LowAt", "Stock", "Unit" },
                values: new object[] { true, null, null, "portions" });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "IsAvailable", "LowAt", "Stock", "Unit" },
                values: new object[] { true, null, null, "portions" });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "IsAvailable", "LowAt", "Stock", "Unit" },
                values: new object[] { true, null, null, "portions" });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "IsAvailable", "LowAt", "Stock", "Unit" },
                values: new object[] { true, null, null, "portions" });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "IsAvailable", "LowAt", "Stock", "Unit" },
                values: new object[] { true, null, null, "portions" });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "IsAvailable", "LowAt", "Stock", "Unit" },
                values: new object[] { true, null, null, "portions" });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "IsAvailable", "LowAt", "Stock", "Unit" },
                values: new object[] { true, null, null, "portions" });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 40,
                columns: new[] { "IsAvailable", "LowAt", "Stock", "Unit" },
                values: new object[] { true, null, null, "portions" });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "IsAvailable", "LowAt", "Stock", "Unit" },
                values: new object[] { true, null, null, "portions" });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "IsAvailable", "LowAt", "Stock", "Unit" },
                values: new object[] { true, null, null, "portions" });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "IsAvailable", "LowAt", "Stock", "Unit" },
                values: new object[] { true, null, null, "portions" });

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 1, 1 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 2, 1 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 3, 1 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 4, 1 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 5, 1 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 6, 1 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 1, 2 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 4, 2 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 7, 2 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 8, 2 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 9, 2 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 10, 2 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 11, 2 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 12, 3 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 13, 3 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 14, 3 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 15, 3 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 16, 3 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 17, 3 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 2, 4 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 9, 4 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 18, 4 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 19, 4 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 20, 4 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 21, 4 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 18, 5 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 22, 5 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 23, 5 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 24, 5 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 25, 5 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 26, 5 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 2, 6 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 4, 6 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 43, 6 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 9, 7 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 11, 7 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 18, 7 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 19, 7 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 27, 7 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 28, 8 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 29, 9 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 30, 9 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 31, 9 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 32, 9 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 33, 9 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 34, 10 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 35, 10 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 36, 10 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 37, 11 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 38, 11 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 39, 11 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 40, 11 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 30, 12 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 39, 12 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 41, 12 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "MenuItemIngredients",
                keyColumns: new[] { "IngredientId", "MenuItemId" },
                keyValues: new object[] { 42, 12 },
                column: "Quantity",
                value: 1m);

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Number",
                keyValue: 1,
                column: "HasOrdered",
                value: false);

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Number",
                keyValue: 2,
                column: "HasOrdered",
                value: false);

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Number",
                keyValue: 3,
                column: "HasOrdered",
                value: false);

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Number",
                keyValue: 4,
                column: "HasOrdered",
                value: false);

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Number",
                keyValue: 5,
                column: "HasOrdered",
                value: false);

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Number",
                keyValue: 6,
                column: "HasOrdered",
                value: false);

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Number",
                keyValue: 7,
                column: "HasOrdered",
                value: false);

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Number",
                keyValue: 8,
                column: "HasOrdered",
                value: false);

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Number",
                keyValue: 9,
                column: "HasOrdered",
                value: false);

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Number",
                keyValue: 10,
                column: "HasOrdered",
                value: false);

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Number",
                keyValue: 11,
                column: "HasOrdered",
                value: false);

            migrationBuilder.UpdateData(
                table: "Tables",
                keyColumn: "Number",
                keyValue: 12,
                column: "HasOrdered",
                value: false);

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_CreatedAt",
                table: "StockMovements",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_IngredientId",
                table: "StockMovements",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_OrderId",
                table: "StockMovements",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockMovements");

            migrationBuilder.DropColumn(
                name: "HasOrdered",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "MenuItemIngredients");

            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "LowAt",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "Stock",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "Ingredients");

            migrationBuilder.AlterColumn<int>(
                name: "TableNumber",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}

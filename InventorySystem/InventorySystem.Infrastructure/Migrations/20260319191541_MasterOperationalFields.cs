using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventorySystem.Infrastructure.Migrations
{
    public partial class MasterOperationalFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WarehouseId",
                table: "WarehouseBins",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "GstRate",
                table: "Products",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<int>(
                name: "PaymentTermsDays",
                table: "Customers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
                UPDATE wb
                SET wb.WarehouseId = w.Id
                FROM WarehouseBins wb
                INNER JOIN Warehouses w ON w.Name = wb.WarehouseName
            ");

            migrationBuilder.Sql(@"
                UPDATE WarehouseBins
                SET WarehouseId = (
                    SELECT TOP 1 Id
                    FROM Warehouses
                    ORDER BY Id
                )
                WHERE WarehouseId IS NULL
            ");

            migrationBuilder.AlterColumn<int>(
                name: "WarehouseId",
                table: "WarehouseBins",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseBins_WarehouseId",
                table: "WarehouseBins",
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_WarehouseBins_Warehouses_WarehouseId",
                table: "WarehouseBins",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WarehouseBins_Warehouses_WarehouseId",
                table: "WarehouseBins");

            migrationBuilder.DropIndex(
                name: "IX_WarehouseBins_WarehouseId",
                table: "WarehouseBins");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "WarehouseBins");

            migrationBuilder.DropColumn(
                name: "PaymentTermsDays",
                table: "Customers");

            migrationBuilder.AlterColumn<decimal>(
                name: "GstRate",
                table: "Products",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldPrecision: 5,
                oldScale: 2);
        }
    }
}

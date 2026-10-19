using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CountrySecure.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeEnumsAndNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Turnos: Pending pasa de 2 a 1 y Completed de 1 a 2 (mismo orden que el resto de estados).
            // Un solo UPDATE con CASE para no pisar valores a mitad de camino.
            migrationBuilder.Sql(@"UPDATE ""Turns"" SET ""TurnStatus"" = CASE ""TurnStatus"" WHEN 1 THEN 2 WHEN 2 THEN 1 ELSE ""TurnStatus"" END;");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Orders_IdOrder",
                table: "Requests");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Users_IdUser",
                table: "Requests");

            migrationBuilder.RenameColumn(
                name: "IdUser",
                table: "Requests",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "IdOrder",
                table: "Requests",
                newName: "OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_Requests_IdUser",
                table: "Requests",
                newName: "IX_Requests_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Requests_IdOrder",
                table: "Requests",
                newName: "IX_Requests_OrderId");

            migrationBuilder.RenameColumn(
                name: "OrderType",
                table: "Orders",
                newName: "Category");

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Orders_OrderId",
                table: "Requests",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Users_UserId",
                table: "Requests",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"UPDATE ""Turns"" SET ""TurnStatus"" = CASE ""TurnStatus"" WHEN 1 THEN 2 WHEN 2 THEN 1 ELSE ""TurnStatus"" END;");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Orders_OrderId",
                table: "Requests");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Users_UserId",
                table: "Requests");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Requests",
                newName: "IdUser");

            migrationBuilder.RenameColumn(
                name: "OrderId",
                table: "Requests",
                newName: "IdOrder");

            migrationBuilder.RenameIndex(
                name: "IX_Requests_UserId",
                table: "Requests",
                newName: "IX_Requests_IdUser");

            migrationBuilder.RenameIndex(
                name: "IX_Requests_OrderId",
                table: "Requests",
                newName: "IX_Requests_IdOrder");

            migrationBuilder.RenameColumn(
                name: "Category",
                table: "Orders",
                newName: "OrderType");

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Orders_IdOrder",
                table: "Requests",
                column: "IdOrder",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Users_IdUser",
                table: "Requests",
                column: "IdUser",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

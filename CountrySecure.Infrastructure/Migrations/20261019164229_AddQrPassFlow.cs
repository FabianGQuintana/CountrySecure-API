using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CountrySecure.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQrPassFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BlockReason",
                table: "Visits",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBlocked",
                table: "Visits",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "BlockReason",
                table: "Users",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBlocked",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CompanionsCount",
                table: "EntryPermissions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PassType",
                table: "EntryPermissions",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "SingleEntry");

            migrationBuilder.AddColumn<string>(
                name: "VehicleModel",
                table: "EntryPermissions",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehiclePlate",
                table: "EntryPermissions",
                type: "character varying(15)",
                maxLength: 15,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehicleType",
                table: "EntryPermissions",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "OnFoot");

            migrationBuilder.CreateTable(
                name: "AccessLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uuid", nullable: true),
                    GuardId = table.Column<Guid>(type: "uuid", nullable: false),
                    Outcome = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Reason = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Message = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    ScannedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Offline = table.Column<bool>(type: "boolean", nullable: false),
                    TokenPrefix = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccessLogs_EntryPermissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "EntryPermissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AccessLogs_Users_GuardId",
                        column: x => x.GuardId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccessLogs_GuardId",
                table: "AccessLogs",
                column: "GuardId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessLogs_PermissionId",
                table: "AccessLogs",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessLogs_ScannedAt",
                table: "AccessLogs",
                column: "ScannedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessLogs");

            migrationBuilder.DropColumn(
                name: "BlockReason",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "IsBlocked",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "BlockReason",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsBlocked",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CompanionsCount",
                table: "EntryPermissions");

            migrationBuilder.DropColumn(
                name: "PassType",
                table: "EntryPermissions");

            migrationBuilder.DropColumn(
                name: "VehicleModel",
                table: "EntryPermissions");

            migrationBuilder.DropColumn(
                name: "VehiclePlate",
                table: "EntryPermissions");

            migrationBuilder.DropColumn(
                name: "VehicleType",
                table: "EntryPermissions");
        }
    }
}

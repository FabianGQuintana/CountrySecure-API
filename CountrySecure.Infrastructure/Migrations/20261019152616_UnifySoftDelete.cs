using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CountrySecure.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UnifySoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Backfill: la baja lógica pasa a expresarse SOLO con "DeletedAt".
            //    Una fila se considera dada de baja si cualquiera de los mecanismos anteriores lo indicaba.
            migrationBuilder.Sql(@"UPDATE ""Amenities"" SET ""DeletedAt"" = COALESCE(""DeletedAt"", NOW()) WHERE ""Status"" = 'Inactive';");
            migrationBuilder.Sql(@"UPDATE ""EntryPermissions"" SET ""DeletedAt"" = COALESCE(""DeletedAt"", NOW()) WHERE ""Status"" = 'Inactive';");
            migrationBuilder.Sql(@"UPDATE ""Lots"" SET ""DeletedAt"" = COALESCE(""DeletedAt"", NOW()) WHERE ""Status"" = 'Inactive';");
            migrationBuilder.Sql(@"UPDATE ""Orders"" SET ""DeletedAt"" = COALESCE(""DeletedAt"", NOW()) WHERE ""Status"" = 'Inactive';");
            migrationBuilder.Sql(@"UPDATE ""Properties"" SET ""DeletedAt"" = COALESCE(""DeletedAt"", NOW()) WHERE ""Status"" = 'Inactive';");
            migrationBuilder.Sql(@"UPDATE ""Requests"" SET ""DeletedAt"" = COALESCE(""DeletedAt"", NOW()) WHERE ""Status"" = 'Inactive';");
            migrationBuilder.Sql(@"UPDATE ""Turns"" SET ""DeletedAt"" = COALESCE(""DeletedAt"", NOW()) WHERE ""Status"" = 'Inactive';");
            migrationBuilder.Sql(@"UPDATE ""Visits"" SET ""DeletedAt"" = COALESCE(""DeletedAt"", NOW()) WHERE ""Status"" = 'Inactive';");
            migrationBuilder.Sql(@"UPDATE ""Users"" SET ""DeletedAt"" = COALESCE(""DeletedAt"", NOW()) WHERE ""Status"" = 'Inactive' OR ""Active"" = FALSE;");
            migrationBuilder.Sql(@"UPDATE ""RefreshTokens"" SET ""DeletedAt"" = COALESCE(""DeletedAt"", NOW()) WHERE ""Status"" = 'Inactive';");

            // 2) Ahora sí se eliminan las columnas redundantes
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "Active",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Turns");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Lots");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "EntryPermissions");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Amenities");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Visits",
                type: "text",
                nullable: false,
                defaultValue: "Active");

            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "Active");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Turns",
                type: "text",
                nullable: false,
                defaultValue: "Active");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Requests",
                type: "text",
                nullable: false,
                defaultValue: "Active");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "RefreshTokens",
                type: "text",
                nullable: false,
                defaultValue: "Active");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Properties",
                type: "text",
                nullable: false,
                defaultValue: "Active");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Orders",
                type: "text",
                nullable: false,
                defaultValue: "Active");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Lots",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Active");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "EntryPermissions",
                type: "text",
                nullable: false,
                defaultValue: "Active");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Amenities",
                type: "text",
                nullable: false,
                defaultValue: "Active");

            // Reconstruye las columnas a partir de DeletedAt (la fuente de verdad)
            migrationBuilder.Sql(@"UPDATE ""Amenities"" SET ""Status"" = CASE WHEN ""DeletedAt"" IS NULL THEN 'Active' ELSE 'Inactive' END;");
            migrationBuilder.Sql(@"UPDATE ""EntryPermissions"" SET ""Status"" = CASE WHEN ""DeletedAt"" IS NULL THEN 'Active' ELSE 'Inactive' END;");
            migrationBuilder.Sql(@"UPDATE ""Lots"" SET ""Status"" = CASE WHEN ""DeletedAt"" IS NULL THEN 'Active' ELSE 'Inactive' END;");
            migrationBuilder.Sql(@"UPDATE ""Orders"" SET ""Status"" = CASE WHEN ""DeletedAt"" IS NULL THEN 'Active' ELSE 'Inactive' END;");
            migrationBuilder.Sql(@"UPDATE ""Properties"" SET ""Status"" = CASE WHEN ""DeletedAt"" IS NULL THEN 'Active' ELSE 'Inactive' END;");
            migrationBuilder.Sql(@"UPDATE ""Requests"" SET ""Status"" = CASE WHEN ""DeletedAt"" IS NULL THEN 'Active' ELSE 'Inactive' END;");
            migrationBuilder.Sql(@"UPDATE ""Turns"" SET ""Status"" = CASE WHEN ""DeletedAt"" IS NULL THEN 'Active' ELSE 'Inactive' END;");
            migrationBuilder.Sql(@"UPDATE ""Visits"" SET ""Status"" = CASE WHEN ""DeletedAt"" IS NULL THEN 'Active' ELSE 'Inactive' END;");
            migrationBuilder.Sql(@"UPDATE ""Users"" SET ""Status"" = CASE WHEN ""DeletedAt"" IS NULL THEN 'Active' ELSE 'Inactive' END;");
            migrationBuilder.Sql(@"UPDATE ""RefreshTokens"" SET ""Status"" = CASE WHEN ""DeletedAt"" IS NULL THEN 'Active' ELSE 'Inactive' END;");
            migrationBuilder.Sql(@"UPDATE ""Users"" SET ""Active"" = (""DeletedAt"" IS NULL);");

        }
    }
}

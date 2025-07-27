using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NicolasQuiPaieAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "1af17a5e-3d0e-4366-ab16-bc988f7cfd6b", "59a7a3e9-20b8-4169-99a5-ee9be36a486b", "User", "USER" },
                    { "5611b112-f104-4f6d-86cb-1c2b5a79ec40", "87841e58-d992-4236-ae83-7ccf4efe36c0", "SuperUser", "SUPERUSER" },
                    { "ffbea19a-4701-4b91-b145-a0723ec78f89", "7469e6ee-42c9-44aa-89be-d35e68e10e44", "Admin", "ADMIN" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1af17a5e-3d0e-4366-ab16-bc988f7cfd6b");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5611b112-f104-4f6d-86cb-1c2b5a79ec40");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "ffbea19a-4701-4b91-b145-a0723ec78f89");
        }
    }
}

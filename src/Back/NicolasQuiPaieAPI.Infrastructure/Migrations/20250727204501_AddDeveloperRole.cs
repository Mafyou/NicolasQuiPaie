using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NicolasQuiPaieAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeveloperRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "ffbea19a-4701-4b91-b145-a0723ec78f8A", "7469e6ee-42c9-44aa-89be-d35e68e10e45", "Developer", "DEVELOPER" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "ffbea19a-4701-4b91-b145-a0723ec78f8A");
        }
    }
}

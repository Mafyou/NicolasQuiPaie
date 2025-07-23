using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NicolasQuiPaieAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Contributor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FiscalLevel",
                table: "AspNetUsers",
                newName: "ContributionLevel");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ContributionLevel",
                table: "AspNetUsers",
                newName: "FiscalLevel");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NicolasQuiPaieAPI.Infrastructure.Migrations;

/// <inheritdoc />
public partial class _20261126080416_MoveToNqpSchema : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "nqp");

        migrationBuilder.RenameTable(
            name: "Votes",
            newName: "Votes",
            newSchema: "nqp");

        migrationBuilder.RenameTable(
            name: "Proposals",
            newName: "Proposals",
            newSchema: "nqp");

        migrationBuilder.RenameTable(
            name: "Comments",
            newName: "Comments",
            newSchema: "nqp");

        migrationBuilder.RenameTable(
            name: "CommentLikes",
            newName: "CommentLikes",
            newSchema: "nqp");

        migrationBuilder.RenameTable(
            name: "Categories",
            newName: "Categories",
            newSchema: "nqp");

        migrationBuilder.RenameTable(
            name: "AspNetUserTokens",
            newName: "AspNetUserTokens",
            newSchema: "nqp");

        migrationBuilder.RenameTable(
            name: "AspNetUsers",
            newName: "AspNetUsers",
            newSchema: "nqp");

        migrationBuilder.RenameTable(
            name: "AspNetUserRoles",
            newName: "AspNetUserRoles",
            newSchema: "nqp");

        migrationBuilder.RenameTable(
            name: "AspNetUserLogins",
            newName: "AspNetUserLogins",
            newSchema: "nqp");

        migrationBuilder.RenameTable(
            name: "AspNetUserClaims",
            newName: "AspNetUserClaims",
            newSchema: "nqp");

        migrationBuilder.RenameTable(
            name: "AspNetRoles",
            newName: "AspNetRoles",
            newSchema: "nqp");

        migrationBuilder.RenameTable(
            name: "AspNetRoleClaims",
            newName: "AspNetRoleClaims",
            newSchema: "nqp");

        migrationBuilder.RenameTable(
            name: "ApiLogs",
            newName: "ApiLogs",
            newSchema: "nqp");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameTable(
            name: "Votes",
            schema: "nqp",
            newName: "Votes");

        migrationBuilder.RenameTable(
            name: "Proposals",
            schema: "nqp",
            newName: "Proposals");

        migrationBuilder.RenameTable(
            name: "Comments",
            schema: "nqp",
            newName: "Comments");

        migrationBuilder.RenameTable(
            name: "CommentLikes",
            schema: "nqp",
            newName: "CommentLikes");

        migrationBuilder.RenameTable(
            name: "Categories",
            schema: "nqp",
            newName: "Categories");

        migrationBuilder.RenameTable(
            name: "AspNetUserTokens",
            schema: "nqp",
            newName: "AspNetUserTokens");

        migrationBuilder.RenameTable(
            name: "AspNetUsers",
            schema: "nqp",
            newName: "AspNetUsers");

        migrationBuilder.RenameTable(
            name: "AspNetUserRoles",
            schema: "nqp",
            newName: "AspNetUserRoles");

        migrationBuilder.RenameTable(
            name: "AspNetUserLogins",
            schema: "nqp",
            newName: "AspNetUserLogins");

        migrationBuilder.RenameTable(
            name: "AspNetUserClaims",
            schema: "nqp",
            newName: "AspNetUserClaims");

        migrationBuilder.RenameTable(
            name: "AspNetRoles",
            schema: "nqp",
            newName: "AspNetRoles");

        migrationBuilder.RenameTable(
            name: "AspNetRoleClaims",
            schema: "nqp",
            newName: "AspNetRoleClaims");

        migrationBuilder.RenameTable(
            name: "ApiLogs",
            schema: "nqp",
            newName: "ApiLogs");
    }
}

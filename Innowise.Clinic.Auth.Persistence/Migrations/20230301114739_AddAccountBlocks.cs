using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Innowise.Clinic.Auth.Persistence.Migrations
{
    public partial class AddAccountBlocks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountBlocks",
                columns: table => new
                {
                    AccountBlockId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountBlocks", x => x.AccountBlockId);
                    table.ForeignKey(
                        name: "FK_AccountBlocks_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("1bb5d47f-bab5-4135-945c-60da30ea104d"),
                column: "ConcurrencyStamp",
                value: "60ab9283-5918-4883-a459-d17104df34da");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("8c1054b3-6986-468b-8583-72e5c53f5a20"),
                column: "ConcurrencyStamp",
                value: "4650906b-f630-4297-8abd-e32051080a8f");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("c1460814-4592-4cd9-944e-691db26b315e"),
                column: "ConcurrencyStamp",
                value: "92cb9dc2-b090-4246-bd02-91b78e5305ae");

            migrationBuilder.CreateIndex(
                name: "IX_AccountBlocks_UserId",
                table: "AccountBlocks",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountBlocks");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("1bb5d47f-bab5-4135-945c-60da30ea104d"),
                column: "ConcurrencyStamp",
                value: "03210297-b82e-4384-ae1a-ac2e7e900835");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("8c1054b3-6986-468b-8583-72e5c53f5a20"),
                column: "ConcurrencyStamp",
                value: "6c47cf6b-2b04-4bac-a635-b8f107bfda8f");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("c1460814-4592-4cd9-944e-691db26b315e"),
                column: "ConcurrencyStamp",
                value: "e1a6dda4-f16b-4419-b587-9c483599238d");
        }
    }
}

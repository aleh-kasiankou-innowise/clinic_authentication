using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Innowise.Clinic.Auth.Persistence.Migrations
{
    public partial class Add_Roles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { new Guid("559ad71d-a52c-4973-ac27-324924b69716"), "635a2ee9-192f-474b-bc3a-fd4ef935e9c9", "Doctor", "DOCTOR" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { new Guid("9dcef89f-dc49-440f-9ab3-9579876f0318"), "47cba5ab-09ed-4519-84dd-9d07a1ed8227", "Patient", "PATIENT" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { new Guid("cefe4306-f476-4f23-84f8-425392e2adfe"), "4cf4245a-4c9d-4a30-8987-bb8337021819", "Receptionist", "RECEPTIONIST" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("559ad71d-a52c-4973-ac27-324924b69716"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("9dcef89f-dc49-440f-9ab3-9579876f0318"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("cefe4306-f476-4f23-84f8-425392e2adfe"));
        }
    }
}

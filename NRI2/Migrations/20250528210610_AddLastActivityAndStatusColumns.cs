using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NRI.Migrations
{
    public partial class AddLastActivityAndStatusColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastActivity",
                table: "Users",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Users",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 1,
                column: "password",
                value: "$2a$11$f2wnWY/Sa2WiOaAtsXNSa.RyE9H5Lqrn2HuMC4KPn94z.wG6ehHd2");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastActivity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 1,
                column: "password",
                value: "$2a$11$OJNZ1pIcETCs3CS1f6iQh.34fze2Z/iktVdZhSVqcRVlDP.3CxlMu");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

namespace NRI.Migrations
{
    public partial class DiceRoll : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 1,
                column: "password",
                value: "$2a$11$CjD7YNEzfSmUmlMuyY.cQeYl8n1Q5AhsRfU.aV2I.93jYDaMxps4u");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 1,
                column: "password",
                value: "$2a$11$zfssgPYWEEsosgUGhg3Mh.G6h7akYrtZHMTUDN00Exh9RnaHaefx.");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

namespace NRI.Migrations
{
    public partial class FixJsonCollections : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 1,
                column: "password",
                value: "$2a$11$fqliBqKf5FOB97SuawIB4OZrWhaj7PVgI0I1UeiOexLq/e3PBXh4a");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 1,
                column: "password",
                value: "$2a$11$/MmCoMy8em32Mib7Sb.9ke28.a8/pRWzFt5pJkBwQWoRRDlMntQZW");
        }
    }
}

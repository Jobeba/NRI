using Microsoft.EntityFrameworkCore.Migrations;

namespace NRI.Migrations
{
    public partial class AddInventoryColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Inventory",
                table: "Characters",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 1,
                column: "password",
                value: "$2a$11$x9jcZkR4.G/MS09sGiBVsem7sqhz171GQpzqAAO5mi1VYMjkuDcL6");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Inventory",
                table: "Characters");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 1,
                column: "password",
                value: "$2a$11$dnk3zpfRJd4M4o2k74LgRuYEijiMUQm8nDIApeyJ9Cs2tBArh3LiO");
        }
    }
}

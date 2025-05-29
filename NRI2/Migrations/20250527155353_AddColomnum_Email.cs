using Microsoft.EntityFrameworkCore.Migrations;

namespace NRI.Migrations
{
    public partial class AddColomnum_Email : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "email_confirmed",
                table: "Users",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 1,
                column: "password",
                value: "$2a$11$OJNZ1pIcETCs3CS1f6iQh.34fze2Z/iktVdZhSVqcRVlDP.3CxlMu");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "email_confirmed",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 1,
                column: "password",
                value: "$2a$11$5tDZZXsEO6GwJwUyYc/r.eoO7cycmvqRlk3zvyry80/paMV0Sr2oK");
        }
    }
}

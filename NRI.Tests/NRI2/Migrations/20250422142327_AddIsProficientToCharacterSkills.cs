using Microsoft.EntityFrameworkCore.Migrations;

namespace NRI.Migrations
{
    public partial class AddIsProficientToCharacterSkills : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsProficient",
                table: "CharacterSkills",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 1,
                column: "password",
                value: "$2a$11$zpWqYgaymlIoca7nHrvpGuKeObawOmHjk3sq/nVigAbM2oyZhvRei");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsProficient",
                table: "CharacterSkills");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 1,
                column: "password",
                value: "$2a$11$x9jcZkR4.G/MS09sGiBVsem7sqhz171GQpzqAAO5mi1VYMjkuDcL6");
        }
    }
}

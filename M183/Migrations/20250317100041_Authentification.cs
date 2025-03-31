using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace M183.Migrations
{
    /// <inheritdoc />
    public partial class Authentification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SecretKey",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "SecretKey",
                value: "GYZTOOBVGQ3DEOBZGQ3DSMRYG4ZTMNBZHA3TEMZWGQ");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "SecretKey",
                value: "GYZTOOBVGQ3DEOBZGQ3DSMRYG4ZTMNBZHA3TEMZWGQ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SecretKey",
                table: "Users");
        }
    }
}

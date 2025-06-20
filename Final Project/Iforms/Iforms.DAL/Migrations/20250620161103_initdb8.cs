using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Iforms.DAL.Migrations
{
    /// <inheritdoc />
    public partial class initdb8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "UserRole",
                table: "Users",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserRole",
                table: "Users",
                type: "VARCHAR",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}

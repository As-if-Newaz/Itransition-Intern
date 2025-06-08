using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Presentation_Collab.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePresentationStringLengths : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Presentation",
                type: "VARCHAR(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "CreatorName",
                table: "Presentation",
                type: "VARCHAR(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(50)",
                oldMaxLength: 50);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Presentation",
                type: "VARCHAR(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "CreatorName",
                table: "Presentation",
                type: "VARCHAR(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(255)",
                oldMaxLength: 255);
        }
    }
}

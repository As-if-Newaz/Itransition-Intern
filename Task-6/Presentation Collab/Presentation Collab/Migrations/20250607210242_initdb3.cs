using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Presentation_Collab.Migrations
{
    /// <inheritdoc />
    public partial class initdb3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Presentation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "VARCHAR(50)", maxLength: 50, nullable: false),
                    CreatorName = table.Column<string>(type: "VARCHAR(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Presentation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Slides",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Order = table.Column<int>(type: "int", nullable: false),
                    SvgData = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PresentationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Slides", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Slides_Presentation_PresentationId",
                        column: x => x.PresentationId,
                        principalTable: "Presentation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "VARCHAR(50)", maxLength: 50, nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    PresentationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Presentation_PresentationId",
                        column: x => x.PresentationId,
                        principalTable: "Presentation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Slides_PresentationId",
                table: "Slides",
                column: "PresentationId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_PresentationId",
                table: "Users",
                column: "PresentationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Slides");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Presentation");
        }
    }
}

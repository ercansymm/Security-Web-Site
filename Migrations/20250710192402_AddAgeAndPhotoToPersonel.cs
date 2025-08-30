using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecurityWebSite.Migrations
{
    /// <inheritdoc />
    public partial class AddAgeAndPhotoToPersonel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Firms",
                columns: table => new
                {
                    Ref = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PersonelRef = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CardName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "Personel",
                columns: table => new
                {
                    Ref = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CardName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    City = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Working = table.Column<bool>(type: "bit", nullable: true),
                    Gun = table.Column<bool>(type: "bit", nullable: true),
                    Shift = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Ref = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CardName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UserPassword = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "WebDes",
                columns: table => new
                {
                    Ref = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CardDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Firms");

            migrationBuilder.DropTable(
                name: "Personel");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "WebDes");
        }
    }
}

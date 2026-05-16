using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovcoreBse.Store.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CoreActivityLog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserKey = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ActivityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ActivityMessage = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ActivityEncData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    updatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoreActivityLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CoreFileDoc",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LinkTable = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LinkId = table.Column<int>(type: "int", nullable: false),
                    DocType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UploadFilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RelativePath = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    updatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    updatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    updatedPost = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoreFileDoc", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CoreSetting",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SettingId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SettingValue = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CanEdit = table.Column<bool>(type: "bit", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    updatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoreSetting", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CoreUser",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Person = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EncPassword = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Disabled = table.Column<bool>(type: "bit", nullable: false),
                    IsAdmin = table.Column<bool>(type: "bit", nullable: false),
                    IsReset = table.Column<bool>(type: "bit", nullable: false),
                    loginedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    updatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    updatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    level = table.Column<int>(type: "int", nullable: false),
                    post = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    tel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Division = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AdminScope = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    email = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoreUser", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CoreActivityLog");

            migrationBuilder.DropTable(
                name: "CoreFileDoc");

            migrationBuilder.DropTable(
                name: "CoreSetting");

            migrationBuilder.DropTable(
                name: "CoreUser");
        }
    }
}

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace qtbot.Migrations
{
    public partial class XPUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    PrimaryKey = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DisplayXP = table.Column<int>(nullable: false),
                    FullXP = table.Column<int>(nullable: false),
                    LastMessage = table.Column<DateTime>(nullable: false),
                    LastResettedXP = table.Column<DateTime>(nullable: false),
                    ServerID = table.Column<ulong>(nullable: false),
                    UserID = table.Column<ulong>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.PrimaryKey);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "users");
        }
    }
}

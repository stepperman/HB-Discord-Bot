using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace qtbot.Migrations
{
    public partial class Exclude : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ExcludeFromStats",
                table: "users",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExcludeFromStats",
                table: "users");
        }
    }
}

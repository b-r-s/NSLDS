using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace @global.domain.Migrations
{
    public partial class PellAward : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PellAward",
                schema: "nslds",
                columns: table => new
                {
                    AwardYear = table.Column<int>(nullable: false),
                    AYDisplay = table.Column<string>(nullable: true),
                    MaxAmount = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PellAward", x => x.AwardYear);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "PellAward", schema: "nslds");
        }
    }
}

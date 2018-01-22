using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace @global.domain.Migrations
{
    public partial class Fah_Tables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema("nslds");
            migrationBuilder.CreateTable(
                name: "FahAlert",
                schema: "nslds",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FahAlert", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "FahCode",
                schema: "nslds",
                columns: table => new
                {
                    FahFieldId = table.Column<string>(nullable: false),
                    Code = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FahCode", x => new { x.FahFieldId, x.Code });
                });
            migrationBuilder.CreateTable(
                name: "FahField",
                schema: "nslds",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FahField", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "FahAlert", schema: "nslds");
            migrationBuilder.DropTable(name: "FahCode", schema: "nslds");
            migrationBuilder.DropTable(name: "FahField", schema: "nslds");
        }
    }
}

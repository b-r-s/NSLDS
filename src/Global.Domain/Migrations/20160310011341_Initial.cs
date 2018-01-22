using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace @global.domain.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema("dbo");
            migrationBuilder.CreateTable(
                name: "FedSchoolCodeList",
                schema: "dbo",
                columns: table => new
                {
                    SchoolCode = table.Column<string>(nullable: false),
                    SchoolName = table.Column<string>(nullable: true),
                    Address = table.Column<string>(nullable: true),
                    City = table.Column<string>(nullable: true),
                    StateCode = table.Column<string>(nullable: true),
                    ZipCode = table.Column<string>(nullable: true),
                    Province = table.Column<string>(nullable: true),
                    Country = table.Column<string>(nullable: true),
                    PostalCode = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FedSchoolCodeList", x => x.SchoolCode);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "FedSchoolCodeList", schema: "dbo");
        }
    }
}

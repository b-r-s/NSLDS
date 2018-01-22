using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace @global.domain.Migrations
{
    public partial class Create_Tenant_Table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema("dbo");
            migrationBuilder.CreateTable(
                name: "Tenant",
                schema: "dbo",
                columns: table => new
                {
                    TenantId = table.Column<string>(nullable: false),
                    TenantDomain = table.Column<string>(nullable: true),
                    DatabaseName = table.Column<string>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenant", x => x.TenantId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Tenant", schema: "dbo");
        }
    }
}

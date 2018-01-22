using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace @global.domain.Migrations
{
    public partial class Tenant_Active : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "dbo",
                table: "Tenant",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "IsActive", schema: "dbo", table: "Tenant");
        }
    }
}

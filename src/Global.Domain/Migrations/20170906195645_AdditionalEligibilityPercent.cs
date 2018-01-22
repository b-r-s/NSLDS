using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace @global.domain.Migrations
{
    public partial class AdditionalEligibilityPercent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AdditionalPercent",
                schema: "nslds",
                table: "PellAward",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "AdditionalPercent", schema: "nslds", table: "PellAward");
        }
    }
}

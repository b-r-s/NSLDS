using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace nslds.domain.Migrations
{
    public partial class Encrypt_DataProviderID : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "DataProviderID", schema: "nslds", table: "NsldsFAHType5");
            migrationBuilder.AddColumn<string>(
                name: "EncDataProviderID",
                schema: "nslds",
                table: "NsldsFAHType5",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "EncDataProviderID", schema: "nslds", table: "NsldsFAHType5");
            migrationBuilder.AddColumn<string>(
                name: "DataProviderID",
                schema: "nslds",
                table: "NsldsFAHType5",
                nullable: true);
        }
    }
}

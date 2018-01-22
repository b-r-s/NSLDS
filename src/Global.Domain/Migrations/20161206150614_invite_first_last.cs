using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace @global.domain.Migrations
{
    public partial class invite_first_last : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "UserName", schema: "nslds", table: "UserInvite");
            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                schema: "nslds",
                table: "UserInvite",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "LastName",
                schema: "nslds",
                table: "UserInvite",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "FirstName", schema: "nslds", table: "UserInvite");
            migrationBuilder.DropColumn(name: "LastName", schema: "nslds", table: "UserInvite");
            migrationBuilder.AddColumn<string>(
                name: "UserName",
                schema: "nslds",
                table: "UserInvite",
                nullable: true);
        }
    }
}

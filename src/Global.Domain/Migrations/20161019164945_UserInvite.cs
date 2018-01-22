using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace @global.domain.Migrations
{
    public partial class UserInvite : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserInvite",
                schema: "nslds",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ExpireOn = table.Column<DateTime>(nullable: false),
                    HasRegistered = table.Column<bool>(nullable: false, defaultValue: false),
                    OpeId = table.Column<string>(nullable: false),
                    SenderEmail = table.Column<string>(nullable: false),
                    SenderName = table.Column<string>(nullable: true),
                    UserEmail = table.Column<string>(nullable: false),
                    UserName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserInvite", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "UserInvite", schema: "nslds");
        }
    }
}

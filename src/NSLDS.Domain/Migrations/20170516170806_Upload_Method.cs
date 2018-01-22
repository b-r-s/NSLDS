using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace nslds.domain.Migrations
{
    public partial class Upload_Method : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_ClientRequest_ClientProfile_Link2ClientProfile", schema: "nslds", table: "ClientRequest");
            migrationBuilder.DropForeignKey(name: "FK_ClientRequestAlert_ClientRequest_Link2ClientRequest", schema: "nslds", table: "ClientRequestAlert");
            migrationBuilder.DropForeignKey(name: "FK_ClientRequestStudent_ClientRequest_Link2ClientRequest", schema: "nslds", table: "ClientRequestStudent");
            migrationBuilder.DropForeignKey(name: "FK_ClientRequestStudentAlert_ClientRequestStudent_Link2ClientRequestStudent", schema: "nslds", table: "ClientRequestStudentAlert");
            migrationBuilder.DropForeignKey(name: "FK_Job_ClientProfile_Link2ClientProfile", schema: "nslds", table: "Job");
            migrationBuilder.DropForeignKey(name: "FK_NsldsFAHType1_ClientRequestStudent_Link2ClientRequestStudent", schema: "nslds", table: "NsldsFAHType1");
            migrationBuilder.DropForeignKey(name: "FK_NsldsFAHType2_ClientRequestStudent_Link2ClientRequestStudent", schema: "nslds", table: "NsldsFAHType2");
            migrationBuilder.DropForeignKey(name: "FK_NsldsFAHType3_ClientRequestStudent_Link2ClientRequestStudent", schema: "nslds", table: "NsldsFAHType3");
            migrationBuilder.DropForeignKey(name: "FK_NsldsFAHType4_ClientRequestStudent_Link2ClientRequestStudent", schema: "nslds", table: "NsldsFAHType4");
            migrationBuilder.DropForeignKey(name: "FK_NsldsFAHType5_ClientRequestStudent_Link2ClientRequestStudent", schema: "nslds", table: "NsldsFAHType5");
            migrationBuilder.AddColumn<string>(
                name: "Upload_Method",
                schema: "nslds",
                table: "ClientProfile_History",
                nullable: true, maxLength: 1);
            migrationBuilder.AddColumn<string>(
                name: "Upload_Method",
                schema: "nslds",
                table: "ClientProfile",
                nullable: true, maxLength: 1);
            migrationBuilder.AddForeignKey(
                name: "FK_ClientRequest_ClientProfile_Link2ClientProfile",
                schema: "nslds",
                table: "ClientRequest",
                column: "Link2ClientProfile",
                principalSchema: "nslds",
                principalTable: "ClientProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_ClientRequestAlert_ClientRequest_Link2ClientRequest",
                schema: "nslds",
                table: "ClientRequestAlert",
                column: "Link2ClientRequest",
                principalSchema: "nslds",
                principalTable: "ClientRequest",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_ClientRequestStudent_ClientRequest_Link2ClientRequest",
                schema: "nslds",
                table: "ClientRequestStudent",
                column: "Link2ClientRequest",
                principalSchema: "nslds",
                principalTable: "ClientRequest",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_ClientRequestStudentAlert_ClientRequestStudent_Link2ClientRequestStudent",
                schema: "nslds",
                table: "ClientRequestStudentAlert",
                column: "Link2ClientRequestStudent",
                principalSchema: "nslds",
                principalTable: "ClientRequestStudent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_Job_ClientProfile_Link2ClientProfile",
                schema: "nslds",
                table: "Job",
                column: "Link2ClientProfile",
                principalSchema: "nslds",
                principalTable: "ClientProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_NsldsFAHType1_ClientRequestStudent_Link2ClientRequestStudent",
                schema: "nslds",
                table: "NsldsFAHType1",
                column: "Link2ClientRequestStudent",
                principalSchema: "nslds",
                principalTable: "ClientRequestStudent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_NsldsFAHType2_ClientRequestStudent_Link2ClientRequestStudent",
                schema: "nslds",
                table: "NsldsFAHType2",
                column: "Link2ClientRequestStudent",
                principalSchema: "nslds",
                principalTable: "ClientRequestStudent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_NsldsFAHType3_ClientRequestStudent_Link2ClientRequestStudent",
                schema: "nslds",
                table: "NsldsFAHType3",
                column: "Link2ClientRequestStudent",
                principalSchema: "nslds",
                principalTable: "ClientRequestStudent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_NsldsFAHType4_ClientRequestStudent_Link2ClientRequestStudent",
                schema: "nslds",
                table: "NsldsFAHType4",
                column: "Link2ClientRequestStudent",
                principalSchema: "nslds",
                principalTable: "ClientRequestStudent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_NsldsFAHType5_ClientRequestStudent_Link2ClientRequestStudent",
                schema: "nslds",
                table: "NsldsFAHType5",
                column: "Link2ClientRequestStudent",
                principalSchema: "nslds",
                principalTable: "ClientRequestStudent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_ClientRequest_ClientProfile_Link2ClientProfile", schema: "nslds", table: "ClientRequest");
            migrationBuilder.DropForeignKey(name: "FK_ClientRequestAlert_ClientRequest_Link2ClientRequest", schema: "nslds", table: "ClientRequestAlert");
            migrationBuilder.DropForeignKey(name: "FK_ClientRequestStudent_ClientRequest_Link2ClientRequest", schema: "nslds", table: "ClientRequestStudent");
            migrationBuilder.DropForeignKey(name: "FK_ClientRequestStudentAlert_ClientRequestStudent_Link2ClientRequestStudent", schema: "nslds", table: "ClientRequestStudentAlert");
            migrationBuilder.DropForeignKey(name: "FK_Job_ClientProfile_Link2ClientProfile", schema: "nslds", table: "Job");
            migrationBuilder.DropForeignKey(name: "FK_NsldsFAHType1_ClientRequestStudent_Link2ClientRequestStudent", schema: "nslds", table: "NsldsFAHType1");
            migrationBuilder.DropForeignKey(name: "FK_NsldsFAHType2_ClientRequestStudent_Link2ClientRequestStudent", schema: "nslds", table: "NsldsFAHType2");
            migrationBuilder.DropForeignKey(name: "FK_NsldsFAHType3_ClientRequestStudent_Link2ClientRequestStudent", schema: "nslds", table: "NsldsFAHType3");
            migrationBuilder.DropForeignKey(name: "FK_NsldsFAHType4_ClientRequestStudent_Link2ClientRequestStudent", schema: "nslds", table: "NsldsFAHType4");
            migrationBuilder.DropForeignKey(name: "FK_NsldsFAHType5_ClientRequestStudent_Link2ClientRequestStudent", schema: "nslds", table: "NsldsFAHType5");
            migrationBuilder.DropColumn(name: "Upload_Method", schema: "nslds", table: "ClientProfile_History");
            migrationBuilder.DropColumn(name: "Upload_Method", schema: "nslds", table: "ClientProfile");
            migrationBuilder.AddForeignKey(
                name: "FK_ClientRequest_ClientProfile_Link2ClientProfile",
                schema: "nslds",
                table: "ClientRequest",
                column: "Link2ClientProfile",
                principalSchema: "nslds",
                principalTable: "ClientProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_ClientRequestAlert_ClientRequest_Link2ClientRequest",
                schema: "nslds",
                table: "ClientRequestAlert",
                column: "Link2ClientRequest",
                principalSchema: "nslds",
                principalTable: "ClientRequest",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_ClientRequestStudent_ClientRequest_Link2ClientRequest",
                schema: "nslds",
                table: "ClientRequestStudent",
                column: "Link2ClientRequest",
                principalSchema: "nslds",
                principalTable: "ClientRequest",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_ClientRequestStudentAlert_ClientRequestStudent_Link2ClientRequestStudent",
                schema: "nslds",
                table: "ClientRequestStudentAlert",
                column: "Link2ClientRequestStudent",
                principalSchema: "nslds",
                principalTable: "ClientRequestStudent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_Job_ClientProfile_Link2ClientProfile",
                schema: "nslds",
                table: "Job",
                column: "Link2ClientProfile",
                principalSchema: "nslds",
                principalTable: "ClientProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_NsldsFAHType1_ClientRequestStudent_Link2ClientRequestStudent",
                schema: "nslds",
                table: "NsldsFAHType1",
                column: "Link2ClientRequestStudent",
                principalSchema: "nslds",
                principalTable: "ClientRequestStudent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_NsldsFAHType2_ClientRequestStudent_Link2ClientRequestStudent",
                schema: "nslds",
                table: "NsldsFAHType2",
                column: "Link2ClientRequestStudent",
                principalSchema: "nslds",
                principalTable: "ClientRequestStudent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_NsldsFAHType3_ClientRequestStudent_Link2ClientRequestStudent",
                schema: "nslds",
                table: "NsldsFAHType3",
                column: "Link2ClientRequestStudent",
                principalSchema: "nslds",
                principalTable: "ClientRequestStudent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_NsldsFAHType4_ClientRequestStudent_Link2ClientRequestStudent",
                schema: "nslds",
                table: "NsldsFAHType4",
                column: "Link2ClientRequestStudent",
                principalSchema: "nslds",
                principalTable: "ClientRequestStudent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_NsldsFAHType5_ClientRequestStudent_Link2ClientRequestStudent",
                schema: "nslds",
                table: "NsldsFAHType5",
                column: "Link2ClientRequestStudent",
                principalSchema: "nslds",
                principalTable: "ClientRequestStudent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

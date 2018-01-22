using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace nslds.domain.Migrations
{
    public partial class TM_Alerts : Migration
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
            migrationBuilder.AddColumn<bool>(
                name: "IsGrantReviewed",
                schema: "nslds",
                table: "ClientRequestStudent_History",
                nullable: true);
            migrationBuilder.AddColumn<bool>(
                name: "IsLoanReviewed",
                schema: "nslds",
                table: "ClientRequestStudent_History",
                nullable: true);
            migrationBuilder.AddColumn<bool>(
                name: "IsPellReviewed",
                schema: "nslds",
                table: "ClientRequestStudent_History",
                nullable: true);
            migrationBuilder.AddColumn<bool>(
                name: "IsRefreshed",
                schema: "nslds",
                table: "ClientRequestStudent_History",
                nullable: false,
                defaultValue: false);
            migrationBuilder.AddColumn<bool>(
                name: "IsTeachReviewed",
                schema: "nslds",
                table: "ClientRequestStudent_History",
                nullable: true);
            migrationBuilder.AddColumn<bool>(
                name: "IsGrantReviewed",
                schema: "nslds",
                table: "ClientRequestStudent",
                nullable: true);
            migrationBuilder.AddColumn<bool>(
                name: "IsLoanReviewed",
                schema: "nslds",
                table: "ClientRequestStudent",
                nullable: true);
            migrationBuilder.AddColumn<bool>(
                name: "IsPellReviewed",
                schema: "nslds",
                table: "ClientRequestStudent",
                nullable: true);
            migrationBuilder.AddColumn<bool>(
                name: "IsRefreshed",
                schema: "nslds",
                table: "ClientRequestStudent",
                nullable: false,
                defaultValue: false);
            migrationBuilder.AddColumn<bool>(
                name: "IsTeachReviewed",
                schema: "nslds",
                table: "ClientRequestStudent",
                nullable: true);
            migrationBuilder.AlterColumn<bool>(
                name: "IsOnHold",
                schema: "nslds",
                table: "ClientRequest",
                nullable: false,
                defaultValue: false);
            migrationBuilder.AlterColumn<bool>(
                name: "IsFailed",
                schema: "nslds",
                table: "ClientRequest",
                nullable: false,
                defaultValue: false);
            migrationBuilder.AddColumn<bool>(
                name: "IsTM",
                schema: "nslds",
                table: "ClientRequest",
                nullable: false,
                defaultValue: false);
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
            migrationBuilder.DropColumn(name: "IsGrantReviewed", schema: "nslds", table: "ClientRequestStudent_History");
            migrationBuilder.DropColumn(name: "IsLoanReviewed", schema: "nslds", table: "ClientRequestStudent_History");
            migrationBuilder.DropColumn(name: "IsPellReviewed", schema: "nslds", table: "ClientRequestStudent_History");
            migrationBuilder.DropColumn(name: "IsRefreshed", schema: "nslds", table: "ClientRequestStudent_History");
            migrationBuilder.DropColumn(name: "IsTeachReviewed", schema: "nslds", table: "ClientRequestStudent_History");
            migrationBuilder.DropColumn(name: "IsGrantReviewed", schema: "nslds", table: "ClientRequestStudent");
            migrationBuilder.DropColumn(name: "IsLoanReviewed", schema: "nslds", table: "ClientRequestStudent");
            migrationBuilder.DropColumn(name: "IsPellReviewed", schema: "nslds", table: "ClientRequestStudent");
            migrationBuilder.DropColumn(name: "IsRefreshed", schema: "nslds", table: "ClientRequestStudent");
            migrationBuilder.DropColumn(name: "IsTeachReviewed", schema: "nslds", table: "ClientRequestStudent");
            migrationBuilder.DropColumn(name: "IsTM", schema: "nslds", table: "ClientRequest");
            migrationBuilder.AlterColumn<bool>(
                name: "IsOnHold",
                schema: "nslds",
                table: "ClientRequest",
                nullable: false);
            migrationBuilder.AlterColumn<bool>(
                name: "IsFailed",
                schema: "nslds",
                table: "ClientRequest",
                nullable: false);
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

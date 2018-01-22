using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace nslds.domain.Migrations
{
    public partial class security_audit_fix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // clean-up orphaned history table records before adding new FKs
            var sql1 = @"delete from nslds.clientprofile_history
where link2clientprofile not in (select id from nslds.clientprofile)";

            var sql2 = @"delete from nslds.clientrequeststudent_history 
where link2clientrequeststudent not in (select id from nslds.clientrequeststudent)";

            migrationBuilder.Sql(sql1);
            migrationBuilder.Sql(sql2);

            migrationBuilder.DropForeignKey(
                name: "FK_ClientRequest_ClientProfile_Link2ClientProfile",
                schema: "nslds",
                table: "ClientRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientRequestAlert_ClientRequest_Link2ClientRequest",
                schema: "nslds",
                table: "ClientRequestAlert");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientRequestStudent_ClientRequest_Link2ClientRequest",
                schema: "nslds",
                table: "ClientRequestStudent");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientRequestStudentAlert_ClientRequestStudent_Link2ClientRequestStudent",
                schema: "nslds",
                table: "ClientRequestStudentAlert");

            migrationBuilder.DropForeignKey(
                name: "FK_Job_ClientProfile_Link2ClientProfile",
                schema: "nslds",
                table: "Job");

            migrationBuilder.DropForeignKey(
                name: "FK_NsldsFAHType1_ClientRequestStudent_Link2ClientRequestStudent",
                schema: "nslds",
                table: "NsldsFAHType1");

            migrationBuilder.DropForeignKey(
                name: "FK_NsldsFAHType2_ClientRequestStudent_Link2ClientRequestStudent",
                schema: "nslds",
                table: "NsldsFAHType2");

            migrationBuilder.DropForeignKey(
                name: "FK_NsldsFAHType3_ClientRequestStudent_Link2ClientRequestStudent",
                schema: "nslds",
                table: "NsldsFAHType3");

            migrationBuilder.DropForeignKey(
                name: "FK_NsldsFAHType4_ClientRequestStudent_Link2ClientRequestStudent",
                schema: "nslds",
                table: "NsldsFAHType4");

            migrationBuilder.DropForeignKey(
                name: "FK_NsldsFAHType5_ClientRequestStudent_Link2ClientRequestStudent",
                schema: "nslds",
                table: "NsldsFAHType5");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RevOn",
                schema: "nslds",
                table: "Job",
                type: "datetime",
                nullable: false,
                defaultValueSql: "getdate()",
                oldClrType: typeof(DateTime),
                oldType: "datetime");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RevOn",
                schema: "nslds",
                table: "ClientRequest",
                type: "datetime",
                nullable: false,
                defaultValueSql: "getdate()",
                oldClrType: typeof(DateTime),
                oldType: "datetime");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RevOn",
                schema: "nslds",
                table: "ClientProfile",
                type: "datetime",
                nullable: false,
                defaultValueSql: "getdate()",
                oldClrType: typeof(DateTime),
                oldType: "datetime");

            migrationBuilder.CreateIndex(
                name: "IX_NsldsFAHType5_Link2ClientRequestStudent",
                schema: "nslds",
                table: "NsldsFAHType5",
                column: "Link2ClientRequestStudent");

            migrationBuilder.CreateIndex(
                name: "IX_NsldsFAHType4_Link2ClientRequestStudent",
                schema: "nslds",
                table: "NsldsFAHType4",
                column: "Link2ClientRequestStudent");

            migrationBuilder.CreateIndex(
                name: "IX_NsldsFAHType3_Link2ClientRequestStudent",
                schema: "nslds",
                table: "NsldsFAHType3",
                column: "Link2ClientRequestStudent");

            migrationBuilder.CreateIndex(
                name: "IX_NsldsFAHType2_Link2ClientRequestStudent",
                schema: "nslds",
                table: "NsldsFAHType2",
                column: "Link2ClientRequestStudent");

            migrationBuilder.CreateIndex(
                name: "IX_NsldsFAHType1_Link2ClientRequestStudent",
                schema: "nslds",
                table: "NsldsFAHType1",
                column: "Link2ClientRequestStudent");

            migrationBuilder.CreateIndex(
                name: "IX_Job_Link2ClientProfile",
                schema: "nslds",
                table: "Job",
                column: "Link2ClientProfile");

            migrationBuilder.CreateIndex(
                name: "IX_ClientRequestStudentAlert_Link2ClientRequestStudent",
                schema: "nslds",
                table: "ClientRequestStudentAlert",
                column: "Link2ClientRequestStudent");

            migrationBuilder.CreateIndex(
                name: "IX_ClientRequestStudent_History_Link2ClientRequestStudent",
                schema: "nslds",
                table: "ClientRequestStudent_History",
                column: "Link2ClientRequestStudent");

            migrationBuilder.CreateIndex(
                name: "IX_ClientRequestStudent_Link2ClientRequest",
                schema: "nslds",
                table: "ClientRequestStudent",
                column: "Link2ClientRequest");

            migrationBuilder.CreateIndex(
                name: "IX_ClientRequestAlert_Link2ClientRequest",
                schema: "nslds",
                table: "ClientRequestAlert",
                column: "Link2ClientRequest");

            migrationBuilder.CreateIndex(
                name: "IX_ClientRequest_Link2ClientProfile",
                schema: "nslds",
                table: "ClientRequest",
                column: "Link2ClientProfile");

            migrationBuilder.CreateIndex(
                name: "IX_ClientRequest_Link2Job",
                schema: "nslds",
                table: "ClientRequest",
                column: "Link2Job");

            migrationBuilder.CreateIndex(
                name: "IX_ClientProfile_History_Link2ClientProfile",
                schema: "nslds",
                table: "ClientProfile_History",
                column: "Link2ClientProfile");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientProfile_History_ClientProfile_Link2ClientProfile",
                schema: "nslds",
                table: "ClientProfile_History",
                column: "Link2ClientProfile",
                principalSchema: "nslds",
                principalTable: "ClientProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
                name: "FK_ClientRequestStudent_History_ClientRequestStudent_Link2ClientRequestStudent",
                schema: "nslds",
                table: "ClientRequestStudent_History",
                column: "Link2ClientRequestStudent",
                principalSchema: "nslds",
                principalTable: "ClientRequestStudent",
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
            migrationBuilder.DropForeignKey(
                name: "FK_ClientProfile_History_ClientProfile_Link2ClientProfile",
                schema: "nslds",
                table: "ClientProfile_History");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientRequest_ClientProfile_Link2ClientProfile",
                schema: "nslds",
                table: "ClientRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientRequestAlert_ClientRequest_Link2ClientRequest",
                schema: "nslds",
                table: "ClientRequestAlert");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientRequestStudent_ClientRequest_Link2ClientRequest",
                schema: "nslds",
                table: "ClientRequestStudent");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientRequestStudent_History_ClientRequestStudent_Link2ClientRequestStudent",
                schema: "nslds",
                table: "ClientRequestStudent_History");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientRequestStudentAlert_ClientRequestStudent_Link2ClientRequestStudent",
                schema: "nslds",
                table: "ClientRequestStudentAlert");

            migrationBuilder.DropForeignKey(
                name: "FK_Job_ClientProfile_Link2ClientProfile",
                schema: "nslds",
                table: "Job");

            migrationBuilder.DropForeignKey(
                name: "FK_NsldsFAHType1_ClientRequestStudent_Link2ClientRequestStudent",
                schema: "nslds",
                table: "NsldsFAHType1");

            migrationBuilder.DropForeignKey(
                name: "FK_NsldsFAHType2_ClientRequestStudent_Link2ClientRequestStudent",
                schema: "nslds",
                table: "NsldsFAHType2");

            migrationBuilder.DropForeignKey(
                name: "FK_NsldsFAHType3_ClientRequestStudent_Link2ClientRequestStudent",
                schema: "nslds",
                table: "NsldsFAHType3");

            migrationBuilder.DropForeignKey(
                name: "FK_NsldsFAHType4_ClientRequestStudent_Link2ClientRequestStudent",
                schema: "nslds",
                table: "NsldsFAHType4");

            migrationBuilder.DropForeignKey(
                name: "FK_NsldsFAHType5_ClientRequestStudent_Link2ClientRequestStudent",
                schema: "nslds",
                table: "NsldsFAHType5");

            migrationBuilder.DropIndex(
                name: "IX_NsldsFAHType5_Link2ClientRequestStudent",
                schema: "nslds",
                table: "NsldsFAHType5");

            migrationBuilder.DropIndex(
                name: "IX_NsldsFAHType4_Link2ClientRequestStudent",
                schema: "nslds",
                table: "NsldsFAHType4");

            migrationBuilder.DropIndex(
                name: "IX_NsldsFAHType3_Link2ClientRequestStudent",
                schema: "nslds",
                table: "NsldsFAHType3");

            migrationBuilder.DropIndex(
                name: "IX_NsldsFAHType2_Link2ClientRequestStudent",
                schema: "nslds",
                table: "NsldsFAHType2");

            migrationBuilder.DropIndex(
                name: "IX_NsldsFAHType1_Link2ClientRequestStudent",
                schema: "nslds",
                table: "NsldsFAHType1");

            migrationBuilder.DropIndex(
                name: "IX_Job_Link2ClientProfile",
                schema: "nslds",
                table: "Job");

            migrationBuilder.DropIndex(
                name: "IX_ClientRequestStudentAlert_Link2ClientRequestStudent",
                schema: "nslds",
                table: "ClientRequestStudentAlert");

            migrationBuilder.DropIndex(
                name: "IX_ClientRequestStudent_History_Link2ClientRequestStudent",
                schema: "nslds",
                table: "ClientRequestStudent_History");

            migrationBuilder.DropIndex(
                name: "IX_ClientRequestStudent_Link2ClientRequest",
                schema: "nslds",
                table: "ClientRequestStudent");

            migrationBuilder.DropIndex(
                name: "IX_ClientRequestAlert_Link2ClientRequest",
                schema: "nslds",
                table: "ClientRequestAlert");

            migrationBuilder.DropIndex(
                name: "IX_ClientRequest_Link2ClientProfile",
                schema: "nslds",
                table: "ClientRequest");

            migrationBuilder.DropIndex(
                name: "IX_ClientRequest_Link2Job",
                schema: "nslds",
                table: "ClientRequest");

            migrationBuilder.DropIndex(
                name: "IX_ClientProfile_History_Link2ClientProfile",
                schema: "nslds",
                table: "ClientProfile_History");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RevOn",
                schema: "nslds",
                table: "Job",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "getdate()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RevOn",
                schema: "nslds",
                table: "ClientRequest",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "getdate()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RevOn",
                schema: "nslds",
                table: "ClientProfile",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "getdate()");

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

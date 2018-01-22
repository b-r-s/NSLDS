using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace nslds.domain.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //return;
            migrationBuilder.EnsureSchema("nslds");
            migrationBuilder.CreateTable(
                name: "ClientProfile",
                schema: "nslds",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AY_Definition = table.Column<int>(nullable: false),
                    Address1 = table.Column<string>(nullable: false),
                    Address2 = table.Column<string>(nullable: true),
                    City = table.Column<string>(nullable: false),
                    Contact = table.Column<string>(nullable: false),
                    Email = table.Column<string>(nullable: false),
                    Exits_Counseling = table.Column<bool>(nullable: false),
                    Expiration = table.Column<int>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false, defaultValue: false),
                    IsPwdValid = table.Column<bool>(nullable: false, defaultValue: false),
                    Monitoring = table.Column<int>(nullable: false),
                    NSLDS_Upload = table.Column<bool>(nullable: false),
                    OPEID = table.Column<string>(nullable: false, maxLength: 8),
                    Organization_Name = table.Column<string>(nullable: false),
                    Phone = table.Column<string>(nullable: false),
                    Posting_Time = table.Column<TimeSpan>(type: "time(0)", nullable: true),
                    Retention = table.Column<int>(nullable: false),
                    RevBy = table.Column<string>(nullable: false, maxLength: 450),
                    RevOn = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "getdate()"),
                    SAIG = table.Column<string>(nullable: false, maxLength: 7),
                    State = table.Column<string>(nullable: false),
                    TD_Password = table.Column<string>(nullable: true),
                    Zip = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientProfile", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "ClientProfile_History",
                schema: "nslds",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AY_Definition = table.Column<int>(nullable: false),
                    Action = table.Column<string>(nullable: false, maxLength: 50),
                    ActionBy = table.Column<string>(nullable: false, maxLength: 450),
                    ActionOn = table.Column<DateTime>(type: "datetime", nullable: false),
                    Address1 = table.Column<string>(nullable: false),
                    Address2 = table.Column<string>(nullable: true),
                    City = table.Column<string>(nullable: false),
                    Contact = table.Column<string>(nullable: false),
                    Email = table.Column<string>(nullable: false),
                    Exits_Counseling = table.Column<bool>(nullable: false),
                    Expiration = table.Column<int>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsPwdValid = table.Column<bool>(nullable: false),
                    Link2ClientProfile = table.Column<int>(nullable: false),
                    Monitoring = table.Column<int>(nullable: false),
                    NSLDS_Upload = table.Column<bool>(nullable: false),
                    OPEID = table.Column<string>(nullable: false, maxLength: 8),
                    Organization_Name = table.Column<string>(nullable: false),
                    Phone = table.Column<string>(nullable: false),
                    Posting_Time = table.Column<TimeSpan>(type: "time(0)", nullable: true),
                    Retention = table.Column<int>(nullable: false),
                    RevBy = table.Column<string>(nullable: false, maxLength: 450),
                    RevOn = table.Column<DateTime>(type: "datetime", nullable: false),
                    SAIG = table.Column<string>(nullable: false, maxLength: 7),
                    State = table.Column<string>(nullable: false),
                    TD_Password = table.Column<string>(nullable: true),
                    Zip = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientProfile_History", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "ClientRequestStudent_History",
                schema: "nslds",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Action = table.Column<string>(nullable: false, maxLength: 50),
                    ActionBy = table.Column<string>(nullable: false, maxLength: 450),
                    ActionOn = table.Column<DateTime>(type: "datetime", nullable: false),
                    DOB = table.Column<DateTime>(type: "date", nullable: true),
                    DeleteMonitoring = table.Column<string>(type: "nchar(1)", nullable: true),
                    EncSSN = table.Column<string>(nullable: true, maxLength: 450),
                    EnrollBeginDate = table.Column<DateTime>(type: "date", nullable: true),
                    FirstName = table.Column<string>(nullable: true, maxLength: 100),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsReceived = table.Column<bool>(nullable: false),
                    IsResolved = table.Column<bool>(nullable: false),
                    IsSubmitted = table.Column<bool>(nullable: false),
                    IsValid = table.Column<bool>(nullable: false),
                    LastName = table.Column<string>(nullable: true, maxLength: 100),
                    Link2ClientRequest = table.Column<int>(nullable: false),
                    Link2ClientRequestStudent = table.Column<int>(nullable: false),
                    MonitorBeginDate = table.Column<DateTime>(type: "date", nullable: true),
                    ReceivedOn = table.Column<DateTime>(type: "datetime", nullable: true),
                    RequestType = table.Column<string>(type: "nchar(1)", nullable: true),
                    Response = table.Column<string>(nullable: true),
                    RevBy = table.Column<string>(nullable: false, maxLength: 450),
                    RevOn = table.Column<DateTime>(type: "datetime", nullable: false),
                    SID = table.Column<string>(nullable: true, maxLength: 50),
                    StartDate = table.Column<DateTime>(type: "date", nullable: true),
                    SubmittedOn = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientRequestStudent_History", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "Job",
                schema: "nslds",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BilledOn = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsBilled = table.Column<bool>(nullable: false, defaultValue: false),
                    IsDeleted = table.Column<bool>(nullable: false, defaultValue: false),
                    IsReceived = table.Column<bool>(nullable: false, defaultValue: false),
                    IsSubmitted = table.Column<bool>(nullable: false, defaultValue: false),
                    JobDate = table.Column<DateTime>(type: "date", nullable: false),
                    Link2ClientProfile = table.Column<int>(nullable: false),
                    ReceivedOn = table.Column<DateTime>(type: "datetime", nullable: true),
                    Response = table.Column<string>(nullable: true),
                    RevBy = table.Column<string>(nullable: false, maxLength: 450),
                    RevOn = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "getdate()"),
                    SubmittedOn = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Job", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Job_ClientProfile_Link2ClientProfile",
                        column: x => x.Link2ClientProfile,
                        principalSchema: "nslds",
                        principalTable: "ClientProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "ClientRequest",
                schema: "nslds",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<bool>(nullable: false, defaultValue: false),
                    IsFailed = table.Column<bool>(nullable: false),
                    IsOnHold = table.Column<bool>(nullable: false),
                    IsReceived = table.Column<bool>(nullable: false, defaultValue: false),
                    IsSubmitted = table.Column<bool>(nullable: false, defaultValue: false),
                    Link2ClientProfile = table.Column<int>(nullable: false),
                    Link2Job = table.Column<int>(nullable: true),
                    ReceivedOn = table.Column<DateTime>(type: "datetime", nullable: true),
                    Response = table.Column<string>(nullable: true),
                    RevBy = table.Column<string>(nullable: false, maxLength: 450),
                    RevOn = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "getdate()"),
                    Sequence = table.Column<short>(nullable: false),
                    SubmittedOn = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientRequest_ClientProfile_Link2ClientProfile",
                        column: x => x.Link2ClientProfile,
                        principalSchema: "nslds",
                        principalTable: "ClientProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientRequest_Job_Link2Job",
                        column: x => x.Link2Job,
                        principalSchema: "nslds",
                        principalTable: "Job",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.CreateTable(
                name: "ClientRequestAlert",
                schema: "nslds",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ErrorCode = table.Column<int>(nullable: true),
                    ErrorCount = table.Column<int>(nullable: false),
                    ErrorLevel = table.Column<int>(nullable: true),
                    ErrorMessage = table.Column<string>(nullable: true),
                    ErrorRecCount = table.Column<int>(nullable: false),
                    FieldInError = table.Column<int>(nullable: true),
                    FieldName = table.Column<string>(nullable: true),
                    FieldValue = table.Column<string>(nullable: true),
                    Link2ClientRequest = table.Column<int>(nullable: false),
                    RecordCount = table.Column<int>(nullable: false),
                    WarningCount = table.Column<int>(nullable: false),
                    WarningRecCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientRequestAlert", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientRequestAlert_ClientRequest_Link2ClientRequest",
                        column: x => x.Link2ClientRequest,
                        principalSchema: "nslds",
                        principalTable: "ClientRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "ClientRequestStudent",
                schema: "nslds",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DOB = table.Column<DateTime>(type: "date", nullable: true),
                    DeleteMonitoring = table.Column<string>(type: "nchar(1)", nullable: true),
                    EncSSN = table.Column<string>(nullable: true, maxLength: 450),
                    EnrollBeginDate = table.Column<DateTime>(type: "date", nullable: true),
                    FirstName = table.Column<string>(nullable: true, maxLength: 100),
                    IsDeleted = table.Column<bool>(nullable: false, defaultValue: false),
                    IsReceived = table.Column<bool>(nullable: false, defaultValue: false),
                    IsResolved = table.Column<bool>(nullable: false),
                    IsSubmitted = table.Column<bool>(nullable: false, defaultValue: false),
                    IsValid = table.Column<bool>(nullable: false, defaultValue: false),
                    LastName = table.Column<string>(nullable: true, maxLength: 100),
                    Link2ClientRequest = table.Column<int>(nullable: false),
                    MonitorBeginDate = table.Column<DateTime>(type: "date", nullable: true),
                    ReceivedOn = table.Column<DateTime>(type: "datetime", nullable: true),
                    RequestType = table.Column<string>(type: "nchar(1)", nullable: true),
                    Response = table.Column<string>(nullable: true),
                    RevBy = table.Column<string>(nullable: false, maxLength: 450),
                    RevOn = table.Column<DateTime>(type: "datetime", nullable: false),
                    SID = table.Column<string>(nullable: true, maxLength: 50),
                    StartDate = table.Column<DateTime>(type: "date", nullable: true),
                    SubmittedOn = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientRequestStudent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientRequestStudent_ClientRequest_Link2ClientRequest",
                        column: x => x.Link2ClientRequest,
                        principalSchema: "nslds",
                        principalTable: "ClientRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "ClientRequestStudentAlert",
                schema: "nslds",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    EncFieldValue = table.Column<string>(nullable: true),
                    ErrorCode = table.Column<int>(nullable: false),
                    ErrorLevel = table.Column<int>(nullable: false),
                    ErrorMessage = table.Column<string>(nullable: true),
                    FieldInError = table.Column<int>(nullable: false),
                    FieldName = table.Column<string>(nullable: true),
                    Link2ClientRequestStudent = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientRequestStudentAlert", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientRequestStudentAlert_ClientRequestStudent_Link2ClientRequestStudent",
                        column: x => x.Link2ClientRequestStudent,
                        principalSchema: "nslds",
                        principalTable: "ClientRequestStudent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "NsldsFAHType1",
                schema: "nslds",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ACGChange = table.Column<bool>(nullable: true),
                    ActiveBankruptcy = table.Column<bool>(nullable: false),
                    AggregateChange = table.Column<bool>(nullable: true),
                    CombinedPendingDisb = table.Column<decimal>(nullable: true),
                    CombinedPrincipalBal = table.Column<decimal>(nullable: true),
                    CombinedTotal = table.Column<decimal>(nullable: true),
                    DOB = table.Column<DateTime>(nullable: false),
                    DefaultedLoan = table.Column<bool>(nullable: false),
                    DirectLoanPlusGradMPN = table.Column<string>(nullable: true, maxLength: 1),
                    DirectLoanPlusMPN = table.Column<string>(nullable: true, maxLength: 1),
                    DirectStaffordMPN = table.Column<string>(nullable: true, maxLength: 1),
                    DischargedLoanCode = table.Column<string>(nullable: true, maxLength: 1),
                    FirstName = table.Column<string>(nullable: true, maxLength: 12),
                    Fraud = table.Column<bool>(nullable: false),
                    GradAY = table.Column<string>(nullable: true, maxLength: 4),
                    GradCombinedLoanLimit = table.Column<string>(nullable: true, maxLength: 1),
                    GradCombinedPendingDisb = table.Column<decimal>(nullable: true),
                    GradCombinedPrincipalBal = table.Column<decimal>(nullable: true),
                    GradCombinedTotal = table.Column<decimal>(nullable: true),
                    GradDependency = table.Column<string>(nullable: true, maxLength: 1),
                    GradEligibilityUsed = table.Column<decimal>(nullable: true),
                    GradRemainingAmount = table.Column<decimal>(nullable: true),
                    GradSubLoanLimit = table.Column<string>(nullable: true, maxLength: 1),
                    GradSubPendingDisb = table.Column<decimal>(nullable: true),
                    GradSubPrincipalBal = table.Column<decimal>(nullable: true),
                    GradSubTotal = table.Column<decimal>(nullable: true),
                    GradTeachTotalDisb = table.Column<decimal>(nullable: true),
                    GradUnallocPrincipalBal = table.Column<decimal>(nullable: true),
                    GradUnallocTotal = table.Column<decimal>(nullable: true),
                    GradUnsubPendingDisb = table.Column<decimal>(nullable: true),
                    GradUnsubPrincipalBal = table.Column<decimal>(nullable: true),
                    GradUnsubTotal = table.Column<decimal>(nullable: true),
                    LastName = table.Column<string>(nullable: true, maxLength: 35),
                    LifeEligibilityUsed = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    Link2ClientRequestStudent = table.Column<int>(nullable: false),
                    LoanChange = table.Column<bool>(nullable: true),
                    PellChange = table.Column<bool>(nullable: true),
                    PellLifeLimit = table.Column<string>(nullable: true, maxLength: 1),
                    PerkinsCurrentAYDisb = table.Column<decimal>(nullable: true),
                    PerkinsTotalPrincipalBal = table.Column<decimal>(nullable: true),
                    PlusLoanPrincipalBal = table.Column<decimal>(nullable: true),
                    PlusLoanTotal = table.Column<decimal>(nullable: true),
                    PlusProPrincipalBal = table.Column<decimal>(nullable: true),
                    PlusProTotal = table.Column<decimal>(nullable: true),
                    SMARTChange = table.Column<bool>(nullable: true),
                    SatisfactoryRepayLoan = table.Column<bool>(nullable: false),
                    SubPendingDisb = table.Column<decimal>(nullable: true),
                    SubPrincipalBal = table.Column<decimal>(nullable: true),
                    SubTotal = table.Column<decimal>(nullable: true),
                    SubUsagePeriod = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    SulaFlag = table.Column<bool>(nullable: true),
                    TeachGrantChange = table.Column<bool>(nullable: true),
                    TeachGrantConverted = table.Column<bool>(nullable: false),
                    TeachGrantDataChange = table.Column<bool>(nullable: true),
                    TeachLoanChange = table.Column<bool>(nullable: true),
                    TeachLoanPrincipalBal = table.Column<decimal>(nullable: true),
                    TeachLoanTotal = table.Column<decimal>(nullable: true),
                    UnAllocPrincipalBal = table.Column<decimal>(nullable: true),
                    UnAllocPrincipalTotal = table.Column<decimal>(nullable: true),
                    UnSubPendingDisb = table.Column<decimal>(nullable: true),
                    UnSubPrincipalBal = table.Column<decimal>(nullable: true),
                    UnSubTotal = table.Column<decimal>(nullable: true),
                    UnderGradCombinedLoanLimit = table.Column<string>(nullable: true, maxLength: 1),
                    UnderGradSubLoanLimit = table.Column<string>(nullable: true, maxLength: 1),
                    UndergradAY = table.Column<string>(nullable: true, maxLength: 4),
                    UndergradCombinedPendingDisb = table.Column<decimal>(nullable: true),
                    UndergradCombinedPrincipalBal = table.Column<decimal>(nullable: true),
                    UndergradCombinedTotal = table.Column<decimal>(nullable: true),
                    UndergradDependency = table.Column<string>(nullable: true, maxLength: 1),
                    UndergradEligibilityUsed = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    UndergradRemainingAmount = table.Column<decimal>(nullable: true),
                    UndergradSubPendingDisb = table.Column<decimal>(nullable: true),
                    UndergradSubPrincipalBal = table.Column<decimal>(nullable: true),
                    UndergradSubTotal = table.Column<decimal>(nullable: true),
                    UndergradTeachDisbTotal = table.Column<decimal>(nullable: true),
                    UndergradUnallocPrincipalBal = table.Column<decimal>(nullable: true),
                    UndergradUnallocTotal = table.Column<decimal>(nullable: true),
                    UndergradUnsubPendingDisb = table.Column<decimal>(nullable: true),
                    UndergradUnsubPrincipalBal = table.Column<decimal>(nullable: true),
                    UndergradUnsubTotal = table.Column<decimal>(nullable: true),
                    UnusualEnrollHistory = table.Column<string>(nullable: true, maxLength: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NsldsFAHType1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NsldsFAHType1_ClientRequestStudent_Link2ClientRequestStudent",
                        column: x => x.Link2ClientRequestStudent,
                        principalSchema: "nslds",
                        principalTable: "ClientRequestStudent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "NsldsFAHType2",
                schema: "nslds",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CurrentDOB = table.Column<DateTime>(type: "date", nullable: false),
                    CurrentFirstName = table.Column<string>(nullable: true, maxLength: 12),
                    CurrentLastName = table.Column<string>(nullable: true, maxLength: 35),
                    FirstNameHistory = table.Column<string>(nullable: true, maxLength: 12),
                    LastNameHistory = table.Column<string>(nullable: true, maxLength: 35),
                    Link2ClientRequestStudent = table.Column<int>(nullable: false),
                    MiddleInitialHistory = table.Column<string>(nullable: true, maxLength: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NsldsFAHType2", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NsldsFAHType2_ClientRequestStudent_Link2ClientRequestStudent",
                        column: x => x.Link2ClientRequestStudent,
                        principalSchema: "nslds",
                        principalTable: "ClientRequestStudent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "NsldsFAHType3",
                schema: "nslds",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    GrantContact = table.Column<string>(nullable: true, maxLength: 8),
                    Link2ClientRequestStudent = table.Column<int>(nullable: false),
                    OverDisbAY = table.Column<string>(nullable: true, maxLength: 4),
                    Overpayment = table.Column<string>(nullable: true, maxLength: 1),
                    OverpaymentType = table.Column<string>(nullable: true, maxLength: 2)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NsldsFAHType3", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NsldsFAHType3_ClientRequestStudent_Link2ClientRequestStudent",
                        column: x => x.Link2ClientRequestStudent,
                        principalSchema: "nslds",
                        principalTable: "ClientRequestStudent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "NsldsFAHType4",
                schema: "nslds",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AYLevel = table.Column<string>(nullable: true, maxLength: 1),
                    AcceptedVerification = table.Column<string>(nullable: true, maxLength: 3),
                    AddEligibility = table.Column<bool>(nullable: true),
                    AwardAmount = table.Column<decimal>(nullable: true),
                    AwardId = table.Column<string>(nullable: true, maxLength: 21),
                    AwardYear = table.Column<string>(nullable: true, maxLength: 4),
                    CIPCode = table.Column<string>(nullable: true, maxLength: 7),
                    CODPostedDate = table.Column<DateTime>(nullable: true),
                    DisbAmount = table.Column<decimal>(nullable: true),
                    EligibilityCode = table.Column<string>(nullable: true, maxLength: 2),
                    EligibilityUsed = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    FamilyContribution = table.Column<decimal>(nullable: true),
                    FirstTimePell = table.Column<bool>(nullable: true),
                    GrantChangeFlag = table.Column<bool>(nullable: true),
                    GrantSchoolCode = table.Column<string>(nullable: true, maxLength: 8),
                    GrantSequence = table.Column<short>(nullable: true),
                    GrantType = table.Column<string>(nullable: true, maxLength: 2),
                    HSProgramCode = table.Column<string>(nullable: true, maxLength: 6),
                    LastDisbDate = table.Column<DateTime>(nullable: true),
                    Link2ClientRequestStudent = table.Column<int>(nullable: false),
                    Post911DeceasedDep = table.Column<bool>(nullable: true),
                    SchedAmount = table.Column<decimal>(nullable: true),
                    TEACHConversionDate = table.Column<DateTime>(nullable: true),
                    TEACHConverted = table.Column<bool>(nullable: true),
                    TotalEligibilityUsed = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    TransNumber = table.Column<string>(nullable: true, maxLength: 2)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NsldsFAHType4", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NsldsFAHType4_ClientRequestStudent_Link2ClientRequestStudent",
                        column: x => x.Link2ClientRequestStudent,
                        principalSchema: "nslds",
                        principalTable: "ClientRequestStudent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "NsldsFAHType5",
                schema: "nslds",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AYBeginDate = table.Column<DateTime>(nullable: true),
                    AYEndDate = table.Column<DateTime>(nullable: true),
                    AcademicLevel = table.Column<string>(nullable: true, maxLength: 1),
                    AdditionalUnsubLoan = table.Column<string>(nullable: true, maxLength: 1),
                    CalcCombinedAmount = table.Column<decimal>(nullable: true),
                    CalcSubAmount = table.Column<decimal>(nullable: true),
                    CalcUnsubAmount = table.Column<decimal>(nullable: true),
                    CapitalInterest = table.Column<bool>(nullable: true),
                    ConfirmSubDate = table.Column<DateTime>(nullable: true),
                    ConfirmSubStatus = table.Column<string>(nullable: true, maxLength: 1),
                    ContactType = table.Column<string>(nullable: true, maxLength: 3),
                    CurrentGACode = table.Column<string>(nullable: true, maxLength: 3),
                    DataProviderID = table.Column<string>(nullable: true, maxLength: 21),
                    LastDisbAmount = table.Column<decimal>(nullable: true),
                    LastLoanDisbDate = table.Column<DateTime>(nullable: true),
                    LenderCode = table.Column<string>(nullable: true, maxLength: 6),
                    LenderServicer = table.Column<string>(nullable: true, maxLength: 6),
                    Link2ClientRequestStudent = table.Column<int>(nullable: false),
                    LoanAmount = table.Column<decimal>(nullable: true),
                    LoanContact = table.Column<string>(nullable: true, maxLength: 8),
                    LoanDate = table.Column<DateTime>(nullable: true),
                    LoanPeriodBeginDate = table.Column<DateTime>(nullable: true),
                    LoanPeriodEndDate = table.Column<DateTime>(nullable: true),
                    LoanRecChange = table.Column<bool>(nullable: true),
                    LoanSchoolCode = table.Column<string>(nullable: true, maxLength: 8),
                    LoanStatusCode = table.Column<string>(nullable: true, maxLength: 2),
                    LoanStatusDate = table.Column<DateTime>(nullable: false),
                    LoanTypeCode = table.Column<string>(nullable: true, maxLength: 2),
                    NetLoanAmount = table.Column<decimal>(nullable: true),
                    PerkinsCancelCode = table.Column<string>(nullable: true, maxLength: 3),
                    PrincipalBal = table.Column<decimal>(nullable: true),
                    PrincipalDate = table.Column<DateTime>(nullable: true),
                    Reaffirmation = table.Column<bool>(nullable: true),
                    TotalDisb = table.Column<decimal>(nullable: true),
                    UnallocatedAmount = table.Column<decimal>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NsldsFAHType5", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NsldsFAHType5_ClientRequestStudent_Link2ClientRequestStudent",
                        column: x => x.Link2ClientRequestStudent,
                        principalSchema: "nslds",
                        principalTable: "ClientRequestStudent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateIndex(
                name: "IX_ClientProfile_OPEID",
                schema: "nslds",
                table: "ClientProfile",
                column: "OPEID");
            migrationBuilder.CreateIndex(
                name: "IX_ClientRequest_RevOn",
                schema: "nslds",
                table: "ClientRequest",
                column: "RevOn");
            migrationBuilder.CreateIndex(
                name: "IX_ClientRequestStudent_DOB",
                schema: "nslds",
                table: "ClientRequestStudent",
                column: "DOB");
            migrationBuilder.CreateIndex(
                name: "IX_ClientRequestStudent_EncSSN",
                schema: "nslds",
                table: "ClientRequestStudent",
                column: "EncSSN");
            migrationBuilder.CreateIndex(
                name: "IX_ClientRequestStudent_FirstName",
                schema: "nslds",
                table: "ClientRequestStudent",
                column: "FirstName");
            migrationBuilder.CreateIndex(
                name: "IX_ClientRequestStudent_IsReceived",
                schema: "nslds",
                table: "ClientRequestStudent",
                column: "IsReceived");
            migrationBuilder.CreateIndex(
                name: "IX_ClientRequestStudent_LastName",
                schema: "nslds",
                table: "ClientRequestStudent",
                column: "LastName");
            migrationBuilder.CreateIndex(
                name: "IX_ClientRequestStudent_RevOn",
                schema: "nslds",
                table: "ClientRequestStudent",
                column: "RevOn");
            migrationBuilder.CreateIndex(
                name: "IX_ClientRequestStudent_StartDate",
                schema: "nslds",
                table: "ClientRequestStudent",
                column: "StartDate");
            migrationBuilder.CreateIndex(
                name: "IX_NsldsFAHType1_SulaFlag",
                schema: "nslds",
                table: "NsldsFAHType1",
                column: "SulaFlag");
            migrationBuilder.CreateIndex(
                name: "IX_NsldsFAHType5_LoanStatusCode",
                schema: "nslds",
                table: "NsldsFAHType5",
                column: "LoanStatusCode");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ClientProfile_History", schema: "nslds");
            migrationBuilder.DropTable(name: "ClientRequestAlert", schema: "nslds");
            migrationBuilder.DropTable(name: "ClientRequestStudent_History", schema: "nslds");
            migrationBuilder.DropTable(name: "ClientRequestStudentAlert", schema: "nslds");
            migrationBuilder.DropTable(name: "NsldsFAHType1", schema: "nslds");
            migrationBuilder.DropTable(name: "NsldsFAHType2", schema: "nslds");
            migrationBuilder.DropTable(name: "NsldsFAHType3", schema: "nslds");
            migrationBuilder.DropTable(name: "NsldsFAHType4", schema: "nslds");
            migrationBuilder.DropTable(name: "NsldsFAHType5", schema: "nslds");
            migrationBuilder.DropTable(name: "ClientRequestStudent", schema: "nslds");
            migrationBuilder.DropTable(name: "ClientRequest", schema: "nslds");
            migrationBuilder.DropTable(name: "Job", schema: "nslds");
            migrationBuilder.DropTable(name: "ClientProfile", schema: "nslds");
        }
    }
}

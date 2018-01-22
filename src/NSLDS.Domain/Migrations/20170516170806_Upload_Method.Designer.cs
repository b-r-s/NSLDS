using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using NSLDS.Domain;

namespace nslds.domain.Migrations
{
    [DbContext(typeof(NSLDS_Context))]
    [Migration("20170516170806_Upload_Method")]
    partial class Upload_Method
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0-rc1-16348")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("NSLDS.Domain.ClientProfile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AY_Definition");

                    b.Property<string>("Address1")
                        .IsRequired();

                    b.Property<string>("Address2");

                    b.Property<string>("City")
                        .IsRequired();

                    b.Property<string>("Contact")
                        .IsRequired();

                    b.Property<string>("Email")
                        .IsRequired();

                    b.Property<bool>("Exits_Counseling");

                    b.Property<int>("Expiration");

                    b.Property<bool>("IsDeleted")
                        .HasAnnotation("Relational:DefaultValue", "False")
                        .HasAnnotation("Relational:DefaultValueType", "System.Boolean");

                    b.Property<bool>("IsPwdValid")
                        .HasAnnotation("Relational:DefaultValue", "False")
                        .HasAnnotation("Relational:DefaultValueType", "System.Boolean");

                    b.Property<int>("Monitoring");

                    b.Property<bool>("NSLDS_Upload");

                    b.Property<string>("OPEID")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 8);

                    b.Property<string>("Organization_Name")
                        .IsRequired();

                    b.Property<string>("Phone")
                        .IsRequired();

                    b.Property<TimeSpan?>("Posting_Time")
                        .HasAnnotation("Relational:ColumnType", "time(0)");

                    b.Property<int>("Retention");

                    b.Property<string>("RevBy")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 450);

                    b.Property<DateTime>("RevOn")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("Relational:ColumnType", "datetime")
                        .HasAnnotation("Relational:GeneratedValueSql", "getdate()");

                    b.Property<string>("SAIG")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 7);

                    b.Property<string>("State")
                        .IsRequired();

                    b.Property<string>("TD_Password");

                    b.Property<string>("Upload_Method")
                        .HasAnnotation("MaxLength", 1);

                    b.Property<string>("Zip")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("OPEID");

                    b.HasAnnotation("Relational:Schema", "nslds");

                    b.HasAnnotation("Relational:TableName", "ClientProfile");
                });

            modelBuilder.Entity("NSLDS.Domain.ClientProfile_History", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AY_Definition");

                    b.Property<string>("Action")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 50);

                    b.Property<string>("ActionBy")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 450);

                    b.Property<DateTime>("ActionOn")
                        .HasAnnotation("Relational:ColumnType", "datetime");

                    b.Property<string>("Address1")
                        .IsRequired();

                    b.Property<string>("Address2");

                    b.Property<string>("City")
                        .IsRequired();

                    b.Property<string>("Contact")
                        .IsRequired();

                    b.Property<string>("Email")
                        .IsRequired();

                    b.Property<bool>("Exits_Counseling");

                    b.Property<int>("Expiration");

                    b.Property<bool>("IsDeleted");

                    b.Property<bool>("IsPwdValid");

                    b.Property<int>("Link2ClientProfile");

                    b.Property<int>("Monitoring");

                    b.Property<bool>("NSLDS_Upload");

                    b.Property<string>("OPEID")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 8);

                    b.Property<string>("Organization_Name")
                        .IsRequired();

                    b.Property<string>("Phone")
                        .IsRequired();

                    b.Property<TimeSpan?>("Posting_Time")
                        .HasAnnotation("Relational:ColumnType", "time(0)");

                    b.Property<int>("Retention");

                    b.Property<string>("RevBy")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 450);

                    b.Property<DateTime>("RevOn")
                        .HasAnnotation("Relational:ColumnType", "datetime");

                    b.Property<string>("SAIG")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 7);

                    b.Property<string>("State")
                        .IsRequired();

                    b.Property<string>("TD_Password");

                    b.Property<string>("Upload_Method")
                        .HasAnnotation("MaxLength", 1);

                    b.Property<string>("Zip")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasAnnotation("Relational:Schema", "nslds");

                    b.HasAnnotation("Relational:TableName", "ClientProfile_History");
                });

            modelBuilder.Entity("NSLDS.Domain.ClientRequest", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("IsDeleted")
                        .HasAnnotation("Relational:DefaultValue", "False")
                        .HasAnnotation("Relational:DefaultValueType", "System.Boolean");

                    b.Property<bool>("IsFailed");

                    b.Property<bool>("IsOnHold");

                    b.Property<bool>("IsReceived")
                        .HasAnnotation("Relational:DefaultValue", "False")
                        .HasAnnotation("Relational:DefaultValueType", "System.Boolean");

                    b.Property<bool>("IsSubmitted")
                        .HasAnnotation("Relational:DefaultValue", "False")
                        .HasAnnotation("Relational:DefaultValueType", "System.Boolean");

                    b.Property<int>("Link2ClientProfile");

                    b.Property<int?>("Link2Job");

                    b.Property<DateTime?>("ReceivedOn")
                        .HasAnnotation("Relational:ColumnType", "datetime");

                    b.Property<string>("Response");

                    b.Property<string>("RevBy")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 450);

                    b.Property<DateTime>("RevOn")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("Relational:ColumnType", "datetime")
                        .HasAnnotation("Relational:GeneratedValueSql", "getdate()");

                    b.Property<short>("Sequence");

                    b.Property<DateTime?>("SubmittedOn")
                        .HasAnnotation("Relational:ColumnType", "datetime");

                    b.HasKey("Id");

                    b.HasIndex("RevOn");

                    b.HasAnnotation("Relational:Schema", "nslds");

                    b.HasAnnotation("Relational:TableName", "ClientRequest");
                });

            modelBuilder.Entity("NSLDS.Domain.ClientRequestAlert", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("ErrorCode");

                    b.Property<int>("ErrorCount");

                    b.Property<int?>("ErrorLevel");

                    b.Property<string>("ErrorMessage");

                    b.Property<int>("ErrorRecCount");

                    b.Property<int?>("FieldInError");

                    b.Property<string>("FieldName");

                    b.Property<string>("FieldValue");

                    b.Property<int>("Link2ClientRequest");

                    b.Property<int>("RecordCount");

                    b.Property<int>("WarningCount");

                    b.Property<int>("WarningRecCount");

                    b.HasKey("Id");

                    b.HasAnnotation("Relational:Schema", "nslds");

                    b.HasAnnotation("Relational:TableName", "ClientRequestAlert");
                });

            modelBuilder.Entity("NSLDS.Domain.ClientRequestStudent", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime?>("DOB")
                        .HasAnnotation("Relational:ColumnType", "date");

                    b.Property<string>("DeleteMonitoring")
                        .HasAnnotation("MaxLength", 1)
                        .HasAnnotation("Relational:ColumnType", "nchar(1)");

                    b.Property<string>("EncSSN")
                        .HasAnnotation("MaxLength", 450);

                    b.Property<DateTime?>("EnrollBeginDate")
                        .HasAnnotation("Relational:ColumnType", "date");

                    b.Property<string>("FirstName")
                        .HasAnnotation("MaxLength", 100);

                    b.Property<bool>("IsDeleted")
                        .HasAnnotation("Relational:DefaultValue", "False")
                        .HasAnnotation("Relational:DefaultValueType", "System.Boolean");

                    b.Property<bool>("IsReceived")
                        .HasAnnotation("Relational:DefaultValue", "False")
                        .HasAnnotation("Relational:DefaultValueType", "System.Boolean");

                    b.Property<bool>("IsResolved");

                    b.Property<bool>("IsSubmitted")
                        .HasAnnotation("Relational:DefaultValue", "False")
                        .HasAnnotation("Relational:DefaultValueType", "System.Boolean");

                    b.Property<bool>("IsValid")
                        .HasAnnotation("Relational:DefaultValue", "False")
                        .HasAnnotation("Relational:DefaultValueType", "System.Boolean");

                    b.Property<string>("LastName")
                        .HasAnnotation("MaxLength", 100);

                    b.Property<int>("Link2ClientRequest");

                    b.Property<DateTime?>("MonitorBeginDate")
                        .HasAnnotation("Relational:ColumnType", "date");

                    b.Property<DateTime?>("ReceivedOn")
                        .HasAnnotation("Relational:ColumnType", "datetime");

                    b.Property<string>("RequestType")
                        .HasAnnotation("MaxLength", 1)
                        .HasAnnotation("Relational:ColumnType", "nchar(1)");

                    b.Property<string>("Response");

                    b.Property<string>("RevBy")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 450);

                    b.Property<DateTime>("RevOn")
                        .HasAnnotation("Relational:ColumnType", "datetime");

                    b.Property<string>("SID")
                        .HasAnnotation("MaxLength", 50);

                    b.Property<DateTime?>("StartDate")
                        .HasAnnotation("Relational:ColumnType", "date");

                    b.Property<DateTime?>("SubmittedOn")
                        .HasAnnotation("Relational:ColumnType", "datetime");

                    b.HasKey("Id");

                    b.HasIndex("DOB");

                    b.HasIndex("EncSSN");

                    b.HasIndex("FirstName");

                    b.HasIndex("IsReceived");

                    b.HasIndex("LastName");

                    b.HasIndex("RevOn");

                    b.HasIndex("StartDate");

                    b.HasAnnotation("Relational:Schema", "nslds");

                    b.HasAnnotation("Relational:TableName", "ClientRequestStudent");
                });

            modelBuilder.Entity("NSLDS.Domain.ClientRequestStudent_History", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Action")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 50);

                    b.Property<string>("ActionBy")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 450);

                    b.Property<DateTime>("ActionOn")
                        .HasAnnotation("Relational:ColumnType", "datetime");

                    b.Property<DateTime?>("DOB")
                        .HasAnnotation("Relational:ColumnType", "date");

                    b.Property<string>("DeleteMonitoring")
                        .HasAnnotation("MaxLength", 1)
                        .HasAnnotation("Relational:ColumnType", "nchar(1)");

                    b.Property<string>("EncSSN")
                        .HasAnnotation("MaxLength", 450);

                    b.Property<DateTime?>("EnrollBeginDate")
                        .HasAnnotation("Relational:ColumnType", "date");

                    b.Property<string>("FirstName")
                        .HasAnnotation("MaxLength", 100);

                    b.Property<bool>("IsDeleted");

                    b.Property<bool>("IsReceived");

                    b.Property<bool>("IsResolved");

                    b.Property<bool>("IsSubmitted");

                    b.Property<bool>("IsValid");

                    b.Property<string>("LastName")
                        .HasAnnotation("MaxLength", 100);

                    b.Property<int>("Link2ClientRequest");

                    b.Property<int>("Link2ClientRequestStudent");

                    b.Property<DateTime?>("MonitorBeginDate")
                        .HasAnnotation("Relational:ColumnType", "date");

                    b.Property<DateTime?>("ReceivedOn")
                        .HasAnnotation("Relational:ColumnType", "datetime");

                    b.Property<string>("RequestType")
                        .HasAnnotation("MaxLength", 1)
                        .HasAnnotation("Relational:ColumnType", "nchar(1)");

                    b.Property<string>("Response");

                    b.Property<string>("RevBy")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 450);

                    b.Property<DateTime>("RevOn")
                        .HasAnnotation("Relational:ColumnType", "datetime");

                    b.Property<string>("SID")
                        .HasAnnotation("MaxLength", 50);

                    b.Property<DateTime?>("StartDate")
                        .HasAnnotation("Relational:ColumnType", "date");

                    b.Property<DateTime?>("SubmittedOn")
                        .HasAnnotation("Relational:ColumnType", "datetime");

                    b.HasKey("Id");

                    b.HasAnnotation("Relational:Schema", "nslds");

                    b.HasAnnotation("Relational:TableName", "ClientRequestStudent_History");
                });

            modelBuilder.Entity("NSLDS.Domain.ClientRequestStudentAlert", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("EncFieldValue");

                    b.Property<int>("ErrorCode");

                    b.Property<int>("ErrorLevel");

                    b.Property<string>("ErrorMessage");

                    b.Property<int>("FieldInError");

                    b.Property<string>("FieldName");

                    b.Property<int>("Link2ClientRequestStudent");

                    b.HasKey("Id");

                    b.HasAnnotation("Relational:Schema", "nslds");

                    b.HasAnnotation("Relational:TableName", "ClientRequestStudentAlert");
                });

            modelBuilder.Entity("NSLDS.Domain.Job", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime?>("BilledOn")
                        .HasAnnotation("Relational:ColumnType", "datetime");

                    b.Property<bool>("IsBilled")
                        .HasAnnotation("Relational:DefaultValue", "False")
                        .HasAnnotation("Relational:DefaultValueType", "System.Boolean");

                    b.Property<bool>("IsDeleted")
                        .HasAnnotation("Relational:DefaultValue", "False")
                        .HasAnnotation("Relational:DefaultValueType", "System.Boolean");

                    b.Property<bool>("IsReceived")
                        .HasAnnotation("Relational:DefaultValue", "False")
                        .HasAnnotation("Relational:DefaultValueType", "System.Boolean");

                    b.Property<bool>("IsSubmitted")
                        .HasAnnotation("Relational:DefaultValue", "False")
                        .HasAnnotation("Relational:DefaultValueType", "System.Boolean");

                    b.Property<DateTime>("JobDate")
                        .HasAnnotation("Relational:ColumnType", "date");

                    b.Property<int>("Link2ClientProfile");

                    b.Property<DateTime?>("ReceivedOn")
                        .HasAnnotation("Relational:ColumnType", "datetime");

                    b.Property<string>("Response");

                    b.Property<string>("RevBy")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 450);

                    b.Property<DateTime>("RevOn")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("Relational:ColumnType", "datetime")
                        .HasAnnotation("Relational:GeneratedValueSql", "getdate()");

                    b.Property<DateTime?>("SubmittedOn")
                        .HasAnnotation("Relational:ColumnType", "datetime");

                    b.HasKey("Id");

                    b.HasAnnotation("Relational:Schema", "nslds");

                    b.HasAnnotation("Relational:TableName", "Job");
                });

            modelBuilder.Entity("NSLDS.Domain.NsldsFAHType1", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool?>("ACGChange");

                    b.Property<bool>("ActiveBankruptcy");

                    b.Property<bool?>("AggregateChange");

                    b.Property<decimal?>("CombinedPendingDisb");

                    b.Property<decimal?>("CombinedPrincipalBal");

                    b.Property<decimal?>("CombinedTotal");

                    b.Property<DateTime>("DOB");

                    b.Property<bool>("DefaultedLoan");

                    b.Property<string>("DirectLoanPlusGradMPN")
                        .HasAnnotation("MaxLength", 1);

                    b.Property<string>("DirectLoanPlusMPN")
                        .HasAnnotation("MaxLength", 1);

                    b.Property<string>("DirectStaffordMPN")
                        .HasAnnotation("MaxLength", 1);

                    b.Property<string>("DischargedLoanCode")
                        .HasAnnotation("MaxLength", 1);

                    b.Property<string>("FirstName")
                        .HasAnnotation("MaxLength", 12);

                    b.Property<bool>("Fraud");

                    b.Property<string>("GradAY")
                        .HasAnnotation("MaxLength", 4);

                    b.Property<string>("GradCombinedLoanLimit")
                        .HasAnnotation("MaxLength", 1);

                    b.Property<decimal?>("GradCombinedPendingDisb");

                    b.Property<decimal?>("GradCombinedPrincipalBal");

                    b.Property<decimal?>("GradCombinedTotal");

                    b.Property<string>("GradDependency")
                        .HasAnnotation("MaxLength", 1);

                    b.Property<decimal?>("GradEligibilityUsed");

                    b.Property<decimal?>("GradRemainingAmount");

                    b.Property<string>("GradSubLoanLimit")
                        .HasAnnotation("MaxLength", 1);

                    b.Property<decimal?>("GradSubPendingDisb");

                    b.Property<decimal?>("GradSubPrincipalBal");

                    b.Property<decimal?>("GradSubTotal");

                    b.Property<decimal?>("GradTeachTotalDisb");

                    b.Property<decimal?>("GradUnallocPrincipalBal");

                    b.Property<decimal?>("GradUnallocTotal");

                    b.Property<decimal?>("GradUnsubPendingDisb");

                    b.Property<decimal?>("GradUnsubPrincipalBal");

                    b.Property<decimal?>("GradUnsubTotal");

                    b.Property<string>("LastName")
                        .HasAnnotation("MaxLength", 35);

                    b.Property<decimal?>("LifeEligibilityUsed")
                        .HasAnnotation("Relational:ColumnType", "decimal(18,3)");

                    b.Property<int>("Link2ClientRequestStudent");

                    b.Property<bool?>("LoanChange");

                    b.Property<bool?>("PellChange");

                    b.Property<string>("PellLifeLimit")
                        .HasAnnotation("MaxLength", 1);

                    b.Property<decimal?>("PerkinsCurrentAYDisb");

                    b.Property<decimal?>("PerkinsTotalPrincipalBal");

                    b.Property<decimal?>("PlusLoanPrincipalBal");

                    b.Property<decimal?>("PlusLoanTotal");

                    b.Property<decimal?>("PlusProPrincipalBal");

                    b.Property<decimal?>("PlusProTotal");

                    b.Property<bool?>("SMARTChange");

                    b.Property<bool>("SatisfactoryRepayLoan");

                    b.Property<decimal?>("SubPendingDisb");

                    b.Property<decimal?>("SubPrincipalBal");

                    b.Property<decimal?>("SubTotal");

                    b.Property<decimal?>("SubUsagePeriod")
                        .HasAnnotation("Relational:ColumnType", "decimal(18,3)");

                    b.Property<bool?>("SulaFlag");

                    b.Property<bool?>("TeachGrantChange");

                    b.Property<bool>("TeachGrantConverted");

                    b.Property<bool?>("TeachGrantDataChange");

                    b.Property<bool?>("TeachLoanChange");

                    b.Property<decimal?>("TeachLoanPrincipalBal");

                    b.Property<decimal?>("TeachLoanTotal");

                    b.Property<decimal?>("UnAllocPrincipalBal");

                    b.Property<decimal?>("UnAllocPrincipalTotal");

                    b.Property<decimal?>("UnSubPendingDisb");

                    b.Property<decimal?>("UnSubPrincipalBal");

                    b.Property<decimal?>("UnSubTotal");

                    b.Property<string>("UnderGradCombinedLoanLimit")
                        .HasAnnotation("MaxLength", 1);

                    b.Property<string>("UnderGradSubLoanLimit")
                        .HasAnnotation("MaxLength", 1);

                    b.Property<string>("UndergradAY")
                        .HasAnnotation("MaxLength", 4);

                    b.Property<decimal?>("UndergradCombinedPendingDisb");

                    b.Property<decimal?>("UndergradCombinedPrincipalBal");

                    b.Property<decimal?>("UndergradCombinedTotal");

                    b.Property<string>("UndergradDependency")
                        .HasAnnotation("MaxLength", 1);

                    b.Property<decimal?>("UndergradEligibilityUsed")
                        .HasAnnotation("Relational:ColumnType", "decimal(18,4)");

                    b.Property<decimal?>("UndergradRemainingAmount");

                    b.Property<decimal?>("UndergradSubPendingDisb");

                    b.Property<decimal?>("UndergradSubPrincipalBal");

                    b.Property<decimal?>("UndergradSubTotal");

                    b.Property<decimal?>("UndergradTeachDisbTotal");

                    b.Property<decimal?>("UndergradUnallocPrincipalBal");

                    b.Property<decimal?>("UndergradUnallocTotal");

                    b.Property<decimal?>("UndergradUnsubPendingDisb");

                    b.Property<decimal?>("UndergradUnsubPrincipalBal");

                    b.Property<decimal?>("UndergradUnsubTotal");

                    b.Property<string>("UnusualEnrollHistory")
                        .HasAnnotation("MaxLength", 1);

                    b.HasKey("Id");

                    b.HasIndex("SulaFlag");

                    b.HasAnnotation("Relational:Schema", "nslds");

                    b.HasAnnotation("Relational:TableName", "NsldsFAHType1");
                });

            modelBuilder.Entity("NSLDS.Domain.NsldsFAHType2", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CurrentDOB")
                        .HasAnnotation("Relational:ColumnType", "date");

                    b.Property<string>("CurrentFirstName")
                        .HasAnnotation("MaxLength", 12);

                    b.Property<string>("CurrentLastName")
                        .HasAnnotation("MaxLength", 35);

                    b.Property<string>("FirstNameHistory")
                        .HasAnnotation("MaxLength", 12);

                    b.Property<string>("LastNameHistory")
                        .HasAnnotation("MaxLength", 35);

                    b.Property<int>("Link2ClientRequestStudent");

                    b.Property<string>("MiddleInitialHistory")
                        .HasAnnotation("MaxLength", 1);

                    b.HasKey("Id");

                    b.HasAnnotation("Relational:Schema", "nslds");

                    b.HasAnnotation("Relational:TableName", "NsldsFAHType2");
                });

            modelBuilder.Entity("NSLDS.Domain.NsldsFAHType3", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("GrantContact")
                        .HasAnnotation("MaxLength", 8);

                    b.Property<int>("Link2ClientRequestStudent");

                    b.Property<string>("OverDisbAY")
                        .HasAnnotation("MaxLength", 4);

                    b.Property<string>("Overpayment")
                        .HasAnnotation("MaxLength", 1);

                    b.Property<string>("OverpaymentType")
                        .HasAnnotation("MaxLength", 2);

                    b.HasKey("Id");

                    b.HasAnnotation("Relational:Schema", "nslds");

                    b.HasAnnotation("Relational:TableName", "NsldsFAHType3");
                });

            modelBuilder.Entity("NSLDS.Domain.NsldsFAHType4", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AYLevel")
                        .HasAnnotation("MaxLength", 1);

                    b.Property<string>("AcceptedVerification")
                        .HasAnnotation("MaxLength", 3);

                    b.Property<bool?>("AddEligibility");

                    b.Property<decimal?>("AwardAmount");

                    b.Property<string>("AwardId")
                        .HasAnnotation("MaxLength", 21);

                    b.Property<string>("AwardYear")
                        .HasAnnotation("MaxLength", 4);

                    b.Property<string>("CIPCode")
                        .HasAnnotation("MaxLength", 7);

                    b.Property<DateTime?>("CODPostedDate");

                    b.Property<decimal?>("DisbAmount");

                    b.Property<string>("EligibilityCode")
                        .HasAnnotation("MaxLength", 2);

                    b.Property<decimal?>("EligibilityUsed")
                        .HasAnnotation("Relational:ColumnType", "decimal(18,4)");

                    b.Property<decimal?>("FamilyContribution");

                    b.Property<bool?>("FirstTimePell");

                    b.Property<bool?>("GrantChangeFlag");

                    b.Property<string>("GrantSchoolCode")
                        .HasAnnotation("MaxLength", 8);

                    b.Property<short?>("GrantSequence");

                    b.Property<string>("GrantType")
                        .HasAnnotation("MaxLength", 2);

                    b.Property<string>("HSProgramCode")
                        .HasAnnotation("MaxLength", 6);

                    b.Property<DateTime?>("LastDisbDate");

                    b.Property<int>("Link2ClientRequestStudent");

                    b.Property<bool?>("Post911DeceasedDep");

                    b.Property<decimal?>("SchedAmount");

                    b.Property<DateTime?>("TEACHConversionDate");

                    b.Property<bool?>("TEACHConverted");

                    b.Property<decimal?>("TotalEligibilityUsed")
                        .HasAnnotation("Relational:ColumnType", "decimal(18,4)");

                    b.Property<string>("TransNumber")
                        .HasAnnotation("MaxLength", 2);

                    b.HasKey("Id");

                    b.HasAnnotation("Relational:Schema", "nslds");

                    b.HasAnnotation("Relational:TableName", "NsldsFAHType4");
                });

            modelBuilder.Entity("NSLDS.Domain.NsldsFAHType5", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime?>("AYBeginDate");

                    b.Property<DateTime?>("AYEndDate");

                    b.Property<string>("AcademicLevel")
                        .HasAnnotation("MaxLength", 1);

                    b.Property<string>("AdditionalUnsubLoan")
                        .HasAnnotation("MaxLength", 1);

                    b.Property<decimal?>("CalcCombinedAmount");

                    b.Property<decimal?>("CalcSubAmount");

                    b.Property<decimal?>("CalcUnsubAmount");

                    b.Property<bool?>("CapitalInterest");

                    b.Property<DateTime?>("ConfirmSubDate");

                    b.Property<string>("ConfirmSubStatus")
                        .HasAnnotation("MaxLength", 1);

                    b.Property<string>("ContactType")
                        .HasAnnotation("MaxLength", 3);

                    b.Property<string>("CurrentGACode")
                        .HasAnnotation("MaxLength", 3);

                    b.Property<string>("EncDataProviderID");

                    b.Property<decimal?>("LastDisbAmount");

                    b.Property<DateTime?>("LastLoanDisbDate");

                    b.Property<string>("LenderCode")
                        .HasAnnotation("MaxLength", 6);

                    b.Property<string>("LenderServicer")
                        .HasAnnotation("MaxLength", 6);

                    b.Property<int>("Link2ClientRequestStudent");

                    b.Property<decimal?>("LoanAmount");

                    b.Property<string>("LoanContact")
                        .HasAnnotation("MaxLength", 8);

                    b.Property<DateTime?>("LoanDate");

                    b.Property<DateTime?>("LoanPeriodBeginDate");

                    b.Property<DateTime?>("LoanPeriodEndDate");

                    b.Property<bool?>("LoanRecChange");

                    b.Property<string>("LoanSchoolCode")
                        .HasAnnotation("MaxLength", 8);

                    b.Property<string>("LoanStatusCode")
                        .HasAnnotation("MaxLength", 2);

                    b.Property<DateTime>("LoanStatusDate");

                    b.Property<string>("LoanTypeCode")
                        .HasAnnotation("MaxLength", 2);

                    b.Property<decimal?>("NetLoanAmount");

                    b.Property<string>("PerkinsCancelCode")
                        .HasAnnotation("MaxLength", 3);

                    b.Property<decimal?>("PrincipalBal");

                    b.Property<DateTime?>("PrincipalDate");

                    b.Property<bool?>("Reaffirmation");

                    b.Property<decimal?>("TotalDisb");

                    b.Property<decimal?>("UnallocatedAmount");

                    b.HasKey("Id");

                    b.HasIndex("LoanStatusCode");

                    b.HasAnnotation("Relational:Schema", "nslds");

                    b.HasAnnotation("Relational:TableName", "NsldsFAHType5");
                });

            modelBuilder.Entity("NSLDS.Domain.ClientRequest", b =>
                {
                    b.HasOne("NSLDS.Domain.ClientProfile")
                        .WithMany()
                        .HasForeignKey("Link2ClientProfile");

                    b.HasOne("NSLDS.Domain.Job")
                        .WithMany()
                        .HasForeignKey("Link2Job");
                });

            modelBuilder.Entity("NSLDS.Domain.ClientRequestAlert", b =>
                {
                    b.HasOne("NSLDS.Domain.ClientRequest")
                        .WithMany()
                        .HasForeignKey("Link2ClientRequest");
                });

            modelBuilder.Entity("NSLDS.Domain.ClientRequestStudent", b =>
                {
                    b.HasOne("NSLDS.Domain.ClientRequest")
                        .WithMany()
                        .HasForeignKey("Link2ClientRequest");
                });

            modelBuilder.Entity("NSLDS.Domain.ClientRequestStudentAlert", b =>
                {
                    b.HasOne("NSLDS.Domain.ClientRequestStudent")
                        .WithMany()
                        .HasForeignKey("Link2ClientRequestStudent");
                });

            modelBuilder.Entity("NSLDS.Domain.Job", b =>
                {
                    b.HasOne("NSLDS.Domain.ClientProfile")
                        .WithMany()
                        .HasForeignKey("Link2ClientProfile");
                });

            modelBuilder.Entity("NSLDS.Domain.NsldsFAHType1", b =>
                {
                    b.HasOne("NSLDS.Domain.ClientRequestStudent")
                        .WithMany()
                        .HasForeignKey("Link2ClientRequestStudent");
                });

            modelBuilder.Entity("NSLDS.Domain.NsldsFAHType2", b =>
                {
                    b.HasOne("NSLDS.Domain.ClientRequestStudent")
                        .WithMany()
                        .HasForeignKey("Link2ClientRequestStudent");
                });

            modelBuilder.Entity("NSLDS.Domain.NsldsFAHType3", b =>
                {
                    b.HasOne("NSLDS.Domain.ClientRequestStudent")
                        .WithMany()
                        .HasForeignKey("Link2ClientRequestStudent");
                });

            modelBuilder.Entity("NSLDS.Domain.NsldsFAHType4", b =>
                {
                    b.HasOne("NSLDS.Domain.ClientRequestStudent")
                        .WithMany()
                        .HasForeignKey("Link2ClientRequestStudent");
                });

            modelBuilder.Entity("NSLDS.Domain.NsldsFAHType5", b =>
                {
                    b.HasOne("NSLDS.Domain.ClientRequestStudent")
                        .WithMany()
                        .HasForeignKey("Link2ClientRequestStudent");
                });
        }
    }
}

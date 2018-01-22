using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using NSLDS.Domain;

namespace nslds.domain.Migrations
{
    [DbContext(typeof(NSLDS_Context))]
    [Migration("20171012210247_security_audit_fix")]
    partial class security_audit_fix
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2")
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
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(false);

                    b.Property<bool>("IsPwdValid")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(false);

                    b.Property<int>("Monitoring");

                    b.Property<string>("OPEID")
                        .IsRequired()
                        .HasMaxLength(8);

                    b.Property<string>("Organization_Name")
                        .IsRequired();

                    b.Property<string>("Phone")
                        .IsRequired();

                    b.Property<int>("Retention");

                    b.Property<string>("RevBy")
                        .IsRequired()
                        .HasMaxLength(450);

                    b.Property<DateTime?>("RevOn")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("getdate()");

                    b.Property<string>("SAIG")
                        .IsRequired()
                        .HasMaxLength(7);

                    b.Property<string>("State")
                        .IsRequired();

                    b.Property<string>("TD_Password");

                    b.Property<string>("Upload_Method")
                        .HasMaxLength(1);

                    b.Property<string>("Zip")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("OPEID");

                    b.ToTable("ClientProfile","nslds");
                });

            modelBuilder.Entity("NSLDS.Domain.ClientProfile_History", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AY_Definition");

                    b.Property<string>("Action")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<string>("ActionBy")
                        .IsRequired()
                        .HasMaxLength(450);

                    b.Property<DateTime?>("ActionOn")
                        .IsRequired()
                        .HasColumnType("datetime");

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

                    b.Property<string>("OPEID")
                        .IsRequired()
                        .HasMaxLength(8);

                    b.Property<string>("Organization_Name")
                        .IsRequired();

                    b.Property<string>("Phone")
                        .IsRequired();

                    b.Property<int>("Retention");

                    b.Property<string>("RevBy")
                        .IsRequired()
                        .HasMaxLength(450);

                    b.Property<DateTime?>("RevOn")
                        .IsRequired()
                        .HasColumnType("datetime");

                    b.Property<string>("SAIG")
                        .IsRequired()
                        .HasMaxLength(7);

                    b.Property<string>("State")
                        .IsRequired();

                    b.Property<string>("TD_Password");

                    b.Property<string>("Upload_Method")
                        .HasMaxLength(1);

                    b.Property<string>("Zip")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("Link2ClientProfile");

                    b.ToTable("ClientProfile_History","nslds");
                });

            modelBuilder.Entity("NSLDS.Domain.ClientRequest", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("IsDeleted")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(false);

                    b.Property<bool>("IsFailed")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(false);

                    b.Property<bool>("IsOnHold")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(false);

                    b.Property<bool>("IsReceived")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(false);

                    b.Property<bool>("IsSubmitted")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(false);

                    b.Property<bool>("IsTM")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(false);

                    b.Property<int>("Link2ClientProfile");

                    b.Property<int?>("Link2Job");

                    b.Property<DateTime?>("ReceivedOn")
                        .HasColumnType("datetime");

                    b.Property<string>("Response");

                    b.Property<string>("RevBy")
                        .IsRequired()
                        .HasMaxLength(450);

                    b.Property<DateTime?>("RevOn")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("getdate()");

                    b.Property<short>("Sequence");

                    b.Property<DateTime?>("SubmittedOn")
                        .HasColumnType("datetime");

                    b.HasKey("Id");

                    b.HasIndex("Link2ClientProfile");

                    b.HasIndex("Link2Job");

                    b.HasIndex("RevOn");

                    b.ToTable("ClientRequest","nslds");
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

                    b.HasIndex("Link2ClientRequest");

                    b.ToTable("ClientRequestAlert","nslds");
                });

            modelBuilder.Entity("NSLDS.Domain.ClientRequestStudent", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime?>("DOB")
                        .HasColumnType("date");

                    b.Property<string>("DeleteMonitoring")
                        .HasColumnType("nchar(1)")
                        .HasMaxLength(1);

                    b.Property<string>("EncSSN")
                        .HasMaxLength(450);

                    b.Property<DateTime?>("EnrollBeginDate")
                        .HasColumnType("date");

                    b.Property<string>("FirstName")
                        .HasMaxLength(100);

                    b.Property<bool>("IsDeleted")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(false);

                    b.Property<bool?>("IsGrantReviewed");

                    b.Property<bool?>("IsLoanReviewed");

                    b.Property<bool?>("IsPellReviewed");

                    b.Property<bool>("IsReceived")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(false);

                    b.Property<bool>("IsRefreshed");

                    b.Property<bool>("IsResolved");

                    b.Property<bool>("IsSubmitted")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(false);

                    b.Property<bool?>("IsTeachReviewed");

                    b.Property<bool>("IsValid")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(false);

                    b.Property<string>("LastName")
                        .HasMaxLength(100);

                    b.Property<int>("Link2ClientRequest");

                    b.Property<DateTime?>("MonitorBeginDate")
                        .HasColumnType("date");

                    b.Property<DateTime?>("ReceivedOn")
                        .HasColumnType("datetime");

                    b.Property<string>("RequestType")
                        .HasColumnType("nchar(1)")
                        .HasMaxLength(1);

                    b.Property<string>("Response");

                    b.Property<string>("RevBy")
                        .IsRequired()
                        .HasMaxLength(450);

                    b.Property<DateTime?>("RevOn")
                        .IsRequired()
                        .HasColumnType("datetime");

                    b.Property<string>("SID")
                        .HasMaxLength(50);

                    b.Property<DateTime?>("StartDate")
                        .HasColumnType("date");

                    b.Property<DateTime?>("SubmittedOn")
                        .HasColumnType("datetime");

                    b.HasKey("Id");

                    b.HasIndex("DOB");

                    b.HasIndex("EncSSN");

                    b.HasIndex("FirstName");

                    b.HasIndex("IsReceived");

                    b.HasIndex("LastName");

                    b.HasIndex("Link2ClientRequest");

                    b.HasIndex("RevOn");

                    b.HasIndex("StartDate");

                    b.ToTable("ClientRequestStudent","nslds");
                });

            modelBuilder.Entity("NSLDS.Domain.ClientRequestStudent_History", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Action")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<string>("ActionBy")
                        .IsRequired()
                        .HasMaxLength(450);

                    b.Property<DateTime?>("ActionOn")
                        .IsRequired()
                        .HasColumnType("datetime");

                    b.Property<DateTime?>("DOB")
                        .HasColumnType("date");

                    b.Property<string>("DeleteMonitoring")
                        .HasColumnType("nchar(1)")
                        .HasMaxLength(1);

                    b.Property<string>("EncSSN")
                        .HasMaxLength(450);

                    b.Property<DateTime?>("EnrollBeginDate")
                        .HasColumnType("date");

                    b.Property<string>("FirstName")
                        .HasMaxLength(100);

                    b.Property<bool>("IsDeleted");

                    b.Property<bool?>("IsGrantReviewed");

                    b.Property<bool?>("IsLoanReviewed");

                    b.Property<bool?>("IsPellReviewed");

                    b.Property<bool>("IsReceived");

                    b.Property<bool>("IsRefreshed");

                    b.Property<bool>("IsResolved");

                    b.Property<bool>("IsSubmitted");

                    b.Property<bool?>("IsTeachReviewed");

                    b.Property<bool>("IsValid");

                    b.Property<string>("LastName")
                        .HasMaxLength(100);

                    b.Property<int>("Link2ClientRequest");

                    b.Property<int>("Link2ClientRequestStudent");

                    b.Property<DateTime?>("MonitorBeginDate")
                        .HasColumnType("date");

                    b.Property<DateTime?>("ReceivedOn")
                        .HasColumnType("datetime");

                    b.Property<string>("RequestType")
                        .HasColumnType("nchar(1)")
                        .HasMaxLength(1);

                    b.Property<string>("Response");

                    b.Property<string>("RevBy")
                        .IsRequired()
                        .HasMaxLength(450);

                    b.Property<DateTime?>("RevOn")
                        .IsRequired()
                        .HasColumnType("datetime");

                    b.Property<string>("SID")
                        .HasMaxLength(50);

                    b.Property<DateTime?>("StartDate")
                        .HasColumnType("date");

                    b.Property<DateTime?>("SubmittedOn")
                        .HasColumnType("datetime");

                    b.HasKey("Id");

                    b.HasIndex("Link2ClientRequestStudent");

                    b.ToTable("ClientRequestStudent_History","nslds");
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

                    b.HasIndex("Link2ClientRequestStudent");

                    b.ToTable("ClientRequestStudentAlert","nslds");
                });

            modelBuilder.Entity("NSLDS.Domain.Job", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime?>("BilledOn")
                        .HasColumnType("datetime");

                    b.Property<bool>("IsBilled")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(false);

                    b.Property<bool>("IsDeleted")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(false);

                    b.Property<bool>("IsReceived")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(false);

                    b.Property<bool>("IsSubmitted")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(false);

                    b.Property<DateTime>("JobDate")
                        .HasColumnType("date");

                    b.Property<int>("Link2ClientProfile");

                    b.Property<DateTime?>("ReceivedOn")
                        .HasColumnType("datetime");

                    b.Property<string>("Response");

                    b.Property<string>("RevBy")
                        .IsRequired()
                        .HasMaxLength(450);

                    b.Property<DateTime?>("RevOn")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("getdate()");

                    b.Property<DateTime?>("SubmittedOn")
                        .HasColumnType("datetime");

                    b.HasKey("Id");

                    b.HasIndex("Link2ClientProfile");

                    b.ToTable("Job","nslds");
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
                        .HasMaxLength(1);

                    b.Property<string>("DirectLoanPlusMPN")
                        .HasMaxLength(1);

                    b.Property<string>("DirectStaffordMPN")
                        .HasMaxLength(1);

                    b.Property<string>("DischargedLoanCode")
                        .HasMaxLength(1);

                    b.Property<string>("FirstName")
                        .HasMaxLength(12);

                    b.Property<bool>("Fraud");

                    b.Property<string>("GradAY")
                        .HasMaxLength(4);

                    b.Property<string>("GradCombinedLoanLimit")
                        .HasMaxLength(1);

                    b.Property<decimal?>("GradCombinedPendingDisb");

                    b.Property<decimal?>("GradCombinedPrincipalBal");

                    b.Property<decimal?>("GradCombinedTotal");

                    b.Property<string>("GradDependency")
                        .HasMaxLength(1);

                    b.Property<decimal?>("GradEligibilityUsed");

                    b.Property<decimal?>("GradRemainingAmount");

                    b.Property<string>("GradSubLoanLimit")
                        .HasMaxLength(1);

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
                        .HasMaxLength(35);

                    b.Property<decimal?>("LifeEligibilityUsed")
                        .HasColumnType("decimal(18,3)");

                    b.Property<int>("Link2ClientRequestStudent");

                    b.Property<bool?>("LoanChange");

                    b.Property<bool?>("PellChange");

                    b.Property<string>("PellLifeLimit")
                        .HasMaxLength(1);

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
                        .HasColumnType("decimal(18,3)");

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
                        .HasMaxLength(1);

                    b.Property<string>("UnderGradSubLoanLimit")
                        .HasMaxLength(1);

                    b.Property<string>("UndergradAY")
                        .HasMaxLength(4);

                    b.Property<decimal?>("UndergradCombinedPendingDisb");

                    b.Property<decimal?>("UndergradCombinedPrincipalBal");

                    b.Property<decimal?>("UndergradCombinedTotal");

                    b.Property<string>("UndergradDependency")
                        .HasMaxLength(1);

                    b.Property<decimal?>("UndergradEligibilityUsed")
                        .HasColumnType("decimal(18,4)");

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
                        .HasMaxLength(1);

                    b.HasKey("Id");

                    b.HasIndex("Link2ClientRequestStudent");

                    b.HasIndex("SulaFlag");

                    b.ToTable("NsldsFAHType1","nslds");
                });

            modelBuilder.Entity("NSLDS.Domain.NsldsFAHType2", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CurrentDOB")
                        .HasColumnType("date");

                    b.Property<string>("CurrentFirstName")
                        .HasMaxLength(12);

                    b.Property<string>("CurrentLastName")
                        .HasMaxLength(35);

                    b.Property<string>("FirstNameHistory")
                        .HasMaxLength(12);

                    b.Property<string>("LastNameHistory")
                        .HasMaxLength(35);

                    b.Property<int>("Link2ClientRequestStudent");

                    b.Property<string>("MiddleInitialHistory")
                        .HasMaxLength(1);

                    b.HasKey("Id");

                    b.HasIndex("Link2ClientRequestStudent");

                    b.ToTable("NsldsFAHType2","nslds");
                });

            modelBuilder.Entity("NSLDS.Domain.NsldsFAHType3", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("GrantContact")
                        .HasMaxLength(8);

                    b.Property<int>("Link2ClientRequestStudent");

                    b.Property<string>("OverDisbAY")
                        .HasMaxLength(4);

                    b.Property<string>("Overpayment")
                        .HasMaxLength(1);

                    b.Property<string>("OverpaymentType")
                        .HasMaxLength(2);

                    b.HasKey("Id");

                    b.HasIndex("Link2ClientRequestStudent");

                    b.ToTable("NsldsFAHType3","nslds");
                });

            modelBuilder.Entity("NSLDS.Domain.NsldsFAHType4", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AYLevel")
                        .HasMaxLength(1);

                    b.Property<string>("AcceptedVerification")
                        .HasMaxLength(3);

                    b.Property<bool?>("AddEligibility");

                    b.Property<decimal?>("AwardAmount");

                    b.Property<string>("AwardId")
                        .HasMaxLength(21);

                    b.Property<string>("AwardYear")
                        .HasMaxLength(4);

                    b.Property<string>("CIPCode")
                        .HasMaxLength(7);

                    b.Property<DateTime?>("CODPostedDate");

                    b.Property<decimal?>("DisbAmount");

                    b.Property<string>("EligibilityCode")
                        .HasMaxLength(2);

                    b.Property<decimal?>("EligibilityUsed")
                        .HasColumnType("decimal(18,4)");

                    b.Property<decimal?>("FamilyContribution");

                    b.Property<bool?>("FirstTimePell");

                    b.Property<bool?>("GrantChangeFlag");

                    b.Property<string>("GrantSchoolCode")
                        .HasMaxLength(8);

                    b.Property<short?>("GrantSequence");

                    b.Property<string>("GrantType")
                        .HasMaxLength(2);

                    b.Property<string>("HSProgramCode")
                        .HasMaxLength(6);

                    b.Property<DateTime?>("LastDisbDate");

                    b.Property<int>("Link2ClientRequestStudent");

                    b.Property<bool?>("Post911DeceasedDep");

                    b.Property<decimal?>("SchedAmount");

                    b.Property<DateTime?>("TEACHConversionDate");

                    b.Property<bool?>("TEACHConverted");

                    b.Property<decimal?>("TotalEligibilityUsed")
                        .HasColumnType("decimal(18,4)");

                    b.Property<string>("TransNumber")
                        .HasMaxLength(2);

                    b.HasKey("Id");

                    b.HasIndex("Link2ClientRequestStudent");

                    b.ToTable("NsldsFAHType4","nslds");
                });

            modelBuilder.Entity("NSLDS.Domain.NsldsFAHType5", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime?>("AYBeginDate");

                    b.Property<DateTime?>("AYEndDate");

                    b.Property<string>("AcademicLevel")
                        .HasMaxLength(1);

                    b.Property<string>("AdditionalUnsubLoan")
                        .HasMaxLength(1);

                    b.Property<decimal?>("CalcCombinedAmount");

                    b.Property<decimal?>("CalcSubAmount");

                    b.Property<decimal?>("CalcUnsubAmount");

                    b.Property<bool?>("CapitalInterest");

                    b.Property<DateTime?>("ConfirmSubDate");

                    b.Property<string>("ConfirmSubStatus")
                        .HasMaxLength(1);

                    b.Property<string>("ContactType")
                        .HasMaxLength(3);

                    b.Property<string>("CurrentGACode")
                        .HasMaxLength(3);

                    b.Property<string>("EncDataProviderID");

                    b.Property<decimal?>("LastDisbAmount");

                    b.Property<DateTime?>("LastLoanDisbDate");

                    b.Property<string>("LenderCode")
                        .HasMaxLength(6);

                    b.Property<string>("LenderServicer")
                        .HasMaxLength(6);

                    b.Property<int>("Link2ClientRequestStudent");

                    b.Property<decimal?>("LoanAmount");

                    b.Property<string>("LoanContact")
                        .HasMaxLength(8);

                    b.Property<DateTime?>("LoanDate");

                    b.Property<DateTime?>("LoanPeriodBeginDate");

                    b.Property<DateTime?>("LoanPeriodEndDate");

                    b.Property<bool?>("LoanRecChange");

                    b.Property<string>("LoanSchoolCode")
                        .HasMaxLength(8);

                    b.Property<string>("LoanStatusCode")
                        .HasMaxLength(2);

                    b.Property<DateTime>("LoanStatusDate");

                    b.Property<string>("LoanTypeCode")
                        .HasMaxLength(2);

                    b.Property<decimal?>("NetLoanAmount");

                    b.Property<string>("PerkinsCancelCode")
                        .HasMaxLength(3);

                    b.Property<decimal?>("PrincipalBal");

                    b.Property<DateTime?>("PrincipalDate");

                    b.Property<bool?>("Reaffirmation");

                    b.Property<decimal?>("TotalDisb");

                    b.Property<decimal?>("UnallocatedAmount");

                    b.HasKey("Id");

                    b.HasIndex("Link2ClientRequestStudent");

                    b.HasIndex("LoanStatusCode");

                    b.ToTable("NsldsFAHType5","nslds");
                });

            modelBuilder.Entity("NSLDS.Domain.ClientProfile_History", b =>
                {
                    b.HasOne("NSLDS.Domain.ClientProfile", "ClientProfile")
                        .WithMany()
                        .HasForeignKey("Link2ClientProfile")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("NSLDS.Domain.ClientRequest", b =>
                {
                    b.HasOne("NSLDS.Domain.ClientProfile", "ClientProfile")
                        .WithMany("ClientRequests")
                        .HasForeignKey("Link2ClientProfile")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("NSLDS.Domain.Job", "Job")
                        .WithMany("ClientRequests")
                        .HasForeignKey("Link2Job");
                });

            modelBuilder.Entity("NSLDS.Domain.ClientRequestAlert", b =>
                {
                    b.HasOne("NSLDS.Domain.ClientRequest", "ClientRequest")
                        .WithMany("Alerts")
                        .HasForeignKey("Link2ClientRequest")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("NSLDS.Domain.ClientRequestStudent", b =>
                {
                    b.HasOne("NSLDS.Domain.ClientRequest", "ClientRequest")
                        .WithMany("Students")
                        .HasForeignKey("Link2ClientRequest")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("NSLDS.Domain.ClientRequestStudent_History", b =>
                {
                    b.HasOne("NSLDS.Domain.ClientRequestStudent", "ClientRequestStudent")
                        .WithMany()
                        .HasForeignKey("Link2ClientRequestStudent")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("NSLDS.Domain.ClientRequestStudentAlert", b =>
                {
                    b.HasOne("NSLDS.Domain.ClientRequestStudent", "ClientRequestStudent")
                        .WithMany("Alerts")
                        .HasForeignKey("Link2ClientRequestStudent")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("NSLDS.Domain.Job", b =>
                {
                    b.HasOne("NSLDS.Domain.ClientProfile", "ClientProfile")
                        .WithMany("Jobs")
                        .HasForeignKey("Link2ClientProfile")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("NSLDS.Domain.NsldsFAHType1", b =>
                {
                    b.HasOne("NSLDS.Domain.ClientRequestStudent", "ClientRequestStudent")
                        .WithMany()
                        .HasForeignKey("Link2ClientRequestStudent")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("NSLDS.Domain.NsldsFAHType2", b =>
                {
                    b.HasOne("NSLDS.Domain.ClientRequestStudent", "ClientRequestStudent")
                        .WithMany()
                        .HasForeignKey("Link2ClientRequestStudent")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("NSLDS.Domain.NsldsFAHType3", b =>
                {
                    b.HasOne("NSLDS.Domain.ClientRequestStudent", "ClientRequestStudent")
                        .WithMany()
                        .HasForeignKey("Link2ClientRequestStudent")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("NSLDS.Domain.NsldsFAHType4", b =>
                {
                    b.HasOne("NSLDS.Domain.ClientRequestStudent", "ClientRequestStudent")
                        .WithMany()
                        .HasForeignKey("Link2ClientRequestStudent")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("NSLDS.Domain.NsldsFAHType5", b =>
                {
                    b.HasOne("NSLDS.Domain.ClientRequestStudent", "ClientRequestStudent")
                        .WithMany()
                        .HasForeignKey("Link2ClientRequestStudent")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}

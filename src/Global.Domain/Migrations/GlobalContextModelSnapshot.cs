using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Global.Domain;

namespace global.domain.Migrations
{
    [DbContext(typeof(GlobalContext))]
    partial class GlobalContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0-rc1-16348")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Global.Domain.FahAlert", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(50);

                    b.Property<string>("Description");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("FahAlert","nslds");
                });

            modelBuilder.Entity("Global.Domain.FahCode", b =>
                {
                    b.Property<string>("FahFieldId")
                        .HasMaxLength(50);

                    b.Property<string>("Code")
                        .HasMaxLength(10);

                    b.Property<string>("Name");

                    b.HasKey("FahFieldId", "Code");

                    b.ToTable("FahCode","nslds");
                });

            modelBuilder.Entity("Global.Domain.FahField", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(50);

                    b.Property<string>("Description");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("FahField","nslds");
                });

            modelBuilder.Entity("Global.Domain.FedSchoolCodeList", b =>
                {
                    b.Property<string>("SchoolCode")
                        .HasMaxLength(8);

                    b.Property<string>("Address")
                        .HasMaxLength(255);

                    b.Property<string>("City")
                        .HasMaxLength(255);

                    b.Property<string>("Country")
                        .HasMaxLength(255);

                    b.Property<string>("PostalCode")
                        .HasMaxLength(255);

                    b.Property<string>("Province")
                        .HasMaxLength(255);

                    b.Property<string>("SchoolName")
                        .HasMaxLength(255);

                    b.Property<string>("StateCode")
                        .HasMaxLength(255);

                    b.Property<string>("ZipCode")
                        .HasMaxLength(255);

                    b.HasKey("SchoolCode");

                    b.ToTable("FedSchoolCodeList","dbo");
                });

            modelBuilder.Entity("Global.Domain.PellAward", b =>
                {
                    b.Property<int>("AwardYear");

                    b.Property<string>("AYDisplay")
                        .HasMaxLength(5);

                    b.Property<decimal>("AdditionalPercent");

                    b.Property<decimal>("MaxAmount");

                    b.HasKey("AwardYear");

                    b.ToTable("PellAward","nslds");
                });

            modelBuilder.Entity("Global.Domain.Tenant", b =>
                {
                    b.Property<string>("TenantId")
                        .HasMaxLength(10);

                    b.Property<DateTime>("CreatedOn");

                    b.Property<string>("DatabaseName")
                        .HasMaxLength(50);

                    b.Property<bool>("IsActive");

                    b.Property<string>("TenantDomain")
                        .HasMaxLength(255);

                    b.HasKey("TenantId");

                    b.ToTable("Tenant","dbo");
                });

            modelBuilder.Entity("Global.Domain.UserInvite", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("ExpireOn");

                    b.Property<string>("FirstName");

                    b.Property<bool>("HasRegistered")
                        .HasDefaultValue("False")
                        .HasAnnotation("Relational:DefaultValueType", "System.Boolean");

                    b.Property<string>("LastName");

                    b.Property<string>("NSLDS_Role");

                    b.Property<string>("OpeId")
                        .IsRequired()
                        .HasMaxLength(8);

                    b.Property<string>("SenderEmail")
                        .IsRequired();

                    b.Property<string>("SenderName");

                    b.Property<string>("UserEmail")
                        .IsRequired();

                    b.HasKey("Id");

                    b.ToTable("UserInvite","nslds");
                });
        }
    }
}

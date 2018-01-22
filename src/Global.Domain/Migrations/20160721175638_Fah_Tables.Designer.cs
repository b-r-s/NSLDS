using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Global.Domain;

namespace @global.domain.Migrations
{
    [DbContext(typeof(GlobalContext))]
    [Migration("20160721175638_Fah_Tables")]
    partial class Fah_Tables
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0-rc1-16348")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Global.Domain.FahAlert", b =>
                {
                    b.Property<string>("Id")
                        .HasAnnotation("MaxLength", 50);

                    b.Property<string>("Description");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.HasAnnotation("Relational:Schema", "nslds");

                    b.HasAnnotation("Relational:TableName", "FahAlert");
                });

            modelBuilder.Entity("Global.Domain.FahCode", b =>
                {
                    b.Property<string>("FahFieldId")
                        .HasAnnotation("MaxLength", 50);

                    b.Property<string>("Code")
                        .HasAnnotation("MaxLength", 10);

                    b.Property<string>("Name");

                    b.HasKey("FahFieldId", "Code");

                    b.HasAnnotation("Relational:Schema", "nslds");

                    b.HasAnnotation("Relational:TableName", "FahCode");
                });

            modelBuilder.Entity("Global.Domain.FahField", b =>
                {
                    b.Property<string>("Id")
                        .HasAnnotation("MaxLength", 50);

                    b.Property<string>("Description");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.HasAnnotation("Relational:Schema", "nslds");

                    b.HasAnnotation("Relational:TableName", "FahField");
                });

            modelBuilder.Entity("Global.Domain.FedSchoolCodeList", b =>
                {
                    b.Property<string>("SchoolCode")
                        .HasAnnotation("MaxLength", 8);

                    b.Property<string>("Address")
                        .HasAnnotation("MaxLength", 255);

                    b.Property<string>("City")
                        .HasAnnotation("MaxLength", 255);

                    b.Property<string>("Country")
                        .HasAnnotation("MaxLength", 255);

                    b.Property<string>("PostalCode")
                        .HasAnnotation("MaxLength", 255);

                    b.Property<string>("Province")
                        .HasAnnotation("MaxLength", 255);

                    b.Property<string>("SchoolName")
                        .HasAnnotation("MaxLength", 255);

                    b.Property<string>("StateCode")
                        .HasAnnotation("MaxLength", 255);

                    b.Property<string>("ZipCode")
                        .HasAnnotation("MaxLength", 255);

                    b.HasKey("SchoolCode");

                    b.HasAnnotation("Relational:Schema", "dbo");

                    b.HasAnnotation("Relational:TableName", "FedSchoolCodeList");
                });

            modelBuilder.Entity("Global.Domain.Tenant", b =>
                {
                    b.Property<string>("TenantId")
                        .HasAnnotation("MaxLength", 10);

                    b.Property<DateTime>("CreatedOn");

                    b.Property<string>("DatabaseName")
                        .HasAnnotation("MaxLength", 50);

                    b.Property<bool>("IsActive");

                    b.Property<string>("TenantDomain")
                        .HasAnnotation("MaxLength", 255);

                    b.HasKey("TenantId");

                    b.HasAnnotation("Relational:Schema", "dbo");

                    b.HasAnnotation("Relational:TableName", "Tenant");
                });
        }
    }
}

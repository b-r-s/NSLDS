using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Global.Domain;

namespace @global.domain.Migrations
{
    [DbContext(typeof(GlobalContext))]
    [Migration("20160310011341_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0-rc1-16348")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Global.Models.FedSchoolCodeList", b =>
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
        }
    }
}

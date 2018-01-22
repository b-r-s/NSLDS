using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace NSLDS.Domain
{

    public partial class NSLDS_Context : DbContext, INSLDS_Context
	{
        public NSLDS_Context()
        {

        }

        private string _dbName;

        public NSLDS_Context(string dbName)
        {
            _dbName = dbName;
        }

        public NSLDS_Context(DbContextOptions options) : base(options)
        {
            Database.SetCommandTimeout(90);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
			options.UseSqlServer(@"Data Source=B-PC\SQLEXPRESS;Initial Catalog=NextGen_Corp-0;Integrated Security=True;");
			//options.UseSqlServer(@"Data Source=dbt07p.gfasphx.local;Initial Catalog=NextGen_APU;Integrated Security=False;User ID=NSLDS_APP_USER;Password=Global123;");
			//options.UseSqlServer(@"Data Source=dbt07p.gfasphx.local;Initial Catalog=NextGen_Demo;Integrated Security=False;User ID=NSLDS_APP_USER;Password=Global123;");
			//options.UseSqlServer(@"Data Source=dbt07p.gfasphx.local;Initial Catalog=NextGen_123456;Integrated Security=False;User ID=NSLDS_APP_USER;Password=Global123;");
			//options.UseSqlServer(@"Data Source=B-PC\SQLEXPRESS;Initial Catalog=NextGen_Global;Integrated Security=True;");

		}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Non-mapped entities
            // entities not mapped to db tables can still be made available in the context
            // to be populated from stored procedures using .FromSql()
            // !! comment out before creating new migrations on client DBs !!
            modelBuilder.Entity<StudentSearchDetail>();
            #endregion

            modelBuilder.Entity<ClientProfile>(entity =>
            {
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);
                entity.Property(e => e.IsPwdValid).HasDefaultValue(false);

                entity.Property(e => e.RevOn)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("getdate()");

                entity.HasIndex(i => i.OPEID);
            });

            modelBuilder.Entity<ClientProfile_History>(entity =>
            {
                entity.Property(e => e.ActionOn).HasColumnType("datetime");

                entity.Property(e => e.RevOn).HasColumnType("datetime");
            });

            modelBuilder.Entity<ClientRequestStudent>(entity =>
            {
                entity.Property(e => e.DeleteMonitoring).HasColumnType("nchar(1)");

                entity.Property(e => e.DOB).HasColumnType("date");

                entity.Property(e => e.EnrollBeginDate).HasColumnType("date");

                entity.Property(e => e.IsDeleted).HasDefaultValue(false);

                entity.Property(e => e.IsReceived).HasDefaultValue(false);

                entity.Property(e => e.IsSubmitted).HasDefaultValue(false);

                entity.Property(e => e.IsValid).HasDefaultValue(false);

                entity.Property(e => e.MonitorBeginDate).HasColumnType("date");

                entity.Property(e => e.ReceivedOn).HasColumnType("datetime");

                entity.Property(e => e.RequestType).HasColumnType("nchar(1)");

                entity.Property(e => e.RevOn).HasColumnType("datetime");

                entity.Property(e => e.StartDate).HasColumnType("date");

                entity.Property(e => e.SubmittedOn).HasColumnType("datetime");

                entity.HasIndex(i => i.FirstName);
                entity.HasIndex(i => i.LastName);
                entity.HasIndex(i => i.DOB);
                entity.HasIndex(i => i.RevOn);
                entity.HasIndex(i => i.IsReceived);
                entity.HasIndex(i => i.StartDate);
                // EncSSN is protected/inaccessible to hide it from the json result
                entity.HasIndex(new[] { "EncSSN" });
            });

            modelBuilder.Entity<ClientRequestStudent_History>(entity =>
            {
                entity.Property(e => e.ActionOn).HasColumnType("datetime");

                entity.Property(e => e.DeleteMonitoring).HasColumnType("nchar(1)");

                entity.Property(e => e.DOB).HasColumnType("date");

                entity.Property(e => e.EnrollBeginDate).HasColumnType("date");

                entity.Property(e => e.MonitorBeginDate).HasColumnType("date");

                entity.Property(e => e.ReceivedOn).HasColumnType("datetime");

                entity.Property(e => e.RequestType).HasColumnType("nchar(1)");

                entity.Property(e => e.RevOn).HasColumnType("datetime");

                entity.Property(e => e.StartDate).HasColumnType("date");

                entity.Property(e => e.SubmittedOn).HasColumnType("datetime");
            });

            modelBuilder.Entity<NsldsFAHType2>(entity =>
            {
                entity.Property(e => e.CurrentDOB).HasColumnType("date");
            });

            modelBuilder.Entity<ClientRequest>(entity =>
            {
               entity.Property(e => e.IsDeleted).HasDefaultValue(false);

               entity.Property(e => e.IsReceived).HasDefaultValue(false);

               entity.Property(e => e.IsSubmitted).HasDefaultValue(false);

               entity.Property(e => e.IsFailed).HasDefaultValue(false);

               entity.Property(e => e.IsOnHold).HasDefaultValue(false);

               entity.Property(e => e.IsTM).HasDefaultValue(false);

               entity.Property(e => e.ReceivedOn).HasColumnType("datetime");

               entity.Property(e => e.RevOn)
                   .HasColumnType("datetime")
                   .HasDefaultValueSql("getdate()");

               entity.Property(e => e.SubmittedOn).HasColumnType("datetime");

               entity.HasIndex(i => i.RevOn);
            });

            modelBuilder.Entity<Job>(entity =>
            {
                entity.Property(e => e.BilledOn).HasColumnType("datetime");

                entity.Property(e => e.IsBilled).HasDefaultValue(false);

                entity.Property(e => e.IsDeleted).HasDefaultValue(false);

                entity.Property(e => e.IsReceived).HasDefaultValue(false);

                entity.Property(e => e.IsSubmitted).HasDefaultValue(false);

                entity.Property(e => e.JobDate).HasColumnType("date");

                entity.Property(e => e.ReceivedOn).HasColumnType("datetime");

                entity.Property(e => e.RevOn)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("getdate()");

                entity.Property(e => e.SubmittedOn).HasColumnType("datetime");
            });

            modelBuilder.Entity<NsldsFAHType1>(entity =>
            {
                entity.Property(e => e.UndergradEligibilityUsed).HasColumnType("decimal(18,4)");
                entity.Property(e => e.LifeEligibilityUsed).HasColumnType("decimal(18,3)");
                entity.Property(e => e.SubUsagePeriod).HasColumnType("decimal(18,3)");
                entity.HasIndex(i => i.SulaFlag);
            });

            modelBuilder.Entity<NsldsFAHType4>(entity =>
            {
                entity.Property(e => e.EligibilityUsed).HasColumnType("decimal(18,4)");
                entity.Property(e => e.TotalEligibilityUsed).HasColumnType("decimal(18,4)");
            });

            modelBuilder.Entity<NsldsFAHType5>().HasIndex(i => i.LoanStatusCode);
        }

        public virtual DbSet<ClientProfile> ClientProfiles { get; set; }
        public virtual DbSet<ClientProfile_History> ClientProfiles_History { get; set; }
        public virtual DbSet<ClientRequestStudent> ClientRequestStudents { get; set; }
        public virtual DbSet<ClientRequestStudentAlert> ClientRequestStudentAlerts { get; set; }
        public virtual DbSet<ClientRequestStudent_History> ClientRequestStudents_History { get; set; }
        public virtual DbSet<ClientRequestAlert> ClientRequestAlerts { get; set; }
        public virtual DbSet<ClientRequest> ClientRequests { get; set; }
        public virtual DbSet<Job> Jobs { get; set; }
        public virtual DbSet<NsldsFAHType1> FahType1Recs { get; set; }
        public virtual DbSet<NsldsFAHType2> FahType2Recs { get; set; }
        public virtual DbSet<NsldsFAHType3> FahType3Recs { get; set; }
        public virtual DbSet<NsldsFAHType4> FahType4Recs { get; set; }
        public virtual DbSet<NsldsFAHType5> FahType5Recs { get; set; }
        #region Non-mapped DbSets
        // non mapped entities for sql statements & stored procedures
        // comment out before creating new migrations on client DBs
        //[NotMapped]
        //public virtual DbSet<StudentSearchDetail> StudentSearchDetails { get; set; }
        #endregion
    }
}
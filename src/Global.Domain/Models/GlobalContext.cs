using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Global.Domain
{

    public partial class GlobalContext : DbContext, IGlobalContext
	{
        public GlobalContext()
        {

        }

        public GlobalContext(DbContextOptions options) : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            //options.UseSqlServer(@"Data Source=.;Initial Catalog=NextGen_Global;Integrated Security=True;");
            //options.UseSqlServer(@"Data Source=dbt07p.gfasphx.local;Initial Catalog=NextGen_Global;Persist Security Info=True;User ID=NSLDS_APP_USER;Password=Global123;Integrated Security=False;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // configure the composite primary key field + code
            modelBuilder.Entity<FahCode>()
                .HasKey(f => new { f.FahFieldId, f.Code });

            modelBuilder.Entity<UserInvite>(entity =>
            {
                entity.Property(e => e.HasRegistered).HasDefaultValue(false);
            });
        }

        public virtual DbSet<FedSchoolCodeList> FedSchoolCodeList { get; set; }
        public virtual DbSet<Tenant> Tenants { get; set; }
        public virtual DbSet<FahField> FahFields { get; set; }
        public virtual DbSet<FahAlert> FahAlerts { get; set; }
        public virtual DbSet<FahCode> FahCodes { get; set; }
        public virtual DbSet<UserInvite> UserInvites { get; set; }
        public virtual DbSet<PellAward> PellAwards { get; set; }
    }
}
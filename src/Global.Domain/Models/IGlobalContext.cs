using Microsoft.EntityFrameworkCore;

namespace Global.Domain
{
	public interface IGlobalContext
	{
		DbSet<FahAlert> FahAlerts { get; set; }
		DbSet<FahCode> FahCodes { get; set; }
		DbSet<FahField> FahFields { get; set; }
		DbSet<FedSchoolCodeList> FedSchoolCodeList { get; set; }
		DbSet<PellAward> PellAwards { get; set; }
		DbSet<Tenant> Tenants { get; set; }
		DbSet<UserInvite> UserInvites { get; set; }
	}
}
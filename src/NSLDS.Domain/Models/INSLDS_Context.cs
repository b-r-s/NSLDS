using Microsoft.EntityFrameworkCore;

namespace NSLDS.Domain
{
	public interface INSLDS_Context
	{
		DbSet<ClientProfile> ClientProfiles { get; set; }
		DbSet<ClientProfile_History> ClientProfiles_History { get; set; }
		DbSet<ClientRequestAlert> ClientRequestAlerts { get; set; }
		DbSet<ClientRequest> ClientRequests { get; set; }
		DbSet<ClientRequestStudentAlert> ClientRequestStudentAlerts { get; set; }
		DbSet<ClientRequestStudent> ClientRequestStudents { get; set; }
		DbSet<ClientRequestStudent_History> ClientRequestStudents_History { get; set; }
		DbSet<NsldsFAHType1> FahType1Recs { get; set; }
		DbSet<NsldsFAHType2> FahType2Recs { get; set; }
		DbSet<NsldsFAHType3> FahType3Recs { get; set; }
		DbSet<NsldsFAHType4> FahType4Recs { get; set; }
		DbSet<NsldsFAHType5> FahType5Recs { get; set; }
		DbSet<Job> Jobs { get; set; }
	}
}
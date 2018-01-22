using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Global.Domain;
using NSLDS.Domain;
using NSLDS.Common;
using NSLDS.API;
using NSLDS.API.Controllers;
using Microsoft.EntityFrameworkCore;

namespace NSLDS.API.Tests.Common
{
	public class Fakes
	{
		private IGlobalContext fakeGlobalContext = null;
		private INSLDS_Context fakeNslds_Context = null;
		private IClientProfile fakeClientProfile = null;
		private IClientRequest fakeClientRequest = null;


	}

	public class FakeGlobalContext : DbContext, IGlobalContext
	{
		public FakeGlobalContext()
		{
		}

		// only used by "Scheduler" at this time 
		public FakeGlobalContext(DbContextOptions options) : base(options)
		{

		}

		public DbSet<FahAlert> FahAlerts { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public DbSet<FahCode> FahCodes { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public DbSet<FahField> FahFields { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public DbSet<FedSchoolCodeList> FedSchoolCodeList { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public DbSet<PellAward> PellAwards { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public DbSet<Tenant> Tenants { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public DbSet<UserInvite> UserInvites { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
	}

	public class FakeClientProfile : IClientProfile
	{
		public FakeClientProfile()
		{
		}

		public string Address1 { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public string Address2 { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public int AY_Definition { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public string City { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public ICollection<ClientRequest> ClientRequests { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public string Contact { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public string Email { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public bool Exits_Counseling { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public int Expiration { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public int Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public bool IsDeleted { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public bool IsPwdValid { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public ICollection<Job> Jobs { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public DateTime? LastPwdChanged { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public int Monitoring { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public string OPEID { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public string Organization_Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public string Phone { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public int Retention { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public string RevBy { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public DateTime? RevOn { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public string SAIG { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public string State { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public string TD_Password { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public string Upload_Method { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public string Zip { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
	}

	public class FakeNsldsContext : INSLDS_Context
	{
		public FakeNsldsContext()
		{
		}

		public DbSet<ClientProfile> ClientProfiles { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public DbSet<ClientProfile_History> ClientProfiles_History { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public DbSet<ClientRequestAlert> ClientRequestAlerts { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public DbSet<ClientRequest> ClientRequests { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public DbSet<ClientRequestStudentAlert> ClientRequestStudentAlerts { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public DbSet<ClientRequestStudent> ClientRequestStudents { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public DbSet<ClientRequestStudent_History> ClientRequestStudents_History { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public DbSet<NsldsFAHType1> FahType1Recs { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public DbSet<NsldsFAHType2> FahType2Recs { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public DbSet<NsldsFAHType3> FahType3Recs { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public DbSet<NsldsFAHType4> FahType4Recs { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public DbSet<NsldsFAHType5> FahType5Recs { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public DbSet<Job> Jobs { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
	}

	public class FakeClientRequest : IClientRequest
	{
		public FakeClientRequest()
		{
		}

		public ICollection<ClientRequestAlert> Alerts { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public ClientProfile ClientProfile { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public int Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public bool IsDeleted { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public bool IsFailed { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public bool IsOnHold { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public bool IsReceived { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public bool IsSubmitted { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public bool IsTM { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public Job Job { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public int Link2ClientProfile { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public int? Link2Job { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public DateTime? ReceivedOn { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public string Response { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public string RevBy { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public DateTime? RevOn { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public short Sequence { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public ICollection<ClientRequestStudent> Students { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public DateTime? SubmittedOn { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
	}
}

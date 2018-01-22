using System;
using System.Collections.Generic;

namespace NSLDS.Domain
{
	public interface IClientProfile
	{
		string Address1 { get; set; }
		string Address2 { get; set; }
		int AY_Definition { get; set; }
		string City { get; set; }
		ICollection<ClientRequest> ClientRequests { get; set; }
		string Contact { get; set; }
		string Email { get; set; }
		bool Exits_Counseling { get; set; }
		int Expiration { get; set; }
		int Id { get; set; }
		bool IsDeleted { get; set; }
		bool IsPwdValid { get; set; }
		ICollection<Job> Jobs { get; set; }
		DateTime? LastPwdChanged { get; set; }
		int Monitoring { get; set; }
		string OPEID { get; set; }
		string Organization_Name { get; set; }
		string Phone { get; set; }
		int Retention { get; set; }
		string RevBy { get; set; }
		DateTime? RevOn { get; set; }
		string SAIG { get; set; }
		string State { get; set; }
		string TD_Password { get; set; }
		string Upload_Method { get; set; }
		string Zip { get; set; }
	}
}
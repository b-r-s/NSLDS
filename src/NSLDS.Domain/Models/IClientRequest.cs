using System;
using System.Collections.Generic;

namespace NSLDS.Domain
{
	public interface IClientRequest
	{
		ICollection<ClientRequestAlert> Alerts { get; set; }
		ClientProfile ClientProfile { get; set; }
		int Id { get; set; }
		bool IsDeleted { get; set; }
		bool IsFailed { get; set; }
		bool IsOnHold { get; set; }
		bool IsReceived { get; set; }
		bool IsSubmitted { get; set; }
		bool IsTM { get; set; }
		Job Job { get; set; }
		int Link2ClientProfile { get; set; }
		int? Link2Job { get; set; }
		DateTime? ReceivedOn { get; set; }
		string Response { get; set; }
		string RevBy { get; set; }
		DateTime? RevOn { get; set; }
		short Sequence { get; set; }
		ICollection<ClientRequestStudent> Students { get; set; }
		DateTime? SubmittedOn { get; set; }
	}
}
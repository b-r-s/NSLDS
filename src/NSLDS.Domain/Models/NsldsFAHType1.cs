using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace NSLDS.Domain
{
    [Table("NsldsFAHType1", Schema = "nslds")]
    public partial class NsldsFAHType1
    {
        public NsldsFAHType1()
        {
        }

        public int Id { get; set; }
        public int Link2ClientRequestStudent { get; set; }
        public bool DefaultedLoan { get; set; }
        [MaxLength(1)]
        public string DischargedLoanCode { get; set; }
        public bool SatisfactoryRepayLoan { get; set; }
        public bool ActiveBankruptcy { get; set; }
        public decimal? SubPrincipalBal { get; set; }
        public decimal? UnSubPrincipalBal { get; set; }
        public decimal? CombinedPrincipalBal { get; set; }
        public decimal? SubPendingDisb { get; set; }
        public decimal? UnSubPendingDisb { get; set; }
        public decimal? CombinedPendingDisb { get; set; }
        public decimal? SubTotal { get; set; }
        public decimal? UnSubTotal { get; set; }
        public decimal? CombinedTotal { get; set; }
        public decimal? PerkinsTotalPrincipalBal { get; set; }
        public decimal? PerkinsCurrentAYDisb { get; set; }
        [MaxLength(1)]
        public string DirectStaffordMPN { get; set; }
        public bool? PellChange { get; set; }
        public bool? LoanChange { get; set; }
        public bool? AggregateChange { get; set; }
        [MaxLength(12)]
        public string FirstName { get; set; }
        [MaxLength(35)]
        public string LastName { get; set; }
        public DateTime DOB { get; set; }
        [MaxLength(1)]
        public string DirectLoanPlusMPN { get; set; }
        [MaxLength(1)]
        public string UnderGradSubLoanLimit { get; set; }
        [MaxLength(1)]
        public string UnderGradCombinedLoanLimit { get; set; }
        public decimal? UnAllocPrincipalBal { get; set; }
        public decimal? PlusLoanPrincipalBal { get; set; }
        public decimal? UnAllocPrincipalTotal { get; set; }
        public decimal? PlusLoanTotal { get; set; }
        public bool? ACGChange { get; set; }
        public bool? SMARTChange { get; set; }
        [MaxLength(1)]
        public string DirectLoanPlusGradMPN { get; set; }
        public bool Fraud { get; set; }
        public decimal? PlusProPrincipalBal { get; set; }
        public decimal? PlusProTotal { get; set; }
        [MaxLength(1)]
        public string GradSubLoanLimit { get; set; }
        [MaxLength(1)]
        public string GradCombinedLoanLimit { get; set; }
        public bool TeachGrantConverted { get; set; }
        public decimal? TeachLoanPrincipalBal { get; set; }
        public decimal? TeachLoanTotal { get; set; }
        public decimal? UndergradTeachDisbTotal { get; set; }
        public decimal? UndergradEligibilityUsed { get; set; }
        public decimal? UndergradRemainingAmount { get; set; }
        public decimal? GradTeachTotalDisb { get; set; }
        public decimal? GradEligibilityUsed { get; set; }
        public decimal? GradRemainingAmount { get; set; }
        public bool? TeachGrantDataChange { get; set; }
        public bool? TeachGrantChange { get; set; }
        public bool? TeachLoanChange { get; set; }
        public decimal? UndergradSubPrincipalBal { get; set; }
        public decimal? UndergradUnsubPrincipalBal { get; set; }
        public decimal? UndergradCombinedPrincipalBal { get; set; }
        public decimal? UndergradUnallocPrincipalBal { get; set; }
        public decimal? UndergradSubPendingDisb { get; set; }
        public decimal? UndergradUnsubPendingDisb { get; set; }
        public decimal? UndergradCombinedPendingDisb { get; set; }
        public decimal? UndergradSubTotal { get; set; }
        public decimal? UndergradUnsubTotal { get; set; }
        public decimal? UndergradCombinedTotal { get; set; }
        public decimal? UndergradUnallocTotal { get; set; }
        [MaxLength(4)]
        public string UndergradAY { get; set; }
        [MaxLength(1)]
        public string UndergradDependency { get; set; }
        public decimal? GradSubPrincipalBal { get; set; }
        public decimal? GradUnsubPrincipalBal { get; set; }
        public decimal? GradCombinedPrincipalBal { get; set; }
        public decimal? GradUnallocPrincipalBal { get; set; }
        public decimal? GradSubPendingDisb { get; set; }
        public decimal? GradUnsubPendingDisb { get; set; }
        public decimal? GradCombinedPendingDisb { get; set; }
        public decimal? GradSubTotal { get; set; }
        public decimal? GradUnsubTotal { get; set; }
        public decimal? GradCombinedTotal { get; set; }
        public decimal? GradUnallocTotal { get; set; }
        [MaxLength(4)]
        public string GradAY { get; set; }
        [MaxLength(1)]
        public string GradDependency { get; set; }
        public decimal? LifeEligibilityUsed { get; set; }
        [MaxLength(1)]
        public string UnusualEnrollHistory { get; set; }
        [MaxLength(1)]
        public string PellLifeLimit { get; set; }
        public bool? SulaFlag { get; set; }
        public decimal? SubUsagePeriod { get; set; }

        [IgnoreDataMember]
        [ForeignKey("Link2ClientRequestStudent")]
        public virtual ClientRequestStudent ClientRequestStudent { get; set; }
    }
}

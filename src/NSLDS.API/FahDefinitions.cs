using Global.Domain;
using NSLDS.Common;
using NSLDS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NSLDS.API
{
    /// <summary>
    /// All financial aid history calculations
    /// </summary>

    #region Eligibility remaining calculation engine

    // loan aggregate limit calculation class
    public class LoanAggregateLimitCalc
    {
        private bool _resolved = false;
        private string status;
        private decimal combinedbalance;
        private decimal combinedsub;
        private decimal u_combinedbalance;
        private decimal u_subbalance;
        private decimal u_unsubbalance;
        private decimal g_combinedbalance;
        private bool u_sublimit;
        private bool u_unsublimit;
        private bool u_combinedlimit;
        private bool g_combinedlimit;

        public decimal subdependent;
        public decimal subindependent;
        public decimal unsubdependent;
        public decimal unsubindependent;
        public decimal totaldependent;
        public decimal totalindependent;
        public decimal gradtotal;

        public string SubDependent
        {
            get
            { return (subdependent < 0) ? FahDef.Zero : $"{subdependent:C}"; }
        }
        public string SubIndependent
        {
            get
            { return (subindependent < 0) ? FahDef.Zero : $"{subindependent:C}"; }
        }
        public string UnsubDependent
        {
            get
            { return (unsubdependent < 0) ? FahDef.Zero : $"{unsubdependent:C}"; }
        }
        public string UnsubIndependent
        {
            get
            { return (unsubindependent < 0) ? FahDef.Zero : $"{unsubindependent:C}"; }
        }
        public string TotalDependent
        {
            get
            { return (totaldependent < 0) ? FahDef.Zero : $"{totaldependent:C}"; }
        }
        public string TotalIndependent
        {
            get
            { return (totalindependent < 0) ? FahDef.Zero : $"{totalindependent:C}"; }
        }
        public string GradTotal
        {
            get
            { return (gradtotal < 0) ? FahDef.Zero : $"{gradtotal:C}"; }
        }

        public LoanAggregateLimitCalc(NsldsFAHType1 type1, bool resolved)
        {
            // nslds-141: $0 eligibility when flags are present, even when resolved by user
            // datachk-192: when resolved by user, display actual agg limit amounts
            _resolved = resolved;
            if (FahDef.HasLoanOverpayment(type1) && !_resolved)
            {
                subdependent = 0;
                subindependent = 0;
                unsubdependent = 0;
                unsubindependent = 0;
                totaldependent = 0;
                totalindependent = 0;
                gradtotal = 0;
            }
            else
            {
                decimal subtotal1, subtotal2, subtotal3, subtotal4, subtotal5;
                // initialize status & limit codes
                status = type1.UndergradDependency;
                u_unsublimit = FahDef.loanCodesER.Contains(type1.UnderGradCombinedLoanLimit);
                u_sublimit = FahDef.loanCodesER.Contains(type1.UnderGradSubLoanLimit) || u_unsublimit;
                u_combinedlimit = u_unsublimit;
                g_combinedlimit = FahDef.loanCodesER.Contains(type1.GradCombinedLoanLimit);
                // initialize balances
                combinedbalance = type1.CombinedTotal ?? 0;
                combinedsub = type1.SubTotal ?? 0;
                u_combinedbalance = type1.UndergradCombinedTotal ?? 0;
                u_subbalance = type1.UndergradSubTotal ?? 0;
                u_unsubbalance = type1.UndergradUnsubTotal ?? 0;
                g_combinedbalance = (type1.SubTotal ?? 0) + (type1.UnSubTotal ?? 0);

                // subsidized dependent
                subtotal1 = 138500 - combinedbalance;
                subtotal2 = 65500 - combinedsub;
                subtotal3 = 31000 - u_combinedbalance;
                subtotal4 = 23000 - u_subbalance;
                if (subtotal1 <= 0) { subdependent = (status == FahDef.depStatus) ? subtotal1 : 0; }
                else if (subtotal2 <= 0) { subdependent = (status == FahDef.depStatus) ? subtotal2 : 0; }
                else if (subtotal3 <= 0) { subdependent = (status == FahDef.depStatus) ? subtotal3 : 0; }
                else { subdependent = new[] { subtotal1, subtotal2, subtotal3, subtotal4 }.Min(); }
                // subsidized independent
                subtotal1 = 138500 - combinedbalance;
                subtotal2 = 65500 - combinedsub;
                subtotal3 = 57500 - u_combinedbalance;
                subtotal4 = 23000 - u_subbalance;
                if (subtotal1 <= 0) { subindependent = subtotal1; }
                else if (subtotal2 <= 0) { subindependent = subtotal2; }
                else if (subtotal3 <= 0) { subindependent = subtotal3; }
                else if (subtotal4 <= 0) { subindependent = subtotal4; }
                else { subindependent = new[] { subtotal1, subtotal2, subtotal3, subtotal4 }.Min(); }
                // unsubsidized dependent
                subtotal1 = 138500 - combinedbalance;
                subtotal2 = 31000 - u_combinedbalance;
                subtotal3 = Math.Min(subtotal1, subtotal2);
                // datachk-192: unsub funding not affected by sub overpayment
                subtotal4 = (subdependent < 0) ? subtotal3 : subtotal3 - subdependent;
                if (subtotal4 <= 0) { unsubdependent = (status == FahDef.depStatus) ? subtotal4 : 0; }
                else { unsubdependent = subtotal4; }
                // unsubsidized independent
                subtotal1 = 138500 - combinedbalance;
                subtotal2 = 57500 - u_combinedbalance;
                subtotal3 = 34500 - u_unsubbalance;
                if (subtotal1 <= 0) { unsubindependent = subtotal1; }
                else if (subtotal2 <= 0) { unsubindependent = subtotal2; }
                else if (subtotal3 < 0)
                {
                    subtotal4 = 23000 - (u_subbalance + subtotal2);
                    subtotal5 = subtotal4 + subtotal3;
                    unsubindependent = new[] { subtotal1, subtotal2, subtotal5 }.Min();
                }
                else
                {
                    // nslds-136: add check for combined limit left
                    subtotal4 = new[] { subtotal1, subtotal2, subtotal3 }.Min();
                    subtotal5 = (subindependent < 0) ? subtotal4 : subtotal1 - subindependent;
                    unsubindependent = Math.Min(subtotal4, subtotal5);
                }
                // datachk-192: total funding not affected by overpayments
                subdependent = Math.Max(0, subdependent);
                subindependent = Math.Max(0, subindependent);
                unsubdependent = Math.Max(0, unsubdependent);
                unsubindependent = Math.Max(0, unsubindependent);
                // total dependent
                //subtotal1 = Math.Min(subdependent, unsubdependent);
                //subtotal2 = subdependent + unsubdependent;
                //totaldependent = (subtotal1 < 0) ? subtotal1 : subtotal2;
                totaldependent = subdependent + unsubdependent;
                // total independent
                //subtotal1 = Math.Min(subindependent, unsubindependent);
                //subtotal2 = subindependent + unsubindependent;
                //totalindependent = (subtotal1 < 0) ? subtotal1 : subtotal2;
                totalindependent = subindependent + unsubindependent;
                // graduate total
                gradtotal = 138500 - combinedbalance;
                // gradtotal = (subindependent <= 0) ? subtotal1 : subtotal1 - subindependent;
                // reconcile overpayment eligibility using nslds limit flags
                subdependent = (subdependent < 0 && !u_sublimit) ? 0 : subdependent;
                subindependent = (subindependent < 0 && !u_sublimit) ? 0 : subindependent;
                unsubdependent = (unsubdependent < 0 && !u_unsublimit) ? 0 : unsubdependent;
                unsubindependent = (unsubindependent < 0 && !u_unsublimit) ? 0 : unsubindependent;
                totaldependent = (totaldependent < 0 && !u_combinedlimit) ? 0 : totaldependent;
                totalindependent = (totalindependent < 0 && !u_combinedlimit) ? 0 : totalindependent;
                gradtotal = (gradtotal < 0 && !g_combinedlimit) ? 0 : gradtotal;
            }
        }
    }

    #endregion

    // miscellaneous FAH definitions & calculations
    public static class FahDef
    {
        #region Private fields and functions

        private struct AY
        {
            public int Year;
            public string Display;
            public decimal Value;
            public decimal MaxAmount;
            public decimal Amount;
        }

        #endregion

        #region Public static methods & fields

        // Alert validation constants
        public const string
            a1 = "1", a2 = "2", a3 = "3", a4 = "4", a5 = "5", a6 = "6",
            a7 = "7", a8 = "8", a9 = "9", a10 = "10", a11 = "11", a12 = "12",
            a13 = "13", a14 = "14", a15 = "15", a16 = "16", a17 = "17";
        public const string depStatus = "D";
        public const string indStatus = "I";
        public const string IAStatus = "IA";
        public const string CAStatus = "CA";
        public const string VAStatus = "VA";
        // other funding types
        public const string perkins = "Perkins";
        public const string parentplus = "Parent PLUS";
        public const string graduateplus = "Graduate PLUS";
        public const string teachloan = "TEACH Loan";
        public static readonly string[] grantCodes = { "AG", "SG", "TG" };
        public static readonly string[] grantFlags = { "F", "Y" };
        public const string grantPE = "PE";
        public const string grantAG = "AG";
        public const string grantSG = "SG";
        public const string grantTG = "TG";
        public const string loanD0 = "D0";
        public const string loanD8 = "D8";
        // other constants
        public const string Default = "DEFAULT";
        public const string Fraud = "FRAUD";
        public const string Death = "DEATH";
        public const string Discharged = "DISCHARGED";
        public const string Overpay = "OVERPAYMENT";
        public const string LoanOverpay = "LOAN OVERPAYMENT";
        public const string GrantOverpay = "GRANT OVERPAYMENT";
        public const string AddUnsub = "ADDITIONAL UNSUB";
        public const string ConUnalloc = "CONSOLIDATION UNALLOCATED";
        // loan codes and types
        public static readonly string[] loanDefault =
            { "DZ", "DB", "DF", "DL", "DO", "DT", "DU", "DW" };
        public static readonly string[] loanDischarged =
            { "OD", "DK", "DI", "CS", "DS" };
        public static readonly string[] loanFraud = { "FR" };
        public static readonly string[] loanDeath = { "DD", "DE" };
        public static readonly string[] loanCodesER = { "E", "R" };
        public static readonly string[] loanCodes = { "E", "C", "R" };
        public static readonly string[] loanCodesDischarged = { "D", "C", "P", "M" };
        public static readonly string[] leuCodes = { "E", "C", "H" };
        public static readonly string[] loanTypes = { "D0", "D1", "D2", "D8", "SU", "SF" };
        public static readonly string[] loanSubTypes = { "D0", "D1", "SF" };
        public static readonly string[] loanUnsubTypes = { "SU", "D2", "D8" };
        public static readonly string[] loanConsolidationTypes = { "CL", "D5", "D6" };
        public static readonly string[] loanFlags = { "H", "P", "B" };
        public static readonly string[] undergradLevels = { "1", "2", "3", "4", "5" }; // others Grad
        // define all the FAH constants
        public static string NoFah = "No Financial Aid History Available";
        public static string NA = "N/A";
        public static string N = "N";
        public static string Y = "Y";
        public static string No = "No";
        public static string Yes = "Yes";
        public static string Zero = $"{0:C}";
        public static string Reaffirmation = "R";
        // FAH constant virtual fields for grade level tables
        public static string GradeLevel = "Grade Level";
        public static string Subsidized = "Subsidized";
        public static string Unsubsidized = "Unsubsidized";
        public static string LoanType = "Loan Type";
        public static string ApprovedAmount = "Approved Amount";
        public static string PendingDisbAmount = "Pending Disb. Amount";
        public static string DisbursedAmount = "Disbursed Amount";
        public static string PeriodDates = "Loan Period Dates";
        public static string AYDates = "Academic Year Dates";
        public static string Servicer = "Servicer";
        public static string School = "School";
        public static string UnusualEnrollment = "UnusualEnrollment";

        public static string DateOrNA(DateTime? item)
        {
            return (item.HasValue) ? item.Value.ToShortDateString() : NA;
        }

        public static string CodeOrNA(string code)
        {
            return (code == N) ? NA : code;
        }

        public static string YOrN(bool? code)
        {
            return (code == true) ? Y : N;
        }

        // calculate a past date excluding weekends
        public static DateTime ValidFrom(DateTime date, int days)
        {
            var expdate = date.AddDays(0);

            while (days > 0)
            {
                expdate = expdate.AddDays(-1);
                if (expdate.DayOfWeek < DayOfWeek.Saturday &&
                    expdate.DayOfWeek > DayOfWeek.Sunday)
                    days--;
            }
            return expdate;
        }

        public static DateTime ValidUntil(DateTime date, int days)
        {
            var expdate = date.AddDays(0);

            while (days > 0)
            {
                expdate = expdate.AddDays(1);
                if (expdate.DayOfWeek < DayOfWeek.Saturday &&
                    expdate.DayOfWeek > DayOfWeek.Sunday)
                    days--;
            }
            return expdate;
        }

        public static string IsValidUntil(DateTime? date, int days)
        {
            if (!date.HasValue) { return NA; }

            // NSLDS-149: expiration date exclude weekends and starts on the received date
            return ValidUntil(date.Value, days).ToShortDateString();
        }

        /* DATACHK-158
        Grant overpayment= show loans amounts, $0 pell when resolved.
        Grant and loan overpayment= show $0 on both loans and pell when resolved.
        Loan overpayment = show Pell amounts, $0 loans when resolved.
        Default, fraud, death = Show both pell and loan amounts when resolved.
        Default, fraud or death combined with a loan overpayment= show Pell amounts, $0 loans.
        Default, fraud or death combined with a Pell overpayment= show loan amounts, $0 pell.
         */
        public static bool HasOverpayment(string loanflag)
        {
            return (loanflag == null) ? false : loanflag.Contains(LoanOverpay) || loanflag.Contains(GrantOverpay);
        }

        public static bool HasLoanOverpayment(string loanflag)
        {
            return (loanflag == null) ? false : loanflag.Contains(LoanOverpay);
        }

        public static bool HasLoanOverpayment(NsldsFAHType1 type1)
        {
            return loanCodesER.Contains(type1.UnderGradCombinedLoanLimit)
                || loanCodesER.Contains(type1.UnderGradSubLoanLimit)
                || loanCodesER.Contains(type1.GradCombinedLoanLimit);
        }

        public static bool HasPellFlag(List<NsldsFAHType3> grantflags)
        {
            return grantflags
                .Any(x => grantFlags.Contains(x.Overpayment) && x.OverpaymentType == grantPE);
        }

        public static string DeathOrDefault(List<NsldsFAHType5> type5, NsldsFAHType1 type1, bool grantflag)
        {
            List<string> exception = new List<string>();

            // NSLDS-141: check loan combined limit flags for overpayment
            var loanoverpay = loanCodesER.Contains(type1.UnderGradCombinedLoanLimit)
                || loanCodesER.Contains(type1.UnderGradSubLoanLimit)
                || loanCodesER.Contains(type1.GradCombinedLoanLimit);

            // loan flags
            if (type5.Any(x => loanDefault.Contains(x.LoanStatusCode)))
            { exception.Add(Default); }
            if (type1.Fraud || type5.Any(x => loanFraud.Contains(x.LoanStatusCode)))
            { exception.Add(Fraud); }
            if (type5.Any(x => loanDeath.Contains(x.LoanStatusCode)))
            { exception.Add(Death); }

            // loan overpayment flag
            if (loanoverpay) { exception.Add(LoanOverpay); }

            //grant overpayment flag
            if (grantflag) { exception.Add(GrantOverpay); }

            return (exception.Count > 0) ? string.Join(", ", exception.Distinct()) : null;
        }

        public static string Exceptions(NsldsFAHType1 type1, List<NsldsFAHType5> type5, bool grantflag)
        {
            List<string> exception = new List<string>();

            // NSLDS-141: check loan combined limit flags for overpayment
            var loanoverpay = loanCodesER.Contains(type1.UnderGradCombinedLoanLimit)
                || loanCodesER.Contains(type1.UnderGradSubLoanLimit)
                || loanCodesER.Contains(type1.GradCombinedLoanLimit);

            bool isover = type1.UndergradDependency == depStatus &&
                type1.UndergradCombinedTotal > 31000;

            // loan flags
            if (type5.Any(x => loanDefault.Contains(x.LoanStatusCode)))
            { exception.Add(Default); }
            if (type1.Fraud || type5.Any(x => loanFraud.Contains(x.LoanStatusCode)))
            { exception.Add(Fraud); }
            if (type5.Any(x => loanDeath.Contains(x.LoanStatusCode)))
            { exception.Add(Death); }
            if (isover && type5.Any(x => loanFlags.Contains(x.AdditionalUnsubLoan)))
            { exception.Add(AddUnsub); }

            // consolidation flag
            if (type1.UndergradUnallocTotal > 0 || type1.GradUnallocTotal > 0)
            { exception.Add(ConUnalloc); }

            // loan overpayment flag
            if (loanoverpay) { exception.Add(LoanOverpay); }

            //grant flag
            if (grantflag) { exception.Add(GrantOverpay); }

            return (exception.Count > 0) ? string.Join(", ", exception) : null;
        }

        // loan grade level tables

        public static string CalcUndergradSubLoanGradeLevel(int level, decimal amount, bool resolved, string loanflag)
        {
            // nslds 141: if not resolved return warnings
            if (!resolved) { return loanflag; }
            // datachk-192: when resolved by user, display actual agg limit amounts
            // else if (HasLoanOverpayment(loanflag)) { return Zero; }

            decimal bal = 0;
            switch (level)
            {
                case 1:
                    bal = Math.Min(3500, amount);
                    break;
                case 2:
                    bal = Math.Min(4500, amount);
                    break;
                case 3:
                case 4:
                case 5:
                    bal = Math.Min(5500, amount);
                    break;
            }
            return (bal < 0) ? Zero : $"{bal:C}";
        }

        public static string CalcUndergradUnsubLoanDepGradeLevel(int level, decimal subdep, decimal unsubdep, bool resolved, string loanflag)
        {
            // nslds 141: if not resolved return warnings
            if (!resolved) { return loanflag; }
            // datachk-192: when resolved by user, display actual agg limit amounts
            // else if (HasLoanOverpayment(loanflag)) { return Zero; }

            // datachk-157: move amounts for combined aggregate limits
            decimal maxdep = subdep + unsubdep;
            decimal sub = 0;
            decimal bal = 0;

            switch (level)
            {
                case 1:
                    sub = Math.Min(3500, subdep);
                    bal = Math.Min(5500 - sub, maxdep - sub);
                    break;
                case 2:
                    sub = Math.Min(4500, subdep);
                    bal = Math.Min(6500 - sub, maxdep - sub);
                    break;
                case 3:
                case 4:
                case 5:
                    sub = Math.Min(5500, subdep);
                    bal = Math.Min(7500 - sub, maxdep - sub);
                    break;
            }
            return (bal < 0) ? Zero : $"{bal:C}";
        }

        public static string CalcUndergradUnsubLoanIndGradeLevel(int level, decimal subind, decimal unsubind, bool resolved, string loanflag)
        {
            // nslds 141: if not resolved return warnings
            if (!resolved) { return loanflag; }
            // datachk-192: when resolved by user, display actual agg limit amounts
            // else if (HasLoanOverpayment(loanflag)) { return Zero; }

            // datachk-157: move amounts for combined aggregate limits
            decimal maxind = subind + unsubind;
            decimal sub = 0;
            decimal bal = 0;

            switch (level)
            {
                case 1:
                    sub = Math.Min(3500, subind);
                    bal = Math.Min(9500 - sub, maxind - sub);
                    break;
                case 2:
                    sub = Math.Min(4500, subind);
                    bal = Math.Min(10500 - sub, maxind - sub);
                    break;
                case 3:
                case 4:
                case 5:
                    sub = Math.Min(5500, subind);
                    bal = Math.Min(12500 - sub, maxind - sub);
                    break;
            }
            bal = Math.Min(maxind, bal);
            return (bal < 0) ? Zero : $"{bal:C}";
        }

        public static string CalcGradUnsubLoanGradeLevel(decimal? sub, decimal? unsub, bool resolved, string loanflag)
        {
            // nslds 141: if not resolved return warnings
            if (!resolved) { return loanflag; }
            // datachk-192: when resolved by user, display actual agg limit amounts
            // else if (HasLoanOverpayment(loanflag)) { return Zero; }

            //if (sub == null && unsub == null) { bal = 0; }
            //else { bal = 138500 - ((sub ?? 0) + (unsub ?? 0)); }
            decimal bal = 138500 - ((sub ?? 0) + (unsub ?? 0));
            bal = Math.Min(20500, bal);
            return (bal < 0) ? Zero : $"{bal:C}";
        }

        // open loan grade level tables

        public static string CalcUndergradSubOpenLoanDepGradeLevel(int level, decimal subbal, decimal unsubbal, decimal max, decimal gradbal, bool resolved, string loanflag)
        {
            // nslds 141: if not resolved return warnings
            if (!resolved) { return loanflag; }
            // datachk-192: when resolved by user, display actual agg limit amounts
            // else if (HasLoanOverpayment(loanflag)) { return Zero; }

            // datachk-190: check unsub open loans for funds already moved from sub to unsub
            decimal totalbal = 20500 - (subbal + unsubbal + gradbal); // if currently in grad open AY
            decimal unsub = (unsubbal > 2000) ? unsubbal - 2000 : 0;
            decimal bal = 0;
            switch (level)
            {
                case 1:
                    bal = 3500 - (subbal + unsub);
                    break;
                case 2:
                    bal = 4500 - (subbal + unsub);
                    break;
                case 3:
                case 4:
                case 5:
                    bal = 5500 - (subbal + unsub);
                    break;
            }
            bal = new[] { bal, max, totalbal }.Min();
            return (bal < 0) ? Zero : $"{bal:C}";
        }

        public static string CalcUndergradSubOpenLoanIndGradeLevel(int level, decimal subbal, decimal unsubbal, decimal max, decimal gradbal, bool resolved, string loanflag)
        {
            // nslds 141: if not resolved return warnings
            if (!resolved) { return loanflag; }
            // datachk-192: when resolved by user, display actual agg limit amounts
            // else if (HasLoanOverpayment(loanflag)) { return Zero; }

            // datachk-190: check unsub open loans for funds already moved from sub to unsub
            decimal totalbal = 20500 - (subbal + unsubbal + gradbal); // if currently in grad open AY
            decimal unsub = 0;
            decimal bal = 0;
            switch (level)
            {
                case 1:
                    unsub = (unsubbal > 6000) ? unsubbal - 6000 : 0;
                    bal = 3500 - (subbal + unsub);
                    break;
                case 2:
                    unsub = (unsubbal > 6000) ? unsubbal - 6000 : 0;
                    bal = 4500 - (subbal + unsub);
                    break;
                case 3:
                case 4:
                case 5:
                    unsub = (unsubbal > 7000) ? unsubbal - 7000 : 0;
                    bal = 5500 - (subbal + unsub);
                    break;
            }
            bal = new[] { bal, max, totalbal }.Min();
            return (bal < 0) ? Zero : $"{bal:C}";
        }

        public static string CalcUndergradUnsubOpenLoanDepGradeLevel(int level, decimal subbal, decimal unsubbal, decimal submax, decimal max, decimal gradbal, bool resolved, string loanflag)
        {
            // nslds 141: if not resolved return warnings
            if (!resolved) { return loanflag; }
            // datachk-192: when resolved by user, display actual agg limit amounts
            // else if (HasLoanOverpayment(loanflag)) { return Zero; }

            // datachk-190: remove unsub dependency, move money from remaining sub to unsub
            decimal sub = 0;
            decimal unsub = (unsubbal > 2000) ? unsubbal - 2000 : 0;
            decimal totalbal = subbal + unsubbal + gradbal; // total oay disbursed
            decimal totalgrad = 20500 - totalbal; // if currently in grad open AY
            decimal bal = 0;
            switch (level)
            {
                case 1:
                    sub = Math.Min(submax, 3500 - (subbal + unsub));
                    bal = 5500 - (subbal + unsubbal + Math.Max(0, sub));
                    break;
                case 2:
                    sub = Math.Min(submax, 4500 - (subbal + unsub));
                    bal = 6500 - (subbal + unsubbal + Math.Max(0, sub));
                    break;
                case 3:
                case 4:
                case 5:
                    sub = Math.Min(submax, 5500 - (subbal + unsub));
                    bal = 7500 - (subbal + unsubbal + Math.Max(0, sub));
                    break;
            }
            bal = new[] { bal, (max - sub), (totalgrad - sub) }.Min();
            return (bal < 0) ? Zero : $"{bal:C}";
        }

        public static string CalcUndergradUnsubOpenLoanIndGradeLevel(int level, decimal subbal, decimal unsubbal, decimal submax, decimal max, decimal gradbal, bool resolved, string loanflag)
        {
            // nslds 141: if not resolved return warnings
            if (!resolved) { return loanflag; }
            // datachk-192: when resolved by user, display actual agg limit amounts
            // else if (HasLoanOverpayment(loanflag)) { return Zero; }

            // datachk-190: remove unsub dependency, move money from remaining sub to unsub
            decimal sub = 0;
            decimal unsub = 0;
            decimal totalbal = subbal + unsubbal + gradbal; // total oay disbursed
            decimal totalgrad = 20500 - totalbal; // if currently in grad open AY
            decimal bal = 0;
            switch (level)
            {
                case 1:
                    unsub = (unsubbal > 6000) ? unsubbal - 6000 : 0;
                    sub = Math.Min(submax, 3500 - (subbal + unsub));
                    bal = 9500 - (subbal + unsubbal + Math.Max(0, sub));
                    break;
                case 2:
                    unsub = (unsubbal > 6000) ? unsubbal - 6000 : 0;
                    sub = Math.Min(submax, 4500 - (subbal + unsub));
                    bal = 10500 - (subbal + unsubbal + Math.Max(0, sub));
                    break;
                case 3:
                case 4:
                case 5:
                    unsub = (unsubbal > 7000) ? unsubbal - 7000 : 0;
                    sub = Math.Min(submax, 5500 - (subbal + unsub));
                    bal = 12500 - (subbal + unsubbal + Math.Max(0, sub));
                    break;
            }
            bal = new[] { bal, (max - sub), (totalgrad - sub) }.Min();
            return (bal < 0) ? Zero : $"{bal:C}";
        }

        public static string CalcGradUnsubOpenLoanGradeLevel(int level, decimal total, decimal gradbal, bool resolved, string loanflag)
        {
            // nslds 141: if not resolved return warnings
            if (!resolved) { return loanflag; }
            // datachk-192: when resolved by user, display actual agg limit amounts
            // else if (HasLoanOverpayment(loanflag)) { return Zero; }

            // datachk-190: don't go over total combined limits in OAY
            // datachk-232: fix OAY eligibility bug for graduate students
            decimal gradmax = 20500;
            decimal bal = 0;
            switch (level)
            {
                case 6:
                case 7:
                    bal = Math.Min(total, gradmax - gradbal);
                    break;
            }
            return (bal < 0) ? Zero : $"{bal:C}";
        }

        public static string CalcOpenAY(DateTime? date, List<NsldsFAHType5> loans, int ayweeks)
        {
            var result = NA;
            if (date.HasValue)
            {
                result = N;
                bool isopen = false;
                DateTime start = date.Value;
                var ayloans = loans.Where(x => start >= (x.AYBeginDate ?? x.LoanPeriodBeginDate));
                foreach (var item in ayloans)
                {
                    DateTime aybegin, ayend;

                    if (item.AYBeginDate.HasValue && item.AYEndDate.HasValue)
                    {
                        int minDays = 26 * 7; // minimum 26 weeks for AY period
                        int days = (item.AYEndDate - item.AYBeginDate).Value.Days;
                        if (days < minDays)
                        {
                            aybegin = item.LoanPeriodBeginDate.Value;
                            ayend = aybegin.AddDays(minDays);
                        }
                        else
                        {
                            aybegin = item.AYBeginDate.Value;
                            ayend = item.AYEndDate.Value;
                        }
                    }
                    else
                    {
                        aybegin = item.LoanPeriodBeginDate.Value;
                        ayend = aybegin.AddDays(ayweeks * 7);
                    }
                    if (start <= ayend) { isopen = true; break; }
                }
                if (isopen) { result = Y; }
            }
            return result;
        }

        public static string CalcSulaUsage(decimal? usage)
        {
            if (usage == null) { return null; }
            return $"{Math.Round(usage.Value, 1, MidpointRounding.AwayFromZero):F1}";
        }

        public static string CalcLeuRemain(decimal percentused, bool resolved, string loanflag)
        {
            // nslds-141: return warning flags if not resolved
            if (!resolved) { return loanflag; }

            decimal remain = 600 - percentused;
            return (remain < 0) ? $"{0:F4}%" : $"{remain:F4}%";
        }

        /// <summary>
        /// Function to calculate Pell award year eligibility
        /// </summary>
        /// <param name="request"></param>
        /// <param name="type1"></param>
        /// <param name="grants"></param>
        /// <param name="pellflag"></param>
        /// <param name="resolved"></param>
        /// <param name="fp"></param>
        /// <param name="ay1amount"></param>
        /// <param name="ay2amount"></param>
        /// <param name="ay1tentative"></param>
        /// <param name="ay2tentative"></param>
        /// <param name="ay1addamount"></param>
        /// <param name="ay2addamount"></param>
        /// <param name="ay1addtentative"></param>
        /// <param name="ay2addtentative"></param>
        /// <param name="apply1"></param>
        /// <param name="apply2"></param>
        /// <returns></returns>
        public static List<AwardYear> CalcAnnualAwards(ClientRequestStudent request, NsldsFAHType1 type1, List<NsldsFAHType4> grants, bool pellflag, bool resolved, List<PellAward> fp, decimal? ay1amount, decimal? ay2amount, decimal? ay1tentative, decimal? ay2tentative, decimal? ay1addamount, decimal? ay2addamount, decimal? ay1addtentative, decimal? ay2addtentative, bool apply1 = false, bool apply2 = false)
        {
            /*
            Create award year table that defines beginning and end date of award year.  
            Example: 7/1/15- 6/30/16 will always be 15/16.
            (2) Determine AY1 by current date of request to award year table.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       (3) Current award year is active from 7/1/XX- 9/30/YY.  If start date or current date (if no start date) is before 7/1/XX and request is 10/1/YY or after, pull AY1 as current open award year, plus 1 for AY2.
            it states it needs to be active until 9/30/yy of that award year 
            before moving on to the next award year
            */
            var result = new List<AwardYear>();
            // datachk-187: additional eligibility percent
            AY ay1, ay2, ay1add, ay2add;
            bool hasay1add, hasay2add;

            var reqdate = request.SubmittedOn.Value;
            var startdate = request.StartDate ?? request.ReceivedOn.Value;
            // determine the 2 award years based on date of current request
            if (reqdate.Month < 10) // before October 1st
            {
                ay1.Year = reqdate.Year;
                ay1.Display = $"{reqdate.AddYears(-1):yy}/{reqdate:yy}";
                ay2.Year = reqdate.AddYears(1).Year;
                ay2.Display = $"{reqdate:yy}/{reqdate.AddYears(1):yy}";
            }
            else
            {
                ay1.Year = reqdate.AddYears(1).Year;
                ay1.Display = $"{reqdate:yy}/{reqdate.AddYears(1):yy}";
                ay2.Year = reqdate.AddYears(2).Year;
                ay2.Display = $"{reqdate.AddYears(1):yy}/{reqdate.AddYears(2):yy}";
            }
            // calculate eligibility for both award years
            ay1.Value = 0;
            ay1.Amount = 0;
            ay1.MaxAmount = fp.SingleOrDefault(x => x.AwardYear == ay1.Year)?.MaxAmount ?? 0;
            ay2.Value = 0;
            ay2.Amount = 0;
            ay2.MaxAmount = fp.SingleOrDefault(x => x.AwardYear == ay2.Year)?.MaxAmount ?? 0;

            // datachk-187: retrieve and calculate additional eligibility
            ay1add.Year = ay1.Year;
            ay1add.Display = ay1.Display;
            ay1add.Value = fp.SingleOrDefault(x => x.AwardYear == ay1add.Year)?.AdditionalPercent ?? 0;
            hasay1add = ay1add.Value > 0;
            ay1add.Amount = 0;
            ay1add.MaxAmount = ay1.MaxAmount;
            ay2add.Year = ay2.Year;
            ay2add.Display = ay2.Display;
            ay2add.Value = fp.SingleOrDefault(x => x.AwardYear == ay2add.Year)?.AdditionalPercent ?? 0;
            hasay2add = ay2add.Value > 0;
            ay2add.Amount = 0;
            ay2add.MaxAmount = ay2.MaxAmount;

            // assign initial tentative amounts max or tentative1/2 parameter values
            if (ay1tentative.HasValue) { ay1.MaxAmount = Math.Min(ay1tentative.Value, ay1.MaxAmount); }
            if (ay2tentative.HasValue) { ay2.MaxAmount = Math.Min(ay2tentative.Value, ay2.MaxAmount); }
            // datachk-187: assign additional tentative amounts max or add. tentative1/2 parameter values
            if (ay1addtentative.HasValue) { ay1add.MaxAmount = new[] { ay1addtentative.Value, ay1add.MaxAmount, ay1.MaxAmount }.Min(); }
            if (ay2addtentative.HasValue) { ay2add.MaxAmount = new[] { ay2addtentative.Value, ay2add.MaxAmount, ay2.MaxAmount }.Min(); }

            // calculate only if no warning flag or resolved by user
            if (resolved && !pellflag)
            {
                // nslds-137: revert to using type1 leu due to possible duplicates records from nslds
                //var LEU1 = grants.Where(x => x.GrantType == grantPE).Sum(x => x.EligibilityUsed) ?? 0;
                // compare with NSLDS LEU and use the greater percentage
                // some older grants are not provided by NSLDS
                var LEU2 = type1.LifeEligibilityUsed ?? 0;
                // if type1.LEU is used add 9 as 4th decimal
                if (LEU2 > 0) { LEU2 += 0.0009M; }
                var LEU = LEU2; //Math.Max(LEU1, LEU2);
                var remainLEU = 600 - LEU;
                var EUay1 = grants.Where(x => x.GrantType == grantPE && x.AwardYear == ay1.Year.ToString())
                    .Sum(x => x.EligibilityUsed) ?? 0;
                var EUay2 = grants.Where(x => x.GrantType == grantPE && x.AwardYear == ay2.Year.ToString())
                    .Sum(x => x.EligibilityUsed) ?? 0;
                var remainay1 = 100 - EUay1;
                var remainay2 = 100 - EUay2;

                if (LEU <= 400)
                {
                    ay1.Value = (LEU < 0) ? LEU : remainay1;
                    ay2.Value = (LEU < 0) ? LEU : remainay2;
                }
                else if (LEU < 500)
                {
                    ay1.Value = Math.Min(remainay1, remainLEU - (EUay1 + EUay2));
                    ay2.Value = Math.Min(remainay2, remainLEU - ay1.Value);
                }
                else
                {
                    ay1.Value = Math.Min(remainay1, (remainLEU > remainay1) ? remainay1 : remainLEU);
                    var leuay2 = (remainLEU - ay1.Value < 0) ? 0 : remainLEU - ay1.Value;
                    ay2.Value = Math.Min(remainay2, (leuay2 > remainay2) ? remainay2 : leuay2);
                }

                // optional recalculation based on different tentative amounts
                // case 1: amount1 = 0 -> move ay1 eligibility to ay2 (max 100%)
                if (ay1.Value > 0 && ay1amount.HasValue && ay1amount.Value <= ay1.MaxAmount)
                {
                    var neway1 = (ay1.MaxAmount == 0) ? 0 : ay1amount.Value / ay1.MaxAmount * 100;
                    if (neway1 < ay1.Value)
                    {
                        ay2.Value = Math.Min(ay2.Value + ay1.Value - neway1, remainay2);
                        ay1.Value = neway1;
                    }
                }

                // datachk-187:  apply eligible AY1 additional % if not enough AY2 remaining
                decimal remainAY1 = 0, remainAY2 = 0;

                // if (apply1 && remainLEU - ay1.Value < remainay2)
                // datachk-228: apply AY1 additional eligibility % before AY2 eligibility
                if (apply1 && remainLEU > ay1.Value)
                {
                    remainAY1 = remainLEU - ay1.Value;
                    ay1add.Value = Math.Min(ay1add.Value, remainAY1);
                    // optional additional amount recalc
                    if (ay1add.Value > 0 && ay1addamount.HasValue && ay1addamount.Value <= ay1add.MaxAmount)
                    {
                        var neway1add = (ay1add.MaxAmount == 0) ? 0 : ay1addamount.Value / ay1add.MaxAmount * 100;
                        if (neway1add < ay1add.Value)
                        {
                            ay1add.Value = neway1add;
                        }
                    }
                    ay2.Value = Math.Min(ay2.Value, remainAY1 - ay1add.Value);
                    // amount2 calculation
                    if (ay2.Value > 0 && ay2amount.HasValue && ay2amount.Value <= ay2.MaxAmount)
                    {
                        var neway2 = (ay2.MaxAmount == 0) ? 0 : ay2amount.Value / ay2.MaxAmount * 100;
                        if (neway2 < ay2.Value)
                        {
                            ay2.Value = neway2;
                        }
                    }
                    remainAY2 = remainAY1 - (ay1add.Value + ay2.Value);
                }
                else
                {
                    // amount2 calculation
                    if (ay2.Value > 0 && ay2amount.HasValue && ay2amount.Value <= ay2.MaxAmount)
                    {
                        var neway2 = (ay2.MaxAmount == 0) ? 0 : ay2amount.Value / ay2.MaxAmount * 100;
                        if (neway2 < ay2.Value)
                        {
                            ay2.Value = neway2;
                        }
                    }
                    // datachk-187: calculate additional eligibility
                    remainAY1 = Math.Max(0, remainLEU - (ay1.Value + ay2.Value));
                    ay1add.Value = (apply1) ? Math.Min(ay1add.Value, remainAY1) : 0;

                    // optional recalculation based on different tentative amounts
                    // case 1: amount1 = 0 -> move ay1 eligibility to ay2 (max 100%)
                    if (ay1add.Value > 0 && ay1addamount.HasValue && ay1addamount.Value <= ay1add.MaxAmount)
                    {
                        var neway1add = (ay1add.MaxAmount == 0) ? 0 : ay1addamount.Value / ay1add.MaxAmount * 100;
                        if (neway1add < ay1add.Value)
                        {
                            ay1add.Value = neway1add;
                        }
                    }
                    remainAY2 = Math.Max(0, remainAY1 - ay1add.Value);
                }

                ay2add.Value = (apply2) ? Math.Min(ay2add.Value, remainAY2) : 0;
                // amount2 calculation
                if (ay2add.Value > 0 && ay2addamount.HasValue && ay2addamount.Value <= ay2add.MaxAmount)
                {
                    var neway2add = (ay2add.MaxAmount == 0) ? 0 : ay2addamount.Value / ay2add.MaxAmount * 100;
                    if (neway2add < ay2add.Value)
                    {
                        ay2add.Value = neway2add;
                    }
                }
                // final eligibility amounts
                // datachk-216: eligible amounts cannot be negative
                ay1.Amount = Math.Max(0, ay1.MaxAmount * ay1.Value / 100);
                ay2.Amount = Math.Max(0, ay2.MaxAmount * ay2.Value / 100);
                ay1add.Amount = Math.Max(0, ay1add.MaxAmount * ay1add.Value / 100);
                ay2add.Amount = Math.Max(0, ay2add.MaxAmount * ay2add.Value / 100);
            }

            result.Add(new AwardYear
            {
                AYear = ay1.Display,
                TentativeAmount = ay1.MaxAmount,
                RevisedAmount = ay1amount,
                EligiblePercent = (ay1.Value < 0) ? $"{0:F4}%" : $"{ay1.Value:F4}%",
                EligibleAmount = $"{ay1.Amount:F4}",
                AdditionalEligibility = (hasay1add) ? new AdditionalEligibility
                {
                    AYear = ay1add.Display,
                    Apply = apply1,
                    TentativeAmount = ay1add.MaxAmount,
                    RevisedAmount = ay1addamount,
                    EligiblePercent = $"{ay1add.Value:F4}%",
                    EligibleAmount = $"{ay1add.Amount:F4}"
                } : null
            });
            result.Add(new AwardYear
            {
                AYear = ay2.Display,
                TentativeAmount = ay2.MaxAmount,
                RevisedAmount = ay2amount,
                EligiblePercent = (ay2.Value < 0) ? $"{0:F4}%" : $"{ay2.Value:F4}%",
                EligibleAmount = $"{ay2.Amount:F4}",
                AdditionalEligibility = (hasay2add) ? new AdditionalEligibility
                {
                    AYear = ay2add.Display,
                    Apply = apply2,
                    TentativeAmount = ay2add.MaxAmount,
                    RevisedAmount = ay2addamount,
                    EligiblePercent = $"{ay2add.Value:F4}%",
                    EligibleAmount = $"{ay2add.Amount:F4}"
                } : null
            });

            return result;
        }

        public static bool HasOtherFunding(NsldsFAHType1 type1, List<NsldsFAHType4> grants)
        {
            var result = false;
            if (grants.Any(x => grantCodes.Contains(x.GrantType)) ||
                type1.PerkinsTotalPrincipalBal > 0 ||
                type1.PlusLoanPrincipalBal > 0 ||
                type1.PlusProPrincipalBal > 0 ||
                type1.TeachLoanPrincipalBal > 0)
            { result = true; }
            return result;
        }

        public static string CalcOtherFunding(NsldsFAHType1 type1, List<NsldsFAHType4> grants, List<FahCode> fc)
        {
            List<string> result = new List<string>();

            if (type1.PerkinsTotalPrincipalBal > 0) { result.Add(perkins); }
            if (type1.PlusLoanPrincipalBal > 0) { result.Add(parentplus); }
            if (type1.PlusProPrincipalBal > 0) { result.Add(graduateplus); }
            if (type1.TeachLoanPrincipalBal > 0) { result.Add(teachloan); }

            var otherGrants = grants.Where(x => grantCodes.Contains(x.GrantType));
            foreach (var item in otherGrants)
            {
                result.Add(item.CodeName(x => x.GrantType, fc));
            }

            return (result.Count > 0) ? string.Join(", ", result) : null;
        }

        /* NSLDS detail response methods */

        public static NameHistoryDetail GetNameHistoryDetail(List<NsldsFAHType2> type2, List<FahAlert> fa, List<FahField> ff)
        {
            NsldsDetailResult dr = new NsldsDetailResult();
            NameHistoryDetail nh = new NameHistoryDetail();

            // name history table
            nh.Changed = type2.Count() > 0;
            nh.Display = dr.AlertName(x => x.NameHistory, fa);
            nh.Description = dr.AlertDescription(x => x.NameHistory, fa);
            if (nh.Changed)
            {
                var f = type2.First();
                var header = new List<HeaderField>
                    {
                        new HeaderField { Display = f.FieldName(x => x.FirstNameHistory, ff),
                            Description = f.FieldDescription(x => x.FirstNameHistory, ff) },
                        new HeaderField { Display = f.FieldName(x => x.MiddleInitialHistory, ff),
                            Description = f.FieldDescription(x => x.MiddleInitialHistory, ff) },
                        new HeaderField { Display = f.FieldName(x => x.LastNameHistory, ff),
                            Description = f.FieldDescription(x => x.LastNameHistory, ff) }
                    };
                var data = new List<NameHistoryRow>();
                foreach (var item in type2)
                {
                    data.Add(new NameHistoryRow
                    {
                        FirstName = item.FirstNameHistory,
                        MiddleInitial = item.MiddleInitialHistory,
                        LastName = item.LastNameHistory
                    });
                }

                nh.HeaderRow = header;
                nh.DataRows = data;
            }

            return nh;
        }

        public static LoanAggregateLimit GetLoanAggregateLimits(NsldsFAHType1 type1, List<NsldsFAHType5> loans, List<FahAlert> fa, List<FahCode> fc, bool resolved, string loanflag)
        {
            NsldsDetailResult dr = new NsldsDetailResult();
            LoanAggregateLimit nh = new LoanAggregateLimit();
            // undergraduate loan aggregate limit calculation engine
            var lnagg = new LoanAggregateLimitCalc(type1, resolved);

            nh.Display = dr.AlertName(x => x.LoanAggregateLimits, fa);
            nh.Description = dr.AlertDescription(x => x.LoanAggregateLimits, fa);

            #region Undergrad Dependent

            nh.UndergradDependent.Display = nh.AlertName(x => x.UndergradDependent, fa);
            nh.UndergradDependent.Description = nh.AlertDescription(x => x.UndergradDependent, fa);

            nh.UndergradDependent.TotalRemain.Display = nh.UndergradDependent.AlertName(x => x.TotalRemain, fa);
            nh.UndergradDependent.TotalRemain.Description = nh.UndergradDependent.AlertDescription(x => x.TotalRemain, fa);
            nh.UndergradDependent.TotalRemain.Value = (!resolved) ? loanflag : lnagg.TotalDependent;

            nh.UndergradDependent.SubLoanRemain.Display = nh.UndergradDependent.AlertName(x => x.SubLoanRemain, fa);
            nh.UndergradDependent.SubLoanRemain.Description = nh.UndergradDependent.AlertDescription(x => x.SubLoanRemain, fa);
            nh.UndergradDependent.SubLoanRemain.Value = (!resolved) ? loanflag : lnagg.SubDependent;

            nh.UndergradDependent.UnsubLoanRemain.Display = nh.UndergradDependent.AlertName(x => x.UnsubLoanRemain, fa);
            nh.UndergradDependent.UnsubLoanRemain.Description = nh.UndergradDependent.AlertDescription(x => x.UnsubLoanRemain, fa);
            nh.UndergradDependent.UnsubLoanRemain.Value = (!resolved) ? loanflag : lnagg.UnsubDependent;

            nh.UndergradDependent.GradeLevel.Display = nh.UndergradDependent.AlertName(x => x.GradeLevel, fa);
            nh.UndergradDependent.GradeLevel.Description = nh.AlertDescription(x => x.UndergradDependent, fa);

            var header1 = new List<HeaderField>
                    {
                        new HeaderField { Display = GradeLevel },
                        new HeaderField { Display = Subsidized },
                        new HeaderField { Display = Unsubsidized }
                    };
            var data1 = new List<UndergradLevelRow>();
            for (int i = 1; i <= 5; i++)
            {
                data1.Add(new UndergradLevelRow
                {
                    GradeLevel = i.ToString(),
                    Subsidized = CalcUndergradSubLoanGradeLevel(i, lnagg.subdependent, resolved, loanflag),
                    Unsubsidized = CalcUndergradUnsubLoanDepGradeLevel(i, lnagg.subdependent, lnagg.unsubdependent, resolved, loanflag)
                });
            }

            nh.UndergradDependent.GradeLevel.HeaderRow = header1;
            nh.UndergradDependent.GradeLevel.DataRows = data1;

            #endregion

            #region Undergrad Independent

            nh.UndergradIndependent.Display = nh.AlertName(x => x.UndergradIndependent, fa);
            nh.UndergradIndependent.Description = nh.AlertDescription(x => x.UndergradIndependent, fa);

            nh.UndergradIndependent.TotalRemain.Display = nh.UndergradIndependent.AlertName(x => x.TotalRemain, fa);
            nh.UndergradIndependent.TotalRemain.Description = nh.UndergradIndependent.AlertDescription(x => x.TotalRemain, fa);
            nh.UndergradIndependent.TotalRemain.Value = (!resolved) ? loanflag : lnagg.TotalIndependent;

            nh.UndergradIndependent.SubLoanRemain.Display = nh.UndergradIndependent.AlertName(x => x.SubLoanRemain, fa);
            nh.UndergradIndependent.SubLoanRemain.Description = nh.UndergradIndependent.AlertDescription(x => x.SubLoanRemain, fa);
            nh.UndergradIndependent.SubLoanRemain.Value = (!resolved) ? loanflag : lnagg.SubIndependent;

            nh.UndergradIndependent.UnsubLoanRemain.Display = nh.UndergradIndependent.AlertName(x => x.UnsubLoanRemain, fa);
            nh.UndergradIndependent.UnsubLoanRemain.Description = nh.UndergradIndependent.AlertDescription(x => x.UnsubLoanRemain, fa);
            nh.UndergradIndependent.UnsubLoanRemain.Value = (!resolved) ? loanflag : lnagg.UnsubIndependent;

            nh.UndergradIndependent.GradeLevel.Display = nh.UndergradIndependent.AlertName(x => x.GradeLevel, fa);
            nh.UndergradIndependent.GradeLevel.Description = nh.AlertDescription(x => x.UndergradIndependent, fa);

            var header2 = new List<HeaderField>
                    {
                        new HeaderField { Display = GradeLevel },
                        new HeaderField { Display = Subsidized },
                        new HeaderField { Display = Unsubsidized }
                    };
            var data2 = new List<UndergradLevelRow>();
            for (int i = 1; i <= 5; i++)
            {
                data2.Add(new UndergradLevelRow
                {
                    GradeLevel = i.ToString(),
                    Subsidized = CalcUndergradSubLoanGradeLevel(i, lnagg.subindependent, resolved, loanflag),
                    Unsubsidized = CalcUndergradUnsubLoanIndGradeLevel(i, lnagg.subindependent, lnagg.unsubindependent, resolved, loanflag)
                });
            }

            nh.UndergradIndependent.GradeLevel.HeaderRow = header2;
            nh.UndergradIndependent.GradeLevel.DataRows = data2;

            #endregion

            #region Graduate

            nh.Graduate.Display = nh.AlertName(x => x.Graduate, fa);
            nh.Graduate.Description = nh.AlertDescription(x => x.Graduate, fa);

            nh.Graduate.TotalRemain.Display = nh.Graduate.AlertName(x => x.TotalRemain, fa);
            nh.Graduate.TotalRemain.Description = nh.Graduate.AlertDescription(x => x.TotalRemain, fa);
            nh.Graduate.TotalRemain.Value = (!resolved) ? loanflag : lnagg.GradTotal;
            //CalcGradUnsubLoanRemain(type1.SubTotal, type1.UnSubTotal);

            nh.Graduate.UnsubLoanRemain.Display = nh.Graduate.AlertName(x => x.UnsubLoanRemain, fa);
            nh.Graduate.UnsubLoanRemain.Description = nh.Graduate.AlertDescription(x => x.UnsubLoanRemain, fa);
            nh.Graduate.UnsubLoanRemain.Value = (!resolved) ? loanflag : lnagg.GradTotal;
            //CalcGradUnsubLoanRemain(type1.SubTotal, type1.UnSubTotal);

            nh.Graduate.GradeLevel.Display = nh.Graduate.AlertName(x => x.GradeLevel, fa);
            nh.Graduate.GradeLevel.Description = nh.Graduate.AlertDescription(x => x.GradeLevel, fa);

            var header3 = new List<HeaderField>
                    {
                        new HeaderField { Display = GradeLevel },
                        new HeaderField { Display = Unsubsidized }
                    };
            var data3 = new List<GradLevelRow>();
            for (int i = 6; i <= 7; i++)
            {
                data3.Add(new GradLevelRow
                {
                    GradeLevel = i.ToString(),
                    Unsubsidized = CalcGradUnsubLoanGradeLevel(type1.SubTotal, type1.UnSubTotal, resolved, loanflag)
                });
            }

            nh.Graduate.GradeLevel.HeaderRow = header3;
            nh.Graduate.GradeLevel.DataRows = data3;

            #endregion

            #region Loan History (sub/unsub)

            nh.LoanHistory.Display = nh.AlertName(x => x.LoanHistory, fa);
            nh.LoanHistory.Description = nh.AlertDescription(x => x.LoanHistory, fa);

            var pdr = new LoanDetailRow();
            var header = new List<HeaderField>
                    {
                        new HeaderField { Display = pdr.AlertName(x => x.LoanType, fa) },
                        new HeaderField { Display = pdr.AlertName(x => x.LoanStatus, fa) },
                        new HeaderField { Display = pdr.AlertName(x => x.LoanFlag, fa) },
                        new HeaderField { Display = pdr.AlertName(x => x.LoanStatusDate, fa) },
                        new HeaderField { Display = pdr.AlertName(x => x.LoanAmount, fa) },
                        new HeaderField { Display = pdr.AlertName(x => x.DisbursedAmount, fa) },
                        new HeaderField { Display = pdr.AlertName(x => x.LoanDates, fa) },
                        new HeaderField { Display = pdr.AlertName(x => x.AYDates, fa) },
                        new HeaderField { Display = pdr.AlertName(x => x.AcademicLevel, fa) },
                        new HeaderField { Display = pdr.AlertName(x => x.School, fa) },
                        new HeaderField()
                    };

            //bool overpay =
            //    (type1.UndergradDependency == depStatus) &&
            //    (type1.UndergradCombinedPrincipalBal > 31000);

            var data = new List<LoanDetailRow>();

            // only sub & unsub loans + consolidation loans CL/D5/D6
            var suloans = loans.Where(
                x => loanSubTypes.Contains(x.LoanTypeCode) ||
                loanUnsubTypes.Contains(x.LoanTypeCode) ||
                loanConsolidationTypes.Contains(x.LoanTypeCode))
                .OrderByDescending(x => x.LoanDate);

            foreach (var item in suloans)
            {
                // nslds-116: add reaffirmation loan flag
                var loanflag2 = new List<string>();
                if (loanFlags.Contains(item.AdditionalUnsubLoan)) { loanflag2.Add(item.AdditionalUnsubLoan); }
                if (item.Reaffirmation == true) { loanflag2.Add(Reaffirmation); }

                decimal balance = item.TotalDisb ?? 0;
                //switch (item.LoanTypeCode)
                //{
                //    case loanD0:
                //        balance = item.CalcSubAmount ?? 0;
                //        break;
                //    case loanD8:
                //        balance = item.CalcUnsubAmount ?? 0;
                //        break;
                //    default:
                //        balance = item.PrincipalBal ?? 0;
                //        break;
                //}

                data.Add(new LoanDetailRow
                {
                    LoanType = item.CodeName(x => x.LoanTypeCode, fc),
                    LoanStatus = item.LoanStatusCode,
                    LoanFlag = (loanflag2.Count > 0) ? string.Join(", ", loanflag2) : null,
                    LoanStatusDate = $"{item.LoanStatusDate:MM/dd/yy}",
                    LoanAmount = $"{item.LoanAmount ?? 0:C}",
                    DisbursedAmount = $"{balance:C}",
                    LoanDates = $"{item.LoanPeriodBeginDate:MM/dd/yy} - {item.LoanPeriodEndDate:MM/dd/yy}",
                    AYDates = $"{item.AYBeginDate:MM/dd/yy} - {item.AYEndDate:MM/dd/yy}",
                    AcademicLevel = item.AcademicLevel,
                    School = item.LoanSchoolCode,
                    TmFlag = item.LoanRecChange
                });
            }
            nh.LoanHistory.HeaderRow = header;
            nh.LoanHistory.DataRows = data;

            #endregion

            return nh;
        }

        public static OpenAcademicYear GetOpenAcademicYear(DateTime? start, int ayweeks, NsldsFAHType1 type1, List<NsldsFAHType5> loans, List<FahAlert> fa, List<FahCode> fc, bool resolved, string loanflag)
        {
            NsldsDetailResult dr = new NsldsDetailResult();
            OpenAcademicYear nh = new OpenAcademicYear();
            nh.Display = dr.AlertName(x => x.OpenAcademicYear, fa);
            nh.OpenAY = CalcOpenAY(start, loans, ayweeks);

            if (nh.OpenAY == Y)
            {
                //display disclaimer when student has open ay loans
                nh.Description = dr.AlertDescription(x => x.OpenAcademicYear, fa);
                var openloans = loans.Where(x => start >= (x.AYBeginDate ?? x.LoanPeriodBeginDate)).ToList();
                foreach (var item in openloans.ToList()) // trick to create a 2nd virtual list
                {
                    DateTime aybegin, ayend;

                    if (item.AYBeginDate != null && item.AYEndDate != null)
                    {
                        int minDays = 26 * 7; // minimum weeks for AY period
                        int days = (item.AYEndDate - item.AYBeginDate).Value.Days;
                        if (days < minDays)
                        {
                            aybegin = item.LoanPeriodBeginDate.Value;
                            ayend = aybegin.AddDays(minDays);
                        }
                        else
                        {
                            aybegin = item.AYBeginDate.Value;
                            ayend = item.AYEndDate.Value;
                        }
                    }
                    else
                    {
                        aybegin = item.LoanPeriodBeginDate.Value;
                        ayend = aybegin.AddDays(ayweeks * 7);
                    }
                    if (start > ayend) { openloans.Remove(item); }
                }
                if (openloans.Count > 0)
                {
                    // undergraduate loan aggregate limit calculation engine
                    var lnagg = new LoanAggregateLimitCalc(type1, resolved);

                    // build open AY loan detail table
                    // DATACHK-68: added TM alert change flag for highlighting
                    var header1 = new List<HeaderField>
                    {
                        new HeaderField { Display = LoanType },
                        new HeaderField { Display = ApprovedAmount },
                        new HeaderField { Display = PendingDisbAmount },
                        new HeaderField { Display = DisbursedAmount },
                        new HeaderField { Display = PeriodDates },
                        new HeaderField { Display = AYDates },
                        new HeaderField { Display = Servicer },
                        new HeaderField { Display = School },
                        new HeaderField()
                    };
                    var data1 = new List<OpenLoanDetailRow>();
                    foreach (var item in openloans)
                    {
                        //check type of loan for pending disbursement
                        decimal pendingdisb = (item.LoanAmount ?? 0) - (item.TotalDisb ?? 0);
                        //(undergradLevels.Contains(item.AcademicLevel)) ?
                        //(loanSubTypes.Contains(item.LoanTypeCode)) ?
                        //type1.UndergradSubPendingDisb ?? 0 : 
                        //type1.UndergradUnsubPendingDisb ?? 0 : 
                        //type1.GradUnsubPendingDisb ?? 0;

                        data1.Add(new OpenLoanDetailRow
                        {
                            LoanType = item.CodeName(x => x.LoanTypeCode, fc),
                            ApprovedAmount = $"{item.LoanAmount:C}",
                            PendingDisbAmount = $"{pendingdisb:C}",
                            DisbursedAmount = $"{item.TotalDisb:C}",
                            PeriodDates = $"{item.LoanPeriodBeginDate:MM/dd/yy} - {item.LoanPeriodEndDate:MM/dd/yy}",
                            AYDates = $"{item.AYBeginDate:MM/dd/yy} - {item.AYEndDate:MM/dd/yy}",
                            Servicer = item.LenderServicer,
                            School = item.LoanSchoolCode,
                            TmFlag = item.LoanRecChange
                        });
                    }
                    nh.OpenLoanDetails.HeaderRow = header1;
                    nh.OpenLoanDetails.DataRows = data1;

                    // build grade level tables
                    decimal subbal = openloans
                        .Where(x => loanSubTypes.Contains(x.LoanTypeCode) &&
                            undergradLevels.Contains(x.AcademicLevel))
                        .Sum(x => x.LoanAmount ?? 0);
                    decimal unsubbal = openloans
                        .Where(x => loanUnsubTypes.Contains(x.LoanTypeCode) &&
                            undergradLevels.Contains(x.AcademicLevel))
                        .Sum(x => x.LoanAmount ?? 0);
                    decimal gradbal = openloans
                        .Where(x => loanTypes.Contains(x.LoanTypeCode) &&
                            !undergradLevels.Contains(x.AcademicLevel))
                        .Sum(x => x.LoanAmount ?? 0);
                    // datachk-190: combined open loans for grad OAY
                    decimal totalbal = subbal + unsubbal + gradbal;

                    // undergraduate dependent grade level
                    nh.OpenUndergradDependent.Display = nh.AlertName(x => x.OpenUndergradDependent, fa);
                    nh.OpenUndergradDependent.Description = nh.AlertDescription(x => x.OpenUndergradDependent, fa);

                    var header2 = new List<HeaderField>
                    {
                        new HeaderField { Display = GradeLevel },
                        new HeaderField { Display = Subsidized },
                        new HeaderField { Display = Unsubsidized }
                    };
                    var data2 = new List<UndergradLevelRow>();
                    for (int i = 1; i <= 5; i++)
                    {
                        data2.Add(new UndergradLevelRow
                        {
                            GradeLevel = i.ToString(),
                            Subsidized = CalcUndergradSubOpenLoanDepGradeLevel(i, subbal, unsubbal, lnagg.subdependent, gradbal, resolved, loanflag),
                            Unsubsidized = CalcUndergradUnsubOpenLoanDepGradeLevel(i, subbal, unsubbal, lnagg.subdependent, lnagg.totaldependent, gradbal, resolved, loanflag)
                        });
                    }

                    nh.OpenUndergradDependent.HeaderRow = header2;
                    nh.OpenUndergradDependent.DataRows = data2;

                    // undergraduate independent grade level
                    nh.OpenUndergradIndependent.Display = nh.AlertName(x => x.OpenUndergradIndependent, fa);
                    nh.OpenUndergradIndependent.Description = nh.AlertDescription(x => x.OpenUndergradIndependent, fa);

                    var header3 = new List<HeaderField>
                    {
                        new HeaderField { Display = GradeLevel },
                        new HeaderField { Display = Subsidized },
                        new HeaderField { Display = Unsubsidized }
                    };
                    var data3 = new List<UndergradLevelRow>();
                    for (int i = 1; i <= 5; i++)
                    {
                        data3.Add(new UndergradLevelRow
                        {
                            GradeLevel = i.ToString(),
                            Subsidized = CalcUndergradSubOpenLoanIndGradeLevel(i, subbal, unsubbal, lnagg.subindependent, gradbal, resolved, loanflag),
                            Unsubsidized = CalcUndergradUnsubOpenLoanIndGradeLevel(i, subbal, unsubbal, lnagg.subindependent, lnagg.totalindependent, gradbal, resolved, loanflag)
                        });
                    }

                    nh.OpenUndergradIndependent.HeaderRow = header3;
                    nh.OpenUndergradIndependent.DataRows = data3;

                    // graduate grade level
                    nh.OpenGraduate.Display = nh.AlertName(x => x.OpenGraduate, fa);
                    nh.OpenGraduate.Description = nh.AlertDescription(x => x.OpenGraduate, fa);

                    var header4 = new List<HeaderField>
                    {
                        new HeaderField { Display = GradeLevel },
                        new HeaderField { Display = Unsubsidized }
                    };
                    var data4 = new List<GradLevelRow>();
                    for (int i = 6; i <= 7; i++)
                    {
                        data4.Add(new GradLevelRow
                        {
                            GradeLevel = i.ToString(),
                            Unsubsidized = CalcGradUnsubOpenLoanGradeLevel(i, lnagg.gradtotal, totalbal, resolved, loanflag)
                        });
                    }

                    nh.OpenGraduate.HeaderRow = header4;
                    nh.OpenGraduate.DataRows = data4;

                }
            }

            return nh;
        }

        public static SubUsageLimit GetSubUsageLimit(NsldsFAHType1 type1, List<FahAlert> fa)
        {
            NsldsDetailResult dr = new NsldsDetailResult();
            SubUsageLimit nh = new SubUsageLimit();
            nh.Display = dr.AlertName(x => x.SULA, fa);
            nh.Description = dr.AlertDescription(x => x.SULA, fa);
            nh.SULA = YOrN(type1.SulaFlag);

            if (nh.SULA == Y)
            {
                nh.SubUsage.Display = nh.AlertName(x => x.SubUsage, fa);
                nh.SubUsage.Value = CalcSulaUsage(type1.SubUsagePeriod);

                nh.MEP.Display = nh.AlertName(x => x.MEP, fa);
                nh.REP.Display = nh.AlertName(x => x.REP, fa);
            }

            return nh;
        }

        public static PellHistory GetPellHistory(List<NsldsFAHType4> grants, List<FahAlert> fa)
        {
            var pell = new Pell();
            var pellhist = new PellHistory();
            pellhist.Display = pell.AlertName(x => x.PellHistory, fa);
            pellhist.Description = pell.AlertDescription(x => x.PellHistory, fa);

            var pellgrants = grants.Where(x => x.GrantType == grantPE)
                .OrderByDescending(x => x.AwardYear).ToList();

            var pdr = new PellDetailRow();
            var header = new List<HeaderField>
                    {
                        new HeaderField { Display = pdr.AlertName(x => x.AwardYear, fa) },
                        new HeaderField { Display = pdr.AlertName(x => x.AwardAmount, fa) },
                        new HeaderField { Display = pdr.AlertName(x => x.DisbursedAmount, fa) },
                        new HeaderField { Display = pdr.AlertName(x => x.AYPercentUsed, fa) },
                        new HeaderField { Display = pdr.AlertName(x => x.EFC, fa) },
                        new HeaderField { Display = pdr.AlertName(x => x.School, fa) },
                        new HeaderField()
                    };
            var data = new List<PellDetailRow>();
            foreach (var item in pellgrants)
            {
                data.Add(new PellDetailRow
                {
                    AwardYear = $"{int.Parse(item.AwardYear) - 1}/{item.AwardYear}",
                    AwardAmount = $"{item.AwardAmount ?? 0:C}",
                    DisbursedAmount = $"{item.DisbAmount ?? 0:C}",
                    AYPercentUsed = $"{item.EligibilityUsed ?? 0:F4}%",
                    EFC = $"{item.FamilyContribution ?? 0:F2}",
                    School = item.GrantSchoolCode,
                    TmFlag = item.GrantChangeFlag
                });
            }
            pellhist.HeaderRow = header;
            pellhist.DataRows = data;

            return pellhist;
        }

        public static TeachHistory GetTeachHistory(List<NsldsFAHType4> grants, List<FahAlert> fa)
        {
            var teach = new Teach();
            var teachhist = new TeachHistory();
            teachhist.Display = teach.AlertName(x => x.TeachHistory, fa);
            teachhist.Description = teach.AlertDescription(x => x.TeachHistory, fa);

            var teachgrants = grants.Where(x => x.GrantType == grantTG)
                .OrderByDescending(x => x.AwardYear).ToList();

            var pdr = new TeachDetailRow();
            var header = new List<HeaderField>
                    {
                        new HeaderField { Display = pdr.AlertName(x => x.AwardYear, fa) },
                        new HeaderField { Display = pdr.AlertName(x => x.AwardAmount, fa) },
                        new HeaderField { Display = pdr.AlertName(x => x.DisbursedAmount, fa) },
                        new HeaderField { Display = pdr.AlertName(x => x.AYLevel, fa) },
                        new HeaderField { Display = pdr.AlertName(x => x.School, fa) }
                    };
            var data = new List<TeachDetailRow>();
            foreach (var item in teachgrants)
            {
                data.Add(new TeachDetailRow
                {
                    AwardYear = $"{int.Parse(item.AwardYear) - 1}/{item.AwardYear}",
                    AwardAmount = $"{item.AwardAmount ?? 0:C}",
                    DisbursedAmount = $"{item.DisbAmount ?? 0:C}",
                    AYLevel = item.AYLevel,
                    School = item.GrantSchoolCode
                });
            }
            teachhist.HeaderRow = header;
            teachhist.DataRows = data;

            return teachhist;
        }

        public static ACGHistory GetACGHistory(List<NsldsFAHType4> grants, List<FahAlert> fa)
        {
            var acg = new ACG();
            var acghist = new ACGHistory();
            acghist.Display = acg.AlertName(x => x.ACGHistory, fa);
            acghist.Description = acg.AlertDescription(x => x.ACGHistory, fa);

            var acggrants = grants.Where(x => x.GrantType == grantAG)
                .OrderByDescending(x => x.AwardYear).ToList();

            var pdr = new ACGDetailRow();
            var header = new List<HeaderField>
                    {
                        new HeaderField { Display = pdr.AlertName(x => x.AwardYear, fa) },
                        new HeaderField { Display = pdr.AlertName(x => x.AwardAmount, fa) },
                        new HeaderField { Display = pdr.AlertName(x => x.DisbursedAmount, fa) },
                        new HeaderField { Display = pdr.AlertName(x => x.HSProgram, fa) },
                        new HeaderField { Display = pdr.AlertName(x => x.AYPercentUsed, fa) },
                        new HeaderField { Display = pdr.AlertName(x => x.School, fa) }
                    };
            var data = new List<ACGDetailRow>();
            foreach (var item in acggrants)
            {
                data.Add(new ACGDetailRow
                {
                    AwardYear = $"{int.Parse(item.AwardYear) - 1}/{item.AwardYear}",
                    AwardAmount = $"{item.AwardAmount ?? 0:C}",
                    DisbursedAmount = $"{item.DisbAmount ?? 0:C}",
                    HSProgram = item.HSProgramCode,
                    AYPercentUsed = $"{item.EligibilityUsed ?? 0:F3}%",
                    School = item.GrantSchoolCode
                });
            }
            acghist.HeaderRow = header;
            acghist.DataRows = data;

            return acghist;
        }

        public static SmartHistory GetSmartHistory(List<NsldsFAHType4> grants, List<FahAlert> fa)
        {
            var smart = new Smart();
            var smarthist = new SmartHistory();
            smarthist.Display = smart.AlertName(x => x.SmartHistory, fa);
            smarthist.Description = smart.AlertDescription(x => x.SmartHistory, fa);

            var smartgrants = grants.Where(x => x.GrantType == grantSG)
                .OrderByDescending(x => x.AwardYear).ToList();

            var pdr = new SmartDetailRow();
            var header = new List<HeaderField>
                    {
                        new HeaderField { Display = pdr.AlertName(x => x.AwardYear, fa) },
                        new HeaderField { Display = pdr.AlertName(x => x.AwardAmount, fa) },
                        new HeaderField { Display = pdr.AlertName(x => x.DisbursedAmount, fa) },
                        new HeaderField { Display = pdr.AlertName(x => x.CIPCode, fa) },
                        new HeaderField { Display = pdr.AlertName(x => x.AYPercentUsed, fa) },
                        new HeaderField { Display = pdr.AlertName(x => x.School, fa) }
                    };
            var data = new List<SmartDetailRow>();
            foreach (var item in smartgrants)
            {
                data.Add(new SmartDetailRow
                {
                    AwardYear = $"{int.Parse(item.AwardYear) - 1}/{item.AwardYear}",
                    AwardAmount = $"{item.AwardAmount ?? 0:C}",
                    DisbursedAmount = $"{item.DisbAmount ?? 0:C}",
                    CIPCode = item.CIPCode,
                    AYPercentUsed = $"{item.EligibilityUsed ?? 0:F3}%",
                    School = item.GrantSchoolCode
                });
            }
            smarthist.HeaderRow = header;
            smarthist.DataRows = data;

            return smarthist;
        }

        public static GrantHistory GetGrants(ClientRequestStudent request, NsldsFAHType1 type1, List<NsldsFAHType4> grants, List<FahAlert> fa, List<PellAward> fp, bool pellflag, bool resolved, string loanflag)
        {
            NsldsDetailResult dr = new NsldsDetailResult();
            GrantHistory nh = new GrantHistory();
            nh.Display = dr.AlertName(x => x.Grants, fa);
            nh.Description = dr.AlertDescription(x => x.Grants, fa);

            // Pell grants
            nh.Pell.Display = nh.AlertName(x => x.Pell, fa);
            nh.Pell.Description = nh.AlertDescription(x => x.Pell, fa);

            // nslds-120: calculate LEU based on actual grants disbursed
            // nslds-137: revert to using type1 leu due to possible duplicates records from nslds
            //decimal granttotal = grants
            //    .Where(x => x.GrantType == grantPE).Sum(x => x.EligibilityUsed) ?? 0;
            decimal type1leu = type1.LifeEligibilityUsed ?? 0;
            if (type1leu > 0) { type1leu += 0.0009M; }
            decimal percentused = type1leu; //Math.Max(granttotal, type1leu);

            nh.Pell.LEU.Display = nh.Pell.AlertName(x => x.LEU, fa);
            nh.Pell.LEU.Description = nh.Pell.AlertDescription(x => x.LEU, fa);
            nh.Pell.LEU.Value = $"{percentused:F4}%";

            nh.Pell.Remain.Display = nh.Pell.AlertName(x => x.Remain, fa);
            nh.Pell.Remain.Description = nh.Pell.AlertDescription(x => x.Remain, fa);
            nh.Pell.Remain.Value = CalcLeuRemain(percentused, resolved, loanflag);

            nh.Pell.AnnualRemain.Display = nh.Pell.AlertName(x => x.AnnualRemain, fa);
            nh.Pell.AnnualRemain.Description = nh.Pell.AlertDescription(x => x.AnnualRemain, fa);
            //nh.Pell.AnnualRemain.Values = CalcAnnualAwards(request, type1, grants, pellflag, resolved, fp, null, null, null, null);
            nh.Pell.AnnualRemain.Values = CalcAnnualAwards(request, type1, grants, pellflag, resolved, fp, null, null, null, null, null, null, null, null);

            nh.Pell.PellHistory = GetPellHistory(grants, fa);

            // TEACH grants
            nh.Teach.Display = nh.AlertName(x => x.Teach, fa);
            nh.Teach.Description = nh.AlertDescription(x => x.Teach, fa);

            nh.Teach.UndergradReceived.Display = nh.Teach.AlertName(x => x.UndergradReceived, fa);
            nh.Teach.UndergradReceived.Description = nh.Teach.AlertDescription(x => x.UndergradReceived, fa);
            nh.Teach.UndergradReceived.Value = $"{(type1.UndergradTeachDisbTotal ?? 0):C}";

            nh.Teach.UndergradRemain.Display = nh.Teach.AlertName(x => x.UndergradRemain, fa);
            nh.Teach.UndergradRemain.Description = nh.Teach.AlertDescription(x => x.UndergradRemain, fa);
            nh.Teach.UndergradRemain.Value = $"{(type1.UndergradRemainingAmount ?? 0):C}";

            nh.Teach.GradReceived.Display = nh.Teach.AlertName(x => x.GradReceived, fa);
            nh.Teach.GradReceived.Description = nh.Teach.AlertDescription(x => x.GradReceived, fa);
            nh.Teach.GradReceived.Value = $"{(type1.GradTeachTotalDisb ?? 0):C}";

            nh.Teach.GradRemain.Display = nh.Teach.AlertName(x => x.GradRemain, fa);
            nh.Teach.GradRemain.Description = nh.Teach.AlertDescription(x => x.GradRemain, fa);
            nh.Teach.GradRemain.Value = $"{(type1.GradRemainingAmount ?? 0):C}";

            nh.Teach.TeachHistory = GetTeachHistory(grants, fa);

            // ACG grants
            nh.ACG.Display = nh.AlertName(x => x.ACG, fa);
            nh.ACG.Description = nh.AlertDescription(x => x.ACG, fa);

            var acggrants = grants.Where(x => x.GrantType == grantAG);

            nh.ACG.TotalAwarded.Display = nh.ACG.AlertName(x => x.TotalAwarded, fa);
            nh.ACG.TotalAwarded.Description = nh.ACG.AlertDescription(x => x.TotalAwarded, fa);
            nh.ACG.TotalAwarded.Value = $"{acggrants.Sum(x => x.AwardAmount ?? 0):C}";

            nh.ACG.TotalReceived.Display = nh.ACG.AlertName(x => x.TotalReceived, fa);
            nh.ACG.TotalReceived.Description = nh.ACG.AlertDescription(x => x.TotalReceived, fa);
            nh.ACG.TotalReceived.Value = $"{acggrants.Sum(x => x.DisbAmount ?? 0):C}";

            nh.ACG.TotalPercentUsed.Display = nh.ACG.AlertName(x => x.TotalPercentUsed, fa);
            nh.ACG.TotalPercentUsed.Description = nh.ACG.AlertDescription(x => x.TotalPercentUsed, fa);
            nh.ACG.TotalPercentUsed.Value = $"{acggrants.Sum(x => x.EligibilityUsed ?? 0):F3}%";

            nh.ACG.ACGHistory = GetACGHistory(grants, fa);

            // SMART grants
            nh.Smart.Display = nh.AlertName(x => x.Smart, fa);
            nh.Smart.Description = nh.AlertDescription(x => x.Smart, fa);

            var smartgrants = grants.Where(x => x.GrantType == grantSG);

            nh.Smart.TotalAwarded.Display = nh.Smart.AlertName(x => x.TotalAwarded, fa);
            nh.Smart.TotalAwarded.Description = nh.Smart.AlertDescription(x => x.TotalAwarded, fa);
            nh.Smart.TotalAwarded.Value = $"{smartgrants.Sum(x => x.AwardAmount ?? 0):C}";

            nh.Smart.TotalReceived.Display = nh.Smart.AlertName(x => x.TotalReceived, fa);
            nh.Smart.TotalReceived.Description = nh.Smart.AlertDescription(x => x.TotalReceived, fa);
            nh.Smart.TotalReceived.Value = $"{smartgrants.Sum(x => x.DisbAmount ?? 0):C}";

            nh.Smart.TotalPercentUsed.Display = nh.Smart.AlertName(x => x.TotalPercentUsed, fa);
            nh.Smart.TotalPercentUsed.Description = nh.Smart.AlertDescription(x => x.TotalPercentUsed, fa);
            nh.Smart.TotalPercentUsed.Value = $"{smartgrants.Sum(x => x.EligibilityUsed ?? 0):F3}%";

            nh.Smart.SmartHistory = GetSmartHistory(grants, fa);

            return nh;
        }

        public static Loans GetLoans(NsldsFAHType1 type1, List<NsldsFAHType5> loans, List<FahAlert> fa, List<FahCode> fc)
        {
            NsldsDetailResult dr = new NsldsDetailResult();
            Loans ln = new Loans();
            ln.Display = dr.AlertName(x => x.Loans, fa);
            ln.Description = dr.AlertDescription(x => x.Loans, fa);

            // loans summary
            ln.ParentPlusBalance.Display = ln.AlertName(x => x.ParentPlusBalance, fa);
            ln.ParentPlusBalance.Description = ln.AlertDescription(x => x.ParentPlusBalance, fa);
            ln.ParentPlusBalance.Value = (type1.PlusLoanPrincipalBal != null) ? $"{type1.PlusLoanPrincipalBal ?? 0:C}" : null;

            ln.GraduatePlusBalance.Display = ln.AlertName(x => x.GraduatePlusBalance, fa);
            ln.GraduatePlusBalance.Description = ln.AlertDescription(x => x.GraduatePlusBalance, fa);
            ln.GraduatePlusBalance.Value = (type1.PlusProPrincipalBal != null) ? $"{type1.PlusProPrincipalBal ?? 0:C}" : null;

            ln.PerkinsBalance.Display = ln.AlertName(x => x.PerkinsBalance, fa);
            ln.PerkinsBalance.Description = ln.AlertDescription(x => x.PerkinsBalance, fa);
            ln.PerkinsBalance.Value = (type1.PerkinsTotalPrincipalBal != null) ? $"{type1.PerkinsTotalPrincipalBal ?? 0:C}" : null;

            ln.PerkinsAYDisbursed.Display = ln.AlertName(x => x.PerkinsAYDisbursed, fa);
            ln.PerkinsAYDisbursed.Description = ln.AlertDescription(x => x.PerkinsAYDisbursed, fa);
            ln.PerkinsAYDisbursed.Value = (type1.PerkinsTotalPrincipalBal != null || type1.PerkinsCurrentAYDisb != null) ? $"{type1.PerkinsCurrentAYDisb ?? 0:C}" : null;

            ln.TeachBalance.Display = ln.AlertName(x => x.TeachBalance, fa);
            ln.TeachBalance.Description = ln.AlertDescription(x => x.TeachBalance, fa);
            ln.TeachBalance.Value = (type1.TeachLoanPrincipalBal != null) ? $"{type1.TeachLoanPrincipalBal ?? 0:C}" : null;

            // populate all loans table
            ln.TotalLoanHistory.Display = ln.AlertName(x => x.TotalLoanHistory, fa);
            ln.TotalLoanHistory.Description = ln.AlertDescription(x => x.TotalLoanHistory, fa);

            var pdr = new LoanDetailRow();
            var header = new List<HeaderField>
                    {
                        new HeaderField { Display = pdr.AlertName(x => x.LoanType, fa) },
                        new HeaderField { Display = pdr.AlertName(x => x.LoanStatus, fa) },
                        new HeaderField { Display = pdr.AlertName(x => x.LoanFlag, fa) },
                        new HeaderField { Display = pdr.AlertName(x => x.LoanStatusDate, fa) },
                        new HeaderField { Display = pdr.AlertName(x => x.LoanAmount, fa) },
                        new HeaderField { Display = pdr.AlertName(x => x.DisbursedAmount, fa) },
                        new HeaderField { Display = pdr.AlertName(x => x.LoanDates, fa) },
                        new HeaderField { Display = pdr.AlertName(x => x.AYDates, fa) },
                        new HeaderField { Display = pdr.AlertName(x => x.AcademicLevel, fa) },
                        new HeaderField { Display = pdr.AlertName(x => x.School, fa) },
                        new HeaderField()
                    };

            //bool overpay =
            //    (type1.UndergradDependency == depStatus) &&
            //    (type1.UndergradCombinedPrincipalBal > 31000);

            var data = new List<LoanDetailRow>();

            // all loans
            var all_loans = loans.OrderByDescending(x => x.LoanDate);

            foreach (var item in all_loans)
            {
                // nslds-116: add reaffirmation loan flag
                var loanflag = new List<string>();
                if (loanFlags.Contains(item.AdditionalUnsubLoan)) { loanflag.Add(item.AdditionalUnsubLoan); }
                if (item.Reaffirmation == true) { loanflag.Add(Reaffirmation); }

                decimal balance = item.TotalDisb ?? 0;
                //switch (item.LoanTypeCode)
                //{
                //    case loanD0:
                //        balance = item.CalcSubAmount ?? 0;
                //        break;
                //    case loanD8:
                //        balance = item.CalcUnsubAmount ?? 0;
                //        break;
                //    default:
                //        balance = item.PrincipalBal ?? 0;
                //        break;
                //}

                data.Add(new LoanDetailRow
                {
                    LoanType = item.CodeName(x => x.LoanTypeCode, fc),
                    LoanStatus = item.LoanStatusCode,
                    LoanFlag = (loanflag.Count > 0) ? string.Join(", ", loanflag) : null,
                    LoanStatusDate = $"{item.LoanStatusDate:MM/dd/yy}",
                    LoanAmount = $"{item.LoanAmount ?? 0:C}",
                    DisbursedAmount = $"{balance:C}",
                    LoanDates = $"{item.LoanPeriodBeginDate:MM/dd/yy} - {item.LoanPeriodEndDate:MM/dd/yy}",
                    AYDates = $"{item.AYBeginDate:MM/dd/yy} - {item.AYEndDate:MM/dd/yy}",
                    AcademicLevel = item.AcademicLevel,
                    School = item.LoanSchoolCode,
                    TmFlag = item.LoanRecChange
                });
            }
            ln.TotalLoanHistory.HeaderRow = header;
            ln.TotalLoanHistory.DataRows = data;

            return ln;
        }

        #endregion
    }
}

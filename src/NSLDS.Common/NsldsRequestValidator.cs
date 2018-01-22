using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSLDS.Domain;
using System.Text.RegularExpressions;
using System.Collections.Specialized;

namespace NSLDS.Common
{
    // validate an nslds request and add optional validation message
    public static class NsldsRequestValidator
    {
        public static bool ValidateRequest(ClientRequestStudent nsldsRequest)
        {
            var date = DateTime.Today;
            bool isValid = true;

            nsldsRequest.With(x =>
            {
                x.ValidationMessage.Clear();
                if (x.RequestType == null) { x.RequestType = string.Empty; }
                else { x.RequestType = x.RequestType.ToUpper(); }
                if (x.DeleteMonitoring == null || Regex.IsMatch(x.DeleteMonitoring, RegExConstant.NoDelete))
                    { x.DeleteMonitoring = string.Empty; }
                if (x.SSN == null) { x.SSN = string.Empty; }
                if (x.FirstName == null) { x.FirstName = string.Empty; }
                if (x.LastName == null) { x.LastName = string.Empty; }

                if (!Regex.IsMatch(x.SSN, RegExConstant.SSN))
                { x.ValidationMessage.Add(NsldsValidator.SSN); isValid = false; }

                if (x.DOB == null || x.DOB > date.AddYears(-12) || x.DOB < date.AddYears(-100))
                { x.ValidationMessage.Add(NsldsValidator.DOB); isValid = false; }

                if ((Regex.IsMatch(x.RequestType, RegExConstant.FAH) && x.MonitorBeginDate != null)
                    || (Regex.IsMatch(x.RequestType, RegExConstant.FAH) && x.EnrollBeginDate != null))
                {
                    x.ValidationMessage.Add(NsldsValidator.FAHNoDates);
                    x.EnrollBeginDate = null; x.MonitorBeginDate = null;
                }

                if (Regex.IsMatch(x.RequestType, RegExConstant.NotFAH)
                    && (x.MonitorBeginDate == null || x.MonitorBeginDate > x.EnrollBeginDate))
                { x.ValidationMessage.Add(NsldsValidator.MonitorBegin); isValid = false; }

                if (Regex.IsMatch(x.RequestType, RegExConstant.NotFAH)
                    && (x.EnrollBeginDate == null || x.EnrollBeginDate < date.AddDays(-90) || x.EnrollBeginDate > date.AddMonths(18)))
                {
                    //nslds-147: bug fix for enroll begin date validation message
                    var enrollBegin = string.Format(NsldsValidator.Message.EnrollBegin, date.AddDays(-90).ToShortDateString(), date.AddMonths(18).ToShortDateString());
                    x.ValidationMessage.Add(new[] { NsldsValidator.Field.EnrollBegin, enrollBegin });
                    isValid = false;
                }

                if (!Regex.IsMatch(x.RequestType, RegExConstant.RequestType))
                { x.ValidationMessage.Add(NsldsValidator.RequestType); isValid = false; }

                if ((Regex.IsMatch(x.RequestType, RegExConstant.FAH) 
                    && (x.DeleteMonitoring != string.Empty))
                    || (Regex.IsMatch(x.RequestType, RegExConstant.NotFAH)
                    && (!Regex.IsMatch(x.DeleteMonitoring, RegExConstant.Delete))))
                { x.ValidationMessage.Add(NsldsValidator.DelMonitor); isValid = false; }

                //if (x.StartDate == null)
                //{ x.ValidationMessage += ValidationConstant.StartDate; }

                if (x.FirstName == string.Empty)
                { x.ValidationMessage.Add(NsldsValidator.First); isValid = false; }

                if (x.LastName == string.Empty)
                { x.ValidationMessage.Add(NsldsValidator.Last); isValid = false; }

                // passed all the validation tests?
                x.IsValid = isValid;
            });

            return isValid;
        }

        public static bool ValidateBatch(ClientRequest currentBatch)
        {
            // check that all student records are valid
            return currentBatch.Students.Count(x => x.IsValid == false) == 0;
        }
    }
}

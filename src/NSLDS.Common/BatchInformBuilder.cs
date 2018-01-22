using NSLDS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSLDS.Common
{
    // this class builds the fixed length batch inform header/details/footer file to be uploaded
    // added the TDClient header/trailer to identify OPEID & batch ID
    // EDIT: NSLDS doesn't return the TDClient custom parameters, use Submit date + Sequence
    public static class BatchInformBuilder
    {
        #region Sample file
/*
O*N05TG53163       ,CLS=TRNINFIN,XXX,BAT=batchId,OPEID=OpeId // > 70
0TSM/FAH INFORM HEADER                         03819300000000002016062201E                                                                            
1552318741Leslie Richardson                         196904232016070420160704N12345678B                                                           
1614549822Kyra Shoolman                           196904120000000000000000 12345678H                                                           
1612328622NFN NLN                                198405192016040420160404N12345678T                                                           
1431572999Jennifer Carruba                            198411220000000000000000 12345678H                                                           
1510985820Matthew Norman                             198810120000000000000000 12345678H                                                           
9TSM/FAH INFORM TRAILER                        000000005                                                                                              
O*N95TG53163       ,CLS=TRNINFIN,XXX,BAT=batchId,OPEID=OpeId // > 70
*/
        #endregion

        #region Private methods

        private static string _buildTDHeader(ClientProfile cp, int crId)
        {
            string[] result = new string[]
            {
                string.Format("{0,5}", "O*N05"),
                string.Format("{0,7}", "TG53163"),
                string.Format("{0,7}", string.Empty),
                string.Format("{0,22}", ",CLS=TRNINFIN,XXX,BAT="),
                string.Format("{0:D17}", crId),
                string.Format("{0,7}", ",OPEID="),
                string.Format("{0,8}", cp.OPEID)
            };

            return string.Concat(result);
        }

        private static string _buildTDFooter(ClientProfile cp, int crId)
        {
            string[] result = new string[]
            {
                string.Format("{0,5}", "O*N95"),
                string.Format("{0,7}", "TG53163"),
                string.Format("{0,7}", string.Empty),
                string.Format("{0,22}", ",CLS=TRNINFIN,XXX,BAT="),
                string.Format("{0:D17}", crId),
                string.Format("{0,7}", ",OPEID="),
                string.Format("{0,8}", cp.OPEID)
            };

            return string.Concat(result);
        }

        private static string _buildHeader(string opeId, ClientRequest cReq)
        {
            string[] result = new string[]
            {
                string.Format("{0,1}", "0"),
                string.Format("{0,-46}", "TSM/FAH INFORM HEADER"),
                string.Format("{0,8}", opeId),
                string.Format("{0,8}", "00000000"),
                string.Format("{0,8:yyyyMMdd}", cReq.SubmittedOn),
                string.Format("{0,2:00}", cReq.Sequence),
                string.Format("{0,1}", "E"),
                string.Format("{0,-76}", string.Empty)
            };
           
            return string.Concat(result);
        }

        private static string _buildDetail(string opeId, ClientRequestStudent cReqS)
        {
            string[] result = new string[]
            {
                string.Format("{0,1}", "1"),
                string.Format("{0,-9}", cReqS.SSN),
                string.Format("{0,-12}", cReqS.FirstName.Limit(12)),
                string.Format("{0,-35}", cReqS.LastName.Limit(35)),
                string.Format("{0,8:yyyyMMdd}", cReqS.DOB),
                (cReqS.EnrollBeginDate == null) ? 
                    string.Format("{0,8}", "00000000") :
                    string.Format("{0,8:yyyyMMdd}", cReqS.EnrollBeginDate),
                (cReqS.MonitorBeginDate == null) ?
                    string.Format("{0,8}", "00000000") :
                    string.Format("{0,8:yyyyMMdd}", cReqS.MonitorBeginDate),
                string.Format("{0,1}", cReqS.DeleteMonitoring),
                string.Format("{0,8}", opeId),
                string.Format("{0,1}", cReqS.RequestType),
                string.Format("{0,-59}", string.Empty)
            };

            return string.Concat(result);
        }

        private static string _buildFooter(ClientRequest cReq)
        {
            string[] result = new string[]
            {
                string.Format("{0,1}", "9"),
                string.Format("{0,-46}", "TSM/FAH INFORM TRAILER"),
                string.Format("{0,9:000000000}", cReq.Students.Count),
                string.Format("{0,-94}", string.Empty)
            };

            return string.Concat(result);
        }

        #endregion

        #region Public Methods

        // batch inform file generation for a single batch request
        // nslds-110: add optional tdclient header/footer
        public static StringBuilder Build(ClientProfile cp, ClientRequest clientRequest, bool tdclient = true)
        {
            StringBuilder sb = new StringBuilder();

            if (tdclient) { sb.AppendLine(_buildTDHeader(cp, clientRequest.Id)); }
            sb.AppendLine(_buildHeader(cp.OPEID, clientRequest));

            foreach (var student in clientRequest.Students)
            {
                sb.AppendLine(_buildDetail(cp.OPEID, student));
            }

            sb.AppendLine(_buildFooter(clientRequest));
            if (tdclient) { sb.AppendLine(_buildTDFooter(cp, clientRequest.Id)); }

            return sb;
        }

        // batch inform file generation for multiple batch requests (overload)
        public static StringBuilder Build(ClientProfile cp, List<ClientRequest> clientRequests, bool tdclient = true)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var clientRequest in clientRequests)
            {
                if (tdclient) { sb.AppendLine(_buildTDHeader(cp, clientRequest.Id)); }
                sb.AppendLine(_buildHeader(cp.OPEID, clientRequest));

                foreach (var student in clientRequest.Students)
                {
                    sb.AppendLine(_buildDetail(cp.OPEID, student));
                }

                sb.AppendLine(_buildFooter(clientRequest));
                if (tdclient) { sb.AppendLine(_buildTDFooter(cp, clientRequest.Id)); }
            }

            return sb;
        }

        #endregion
    }
}

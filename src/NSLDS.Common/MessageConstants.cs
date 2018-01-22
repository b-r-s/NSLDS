using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NSLDS.Common
{
    public static class MessageConstant
    {
        public const string
            InformFileNotFound = "The previously created NSLDS inform file was not found.",
            InformFileRetrieved = @"Hello, it is important that you submit this inform file to NSLDS 
the same day that it is created. Once your response file is received, The Error/Acknowledgement 
response (TRNINFOP) will need to be uploaded at the same time as the Financial Aid History response 
(FAHEXTOP) is being uploaded if there is a TRNINFOP available and you are creating a new batch. 
If you attempt to upload a TRNINFOP response file before the FAH, you will not be able to complete 
your upload unless our system matches your Error/Acknowledgement response file to an already 
existing batch waiting processing.",
        InformFileGenerated = @"Hello, it is important that you submit this inform file to NSLDS 
the same day that it is created. Once your response file is received, The Error/Acknowledgement 
response (TRNINFOP) will need to be uploaded at the same time as the Financial Aid History response 
(FAHEXTOP) is being uploaded if there is a TRNINFOP available and you are creating a new batch. 
If you attempt to upload a TRNINFOP response file before the FAH, you will not be able to complete 
your upload unless our system matches your Error/Acknowledgement response file to an already 
existing batch waiting processing.",
            NoResponseFiles = "No valid response files found for this batch.",
            BatchNotFound = "The batch request {0} was not found.",
            BatchAlreadyProcessed = "The batch request {0} is already processed.",
            BatchRequiresResponse = "The batch request {0} requires the {1} response.",
            BatchRecordNotFound = "The {0} record {1} {2} does not exist in this request.",
            ResponseErrorFound = @"An error occurred while processing the {0} response.
 Please make sure there are no errors preventing upload within the response file.",
            NewBatchNoResponse = "New batch request requires the FAHEXTOP response (TRNINFOP optional) or TRALRTOP response.",
            NewBatchResponseSuccess = "Batch request {0} created and queued for processing.",
            BatchResponseSuccess = "Batch request {0} queued for processing.";
    }
}

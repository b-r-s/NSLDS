using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NSLDS.Common
{
    // Summary:
    // Defines all error message constants as an enum style static class
    public static class ErrorConstant
    {
        public const string
            InvalidClientProfileId = "Invalid client profile Id.",
            InvalidClientProfile = "Invalid client profile.",
            ClientProfileNotFound = "Client profile not found.",
            ClientProfileAlreadyExists = "Client profile already exists.",
            InvalidBatchId = "Invalid batch Id.",
            InvalidBatch = "Batch is invalid and cannot be submitted.",
            InvalidUploadMethod = "The communication method specified does not allow manual submission.",
            BatchAlreadyInQueue = "Batch is already in queue.",
            BatchAlreadySubmitted = "Batch is already submitted.",
            BatchAlreadyReceived = "Batch is already received.",
            BatchOnHoldOrDisabled = "Batch is on hold or disabled.",
            InvalidNsldsRequest = "Invalid nslds request.",
            NsldsRequestNotFound = "Nslds request not found.",
            InvalidFile = "Invalid file.",
            InvalidFileUpload = "Invalid or corrupted response file.",
            FormatNotRecognized = "File format not recognized.",
            FileHasNoValidRecords = "File has no valid records.",
            BatchHasNoRecords = "Batch request has no records to process.",
            InviteExpired = "Invitation has expired.",
            InviteUsed = "Invitation has already been used."
            ;
    }
}

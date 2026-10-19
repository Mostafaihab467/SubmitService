using System;

namespace SubmitServ.Models.ETA
{
    public class ETASubmissionAPIResultModel
    {
        public bool IsSuccess { get; set; } = false;
        public string error { get; set; }
        public DateTime? TokenRequestDate { get; set; }
        public int expires_in { get; set; }
        public string Token { get; set; }
        public SuccessModel Result { get; set; }
    }
    public class SuccessModel
    {
        public string submissionId { get; set; }
        public acceptedDocuments[] acceptedDocuments { get; set; }
        public rejectedDocuments[] rejectedDocuments { get; set; }
        public header header { get; set; }
    }
    public class rejectedDocuments
    {
        public string receiptNumber { get; set; }
        public string uuid { get; set; }
        public error error { get; set; }
    }
    public class acceptedDocuments
    {
        public string uuid { get; set; }
        public string longId { get; set; }
        public string receiptNumber { get; set; }
        public string hashKey { get; set; }
    }

    public class header
    {
        public string statusCode { get; set; }
        public string code { get; set; }
        public details[] details { get; set; }
        public string correlationId { get; set; }
        public string requestTime { get; set; }
        public string responseProcessingTimeInTicks { get; set; }
    }

    public class details
    {
        public string message { get; set; }
        public string target { get; set; }
        public string propertyPath { get; set; }
    }
    public class error
    {
        public string message { get; set; }
        public string target { get; set; }
        public details[] details { get; set; }
    }
    public class errorResponse
    {
        public string error { get; set; } 
    }
}

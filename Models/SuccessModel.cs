using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubmitServ.APIModels
{
    public class SuccessModel
    {
        public string submissionId { get; set; }
        public acceptedDocuments[] acceptedDocuments { get; set; }
        public rejectedDocuments[] rejectedDocuments { get; set; }
        public header header { get; set; }
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
        public int? responseProcessingTimeInTicks { get; set; }
    }

    public class details
    {
        public string code { get; set; }
        public string message { get; set; }
        public string target { get; set; }
        public string propertyPath { get; set; }
    }
}

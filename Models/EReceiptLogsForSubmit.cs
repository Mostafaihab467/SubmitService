using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubmitServ.Models
{
    //public class EReceiptLogsForSubmit
    //{
    //    public Data[] data { get; set; }
    //}

    public class EReceiptLogsForSubmit//Data
    {
        public long? LogID { get; set; }
        public long? EReceiptID { get; set; }
        public int LogTypeID { get; set; }
        public string EReceiptNumber { get; set; }
        public DateTime? LogDatetime { get; set; }
        public bool? IsSendTosubmit { get; set; }
        public byte? SubmitStatus { get; set; }
        public string ReuestPayload { get; set; }
        public string ResponsePayload { get; set; }
        public string UUID { get; set; }
        public string QRCode { get; set; }
        public string ETARequestPayload { get; set; }
        public string ETAResponsePayload { get; set; }
        public string ETAQRCode { get; set; }
        public string ETAUUID { get; set; }
        public string Error { get; set; }
        public byte? EReceiptSource { get; set; }
        public string EReceiptStatus { get; set; }
    }
}

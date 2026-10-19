

using System;

namespace SubmitServ.Models.ETA
{
     
    public class ReceiptsModel
    {
        public DateTime? TokenRequestDate { get; set; }
        public int expires_in { get; set; }
        public string Token { get; set; } 
        public EReceiptModel[] receipts { get; set; }
    }
   
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubmitServ.Models
{
    public class ReceiptsModel
    {
        public DateTime? TokenRequestDate { get; set; }
        public int? expires_in { get; set; }
        public string Token { get; set; }
        public EReceiptModel[] receipts { get; set; }
    }
}

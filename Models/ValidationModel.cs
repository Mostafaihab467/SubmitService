using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubmitServ.APIModels
{
    public class ValidationModel
    {
        public rejectedDocuments[] rejectedDocuments { get; set; }
        public header header { get; set; }
    }
    public class rejectedDocuments
    {
        public string receiptNumber { get; set; }
        public string uuid { get; set; }
        public error error { get; set; }
    }

    public class error
    {
        public string message { get; set; }
        public string target { get; set; }
        public details[] details { get; set; }
    }
}

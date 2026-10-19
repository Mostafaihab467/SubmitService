using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubmitServ
{
    public class AppSettings
    {
        public static string client_id { get; set; }
        public static string client_secret { get; set; }
        public static string posserial { get; set; }
        public static string pososversion { get; set; }
        public static string StatusLimit { get; set; }


        public static string ETAApiURL { get; set; } = "https://api.invoicing.eta.gov.eg/api/";
        public static string ETATokenURL { get; set; } = "https://id.eta.gov.eg/connect/token";
        public static string EreciptURL { get; set; }
        public static string connectionStr { get; set; }
        public static string qrURL { get; set; }
    }
}

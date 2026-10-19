using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SubmitServ.APIModels;

namespace SubmitServ.Models
{
    public class ETASubmissionAPIResultModel
    {
        public bool IsSuccess { get; set; } = false;
        public string error { get; set; }
        public DateTime TokenRequestDate { get; set; }
        public int expires_in { get; set; }
        public string Token { get; set; }
        public SuccessModel Result { get; set; }
    }
}

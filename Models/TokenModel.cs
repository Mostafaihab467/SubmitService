using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubmitServ.Models
{
    public class Token
    {
        public bool IsSuccess { get; set; } = false;
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string token_type { get; set; }
        public string scope { get; set; }
        public string error { get; set; }
        public string Response { get; set; }

    }
    public class TokenCkeck
    {
        public bool IsSuccess { get; set; } = false;
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string token_type { get; set; }
        public string scope { get; set; }
        public string error { get; set; }
        public string URL { get; set; }
        public string Content { get; set; }
        public List<object> Header { get; set; }

        public string Response { get; set; }

    }
}

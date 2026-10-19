using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubmitServ.APIModels
{
    public class GenericApiModel<T> where T : class
    {
        [JsonProperty("Response ID")]
        public string ResponseID { get; set; }
        [JsonProperty("Response MSG")]
        public string ResponseMSG { get; set; }
        public bool? Success { get; set; }
        [JsonProperty("responseArMsg")]
        public string ResponseArMessage { get; set; }
        [JsonProperty("responseEngMsg")]
        public string ResponseEnMessage { get; set; }
        public List<T> Data { get; set; }
        public string Title { get; set; }
    }


    public class CustomPostApiResponse<T> where T : class
    {

        public T CustomeRespons { get; set; }

    }
    public class CustomGetApiResponse<T> where T : class
    {

        public List<T> CustomeRespons { get; set; }

    }
    public class PostApiResponse
    {

        public CustomeRespons CustomeRespons { get; set; }

    }
    public class GetApiResponse
    {

        public CustomeRespons CustomeRespons { get; set; }

    }
    public class CustomeRespons
    {
        [JsonProperty("Response ID")]
        public string ResponseID { get; set; }
        [JsonProperty("Response MSG")]
        public string ResponseMSG { get; set; }

        public string Token { get; set; }
        public string CountOfAll { get; set; }
        public long? ERPInternalID { get; set; }
        public string PersonalID { get; set; }
        public long CodeExists { get; set; }
        public string HasPermission { get; set; }
    }
}

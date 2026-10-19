using Newtonsoft.Json;

namespace SubmitServ.ApiModels
{
    public class PostApiModel<T> where T : class
    {
        [JsonProperty("Response ID")]
        public string ResponseID { get; set; }
        public bool? Success { get; set; }
        [JsonProperty("responseArMsg")]
        public string ResponseArMessage { get; set; }
        [JsonProperty("responseEngMsg")]
        public string ResponseEnMessage { get; set; }
        [JsonProperty("Response MSG")]
        public string ResponseMSG { get; set; }
        public T Data { get; set; }
        public string Title { get; set; }
    }
}

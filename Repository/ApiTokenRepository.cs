using System;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using SubmitServ.Models;

namespace SubmitServ.Repository
{
    public class ApiTokenRepository
    {
        private static ApiTokenRepository _instance;

        public static ApiTokenRepository GetAPI()
        {
            _instance = new ApiTokenRepository();
            return _instance;
        }

        public HttpClient Client { get; set; }

        public ApiTokenRepository()
        {
            Client = new HttpClient();
            Client.BaseAddress = new Uri(AppSettings.EreciptURL.ToString());
        }

        public HttpResponseMessage PutResponse(string url, object model)
        {
            var payload = JsonConvert.SerializeObject(model);
            using var content = new StringContent(payload, Encoding.UTF8, "application/json");
            return Client.PutAsync(url, content).Result;
        }

        public HttpResponseMessage PostResponse(string url, object model)
        {
            var payload = JsonConvert.SerializeObject(model);
            using var content = new StringContent(payload, Encoding.UTF8, "application/json");
            return Client.PostAsync(url, content).Result;
        }

        public HttpResponseMessage DeleteResponse(string url)
        {
            return Client.DeleteAsync(url).Result;
        }

        public HttpResponseMessage GetResponse(string url)
        {
            return Client.GetAsync(url).Result;
        }
    }
}

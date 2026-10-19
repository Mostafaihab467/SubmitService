using Newtonsoft.Json;
using SubmitServ.ApiModels;
using SubmitServ.APIModels;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SubmitServ.Helpers
{
    public class ResponseResolver<T> where T : class
    {
        //private readonly IHttpContextAccessor _httpContextAccessor = Activator.CreateInstance<HttpContextAccessor>();

        public ResponseResolver()
        {
        }
        public async Task<PostApiModel<T>> ResolvePostResponse(HttpResponseMessage response)
        {
            string reasonPhrase = "Error Occured While Executing";
            string content = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                PostApiModel<T> ret = new PostApiModel<T>();
                ret.Success = false;
                ret.ResponseEnMessage = "Not Found";
                return ret;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PostApiModel<T> ret = new PostApiModel<T>();
                ret.Success = false;
                ret.ResponseEnMessage = "Unauthorized";
                return ret;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            {
                PostApiModel<T> ret = new PostApiModel<T>();
                ret.Success = false;
                ret.ResponseEnMessage = "ServiceUnavailable";

                return ret;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                PostApiModel<T> ret = new PostApiModel<T>();
                ret.Success = false;
                ret.ResponseEnMessage = "Error Occured While Executing";
                return ret;
            }

            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<PostApiModel<T>>(content);

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var result = JsonConvert.DeserializeObject<PostApiModel<T>>(content);
                if (string.IsNullOrWhiteSpace(result.Title))
                {
                    string ResponseEnMessage = "";
                    PostApiModel<T> ret = new PostApiModel<T>();
                    ResponseEnMessage = result.ResponseEnMessage;
                    ret.Success = false;
                    ret.ResponseEnMessage = ResponseEnMessage;
                    return ret;
                }
            }

            throw new Exception(reasonPhrase);
        }
        public async Task<GenericApiModel<T>> ResolveResponse(HttpResponseMessage response)
        {
            string reasonPhrase = "Error Occured While Executing";
            string content = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                GenericApiModel<T> ret = new GenericApiModel<T>();
                ret.Success = false;
                ret.ResponseEnMessage = "Not Found";
                return ret;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                GenericApiModel<T> ret = new GenericApiModel<T>();
                ret.Success = false;
                ret.ResponseEnMessage = "Unauthorized";
                return ret;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                GenericApiModel<T> ret = new GenericApiModel<T>();
                ret.Success = false;
                ret.ResponseEnMessage = "Error Occured While Executing";
                return ret;
            }

            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<GenericApiModel<T>>(content);

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var result = JsonConvert.DeserializeObject<GenericApiModel<T>>(content);
                if (string.IsNullOrWhiteSpace(result.Title))
                {
                    string ResponseEnMessage = "";
                    GenericApiModel<T> ret = new GenericApiModel<T>();
                    ResponseEnMessage = result.ResponseEnMessage;
                    ret.Success = false;
                    ret.ResponseEnMessage = ResponseEnMessage;
                    return ret;
                }
            }

            throw new Exception(reasonPhrase);
        }

        public async Task<PostApiModel<T>> GetResponseAsObject(HttpResponseMessage response)
        {
            string reasonPhrase = "Error Occured While Executing";
            string content = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                PostApiModel<T> ret = new PostApiModel<T>();
                ret.Success = false;
                ret.ResponseEnMessage = "Not Found";
                return ret;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PostApiModel<T> ret = new PostApiModel<T>();
                ret.Success = false;
                ret.ResponseEnMessage = "Unauthorized";
                return ret;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                PostApiModel<T> ret = new PostApiModel<T>();
                ret.Success = false;
                ret.ResponseEnMessage = "Error Occured While Executing";
                return ret;
            }

            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<PostApiModel<T>>(content);



            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var result = JsonConvert.DeserializeObject<PostApiModel<T>>(content);
                if (string.IsNullOrWhiteSpace(result.Title))
                {
                    string ResponseEnMessage = "";
                    PostApiModel<T> ret = new PostApiModel<T>();
                    ResponseEnMessage = result.ResponseEnMessage;
                    ret.Success = false;
                    ret.ResponseEnMessage = ResponseEnMessage;
                    return ret;



                }
            }

            throw new Exception(reasonPhrase);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SubmitServ.Models;

namespace SubmitServ.Repository
{
    public class GetTokenRepository
    {
        private static GetTokenRepository _instance;

        public static GetTokenRepository Get()
        {
            if (_instance == null)
            {
                _instance = new GetTokenRepository();
            }

            return _instance;
        }

        public async Task<Token> GetTokenAsync()
        {
            try
            {
                var client = new HttpClient();
                var form = new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" },
                    { "client_id", AppSettings.client_id },
                    { "client_secret", AppSettings.client_secret }
                };

                client.DefaultRequestHeaders.Add("pososversion", AppSettings.pososversion);
                client.DefaultRequestHeaders.Add("posserial", AppSettings.posserial);

                System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                var tokenResponse = await client.PostAsync(AppSettings.ETATokenURL, new FormUrlEncodedContent(form));
                var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
                var token = JsonConvert.DeserializeObject<Token>(tokenJson);
                token.IsSuccess = true;

                return token;
            }
            catch (Exception ex)
            {
                Token token = new Token();
                token.error = Convert.ToString(ex.InnerException);
                return token;
            }
        }
    }
}

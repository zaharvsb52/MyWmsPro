using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net.Http;
using MLC.SvcClient;
using Newtonsoft.Json;
using wmsMLC.General;

namespace MLC.WebClient.Impl
{
    public class WmsAuthService : IAuthService
    {
        class AuthResult
        {
            [JsonProperty("success")]
            public bool Success { get; set; }

            [JsonProperty("userCode")]
            public string UserCode { get; set; }
        }

        private readonly string _baseUrl;
        private readonly IHttpClientStore _httpClientStore;

        public WmsAuthService(string baseUrl, IHttpClientStore httpClientStore)
        {
            Contract.Requires(baseUrl != null);
            Contract.Requires(httpClientStore != null);

            _baseUrl = baseUrl;
            _httpClientStore = httpClientStore;
        }

        public bool Authenticate(string login, string password, out string userCode)
        {
            var client = _httpClientStore.GetOrCreate(_baseUrl);
            var content = new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    { "login", login },
                    { "password", password }
                });
            var res = client.PostAsync("security/login", content).Result;
            res.EnsureSuccessStatusCode();

            var result = JsonConvert.DeserializeObject<AuthResult>(res.Content.ReadAsStringAsync().Result);
            if (result.Success)
            {
                userCode = result.UserCode;
                return true;
            }

            userCode = null;
            return false;
        }

        public void LogOff()
        {
            var client = _httpClientStore.GetOrCreate(_baseUrl);
            var res = client.PostAsync("security/logout", null).Result;
            res.EnsureSuccessStatusCode();
        }
    }
}
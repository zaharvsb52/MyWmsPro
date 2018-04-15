using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Web;
using System.Text;
using MLC.SvcClient.Impl.ExtDirect.Dto;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MLC.SvcClient.Impl.ExtDirect
{
    public class ExtDirectProvider: IProvider, IDisposable
    {
        #region .  Fields  .
        private readonly string _rpcPath;
        private readonly HttpClient _httpClient;
        private readonly IMetadata _metadata;
        #endregion

        public ExtDirectProvider(string baseUrl, string path, IHttpClientStore httpClientStore)
        {
            Contract.Requires(!string.IsNullOrEmpty(baseUrl));
            Contract.Requires(httpClientStore != null);

            _rpcPath = (baseUrl.Last() == '/' ? baseUrl : baseUrl + "/") + path;
            _httpClient = httpClientStore.GetOrCreate(baseUrl);

            _metadata = GetMetadata(_rpcPath);
        }

        #region .  IProvider  .

        public object Execute(Transaction transaction)
        {
            var directResponse = ProcessTransaction(transaction);
            var res = directResponse.Result.ToObject(transaction.ResultType);
            return res;
        }

        public TResult Execute<TResult>(Transaction transaction)
        {
            var directResponse = ProcessTransaction(transaction);
            var res = directResponse.Result.ToObject<TResult>();
            return res;
        }

        public void ExecuteNonQuery(Transaction transaction)
        {
            throw new NotImplementedException();
        }

        public IMetadata GetMetadata()
        {
            return _metadata;
        }

        #endregion

        private DirectResponse ProcessTransaction(Transaction transaction)
        {
            var content = CreateContent(transaction);
            var response = _httpClient.PostAsync(_rpcPath, content).Result;
            if (!response.IsSuccessStatusCode)
                throw new WebFaultException(response.StatusCode);

            var result = response.Content.ReadAsStringAsync().Result;
            var directResponse = JsonConvert.DeserializeObject<DirectResponse>(result);

            CheckOnException(directResponse);
            return directResponse;
        }

        private IMetadata GetMetadata(string url)
        {
            var correctUrl = url + "?format=json";
            var res = _httpClient.GetStringAsync(correctUrl).Result;
            var config = JsonConvert.DeserializeObject<ExtDirectServiceConfig>(res);
            return new ExtDirectServiceMetadata(config);
        }

        private void CheckOnException(DirectResponse directResponse)
        {
            if (!directResponse.Error.HasValues)
                return;

            var errorDesc = directResponse.Error.ToObject<ErrorDescriptor>();
            throw new ServerException(errorDesc);
        }

        private HttpContent CreateContent(Transaction transaction)
        {
            var request = new DirectRequest
            {
                Action = transaction.Action,
                Method = transaction.Method,
                Type = "rpc"
            };

            if (transaction.Parameters != null)
                request.JsonData = JToken.FromObject(transaction.Parameters.ToDictionary(i => i.Name, i => i.Value));

            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            return content;
        }

        public void Dispose()
        {
            if (_httpClient != null)
                _httpClient.Dispose();
        }
    }
}
using System;
using System.Collections.Concurrent;
using System.Net.Http;

namespace MLC.SvcClient.Impl
{
    public class HttpClientStore : IHttpClientStore
    {
        private readonly ConcurrentDictionary<string, HttpClient> _store;

        public HttpClientStore()
        {
            _store = new ConcurrentDictionary<string, HttpClient>();
        }

        public HttpClient GetOrCreate(string baseUrl)
        {
            return _store.GetOrAdd(baseUrl, CreateHttpClient(baseUrl));
        }

        public HttpClient AddOrUpdate(HttpClient httpClient)
        {
            return _store.AddOrUpdate(GetKey(httpClient), key => httpClient, (key, prev) => httpClient);
        }

        public void Remove(HttpClient httpClient)
        {
            _store.AddOrUpdate(GetKey(httpClient), key => httpClient, (key, prev) => httpClient);
        }

        private HttpClient CreateHttpClient(string baseUrl)
        {
            var res = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };
            return res;
        }

        private string GetKey(HttpClient httpClient)
        {
            return httpClient.BaseAddress.AbsoluteUri;
        }
    }
}
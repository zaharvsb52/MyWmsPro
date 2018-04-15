using System.Diagnostics.Contracts;
using System.Net.Http;

namespace MLC.SvcClient
{
    /// <summary>
    /// Хранилище инициализированных HttpClient-ов. Обеспечивает единый контекс для передачи httpClient-ов
    /// </summary>
    [ContractClass(typeof (IHttpClientStoreContract))]
    public interface IHttpClientStore
    {
        HttpClient GetOrCreate(string baseUrl);

        HttpClient AddOrUpdate(HttpClient httpClient);
        void Remove(HttpClient httpClient);
    }

    [ContractClassFor(typeof (IHttpClientStore))]
    abstract class IHttpClientStoreContract : IHttpClientStore
    {
        HttpClient IHttpClientStore.GetOrCreate(string baseUrl)
        {
            Contract.Requires(baseUrl != null);
            Contract.Ensures(Contract.Result<HttpClient>() != null);

            throw new System.NotImplementedException();
        }

        HttpClient IHttpClientStore.AddOrUpdate(HttpClient httpClient)
        {
            Contract.Requires(httpClient != null);
            Contract.Ensures(Contract.Result<HttpClient>() != null);
            throw new System.NotImplementedException();
        }

        void IHttpClientStore.Remove(HttpClient httpClient)
        {
            Contract.Requires(httpClient != null);
            throw new System.NotImplementedException();
        }
    }
}
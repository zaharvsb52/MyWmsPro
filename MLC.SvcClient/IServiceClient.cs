using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace MLC.SvcClient
{
    /// <summary>
    /// Универсальный клиент. Реализует универсальную логику взаимодействия клиентской части с серверной
    /// </summary>
    [ContractClass(typeof (IServiceClientContract))]
    public interface IServiceClient
    {
        void Exec(string action, string method, Parameter[] parameters);
        Task ExecAsync(string action, string method, Parameter[] parameters);

        TResult Exec<TResult>(string action, string method, Parameter[] parameters);
        Task<TResult> ExecAsync<TResult>(string action, string method, Parameter[] parameters);

        object Exec(string action, string method, Parameter[] parameters, Type resultType);
        Task<object> ExecAsync(string action, string method, Parameter[] parameters, Type resultType);

        dynamic AsDynamic();
    }

    [ContractClassFor(typeof(IServiceClient))]
    abstract class IServiceClientContract : IServiceClient
    {
        void IServiceClient.Exec(string action, string method, Parameter[] parameters)
        {
            Contract.Requires(action != null);
            Contract.Requires(method != null);

            throw new NotImplementedException();
        }

        Task IServiceClient.ExecAsync(string action, string method, Parameter[] parameters)
        {
            Contract.Requires(action != null);
            Contract.Requires(method != null);

            throw new NotImplementedException();
        }

        TResult IServiceClient.Exec<TResult>(string action, string method, Parameter[] parameters)
        {
            Contract.Requires(action != null);
            Contract.Requires(method != null);

            throw new NotImplementedException();
        }

        Task<TResult> IServiceClient.ExecAsync<TResult>(string action, string method, Parameter[] parameters)
        {
            Contract.Requires(action != null);
            Contract.Requires(method != null);

            throw new NotImplementedException();
        }

        object IServiceClient.Exec(string action, string method, Parameter[] parameters, Type resultType)
        {
            Contract.Requires(action != null);
            Contract.Requires(method != null);

            throw new NotImplementedException();
        }

        Task<object> IServiceClient.ExecAsync(string action, string method, Parameter[] parameters, Type resultType)
        {
            Contract.Requires(action != null);
            Contract.Requires(method != null);

            throw new NotImplementedException();
        }

        dynamic IServiceClient.AsDynamic()
        {
            throw new System.NotImplementedException();
        }
    }
}
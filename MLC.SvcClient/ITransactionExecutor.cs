using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace MLC.SvcClient
{
    /// <summary>
    /// Удобная обертка для запуска транзакций в fluent стиле
    /// </summary>
    [ContractClass(typeof (ITransactionExecutorContract))]
    public interface ITransactionExecutor
    {
        ITransactionExecutor AddParameter<TParamType>(string name, TParamType value);

        TResult Process<TResult>();
        object Process();
        Task<TResult> ProcessAsync<TResult>();
        Task ProcessAsync();
    }

    [ContractClassFor(typeof (ITransactionExecutor))]
    abstract class ITransactionExecutorContract : ITransactionExecutor
    {
        ITransactionExecutor ITransactionExecutor.AddParameter<TParamType>(string name, TParamType value)
        {
            Contract.Requires(!string.IsNullOrEmpty(name));
            throw new System.NotImplementedException();
        }

        TResult ITransactionExecutor.Process<TResult>()
        {
            throw new System.NotImplementedException();
        }

        object ITransactionExecutor.Process()
        {
            throw new System.NotImplementedException();
        }

        Task<TResult> ITransactionExecutor.ProcessAsync<TResult>()
        {
            throw new System.NotImplementedException();
        }

        Task ITransactionExecutor.ProcessAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace MLC.SvcClient
{
    /// <summary>
    /// Управляет провайдерами и пробрасывает в них транзакции
    /// </summary>
    [ContractClass(typeof (IManagerContract))]
    public interface IManager
    {
        void AddProvider(IProvider provider);

        void RemoveProvider(IProvider provider);

        object ProcessTransaction(Transaction transaction);

        Task<object> ProcessTransactionAsync(Transaction transaction);

        TResult ProcessTransaction<TResult>(Transaction transaction);

        Task<TResult> ProcessTransactionAsync<TResult>(Transaction transaction);

        void ProcessOneWayTransaction(Transaction transaction);

        Task ProcessOneWayTransactionAsync(Transaction transaction);
    }

    [ContractClassFor(typeof (IManager))]
    abstract class IManagerContract : IManager
    {
        void IManager.AddProvider(IProvider provider)
        {
            Contract.Requires(provider != null);
            throw new System.NotImplementedException();
        }

        void IManager.RemoveProvider(IProvider provider)
        {
            Contract.Requires(provider != null);
            throw new System.NotImplementedException();
        }

        object IManager.ProcessTransaction(Transaction transaction)
        {
            Contract.Requires(transaction != null);
            throw new System.NotImplementedException();
        }

        Task<object> IManager.ProcessTransactionAsync(Transaction transaction)
        {
            Contract.Requires(transaction != null);
            throw new System.NotImplementedException();
        }

        TResult IManager.ProcessTransaction<TResult>(Transaction transaction)
        {
            Contract.Requires(transaction != null);
            throw new System.NotImplementedException();
        }

        Task<TResult> IManager.ProcessTransactionAsync<TResult>(Transaction transaction)
        {
            Contract.Requires(transaction != null);
            throw new System.NotImplementedException();
        }

        void IManager.ProcessOneWayTransaction(Transaction transaction)
        {
            Contract.Requires(transaction != null);
            throw new System.NotImplementedException();
        }

        Task IManager.ProcessOneWayTransactionAsync(Transaction transaction)
        {
            Contract.Requires(transaction != null);
            throw new System.NotImplementedException();
        }
    }
}
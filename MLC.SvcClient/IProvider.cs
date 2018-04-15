using System;
using System.Diagnostics.Contracts;

namespace MLC.SvcClient
{
    /// <summary>
    /// Реализует логику обращения к backend. Преобразование транзакции в сообщения требуемой технологии
    /// </summary>
    [ContractClass(typeof (IProviderContract))]
    public interface IProvider
    {
        object Execute(Transaction transaction);

        TResult Execute<TResult>(Transaction transaction);

        void ExecuteNonQuery(Transaction transaction);

        IMetadata GetMetadata();
    }

    [ContractClassFor(typeof (IProvider))]
    abstract class IProviderContract : IProvider
    {
        object IProvider.Execute(Transaction transaction)
        {
            Contract.Requires(transaction != null);
            throw new NotImplementedException();
        }

        TResult IProvider.Execute<TResult>(Transaction transaction)
        {
            Contract.Requires(transaction != null);
            throw new NotImplementedException();
        }

        void IProvider.ExecuteNonQuery(Transaction transaction)
        {
            Contract.Requires(transaction != null);
            throw new NotImplementedException();
        }

        IMetadata IProvider.GetMetadata()
        {
            Contract.Ensures(Contract.Result<IMetadata>() != null);
            throw new NotImplementedException();
        }
    }
}
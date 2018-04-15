using System;
using System.Diagnostics.Contracts;

namespace wmsMLC.General.DAL
{
    [ContractClass(typeof(IUnitOfWorkUserContract))]
    public interface IUnitOfWorkUser
    {
        void SetUnitOfWork(IUnitOfWork uow, bool dispose);
    }

    [ContractClassFor(typeof(IUnitOfWorkUser))]
    abstract class IUnitOfWorkUserContract : IUnitOfWorkUser
    {
        public void SetUnitOfWork(IUnitOfWork uow, bool dispose)
        {
            Contract.Requires<ArgumentNullException>(uow != null);
        }
    }
}
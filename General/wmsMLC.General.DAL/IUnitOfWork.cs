using System;
using System.Data;

namespace wmsMLC.General.DAL
{
    public interface IUnitOfWork : IDisposable
    {
        int? TimeOut { get; set; }

        void BeginChanges();
        void BeginChanges(IsolationLevel isolationLevel);
        void CommitChanges();
        void RollbackChanges();

        bool IsInTransaction();

        Guid GetId();

        event EventHandler<EventArgs> Began;
        event EventHandler<EventArgs> Committed;
        event EventHandler<EventArgs> Rolled;
        event EventHandler<EventArgs> Disposed;
    }
}
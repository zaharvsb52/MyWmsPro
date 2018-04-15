using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Xml;
using wmsMLC.General.DAL;

namespace wmsMLC.General.BL
{
    public interface ITrueBaseManager : IDisposable
    {
        void SetUnitOfWork(IUnitOfWork unitOfWork, bool dispose = false);

        event EventHandler Disposed;
    }

    public interface IBaseManager : ITrueBaseManager
    {
        bool AllowMonitorChangesInOtherInsances { get; set; }

        void Insert(ref object entity);
        void Insert(ref IEnumerable<object> entities);

        void Update(object entity);
        void Update(IEnumerable<object> entities);

        void Delete(object entity);
        void Delete(IEnumerable<object> entities);
        void DeleteByKey(object key);

        void Load(object entity);

        object New();
        object Get(object key, GetModeEnum mode = GetModeEnum.Full);
        object Get(object key, string attrentity);
        IEnumerable<object> GetAll(GetModeEnum mode = GetModeEnum.Full);
        IEnumerable<object> GetFiltered(string filter, GetModeEnum mode = GetModeEnum.Full);
        IEnumerable<object> GetFiltered(string filter, string attrentity);

        void ClearCache();

        event NotifyCollectionChangedEventHandler Changed;
        void RiseManagerChanged();
        void RiseManagerChanged(NotifyCollectionChangedAction action, IList changedItems);
        void SuspendNotifications();
        void ResumeNotifications();

        /// <summary>
        /// Время выполнения последнего запроса.
        /// </summary>
        double LastQueryExecutionTime { get; }
    }

    public interface IBaseManager<T> : IBaseManager
    {
        void Insert(ref T entity);
        void Insert(ref IEnumerable<T> entities);

        void Update(T entity);
        void Update(IEnumerable<T> entities);

        void Update(XmlDocument xmlDoc);

        void Update(IEnumerable<XmlDocument> xmlDocuments);

        void Delete(T entity);
        void Delete(IEnumerable<T> entities);

        void Load(T entity);

        new T New();
        new T Get(object key, GetModeEnum mode = GetModeEnum.Full);
        new T Get(object key, string attrentity);
        new IEnumerable<T> GetAll(GetModeEnum mode = GetModeEnum.Full);
        new IEnumerable<T> GetFiltered(string filter, GetModeEnum mode = GetModeEnum.Full);
        new IEnumerable<T> GetFiltered(string filter, string attrentity);
    }

    public interface IBaseManager<T, in TKey> : IBaseManager<T>
    {
        T Get(TKey key, GetModeEnum mode = GetModeEnum.Full);
        T Get(TKey key, string attrentity);

        void DeleteByKey(TKey key);
    }
    public interface IHistoryManager : IBaseManager
    {
        IEnumerable<object> GetHistory(string filter, string attrentity = null);
        IEnumerable<object> GetHistory(string filter, GetModeEnum mode = GetModeEnum.Full);
    }

    public interface IHistoryItem
    {
    }
}
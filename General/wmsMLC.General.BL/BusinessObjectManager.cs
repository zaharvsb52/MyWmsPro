using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Xml;
using wmsMLC.General.DAL;

namespace wmsMLC.General.BL
{
    public class BusinessObjectManager<T, TKey> : IBaseManager<T, TKey>, ICacheable
        where T : class
    {
        #region .  Fields&Consts  .

        public const string NewMethodName = "New";
        public const string InsertMethodName = "Insert";
        public const string UpdateMethodName = "Update";
        public const string DeleteMethodName = "Delete";
        public const string LoadMethodName = "Load";
        public const string GetMethodName = "Get";
        public const string GetAllMethodName = "GetAll";
        public const string GetFilteredMethodName = "GetFiltered";
        public const string ClearCacheMethodName = "ClearCache";

        private bool _enableNotifyCollectionChanged = true;
        private bool _isDisposeUnitOfWork = true;
        private IUnitOfWork _currentUnitOfWork;
        //private static long _nCount = 0;
        private static bool _isInStaticNotification;

        private bool _allowMonitorChangesInOtherInsances;

        protected List<T> DeletedObjects = new List<T>();
        protected List<T> ObjectsForAccept = new List<T>();

        #endregion

        public BusinessObjectManager()
        {
            IsEnableClearCacheForInternalObject = true;

            RepositoryFactory = IoC.Instance.Resolve<IRepositoryFactory>();
            UnitOfWorkFactory = IoC.Instance.Resolve<IUnitOfWorkFactory>();
        }

        /// <summary>
        /// Признак того, что данный менеджер будет следить за изменениями всех инстансов данного класа
        /// </summary>
        public bool AllowMonitorChangesInOtherInsances
        {
            get { return _allowMonitorChangesInOtherInsances; }
            set
            {
                if (_allowMonitorChangesInOtherInsances == value)
                    return;

                _allowMonitorChangesInOtherInsances = value;

                if (_allowMonitorChangesInOtherInsances)
                    NotifyManagerChanged += NotifyManagerChangedListener;
                else
                    NotifyManagerChanged -= NotifyManagerChangedListener;
            }
        }

        #region .  Dispose  .

        public void Dispose()
        {
            try
            {
                // отписываемся от статического события
                NotifyManagerChanged -= NotifyManagerChangedListener;

                // отписываемся от UoW
                if (CurrentUnitOfWork != null)
                    ReleaseCurrentUnitOfWork();

                OnDisposed();
            }
            catch
            {
                // ничего не делаем - dispose должен проходить без ошибок
                // мы толком ничего и не сделаем
            }
        }

        private void OnDisposed()
        {
            var handler = Disposed;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public event EventHandler Disposed;

        #endregion

        #region .  Properties  .

        public bool IsEnableClearCacheForInternalObject { get; set; }

        /// <summary>
        /// Признак разрешения очистки кэша перед выполнением Get-запросов
        /// </summary>
        protected bool IsEnableClearCacheBeforeGet { get; set; }

        protected IRepositoryFactory RepositoryFactory { get; private set; }

        protected IUnitOfWorkFactory UnitOfWorkFactory { get; private set; }

        protected IUnitOfWork CurrentUnitOfWork
        {
            get { return _currentUnitOfWork; }
            private set
            {
                if (ReferenceEquals(_currentUnitOfWork, value))
                    return;

                if (_currentUnitOfWork != null)
                    UnSubscribeOnUnitOfWork(_currentUnitOfWork);

                _currentUnitOfWork = value;

                if (_currentUnitOfWork != null)
                    SubscribeOnUnitOfWork(_currentUnitOfWork);
            }
        }

        #endregion

        protected IUnitOfWork GetUnitOfWork(out bool isNeedDispose, bool longSession = false)
        {
            isNeedDispose = _isDisposeUnitOfWork;
            if (CurrentUnitOfWork != null)
                return CurrentUnitOfWork;

            isNeedDispose = true;
            var uow = UnitOfWorkFactory.Create(!longSession);
            SubscribeOnUnitOfWork(uow);
            return uow;
        }

        private void SubscribeOnUnitOfWork(IUnitOfWork uow)
        {
            UnSubscribeOnUnitOfWork(uow);
            uow.Disposed += OnUnitOfWorkDisposed;
            uow.Committed += UnitOfWork_Committed;
            uow.Rolled += UnitOfWork_Rolled;
        }

        private void UnSubscribeOnUnitOfWork(IUnitOfWork uow)
        {
            uow.Committed -= UnitOfWork_Committed;
            uow.Rolled -= UnitOfWork_Rolled;
            uow.Disposed -= OnUnitOfWorkDisposed;
        }

        private void OnUnitOfWorkDisposed(object sender, EventArgs e)
        {
            UnSubscribeOnUnitOfWork((IUnitOfWork) sender);
            if (sender == CurrentUnitOfWork)
            {
                ReleaseCurrentUnitOfWork();
                CurrentUnitOfWork = null;
                _isDisposeUnitOfWork = true;
            }
        }

        protected void ReleaseCurrentUnitOfWork()
        {
            if (CurrentUnitOfWork == null)
            {
                if (!_isDisposeUnitOfWork)
                    throw new DeveloperException(
                        "Can't release CurrentUnitOfWork (== null && _isDisposeUnitOfWork == false).");
                return;
            }

            // если создавали не сами - ничего не делаем
            if (!_isDisposeUnitOfWork)
                return;

            // Если получили не снаружи - значит создавали сами,
            // следовательно, сами и должны подчистить
            CurrentUnitOfWork.Dispose();
            CurrentUnitOfWork = null;
            _isDisposeUnitOfWork = true;
        }

        protected virtual void UnitOfWork_Committed(object sender, EventArgs e)
        {
            ProcessAcceptChanges();
        }
        protected virtual void UnitOfWork_Rolled(object sender, EventArgs e)
        {
            ProcessRejectChanges();
        }

        private void ProcessAcceptChanges()
        {
            if (ObjectsForAccept.Count == 0 && DeletedObjects.Count == 0)
                return;

            ClearCache();

            // фиксируем изменения и сообщаем о них
            if (ObjectsForAccept.Count > 0)
            {
                foreach (T obj in ObjectsForAccept)
                {
                    var ed = obj as IEditable;
                    if (ed != null)
                        ed.AcceptChanges();
                }

                OnChanged(NotifyCollectionChangedAction.Add, ObjectsForAccept);
                ObjectsForAccept.Clear();
            }

            // фиксируем удаления и сообщаем о них
            if (DeletedObjects.Count > 0)
            {
                OnChanged(NotifyCollectionChangedAction.Remove, DeletedObjects);
                DeletedObjects.Clear();
            }
        }

        private void ProcessRejectChanges()
        {
            // откатываем изменения и сообщаем о них
            if (ObjectsForAccept.Count > 0)
            {
                foreach (T obj in ObjectsForAccept)
                {
                    var ed = obj as IEditable;
                    if (ed != null)
                        ed.RejectChanges();
                }
                OnChanged(NotifyCollectionChangedAction.Add, ObjectsForAccept);
                ObjectsForAccept.Clear();
            }
            DeletedObjects.Clear();
        }

        protected IRepository<T, TKey> GetRepository()
        {
            bool isNeedDispose;
            var uow = GetUnitOfWork(out isNeedDispose);
            return RepositoryFactory.Get<IRepository<T, TKey>>(uow, isNeedDispose);
        }

        protected TRep GetRepository<TRep>() where TRep : IBaseRepository
        {
            return (TRep)GetRepository();
        }

        #region .  IBaseManager  .

        void IBaseManager.Insert(ref object entity)
        {
            var item = (T)entity;
            Insert(ref item);
            entity = item;
        }

        void IBaseManager.Insert(ref IEnumerable<object> entities)
        {
            var obj = (IEnumerable<T>)entities;
            Insert(ref obj);
            entities = obj;
        }

        void IBaseManager.Update(object entity)
        {
            Update((T)entity);
        }

        void IBaseManager.Update(IEnumerable<object> entities)
        {
            Update((IEnumerable<T>)entities);
        }

        void IBaseManager.Delete(object entity)
        {
            Delete((T)entity);
        }

        void IBaseManager.Delete(IEnumerable<object> entities)
        {
            Delete((IEnumerable<T>)entities);
        }

        void IBaseManager.DeleteByKey(object key)
        {
            DeleteByKey((TKey)key);
        }

        void IBaseManager.Load(object entity)
        {
            Load((T)entity);
        }

        object IBaseManager.Get(object key, GetModeEnum mode)
        {
            return Get((TKey)key, mode);
        }

        object IBaseManager.Get(object key, string attrentity)
        {
            return Get((TKey)key, attrentity);
        }

        object IBaseManager.New()
        {
            return New();
        }

        IEnumerable<object> IBaseManager.GetAll(GetModeEnum mode)
        {
            return GetAll(mode);
        }

        IEnumerable<object> IBaseManager.GetFiltered(string filter, GetModeEnum mode)
        {
            return GetFiltered(filter, mode);
        }

        IEnumerable<object> IBaseManager.GetFiltered(string filter, string attrentity)
        {
            return GetFiltered(filter, attrentity);
        }

        /// <summary>
        /// Время выполнения последнего запроса.
        /// </summary>
        double IBaseManager.LastQueryExecutionTime
        {
            get
            {
                using (var repo = GetRepository())
                    return repo.LastQueryExecutionTime;
            }
        }
        #endregion  .  IBaseManager  .

        #region .  IBaseManager<T>  .

        T IBaseManager<T>.Get(object key, GetModeEnum mode)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            //HACK: сделано для SysObject'а. 
            if (typeof(TKey) == typeof(int) && key is decimal)
                key = key.To(0);
            return Get((TKey)key, mode);
        }

        T IBaseManager<T>.Get(object key, string attrentity)
        {
            return Get((TKey)key, attrentity);
        }

        #endregion

        #region .  IBaseManager<T, TKey>  .

        public virtual T New()
        {
//            try
//            {
                return Activator.CreateInstance<T>();
//            }
//            catch (Exception ex)
//            {
//                HandleException(ex);
//                return default(T);
//            }
        }

        public virtual T Get(TKey key, GetModeEnum mode)
        {
            return Get(key, GetAttrEntity(typeof(T), mode));
        }

        protected virtual string GetAttrEntity(Type type, GetModeEnum mode)
        {
            return mode == GetModeEnum.Full
                ? null
                : XmlDocumentConverter.GetShortTemplate(typeof(T)).OuterXml;
        }

        public virtual T Get(TKey key, string attrentity)
        {
            using (var repo = GetRepository())
                return repo.Get(key, attrentity);
        }

        public virtual XmlDocument GetXml(TKey key, string attrentity)
        {
            using (var repo = GetRepository())
                return repo.GetXml(key, attrentity);
        }

        public virtual IEnumerable<T> GetAll(GetModeEnum mode = GetModeEnum.Full)
        {
            return GetAll(GetAttrEntity(typeof(T), mode));
        }

        public virtual IEnumerable<T> GetAll(string attrentity)
        {
            using (var repo = GetRepository())
                return repo.GetAll(attrentity);
        }

        public virtual IEnumerable<T> GetFiltered(string filter, GetModeEnum mode)
        {
            return GetFiltered(filter, GetAttrEntity(typeof(T), mode));
        }

        public virtual IEnumerable<T> GetFiltered(string filter, string attrentity)
        {
            using (var repo = GetRepository())
                return repo.GetFiltered(filter, attrentity);
        }

        public virtual IEnumerable<XmlDocument> GetXmlFiltered(string filter, string attrentity)
        {
            using (var repo = GetRepository())
                return repo.GetXmlFiltered(filter, attrentity);
        }

        public virtual void Insert(ref T entity)
        {
            var item = entity;
            DoInCurrentUnitOfWork(uow =>
                {
                    BeforeInsert(ref item);
                    ClearCacheForInternalObject(item);
                    using (var repo = GetRepository())
                        repo.Insert(ref item);

                    // подтверждаем изменения
                    ObjectsForAccept.Add(item);
                    if (!uow.IsInTransaction())
                        ProcessAcceptChanges();
                });
            entity = item;
        }

        public virtual void Insert(ref IEnumerable<T> entities)
        {
            var items = entities;
            DoInCurrentUnitOfWork(uow =>
                {
                    BeforeInsert(ref items);
                    foreach (var entity in items)
                        ClearCacheForInternalObject(entity);
                    using (var repo = GetRepository())
                        repo.Insert(ref items);

                    // подтверждаем изменения
                    ObjectsForAccept.AddRange(items);
                    if (!uow.IsInTransaction())
                        ProcessAcceptChanges();
                });

            entities = items;
        }

        public virtual void Update(T entity)
        {
            DoInCurrentUnitOfWork(uow =>
                {
                    BeforeUpdate(ref entity);
                    ClearCacheForInternalObject(entity);

                    var serializable = entity as ICustomXmlSerializable;
                    if (serializable != null)
                        serializable.IgnoreInnerEntity = serializable.OverrideIgnore ?? true;
                    var xmlDoc = XmlDocumentConverter.ConvertFrom(entity);

                    using (var repo = GetRepository())
                        repo.Update(xmlDoc);

                    // подтверждаем изменения
                    ObjectsForAccept.Add(entity);
                    if (!uow.IsInTransaction())
                        ProcessAcceptChanges();
                });
        }

        public virtual void Update(IEnumerable<T> entities)
        {
            DoInCurrentUnitOfWork(uow =>
            {
                BeforeUpdate(ref entities);
                var xmlDocuments = new List<XmlDocument>();
                foreach (var entity in entities)
                {
                    ClearCacheForInternalObject(entity);
                    var serializable = entity as ICustomXmlSerializable;
                    if (serializable != null)
                        serializable.IgnoreInnerEntity = serializable.OverrideIgnore ?? true;

                    var xmlDoc = XmlDocumentConverter.ConvertFrom(entity);
                    xmlDocuments.Add(xmlDoc);
                }

                using (var repo = GetRepository())
                    repo.Update(xmlDocuments);

                // подтверждаем изменения
                ObjectsForAccept.AddRange(entities);
                if (!uow.IsInTransaction())
                    ProcessAcceptChanges();
            });
        }

        public virtual void Update(XmlDocument xmlDoc)
        {
            DoInCurrentUnitOfWork(uow =>
                {
                    using (var repo = GetRepository())
                        repo.Update(xmlDoc);
                });
        }

        public virtual void Update(IEnumerable<XmlDocument> xmlDocuments)
        {
            DoInCurrentUnitOfWork(uow =>
            {
                using (var repo = GetRepository())
                    repo.Update(xmlDocuments);
            });
        }

        public virtual void Delete(T entity)
        {
            DoInCurrentUnitOfWork(uow =>
                {
                    BeforeDelete(ref entity);
                    ClearCacheForInternalObject(entity);
                    using (var repo = GetRepository())
                        repo.Delete(entity);

                    DeletedObjects.Add(entity);
                    if (!uow.IsInTransaction())
                        ProcessAcceptChanges();
                });
        }

        public virtual void Delete(IEnumerable<T> entities)
        {
            DoInCurrentUnitOfWork(uow =>
                {
                    BeforeDelete(ref entities);
                    foreach (var entity in entities)
                        ClearCacheForInternalObject(entity);

                    using (var repo = GetRepository())
                        repo.Delete(entities);

                    DeletedObjects.AddRange(entities);
                    if (!uow.IsInTransaction())
                        ProcessAcceptChanges();
                });
        }

        public void DeleteByKey(TKey key)
        {
            using (var repo = GetRepository())
                repo.DeleteByKey(key);
        }

        protected void DoInCurrentUnitOfWork(Action<IUnitOfWork> action, bool useTransaction = false)
        {
            bool dispose;
            var uow = GetUnitOfWork(out dispose, useTransaction);
            var needTransaction = useTransaction && !uow.IsInTransaction();
            try
            {
                SetUnitOfWork(uow);

                if (needTransaction)
                    uow.BeginChanges();

                action(uow);

                if (needTransaction)
                    uow.CommitChanges();
            }
            catch
            {
                if (needTransaction)
                    uow.RollbackChanges();
                else
                    ProcessRejectChanges();
                throw;
            }
            finally
            {
                if (dispose)
                {
                    _isDisposeUnitOfWork = true;
                    ReleaseCurrentUnitOfWork();
                }
            }
        }

        public virtual void Load(T entity)
        {
            using (var repo = GetRepository())
                repo.Load(entity);
        }

        #endregion

        #region .  Notifications  .

        public void RiseManagerChanged()
        {
            RiseManagerChanged(NotifyCollectionChangedAction.Reset, null);
        }

        public void RiseManagerChanged(NotifyCollectionChangedAction action, IList changedItems)
        {
            var changedItemsT = (IList<T>)changedItems;
            OnChanged(action, changedItemsT);
        }

        protected bool EnableNotifyCollectionChanged
        {
            get { return _enableNotifyCollectionChanged; }
            private set { _enableNotifyCollectionChanged = value; }
        }

        /// <summary>
        /// Cобытие изменения (Вызывается как при изменении текущего инстанса, так и всех инстансов данного класса)
        /// </summary>
        public event NotifyCollectionChangedEventHandler Changed;

        //HACK: "Мега" механизм нотификации. При создании инстанса менеджер подписывается на 
        //      статическое событие класса NotifyManagerChanged. Если fire NotifyManagerChanged,
        //      то вызывается событие Changed инстанса
        protected static event NotifyCollectionChangedEventHandler NotifyManagerChanged
        {
            add
            {
                AddListener(value.Target);
                //_notifyManagerChanged += value;
                //_inst.Add(value.Target);
            }
            remove
            {
                RemoveListener(value.Target);
                //_notifyManagerChanged -= value;
                //_inst.Remove(value.Target);
            }
        }
        private static readonly List<WeakReference> items = new List<WeakReference>();

        private static void RemoveListener(object listener)
        {
            lock (items)
                items.RemoveAll(x => !x.IsAlive || x.Target == listener || x.Target == null);
        }
        private static void AddListener(object listener)
        {
            if (listener == null)
                return;

            var newRef = new WeakReference(listener);
            lock (items)
                items.Add(newRef);
        }

        private static void NotifyListeners(object sender, NotifyCollectionChangedAction action, IList<T> changedItems)
        {
            if (_isInStaticNotification)
                return;

            lock (items)
            {
                if (_isInStaticNotification)
                    return;
                try
                {
                    _isInStaticNotification = true;

                    foreach (var item in items)
                    {
                        var h = item.IsAlive ? item.Target as BusinessObjectManager<T, TKey> : null;
                        if (h != null && sender != h)
                            h.RiseManagerChanged(action, (IList)changedItems);
                    }
                }
                finally
                {
                    _isInStaticNotification = false;
                }
            }
        }

        public virtual void SuspendNotifications()
        {
            EnableNotifyCollectionChanged = false;
        }

        public virtual void ResumeNotifications()
        {
            EnableNotifyCollectionChanged = true;
        }

        private void OnChanged(NotifyCollectionChangedAction action, IList<T> changedItems)
        {
            if (!EnableNotifyCollectionChanged)
                return;

            // сначала сообщаем своим подписчикам
            var ch = Changed;
            if (ch != null)
                ch(this, new NotifyCollectionChangedEventArgs(action, changedItems));

            // сообщаем подписчикам других инстансов
            NotifyListeners(this, action, changedItems);
        }

        private void NotifyManagerChangedListener(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (sender == this ||
                sender == null ||
                sender.GetType() != GetType())
                return;

            var h = Changed;
            if (h != null)
                h(sender, e);
        }

        #endregion

        public void SetUnitOfWork(IUnitOfWork unitOfWork, bool dispose = false)
        {
            CurrentUnitOfWork = unitOfWork;
            _isDisposeUnitOfWork = dispose;
        }

        #region .  ICacheable  .
        public virtual void ClearCache()
        {
            //try
            //{
                using (var repo = GetRepository())
                {
                    var repository = repo as ICacheableRepository;
                    if (repository != null)
                        repository.ClearCache();
                }
            //}
            //catch (Exception ex)
            //{
            //    HandleException(ex);
            //}
        }

        protected void ClearCacheForInternalObject(T entity)
        {
            if (!IsEnableClearCacheForInternalObject)
                return;

            //try
            //{
                var properties = TypeDescriptor.GetProperties(typeof(T)).Cast<PropertyDescriptor>().Where(i => typeof(IEditable).IsAssignableFrom(i.PropertyType));
                foreach (var property in properties)
                {
                    if (entity != null)
                    {
                        var value = property.GetValue(entity) as IEditable;
                        if (value == null || !value.IsDirty)
                            continue;
                    }

                    var objectType = property.PropertyType;

                    //проверяем что св-во - это коллекция бизнес-объектов(вложенная сущность)
                    if (typeof(IList).IsAssignableFrom(property.PropertyType))
                    {
                        var genericType =
                            property.PropertyType.GetGenericTypeFormInheritanceNode(typeof(BusinessObjectCollection<>));
                        if (genericType == null)
                            continue;

                        objectType = genericType.GetGenericArguments()[0];
                    }

                    var mgrInterfaceType = typeof(IBaseManager<>).MakeGenericType(objectType);
                    object mgrInstance;
                    // NOTE: не все объекты имеют Manager-ы
                    if (IoC.Instance.TryResolve(mgrInterfaceType, out mgrInstance))
                    {
                        ((IBaseManager)mgrInstance).ClearCache();
                    }
                }
            //}
            //catch (Exception ex)
            //{
            //    HandleException(ex);
            //}
        }
        #endregion

        protected virtual void HandleException(Exception ex)
        {
            BlExceptionHandler.HandleException(ref ex);
            throw ex;
        }

        #region . BeforeAfterOperations .
        protected virtual void BeforeInsert(ref T entity) { }

        protected virtual void BeforeInsert(ref IEnumerable<T> entities) { }

        protected virtual void BeforeUpdate(ref T entity) { }

        protected virtual void BeforeUpdate(ref IEnumerable<T> entities) { }

        protected virtual void BeforeDelete(ref T entity) { }

        protected virtual void BeforeDelete(ref IEnumerable<T> entities) { }
        #endregion
    }
}
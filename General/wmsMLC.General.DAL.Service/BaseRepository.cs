using System;
using System.Collections.Generic;
using System.Xml;
using wmsMLC.General.DAL.Service.Properties;
using wmsMLC.General.DAL.Service.Telegrams;
using wmsMLC.General.Services;

namespace wmsMLC.General.DAL.Service
{
    public class BaseRepository<T, TKey> : BaseRepository, IRepository<T, TKey>
        where T : class, new()
    {
        // ReSharper disable once StaticFieldInGenericType
        private static double _lastQueryExecutionTime;

        public bool IsNeedClearCacheBeforGetFiltered { get; set; }

        public virtual void Insert(ref T entity)
        {
            var param = new TransmitterParam { Name = "entity", Type = typeof(T), Value = entity, IsOut = true };
            var telegram = new RepoQueryTelegramWrapper(typeof(T).Name, "Insert", new[] { param });
            ProcessTelegramm(telegram);
            entity = (T)param.Value;
        }

        public void Insert(ref IEnumerable<T> entities)
        {
            var entityParam = new TransmitterParam { Name = "entities", Type = typeof(IEnumerable<T>), Value = entities, IsOut = true };
            var telegram = new RepoQueryTelegramWrapper(typeof(T).Name, "Insert", new[] { entityParam });
            ProcessTelegramm(telegram);
        }

        public virtual void Update(T entity)
        {
            var entityParam = new TransmitterParam { Name = "entity", Type = typeof(T), Value = entity };
            var telegram = new RepoQueryTelegramWrapper(typeof(T).Name, "Update", new[] { entityParam });
            ProcessTelegramm(telegram);
        }

        public void Update(IEnumerable<T> entities)
        {
            var entityParam = new TransmitterParam { Name = "entities", Type = typeof(IEnumerable<T>), Value = entities };
            var telegram = new RepoQueryTelegramWrapper(typeof(T).Name, "Update", new[] { entityParam });
            ProcessTelegramm(telegram);
        }

        public virtual void Update(XmlDocument xmlDoc)
        {
            var xmlDocParam = new TransmitterParam { Name = "xmlDoc", Type = typeof(XmlDocument), Value = xmlDoc };
            var telegram = new RepoQueryTelegramWrapper(typeof(T).Name, "Update", new[] { xmlDocParam });
            ProcessTelegramm(telegram);
        }

        public void Update(IEnumerable<XmlDocument> xmlDocuments)
        {
            var xmlDocumentsParam = new TransmitterParam { Name = "xmlDocuments", Type = typeof(IEnumerable<XmlDocument>), Value = xmlDocuments };
            var telegram = new RepoQueryTelegramWrapper(typeof(T).Name, "Update", new[] { xmlDocumentsParam });
            ProcessTelegramm(telegram);
        }

        public virtual void Delete(T entity)
        {
            var entityParam = new TransmitterParam { Name = "entity", Type = typeof(T), Value = entity };
            var telegram = new RepoQueryTelegramWrapper(typeof(T).Name, "Delete", new[] { entityParam });
            ProcessTelegramm(telegram);
        }

        public virtual void Delete(IEnumerable<T> entities)
        {
            var entityParam = new TransmitterParam { Name = "entities", Type = typeof(IEnumerable<T>), Value = entities };
            var telegram = new RepoQueryTelegramWrapper(typeof(T).Name, "Delete", new[] { entityParam });
            ProcessTelegramm(telegram);
        }

        public virtual void DeleteByKey(TKey key)
        {
            var keyParam = new TransmitterParam { Name = "key", Type = typeof(TKey), Value = key };
            var telegram = new RepoQueryTelegramWrapper(typeof(T).Name, "DeleteByKey", new[] { keyParam });
            ProcessTelegramm(telegram);
        }

        public virtual void Load(T entity)
        {
            throw new NotImplementedException();
        }

        public virtual T Get(TKey key, string attrentity)
        {
            var xmlItem = GetXml(key, attrentity);
            return XmlDocumentConverter.ConvertTo<T>(xmlItem);
        }

        public virtual XmlDocument GetXml(TKey key, string attrentity)
        {
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(XmlDocument), IsOut = true };
            var keyParam = new TransmitterParam { Name = "key", Type = typeof(TKey), Value = key };
            var attrentityParam = new TransmitterParam { Name = "attrentity", Type = typeof(string), Value = attrentity };
            
            var telegram = new RepoQueryTelegramWrapper(typeof(T).Name, "GetXml", new[] { resultParam, keyParam, attrentityParam });
            ProcessTelegramm(telegram);
            return (XmlDocument)resultParam.Value;
        }

        public virtual List<T> GetAll(string attrentity = null)
        {
            var xmlItems = GetXmlFiltered(null, attrentity);
            return XmlDocumentConverter.ConvertToListOf<T>(xmlItems);
        }

        public virtual List<T> GetFiltered(string filter, string attrentity)
        {
            var xmlItems = GetXmlFiltered(filter, attrentity);
            return XmlDocumentConverter.ConvertToListOf<T>(xmlItems);
        }

        public virtual List<XmlDocument> GetXmlFiltered(string filter, string attrentity)
        {
            ClearStatistics();
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(List<XmlDocument>), IsOut = true };
            var filterParam = new TransmitterParam { Name = "filter", Type = typeof(string), Value = filter };
            var attrentityParam = new TransmitterParam { Name = "attrentity", Type = typeof(string), Value = attrentity };

            var telegram = new RepoQueryTelegramWrapper(typeof (T).Name, "GetXmlFiltered", new[] {resultParam, filterParam, attrentityParam});
            ProcessTelegramm(telegram);
            _lastQueryExecutionTime = telegram.LastQueryExecutionTime;
            return (List<XmlDocument>)resultParam.Value;
        }

        /// <summary>
        /// Время выполнения последнего запроса.
        /// </summary>
        public virtual double LastQueryExecutionTime { get { return _lastQueryExecutionTime; } }

        public void ClearStatistics()
        {
            _lastQueryExecutionTime = 0;
        }

        public void ChangeStateByKey(object entityKey, string operationName)
        {
            var entityKeyParam = new TransmitterParam { Name = "entityKey", Type = typeof(object), Value = entityKey };
            var operationNameParam = new TransmitterParam { Name = "operationName", Type = typeof(string), Value = operationName };
            var telegram = new RepoQueryTelegramWrapper(typeof(T).Name, "ChangeStateByKey", new[] { entityKeyParam, operationNameParam });
            ProcessTelegramm(telegram);
        }
    }

    public class BaseRepository : IDisposable
    {
        protected static int DefaultTimeout = 30000;
        protected IUnitOfWork UnitOfWork;
        protected bool _needDispose;
        protected int? TimeOut;

        static BaseRepository()
        {
            // выставляем время ожидания из настроек
            DefaultTimeout = Settings.Default.DefaultTimeOutInMS;
        }

        private static ITransmitter GetTransmitter()
        {
            return IoC.Instance.Resolve<ITransmitter>();
        }

        public void SetUnitOfWork(IUnitOfWork uow, bool dispose)
        {
            UnitOfWork = uow;
            TimeOut = UnitOfWork.TimeOut;
            _needDispose = dispose;
        }

        protected void ProcessTelegramm(Telegram telegram, TelegramBodyType bodyType = TelegramBodyType.Wms)
        {
            // выставляем uow для управления транзакциями
            if (UnitOfWork != null)
            {
                telegram.UnitOfWork = UnitOfWork.GetId();
            }

            telegram.TimeOut = TimeOut?? DefaultTimeout;

            using (var transmitter = GetTransmitter())
                transmitter.Process(bodyType, telegram);



            //// Непосредственную отправку делаем в отдельном потоке.
            //// Такой подход необходим, т.к. если отправка будет осуществляться из UI, то произойдет DeadLock
            //// Бонусом получаем удобный механизм отслеживания TimeOut-ов
            //var processTask = new Task(() =>
            //{
            //    using (var transmitter = GetTransmitter())
            //    {
            //        transmitter.Process(bodyType, telegram);
            //    }
            //});


            //var waitTime = TimeOut ?? DefaultTimeout;

            //processTask.Start();
            //if (!processTask.Wait(waitTime == 0 ? -1: waitTime))
            //    throw new TimeoutException(ExceptionResources.TimeoutExceptionMessage);
        }

        public void Dispose()
        {
            if (_needDispose && UnitOfWork != null)
                UnitOfWork.Dispose();
        }
    }
}
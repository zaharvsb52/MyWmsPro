using System;
using System.Collections.Generic;
using System.Data;
using System.Xml;
using BLToolkit.Aspects;

namespace wmsMLC.General.DAL.Oracle
{
    /// <summary> Abstract implementer of IRepository interface. </summary>
    /// <typeparam name="T">Repository entity type</typeparam>
    /// <typeparam name="TKey">Entity key type</typeparam>
    public abstract class Repository<T, TKey> : BaseXmlRepository<T, TKey>, IRepository<T, TKey>
        where T : class, new()
    {
        #region .  Public methods  .

        public virtual void DeleteByKey(TKey key)
        {
            RunManualDbOperation(db => { XmlDelete(key); return 0; });
        }

        public virtual void Insert(ref T obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            var res = obj;
            var xmlDoc = XmlDocumentConverter.ConvertFrom(res);
            RunManualDbOperation(db =>
            {
                TKey key;

                // сохраняем
                XmlInsert(xmlDoc, out key);

                // гарантированно сбрасываем кэш, т.к. до insert-а могли сделать get с данным id
                CacheAspect.ClearCache(GetType(), "Get");

                // перевычитываем
                res = Get(key, null);
                if (res == null)
                    throw new DeveloperException("Не удалось получить только-что добавленный объект. Проверьте, что процедуры возвращают правильный ключ");

                return 0;
            }, true);

            obj = res;
        }

        public virtual void Insert(ref IEnumerable<T> entities)
        {
            //NOTE: пока умеем работать только с IList<> (нам нужно возвращать новые значения по ref) потому enumerable - не подходит
            var list = entities as IList<T>;
            if (list == null)
                throw new DeveloperException("Insert can work only with IList<> yet.");

            RunManualDbOperation(db =>
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var entity = list[i];
                    Insert(ref entity);
                    list[i] = entity;
                }

                return 0;
            }, true);
        }

        public virtual void Update(T obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            var xmlDoc = XmlDocumentConverter.ConvertFrom(obj);
            Update(xmlDoc);
        }

        public virtual void Update(XmlDocument xmlDoc)
        {
            if (xmlDoc == null)
                throw new ArgumentNullException("xmlDoc");

            XmlUpdate(xmlDoc);
        }

        public virtual void Update(IEnumerable<T> entities)
        {
            RunManualDbOperation(db =>
            {
                foreach (var entity in entities)
                    Update(entity);

                return 0;
            }, true);
        }

        public virtual void Update(IEnumerable<XmlDocument> xmlDocuments)
        {
            RunManualDbOperation(db =>
            {
                foreach (var xmlDoc in xmlDocuments)
                    Update(xmlDoc);

                return 0;
            }, true);
        }

        public virtual void Delete(T obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            var kh = obj as IKeyHandler;
            if (kh == null)
                throw new DeveloperException("Can't get key from essense {0}.", obj);

            var key = kh.GetKey();
            if (!(key is TKey))
                throw new DeveloperException("Не совпадают типы ключа в определении ({0}) и объекте ({1}) для типа {2}", typeof(TKey).Name, key.GetType().Name, typeof(T).Name);

            DeleteByKey((TKey)key);
        }

        public virtual void Delete(IEnumerable<T> entities)
        {
            if (entities == null)
                throw new ArgumentNullException("entities");

            RunManualDbOperation(db =>
            {
                foreach (var entity in entities)
                    Delete(entity);
                return 0;
            }, true);
        }

        public virtual T Get(TKey key, string attrentity)
        {
            ClearStatistics();

            var attr = XmlDocumentConverter.XmlDocFromString(attrentity);
            var xmlDoc = XmlGet(key, attr);
            if (xmlDoc == null)
                return null;

            return XmlDocumentConverter.ConvertTo<T>(xmlDoc);
        }

        public virtual XmlDocument GetXml(TKey key, string attrentity)
        {
            ClearStatistics();

            var attr = XmlDocumentConverter.XmlDocFromString(attrentity);
            return XmlGet(key, attr);
        }

        public virtual List<T> GetAll(string attrentity = null)
        {
            return GetFiltered(null, attrentity);
        }

        public virtual List<T> GetFiltered(string filter, string attrentity)
        {
            ClearStatistics();

            // если фильтр - это процедура
            if (!string.IsNullOrEmpty(filter) && filter.StartsWith("pkg"))
                return GetListTFromFunction(filter);

            var xmlDocuments = GetXmlFiltered(filter, attrentity);
            if (xmlDocuments == null)
                return null;

            return XmlDocumentConverter.ConvertToListOf<T>(xmlDocuments);
        }

        public virtual List<XmlDocument> GetXmlFiltered(string filter, string attrentity)
        {
            ClearStatistics();

            if (!string.IsNullOrEmpty(filter) && filter.StartsWith("pkg"))
            {
                var items = GetListTFromFunction(filter);
                return XmlDocumentConverter.ConvertFromListOf(items);
            }

            var attr = XmlDocumentConverter.XmlDocFromString(attrentity);
            return XmlGetList(attr, filter);
        }

        public virtual void Load(T obj)
        {
            throw new NotImplementedException("Yet");
        }

        public void ChangeStateByKey(object entityKey, string operationName)
        {
            ClearStatistics();

            var key = (TKey)Convert.ChangeType(entityKey, typeof(TKey));

            RunManualDbOperation(db => db.SetCommand(CommandType.StoredProcedure, "pkgStateMachine.setInstanceNextState",
                db.InputParameter("pEntity", GetObjectName().ToUpper()),
                db.InputParameter("pKey", key.ToString()),
                db.InputParameter("pOperationCode", operationName),
                db.InputParameter("pCurrState", null),
                db.InputParameter("pNextState", null),
                db.InputParameter("pCheckOperation", 1),
                db.InputParameter("pRaiseException", 1))
                .ExecuteNonQuery());
        }

        #endregion
    }
}
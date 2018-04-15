using System.Collections.Generic;
using System.Xml;

namespace wmsMLC.General.DAL
{
    public interface IRepository<T, in TKey> : IBaseRepository
    {
        void Insert(ref T entity);
        void Insert(ref IEnumerable<T> entities);

        void Update(T entity);
        void Update(IEnumerable<T> entities);

        void Update(XmlDocument xmlDoc);

        void Update(IEnumerable<XmlDocument> xmlDocuments);

        void Delete(T entity);
        void Delete(IEnumerable<T> entities);

        void DeleteByKey(TKey key);

        void Load(T entity);

        T Get(TKey key, string attrentity);
        
        XmlDocument GetXml(TKey key, string attrentity);

        List<T> GetFiltered(string filter, string attrentity);
        List<T> GetAll(string attrentity = null);
        
        List<XmlDocument> GetXmlFiltered(string filter, string attrentity);

        /// <summary>
        /// Время выполнения последнего запроса.
        /// </summary>
        double LastQueryExecutionTime { get; }

        void ClearStatistics();

        void ChangeStateByKey(object entityKey, string operationName);
    }
}

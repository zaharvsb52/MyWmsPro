using System.Collections.Generic;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.DAL.Oracle;

namespace wmsMLC.Business.DAL.Oracle
{
    public abstract class BaseHistoryRepository<T, TKey> : Repository<T, TKey>, IHistoryRepository<T>
        where T : class, new()
    {
        private const string GetHistoryLstFunctionName = "{0}.Get{1}HLst";

        public virtual IEnumerable<HistoryWrapper<T>> GetHistory(string filter, string attrentity)
        {
            var packageName = GetPakageName(typeof (T));
            var sourceName = SourceNameHelper.Instance.GetSourceName(typeof (T));
            var functionName = string.Format(GetHistoryLstFunctionName, packageName, sourceName);

            var attrXml = !string.IsNullOrEmpty(attrentity)
                ? XmlDocumentConverter.XmlDocFromString(attrentity)
                : null;

            var resXml = XmlGetList(attrXml, filter, functionName);
            var result = XmlDocumentConverter.ConvertToListOf<HistoryWrapper<T>>(resXml);
            return result;
        }
    }

    public abstract class BaseHistoryCacheableRepository<T, TKey> : CacheableRepository<T, TKey>, IHistoryRepository<T>
        where T : class, new()
    {
        private const string GetHistoryLstFunctionName = "{0}.Get{1}HLst";

        public virtual IEnumerable<HistoryWrapper<T>> GetHistory(string filter, string attrentity)
        {
            var packageName = GetPakageName(typeof(T));
            var sourceName = SourceNameHelper.Instance.GetSourceName(typeof(T));
            var functionName = string.Format(GetHistoryLstFunctionName, packageName, sourceName);

            var attrXml = !string.IsNullOrEmpty(attrentity)
                ? XmlDocumentConverter.XmlDocFromString(attrentity)
                : null;

            var resXml = XmlGetList(attrXml, filter, functionName);
            return XmlDocumentConverter.ConvertToListOf<HistoryWrapper<T>>(resXml);
        }
    }
}
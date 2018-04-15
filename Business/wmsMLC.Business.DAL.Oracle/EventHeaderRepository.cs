using System;
using System.Xml;
using BLToolkit.DataAccess;
using wmsMLC.Business.Objects;
using wmsMLC.General;

namespace wmsMLC.Business.DAL.Oracle
{
    public abstract class EventHeaderRepository : BaseHistoryRepository<EventHeader, decimal>, IEventHeaderRepository
    {
        public void RegEvent(ref EventHeader entity, EventDetail eventDetail)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            var res = entity;
            RunManualDbOperation(db =>
            {
                var xmlDoc = XmlDocumentConverter.ConvertFrom(res);
                var xmlEventDetail = XmlDocumentConverter.ConvertFrom(eventDetail);
                decimal key;

                // сохраняем
                if (xmlEventDetail == null)
                    XmlEventHeaderInsert(xmlDoc, out key);
                else
                    XmlEventHeaderInsert(xmlDoc, out key, xmlEventDetail);

                // перевычитываем
                res = Get(key, null);
                if (res == null)
                    throw new DeveloperException("Не удалось получить только-что добавленный объект. Проверьте, что процедуры возвращают правильный ключ.");

                return 0;
            }, true);
            entity = res;
        }

        [SprocName("pkgEventHeader.insEventHeader")]
        [DiscoverParameters(false)]
        protected abstract void XmlEventHeaderInsert(XmlDocument entxml, out decimal key);

        [SprocName("pkgEventHeader.insEventHeader")]
        [DiscoverParameters(false)]
        protected abstract void XmlEventHeaderInsert(XmlDocument entxml, out decimal key, XmlDocument eventDetailParam);
    }
}
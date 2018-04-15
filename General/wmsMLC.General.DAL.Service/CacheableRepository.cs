using System.Collections.Generic;
using System.Xml;
using BLToolkit.Aspects;
using wmsMLC.General.DAL.Service.Telegrams;

namespace wmsMLC.General.DAL.Service
{
    /// <summary>
    /// Репозиторий, методы чтения данных которого кэшируются
    /// </summary>
    public abstract class CacheableRepository<T, TKey> : BaseRepository<T, TKey>, ICacheableRepository
        where T : class, new()
    {
        public const string ClearCacheActionName = "ClearCache";
        public const string IsNeedSendClearCacheTelegramFieldName = "IsNeedSendClearCacheTelegram";

        // ReSharper disable once StaticFieldInGenericType
        public static bool IsNeedSendClearCacheTelegram;

        [Cache]
        public override T Get(TKey key, string attrentity)
        {
            return base.Get(key, attrentity);
        }

        [Cache]
        public override XmlDocument GetXml(TKey key, string attrentity)
        {
            return base.GetXml(key, attrentity);
        }
        
        [Cache]
        public override List<T> GetAll(string attrentity = null)
        {
            return base.GetAll(attrentity);
        }

        public override List<T> GetFiltered(string filter, string attrentity)
        {
            // если фильтр не указан, то требуется возвратить все, а оно то у нас уже может быть закэшировано
            if (string.IsNullOrEmpty(filter))
                return GetAll(attrentity);

            return base.GetFiltered(filter, attrentity);
        }

        public virtual void ClearLocalCache()
        {
            CacheAspect.ClearCache(GetType());
        }

        public virtual void ClearCache()
        {
            ClearLocalCache();

            if (IsNeedSendClearCacheTelegram)
            {
                // отправляем телеграммку
                var telegram = new RepoQueryTelegramWrapper(typeof (T).Name, ClearCacheActionName, new TransmitterParam[0]);
                ProcessTelegramm(telegram);
            }
        }
    }
}
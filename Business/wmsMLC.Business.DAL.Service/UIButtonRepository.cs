using System;
using System.Linq;
using wmsMLC.Business.Objects;
using wmsMLC.General;

namespace wmsMLC.Business.DAL.Service
{
    public abstract class UIButtonRepository : BaseHistoryCacheableRepository<UIButton, string>
    {
        public override UIButton Get(string key, string attrentity)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            // получаем процессы из кэшированной коллекции
            return GetAll(attrentity).FirstOrDefault(i => key.EqIgnoreCase(i.Code));
        }

    }
}
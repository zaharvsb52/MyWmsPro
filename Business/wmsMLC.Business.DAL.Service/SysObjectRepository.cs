using BLToolkit.Aspects;
using wmsMLC.Business.Objects;

namespace wmsMLC.Business.DAL.Service
{
    public abstract class SysObjectRepository : BaseHistoryCacheableRepository<SysObject, decimal>
    {
        public override void ClearCache()
        {
            // очистим все
            CacheAspect.ClearCache();

            // да еще и на сервер пошлем
            base.ClearCache();
        }
    }
}
using wmsMLC.Business.Objects;

namespace wmsMLC.Business.DAL.Oracle
{
    public abstract class TruckTypeRepository : BaseHistoryCacheableRepository<TruckType, string>
    {
        protected override string GetPakageName(System.Type type)
        {
            return "pkgTruck";
        }
    }
}
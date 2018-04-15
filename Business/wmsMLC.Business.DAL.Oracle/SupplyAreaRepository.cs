using wmsMLC.Business.Objects;

namespace wmsMLC.Business.DAL.Oracle
{
    public abstract class SupplyAreaRepository : BaseHistoryCacheableRepository<SupplyArea, string>
    {
        protected override string GetPakageName(System.Type type)
        {
            return "pkgTypesArea";
        }
    }
}
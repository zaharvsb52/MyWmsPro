using wmsMLC.Business.Objects;

namespace wmsMLC.Business.DAL.Oracle
{
    public abstract class KitTypeRepository : BaseHistoryCacheableRepository<KitType, string>
    {
        protected override string GetPakageName(System.Type type)
        {
            return "pkgKit";
        }
    }
}
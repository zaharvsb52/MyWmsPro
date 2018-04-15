using wmsMLC.Business.Objects;

namespace wmsMLC.Business.DAL.Oracle
{
    public abstract class KitPosRepository : BaseHistoryCacheableRepository<KitPos, decimal>
    {
        protected override string GetPakageName(System.Type type)
        {
            return "pkgKit";
        }
    }
}
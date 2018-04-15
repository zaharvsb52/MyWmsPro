using wmsMLC.Business.Objects;

namespace wmsMLC.Business.DAL.Oracle
{
    public abstract class ReceiveAreaRepository : BaseHistoryCacheableRepository<ReceiveArea, string>
    {
        protected override string GetPakageName(System.Type type)
        {
            return "pkgTypesArea";
        }
    }
}
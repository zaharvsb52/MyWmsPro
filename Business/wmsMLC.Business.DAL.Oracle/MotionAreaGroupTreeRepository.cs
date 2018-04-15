using wmsMLC.Business.Objects;

namespace wmsMLC.Business.DAL.Oracle
{
    public abstract class MotionAreaGroupTreeRepository : BaseHistoryCacheableRepository<MotionAreaGroupTr, decimal>
    {
        protected override string GetPakageName(System.Type type)
        {
            return "pkgMotionArea";
        }
    }
}
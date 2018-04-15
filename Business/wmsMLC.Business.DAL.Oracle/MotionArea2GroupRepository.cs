using wmsMLC.Business.Objects;

namespace wmsMLC.Business.DAL.Oracle
{
    public abstract class MotionArea2GroupRepository : BaseHistoryCacheableRepository<MotionArea2Group, decimal>
    {
        protected override string GetPakageName(System.Type type)
        {
            return "pkgMotionArea";
        }
    }
}
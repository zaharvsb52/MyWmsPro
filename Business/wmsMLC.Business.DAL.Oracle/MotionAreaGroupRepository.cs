using wmsMLC.Business.Objects;

namespace wmsMLC.Business.DAL.Oracle
{
    public abstract class MotionAreaGroupRepository : BaseHistoryCacheableRepository<MotionAreaGroup, string>
    {
        protected override string GetPakageName(System.Type type)
        {
            return "pkgMotionArea";
        }

        protected override string GetSpName(System.Type type, string actionName)
        {
            var pkg = "pkgMotionArea.";
            return pkg + GetEssenceName(type, actionName) == "pkgMotionArea.GetMotionAreaGroupLst" ? "pkgMotionArea.getMotionAreaGroupTree" : base.GetSpName(type, actionName);
        }
    }
}
using wmsMLC.Business.Objects;

namespace wmsMLC.Business.DAL.Oracle
{
    public abstract class MeasureTypeRepository : BaseHistoryCacheableRepository<MeasureType, string>
    {
        protected override string GetPakageName(System.Type type)
        {
            return "pkgMeasure";
        }
    }
}
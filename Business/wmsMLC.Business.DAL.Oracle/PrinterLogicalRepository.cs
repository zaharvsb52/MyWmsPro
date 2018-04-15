using wmsMLC.Business.Objects;

namespace wmsMLC.Business.DAL.Oracle
{
    public abstract class PrinterLogicalRepository : BaseHistoryCacheableRepository<PrinterLogical, string>
    {
        protected override string GetPakageName(System.Type type)
        {
            return "pkgPrinter";
        }
    }
}
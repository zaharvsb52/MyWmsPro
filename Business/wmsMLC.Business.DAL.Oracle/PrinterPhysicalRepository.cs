using wmsMLC.Business.Objects;

namespace wmsMLC.Business.DAL.Oracle
{
    public abstract class PrinterPhysicalRepository : BaseHistoryCacheableRepository<PrinterPhysical, string>
    {
        protected override string GetPakageName(System.Type type)
        {
            return "pkgPrinter";
        }
    }
}
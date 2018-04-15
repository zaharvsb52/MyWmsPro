using wmsMLC.Business.Objects;
using wmsMLC.General.DAL;

namespace wmsMLC.Business.DAL
{
    public interface IReport2EntityRepository : IRepository<Report2Entity, decimal>
    {
        PrintStreamConfig GetDefaultPrinter(string tListParams);
        ReportRedefinition GetDefaultReport(string tListParams);
    }
}

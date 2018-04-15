using wmsMLC.Business.Objects;
using wmsMLC.General.DAL;

namespace wmsMLC.Business.DAL
{
    public interface IEpsOutputRepository : IRepository<EpsOutput, decimal>
    {
        //void Insert(string pEpsHandler, string pReportFile, string pPhysicalPrinter, string pCopies, string pReportParam);
        void Insert(string pReportFile, string pResultReportFile, string pFileFormat, string pReportParam1, string pReportParam2, string pReportValue1, string pReportValue2);
    }
}

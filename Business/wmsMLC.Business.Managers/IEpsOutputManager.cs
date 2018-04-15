using wmsMLC.Business.Objects;
using wmsMLC.General.BL;

namespace wmsMLC.Business.Managers
{
    public interface IEpsOutputManager : IBaseManager<EpsOutput, decimal>
    {
        //void Insert(string pEpsHandler, string pReportFile, string pPhysicalPrinter, string pCopies, string pReportParam);
        void Insert(string pReportFile, string pResultReportFile, string pFileFormat, string pReportParam1, string pReportParam2, string pReportValue1, string pReportValue2);
    }
}

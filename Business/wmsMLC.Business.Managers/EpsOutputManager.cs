using wmsMLC.Business.DAL;
using wmsMLC.Business.Objects;

namespace wmsMLC.Business.Managers
{
    public class EpsOutputManager : WMSBusinessObjectManager<EpsOutput, decimal>, IEpsOutputManager
    {
        //public void Insert(string pEpsHandler, string pReportFile, string pPhysicalPrinter, string pCopies, string pReportParam)
        //{
        //    var repo = GetRepository<IEpsOutputRepository>();
        //    if (repo == null)
        //        throw new DeveloperException(string.Format(DeveloperExceptionResources.IoCNotConfiguredFor, "IEpsOutputManager"));
        //    repo.Insert(pEpsHandler, pReportFile, pPhysicalPrinter, pCopies, pReportParam);
        //}

        public void Insert(string pReportFile, string pResultReportFile, string pFileFormat, string pReportParam1, string pReportParam2, string pReportValue1, string pReportValue2)
        {
            using (var repo = GetRepository<IEpsOutputRepository>())
                repo.Insert(pReportFile, pResultReportFile, pFileFormat, pReportParam1, pReportParam2, pReportValue1, pReportValue2);
        }
    }
}

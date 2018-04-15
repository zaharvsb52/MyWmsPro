using wmsMLC.Business.Objects;

namespace wmsMLC.Business.DAL.Oracle
{
    public abstract class ReportFileRepository : BaseHistoryRepository<ReportFile, decimal>, IReportFileRepository
    {
        private const string PkgName = "PKGREPORTFILE";

        public byte[] GetReportFileBody(string fileName)
        {
            const string pkg = PkgName + ".getReportFileBody";
            var result = RunManualDbOperation(db =>
            {
                var ps = db.GetSpParameters(pkg, false, false);
                ps[0].Value = fileName;

                var stm = string.Format("select {0}(:{1}) from dual", pkg, ps[0].ParameterName);
                return db.SetCommand(stm, ps).ExecuteScalar<byte[]>();
            });
            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using wmsMLC.General;
using wmsMLC.Business.Objects;

namespace wmsMLC.Business.DAL.Oracle
{
    public abstract class OutputRepository : BaseHistoryRepository<Output, decimal>, IOutputRepository
    {
        public IEnumerable<Output> GetEpsOutputLst(int recCount, int epsHandler)
        {
            return RunManualDbOperation(db =>
            {
                var pRecCount = db.InputParameter("pRecCount", recCount);
                var pEpsHandler = db.InputParameter("pEpsHandler", epsHandler);

                var stm = string.Format("select * FROM TABLE(PKGOUTPUT.getOutputLst2EPS(:{0}, :{1}))", pRecCount.ParameterName, pEpsHandler.ParameterName);
                var resXml = db.SetCommand(stm, pRecCount, pEpsHandler).ExecuteScalarList<XmlDocument>();
                return XmlDocumentConverter.ConvertToListOf<Output>(resXml);
            });
        }

        public virtual Stream GetReportPreview(Output task)
        {
            throw new NotSupportedException();
        }

        public virtual Output PrintReport(Output task)
        {
            throw new NotSupportedException();
        }

        public virtual OutputBatch PrintReportBatch(OutputBatch batch)
        {
            throw new NotSupportedException();
        }
    }
}

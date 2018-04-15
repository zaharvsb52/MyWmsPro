using System.Collections.Generic;
using System.IO;
using wmsMLC.Business.Objects;

namespace wmsMLC.Business.DAL.Service
{
    public abstract class OutputRepository : BaseHistoryRepository<Output, decimal>, IOutputRepository
    {
        //public IEnumerable<Output> GetEpsOutputLst(int pRecCount, int pEpsHandler)
        //{
        //    var resultParam = new TransmitterParam { Name = "result", Type = typeof(List<Output>), IsOut = true };
        //    var pRecCountParam = new TransmitterParam { Name = "pRecCount", Type = typeof(int), IsOut = false, Value = pRecCount }; 
        //    var pEpsHandlerParam = new TransmitterParam { Name = "pEpsHandler", Type = typeof(int), IsOut = false, Value = pEpsHandler };
        //    var telegram = new RepoQueryTelegramWrapper(typeof(Output).Name, "GetEpsOutputLst", new[] { resultParam, pRecCountParam, pEpsHandlerParam });
        //    ProcessTelegramm(telegram);
        //    return (List<Output>)resultParam.Value;
        //}
        public IEnumerable<Output> GetEpsOutputLst(int pRecCount, int pEpsHandler)
        {
            throw new System.NotImplementedException();
        }

        public virtual Stream GetReportPreview(Output task)
        {
            throw new System.NotImplementedException();
        }

        public virtual Output PrintReport(Output task)
        {
            throw new System.NotImplementedException();
        }

        public virtual OutputBatch PrintReportBatch(OutputBatch batch)
        {
            throw new System.NotImplementedException();
        }
    }
}

using System;
using System.Net;
using System.Net.Http;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.APS.wmsWebAPI.Controllers.V1
{
    public class EPSPrintReportController : BaseController
    {
        public HttpResponseMessage Post()
        {
            var output = GetFromRequestBody<Output>();
            output.OutputStatus = OutputStatus.OS_ON_TRANSFER.ToString();

            using (var mgr = IoC.Instance.Resolve<IBaseManager<Output>>())
            {
                if (output.GetKey() == null)
                {
                    mgr.Insert(ref output);
                }
                else
                {
                    mgr.Update(output);
                }

                if (output == null)
                    throw new Exception("Couldn not insert output.");

                try
                {
                    var epsjob = new EPS.wmsEPS.EpsJob(output, User.Identity.Name);
                    epsjob.DoJob();
                }
                finally
                {
                    var cs = output as ICustomXmlSerializable;
                    cs.OverrideIgnore = false;
                    mgr.Update(output);
                }

                return Request.CreateResponse(HttpStatusCode.OK, SingleEntityResult(output));
            }
        }
    }
}

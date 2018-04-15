using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;
using EpsJob = wmsMLC.EPS.wmsEPS.EpsJob;

namespace wmsMLC.APS.wmsWebAPI.Controllers.V1
{
    public class EPSPreviewReportController : BaseController
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
                    throw new Exception("Couldn not insert output");

                EpsJob epsjob;
                try
                {
                    epsjob = new EpsJob(output, User.Identity.Name);
                    epsjob.DoJob();
                }
                finally
                {
                    var cs = output as ICustomXmlSerializable;
                    cs.OverrideIgnore = false;

                    mgr.Update(output);
                }

                if (epsjob.Reports.Count > 0 && epsjob.Tasks.Any(t => t.ExportType != null))
                {
                    var report = epsjob.Reports.First().Value;
                    HttpResponseMessage result = Request.CreateResponse(HttpStatusCode.OK);
                    result.Content = new ByteArrayContent(report.GetExportStream(epsjob.Tasks.First(t => t.ExportType != null).ExportType));
                    result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    return result;
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Report stream not found");
                }
            }
        }
    }
}

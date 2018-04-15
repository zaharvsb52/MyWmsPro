using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.APS.wmsWebAPI.Controllers.V1
{
    public class EPSPrintReportBatchController : BaseController
    {
        public HttpResponseMessage Post()
        {
            var batch = GetFromRequestBody<OutputBatch>();
            var onTransfer = new List<Output>();
            var result = new List<Output>();
            using (var mgr = IoC.Instance.Resolve<IBaseManager<Output>>())
            {
                foreach (var output in batch.Batch)
                {
                    var o = output;
                    o.OutputStatus = OutputStatus.OS_ON_TRANSFER.ToString();

                    if (o.GetKey() == null)
                    {
                        mgr.Insert(ref o);
                    }
                    else
                    {
                        mgr.Update(o);
                    }

                    if (o == null)
                        throw new Exception("Couldn not insert output");

                    onTransfer.Add(o);
                }

                foreach (var output in onTransfer)
                {
                    var o = output;

                    try
                    {
                        var epsjob = new EPS.wmsEPS.EpsJob(o, User.Identity.Name);
                        epsjob.DoJob();
                    }
                    catch (Exception ex)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                            string.Format("Ошибочная задача: {0}, ошибка: {1}", o.GetKey(), ex.Message));
                    }
                    finally
                    {
                        var cs = o as ICustomXmlSerializable;
                        cs.OverrideIgnore = false;

                        mgr.Update(o);
                    }

                    result.Add(o);
                }

                return Request.CreateResponse(HttpStatusCode.OK, SingleEntityResult(new OutputBatch() { Batch = new WMSBusinessCollection<Output>(result) }));
            }
        }
    }
}

using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.General;
using wmsMLC.General.DAL;

namespace wmsMLC.APS.wmsWebAPI.Controllers.V1
{
    public class BpController : BaseController
    {
        [AllowAnonymous]
        //[ActionName("getsdclinfo")]
        public HttpResponseMessage GetSdclInfo([FromUri] string clientCode, [FromUri] string prevSdclCode)
        {
            var uowFactory = IoC.Instance.Resolve<IUnitOfWorkFactory>();
            using (var uow = uowFactory.Create(GetUnitOfWorkContext()))
            {
                try
                {
                    using (var mgr = IoC.Instance.Resolve<IBPProcessManager>())
                    {
                        mgr.SetUnitOfWork(uow);

                        var res = mgr.GetSdclConnectInfo(clientCode, prevSdclCode);
                        if (res == null)
                            return Request.CreateResponse(HttpStatusCode.NotFound);

                        return Request.CreateResponse(HttpStatusCode.OK, Serialize(res));
                    }
                }
                catch (Exception ex)
                {
                    Log.Debug(ex);
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
                }
            }
        }
    }
}
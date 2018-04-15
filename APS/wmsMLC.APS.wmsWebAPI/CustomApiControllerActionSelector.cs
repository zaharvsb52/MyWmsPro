using System.Linq;
using System.Web.Http.Controllers;

namespace wmsMLC.APS.wmsWebAPI
{
    public class CustomApiControllerActionSelector : ApiControllerActionSelector
    {
        public override HttpActionDescriptor SelectAction(HttpControllerContext controllerContext)
        {
            var res = base.SelectAction(controllerContext);
            return res;
        }

        public override ILookup<string, HttpActionDescriptor> GetActionMapping(HttpControllerDescriptor controllerDescriptor)
        {
            var res = base.GetActionMapping(controllerDescriptor);
            return res;
        }
    }
}
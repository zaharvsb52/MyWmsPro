using System.Web.Http.Filters;
using log4net;

namespace wmsMLC.APS.wmsWebAPI.Attributes
{
    public class LoggingExceptionFilterAttribute : ExceptionFilterAttribute 
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            LogManager.GetLogger(actionExecutedContext.ActionContext.ControllerContext.ControllerDescriptor.ControllerType).Error("Unhandled exception", actionExecutedContext.Exception);

            base.OnException(actionExecutedContext);
        }
    }
}
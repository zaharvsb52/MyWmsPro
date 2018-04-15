using MLC.Ext.Workflow.Views;

namespace wmsMLC.Business.Wf
{
    internal class ViewLocator : IViewLocator
    {
        public string FindViewNameByWebAppName(string webAppViewClassName)
        {
            if (!string.IsNullOrEmpty(webAppViewClassName))
            {
                //if (webAppViewClassName == "MLC.wms.wf.PassRequest_Create.PassRequestCard")
                //    return typeof(WpfClient.Demo.Wf.PassRequest_Create.PassRequestCardView).Name;
            }

            return "EntityCardView";
        }
    }
}
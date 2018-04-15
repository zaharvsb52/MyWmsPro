using System;
using MLC.Ext.Workflow.ViewModels;

namespace wmsMLC.Business.Wf
{
    internal class ViewModelLocator : IViewModelLocator
    {
        public Type FindViewModelTypeByControllerName(string controllerName)
        {
            //if (controllerName == "MLC.wms.wf.Passoffice_CheckPoint_Arrival.ConfirmPassRequestController")
            //    return typeof(WpfClient.Demo.Wf.Passoffice_CheckPoint_Arrival.ConfirmPassRequestViewModel);

            //if (controllerName == "MLC.wms.wf.PassRequest_Create.PassRequestCardController")
            //    return typeof(WpfClient.Demo.Wf.PassRequest_Create.PassRequestCardViewModel);

            // если хотим WF возвращаем карточку
            if (controllerName.StartsWith("MLC.wms.wf"))
                return typeof(WfCardViewModel);

            return null;
        }
    }
}
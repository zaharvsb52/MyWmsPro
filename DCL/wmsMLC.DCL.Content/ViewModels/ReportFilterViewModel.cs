using System;
using System.Windows;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof(ObjectView))]
    public class ReportFilterViewModel : ObjectViewModelBase<ReportFilter>
    {
        protected override void OnSave()
        {
            if (!CanSave() || Source == null)
                return;

            if (!CheckDefaultValue())
                return;

            base.OnSave();
        }

        protected override void OnSaveAndClose()
        {
            if (!CanSave() || Source == null)
                return;

            if (!CheckDefaultValue())
                return;

            base.OnSaveAndClose();
        }

        private bool CheckDefaultValue()
        {
            if (!string.IsNullOrEmpty(Source.REPORTFILTERDEFAULTVALUE) && Source.REPORTFILTERDATATYPE != null)
            {
                Type dataType;
                using (var sysObjectManager = IoC.Instance.Resolve<ISysObjectManager>())
                    dataType = sysObjectManager.GetTypeBySysObjectId((int)Source.REPORTFILTERDATATYPE);
                try
                {
                    var tmp = SerializationHelper.ConvertToTrueType(Source.REPORTFILTERDEFAULTVALUE, dataType);
                    Source.REPORTFILTERDEFAULTVALUE = tmp.To<String>();
                }
                catch
                {
                    GetViewService().ShowDialog(StringResources.Error, string.Format("Указанное значение '{0}' не соответствует типу '{1}'", Source.REPORTFILTERDEFAULTVALUE, dataType), MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes);
                    return false;
                }
            }
            return true;
        }
    }
}

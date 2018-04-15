using System.Collections.ObjectModel;
using System.Linq;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof(ObjectView))]
    public class MIViewModel : ObjectViewModelBase<MI>
    {
        protected override void SourceObjectPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.SourceObjectPropertyChanged(sender, e);

            if (Source == null)
                return;

            var editable = Source as IEditable;
            if (editable.IsInRejectChanges)
                return;

            if (!e.PropertyName.EqIgnoreCase(MI.MIINVTYPEPropertyName))
                return;

            RefreshView();
        }

        protected override ObservableCollection<DataField> GetFields(SettingDisplay displaySetting)
        {
            var fields = base.GetFields(displaySetting);

            var fieldMILine =
                fields.SingleOrDefault(p => Extensions.EqIgnoreCase(p.Name, MI.MILINEPropertyName));
            if (fieldMILine == null)
                return fields;

            if (Source == null || string.IsNullOrEmpty(Source.MIInvType))
                return fields;

            switch (Source.InvType)
            {
                case InvTypeEnum.BYPLACE:
                    try
                    {
                        Source.SuspendNotifications();
                        Source.MILine = 0;
                    }
                    finally
                    {
                        Source.ResumeNotifications();
                    }
                    fieldMILine.IsEnabled = false;

                    break;
                case InvTypeEnum.BYGROUP:
                    fieldMILine.IsEnabled = true;
                    break;
                default:
                    throw new DeveloperException("Неизвестная стратегия '{0}'.", Source.MIInvType);
            }
            
            return fields;
        }


    }
}
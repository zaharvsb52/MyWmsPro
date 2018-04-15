using wmsMLC.General.PL;
using wmsMLC.General.PL.Model;

namespace wmsMLC.RCL.General.ViewModels
{
    public interface IModelHandler
    {
        object GetSource();
        void SetSource(object source);

        //TODO: понять что это такое и как используется
        object ParentViewModelSource { get; set; }
        SettingDisplay DisplaySetting { get; set; }
        double FontSize { get; set; }
        string LayoutValue { get; set; }

        ValueDataField[] GetDialogFields();
    }
}
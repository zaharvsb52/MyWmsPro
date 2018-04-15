using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using wmsMLC.Business.Objects;
using wmsMLC.General.PL;
using wmsMLC.General.PL.Model;

namespace wmsMLC.DCL.General.ViewModels
{
    public interface IObjectViewModel : IModelHandler, IActionHandler
    {
        ObjectViewModelMode Mode { get; set; }
        ObservableCollection<DataField> GetDataFields(SettingDisplay displaySetting);

        // INFO: из интерфейса ISublistSaveAndContinueViewModel
        bool IsAdd { get; set; }
        WMSBusinessObject SourceBase { get; set; }
        bool IsVisibleMenuSaveAndContinue { get; set; }
        bool IsNeedRefresh { get; set; }
        event EventHandler<NotifyCollectionChangedEventArgs> CollectionChanged;
        event EventHandler NeedRefresh;

        void InitializeMenus();
    }

    public interface IActionHandler
    {
        bool DoAction();
        ICommand DoActionCommand { get; }
    }

    public interface IObjectViewModel<TModel> : IObjectViewModel
    {
    }

    public enum ObjectViewModelMode
    {
        Object, SubObject, MemoryObject
    }
}

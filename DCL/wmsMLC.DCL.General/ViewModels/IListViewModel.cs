using System;
using System.Collections.ObjectModel;
using wmsMLC.General.PL.Model;

namespace wmsMLC.DCL.General.ViewModels
{
    public interface IListViewModel : IModelHandler
    {
        void ApplyFilter();
        void ApplyFilter(string filterExpression);
        void ApplySqlFilter(string sqlExpression);

        bool IsHistoryEnable { get; }
        ObservableCollection<DataField> Fields { get; }

        bool IsSelectedFirstItem { get; set; }

        void InitializeMenus();
    }

    public interface IListViewModel<TModel> : IListViewModel
    {
        IFilterViewModel<TModel> Filters { get; }
        ObservableCollection<TModel> SelectedItems { get; }
        string PanelCaption { get; set; }
        bool IsCloseDoubleClick { get; set; }
    }

    public interface IObjectListViewModel : IListViewModel
    {
        ObjectListMode Mode { get; set; }

        bool IsFilterVisible { get; set; }
        bool AllowAddNew { get; set; }
        object SelectedItem { get; set; }
        object EditValue { get; set; }
        string ValueMember { get; set; }
        IFilterViewModel CustomFilters { get; }
        void ChangeImageFilter(bool isApplyFilter);

        event EventHandler ShouldChangeSelectedItem;
    }

    public interface ICustomListViewModel : IModelHandler
    {
        string PanelCaption { get; set; }
        void InitializeMenus();
    }

    public interface ICustomListViewModel<TModel> : ICustomListViewModel
    {
        //ObservableCollection<TModel> SelectedItems { get; }

        IObjectViewModel<TModel> ObjectViewModel { get; set; }
    }

    public enum ObjectListMode
    {
        /// <summary>
        /// Используется стандартная модель.
        /// </summary>
        ObjectList, 
        
        /// <summary>
        /// Используется при вызове из форм.
        /// </summary>
        LookUpList, 
        
        /// <summary>
        /// Используется как LookUpList, за исключением, использования SettingDisplay.
        /// </summary>
        LookUpList3Points
    }
}
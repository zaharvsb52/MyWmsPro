using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows.Input;
using wmsMLC.General.BL;
using wmsMLC.General.PL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.ViewModels;

namespace wmsMLC.DCL.General.ViewModels
{
    public interface IModelHandler : IViewModel
    {
        #region .  Methods&Events  .

        object GetSource();
        void SetSource(object source);
        void RefreshData();
        void RefreshDataAsync();
        void RefreshView();

        /// <summary>
        /// Событие, возникающее перед началом обновления данных.
        /// </summary>
        event EventHandler SourceUpdateStarted;

        /// <summary>
        /// Событие, возникающее после окончания обновления данных.
        /// </summary>
        event EventHandler SourceUpdateCompleted;

        /// <summary>
        /// Событие обновления вида.
        /// </summary>
        event EventHandler RefreshViewEvent;
        #endregion .  Methods&Events  .

        #region .  Fields&Properties  .

        bool IsReadEnable { get; }
        bool IsEditEnable { get; }
        bool IsNewEnable  { get; }
        bool IsDelEnable  { get; }
        //TODO: понять что это такое и как используется
        object ParentViewModelSource { get; set; }
        SettingDisplay DisplaySetting { get; set; }

        #endregion
    }

    /// <summary>
    /// Интерфейс для many2many деревьев
    /// </summary>
    public interface ITreeViewModelM2M
    {
        IEnumerable GetChild(object parent);
        IEnumerable ParentItems { get; }
    }

    public interface ICustomModelHandler : IViewModel
    {
        CustomExpandoObject Source { get; set; }

        ObservableCollection<ValueDataField> Fields { get; set; }

        string LayoutValue { get; set; }
        bool InsertFromAvailableItems { get; set; }

        //TODO: понять что это такое и как используется
        object ParentViewModelSource { get; set; }
        SettingDisplay DisplaySetting { get; set; }

        Action<string> MenuAction { get; set; }
        void CreateCustomMenu();

        ICommand MenuCommand { get; set; }
    }
}
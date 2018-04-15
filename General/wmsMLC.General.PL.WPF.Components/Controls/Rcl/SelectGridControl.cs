using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DevExpress.Xpf.Grid;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Helpers;

namespace wmsMLC.General.PL.WPF.Components.Controls.Rcl
{
    public class SelectGridControl : ContentControl, ICommandSource
    {
        public SelectGridControl()
        {
            DefaultStyleKey = typeof(SelectGridControl);
        }

        #region . Properties .

        public int AutoShowAutoFilterRowWhenRowsCountMoreThan
        {
            get { return (int)GetValue(AutoShowAutoFilterRowWhenRowsCountMoreThanProperty); }
            set { SetValue(AutoShowAutoFilterRowWhenRowsCountMoreThanProperty, value); }
        }
        public static readonly DependencyProperty AutoShowAutoFilterRowWhenRowsCountMoreThanProperty = DependencyProperty.Register("AutoShowAutoFilterRowWhenRowsCountMoreThan", typeof(int), typeof(SelectGridControl));

        public string TotalRowItemAdditionalInfo
        {
            get { return (string)GetValue(TotalRowItemAdditionalInfoProperty); }
            set { SetValue(TotalRowItemAdditionalInfoProperty, value); }
        }
        public static readonly DependencyProperty TotalRowItemAdditionalInfoProperty = DependencyProperty.Register("TotalRowItemAdditionalInfo", typeof(string), typeof(SelectGridControl));

        public ObservableCollection<DataField> Fields
        {
            get { return (ObservableCollection<DataField>)GetValue(FieldsProperty); }
            set { SetValue(FieldsProperty, value); }
        }
        public static readonly DependencyProperty FieldsProperty = DependencyProperty.Register("TotalRowItemAdditionalInfo", typeof(ObservableCollection<DataField>), typeof(SelectGridControl));

        public object Source
        {
            get { return GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(object), typeof(SelectGridControl));

        public bool WaitIndicatorVisible
        {
            get { return (bool)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }
        public static readonly DependencyProperty WaitIndicatorVisibleProperty = DependencyProperty.Register("WaitIndicatorVisible", typeof(bool), typeof(SelectGridControl));

        public string LookUpCodeEditor { get; set; }
        public string LookUpCodeEditorFilterExt { get; set; }

        #region . ICommandSource .
        public ICommand Command
        {
            get
            {
                return (ICommand)GetValue(CommandProperty);
            }
            set
            {
                SetValue(CommandProperty, value);
            }
        }
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(CustomSelectGridControl));

        public object CommandParameter
        {
            get
            {
                return GetValue(CommandParameterProperty);
            }
            set
            {
                SetValue(CommandParameterProperty, value);
            }
        }
        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register("CommandParameter", typeof(object), typeof(CustomSelectGridControl));

        public IInputElement CommandTarget
        {
            get
            {
                return (IInputElement)GetValue(CommandTargetProperty);
            }
            set
            {
                SetValue(CommandTargetProperty, value);
            }
        }
        public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register("CommandTarget", typeof(IInputElement), typeof(CustomSelectGridControl));
        #endregion . ICommandSource .
        #endregion . Properties .

        #region . Methods .

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Loaded -= OnLoaded;
            PrepareItemSource();
        }

        /// <summary>
        /// Получаем источник данных, если задан LookUpCodeEditor.
        /// </summary>
        private async void PrepareItemSource()
        {
            try
            {
                if (Source == null)
                {
                    WaitIndicatorVisible = true;
                    Source = await PrepareItemSourceAsync();
                }
            }
            finally
            {
                WaitIndicatorVisible = false;
            }
        }

        private async Task<object> PrepareItemSourceAsync()
        {
            return await Task.Factory.StartNew(() =>
            {
                //Thread.Sleep(10000);

                if (string.IsNullOrEmpty(LookUpCodeEditor))
                    return null;

                var lookUp = LookupHelper.GetLookup(LookUpCodeEditor);
                var filtertxt = lookUp.ObjectLookupFilter;
                string filter0;
                LookupHelper.InitializeVarFilter(filtertxt, out filter0);
                var filters = new List<string> { filter0, LookUpCodeEditorFilterExt };
                var filter = string.Join(" AND ", filters.Where(p => !string.IsNullOrEmpty(p)).Select(p => p));

                var itemType = LookupHelper.GetItemSourceType(lookUp);
                Fields = DataFieldHelper.Instance.GetDataFields(itemType, SettingDisplay.LookUp);

                //var valueMember = lookUp.ObjectLookupPkey;
                //var displayMember = lookUp.ObjectLookupDisplay;

                // using не делаем - можем прибить singleton manager
                var mgr = LookupHelper.GetItemSourceManager(lookUp);
                return mgr.GetFiltered(filter, GetModeEnum.Partial);
            });
        }

        private void OnItemsSourceChanged(object sender, ItemsSourceChangedEventArgs e)
        {

        }

        private static void ExecuteCommandSource(ICommandSource commandSource)
        {
            var command = commandSource.Command;
            if (command != null)
            {
                var commandParameter = commandSource.CommandParameter;
                if (command.CanExecute(commandParameter))
                    command.Execute(commandParameter);
            }
        }
        #endregion . Methods .
    }
}

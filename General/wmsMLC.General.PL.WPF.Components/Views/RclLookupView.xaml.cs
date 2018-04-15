using System;
using System.Windows;
using wmsMLC.General.PL.WPF.Components.ViewModels;

namespace wmsMLC.General.PL.WPF.Components.Views
{
    public partial class RclLookupView
    {
        private IRclListViewModel _model;

        public RclLookupView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
            Unloaded += OnUnloaded;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            _model = DataContext as IRclListViewModel;
            if (_model == null)
                return;

            //Из-за диспатчера DCL проблемы с биндингом
            PanelCaption = _model.PanelCaption;

            DataGrid.IsWfDesignMode = _model.IsWfDesignMode;
            DataGrid.Command = _model.ItemSelectCommand;

            //В IsWfDesignMode сохраняем настройкм грида
            if (_model.IsWfDesignMode)
            {
                _model.LayoutViewSaved -= OnLayoutViewSaved;
                _model.LayoutViewSaved += OnLayoutViewSaved;
            }
        }

        private void OnLayoutViewSaved(object sender, EventArgs eventArgs)
        {
            if (_model != null && _model.IsWfDesignMode)
                _model.LayoutValue = DataGrid.SaveLayoutToString();
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            DataContextChanged -= OnDataContextChanged;
            Unloaded -= OnUnloaded;
            if (_model != null)
                _model.LayoutViewSaved -= OnLayoutViewSaved;
        }
    }
}

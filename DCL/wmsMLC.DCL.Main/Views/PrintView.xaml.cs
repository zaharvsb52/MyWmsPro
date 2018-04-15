using System;
using System.ComponentModel;
using System.Windows;
using DevExpress.Xpf.Grid;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.Helpers;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Resources;
using wmsMLC.General.PL.WPF;

namespace wmsMLC.DCL.Main.Views
{
    /// <summary>
    /// Interaction logic for PrintView.xaml
    /// </summary>
    public partial class PrintView : BaseDialogWindow, IHelpHandler
    {
        public const string IconCellTemplateKey = "IconCellTemplate";

        private IPrintViewModel _printViewModel;

        public PrintView()
        {
            InitializeComponent();
            Closing += OnClosing;
            DataContextChanged += OnDataContextChanged;
            gridControl.Loaded += OnGridControlLoaded;
            gridControl.RestoredLayoutFromXml += OnGridRestoredLayoutFromXml;
            Height = SystemParameters.PrimaryScreenHeight*0.4d;
        }

        private void OnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            cancelEventArgs.Cancel = !_printViewModel.CancelPreview();
        }

        protected override void OnClose()
        {
            base.OnClose();
            _printViewModel = null;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            _printViewModel = (IPrintViewModel) DataContext;
        }

        private void OnGridControlLoaded(object sender, RoutedEventArgs e)
        {
            gridControl.Loaded -= OnGridControlLoaded;
            gridControl.CustomUnboundColumnData += GridControl_OnCustomUnboundColumnData;
            RestoreColumnsSettings();
        }

        private void OnGridRestoredLayoutFromXml(object sender, EventArgs eventArgs)
        {
            gridControl.RestoredLayoutFromXml -= OnGridRestoredLayoutFromXml;
            RestoreColumnsSettings();
        }

        private void RestoreColumnsSettings()
        {
            //Восстанавливаем Settings
            try
            {
                gridControl.BeginDataUpdate();
                var column = gridControl.Columns[0];
                column.CellTemplate = (DataTemplate) Resources[IconCellTemplateKey];
            }
            finally
            {
                gridControl.EndDataUpdate();
            }
        }

        private void GridControl_OnCustomUnboundColumnData(object sender, GridColumnDataEventArgs e)
        {
            if (e.IsGetData && e.Column.FieldName == StringResources.Package)
            {
                var obj = (Report) ((GridControl) sender).GetRowByListIndex(e.ListSourceRowIndex);
                if (obj.IsChildReports)
                {
                    if (e.Value == null)
                        e.Value = ImageResources.DCLMultiReport16.GetBitmapImage();
                }
                else
                    e.Value = null;
            }
        }

        private void GridView_OnRowDoubleClick(object sender, RowDoubleClickEventArgs e)
        {
            //Делаем так для того, что бы при двойном клике и после закрытия формы PrintPreview грид перестал выделять строки
            try
            {
                gridControl.IsEnabled = false;
            }
            finally
            {
                gridControl.IsEnabled = true;
            }
        }

        #region . IHelpHandler .
        string IHelpHandler.GetHelpLink()
        {
            return "wmsMLC";
        }

        string IHelpHandler.GetHelpEntity()
        {
            return "Print";
        }
        #endregion
    }
}

using System.Windows;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Printing;
using wmsMLC.DCL.General.Helpers;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.General.PL.WPF.ViewModels;

namespace wmsMLC.DCL.Main.Views
{
    public partial class PreviewGridControlView : BaseDialogWindow, IViewModel, IHelpHandler
    {
        public PreviewGridControlView()
        {
            InitializeComponent();

            Loaded += OnLoaded;
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            DataContextChanged -= OnDataContextChanged;

            var v = DataContext as IListViewModel;
            if (v != null)
                GridPreview.ColumnsSource = v.Fields;

            var vme = DataContext as IExportData;
            if (vme != null && vme.StreamExport != null)
            {
                vme.StreamExport.Position = 0;
                GridPreview.RestoreLayoutFromStream(vme.StreamExport);
            }

            var tv = GridPreview.View as TableView;
            if (tv == null)
                return;
            tv.AutoWidth = false;
            tv.PrintAutoWidth = false;
            tv.ShowGroupPanel = true;
            tv.ShowColumnHeaders = true;
            tv.ShowGroupedColumns = true;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var screenWidth = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width;
            var screenHeight = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height;
            if (this.ActualWidth > screenWidth/2)
            {
                this.SizeToContent = SizeToContent.Manual;
                this.Width = screenWidth/2;
            }
            if (this.ActualHeight > screenHeight/2)
            {
                this.SizeToContent = SizeToContent.Manual;
                this.Height = screenHeight/2;
            }
            double windowWidth = this.Width;
            double windowHeight = this.Height;
            this.Left = (screenWidth/2) - (windowWidth/2);
            this.Top = (screenHeight/2) - (windowHeight/2);

            var tv = GridPreview.View as TableView;
            if (tv != null)
            {
                var pp = new Point();
                pp.X = this.Left + (windowWidth/2);
                pp.Y = this.Top - (windowHeight/2);
                tv.ColumnChooserState = new DefaultColumnChooserState() {Location = pp};
                tv.ShowColumnChooser();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var link = new PrintableControlLink((TableView) GridPreview.View);
            link.ShowPrintPreviewDialog(this);

        }

        #region . IHelpHandler .
        string IHelpHandler.GetHelpLink()
        {
            return "wmsMLC";
        }

        string IHelpHandler.GetHelpEntity()
        {
            return "PreviewGrid";
        }
        #endregion
    }
}

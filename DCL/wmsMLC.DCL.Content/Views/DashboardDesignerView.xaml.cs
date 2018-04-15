using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Xml;
using DevExpress.DashboardWin;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.Content.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.General;
using wmsMLC.General.PL.WPF.Services;

namespace wmsMLC.DCL.Content.Views
{
    /// <summary>
    /// Interaction logic for DashboardDesignerView.xaml
    /// </summary>
    public partial class DashboardDesignerView : DXPanelView
    {
        public DashboardDesignerView()
        {
            InitializeComponent();
            DashboardDesignerPanel.Designer.DashboardSaving += DesignerOnDashboardSaving;
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (DashboardDesignerViewModel)e.NewValue;
            if (viewModel.Source == null)
                return;

            var xaml = viewModel.Source.GetProperty<string>(Dashboard.DashboardBodyPropertyName);
            if (string.IsNullOrEmpty(xaml))
                return;

            using (var s = new MemoryStream(Encoding.UTF8.GetBytes(xaml)))
            {
                try
                {
                    DashboardDesignerPanel.Designer.LoadDashboard(s);
                }
                catch (XmlException)
                {
                    var vs = IoC.Instance.Resolve<IViewService>();
                    vs.ShowDialog("Ошибка", "Некорректный формат Dashboard", MessageBoxButton.OK, MessageBoxImage.Error,
                        MessageBoxResult.OK);
                }
            }
        }

        private void DesignerOnDashboardSaving(object sender, DashboardSavingEventArgs e)
        {
            if (e.Command == DashboardSaveCommand.Save)
            {
                var viewModel = ((DashboardDesignerViewModel)DataContext);
                if (viewModel.SaveDashboardCommand != null && viewModel.SaveDashboardCommand.CanExecute(e.Dashboard))
                {
                    viewModel.SaveDashboardCommand.Execute(e.Dashboard);
                    e.Saved = true;
                }
                e.Handled = true;
            }
        }
    }
}

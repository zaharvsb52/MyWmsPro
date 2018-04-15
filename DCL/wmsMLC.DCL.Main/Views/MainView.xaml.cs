using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using DevExpress.Mvvm;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Docking;
using DevExpress.Xpf.Docking.Platform;
using FastReport.Utils;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Services;

namespace wmsMLC.DCL.Main.Views
{
    public partial class MainView : UserControl, System.Windows.Forms.IWin32Window
    {
        private log4net.ILog _log = log4net.LogManager.GetLogger(typeof(MainView));
        private string _oldTheme;
        private bool _versiovVerified;

        public MainView()
        {
            InitializeComponent();
            CreateThemeMenu();
            ThemeManager.ApplicationThemeChanged += ThemeManagerOnThemeChanged;

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Loaded -= OnLoaded;
            var env = WMSEnvironment.Instance.DbSystemInfo.Environment;
            if (!string.IsNullOrEmpty(env))
                ErrorBox.AddAdditionalParam(BugReportResources.Environment, env);

            var ver = WMSEnvironment.Instance.DbSystemInfo.Version;
            if (!string.IsNullOrEmpty(ver))
                ErrorBox.AddAdditionalParam(BugReportResources.VersionDB, ver);

            var sdcl = WMSEnvironment.Instance.SdclCode;
            if (!string.IsNullOrEmpty(sdcl))
                ErrorBox.AddAdditionalParam(BugReportResources.SdclCode, sdcl);

            var endpoint = WMSEnvironment.Instance.EndPoint;
            if (!string.IsNullOrEmpty(endpoint))
                ErrorBox.AddAdditionalParam(BugReportResources.SDCL_Endpoint, endpoint);

            // регистрируем сервис отображения вкладок
            IoC.Instance.RegisterInstance(typeof(IDocumentManagerService), DocServiceHost, LifeTime.Singleton);

            // регистрируем сервис отображения диалогов
            IoC.Instance.RegisterInstance(typeof(IDocumentManagerService), DocServiceFloat, LifeTime.Singleton, "Float");
        }

        private void CreateThemeMenu()
        {
            foreach (var item in GetThemes().Select(t => new BarButtonItem { Name = t.Name.Replace(" ", "_").Replace(";", "_"), Content = t.FullName }))
            {
                BarManager.Items.Add(item);
                var link = new BarButtonItemLink { BarItemName = item.Name };
                miThemes.ItemLinks.Add(link);
            }
        }

        private Theme[] GetThemes()
        {
            return Theme.Themes.ToArray();
        }

        private void ThemeManagerOnThemeChanged(DependencyObject sender, ThemeChangedRoutedEventArgs themeChangedRoutedEventArgs)
        {
            if (themeChangedRoutedEventArgs.ThemeName == _oldTheme)
                return;

            var i = GetThemes().FirstOrDefault(t => t.Name == themeChangedRoutedEventArgs.ThemeName);
            if (i == null)
                return;

            foreach (var barItemLinkBase in miThemes.ItemLinks)
            {
                var link = (BarButtonItemLink) barItemLinkBase;
                var item = BarManager.Items.FirstOrDefault(t => t.Name == link.BarItemName);
                if (item == null)
                    continue;
                item.Glyph = Equals(item.Content, i.FullName) ? ImageResources.DCLMultiReport16.GetBitmapImage() : null;
            }
            _oldTheme = themeChangedRoutedEventArgs.ThemeName;
            Properties.Settings.Default.Theme = _oldTheme;
            Properties.Settings.Default.Save();
        }

        public void ShowReport(string fileName)
        {
            var report = new FastReport.Report();
            report.LoadPrepared(fileName);

            var lfs = LocalizationResources.RussianFastReport;
            if (lfs != null && lfs.Length > 0)
                Res.LoadLocale(new MemoryStream(lfs));

            report.ShowPrepared(false, this);

            if (File.Exists(fileName))
                File.Delete(fileName);
        }

        public void ShowReport(Stream stream)
        {
            // TODO: Вынести в инициализацию EPS клиента
            Config.PreviewSettings.ShowInTaskbar = true;

            var report = new FastReport.Report();
            report.LoadPrepared(stream);

            var lfs = LocalizationResources.RussianFastReport;
            if (lfs != null && lfs.Length > 0)
                Res.LoadLocale(new MemoryStream(lfs));

            report.ShowPrepared(false, this);
        }

        public void MakeMaxSize(BaseLayoutItem item, bool isMax)
        {
            if (item == null)
                return;

            var panel = item as LayoutPanel;
            if (panel != null)
            {
                panel.ShowRestoreButton = isMax;
                panel.ShowMaximizeButton = isMax;
            }

            if (isMax)
            {
                dockManager.DockController.Dock(item, documentGroup, DevExpress.Xpf.Layout.Core.DockType.Fill);
            }
            else
            {
                var parent = dockManager.DockController.Float(item);
                var restoreBounds = new Rect(parent.FloatLocation, parent.FloatSize);
                var bounds = WindowHelper.GetMaximizeBounds(dockManager, restoreBounds);
                DocumentPanel.SetRestoreBounds(parent, restoreBounds);
                parent.FloatLocation = new Point(bounds.X, bounds.Y);
                parent.FloatSize = new Size(bounds.Width, bounds.Height);
            }
        }

        public IntPtr Handle
        {
            get
            {
                var interopHelper = new WindowInteropHelper(Application.Current.MainWindow);
                return interopHelper.Handle;
            }
        }

        private void BarManager_OnItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.Item.Content == null)
                return;

            var item = GetThemes().FirstOrDefault(t => Equals(t.FullName, e.Item.Content));
            if (item == null)
                return;

            if (!VerifyDotNetVersion())
                return;

            waitIndicator.DeferedVisibility = true;
            ThemeManager.ApplicationThemeName = item.Name;
            waitIndicator.DeferedVisibility = false;
        }

        private bool VerifyDotNetVersion()
        {
            if (_versiovVerified)
                return true;

            //Проверяем ОС.
            var osInfo = Environment.OSVersion;
            string errmessage = null;
            if (osInfo.Version.Major <= 5) //Если ОС не поддерживает .net 4.5 - выходим
            {
                errmessage = string.Format(StringResources.ChangeThemeOsNotSupport, osInfo.VersionString);
            }
            //Если ОС поддерживает .net 4.5, но .net 4.5 не установлен - выходим
            else if (Environment.Version < new Version(4, 0, 30319, 17626)) //(4.0.30319.17626 = .NET 4.5 RC) http://stackoverflow.com/questions/12971881/how-to-reliably-detect-the-actual-net-4-5-version-installed
            {
                errmessage = StringResources.ChangeThemeNotSupport;
            }

            if (!string.IsNullOrEmpty(errmessage))
            {
                _log.Warn(errmessage);
                var vs = IoC.Instance.Resolve<IViewService>();
                vs.ShowDialog
                    (StringResources.Warning
                        , errmessage
                        , MessageBoxButton.OK
                        , MessageBoxImage.Warning
                        , MessageBoxResult.Yes);

                return false;
            }

            _versiovVerified = true;
            return true;
        }
    }
}

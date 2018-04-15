using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing.Printing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels.Menu;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.DCL.General.Helpers;
using wmsMLC.General.BL.Helpers;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Helpers;
using wmsMLC.General.PL.WPF.ViewModels;

namespace wmsMLC.DCL.General.ViewModels
{
    public class PrintViewModel : SourceViewModelBase<Report>, IPrintViewModel
    {
        #region .  Fields && Consts  .

        private const string FilterCpv = "upper(CUSTOMPARAMVAL.CUSTOMPARAMCODE_R) = upper('REPView2MainMenu') and CUSTOMPARAMVAL.CPVVALUE = '1' ";
        private readonly Type _type;
        private readonly string _path = Properties.Settings.Default.ReportPath;

        private ObservableCollection<Report> _selectedReports;
        private CancellationTokenSource _printPreviewCancelToken;
        private string _reportFileName;
        private CommandMenuItem _miPreview;
        private ListMenuItem _miPrintExt;
        private bool _isInPreviewGenerating;
        private string _totalRowItemAdditionalInfo;
        private Report[] _reports;

        private static string _attrEntityCache;
        private static double? _lastQueryExecutionTime;

        private decimal[] _mandantList;

        private Dictionary<string, IEnumerable<KeyValuePair<string, string[]>>> _printerCache;
        private string[] _installedPrinters;

        private log4net.ILog _log = log4net.LogManager.GetLogger(typeof(PrintViewModel));

        # endregion .  Fields && Consts  .

        public PrintViewModel(object[] itemsToPrint, Type type)
        {
            _printerCache = new Dictionary<string, IEnumerable<KeyValuePair<string, string[]>>>();
            PanelCaption = StringResources.Printable;
            SelectedReports = new ObservableCollection<Report>();
            PrintReportCommand = new DelegateCustomCommand(PrintReport, CanPrintReport);
            PrintReportExtCommand = new DelegateCustomCommand<string>(PrintReportExt);
            ViewReportCommand = new DelegateCustomCommand(ViewReport, CanViewReport);
            RefreshReportsCommand = new DelegateCustomCommand(RefreshReports);

            ObjectItems = itemsToPrint;
            // получим список мандантов
            _mandantList = ObjectItems.Cast<WMSBusinessObject>().Select(i => BPH.GetMandantId(i) ?? 0).Distinct().ToArray();

            CreateMenu();

            _type = type;
            FillReportsAsync(type);
        }

        public PrintViewModel(object[] itemsToPrint)
            : this(itemsToPrint, itemsToPrint == null || itemsToPrint.Length == 0 ? null : itemsToPrint[0].GetType())
        {
        }

        #region .  Properties  .

        public Object[] ObjectItems { get; private set; }

        public Report[] Reports
        {
            get { return _reports; }
            private set
            {
                _reports = value;
                OnPropertyChanged("Reports");
            }
        }

        public ICustomCommand PrintReportCommand { get; private set; }

        public ICommand PrintReportExtCommand { get; private set; }

        public ICustomCommand ViewReportCommand { get; private set; }

        public ICustomCommand RefreshReportsCommand { get; private set; }

        public string ReportFileName
        {
            get { return _reportFileName; }
            set
            {
                if (_reportFileName == value)
                    return;

                _reportFileName = value;
                OnPropertyChanged("ReportFileName");
            }
        }

        public ObservableCollection<Report> SelectedReports
        {
            get { return _selectedReports; }
            set
            {
                if (_selectedReports == value)
                    return;

                if (_selectedReports != null)
                    _selectedReports.CollectionChanged -= SelectedItemsCollectionChanged;

                _selectedReports = value;

                if (_selectedReports != null)
                    _selectedReports.CollectionChanged += SelectedItemsCollectionChanged;


                OnPropertyChanged("SelectedReports");
            }
        }

        public bool IsInPreviewGenerating
        {
            get { return _isInPreviewGenerating; }
            private set
            {
                if (_isInPreviewGenerating == value)
                    return;

                _isInPreviewGenerating = value;
                OnPropertyChanged("IsInPreviewGenerating");
                OnIsInPreviewGeneratingChanged();
            }
        }

        public string TotalRowItemAdditionalInfo
        {
            get { return _totalRowItemAdditionalInfo; }
            set
            {
                if (_totalRowItemAdditionalInfo == value)
                    return;
                _totalRowItemAdditionalInfo = value;
                OnPropertyChanged("TotalRowItemAdditionalInfo");
            }
        }

        public ICommand AppearanceStyleCommand { get; private set; }

        # endregion .  Properties  .

        #region .  Methods  .

        private void CreateMenu()
        {
            InitializeCustomizationBar();

            var barTech = Menu.GetOrCreateBarItem(StringResources.Printable, 10);
            barTech.MenuItems.Add(new CommandMenuItem
            {
                IsEnable = true,
                Caption = StringResources.ActionPrint,
                Command = PrintReportCommand,
                ImageSmall = ImageResources.DCLPrintAndPreview16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLPrintAndPreview32.GetBitmapImage(),
                //GlyphSize = GlyphSizeType.Large,
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 1,
                HotKey = new KeyGesture(Key.P, ModifierKeys.Control)
            });

            _miPrintExt = new ListMenuItem
            {
                Caption = string.Empty,
                IsEnable = true,
                IsDynamicBarItem = true,
                IsEnableItems = true,
                DisplayMode = DisplayModeType.Content,
                Priority = 2
            };

            barTech.MenuItems.Add(_miPrintExt);

            _miPreview = new CommandMenuItem
            {
                IsEnable = true,
                Caption = StringResources.Preview,
                Command = ViewReportCommand,
                ImageSmall = ImageResources.DCLPrintPreview16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLPrintPreview32.GetBitmapImage(),
                //GlyphSize = GlyphSizeType.Large,
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 3,
            };
            barTech.MenuItems.Add(_miPreview);

            barTech.MenuItems.Add(new CommandMenuItem
            {
                IsEnable = true,
                Caption = StringResources.RefreshData,
                Command = RefreshReportsCommand,
                ImageSmall = ImageResources.DCLFilterRefresh16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLFilterRefresh32.GetBitmapImage(),
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 4,
                HotKey = new KeyGesture(Key.F5),
            });

        }

        private IEnumerable<KeyValuePair<string, string[]>> GetPrinters()
        {
            Report[] selectedReports;

            try
            {
                if (SelectedReports == null)
                    return null;
                selectedReports = SelectedReports.ToArray();
            }
            catch (ArgumentException)
            {
                //перехватываем странный баг, возникающий в SelectedReports
                return null;
            }

            var reports = selectedReports.Select(i => i.GetKey<string>()).ToArray();
            if (reports.Length == 0)
                return null;
            var key = string.Join("_", reports);
            if (!_printerCache.ContainsKey(key))
                _printerCache[key] = BPH.GetPrinters(reports, GetInstalledPrinters(), _mandantList);

            return _printerCache[key];
        }

        private IEnumerable<string> GetInstalledPrinters()
        {
            if (_installedPrinters == null)
                _installedPrinters = PrinterSettings.InstalledPrinters.Cast<string>().ToArray();
        
            return _installedPrinters;
        }

        private IEnumerable<MenuItemBase> GetPrinterMenuItems()
        {
            var result = new MenuItemCollection();
            var printers = GetPrinters();
            if (printers == null)
                return result;

            var priority = 1000;
            foreach (var p in printers)
            {
                result.Add(new CommandMenuItem()
                {
                    Caption = string.Format("{0}{1}", p.Key, string.IsNullOrEmpty(p.Value[1]) ? string.Empty : " - " + p.Value[1]),
                    Hint = p.Value[0],
                    Command = PrintReportExtCommand,
                    CommandParameter = p.Key,
                    ImageSmall = ImageResources.DCLPrintAndPreview16.GetBitmapImage(),
                    ImageLarge = ImageResources.DCLPrintAndPreview32.GetBitmapImage(),
                    GlyphAlignment = GlyphAlignmentType.Top,
                    DisplayMode = DisplayModeType.Default,
                    Priority = priority++
                });
            }
            return result;
        }

        private void OnIsInPreviewGeneratingChanged()
        {
            RefreshCommands();
            _miPreview.Caption = IsInPreviewGenerating ? StringResources.Cancel : StringResources.Preview;
        }

        private void SelectedItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshCommands();
        }

        private async void RefreshCommands()
        {
            ViewReportCommand.RaiseCanExecuteChanged();
            PrintReportCommand.RaiseCanExecuteChanged();

            if (SelectedReports.Any())
                await RefreshPrinters();
        }

        private async Task RefreshPrinters()
        {
            await Task.Factory.StartNew(() =>
            {
                var menuItems = GetPrinterMenuItems();
                DispatcherHelper.BeginInvoke(new Action(() =>
                {
                    _miPrintExt.MenuItems.Clear();
                    foreach (var item in menuItems)
                        _miPrintExt.MenuItems.Add(item);
                }));
            });
        }

        private async void RefreshReports()
        {
            _installedPrinters = null;
            using (var mgr = IoC.Instance.Resolve<IBaseManager<Report>>())
            {
                mgr.ClearCache();
            }

            await Task.Factory.StartNew(() =>
            {
                FillReportsAsync(_type);
            });
        }

        public bool CancelPreview(bool force = false)
        {
            if (!IsInPreviewGenerating || _printPreviewCancelToken.IsCancellationRequested)
                return true;

            if (!force)
            {
                var res = GetViewService().ShowDialog(StringResources.Confirmation,
                    StringResources.CancelPreview,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question,
                    MessageBoxResult.Yes);
                if (res == MessageBoxResult.No)
                    return false;
            }

            if (_printPreviewCancelToken != null)
                _printPreviewCancelToken.Cancel();
            return true;
        }

        private bool CanViewReport()
        {
            return SelectedReports.Count == 1;
        }

        private bool CanAppearanceStyle()
        {
            return IsCustomizeEnabled;
        }

        private void OnAppearanceStyle()
        {
            if (!CanAppearanceStyle())
                return;
            IsCustomization = false;
            IsCustomization = true;
        }

        protected override void SetupCustomizeMenu(BarItem bar, ListMenuItem listmenu)
        {
            base.SetupCustomizeMenu(bar, listmenu);

            if (AppearanceStyleCommand == null)
                AppearanceStyleCommand = new DelegateCustomCommand(OnAppearanceStyle, CanAppearanceStyle);

            // Добавляем функционал подсветки (в начало списка)
            var minPriority = listmenu.MenuItems.Min(i => i.Priority);
            listmenu.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.AppearanceStyle,
                Command = AppearanceStyleCommand,
                ImageSmall = ImageResources.DCLAppearanceStyle16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLAppearanceStyle32.GetBitmapImage(),
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = minPriority - 100
            });

            // убираем функционал настройки самого layout-а
            var item = listmenu.MenuItems.FirstOrDefault(i => i.Caption == StringResources.CustomizeRegion);
            if (item != null)
                listmenu.MenuItems.Remove(item);
        }

        private async void ViewReport()
        {
            if (!ConnectionManager.Instance.AllowRequest())
                return;

            if (!CanViewReport())
                return;

            if (SelectedReports.Count == 0)
                return;

            var viewService = GetViewService();
            if (SelectedReports.Count > 1)
            {
                viewService.ShowDialog(StringResources.Warning, StringResources.MultiplePreviewReportsError,
                    MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                return;
            }

            var date0 = DateTime.Now;
            var reports = new List<Report>();

            try
            {
                WaitStart();

                IsInPreviewGenerating = !IsInPreviewGenerating;
                if (_printPreviewCancelToken != null)
                {
                    _printPreviewCancelToken.Cancel();
                    _printPreviewCancelToken.Dispose();
                    _printPreviewCancelToken = null;
                    return;
                }

                _printPreviewCancelToken = new CancellationTokenSource();

                // добавляем к списку на печать основной отчет
                var mainReport = SelectedReports[0];
                reports.Add(mainReport);

                // связки к вложенным отчетам уже д.б. получены. Если они есть - вытаскиваем нужные нам отчеты
                if (mainReport.ChildReports != null && mainReport.ChildReports.Count > 0)
                {
                    var childReports = GetChildReports(mainReport.ChildReports);
                    if (childReports.Length > 0)
                        reports.AddRange(childReports);
                }

                var export = new List<ExportParam>();
                // Сначала заполняем параметры для всех отчетов
                foreach (var report in reports)
                {
                    var fileName = string.Format("Preview_{0}_{1}.fpx", report.ReportFile_R, Guid.NewGuid());

                    if (ObjectItems.Length == 0)
                    {
                        var outputParams = GetFilterOutputParam(report, null);
                        export.Add(new ExportParam(report, fileName, null, outputParams));
                    }
                    else
                    {
                        foreach (var item in ObjectItems)
                        {
                            var entity = item as WMSBusinessObject;
                            if (entity == null)
                                throw new DeveloperException("Item is not WMSBusinessObject.");

                            var outputParams = GetFilterOutputParam(report, entity);
                            export.Add(new ExportParam(report, fileName, entity, outputParams));
                        }
                    }
                }

                await PreviewReportsAsync(export, _printPreviewCancelToken);
            }
            catch (UserAbortException)
            {
                // Ничего не делаем. Пользователь и так знает, что он отменил
            }
            catch (Exception ex)
            {
                ExceptionPolicy.Instance.HandleException(ex, "PL");
            }
            finally
            {
                WaitStop();
                IsInPreviewGenerating = false;
                _printPreviewCancelToken = null;
                _log.DebugFormat("Elapsed time preview {0}: {1}",
                    string.Join(", ", reports.Select(p => string.Format("'{0}'", p.ReportName))), DateTime.Now - date0);
            }
        }

        private async Task PreviewReportsAsync(IList<ExportParam> export, CancellationTokenSource cancellationToken)
        {
            await Task.Factory.StartNew(() =>
            {
                var list = new Task[export.Count];
                for (var i = 0; i < export.Count; i++)
                {
                    var exp = export[i];
                    var getAndDisplayReport = Task.Factory.StartNew(() =>
                    {
                        if (cancellationToken.IsCancellationRequested)
                            return;

                        using (var mgr = IoC.Instance.Resolve<Report2EntityManager>())
                        using (var res = mgr.ExpReport(exp.Report.GetKey<string>(), exp.FileName, _path, exp.Entity, exp.ParamExt))
                        {
                            if (res == null)
                                throw new DeveloperException("PrintReportStatus is null.");

                            // если не запросили отмены показа - показываем
                            if (cancellationToken.IsCancellationRequested)
                                return;

                            DispatcherHelper.Invoke(new Action(() => ((IShowReport)GetViewService()).ShowReport(res)));
                        }
                    });

                    list[i] = getAndDisplayReport;
                }

                Task.WaitAll(list);

            }, _printPreviewCancelToken.Token);
        }

        private bool CanPrintReport()
        {
            return SelectedReports.Count > 0 && !IsInPreviewGenerating;
        }

        private async void PrintReport()
        {
            await PrintReportExtAsync(null);
        }

        private async void PrintReportExt(string printerCode)
        {
            await PrintReportExtAsync(printerCode);
        }

        private async Task PrintReportExtAsync(string printerCode)
        {
            if (!ConnectionManager.Instance.AllowRequest())
                return;

            if (SelectedReports.Count == 0)
                return;

            var date0 = DateTime.Now;

            try
            {
                WaitStart();

                var resultlist = await PrintReportAsync(printerCode);

                if (resultlist.Count > 0)
                {
                    // оставляем только пару первых и последних сообщений для краткости
                    if (resultlist.Count > 5)
                    {
                        resultlist = resultlist.Take(2).Union(new[] { "..." }).Union(resultlist.Skip(resultlist.Count - 2)).ToList();
                    }

                    GetViewService().ShowDialog(StringResources.ActionPrint
                        , string.Join(Environment.NewLine, resultlist.ToArray())
                        , MessageBoxButton.OK
                        , MessageBoxImage.Information
                        , MessageBoxResult.Yes);
                }
            }
            catch (UserAbortException)
            {
            }
            catch (Exception ex)
            {
                if (!ExceptionHandler(ex, ExceptionResources.EpsCreateTaskError))
                    throw;
            }
            finally
            {
                WaitStop();
                _log.DebugFormat("Elapsed time printing reports: {0}", DateTime.Now - date0);
            }
        }

        private Report[] GetChildReports(ICollection<Report2Report> childReports)
        {
            if (childReports == null || childReports.Count == 0)
                return new Report[0];

            var filter = FilterHelper.GetFilterIn("REPORT", childReports.Select(i => i.ReportCode).Distinct());
            Report[] chldRpt;
            using (var mgr = IoC.Instance.Resolve<IBaseManager<Report>>())
            {
                chldRpt = mgr.GetFiltered(filter, GetReportAttrEntity()).ToArray();
            }

            if (chldRpt.Length > 0)
            {
                var childReportsInternal = chldRpt.ToDictionary(r => r.GetKey<string>(), r => r);
                return childReports.OrderBy(r => r.Priority).Select(r => childReportsInternal[r.ReportCode]).ToArray();
            }

            return new Report[0];
        }

        private async Task<List<string>> PrintReportAsync(string printerCode)
        {
            return await Task.Factory.StartNew(() =>
            {
                var resultlist = new List<string>();

                var reports = new List<Report>();

                foreach (var report in SelectedReports)
                {
                    // добавляем сам отчет
                    reports.Add(report);

                    // если у него есть вложенные - вытаскиваем их
                    if (report.ChildReports == null || report.ChildReports.Count == 0)
                        continue;

                    var childReports = GetChildReports(report.ChildReports);
                    if (childReports.Length > 0)
                        reports.AddRange(childReports);
                }

                if (ObjectItems.Length != 0)
                    foreach (var rep in reports)
                    {
                        if (rep.IsBatchPrint && ObjectItems.Length > 1)
                        {
                            PrintReportBatch(ObjectItems.OfType<WMSBusinessObject>().ToArray(), resultlist, rep, printerCode);
                        }
                        else
                        {
                            foreach (var item in ObjectItems)
                            {
                                var bo = item as WMSBusinessObject;
                                if (bo == null)
                                    throw new DeveloperException("Item is not WMSBusinessObject.");

                                PrintReport(bo, item, resultlist, rep, printerCode);
                            }
                        }
                    }
                else
                {
                    foreach (var rep in reports)
                    {
                        PrintReport(null, null, resultlist, rep, printerCode);
                    }
                }
                return resultlist;
            });
        }

        private async void FillReportsAsync(Type entityType)
        {
            WaitStart();
            var now = DateTime.Now;
            TotalRowItemAdditionalInfo = null;
            _lastQueryExecutionTime = null;

            try
            {
                Reports = await Task.Factory.StartNew(() => GetReportsAsync(entityType));
                if (_lastQueryExecutionTime.HasValue)
                    TotalRowItemAdditionalInfo =
                        string.Format(StringResources.ListViewModelBaseTotalRowItemAdditionalInfo,
                            (DateTime.Now - now).TotalSeconds, _lastQueryExecutionTime);
            }
            finally
            {
                WaitStop();
            }
        }

        private static string GetReportAttrEntity()
        {
            if (!string.IsNullOrEmpty(_attrEntityCache))
                return _attrEntityCache;

            // формируем AttrEntity
            var reportAttrEntity = FilterHelper.GetAttrEntity<Report>();
            var rfAttrEntity = FilterHelper.GetAttrEntity<ReportFilter>();
            var r2FAttrEntity = FilterHelper.GetAttrEntity<Report2Report>();

            reportAttrEntity = reportAttrEntity.Replace("<REPORTFILTERL />",
                string.Format("<REPORTFILTERL>{0}</REPORTFILTERL>", rfAttrEntity));
            reportAttrEntity = reportAttrEntity.Replace("<CHILD2REPORTL />",
                string.Format("<CHILD2REPORTL>{0}</CHILD2REPORTL>", r2FAttrEntity));

            _attrEntityCache = reportAttrEntity;
            return _attrEntityCache;
        }

        private static Report[] GetReportsAsync(Type entityType)
        {
            // получаем и заполняем список отчетов
            using (var mgr = IoC.Instance.Resolve<IBaseManager<Report>>())
            {
                string filter;
                // получаем отчеты по связке к сущности (права на отчет проверятся при получении "report in")
                if (entityType != null)
                {
                    var entityName = SourceNameHelper.Instance.GetSourceName(entityType).ToUpper();
                    filter =
                        string.Format(
                            "REPORT IN (SELECT distinct REPORT_R FROM WMSREPORT2ENTITY WHERE WMSREPORT2ENTITY.OBJECTNAME_R = '{0}' and {1})",
                            entityName, FilterCpv);
                }
                else
                {
                    filter =
                        string.Format("REPORT not in (select distinct WMSREPORT2ENTITY.REPORT_R from WMSREPORT2ENTITY) and {0}", FilterCpv);
                }

                var result = mgr.GetFiltered(filter, GetReportAttrEntity()).ToArray();
                _lastQueryExecutionTime = mgr.LastQueryExecutionTime;
                return result;
            }
        }

        private static void PrintReportBatch(WMSBusinessObject[] entities, List<string> resultlist, Report report, string printerCode = null)
        {
            var outputParams = entities.ToDictionary(e => e, e => GetFilterOutputParam(report, e));

            PrintReportStatus result;
            using (var mgr = IoC.Instance.Resolve<IReport2EntityManager>())
                result = mgr.PrintReportBatch(entities, report.GetKey<string>(), printerCode, outputParams);

            if (result == null)
                throw new DeveloperException("PrintReportStatus is null.");

            Func<string, string> toLowerStartSymbolHandler = str => str.Substring(0, 1).ToLower() + str.Substring(1);

            var message = string.Format(StringResources.PrintReportMessageBatchFormat, 
                result.HasError
                    ? string.Format(StringResources.EpsJobCreateError, result.Error.TrimEnd()) //TrimEnd() - убираем символы перевода строки
                    : toLowerStartSymbolHandler(string.Format(StringResources.EpsJobCreateOk, result.Job, report.ReportName,
                        result.Printer)));
            resultlist.Add(message);
        }

        private static void PrintReport(WMSBusinessObject entity, object item, List<string> resultlist, Report report,
            string printerCode = null)
        {
            var outputParams = GetFilterOutputParam(report, entity);

            PrintReportStatus result;
            using (var mgr = IoC.Instance.Resolve<IReport2EntityManager>())
                result = mgr.PrintReport(entity, report.GetKey<string>(), printerCode, outputParams);

            if (result == null)
                throw new DeveloperException("PrintReportStatus is null.");

            string message;
            if (item == null)
            {
                message = result.HasError
                    ? string.Format(StringResources.PrintReportMessageFormat, report.ReportName, string.Format(StringResources.EpsJobCreateError, result.Error.TrimEnd())) //TrimEnd() - убираем символы перевода строки
                    : string.Format(StringResources.EpsJobCreateOk, result.Job, report.ReportName, result.Printer);
            }
            else
            {
                message =
                    string.Format(StringResources.PrintReportMessageFormat, item,
                        result.HasError
                            ? string.Format(StringResources.EpsJobCreateError, result.Error.TrimEnd()) //TrimEnd() - убираем символы перевода строки
                            : string.Format(StringResources.EpsJobCreateOk, result.Job, report.ReportName,
                                result.Printer));
            }
            resultlist.Add(message);
        }

        private static IEnumerable<OutputParam> GetFilterOutputParam(Report report, WMSBusinessObject obj)
        {
            // если нечего дозапрашивать - выходим
            if (report.ReportFilterL == null || report.ReportFilterL.Count == 0)
                return null;

            // заполним дефолтные значения
            var soMgr = IoC.Instance.Resolve<ISysObjectManager>();
            foreach (var p in report.ReportFilterL)
            {
                if (string.IsNullOrEmpty(p.REPORTFILTERDEFAULTVALUE))
                    continue;

                var paramType = soMgr.GetTypeBySysObjectId(p.REPORTFILTERDATATYPE.To<int>());
                try
                {
                    var value = Convert.ChangeType(p.REPORTFILTERDEFAULTVALUE, paramType, CultureInfo.InvariantCulture);
                    p.Value = value;
                }
                catch (Exception ex)
                {
                    var message = string.Format(StringResources.ReportFilterParametersErrorFormat, p.REPORTFILTERPARAMETER, p.REPORTFILTERDEFAULTVALUE, paramType);
                    throw new InvalidCastException(message, ex);
                }
            }
            // получим и заполним по возможности все параметры типа find
            var findParams = report.ReportFilterL.Where(i => (ReportFilterMethod)Enum.Parse(typeof(ReportFilterMethod), i.REPORTFILTERMETHOD) != ReportFilterMethod.ASK);
            findParams = ProcessFindParams(findParams.ToArray(), obj);

            // получим параметры, которые нашли и не показывать
            var autoParams = findParams.Where(i => (ReportFilterMethod)Enum.Parse(typeof(ReportFilterMethod), i.REPORTFILTERMETHOD) == ReportFilterMethod.FIND
                && i.Value != null || (ReportFilterMethod)Enum.Parse(typeof(ReportFilterMethod), i.REPORTFILTERMETHOD) == ReportFilterMethod.AUTO
                && i.Value != null).OrderBy(i => i.REPORTFILTERORDER);

            // получим параметры которые надо спросить у пользователя
            var askParams = report.ReportFilterL.Where(i => (ReportFilterMethod)Enum.Parse(typeof(ReportFilterMethod), i.REPORTFILTERMETHOD) == ReportFilterMethod.ASK ||
            (ReportFilterMethod)Enum.Parse(typeof(ReportFilterMethod), i.REPORTFILTERMETHOD) == ReportFilterMethod.FINDASK ||
            (ReportFilterMethod)Enum.Parse(typeof(ReportFilterMethod), i.REPORTFILTERMETHOD) == ReportFilterMethod.AUTO && i.Value == null).OrderBy(i => i.REPORTFILTERORDER);

            // показываем фильтр пользователю
            var viewService = GetViewService();
            var filterModel = new FilterViewModel
            {
                IsFilterMode = false,
                PanelCaption = obj == null
                    ? string.Format("{0} \"{1}\" ", StringResources.ReportFilterCaption, report.ReportName)
                    : string.Format("{0} \"{1}\" {2} = {3}", StringResources.ReportFilterCaption, report.ReportName,
                        obj.GetPrimaryKeyPropertyName(), obj.GetKey())
            };
            var uniq = 0;
            var filterExpression = string.Empty;
            var autoFilterExpression = string.Empty;
            foreach (var p in autoParams)
            {
                var paramType = soMgr.GetTypeBySysObjectId(p.REPORTFILTERDATATYPE.To<int>());
                autoFilterExpression += AppendExpression(autoFilterExpression, paramType, p.REPORTFILTERPARAMETER, p.Value);
            }
            foreach (var p in askParams)
            {
                uniq++;
                var paramType = soMgr.GetTypeBySysObjectId(p.REPORTFILTERDATATYPE.To<int>());
                var field = new DataField
                {
                    Caption = p.REPORTFILTERDISPLAYNAME,
                    Description = p.REPORTFILTERDISPLAYNAME,
                    DisplayFormat = p.REPORTFILTERFORMAT,
                    FieldName = p.REPORTFILTERPARAMETER + uniq.To<string>(),
                    FieldType = paramType,
                    LookupCode = p.OBJECTLOOKUPCODE_R,
                    Name = p.REPORTFILTERPARAMETER,
                    SourceName = p.REPORTFILTERPARAMETER
                };
                filterModel.Fields.Add(field);
                filterExpression += AppendExpression(filterExpression, paramType, p.REPORTFILTERPARAMETER, p.Value);
            }
            filterModel.FilterExpression = filterExpression;
            var clause = string.Empty;
            var userCanceled = false;
            DispatcherHelper.Invoke(new Action(() =>
            {
                var res = viewService.ShowDialogWindow(viewModel: filterModel, isRestoredLayout: true, isNotNeededClosingOnOkResult: false, width: "40%", height: "50%");
                filterModel.AcceptChanges();
                clause = filterModel.GetSqlExpression();
                if (!string.IsNullOrEmpty(autoFilterExpression))
                {
                    filterModel.FilterExpression = autoFilterExpression;
                    filterModel.AcceptChanges();
                    var tmp = filterModel.GetSqlExpression();
                    if (!string.IsNullOrEmpty(tmp))
                        clause += string.Format("{0}({1})", string.IsNullOrEmpty(clause) ? string.Empty : " AND ",
                            tmp);
                }
                if (res != true)
                {
                    if (viewService.ShowDialog(StringResources.ApplyFilterTitle, string.Format(StringResources.CancelPreviewReportQuestionFormat, Environment.NewLine, clause), MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes)
                    {
                        userCanceled = true;
                    }
                }
            }));

            if (userCanceled)
                throw new UserAbortException();

            if (string.IsNullOrEmpty(clause))
                return null;

            var outputParam = new OutputParam
            {
                OutputParamCode = "WHERECLAUSE",
                OutputParamType = EpsParamType.REP.ToString(),
                OutputParamValue = clause,
                OutputParamSubvalue = report.ReportFile_R
            };
            return new[] { outputParam };
        }

        private static string AppendExpression(string expression, Type type, string key, object value)
        {
            if (typeof(string).IsAssignableFrom(type) || typeof(DateTime).IsAssignableFrom(type))
                return string.Format("{0}[{1}] = {2}", string.IsNullOrEmpty(expression) ? string.Empty : " AND ", key, value != null ? "'" + value + "'" : "?");
            return string.Format("{0}[{1}] = {2}", string.IsNullOrEmpty(expression) ? string.Empty : " AND ", key, value ?? "?");
        }

        private static IEnumerable<ReportFilter> ProcessFindParams(ReportFilter[] parameters, WMSBusinessObject obj)
        {
            var properties = TypeDescriptor.GetProperties(obj).Cast<PropertyDescriptor>().ToArray();
            foreach (var p in parameters)
            {
                var property = properties.FirstOrDefault(i => i.Name.EqIgnoreCase(p.REPORTFILTERPARAMETER));
                if (property != null)
                    p.Value = property.GetValue(obj);
            }
            return parameters;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _installedPrinters = null;
                if (_printerCache != null)
                    _printerCache.Clear();
            }
        }

        #endregion .  Methods  .

        public class ExportParam
        {
            public WMSBusinessObject Entity { get; set; }
            public Report Report { get; set; }
            public string FileName { get; set; }
            public string FilePath { get; set; }
            public IEnumerable<OutputParam> ParamExt { get; set; }

            public ExportParam(Report report, string fileName, WMSBusinessObject entity, IEnumerable<OutputParam> paramExt = null)
            {
                FileName = fileName;
                Entity = entity;
                Report = report;
                ParamExt = paramExt;
            }
        }
    }

    public interface IPrintViewModel : IViewModel
    {
        bool CancelPreview(bool force = false);
    }
}
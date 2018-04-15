//TODO: слить ShowView и ShowDialogWindow
//TODO: сейчас используется 2 модели Closing (через событие во View и через EventAggregator) - прийти к одному варианту

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Threading;
using Caliburn.Micro;
using DevExpress.Mvvm;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Docking;
using DevExpress.Xpf.Docking.Base;
using Microsoft.Practices.Unity;
using MLC.WebClient;
using wmsMLC.Business.Managers;
using wmsMLC.DCL.General.Helpers;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Helpers;
using wmsMLC.DCL.Main.Services.Commands;
using wmsMLC.DCL.Main.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.DCL.Main.Views.Controls;
using wmsMLC.DCL.Main.Views.Templates;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Events;
using wmsMLC.General.PL.WPF.Helpers;
using wmsMLC.General.PL.WPF.Services;
using wmsMLC.General.PL.WPF.ViewModels;
using wmsMLC.General.PL.WPF.Views;
using Action = System.Action;
using DispatcherHelper = wmsMLC.General.PL.WPF.Helpers.DispatcherHelper;
using ILog = log4net.ILog;
using IoC = wmsMLC.General.IoC;
using LogManager = log4net.LogManager;

namespace wmsMLC.DCL.Main.Services
{
    public class ViewService : IViewService, IHandle<CloseRequestEvent>, IHandle<ErrorEvent>, IShowReport
    {
        #region . WINAPI .
        [DllImport("user32.dll", EntryPoint = "GetActiveWindow")]
        private static extern IntPtr WinApiGetActiveWindow();
        #endregion . WINAPI .

        #region .  Constants & Fields  .
        private const string EntityFile = "DCLLayout";
        private ILog _log = LogManager.GetLogger(typeof(ViewService));

        public static ShowContext DefaultShowContext = new ShowContext();
        private static Regex captionNumberRegex = new Regex(@"\((\d+)\)$");

        private readonly MainView _mainView;
        private readonly IEventAggregator _eventAggregator;
        private readonly IShell _shell;
        private readonly IUnityContainer _container;
        private readonly Dictionary<string, Type> _registeredCommands = new Dictionary<string, Type>();
        private readonly Dictionary<Type, UIElement> _views = new Dictionary<Type, UIElement>();
        private static readonly Dictionary<string, string> WmsEntityMappingToWeb = new Dictionary<string, string>();
        #endregion .  Constants & Fields  .

        public ViewService(IUnityContainer container)
        {
            _container = container;
            _mainView = container.Resolve<MainView>();
            _eventAggregator = container.Resolve<IEventAggregator>();
            _shell = container.Resolve<IShell>();

            _shell.Closing += (s, a) => { a.Cancel = !CloseAll(); };

            _mainView.dockManager.DockItemClosing += (s, a) => { a.Cancel = !Closing(a.Item); };
            _mainView.dockManager.DockItemClosed += (s, a) =>
            {
                var items = a.AffectedItems;
                foreach (var item in items)
                {
                    var disp = item as IDisposable;
                    if (disp != null)
                    {
                        disp.Dispose();
                    }
                }
            };
            _mainView.dockManager.DockItemActivated += dockManager_DockItemActivated;

            _eventAggregator.Subscribe(this);
        }

        private void dockManager_DockItemActivated(object sender,
            DockItemActivatedEventArgs ea)
        {
            ProcessActivatable(ea.OldItem, false);
            ProcessActivatable(ea.Item, true);
        }

        private void ProcessActivatable(object obj, bool isActive)
        {
            if (obj != null)
            {
                var clp = obj as CustomLayoutPanel;
                if (clp != null)
                {
                    var uc = clp.Control as UserControl;
                    if (uc != null)
                    {
                        var ctx = uc.DataContext as IActivatable;
                        if (ctx != null)
                            ctx.IsActive = isActive;
                    }
                }
            }
        }

        #region .  IViewService  .
        public void Register(string command, Type viewModelOrCommandType)
        {
            _registeredCommands.Add(command.ToUpper(), viewModelOrCommandType);
        }

        public void Show(string command)
        {
            Show(command, DefaultShowContext);
        }

        public IViewModel ResolveViewModel(string command)
        {
            IViewModel vm;
            if (TryResolveViewModel(command, out vm))
                return vm;
            throw new DeveloperException(DeveloperExceptionResources.CommandIsNotRegistered, command);
        }

        public bool TryResolveViewModel(string command, out IViewModel viewModel)
        {
            viewModel = null;
            var key = _registeredCommands.Keys.FirstOrDefault(i => i.EqIgnoreCase(command));
            if (key == null)
                return false;
            viewModel = _container.Resolve(_registeredCommands[key]) as IViewModel;
            return viewModel != null;
        }

        public IViewCommand ResolveViewCommand(string command)
        {
            IViewCommand vc;
            if (TryResolveViewCommand(command, out vc))
                return vc;
            throw new DeveloperException(DeveloperExceptionResources.CommandIsNotRegistered, command);
        }

        public bool TryResolveViewCommand(string command, out IViewCommand viewCommand)
        {
            viewCommand = null;

            if (command == null)
                return false;

            var lowerCommand = command.ToLower();
            if (lowerCommand.StartsWith("command=browseentities"))
            {
                viewCommand = _container.Resolve<BrowseEntitiesCommand>();
                return true;
            }

            var key = _registeredCommands.Keys.FirstOrDefault(i => i.EqIgnoreCase(command));
            if (key == null)
                return false;
            viewCommand = _container.Resolve(_registeredCommands[key]) as IViewCommand;
            return viewCommand != null;
        }

        public void Show(string command, ShowContext context)
        {
            IViewCommand viewCommand;
            if (TryResolveViewCommand(command, out viewCommand))
            {
                viewCommand.Execute(command);
                return;
            }

            var vm = ResolveViewModel(command);
            if (vm == null)
                throw new DeveloperException(string.Format(DeveloperExceptionResources.CantCreateViewmodelForCommand, command));

            Show(vm, context);
        }

        public void Show(IViewModel viewModel)
        {
            Show(viewModel, DefaultShowContext);
        }

        private UIElement GetBindViewByViewModel(IViewModel viewModel)
        {
            if (viewModel == null)
                throw new ArgumentNullException("viewModel");

            UIElement view = null;
            DispatcherHelper.Invoke(new Func<UIElement>(() =>
            {
                var key = viewModel.GetType();
                if (_views.ContainsKey(key))
                {
                    view = _views[key];
                    if (view is PanelView)
                        ((PanelView) view).SetStartLoadTime();

                    _views.Remove(key);
                }
                else
                {
                    view = ViewLocator.LocateForModel(viewModel, null, null);
                }

                if (view == null)
                    throw new DeveloperException(DeveloperExceptionResources.CantFindViewForViewModel, viewModel);

                ViewModelBinder.Bind(viewModel, view, null);
                return view;
            }));

            return view;
        }

        public void Show(IViewModel viewModel, ShowContext context, ref IViewModel currentViewModel)
        {
            var view = GetBindViewByViewModel(viewModel);
            Show((FrameworkElement)view, context, ref currentViewModel);
        }

        public void Show(IViewModel viewModel, ShowContext context)
        {
            IViewModel currentViewModel = null;
            Show(viewModel, context, ref currentViewModel);
        }

        public void Show(object viewModel, object id, object title)
        {
            BrowseEntitiesCommand.Invoke(new Action(() =>
            {
                var docManager = IoC.Instance.Resolve<IDocumentManagerService>();
                var finddoc = BrowseEntitiesCommand.FindDocumentById(docManager, id);
                if (finddoc != null)
                {
                    finddoc.Show();
                    return;
                }

                var doc = docManager.CreateDocument("EntityListView", viewModel);
                doc.Id = id;
                doc.Title = title;
                doc.Show();
            }));
        }

        public IView GetView(IViewModel viewModel)
        {
            return GetBindViewByViewModel(viewModel) as IView;
        }

        public void AddViewToCache(Type modelType, IView view)
        {
            var element = view as UIElement;
            if (element != null)
                _views[modelType] = element;
        }

        public void Show(IView view)
        {
            Show(view, DefaultShowContext);
        }

        public void Show(IView view, ShowContext context)
        {
            Show((FrameworkElement)view, context);
        }

        public void Show(FrameworkElement content)
        {
            Show(content, DefaultShowContext);
        }

        public void Show(FrameworkElement content, ShowContext context)
        {
            IViewModel viewModel = null;
            Show(content, context, ref viewModel);
        }

        private void Show(FrameworkElement content, ShowContext context, ref IViewModel viewModel)
        {
            if (content == null)
                throw new ArgumentNullException("content");

            IViewModel viewModelInternal = null;
            DispatcherHelper.Invoke(new Action(() =>
            {
                // ищем уже открытие панели
                var panels = FindExistsPanels(content);

                // определяем среди открытых вкладку с максимальным индексом
                var maxPanel = GetPanelWithMaxIndex(panels);

                CustomLayoutPanel panel;

                // если ничего не открыто, или явно просят открыть
                var needCreate = panels == null || panels.Length == 0 || context.ShowInNewWindow;
                if (needCreate)
                {
                    panel = CreateNewPanel(content, context.DockingType);
                    // выставляем "следующее имя"
                    if (maxPanel != null)
                    {
                        var nextName = GetNextPanelName(maxPanel);
                        if (!string.IsNullOrEmpty(nextName))
                            panel.Caption = nextName;
                    }
                }
                else
                {
                    panel = maxPanel;
                }

                _mainView.dockManager.Activate(panel);

                var v = panel.Content as IView;
                if (v != null)
                    viewModelInternal = v.DataContext as IViewModel;

                // подписываемся на событие открытия, если необходимо загрузить настройки отображения
                if (needCreate && context.LoadLayoutSettings)
                {
                    panel.Loaded -= OnControlLoaded;
                    panel.Loaded += OnControlLoaded;
                }
            }));

            viewModel = viewModelInternal;
        }

        private void OnControlLoaded(object sender, RoutedEventArgs e)
        {
            var control = sender as Control;
            if (control == null)
                return;

            control.Loaded -= OnControlLoaded;
            RestoreLayout(control);
        }

        public bool? SaveLayout(IViewModel viewModel, string title)
        {
            var views = GetViewsByViewModel(viewModel).ToArray();
            var model = new SaveLayoutViewModel
            {
                PanelCaption = title,
                HasViewWindow = views.Any(p => p is Window),
                IsReadOnly = views.Length == 0
            };
            if (ShowDialogWindow(viewModel: model, isRestoredLayout: false, isNotNeededClosingOnOkResult: true) != true)
                return null;

            var result = true;
            foreach (var view in views.Where(view => !SaveLayout((FrameworkElement) view, model.Components)))
                result = false;
            return result;
        }

        public bool SaveLayout(FrameworkElement element, FormComponents formComponents)
        {
            var result = true;
            Action<DependencyObject, VisualTreeHelperExt.ProcessContext> actionHandler = (o, ctx) =>
                {
                    var el = o as FrameworkElement;

                    if (el == null)
                        return;

                    if (!(el is ISaveRestore))
                        return;

                    if (!SaveRestoreLayoutHelper.SaveLayout(el, formComponents) && result)
                        result = false;
                };

            VisualTreeHelperExt.ProcessElement(element, actionHandler);

            //HACK: Если есть BarManager, то он в НЕКОТОРЫХ СЛУЧАЯХ не выдает children
            if (VisualTreeHelperExt.FindChild(element, typeof(BarManager)) != null)
            {
                VisualTreeHelperExt.ProcessVisualChildren(element, actionHandler);
            }
            else
            {
                VisualTreeHelperExt.ProcessChildren(element, actionHandler);
            }

            return result;
        }

        public bool? SaveDBLayout(IViewModel viewModel, string title, bool upCriticalVersion = false)
        {
            var views = GetViewsByViewModel(viewModel).ToArray();
            var model = new SaveLayoutViewModel
            {
                PanelCaption = title,
                HasViewWindow = views.Any(p => p is Window),
                IsReadOnly = views.Length == 0
            };
            if (ShowDialogWindow(viewModel: model, isRestoredLayout: false, isNotNeededClosingOnOkResult: true) != true)
                return null;

            var result = true;
            foreach (var view in views.Where(view => !SaveDBLayout((FrameworkElement)view, model.Components, upCriticalVersion)))
                result = false;
            return result;
        }

        public bool SaveDBLayout(FrameworkElement element, FormComponents formComponents, bool upCriticalVersion = false)
        {
            var result = true;
            using (var mgr = IoC.Instance.Resolve<IEntityFileManager>())
            {
                if (mgr == null)
                    return false;

                Action<DependencyObject, VisualTreeHelperExt.ProcessContext> actionHandler = (o, ctx) =>
                {
                    var el = o as FrameworkElement;
                    if (el == null)
                        return;

                    if (!(el is ISaveRestore))
                        return;

                    if (!SaveRestoreLayoutHelper.SaveLayout(el, formComponents))
                    {
                        result = false;
                        return;
                    }

                    var version = SaveRestoreLayoutHelper.GetMaxDbVersion(GetFileDB(el));
                    if (version != null)
                    {
                        version = upCriticalVersion
                            ? new Version(version.Major, version.Minor + 1, 0, 0)
                            : new Version(version.Major, version.Minor, version.Build + 1, 0);
                    }
                    else
                    {
                        version = new Version(0, 0, 0, 0);
                    }

                    var fileNames = SaveRestoreLayoutHelper.GetSettingsFullFileName(el);
                    foreach (var fileName in fileNames)
                    {
                        if (!string.IsNullOrEmpty(fileName.CurrentFullFileName))
                        {
                            var fl = new FileInfo(fileName.CurrentFullFileName);
                            if (fl.Exists)
                            {
                                fileName.Version = version;
                                fileName.SetCurrentFileName();
                                fl.MoveTo(fileName.CurrentFullFileName);

                                var body = File.ReadAllText(fileName.CurrentFullFileName);
                                mgr.SetFileBody(EntityFile, fileName.GetFileNameBd(), body);

                                var entityFiles =
                                    mgr.GetFiltered(string.Format("FILE2ENTITY = '{0}' and FILEKEY = '{1}'", EntityFile,
                                        fileName.GetFileNameBd())).ToArray();
                                if (entityFiles.Length > 0)
                                {
                                    entityFiles[0].FileVersion = version.ToString();
                                    mgr.Update(entityFiles[0]);
                                }
                            }
                        }
                    }
                };

                VisualTreeHelperExt.ProcessElement(element, actionHandler);
                VisualTreeHelperExt.ProcessChildren(element, actionHandler);
            }

            return result;
        }

        public void AllRestoreLayout()
        {
            using (var mgr = IoC.Instance.Resolve<IEntityFileManager>())
            {
                var fileHeaders = mgr.GetFileHeaders(EntityFile).ToArray();
                var path = SaveRestoreLayoutHelper.GetDefaultRootPath();
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                var allFile = (Directory.GetFiles(path, "*.xml"));

                foreach (var flDb in fileHeaders)
                {
                    var flDbName = flDb.FileKey;
                    var flMask = Path.GetFileNameWithoutExtension(flDbName);

                    var flPattern = string.Format("{0}{1}", flMask, SaveRestoreLayoutHelper.VersionPrefix);

                    Version flDbVersion;
                    if (!Version.TryParse(flDb.FileVersion, out flDbVersion))
                        flDbVersion = new Version(0, 0, 0, 0);

                    var flClientList = allFile
                        .Where(i => i.Contains(flPattern)).ToArray();

                    bool isNeedWrite = false;

                    if (flClientList.Length == 1)
                    {
                        var version = SaveRestoreLayoutHelper.GetVersionByFile(flClientList[0], flMask);

                        if (version == null || flDbVersion.Minor > version.Minor)
                        {
                            var fl = new FileInfo(flClientList[0]);
                            fl.Delete();
                            isNeedWrite = true;
                        }
                    }
                    if (flClientList.Length == 0)
                    {
                        isNeedWrite = true;
                    }
                    else
                    {
                        Version maxVersion = null;
                        int maxItem = -1;

                        for (int i = 0; i < flClientList.Length; i++)
                        {
                            var version = SaveRestoreLayoutHelper.GetVersionByFile(flClientList[i], flMask);

                            var fl = new FileInfo(flClientList[i]);
                            if (version == null || flDbVersion.Minor > version.Minor)
                            {
                                fl.Delete();
                            }
                            else
                            {
                                if (maxVersion == null || version > maxVersion)
                                {
                                    maxVersion = (Version)version.Clone();
                                    maxItem = i;
                                }
                            }
                        }
                        for (int i = 0; i < flClientList.Length; i++)
                        {
                            if (maxItem == -1 || maxItem != i)
                            {
                                var fl = new FileInfo(flClientList[i]);
                                fl.Delete();
                            }
                        }
                        if (maxItem == -1)
                            isNeedWrite = true;

                    }

                    //Для совместимости со старыми названиями файлов
                    var flold = new FileInfo(Path.Combine(path, flDbName));
                    if (flold.Exists)
                    {
                        //Если не нашлось файла с версией и версия в БД начальная, то берем за роснову старый файл
                        if (!isNeedWrite || flDbVersion.Minor > 0)
                        {
                            flold.Delete();
                        }
                        else
                        {
                            flold.MoveTo(SaveRestoreLayoutHelper.GetFullFileNameClient(path, flMask, new Version(0, 0, 0, 0)));
                            isNeedWrite = false;
                        }
                    }

                    if (isNeedWrite)
                    {
                        var fileName = SaveRestoreLayoutHelper.GetFullFileNameClient(path, flMask, flDbVersion);
                        string contents = null;

                        try
                        {
                            contents = mgr.GetFileBodyByEntity(flDb.File2Entity, flDb.FileKey);
                        }
                        catch (Exception ex)
                        {
                            _log.Error(string.Format("Ошибка получения файла настроек '{0}' (Entity = '{1}', Key = '{2}').", fileName, flDb.File2Entity, flDb.FileKey), ex);
                        }
                        
                        if (!string.IsNullOrEmpty(contents))
                        {
                            File.WriteAllText(fileName, contents, Encoding.UTF8);
                            _log.InfoFormat("Загружен файл настроек {0}", fileName);
                        }
                    }
                }
            }
        }

        public void RestoreLayout(FrameworkElement obj)
        {
            Action<FrameworkElement> actionHandler = frameworkElement =>
            {
                //if (SaveRestoreLayoutHelper.IsElementRestored(frameworkElement))
                //    return;
                SaveRestoreLayoutHelper.RestoreLayout(frameworkElement, FormComponents.All); 
            };

            if (obj is Window)
            {
                DispatcherHelper.BeginInvoke(new Action(() => actionHandler(obj)));
            }
            else
            {
                if (Application.Current.Windows.Count > 1)
                {
                    var window = VisualTreeHelperExt.GetLogicalParent<Window>(obj);
                    if (window != null)
                        DispatcherHelper.BeginInvoke(new Action(() => actionHandler(window)));
                }
            }

            VisualTreeHelperExt.ProcessChildren(obj, true, (o, context) =>
            {
                var element = o as FrameworkElement;
                if (element == null)
                    return;

                DispatcherHelper.BeginInvoke(new Action(() => actionHandler(element)));
            });
        }

        public bool? ClearLayout(IViewModel viewModel, string title, Type[] actionProcessingObjectTypes = null, Type[] doNotActionProcessingObjectTypes = null)
        {
            var views = GetViewsByViewModel(viewModel).ToArray();
            var model = new ClearLayoutViewModel
            {
                PanelCaption = title,
                HasViewWindow = views.Any(p => p is Window),
                IsReadOnly = views.Length == 0
            };
            if (ShowDialogWindow(viewModel: model, isRestoredLayout: false, isNotNeededClosingOnOkResult: true) != true)
                return null;

            var result = true;
            foreach (var view in views)
            {
                if (!ClearLayout((FrameworkElement)view, model.Components, actionProcessingObjectTypes, doNotActionProcessingObjectTypes))
                    result = false;
            }
            return result;
        }

        public bool ClearLayout(FrameworkElement obj, FormComponents formComponents, Type[] actionProcessingObjectTypes = null, Type[] doNotActionProcessingObjectTypes = null)
        {
            var result = true;
            Action<DependencyObject, VisualTreeHelperExt.ProcessContext> actionHandler = (o, ctx) =>
                {
                    var element = o as FrameworkElement;

                    if (element == null)
                        return;

                    if (!(element is ISaveRestore))
                        return;

                    if (!SaveRestoreLayoutHelper.ClearLayout(element, GetFileDB(element), formComponents, actionProcessingObjectTypes,
                            doNotActionProcessingObjectTypes))
                        result = false;
                    else
                        SaveRestoreLayoutHelper.RestoreLayout(element, formComponents, actionProcessingObjectTypes, 
                            doNotActionProcessingObjectTypes);
                };

            VisualTreeHelperExt.ProcessElement(obj, actionHandler);
            VisualTreeHelperExt.ProcessChildren(obj, actionHandler);
            return result;
        }

        private Dictionary<string, FileAppearanceSettingsDb> GetFileDB(FrameworkElement element)
        {
            var result = new Dictionary<string, FileAppearanceSettingsDb>();
            using (var mgr = IoC.Instance.Resolve<IEntityFileManager>())
            {
                if (mgr == null || element == null)
                    return null;

                foreach (var filenames in SaveRestoreLayoutHelper.GetSettingsFullFileName(element))
                {
                    var entityFiles = mgr.GetFiltered(string.Format("FILE2ENTITY = '{0}' and FILEKEY = '{1}'", EntityFile, filenames.GetFileNameBd())).ToArray();

                    Version flDbVersion = null;
                    if (entityFiles.Length > 0)
                    {
                        if (!Version.TryParse(entityFiles[0].FileVersion, out flDbVersion))
                            flDbVersion = new Version(0, 0, 0, 0);
                    }

                    var fileBody = mgr.GetFileBodyByEntity(EntityFile, filenames.GetFileNameBd());
                    if (string.IsNullOrEmpty(fileBody))
                        continue;
                    result[filenames.GetFileNameBd()] = new FileAppearanceSettingsDb(Encoding.UTF8.GetBytes(fileBody), flDbVersion);
                }
            }
            return result;
        }

        public MessageBoxResult ShowDialog(string title, string message, MessageBoxButton buttons, MessageBoxImage image, MessageBoxResult defaultResult, double? fontSize = null)
        {
            if (Application.Current.Dispatcher.CheckAccess())
                return DXMessageBox.Show(message, title, buttons, image, defaultResult);
            return (MessageBoxResult)Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                new Func<MessageBoxResult>(() => DXMessageBox.Show(message, title, buttons, image, defaultResult)));
        }

        public bool? ShowDialogWindow(IViewModel viewModel, bool isRestoredLayout, bool isNotNeededClosingOnOkResult = false, string width = null, string height = null, bool noButtons = false, MessageBoxButton buttons = MessageBoxButton.OKCancel, bool noActionOnCancelKey = false, IntPtr? intPtrOwner = null)
        {
            if (viewModel == null)
                throw new ArgumentNullException("viewModel");

            bool? result = null;
            DispatcherHelper.Invoke(new Action(() =>
            {
                var view = GetBindViewByViewModel(viewModel);

                // если view не является window, то его не нужно оборачивать
                var dialogWindow = view as Window;

                if (dialogWindow == null)
                {
                    //Если калибрун не сбиндил тайтлы, попробуем выставить сами
                    var panel = view as IPanelView;
                    if (panel != null)
                    {
                        if (string.IsNullOrEmpty(panel.PanelCaption) && viewModel is PanelViewModelBase)
                            panel.PanelCaption = ((PanelViewModelBase) viewModel).PanelCaption ?? string.Empty;
                    }
                    var newWindow = CreateDialogWindow(view, noButtons, buttons, noActionOnCancelKey);
                    dialogWindow = newWindow;
                }

                ParseRelativeWindowSize(dialogWindow, width, height);

                if (width != null & height == null)
                {
                    dialogWindow.SizeToContent = SizeToContent.Height;
                }
                else if (width == null & height != null)
                {
                    dialogWindow.SizeToContent = SizeToContent.Width;
                }
                else if (width != null & height != null)
                {
                    dialogWindow.SizeToContent = SizeToContent.Manual;
                }

                // присваиваем родителя
                if (intPtrOwner.HasValue)
                {
                    //Родитель задан как IntPtr
                    SetOwnerWindow(dialogWindow, intPtrOwner.Value);
                }
                else
                {
                    //Родитель задан как Window
                    if (dialogWindow.Owner == null && Application.Current.MainWindow.IsActive)
                    {
                        dialogWindow.Owner = Application.Current.MainWindow;
                    }
                    else
                    {
                        dialogWindow.Owner = GetActiveWindow();
                        if (dialogWindow.Owner == null)
                        {
                            // Так должно работать и на могомониторных системах - проверить
                            var fv = PresentationSource.FromVisual(Application.Current.MainWindow);
                            if (fv != null)
                                dialogWindow.Owner = (Window) fv.RootVisual;
                        }
                    }
                }

                // Загрузка вида
                if (isRestoredLayout && view is Control)
                {
                    var control = (Control)view;
                    control.Loaded -= OnControlLoaded;
                    control.Loaded += OnControlLoaded;
                }

                //Горячие клавиши
                if (viewModel is IActivatable)
                    ((IActivatable) viewModel).IsActive = true;

                if (isNotNeededClosingOnOkResult && dialogWindow is CustomDXDialog && viewModel is IActionHandler)
                {
                    dialogWindow.Loaded += delegate
                    {
                        var window = (CustomDXDialog)dialogWindow;
                        if (window.OkButton != null)
                        {
                            window.OkButton.DataContext = viewModel;
                            window.OkButton.SetBinding(ButtonBase.CommandProperty, new Binding("DoActionCommand"));
                        }
                    };
                }

                // если окно закрываемое
                //            var closableView = view as IClosable;
                //            if (closableView != null)
                //            {
                //                dialogWindow.Closing += (s, a) => a.Cancel = !closableView.CanClose();
                //                dialogWindow.Closed += (s, a) => closableView.Close();
                //            }
                
                bool? dr;
                // подписываемся на закрытие (и сразу же отписываемся)
                CancelEventHandler cancelDelegate = (sender, args) =>
                {
                    if (isNotNeededClosingOnOkResult && dialogWindow.DialogResult == true)
                    {
                        var objectViewModel = viewModel as IActionHandler;
                        if (objectViewModel != null)
                        {
                            args.Cancel = !objectViewModel.DoAction();
                        }
                        return;
                    }
                    // если модель поддерживает DialogResultHandler, то выставим результат диалога
                    var drH = viewModel as IDialogResultHandler;
                    if (drH != null)
                        drH.DialogResult = dialogWindow.DialogResult;

                    //if (isNotNeededClosingOnOkResult && dialogWindow.DialogResult == true) return;
                    args.Cancel = !Closing((Window)sender);
                };

                var customModelHandler = viewModel as ICustomModelHandler;
                if (customModelHandler != null)
                {
                    customModelHandler.MenuAction = parameter =>
                    {
                        if (dialogWindow is CustomDXDialog)
                            ((CustomDXDialog)dialogWindow).TrueResult = true;
                        dialogWindow.DialogResult = true;
                        dialogWindow.Close();
                    };
                }

                try
                {
                    dialogWindow.Closing += cancelDelegate;
                    dr = dialogWindow.ShowDialog();
                }
                finally
                {
                    dialogWindow.Closing -= cancelDelegate;
                }

                result = dialogWindow is CustomDXDialog ? ((CustomDXDialog)dialogWindow).TrueResult : dr;

                var disposable = dialogWindow as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }));
            return result;
        }

        private static void ParseRelativeWindowSize(Window dialogWindow, string width, string height)
        {
            double parsewidth;
            if (Str2Dimension(width, SystemParameters.PrimaryScreenWidth, out parsewidth))
                dialogWindow.Width = parsewidth;

            double parseHeight;
            if (Str2Dimension(height, SystemParameters.PrimaryScreenHeight, out parseHeight))
                dialogWindow.Height = parseHeight;
        }

        private static bool Str2Dimension(string value, double screenMeasure, out double dimension)
        {
            dimension = 0;
            if (value == null)
                return false;

            var trimValue = value.Trim();
            if (trimValue == string.Empty)
                return false;

            var parseStr = trimValue;
            bool isProc = trimValue[value.Length - 1] == '%' || trimValue[value.Length - 1] == '*';
            if (isProc)
                parseStr = trimValue.Substring(0, value.Length - 1);

            double parseValue;
            if (!double.TryParse(parseStr, out parseValue))
                return false;

            dimension = isProc ? screenMeasure * parseValue / 100 : parseValue;
            return true;
        }

        private static CustomDXDialog CreateDialogWindow(UIElement view, bool noButtons = false, MessageBoxButton buttons = MessageBoxButton.OKCancel, bool noActionOnCancelKey = false)
        {
            DialogButtons dlgBtn;
            switch (buttons)
            {
                case MessageBoxButton.OK:
                    dlgBtn = DialogButtons.Ok;
                    break;
                case MessageBoxButton.OKCancel:
                    dlgBtn = DialogButtons.OkCancel;
                    break;
                case MessageBoxButton.YesNo:
                    dlgBtn = DialogButtons.YesNo;
                    break;
                case MessageBoxButton.YesNoCancel:
                    dlgBtn = DialogButtons.YesNoCancel;
                    break;
                default:
                    dlgBtn = DialogButtons.OkCancel;
                    break;
            }
            var newWindow = new CustomDXDialog
            {
                ShowTitle = false,
                ShowIcon = false,
                ShowInTaskbar = false,
                ResizeMode = ResizeMode.CanResize,
                Content = view,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                SizeToContent = SizeToContent.WidthAndHeight,
                NoButtons = noButtons,
                Buttons = dlgBtn,
                NoActionOnCancelKey = noActionOnCancelKey
            };

            if (view is IPanelView)
            {
                newWindow.ShowTitle = true;
                var title = ((IPanelView)view).PanelCaption;
                if (!string.IsNullOrEmpty(title))
                    newWindow.Title = title;
            }
            return newWindow;
        }

        private Window FindWindowByViewModel(object viewModel)
        {
            var window =
                Application.Current.Windows.OfType<Window>()
                    .FirstOrDefault(w => w.IsActive && (w.DataContext == viewModel ||
                                                        (w.Content is IView &&
                                                         ((IView)w.Content).DataContext == viewModel)));
            return window;
        }

        private bool CloseWindow(object viewModel)
        {
            var window = FindWindowByViewModel(viewModel);
            if (window != null)
            {
                window.Close();
                if (window is CustomDXDialog)
                    ((CustomDXDialog)window).TrueResult = true;
                return true;
            }
            return false;
        }

        public bool Close(object viewOrViewModel, bool isDialog = false)
        {
            if (isDialog)
            {
                if (CloseWindow(viewOrViewModel))
                    return false;
            }

            var item = _mainView.dockManager.GetItems()
                .Where(i => i is CustomLayoutPanel)
                .Cast<CustomLayoutPanel>()
                .FirstOrDefault(
                    i => i.Content is IView &&
                         (i.Content == viewOrViewModel ||
                          ((IView)i.Content).DataContext == viewOrViewModel));

            if (item == null)
            {
                if (CloseWindow(viewOrViewModel))
                    return false;

                throw new DeveloperException(DeveloperExceptionResources.CantFindItemForClose);
            }

            if (!item.AllowClose)
                return false;

            return _mainView.dockManager.DockController.Close(item);
        }

        public bool CloseAll(bool absolutely)
        {
            var docManager = IoC.Instance.Resolve<IDocumentManagerService>();
            foreach (var d in docManager.Documents.ToArray())
            {
                d.Close();
            }

            //TODO: доабвить стратегию обхода
            var items = _mainView.dockManager.GetItems().Where(i => i is CustomLayoutPanel).Cast<CustomLayoutPanel>().ToArray();
            if (items.Length == 0)
                return true;

            foreach (var item in items)
            {
                if (!absolutely && !item.AllowClose)
                    continue;

                item.AllowClose = true;

                //if (!_mainView.dockManager.DockController.Close(item))
                //    return false;

                _mainView.dockManager.DockController.Close(item);
                if (!item.IsDisposed)
                    return false;
            }

            return true;
        }

        public bool CloseAll()
        {
            return CloseAll(false);
        }
        #endregion

        private static CustomLayoutPanel GetPanelWithMaxIndex(CustomLayoutPanel[] items)
        {
            if (items == null || items.Length == 0)
                return null;

            if (items.Length == 1)
                return items[0];

            CustomLayoutPanel maxItem = items[0];
            int maxIndex = 0;

            foreach (var item in items)
            {
                int? idx = GetPanelCaptionIndex(item.ActualCaption);
                if (!idx.HasValue)
                    continue;

                if (idx > maxIndex)
                {
                    maxIndex = idx.Value;
                    maxItem = item;
                }
            }
            return maxItem;
        }

        private static int? GetPanelCaptionIndex(string panelCaption)
        {
            var m = captionNumberRegex.Match(panelCaption);
            if (m.Groups.Count == 0 || string.IsNullOrEmpty(m.Groups[0].Value))
                return null;

            if (m.Groups.Count > 1)
                return int.Parse(m.Groups[1].Value);

            throw new DeveloperException(string.Format(DeveloperExceptionResources.CantGetIndexFromPanel, panelCaption));
        }

        private static string GetNextPanelName(CustomLayoutPanel maxPanel)
        {
            if (string.IsNullOrEmpty(maxPanel.ActualCaption))
                return null;

            int? idx = GetPanelCaptionIndex(maxPanel.ActualCaption);
            var baseName = maxPanel.ActualCaption;
            if (idx.HasValue)
                baseName = baseName.Substring(0, baseName.Length - string.Format(" ({0})", idx.Value).Length);

            return baseName + string.Format(" ({0})", idx.HasValue ? idx + 1 : 1);
        }

        private IEnumerable<IView> GetViewsByViewModel(IViewModel viewModel)
        {
            var items = _mainView.dockManager.GetItems().Where(i => i is CustomLayoutPanel).Cast<CustomLayoutPanel>().ToArray();
            var result = items.Where(i => i.Content is IView).Select(i => (IView)i.Content).Where(i => i.DataContext == viewModel).ToList();
            if (Application.Current.Windows.Count > 1)
            {
                var windows = Application.Current.Windows.Cast<Window>().ToArray();
                var views = windows.Where(p => p.Content is IView).Select(p => (IView)p.Content).Where(p => p.DataContext == viewModel).ToList();
                if (!views.Any())
                    views.AddRange(windows.OfType<IView>().Where(p => p.DataContext == viewModel).ToArray());

                if (views.Any())
                    result.AddRange(views);
            }

            foreach (var view in result.OfType<FrameworkElement>()
                        .Where(p => !(p is Window) && p.Parent is Window && p.Parent is IView)
                        .Select(p => (IView) p.Parent)
                        .ToArray())
            {
                if (!result.Contains(view))
                    result.Add(view);
            }

            //return items.Where(i => i.Content is IView).Select(i => (IView)i.Content).Where(i => i.DataContext == viewModel).ToArray();
            return result;
        }

        private CustomLayoutPanel[] FindExistsPanels(object content)
        {
            var items = _mainView.dockManager.GetItems().Where(i => i is CustomLayoutPanel).Cast<CustomLayoutPanel>().ToArray();
            if (items.Length == 0)
                return items;
            
            var contentKey = GetContentKey(content);
            if (contentKey == null)
                return new CustomLayoutPanel[0];

            var results = new List<CustomLayoutPanel>();
            foreach (var item in items)
            {
                var itemKey = GetContentKey(item.Content);
                if (contentKey.Equals(itemKey))
                    results.Add(item);
            }
            return results.ToArray();
        }

        private static object GetContentKey(object content)
        {
            // если не view, то короме как по инстансу контента не поищешь
            var v = content as IView;
            if (v == null)
                return content;

            // если view, то смотрим модель
            var vm = v.DataContext as IModelHandler;
            if (vm == null)
                return v.DataContext;

            var un = vm as IHaveUniqueName;
            if (un != null)
                return un.GetUniqueName();

            // у вьюмодели можно спросить ключ (если это объект)
            var m = vm.GetSource() as IKeyHandler;
            if (m == null)
                return vm;

            var key = m.GetKey();
            if (key == null)
                return null;

            return string.Format("{0}-{1}", m.GetType(), key);
        }

        private CustomLayoutPanel CreateNewPanel(FrameworkElement content, DockType dockType)
        {
            var panel = CustomLayoutPanel.CreateWithView(content);
            LayoutGroup layoutGroup;
            switch (dockType)
            {
                case DockType.Left:
                    layoutGroup = _mainView.leftGroup;
                    break;

                case DockType.Right:
                    layoutGroup = _mainView.rightGroup;
                    break;

                case DockType.Document:
                    layoutGroup = _mainView.documentGroup;
                    break;

                default:
                    throw new NotImplementedException();
            }
            layoutGroup.Items.Add(panel);
            layoutGroup.SelectedTabIndex = layoutGroup.Items.Count - 1;
            return panel;
        }

        private bool Closing(FrameworkElement viewOrViewModel)
        {
            var res = true;
            VisualTreeHelperExt.ProcessChildren(viewOrViewModel, (o, context) =>
            {
                var closable = o as IClosable;
                if (closable == null)
                    return;

                // если объект closable своих детей он опросит сам
                //context.SkipChildren = true;

                // если закрывать нельзя, то останавливаем обход
                if (!closable.CanClose())
                {
                    context.StopEnumerate = true;
                    res = false;
                }
            });
            return res;
        }

        #region .  IHandle<>  .
        void IHandle<CloseRequestEvent>.Handle(CloseRequestEvent message)
        {
            Close(message.Requestor);
        }
        void IHandle<ErrorEvent>.Handle(ErrorEvent message)
        {
            ErrorBox.ShowError(message.Message, message.Exception);
        }
        #endregion

        #region .  IFastReport  .
        public void ShowReport(string fileName)
        {
            _mainView.ShowReport(fileName);
        }

        public void ShowReport(Stream stream)
        {
            _mainView.ShowReport(stream);
        }
        #endregion  .  IFastReport  .

        public void MakeMaxSize(IViewModel viewModel, bool isMax)
        {
            var contentKey = GetContentKey(viewModel);
            if (contentKey == null)
                return;

            var items = _mainView.dockManager.GetItems().Where(i => i is CustomLayoutPanel).Cast<CustomLayoutPanel>().ToArray();

            foreach (var item in from item in items let itemKey = GetContentKey(((IView)item.Content).DataContext) where contentKey.Equals(itemKey) select item)
            {
                _mainView.MakeMaxSize(item, isMax);
                return;
            }
        }

        public void ShowError(string message, Exception ex, string[] attachments,
           Dictionary<string, string> additionalParams,
           bool isBtnSandMailEnable = true, string mandantCode = null, bool isMandantListVisible = true)
        {
            DispatcherHelper.Invoke(
                new Action(() =>
                    ErrorBox.ShowError(message, ex, attachments, additionalParams, isBtnSandMailEnable, mandantCode, isMandantListVisible)));
        }

        public void ShowError(string message, Exception ex, bool isBtnSandMailEnable = true)
        {
            DispatcherHelper.Invoke(new Action(() =>
                ErrorBox.ShowError(message, ex, isBtnSandMailEnable)));
        }

        public Window GetActiveWindow()
        {
            var active = WinApiGetActiveWindow();
            return Application.Current.Windows.OfType<Window>().FirstOrDefault(window => new WindowInteropHelper(window).Handle == active && window.IsActive);
        }

        private void SetOwnerWindow(Window owned, IntPtr intPtrOwner)
        {
            try
            {
                var windowInteropHelper = new WindowInteropHelper(owned);
                var windowHandleOwned = windowInteropHelper.EnsureHandle();
                if (windowHandleOwned != IntPtr.Zero && intPtrOwner != IntPtr.Zero)
                    windowInteropHelper.Owner = intPtrOwner;
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
        }

        public bool TryGetWmsWebObject(string wmsobject, out string wmswebobject)
        {
            wmswebobject = null;
            if (WmsEntityMappingToWeb.ContainsKey(wmsobject))
            {
                wmswebobject = WmsEntityMappingToWeb[wmsobject];
                return true;
            }
            return false;
        }

        public async Task GetWebEntityMappingAsync(string wmsentity)
        {
            await Task.Factory.StartNew(() =>
            {
                if (WmsEntityMappingToWeb.ContainsKey(wmsentity))
                    return;
                var api = IoC.Instance.Resolve<WmsAPI>();
                var mapping = api.GetWebEntityByWmsEntityAsync(wmsentity).Result;
                if (mapping != null)
                {
                    foreach (var pair in mapping)
                    {
                        WmsEntityMappingToWeb[pair.Key] = pair.Value;
                    }
                }
            });
        }
    }
}
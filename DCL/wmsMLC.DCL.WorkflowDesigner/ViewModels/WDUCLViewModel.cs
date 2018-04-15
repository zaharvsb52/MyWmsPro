using System;
using System.Activities;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xaml;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.Business.Objects;
using wmsMLC.Business.Objects.Processes;
using wmsMLC.DCL.General.Helpers;
using wmsMLC.DCL.General.ViewModels.Menu;
using wmsMLC.DCL.Resources;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.WorkflowDesigner.Helpers;
using wmsMLC.DCL.WorkflowDesigner.ViewModels.Xaml;
using wmsMLC.DCL.WorkflowDesigner.Views;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Views;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace wmsMLC.DCL.WorkflowDesigner.ViewModels
{
    [View(typeof(WDUCLView))]
    public class WDUCLViewModel<TModel> : ObjectViewModelBase<TModel>, IHaveUniqueName, IWDUCLViewModel, IWDUCLViewModelInternal
    {
        private const int MaxLengthWorkflowActivitiesField = 1024;
        private const int MaxLengthWorkflowInternalField = 1024;

        private log4net.ILog _log = log4net.LogManager.GetLogger(typeof(WDUCLViewModel<TModel>));

        private BpContext _context;

        private const string XamlPropertyName = "WORKFLOWXAML";
        private readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;
        private bool _isReadOnly;

        //private Lazy<IDesignerViewModel> _designerViewModel;
        //private Lazy<IXamlViewModel> _xamlViewModel;
        private IDesignerViewModel _designerViewModel;
        private IXamlViewModel _xamlViewModel;

        private object _activity;

        private string _uniqueName;

        private string _fileName = string.Empty;
        private string _xamlPath = string.Empty;
        private string _compiledPath = string.Empty;
        private bool _isMaxView;
        private CommandMenuItem _fullScreenModeMenuItem;
        private bool _notNeedCanCloseEvent;

        public event EventHandler CanClose;

        public WDUCLViewModel()
        {
            _isReadOnly = true;
            AllowClosePanel = true;
            Errors = new ObservableCollection<DesignerErrorDefinition>();

            //STUB: не делаем Flush
            //узнаем об изменении модели здесь
            _designerViewModel = IoC.Instance.Resolve<IDesignerViewModel>();
            _designerViewModel.SurfaceChanged += delegate
            {
                ((IDesignerSurface) _designerViewModel.CurrentSurface).Designer.ModelChanged += delegate
                {
                    var xml = GetXamlValue(Source);
                    SetXamlValue(Source, " ");
                    SetXamlValue(Source, xml);
                };
            };
            _xamlViewModel = new XamlViewModel(_designerViewModel);
            _xamlViewModel.Document.TextChanged += delegate
            {
                SetXamlValue(Source, _xamlViewModel.Document.Text);
            };

            if (Commands == null)
                Commands = new List<ICommand>();

            NewCommand = new DelegateCustomCommand(New, CanNew);
            OpenCommand = new DelegateCustomCommand(Open, CanOpen);
            SaveToFileCommand = new DelegateCustomCommand(SaveToFile, CanSaveToFile);
            CompileCommand = new DelegateCustomCommand(Compile, CanCompile);
            RunCommand = new DelegateCustomCommand(Run, CanRun);
            FullScreenModeCommand = new DelegateCustomCommand(FullScreenModeChange);

            Commands.AddRange(new[] { NewCommand, OpenCommand, SaveToFileCommand, SaveCommand, CompileCommand, RunCommand, FullScreenModeCommand });

            RiseCommandsCanExecuteChanged();
        }

        #region .  Properties  .

        public UserControl MenuView { get; set; }

        public bool ErrorPopupIsOpen
        {
            get;
            set;
        }

        public ObservableCollection<DesignerErrorDefinition> Errors
        {
            get;
            private set;
        }

        public string Status
        {
            get;
            private set;
        }

        public ICommand NewCommand { get; private set; }
        public ICommand OpenCommand { get; private set; }
        public ICommand SaveToFileCommand { get; private set; }
        public ICommand CompileCommand { get; private set; }
        public ICommand RunCommand { get; private set; }
        public new List<ICommand> Commands { get; set; }
        public ICommand FullScreenModeCommand { get; private set; }
        #endregion .  Properties  .

        public override void CreateProcessMenu()
        {
            // STUB: нам не нужно меню
        }

        protected override void CreateMainMenu()
        {
            var bar = Menu.GetOrCreateBarItem(StringResources.Commands, 1, "BarItemCommands");

            bar.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.Start,
                Command = RunCommand,
                ImageSmall = ImageResources.DCLWFCompile16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLWFCompile32.GetBitmapImage(),
                HotKey = new KeyGesture(Key.F12),
                DisplayMode = DisplayModeType.Default,
                GlyphAlignment = GlyphAlignmentType.Top,
                Priority = 100
            });

            bar.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.Compile,
                Command = CompileCommand,
                ImageSmall = ImageResources.DCLWFCompile16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLWFCompile32.GetBitmapImage(),
                HotKey = new KeyGesture(Key.F11),
                DisplayMode = DisplayModeType.Default,
                GlyphAlignment = GlyphAlignmentType.Top,
                Priority = 101
            });

            bar.MenuItems.Add(new SeparatorMenuItem { Priority = 102 });

            bar.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.Save,
                Command = SaveCommand,
                ImageSmall = ImageResources.DCLSave16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLSave32.GetBitmapImage(),
                HotKey = new KeyGesture(Key.F7),
                DisplayMode = DisplayModeType.Default,
                GlyphAlignment = GlyphAlignmentType.Top,
                Priority = 200
            });

            bar.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.SaveToFile,
                Command = SaveToFileCommand,
                ImageSmall = ImageResources.DCLUnload16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLUnload32.GetBitmapImage(),
                HotKey = new KeyGesture(Key.F6),
                DisplayMode = DisplayModeType.Default,
                GlyphAlignment = GlyphAlignmentType.Top,
                Priority = 201
            });

            bar.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.Open,
                Command = OpenCommand,
                ImageSmall = ImageResources.DCLOpenFolder16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLOpenFolder32.GetBitmapImage(),
                HotKey = new KeyGesture(Key.F5),
                DisplayMode = DisplayModeType.Default,
                GlyphAlignment = GlyphAlignmentType.Top,
                Priority = 202
            });

            bar.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.New,
                Command = NewCommand,
                ImageSmall = ImageResources.DCLAddNew16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLAddNew32.GetBitmapImage(),
                HotKey = new KeyGesture(Key.F7),
                DisplayMode = DisplayModeType.Default,
                GlyphAlignment = GlyphAlignmentType.Top,
                Priority = 203
            });

            bar.MenuItems.Add(new SeparatorMenuItem { Priority = 204 });

            _fullScreenModeMenuItem = new CommandMenuItem
            {
                Caption = StringResources.ExpandPanel,
                Command = FullScreenModeCommand,
                ImageSmall = ImageResources.DCLExpandPack16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLExpandPack32.GetBitmapImage(),
                HotKey = new KeyGesture(Key.F12),
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 300
            };
            bar.MenuItems.Add(_fullScreenModeMenuItem);
        }

        private bool CanRun()
        {
            return true;
        }

        private void Run()
        {
            if (!ConnectionManager.Instance.AllowRequest())
                return;
            //var items = GetItemsParameter().ToArray();
            var managerInstance = IoC.Instance.Resolve<IBPProcessManager>();
            // TODO: собираем параметры для процесса
            // HACK: сейчас параметры заданы статически
            // настраиваем контекст
            // TODO: необходимо стандартизировать и вынести параметры в одно место
            //var context = new BpContext { Items = items };
            _context = new BpContext();
            _context.Set("DialogVisible", true); // признак того, что надо показывать диалоги
            managerInstance.Parameters.Add(BpContext.BpContextArgumentName, _context);

            // HACK: пока выбираем код БП из ключа WF
            var wfCode = _uniqueName.Replace("WF", string.Empty);
            WaitStart();
            var startTime = DateTime.Now;
            try
            {
                _log.DebugFormat("Start process: {0}", wfCode);
                managerInstance.Run(code: wfCode, completedHandler: OnBpProcessEnd);
            }
            catch (Exception)
            {
                WaitStop();
                throw;
            }
            finally
            {
                _log.DebugFormat("End process: {0} in {1}", wfCode, DateTime.Now - startTime);
            }
        }

        #region IWDUCLViewModelInternal

        public void PostStatus(string message)
        {
            Status = message;
            OnPropertyChanged("Status");
        }

        public void On_DisplayErrors(IEnumerable<DesignerErrorDefinition> errors)
        {
            _dispatcher.BeginInvoke(new Action(() =>
            {
                Errors.Clear();

                var items = errors.ToArray();
                if (!items.Any())
                {
                    ErrorPopupIsOpen = false;
                }
                else
                {
                    foreach (var item in items)
                    {
                        Errors.Add(item);
                    }

                    ErrorPopupIsOpen = true;
                }

                OnPropertyChanged("ErrorPopupIsOpen");
            }));
        }

        public IDesignerViewModel GetDesignerViewModel()
        {
            return _designerViewModel;
        }

        public IXamlViewModel GetXamlViewModel()
        {

            return _xamlViewModel;
        }

        public void StartWait()
        {
            WaitStart();
        }

        public void StopWait()
        {
            WaitStop();
        }

        public void LoadSource()
        {
            var obj = GetXamlValue(Source);
            if (string.IsNullOrEmpty(obj))
            {
                _activity = null;
            }
            else
            {
                try
                {
                    WaitStart();
                    using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(obj)))
                        _activity =
                            XamlServices.Load(ActivityXamlServices.CreateBuilderReader(new XamlXmlReader(stream))) as
                                ActivityBuilder;
                }
                catch (Exception ex)
                {
                    ExceptionPolicy.Instance.HandleException(ex, "Workflow");
                }
                finally
                {
                    WaitStop();
                }
            }

            if (CanNew())
                Reload();
        }

        #endregion

        #region IWDUCLViewModel

        void IWDUCLViewModel.SetSource(object source)
        {
            Source = (TModel)source;
            var src = source as WMSBusinessObject;
            if (src == null)
                throw new DeveloperException("Source type is not WMSBusinessObject");
            //PanelCaption = src.GetKey() != null ? src.GetKey().ToString() : string.Empty;
            var key = src.GetKey();
            _uniqueName = key != null ? key.ToString() : string.Empty;
            PanelCaption = GetUniqueName();
            RiseCommandsCanExecuteChanged();
        }

        bool IWDUCLViewModel.GetIsReadOnly()
        {
            return _isReadOnly;
        }

        void IWDUCLViewModel.SetIsReadOnly(bool isReadOnly)
        {
            _isReadOnly = isReadOnly;
            var src = Source as WMSBusinessObject;
            if (src == null)
                return;

            if (_isReadOnly)
                src.SuspendNotifications();
            else
                src.ResumeNotifications();
            RiseCommandsCanExecuteChanged();
        }
        #endregion IWDUCLViewModel

        private string GetXamlValue(object source)
        {
            var properties = TypeDescriptor.GetProperties(typeof(TModel));
            var property = properties.Cast<PropertyDescriptor>().FirstOrDefault(i => i.Name.EqIgnoreCase(XamlPropertyName));
            if (property == null)
                throw new DeveloperException("Object type has not XAML property.");
            var obj = property.GetValue(source);
            return obj != null ? obj.ToString() : null;
        }

        private void SetXamlValue(object source, string xaml)
        {
            var properties = TypeDescriptor.GetProperties(typeof(TModel));
            var property = properties.Cast<PropertyDescriptor>().FirstOrDefault(i => i.Name.EqIgnoreCase(XamlPropertyName));
            if (property == null)
                throw new DeveloperException("Object type has not XAML property.");
            //var text = property.GetValue(source).To<string>().GetTrim();
            //STUB: не делаем Flush
            //INFO: не режем строку, чтобы увидеть изменение
            //if (!string.Equals(xaml.Trim(), text))
            if(!string.IsNullOrEmpty(xaml))
                property.SetValue(source, xaml.Trim());
        }

        protected override void OnSourceChanged()
        {
            base.OnSourceChanged();
            PanelCaption = GetUniqueName();
        }

        protected virtual bool CanNew()
        {
            return true;// Check(BusinessObjectManager<string, int>.NewMethodName);
        }

        protected virtual void New()
        {
            _activity = null;
            Reload();
        }

        private void Reload()
        {
            _notNeedCanCloseEvent = true;
            if (CanCloseInternal())
            {
                try
                {
                    WaitStart();
                    _fileName = string.Empty;
                    _designerViewModel.ReloadDesigner(_activity ?? new ActivityBuilder());
                }
                finally
                {
                    _notNeedCanCloseEvent = false;
                    WaitStop();
                }
            }
        }

        protected virtual bool CanOpen()
        {
            return true;// Check(BusinessObjectManager<string, int>.NewMethodName);
        }

        protected virtual void Open()
        {
            _notNeedCanCloseEvent = true;
            if (CanCloseInternal())
            {
                var surface = (IDesignerSurface)_designerViewModel.CurrentSurface;
                surface.Designer.Flush();

                var dlg = new OpenFileDialog
                    {
                        FileName = string.IsNullOrEmpty(_uniqueName) ? "Workflow" : _uniqueName,
                        Filter = "Activity files (.xaml)|*.xaml",
                        InitialDirectory = _xamlPath
                    };
                if (dlg.ShowDialog().GetValueOrDefault())
                {
                    try
                    {
                        WaitStart();
                        _fileName = dlg.FileName;
                        _xamlPath = Path.GetDirectoryName(_fileName);
                        _designerViewModel.ReloadDesigner(_fileName);
                        RiseCommandsCanExecuteChanged();
                    }
                    finally
                    {
                        _notNeedCanCloseEvent = false;
                        WaitStop();
                    }
                }
            }
        }

        protected override bool CanSave()
        {
            return false;
        }

        protected virtual bool CanSaveToFile()
        {
            return !_isReadOnly;// Check(BusinessObjectManager<string, int>.NewMethodName);
        }

        protected virtual void SaveToFile()
        {
            var surface = (IDesignerSurface)_designerViewModel.CurrentSurface;
            surface.Designer.Flush();

            var dlg = new SaveFileDialog
                {
                    FileName = string.IsNullOrEmpty(_uniqueName) ? "Workflow" : _uniqueName,
                    Filter = "Activity files (.xaml)|*.xaml",
                    InitialDirectory = _xamlPath
                };
            if (dlg.ShowDialog().GetValueOrDefault())
            {
                try
                {
                    WaitStart();
                    _fileName = dlg.FileName;
                    _xamlPath = Path.GetDirectoryName(_fileName);
                    using (var fs = new FileStream(_fileName, FileMode.Create, FileAccess.Write))
                    {
                        using (var sw = new StreamWriter(fs))
                        {
                            sw.Write(surface.Designer.Text);
                            sw.Flush();
                        }
                    }
                    RiseCommandsCanExecuteChanged();
                }
                finally
                {
                    WaitStop();
                }
            }
        }

        protected override bool Save()
        {
            bool result;

            try
            {
                var surface = (IDesignerSurface) _designerViewModel.CurrentSurface;
                surface.Designer.Flush();

                var xaml = surface.Designer.Text;
                SetXamlValue(Source, xaml);
                UpdateWorkflowInfoFields(xaml);
                AdditionVersion();

                result = base.Save();
                WaitStart();
                var mgr = IoC.Instance.Resolve<IXamlManager<BPWorkflow>>();
                mgr.SetXaml(((IKeyHandler) Source).GetKey().ToString(), xaml ?? string.Empty);
                var eo = Source as IEditable;
                if (eo != null)
                    eo.AcceptChanges();
            }
            finally
            {
                RiseCommandsCanExecuteChanged();
                WaitStop();
            }

            return result;
        }

        private void UpdateWorkflowInfoFields(string wfContents)
        {
            try
            {
                var entity = Source as WMSBusinessObject;
                if (entity == null)
                    return;

                Func<string, int, string> lengthHandler = (text, length) =>
                {
                    var result = text.Trim();
                    if (result.Length > length)
                        result = result.Substring(0, length - 3) + "...";
                    return result;
                };

                var helper = new WfXamlHelper();
                var activitynames = helper.FindActivityByNameSpace(wfContents, "wmsMLC.Activities");
                string activities = null;
                if (activitynames.Any())
                    activities = lengthHandler(string.Join(",", activitynames.OrderBy(p => p)), MaxLengthWorkflowActivitiesField);
                entity.SetProperty("WORKFLOWACTIVITIES", activities);

                //HACK: Нименование активити, вызывающей БП может измениться
                const string executeWorkflowActivityName = "ExecuteWorkflowActivity";
                string wfinternals = null;
                if (activitynames.Contains(executeWorkflowActivityName))
                {
                    var executeWfs = new List<string>();
                    var executeWfregex = new Regex(string.Format(":{0}(?<Activity>.*?)>", executeWorkflowActivityName),
                        RegexOptions.Singleline);
                    executeWfs.AddRange(from Match m in executeWfregex.Matches(wfContents) select m.Groups[1].Value);

                    if (executeWfs.Any())
                    {
                        var wfs = new List<string>();
                        foreach (var p in executeWfs)
                        {
                            //HACK: Нименование свойства активити, вызывающей БП может измениться
                            var wfregex = new Regex(@"WorkflowCode=""(?<WorkflowCode>.*?)""", RegexOptions.Singleline);
                            wfs.AddRange(from Match m in wfregex.Matches(p) select m.Groups[1].Value);
                        }

                        if (wfs.Any())
                            wfinternals = lengthHandler(string.Join(",", wfs.Distinct().OrderBy(p => p)),
                                MaxLengthWorkflowInternalField);
                    }
                }
                entity.SetProperty("WORKFLOWINTERNAL", wfinternals);
            }
            catch (Exception ex)
            {
                _log.WarnFormat("При поулучении информации из xaml БП для значений полей BPWorkflow.WORKFLOWACTIVITIES и BPWorkflow.WORKFLOWINTERNAL возникла ошибка: {0}", 
                    ExceptionHelper.ExceptionToString(ex));
                _log.Debug(ex);
            }
        }

        private void AdditionVersion()
        {
            var entity = Source as BPWorkflow;
            if (entity == null)
                return;
            var oldVersion = entity.WorkflowVersion;
            if (String.IsNullOrEmpty(oldVersion))
                return;
            var ar = oldVersion.Split('.');
            int newVersion;
            if (!Int32.TryParse(ar[ar.Length - 1], out newVersion)) 
                return;
            ar[ar.Length - 1] = (++newVersion).ToString(CultureInfo.InvariantCulture);
            entity.WorkflowVersion = String.Join(".", ar);
        }

        protected virtual bool CanCompile()
        {
            // не даем делать сборку, оставили на потом
#if DEBUG
            return !string.IsNullOrEmpty(_fileName);
#else
            return false;
#endif
        }

        protected virtual void Compile()
        {
            var surface = (IDesignerSurface)_designerViewModel.CurrentSurface;
            surface.Designer.Flush();

            var dlg = new FolderBrowserDialog
                {
                    Description = StringResources.SelectCompilePath,
                    ShowNewFolderButton = true,
                    SelectedPath = _compiledPath
                };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _compiledPath = dlg.SelectedPath;
                    WaitStart();
                    //_toolBoxModel.UnloadToolboxIcons();
                    CompileInternal(_fileName, dlg.SelectedPath);
                    //_toolBoxModel.ReloadToolboxIcons();
                }
                finally
                {
                    WaitStop();
                }
            }
        }

        private void CompileInternal(string fileName, string destinationPath)
        {
            var assemblyNames = new List<string>
                        {
                            // TODO: это надо задавать в конфигах
                            "Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                            "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                            "System.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35",
                            "System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                            "System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                            "System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                            "System.ServiceModel.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35",
                            "System.ServiceModel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                            "System.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                            "System.Xml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                            "System.Xml.Linq, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
                        };
            var compiler = IoC.Instance.Resolve<IActivityCompiler>();
            compiler.Compile(fileName, destinationPath, assemblyNames.ToArray());
        }

        protected override bool CanCloseInternal()
        {
            var result = false;

            try
            {
                result = base.CanCloseInternal();
                if (!result)
                    return false;

                var eo = Source as EditableBusinessObject;
                result = eo == null;
                if (result)
                    return true;

                result = !eo.IsDirty;
                if (result)
                    return true;

                var vs = GetViewService();
                var dr = vs.ShowDialog(StringResources.Confirmation
                    , StringResources.ConfirmationUnsavedData
                    , MessageBoxButton.YesNoCancel
                    , MessageBoxImage.Question
                    , MessageBoxResult.Yes);

                if (dr == MessageBoxResult.Cancel)
                    return (result = false);

                if (dr == MessageBoxResult.Yes)
                {
                    Save();
                    return (result = true);
                }

                if (dr == MessageBoxResult.No)
                {
                    result = RejectChanges();
                    return result;
                }
            }
            finally
            {
                if (result)
                    OnCanClose();
            }

            throw new DeveloperException(DeveloperExceptionResources.UnknownDialogResult);
        }

        private void OnCanClose()
        {
            if (_notNeedCanCloseEvent)
                return;
            var handler = CanClose;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public string GetUniqueName()
        {
            return _uniqueName;
        }

        private void FullScreenModeChange()
        {
            GetViewService().MakeMaxSize(this, _isMaxView);
            _isMaxView = !_isMaxView;

            _fullScreenModeMenuItem.ImageSmall = _isMaxView ? ImageResources.DCLCollapsePack16.GetBitmapImage() : ImageResources.DCLExpandPack16.GetBitmapImage();
            _fullScreenModeMenuItem.ImageLarge = _isMaxView ? ImageResources.DCLCollapsePack32.GetBitmapImage() : ImageResources.DCLExpandPack32.GetBitmapImage();
            _fullScreenModeMenuItem.Caption = _isMaxView ? StringResources.CollapsePanel : StringResources.ExpandPanel;
        }

        protected override void OnBpProcessEnd(CompleteContext completeContext)
        {
            WaitStop();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //DesignerToolBox.Parent
            }
            base.Dispose(disposing);
        }
    }
}

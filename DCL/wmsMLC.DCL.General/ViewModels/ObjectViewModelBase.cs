using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using wmsMLC.Business.Managers.Expressions;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.Business.Managers.Validation;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.Helpers;
using wmsMLC.DCL.General.ViewModels.Menu;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Helpers;
using wmsMLC.General.BL.Validation;
using wmsMLC.General.PL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Helpers;
using wmsMLC.General.PL.WPF.Services;
using wmsMLC.General.PL.WPF.ViewModels;
using ExpressionHelper = wmsMLC.Business.Managers.Expressions.ExpressionHelper;

namespace wmsMLC.DCL.General.ViewModels
{
    public class ObjectViewModelBase<TModel> : BusinessProcessViewModelBase<TModel, TModel>, IFormulaHandler, IObjectViewModel<TModel>, IHelpHandler
    {
        /// <summary>
        /// Режим работы модели.
        /// </summary>
        public ObjectViewModelMode Mode { get; set; }

        private log4net.ILog _log = log4net.LogManager.GetLogger(typeof(ListViewModelBase<TModel>));

        #region .  Const & Fields  .
        private IBaseManager<TModel> _managerCache;
        private ExpandoObject _formulaValues;
        #endregion .  Const & Fields  .

        #region . SubObject .

        private bool _doNotOnAfterSave;
        private bool _notClearInputData;
        private WMSBusinessObject _oldSource;
        private MenuItemBase _menuSaveAndContinue;

        public event EventHandler<NotifyCollectionChangedEventArgs> CollectionChanged;
        public event EventHandler NeedRefresh;

        public WMSBusinessObject SourceBase { get; set; }
        public ICommand SaveNotCloseCommand { get; private set; }

        private bool _isAdd;
        public bool IsAdd
        {
            get
            {
                return _isAdd;
            }
            set
            {
                _isAdd = value;
                if (Mode == ObjectViewModelMode.MemoryObject)
                {
                    if (Menu == null)
                        return;

                    if (Menu.Bars != null && IsAdd)
                    {
                        foreach (
                            var bar in
                                Menu.Bars.Where(p => p != null && p.Caption.EqIgnoreCase(StringResources.Commands))
                                    .ToArray())
                        {
                            foreach (
                                var menu in
                                    bar.MenuItems.Where(p => p.Caption.EqIgnoreCase(StringResources.Save))
                                        .ToArray())
                            {
                                bar.MenuItems.Remove(menu);
                            }
                        }
                    }
                }
            }
        }

        public bool IsNeedRefresh { get; set; }

        public override bool IsVisibleMenuSaveAndContinue
        {
            get
            {
                return base.IsVisibleMenuSaveAndContinue;
            }
            set
            {
                base.IsVisibleMenuSaveAndContinue = value;
                if (_menuSaveAndContinue != null)
                    _menuSaveAndContinue.IsVisible = value;
            }
        }

        protected override void InitializeSettings()
        {
            if (Mode != ObjectViewModelMode.Object)
                MenuSuffix = "SublistViewDetail";
            base.InitializeSettings();
        }

        private bool CanSaveNotClose()
        {
            return CanSave() && SourceBase != null;
        }

        private void OnSaveNotClose()
        {
            if (!CanSaveNotClose())
                return;

            Save();
        }

        protected override bool CanNotClearInputDataCommand()
        {
            if (Mode != ObjectViewModelMode.Object)
                return SourceBase != null;
            return base.CanNotClearInputDataCommand();
        }

        protected override void OnNotClearInputDataCommand()
        {
            if (Mode != ObjectViewModelMode.Object)
            {
                if (!CanNotClearInputDataCommand())
                    return;

                _notClearInputData = !_notClearInputData;
                UpdateSource();
            }
            else
                base.OnNotClearInputDataCommand();
        }

        private void UpdateSource()
        {
            WMSBusinessObject sourcebase = null;
            if (_notClearInputData)
            {
                if (_oldSource != null)
                    sourcebase = (WMSBusinessObject)_oldSource.Clone();
            }
            else
            {
                if (SourceBase != null)
                {
                    sourcebase = (WMSBusinessObject)SourceBase.Clone();
                    sourcebase.AcceptChanges(true);
                }
            }

            if (sourcebase != null)
            {
                sourcebase.Validate();
                Source = (TModel)((object)sourcebase);
            }
        }

        private void UpdateSource(List<TModel> source)
        {
            if (InPropertyEditMode)
                SetSource(source);
            else
                OnSetSource(source != null && source.Any() ? source.First() : default(TModel));
        }

        protected void UpdateOldSource()
        {
            if (Mode == ObjectViewModelMode.Object)
                return;
            var source = Source as WMSBusinessObject;
            if (source != null && source.IsNew)
                _oldSource = source;
        }

        protected void OnNeedRefresh()
        {
            var handler = NeedRefresh;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void OnCollectionChanged(object item)
        {
            var handler = CollectionChanged;
            if (handler != null)
                handler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        #endregion . SubObject .

        public ObjectViewModelBase()
        {
            AllowClosePanel = true;
            PanelCaption = typeof(TModel).GetDisplayName();
            PanelCaptionImage = GetPanelImage();

            RefreshCommand = new DelegateCustomCommand(RefreshData, CanRefresh);
            SaveCommand = new DelegateCustomCommand(OnSave, CanSave);
            DeleteCommand = new DelegateCustomCommand(OnDelete, CanDelete);
            EditInNewWindowCommand = new DelegateCustomCommand(OpenInNewWindow, CanOpenInNewWindow);
            SaveAndCloseCommand = new DelegateCustomCommand(OnSaveAndClose, CanSaveAndClose);

            FormulaValues = new ExpandoObject();

            //InitializeBaseCommandsMenu();

            Commands.AddRange(new[] { RefreshCommand, SaveCommand, DeleteCommand, EditInNewWindowCommand, SaveAndCloseCommand });
        }

        #region .  Methods  .

        protected virtual ImageSource GetPanelImage()
        {
            return ImageResources.DCLDetail16.GetBitmapImage();
        }

        public ObservableCollection<DataField> GetDataFields(SettingDisplay displaySetting)
        {
            return GetFields(displaySetting);
        }

        protected virtual ObservableCollection<DataField> GetFields(SettingDisplay displaySetting)
        {
            return DataFieldHelper.Instance.GetDataFields(typeof(TModel), displaySetting);
        }

        protected static bool CheckOnCriticalErrors(object obj)
        {
            // будем выставлять дефолтное значение, если объект новый
            var eo = obj as EditableBusinessObject;
            if (eo != null && eo.IsNew)
                DefaultValueSetter.Instance.SetDefaultValues((BusinessObject)obj);

            var valid = obj as IValidatable;
            if (valid == null)
                return true;

            valid.Validate();

            // если критических ошибок нет - можно продолжать
            if (!valid.Validator.HasCriticalError())
                return true;

            var properties = TypeDescriptor.GetProperties(obj);
            var desc = new StringBuilder();
            //desc.Append(valid.Validator.GetErrorDescription());
            foreach (PropertyDescriptor p in properties)
            {
                foreach (var validateError in valid.Validator.GetAllErrors(p.Name))
                {
                    if (!validateError.Value.HasCriticalErrors)
                        continue;

                    // получаем описание
                    desc.AppendLine(properties[validateError.Key].DisplayName + ":");
                    // получаем ошибки
                    foreach (var v in validateError.Value)
                        desc.AppendLine("  - " + v.Description);
                }
            }

            GetViewService().ShowDialog
                (StringResources.Error
                    , string.Format("{0}{1}{2}", StringResources.ErrorSave, Environment.NewLine, desc)
                    , MessageBoxButton.OK
                    , MessageBoxImage.Error
                    , MessageBoxResult.Yes);

            return false;
        }

        /// <summary>
        /// Вызывается перед валидацией.
        /// </summary>
        protected virtual void BeforeValidationFormulaValue(IEnumerable<TModel> items) { }

        protected virtual void OnAfterSave()
        {
            if (Mode == ObjectViewModelMode.Object)
                return;
            if (_doNotOnAfterSave)
                return;

            var source = Source;
            OnCollectionChanged(source);
            UpdateSource();
        }

        protected virtual void MgrUpdate()
        {
            var mgr = GetManager();
            mgr.Update(Source);
        }

        protected virtual void MgrUpdate(IEnumerable<TModel> values)
        {
            var mgr = GetManager();
            mgr.Update(values);
        }

        protected virtual bool Save()
        {
            var startTime = DateTime.Now;

            try
            {
                WaitStart();

                if (Mode == ObjectViewModelMode.MemoryObject)
                {
                    UpdateOldSource();

                    var isNew = Source as IIsNew;
                    if (isNew == null)
                        throw new DeveloperException(DeveloperExceptionResources.SourceMustImplementIIsNew);

                    var clonable = Source as ICloneable;
                    if (clonable == null)
                        throw new DeveloperException("Source must implement IClonable.");

                    if (isNew.IsNew)
                    {
                        var items = ExpressionHelper.Process(clonable, new ExpressionContext(FormulaValues)).Cast<TModel>();
                        if (items.Any(p => !CheckOnCriticalErrors(p)))
                            return false;
                    }
                    else
                    {
                        if (!CheckOnCriticalErrors(Source))
                            return false;
                    }

                    OnAfterSave();

                    if (!IsAdd)
                    {
                        var source = Source as WMSBusinessObject;
                        if (source != null)
                            source.AcceptChanges();
                    }

                    return true;
                }

                if (Mode == ObjectViewModelMode.SubObject)
                {
                    UpdateOldSource();
                    if (!IsNeedRefresh)
                        IsNeedRefresh = true;
                }

                if (!ConnectionManager.Instance.AllowRequest())
                    return false;

                if (Source is ViewModelBase)
                    throw new DeveloperException(DeveloperExceptionResources.CantOperateWithViewmodel);

                if (IsNew())
                {
                    if (InFormulaMode)
                    {
                        var clonable = Source as ICloneable;
                        if (clonable == null)
                            throw new DeveloperException("Source must implement IClonable.");

                        var items =
                            ExpressionHelper.Process(clonable, new ExpressionContext(FormulaValues)).Cast<TModel>();
                        BeforeValidationFormulaValue(items);

                        if (items.Any(i => !CheckOnCriticalErrors(i)))
                            return false;

                        MgrInsert(ref items);
                        var count = items.Count();

                        // выводим сообщени о добавленых записях
                        GetViewService()
                            .ShowDialog(StringResources.Information,
                                string.Format(StringResources.FormulaSaveItemsTemplateMessage, count),
                                MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
                    }
                    else
                    {
                        if (!CheckOnCriticalErrors(Source))
                            return false;

                        var item = Source;
                        MgrInsert(ref item);
                        OnSetSource(item);
                    }
                    //FormulaValues = new ExpandoObject();
                }
                else
                {
                    if (InPropertyEditMode)
                    {
                        var values = GetPropertyEditCollection(Source, PropertyEditSource).ToArray();
                        if (values.AsParallel().Any(item => !CheckOnCriticalErrors(item)))
                        {
                            return false;
                        }

                        //Безусловное выключаем сохранение внутренних сущностей
                        foreach (var v in values.OfType<ICustomXmlSerializable>())
                        {
                            v.OverrideIgnore = true;
                        }
                        MgrUpdate(values);

                        var editable = Source as IEditable;
                        if (editable == null)
                            throw new DeveloperException("Source must implement IEditable.");
                        editable.AcceptChanges();
                    }
                    else
                    {
                        if (!CheckOnCriticalErrors(Source))
                            return false;
                        MgrUpdate();
                    }
                    RefreshData(false);
                }
                OnAfterSave();
                return true;
            }
            catch (Exception ex)
            {
                if (!ExceptionHandler(ex, ExceptionResources.ItemCantSave))
                    throw;
                return false;
            }
            finally
            {
                WaitStop();
                _log.DebugFormat("Save of {0} in {1}", typeof(TModel).Name, DateTime.Now - startTime);
            }
        }

        protected virtual bool SaveAndClose()
        {
            var saved = Save();
            if (saved)
                GetViewService().Close(this);
            return saved;
        }

        protected virtual void MgrInsert(ref IEnumerable<TModel> entities)
        {
            var mgr = GetManager();
            mgr.Insert(ref entities);
        }

        protected virtual void MgrInsert(ref TModel entitie)
        {
            var mgr = GetManager();
            mgr.Insert(ref entitie);
        }

        protected virtual bool Delete()
        {
            try
            {
                WaitStart();

                if (!ConnectionManager.Instance.AllowRequest())
                    return false;

                if (Mode == ObjectViewModelMode.MemoryObject)
                    return false;

                if (Source is ViewModelBase)
                    throw new DeveloperException(DeveloperExceptionResources.CantOperateWithViewmodel);

                var mgr = GetManager();
                mgr.Delete(Source);

                if (CanSave())
                {
                    var editable = Source as IEditable;
                    if (editable != null)
                        editable.AcceptChanges();
                }

                return true;
            }
            catch (Exception ex)
            {
                if (!ExceptionHandler(ex, ExceptionResources.ItemCantDelete))
                    throw;
                return false;
            }
            finally
            {
                WaitStop();
            }
        }

        protected virtual bool CanOpenInNewWindow()
        {
            var obj = Source as WMSBusinessObject;
            if (obj == null)
                return false;
            var key = obj.GetKey();
            return key != null;
        }

        protected virtual void OpenInNewWindow()
        {
            try
            {
                if (!CanOpenInNewWindow())
                    return;

                WaitStart();
                if (!ConnectionManager.Instance.AllowRequest())
                    return;
                var obj = Source as WMSBusinessObject;
                if (obj == null)
                    throw new DeveloperException("Source is not WMSBusinessObject");
                var key = obj.GetKey();
                var mgr = GetManager();
                var objNew = mgr.Get(key);

                var ovm = IoC.Instance.Resolve<IObjectViewModel<TModel>>();
                ovm.SetSource(objNew);
                GetViewService().Show(ovm, new ShowContext { DockingType = DockType.Document, ShowInNewWindow = true });
            }
            catch (Exception ex)
            {
                if (!ExceptionHandler(ex, ExceptionResources.ItemCantCreate))
                    throw;
            }
            finally
            {
                WaitStop();
            }
        }

        /// <summary>
        /// Получение Manager-а для сущности.
        /// ВНИМАНИЕ: не нужно делать using (Dispose-ить менеджера) - он мог быть получен как Singleton
        /// </summary>
        /// <returns></returns>
        protected virtual IBaseManager<TModel> GetManager()
        {
            if (_managerCache != null)
                return _managerCache;

            _managerCache = IoC.Instance.Resolve<IBaseManager<TModel>>();
            return _managerCache;
        }

        public virtual void InitializeMenus()
        {
            InitializeCustomizationBar();
            InitializeContextMenu();
            CreateMainMenu();
            if (Mode == ObjectViewModelMode.Object)
                CreateProcessMenu();
        }

        protected virtual void CreateMainMenu()
        {
            var bar = Menu.GetOrCreateBarItem(StringResources.Commands, 1, "BarItemCommands");
            bar.MenuItems.Add(new CommandMenuItem
                {
                    Caption = StringResources.RefreshData,
                    Command = RefreshCommand,
                    ImageSmall = ImageResources.DCLFilterRefresh16.GetBitmapImage(),
                    ImageLarge = ImageResources.DCLFilterRefresh32.GetBitmapImage(),
                    HotKey = new KeyGesture(Key.F5),
                    //GlyphSize = GlyphSizeType.Large,
                    GlyphAlignment = GlyphAlignmentType.Top,
                    DisplayMode = DisplayModeType.Default,
                    Priority = 10
                });

            bar.MenuItems.Add(new CommandMenuItem
                {
                    Caption = StringResources.Delete,
                    Command = DeleteCommand,
                    ImageSmall = ImageResources.DCLDelete16.GetBitmapImage(),
                    ImageLarge = ImageResources.DCLDelete32.GetBitmapImage(),
                    HotKey = new KeyGesture(Key.F9),
                    //GlyphSize = GlyphSizeType.Large,
                    GlyphAlignment = GlyphAlignmentType.Top,
                    DisplayMode = DisplayModeType.Default,
                    Priority = 20
                });

            bar.MenuItems.Add(new CommandMenuItem
                {
                    Caption = StringResources.Save,
                    Command = SaveCommand,
                    HotKey = new KeyGesture(Key.F6),
                    ImageSmall = ImageResources.DCLSave16.GetBitmapImage(),
                    ImageLarge = ImageResources.DCLSave32.GetBitmapImage(),
                    //GlyphSize = GlyphSizeType.Large,
                    GlyphAlignment = GlyphAlignmentType.Top,
                    DisplayMode = DisplayModeType.Default,
                    Priority = 12
                });

            bar.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.SaveAndClose,
                Command = SaveAndCloseCommand,
                HotKey = new KeyGesture(Key.F7),
                ImageSmall = ImageResources.DCLSaveAndClose16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLSaveAndClose32.GetBitmapImage(),
                //GlyphSize = GlyphSizeType.Large,
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 13
            });

            bar.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.EditInNewWindow,
                Command = EditInNewWindowCommand,
                //HotKey = Key.F11,
                ImageSmall = ImageResources.DCLEditInNewWindow16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLEditInNewWindow32.GetBitmapImage(),
                //GlyphSize = GlyphSizeType.Large,
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 14
            });

            if (Mode != ObjectViewModelMode.Object)
            {
                SaveNotCloseCommand = new DelegateCustomCommand(OnSaveNotClose, CanSaveNotClose);

                if (Menu == null)
                    return;

                if (Menu.Bars != null)
                {
                    foreach (var b in Menu.Bars.Where(p => p != null && p.Caption.EqIgnoreCase(StringResources.Commands)).ToArray())
                    {
                        foreach (var menu in b.MenuItems.Where(p => p != null && !p.Caption.EqIgnoreCase(StringResources.Save)).ToArray())
                        {
                            b.MenuItems.Remove(menu);
                        }
                    }

                    foreach (var b in Menu.Bars.Where(p => p != null && p.Caption.EqIgnoreCase(StringResources.BusinessProcesses)).ToArray())
                    {
                        if (Menu.Bars.Contains(b))
                            Menu.Bars.Remove(b);
                    }
                }

                var barcommand = Menu.GetOrCreateBarItem(StringResources.Commands, 1);

                //HACK: пока у SubListView не появится своя viewmodel (wmsMLC-3910)
                if (!(Source is Working))
                {
                    _menuSaveAndContinue = new CommandMenuItem
                    {
                        Caption = StringResources.SaveAndContinue,
                        Command = SaveNotCloseCommand,
                        HotKey = new KeyGesture(Key.F7),
                        ImageSmall = ImageResources.DCLSaveAndContinue16.GetBitmapImage(),
                        ImageLarge = ImageResources.DCLSaveAndContinue32.GetBitmapImage(),
                        GlyphAlignment = GlyphAlignmentType.Top,
                        DisplayMode = DisplayModeType.Default,
                        Priority = 12
                    };
                    barcommand.MenuItems.Add(_menuSaveAndContinue);
                    Commands.Add(SaveNotCloseCommand);
                }

            }
        }

        /// <summary>
        /// Признак того, что объект только-что создан (еще не было схранения)
        /// </summary>
        private bool IsNew()
        {
            var obj = Source as IIsNew;
            return obj != null && obj.IsNew;
        }

        protected override object[] GetItemsParameter()
        {
            return new object[] { Source };
        }

        #region .  Commands  .
        public ICommand RefreshCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICustomCommand SaveCommand { get; private set; }
        public ICustomCommand SaveAndCloseCommand { get; private set; }
        public ICommand EditInNewWindowCommand { get; set; }
        public virtual ICommand DoActionCommand { get { return SaveCommand; } }

        protected virtual bool CanDelete()
        {
            return Source != null && !IsNew() && IsDelEnable;
        }
        protected virtual bool CanSave()
        {
            if (Source == null)
                return false;

            var ed = Source as IEditable;
            if (ed != null && !ed.IsDirty)
                return false;

            if (!IsNew())
                return IsEditEnable;

            return IsNewEnable;
        }

        protected virtual bool CanSaveAndClose()
        {
            return CanSave();
        }

        protected override bool CanCloseInternal()
        {
            var res = base.CanCloseInternal();
            if (!res)
                return false;

            var eo = Source as EditableBusinessObject;
            if (eo == null)
                return true;

            if (!eo.IsDirty)
                return true;

            try
            {
                WaitStart();

                var vs = GetViewService();
                var dr = vs.ShowDialog(StringResources.Confirmation
                    , StringResources.ConfirmationUnsavedData
                    , MessageBoxButton.YesNoCancel
                    , MessageBoxImage.Question
                    , MessageBoxResult.Yes);

                switch (dr)
                {
                    case MessageBoxResult.Cancel:
                        return false;
                    case MessageBoxResult.Yes:
                        return Save();
                    case MessageBoxResult.No:
                        return RejectChanges();
                    default:
                        throw new DeveloperException(DeveloperExceptionResources.UnknownDialogResult);
                }
            }
            finally
            {
                WaitStop();
            }
        }
        protected override bool CanPrintReport()
        {
            var eo = Source as EditableBusinessObject;
            if (eo == null)
                return true;
            return !eo.IsDirty;
        }

        protected virtual bool CanRefresh()
        {
            return !IsNew() && IsReadEnable;
        }

        public override void RefreshData()
        {
            if (!ConnectionManager.Instance.AllowRequest())
                return;
            RefreshData(true);
        }

        protected virtual void OnDelete()
        {
            if (!CanDelete())
                throw new DeveloperException(DeveloperExceptionResources.CommandCanEditError);

            if (!DeleteConfirmation())
                return;

            if (Delete())
                DoCloseRequest();
        }

        protected virtual void OnSave()
        {
            if (Mode == ObjectViewModelMode.Object)
            {
                if (!CanSave())
                    throw new DeveloperException(DeveloperExceptionResources.CommandCanEditError);

                Save();
            }
            else
            {
                if (!CanSave())
                    return;

                try
                {
                    if (Mode == ObjectViewModelMode.SubObject)
                        _doNotOnAfterSave = true;
                    if (Save())
                    {
                        OnNeedRefresh();
                    }
                }
                finally
                {
                    _doNotOnAfterSave = false;
                }
            }
        }

        protected virtual void OnSaveAndClose()
        {
            if (!CanSaveAndClose())
                throw new DeveloperException(DeveloperExceptionResources.CommandCanEditError);

            SaveAndClose();
        }

        public virtual bool DoAction()
        {
            if (Mode != ObjectViewModelMode.Object)
                return true;
            return CanSave() && Save();
        }

        protected override void OnIsDirtyChanged(IEditable eo)
        {
            base.OnIsDirtyChanged(eo);

            SaveCommand.RaiseCanExecuteChanged();
            SaveAndCloseCommand.RaiseCanExecuteChanged();

            if (Mode != ObjectViewModelMode.Object)
                RiseCommandsCanExecuteChanged();
        }

        //Обновляемся после окончания БП
        protected override void OnBpProcessEnd(CompleteContext context)
        {
            try
            {
                if (Application.Current == null)
                    return;

                DispatcherHelper.Invoke(new Action(() => RefreshData(false)));
            }
            catch (Exception ex)
            {
                if (Application.Current != null)
                    DispatcherHelper.Invoke(new Action(() => ExceptionHandler(ex, ExceptionResources.ItemCantRefresh)));
            }
            finally
            {
                WaitStop();
            }
        }

        protected virtual void OnAfterRefresh(bool usewait) { }

        protected virtual void RefreshData(bool usewait)
        {
            if (!CanRefresh())
                return;

            var editable = Source as IEditable;
            if (editable == null)
                throw new DeveloperException("Source is not IEditable.");

            if (editable.IsDirty && GetViewService().ShowDialog(StringResources.Confirmation
                    , StringResources.ConfirmationSaveDataOnRefresh
                    , MessageBoxButton.YesNo
                    , MessageBoxImage.Question
                    , MessageBoxResult.No) != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                if (usewait)
                    WaitStart();

                RejectChanges();
                var data = GetSource(GetManager());
                UpdateSource(data);
                OnAfterRefresh(usewait);
            }
            catch (Exception ex)
            {
                if (!ExceptionHandler(ex, ExceptionResources.ItemCantRefresh))
                    throw;
            }
            finally
            {
                if (usewait)
                    WaitStop();
            }
        }

        private List<TModel> GetSource(IBaseManager<TModel> mgr)
        {
            // очищаем кэш
            mgr.ClearCache();

            if (InPropertyEditMode)
            {
                var values = new List<TModel>();
                var filterList = FilterHelper.GetArrayFilterIn(typeof(TModel), (IEnumerable<object>)PropertyEditSource);
                foreach (var filter in filterList)
                    values.AddRange(mgr.GetFiltered(filter).ToArray());
                if (values.Count() != PropertyEditSource.Count())
                {
                    var diff =
                        PropertyEditSource.Where(
                            p =>
                                values.Cast<IKeyHandler>()
                                    .FirstOrDefault(i => i.GetKey().Equals(((IKeyHandler)p).GetKey())) == null)
                            .ToList();
                    if (diff.Any())
                        throw new OperationException("Объекты были удалены: {0}",
                            string.Join(",", diff.Cast<IKeyHandler>().Select(i => i.GetKey())));
                }
                return values;
            }

            // получаем данные
            var keyHandler = Source as IKeyHandler;
            if (keyHandler == null)
                throw new DeveloperException("Source is not IKeyHandler.");
                
            var result = mgr.Get(keyHandler.GetKey());
            return new List<TModel>(new[] {result});
            // обновляем все связанные списки
            //mgr.RiseManagerChanged();
        }

        public override async void RefreshDataAsync()
        {
            try
            {
                WaitStart();
                var isnew = IsNew();
                var data = await GetSourceAsync(isnew);
                if (!isnew)
                    UpdateSource(data);
            }
            catch (Exception ex)
            {
                if (!ExceptionHandler(ex, ExceptionResources.ItemCantRefresh))
                    throw;
            }
            finally
            {
                WaitStop();
            }
        }

        private async Task<List<TModel>> GetSourceAsync(bool isnew)
        {
            var mgr = isnew ? null : GetManager();
            return await Task.Factory.StartNew(() =>
            {
                System.Threading.Thread.Sleep(200);
                var result = isnew ? null : GetSource(mgr);
                System.Threading.Thread.Sleep(200);
                return result;
            });
        }

        protected virtual bool DeleteConfirmation()
        {
            var vs = GetViewService();
            var dr = vs.ShowDialog(StringResources.Confirmation
                , StringResources.ConfirmationDeleteObject
                , MessageBoxButton.YesNo //MessageBoxButton.YesNoCancel
                , MessageBoxImage.Question
                , MessageBoxResult.Yes);

            return dr == MessageBoxResult.Yes;
        }

        #endregion

        #endregion

        public bool InFormulaMode { get; set; }

        public ExpandoObject FormulaValues
        {
            get { return _formulaValues; }
            private set
            {
                _formulaValues = value;
                OnPropertyChanged("FormulaValues");
            }
        }

        protected override void OnSourceChanged()
        {
            base.OnSourceChanged();
            PanelCaption = (Source == null ? "NULL" : Source.ToString());
        }

        public event EventHandler<FormulaStateEventArgument> FormulaStateChanged;

        protected void OnFormulaStateChanged(string propertyName)
        {
            if (FormulaStateChanged != null)
            {
                FormulaStateChanged(this, new FormulaStateEventArgument(propertyName));
            }
        }

        protected override TModel[] GetPrintedItems()
        {
            return new[] { Source };
        }

        protected override void Dispose(bool disposing)
        {
            _managerCache = null;
            base.Dispose(disposing);
        }

        #region . IHelpHandler .
        string IHelpHandler.GetHelpLink()
        {
            return null;
        }

        string IHelpHandler.GetHelpEntity()
        {
            return typeof(TModel).Name;
        }
        #endregion
    }

    public interface IFormulaHandler
    {
        ExpandoObject FormulaValues { get; }
        bool InFormulaMode { get; set; }
        event EventHandler<FormulaStateEventArgument> FormulaStateChanged;
    }

    public class FormulaStateEventArgument : EventArgs
    {
        public string PropertyName { get; private set; }

        public FormulaStateEventArgument(string propertyName)
        {
            PropertyName = propertyName;
        }
    }

    public interface IPropertyEditHandler
    {
        bool InPropertyEditMode { get; set; }
        void SetSource(IEnumerable source);
        bool IsMergedPropery(string propertyName);
    }

    public interface ILoadImageHandler
    {
        void LoadImage();
        bool ValidateFileSize(long imagesize);
    }
}
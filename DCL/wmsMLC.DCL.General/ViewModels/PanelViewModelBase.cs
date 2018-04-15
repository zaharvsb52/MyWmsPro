using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Caliburn.Micro;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.Helpers;
using wmsMLC.DCL.General.ViewModels.Menu;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Events;
using wmsMLC.General.PL.WPF.Services;
using wmsMLC.General.PL.WPF.ViewModels;

namespace wmsMLC.DCL.General.ViewModels
{
    public abstract class PanelViewModelBase : ViewModelBase, IClosable, IPanelViewModel, IActivatable, IMenuHandler
    {
        #region .  Fields & Consts  .
        public const string CustomizationRightName = "CGUI";
        public const string CustomizationDBRightName = "GUI2DB";
        public const string ChangeRowCountRightName = "ChangeRowCount";

        public const string DownloadXmlRightName = "F2DB";
        public const string UnloadXmlRightName = "DB2F";

        public const string BlockMethodName = "Entity2Block";
        public const string AddBlockMethodName = "AddBlock";

        public const string MenuPropertyName = "Menu";
        public const string ContextMenuPropertyName = "ContextMenu";
        public const string IsVisibleMenuSaveAndContinuePropertyName = "IsVisibleMenuSaveAndContinue";

        public const string IsCustomizationPropertyName = "IsCustomization";

        private static volatile Lazy<ISecurityChecker> _securityChecker =
            new Lazy<ISecurityChecker>(() => wmsMLC.General.IoC.Instance.Resolve<ISecurityChecker>());
        private readonly ConcurrentDictionary<string, bool> _checkCache = new ConcurrentDictionary<string, bool>();

        private bool _isVisibleMenuSaveAndContinue;
        private string _panelCaption;
        private ImageSource _panelCaptionImage;
        private bool _allowClosePanel;
        private bool _waitIndicatorVisible;
        private volatile int _waitCallCount;
        private MenuViewModel _menu;
        private MenuItemCollection _contextMenu;

        private bool _isCustomization;
        private bool _isInSetSettings;
        private bool _isCustomizeBarEnabled;
        private BarItem _сustomizationBar;

        #endregion

        #region .  Ctor  .
        protected PanelViewModelBase()
        {
            Commands = new List<ICommand>();

            try
            {
                _isInSetSettings = true;
                InitializeSettings();
            }
            finally
            {
                _isInSetSettings = false;
            }
            InitializeRights();

            //if (IsMenuEnable)
            //{
            //    if (IsCustomizeBarEnabled)
            //        InitializeCustomizationBar();
            //}

            //if (IsContextMenuEnable)
            //    ContextMenu = new MenuItemCollection();

            IEventAggregator ea;
            if (wmsMLC.General.IoC.Instance.TryResolve(out ea))
            {
                EventAggregator = ea;
                EventAggregator.Subscribe(this);
            }

            NotClearInputDataCommand = new DelegateCustomCommand(OnNotClearInputDataCommand, CanNotClearInputDataCommand);
        }
        #endregion

        #region .  Properties  .

        protected bool IsCustomizeEnabled { get; private set; }
        protected bool IsCustomizeDBEnabled { get; private set; }
        protected bool IsDownloadXmlEnabled { get; private set; }
        protected bool IsUnloadXmlEnabled { get; private set; }
        protected bool IsChangeRowCount { get; private set; }

        public string PanelCaption
        {
            get { return _panelCaption; }
            set
            {
                if (_panelCaption == value)
                    return;

                _panelCaption = value;
                OnPropertyChanged("PanelCaption");
            }
        }

        public bool IsActive { get; set; }

        public ImageSource PanelCaptionImage
        {
            get { return _panelCaptionImage; }
            set
            {
                if (Equals(_panelCaptionImage, value))
                    return;

                _panelCaptionImage = value;
                OnPropertyChanged("PanelCaptionImage");
            }
        }

        public bool WaitIndicatorVisible
        {
            get { return _waitIndicatorVisible; }
            set
            {
                if (_waitIndicatorVisible == value)
                    return;

                _waitIndicatorVisible = value;
                OnPropertyChanged("WaitIndicatorVisible");
            }
        }

        public bool AllowClosePanel
        {
            get { return _allowClosePanel; }
            set
            {
                if (_allowClosePanel == value)
                    return;

                _allowClosePanel = value;
                OnPropertyChanged("AllowClosePanel");
            }
        }

        public virtual bool IsVisibleMenuSaveAndContinue
        {
            get
            {
                return _isVisibleMenuSaveAndContinue;
            }
            set
            {
                if (_isVisibleMenuSaveAndContinue == value)
                    return;
                _isVisibleMenuSaveAndContinue = value;
                OnPropertyChanged(IsVisibleMenuSaveAndContinuePropertyName);
            }
        }

        public ICommand CustomizationCommand { get; private set; }
        public ICommand SaveLayoutCommand { get; private set; }
        public ICommand SaveDBLayoutCommand { get; private set; }
        public ICommand SaveDBLayoutUpVersionCommand { get; private set; }
        public ICommand ClearLayoutCommand { get; private set; }

        /// <summary>
        /// Gets or sets whether customization mode of view is active. 
        /// </summary>
        public bool IsCustomization
        {
            get { return _isCustomization; }
            set
            {
                if (_isCustomization == value)
                    return;

                _isCustomization = value;
                OnPropertyChanged(IsCustomizationPropertyName);
            }
        }

        protected bool IsMenuEnable { get; set; }
        protected bool IsContextMenuEnable { get; set; }

        public bool IsCustomizeBarEnabled
        {
            get { return _isCustomizeBarEnabled; }
            set
            {
                if (_isCustomizeBarEnabled == value)
                    return;

                _isCustomizeBarEnabled = value;
                OnPropertyChanged("IsCustomizeBarEnabled");
                OnIsCustomizeBarEnabledChanged();
            }
        }

        private void OnIsCustomizeBarEnabledChanged()
        {
            if (_isInSetSettings)
                return;

            // если уже создавали, то просто управляем видимостью
            if (_сustomizationBar != null)
            {
                _сustomizationBar.IsVisible = IsCustomizeBarEnabled;
                return;
            }

            // если новая и нужно ее разрешить
            if (IsCustomizeBarEnabled)
                InitializeCustomizationBar();
        }

        public MenuViewModel Menu
        {
            get
            {
                return (_menu ?? (_menu = CreateMenuViewModel()));
            }
            //protected set
            set
            {
                _menu = value;
                OnPropertyChanged(MenuPropertyName);
            }
        }

        protected virtual MenuViewModel CreateMenuViewModel()
        {
            return new MenuViewModel(MenuSuffix);
        }

        public MenuItemCollection ContextMenu
        {
            get { return _contextMenu; }
            protected set
            {
                _contextMenu = value;
                OnPropertyChanged(ContextMenuPropertyName);
            }
        }

        protected string MenuSuffix { get; set; }

        protected List<ICommand> Commands { get; private set; }

        //TODO: вынести из базового класса - перенести ближе к использованию
        public ICommand NotClearInputDataCommand { get; private set; }

        protected IEventAggregator EventAggregator { get; private set; }
        #endregion .  Properties  .

        #region .  Methods  .
        protected virtual void InitializeSettings() { }

        protected void InitializeCustomizationBar()
        {
            //if (!IsMenuEnable)
            //    IsMenuEnable = true;

            //if (!IsCustomizeBarEnabled)
            //    IsCustomizeBarEnabled = true;

            if (!IsMenuEnable)
                return;

            if (!IsCustomizeBarEnabled)
                return;

            InitializeCustomizationBarCommands();

            _сustomizationBar = Menu.GetOrCreateBarItem(StringResources.SettingsInMenu, 10000, StandardBars.BarItemSettingsInMenu.ToString());
            var listmenu = new ListMenuItem
            {
                Name = "ListMenuSettingsInMenu",
                Caption = StringResources.SettingsInMenu,
                ImageSmall = ImageResources.DCLSettings16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLSettings32.GetBitmapImage(),
                GlyphAlignment = GlyphAlignmentType.Top,
            };

            listmenu.MenuItems.Add(new CheckCommandMenuItem
            {
                IsChecked = IsCustomization,
                Caption = StringResources.CustomizeRegion,
                Command = CustomizationCommand,
                ImageSmall = ImageResources.DCLCustomLayer16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLCustomLayer32.GetBitmapImage(),
                GlyphAlignment = GlyphAlignmentType.Top,
                Priority = 10
            });

            listmenu.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.CustomizationRegionSave,
                Command = SaveLayoutCommand,
                ImageSmall = ImageResources.DCLViewSave16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLViewSave32.GetBitmapImage(),
                GlyphAlignment = GlyphAlignmentType.Top,
                Priority = 50
            });

            listmenu.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.CustomizationRegionSaveDB,
                Command = SaveDBLayoutCommand,
                ImageSmall = ImageResources.DCLViewSaveDB16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLViewSaveDB32.GetBitmapImage(),
                GlyphAlignment = GlyphAlignmentType.Top,
                Priority = 100
            });

            listmenu.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.CustomizationRegionSaveDBUpCriticalVersion,
                Command = SaveDBLayoutUpVersionCommand,
                ImageSmall = ImageResources.DCLViewSaveDB16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLViewSaveDB32.GetBitmapImage(),
                GlyphAlignment = GlyphAlignmentType.Top,
                Priority = 101
            });

            listmenu.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.CustomizationRegionClear,
                Command = ClearLayoutCommand,
                ImageSmall = ImageResources.DCLViewClear16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLViewClear32.GetBitmapImage(),
                GlyphAlignment = GlyphAlignmentType.Top,
                Priority = 1000
            });

            SetupCustomizeMenu(_сustomizationBar, listmenu);

            _сustomizationBar.MenuItems.Add(listmenu);
        }

        protected virtual void InitializeContextMenu()
        {
            if (!IsContextMenuEnable)
                return;
            ContextMenu = new MenuItemCollection();
        }

        protected virtual void SetupCustomizeMenu(BarItem bar, ListMenuItem listmenu) { }

        private void InitializeCustomizationBarCommands()
        {
            CustomizationCommand = new DelegateCustomCommand(Customization, CanCustomization);
            SaveLayoutCommand = new DelegateCustomCommand(SaveLayout, CanSaveLayout);
            SaveDBLayoutCommand = new DelegateCustomCommand(SaveDBLayout, CanSaveDBLayout);
            SaveDBLayoutUpVersionCommand = new DelegateCustomCommand(SaveDBLayoutUpVersion, CanSaveDBUpVersionLayout);
            ClearLayoutCommand = new DelegateCustomCommand(OnClearLayout, OnCanClearLayout);

            Commands.AddRange(new[] { CustomizationCommand, SaveDBLayoutCommand, SaveDBLayoutUpVersionCommand, ClearLayoutCommand, SaveLayoutCommand });
        }

        protected virtual void RiseCommandsCanExecuteChanged()
        {
            if (Commands == null)
                return;
            foreach (var command in Commands)
            {
                var dc = command as ICustomCommand;
                if (dc != null)
                    dc.RaiseCanExecuteChanged();
            }
        }

        protected virtual bool CanCustomization()
        {
            return IsCustomizeEnabled;
        }

        protected virtual bool CanSaveDBLayout()
        {
            return IsCustomizeDBEnabled;
        }

        protected virtual bool CanSaveDBUpVersionLayout()
        {
            return IsCustomizeDBEnabled;
        }
        protected virtual bool CanSaveLayout()
        {
            return IsCustomizeEnabled;
        }

        protected virtual void Customization()
        {
            if (!CanCustomization())
                throw new DeveloperException(DeveloperExceptionResources.CommandCanEditError);
            IsCustomization = !IsCustomization;
        }

        protected virtual bool OnCanClearLayout()
        {
            return IsCustomizeEnabled;
        }

        protected virtual void OnClearLayout()
        {
            var vs = GetViewService();
            var title = StringResources.CustomizationRegionClear;
            var result = vs.ClearLayout(this, title);
            var mes = result.HasValue
                ? (result.Value ? StringResources.CustomizationSuccessClear : StringResources.ErrorClear)
                : StringResources.UserCanceled;
            vs.ShowDialog(title,
                mes,
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                MessageBoxResult.Yes);
        }

        protected virtual void SaveLayout()
        {
            var title = StringResources.CustomizationRegionSave;
            var vs = GetViewService();
            var result = vs.SaveLayout(this, title);
            var mes = result.HasValue
                ? (result.Value ? StringResources.CustomizationSuccessSave : StringResources.ErrorSave)
                : StringResources.UserCanceled;
            vs.ShowDialog(title,
                mes,
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                MessageBoxResult.Yes);
        }

        protected virtual void SaveDBLayout()
        {
            if (!ConnectionManager.Instance.AllowRequest())
                return;
            var title = StringResources.CustomizationRegionSaveDB;
            var vs = GetViewService();
            var result = vs.SaveDBLayout(this, title);
            var mes = result.HasValue
                ? (result.Value ? StringResources.CustomizationSuccessSaveDB : StringResources.ErrorSave)
                : StringResources.UserCanceled;
            vs.ShowDialog(title,
                mes,
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                MessageBoxResult.Yes);
        }

        protected virtual void SaveDBLayoutUpVersion()
        {
            if (!ConnectionManager.Instance.AllowRequest())
                return;
            var title = StringResources.CustomizationRegionSaveDB;
            var vs = GetViewService();
            var result = vs.SaveDBLayout(this, title, true);
            var mes = result.HasValue
                ? (result.Value ? StringResources.CustomizationSuccessSaveDB : StringResources.ErrorSave)
                : StringResources.UserCanceled;
            vs.ShowDialog(title,
                mes,
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                MessageBoxResult.Yes);
        }

        private void InitializeRights()
        {
            IsCustomizeEnabled = Check(CustomizationRightName);
            IsCustomizeDBEnabled = Check(CustomizationDBRightName);
            IsDownloadXmlEnabled = Check(DownloadXmlRightName);
            IsUnloadXmlEnabled = Check(UnloadXmlRightName);
            IsChangeRowCount = Check(ChangeRowCountRightName);
        }

        public void SetPanelCaptionPrefix(Type type)
        {
            if (!typeof(WMSBusinessObject).IsAssignableFrom(type)) return;
            var attributes = TypeDescriptor.GetAttributes(type);
            var att = attributes[typeof(ListViewCaptionAttribute)] as ListViewCaptionAttribute;
            PanelCaption = att == null
                               ? PanelCaption
                               : att.Caption + " - " + PanelCaption;
        }

        protected virtual bool ExceptionHandler(Exception ex, string message)
        {
            if (ex == null)
                return true;

            var result = new OperationException(message, ex);
            return ExceptionPolicy.Instance.HandleException(result, "PL");
        }

        protected static IViewService GetViewService()
        {
            return wmsMLC.General.IoC.Instance.Resolve<IViewService>();
        }

        protected void WaitStart(bool doEvents = true)
        {
            if (EventAggregator != null)
                EventAggregator.Publish(new WaitEvent(this, WaitEventType.Start));

            _waitCallCount++;
            if (_waitCallCount == 1)
            {
                WaitIndicatorVisible = true;
                if (doEvents)
                    DoEvents();
            }
        }

        protected void WaitStop(bool doEvents = true)
        {
            if (EventAggregator != null)
                EventAggregator.Publish(new WaitEvent(this, WaitEventType.Stop));

            _waitCallCount--;
            if (_waitCallCount == 0)
            {
                WaitIndicatorVisible = false;
                if (doEvents)
                    DoEvents();
            }
            else if (_waitCallCount < 0)
                throw new DeveloperException("Ошибка работы со счетчиками wait запросов. Кол-во start меньше кол-ва stop");
        }

        /// <summary>
        /// Данный метод позволяет "протолкнуть" исполнение комманды
        /// <remarks>
        /// Пример: Вы взводите флаг необходимости отобразить диалог ожидания, и следующим действием выполняете длительную команду.
        /// В результате, в большинстве случаев диалог так и не появтися, т.к. поток будет занят выполенением следующей комманды.
        /// В этом случаем контролу нужно помочь.
        /// </remarks>
        /// </summary>
        public void DoEvents()
        {
            //копать тутщ
            //Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(delegate { }));
        }

        /// <summary>
        /// Запрос на закрытие соответсвующего view.
        /// </summary>
        protected void DoCloseRequest()
        {
            EventAggregator.Publish(new CloseRequestEvent(this));
        }

        protected override void Dispose(bool disposing)
        {
            if (EventAggregator != null)
                EventAggregator.Unsubscribe(this);

            if (_menu != null)
            {
                _menu.Dispose();
                _menu = null;
            }

            _contextMenu = null;

            base.Dispose(disposing);
        }

        bool IClosable.CanClose()
        {
            return CanCloseInternal();
        }

        protected virtual bool CanCloseInternal()
        {
            return true;
        }

        protected virtual bool CanNotClearInputDataCommand()
        {
            return false;
        }
        protected virtual void OnNotClearInputDataCommand() { }

        /// <summary>
        /// Проверка наличия прав на действие actionName
        /// </summary>
        /// <param name="actionName">Имя действия</param>
        /// <returns>Истина - если право есть</returns>
        protected virtual bool Check(string actionName)
        {
            // на время жизни формы можно разрешить запомнить состояние прав. Это сильно ускоряет работу по обновлению кнопок
            return _checkCache.GetOrAdd(actionName, s =>
            {
                if (_securityChecker.Value == null)
                    return false;

                return _securityChecker.Value.Check(GetFullRightName(actionName));
            });
        }

        protected virtual string GetFullRightName(string shortName)
        {
            if (IsGlobalRightName(shortName))
                return shortName;

            var secType = GetSecurityType();
            if (secType == null)
                return string.Empty;

            var typeName = secType.Name == "GlobalConfig" ? "GC" : secType.Name;
            return string.Format("{0}{1}", typeName + ".", shortName);
        }

        /// <summary>
        /// Опредялет является ли данное право общим для всех сущностей
        /// </summary>
        /// <param name="shortName">Имя права</param>
        protected virtual bool IsGlobalRightName(string shortName)
        {
            return shortName == CustomizationDBRightName || shortName == CustomizationRightName || shortName == ChangeRowCountRightName;
        }

        /// <summary>
        /// Возвращает тип сущности для получения права.
        /// Если TSourceType в наследуемом классе отличается то TModel, то надо override этот метод.
        /// </summary>
        /// <returns>Тип сущности</returns>
        protected virtual Type GetSecurityType()
        {
            return null;
        }

        #endregion
    }

    public enum StandardBars
    {
        BarItemSettingsInMenu
    }
}
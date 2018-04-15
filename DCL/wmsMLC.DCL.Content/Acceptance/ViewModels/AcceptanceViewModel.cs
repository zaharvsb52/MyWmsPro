using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using MLC.Ext.Common.Model;
using MLC.Ext.Common.Model.Domains;
using MLC.Ext.Wpf.Helpers;
using MLC.Ext.Wpf.ViewModels;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.Business.Managers.Validation;
using wmsMLC.Business.Objects;
using wmsMLC.Business.Objects.Processes;
using wmsMLC.DCL.Content.Acceptance.Views;
using wmsMLC.DCL.General.Helpers;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.General.ViewModels.Menu;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Helpers;
using wmsMLC.General.PL.WPF.ViewModels;
using wmsMLC.General.PL.WPF.Views;
using WebClient.Common.Client.Protocol.DataTransferObjects.Query;
using WebClient.Common.Types;

namespace wmsMLC.DCL.Content.Acceptance.ViewModels
{
    [View(typeof (AcceptanceView))]
    public class AcceptanceViewModel : CustomObjectListViewModelBase<AcceptanceItemInfo>
    {
        //private const string SQL_SELECT_WORK = "operationcode_r = '{1}' and workid in (select w2e.workid_r from wmswork2entity w2e where w2e.work2entityentity = 'CARGOIWB' " +
        //                        "and w2e.work2entitykey in (select to_char(min(i2c.cargoiwbid_r)) from wmsiwb2cargo i2c " +
        //                        "left join wmsiwbpos ip on i2c.iwbid_r = ip.iwbid_r " +
        //                        "where ip.iwbposid ={0}))";
        private const string SQL_SELECT_WORK =
            "operationcode_r = '{1}' and workid in (select w2e.workid_r from wmswork2entity w2e where w2e.work2entityentity = 'CARGOIWB' and w2e.work2entitykey = {0})";

        #region .  Fields  .

        private string _placeFilter;
        private string _operationCode;
        private string _timeText;
        private readonly Timer _timer;
        private DateTime _startTime;
        private bool _isEnabled;

        private CommandMenuItem _endCommandMenuItem;
        private CommandMenuItem _beginCommandMenuItem;
        private CommandMenuItem _manageWorkCommandMenuItem;
        private CommandMenuItem _cloneCommandMenuItem;
        private ListMenuItem _cloneListMenuItem;
        private bool _isProductsShown;
        private EntityReference _acceptancePlace;
        private DeferredAction _updateProductAction;

        #endregion .  Fields  .

        #region . Properties  .

        /// <summary> Признак необходимости печати этикеток на принимаемые ТЕ </summary>
        public bool PrintTE { get; set; }

        /// <summary> Мандант, с которым в данный момент осуществляется работа </summary>
        public decimal? MandantId { get; set; }

        /// <summary> Текущая накладная </summary>
        public IWB CurrentIWB { get; set; }

        public decimal? CurrentWorkId { get; set; }

        /// <summary> Признак работы в режиме миграции. Будут отключены некоторые проверки </summary>
        public bool IsMigration { get; set; }

        ///// <summary> Текущее место приемки </summary>
        //public Place SelectedPlace
        //{
        //    get { return _selectedPlace; }
        //    set
        //    {
        //        if (_selectedPlace == value)
        //            return;

        //        _selectedPlace = value;
        //        OnPropertyChanged("SelectedPlace");
        //    }
        //}

        public string PlaceLookupCode { get; private set; }

        /// <summary>
        /// Фильтр, ограничивающий места приемки
        /// </summary>
        public string PlaceFilter
        {
            get { return _placeFilter; }
            set
            {
                _placeFilter = value;
                OnPropertyChanged("PlaceFilter");
            }
        }

        /// <summary>
        /// Операция с которой будет осуществляться приемка
        /// </summary>
        public string OperationCode
        {
            get { return _operationCode; }
            set
            {
                _operationCode = value;
                OnPropertyChanged("OperationCode");
            }
        }

        public string TimeText
        {
            get { return _timeText; }
            set
            {
                _timeText = value;
                OnPropertyChanged("TimeText");
            }
        }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (IsEnabled == value)
                    return;
                _isEnabled = value;

                _beginCommandMenuItem.IsVisible = !IsEnabled;
                _endCommandMenuItem.IsVisible = IsEnabled;

                OnPropertyChanged("IsEnabled");
            }
        }

        public bool IsAllowAccept { get; set; }

        public string KeyFieldName { get; private set; }

        public string ParentFieldName { get; private set; }

        /// <summary> Код workflow для расшифровки batch-кодов. </summary>
        public string BatchcodeWorkflowCode { get; set; }

        /// <summary>
        /// Код workflow для изменения ОВХ SKU.
        /// </summary>
        public string SkuChangeMpWorkflowCode { get; set; }

        public ICommand BatchCommand { get; set; }

        public ICommand BeginWorkCommand { get; private set; }

        public ICommand EndWorkCommand { get; private set; }

        public ICommand CancelCommand { get; private set; }

        public ICommand AcceptCommand { get; private set; }

        public ICommand ManageWorkCommand { get; private set; }

        public ICommand CloneCurrentCommand { get; private set; }

        public ICommand ChangeSkuParamsCommand { get; private set; }

        public ICommand ShowProductsCommand { get; private set; }

        public EntityListViewModel Products { get; private set; }

        public bool IsProductsShown
        {
            get { return _isProductsShown; }
            set
            {
                _isProductsShown = value;
                OnPropertyChanged("IsProductsShown");
            }
        }

        public EntityRefDescriptor PlaceDescriptor { get; private set; }

        public EntityReference AcceptancePlace
        {
            get { return _acceptancePlace; }
            set
            {
                _acceptancePlace = value;
                OnPropertyChanged("AcceptancePlace");
            }
        }

        /// <summary>
        /// Формат отображаемых полей.
        /// </summary>
        public IDictionary<string, string> DisplayFieldsFormat { get; set; }
        #endregion . Properties  .

        public AcceptanceViewModel()
        {
            // фильтр по умолчанию
            // задается из вне через активити
            PlaceLookupCode = "PLACE_PLACECODE";
            PlaceFilter = "STATUSCODE_R = 'PLC_FREE'";

            // если задавать это в XAML, то Dx их преобразует в cammelCase (Id, ParentId)
            KeyFieldName = IWBPosInput.IDPropertyName;
            ParentFieldName = IWBPosInput.ParentIDPropertyName;

            BatchCommand = new DelegateCustomCommand(OnBatchCommand, CanBatchCommand);
            BeginWorkCommand = new DelegateCustomCommand<bool?>(BeginWork, CanBeginWork);
            EndWorkCommand = new DelegateCustomCommand(EndWork, CanEndWork);
            AcceptCommand = new DelegateCustomCommand(Accept, CanAccept);
            CancelCommand = new DelegateCustomCommand(Cancel);
            ManageWorkCommand = new DelegateCustomCommand(ManageWork);
            ChangeSkuParamsCommand = new DelegateCustomCommand(ChangeSkuParams, CanChangeSkuParams);
            ShowProductsCommand = new DelegateCustomCommand(ShowProducts, CanShowProducts);

            CloneCurrentCommand = new DelegateCustomCommand<bool?>(CloneItem, CanCloneItem);

            Commands.AddRange(new[]
            {BatchCommand, AcceptCommand, CloneCurrentCommand, ChangeSkuParamsCommand, ShowProductsCommand});

            _timer = new Timer(1000) {AutoReset = true};
            _timer.Elapsed += delegate { TimeText = (DateTime.Now - _startTime).ToString(@"mm\:ss"); };

            var entityViewModelFactory = IoC.Instance.Resolve<IEntityViewModelFactory>();
            Products = entityViewModelFactory.CreateEntityList<EntityListViewModel>(Product.EntityType);
            Products.IsAllowAutoLoadStoreOnInit = false;

            SelectedItems.CollectionChanged += SelectedItemsOnCollectionChanged;

            PlaceDescriptor = new EntityRefDescriptor
            {
                EntityType = Place.EntityType,
                Fields = new List<EntityRefFieldDescriptor>
                {
                    new EntityRefFieldDescriptor {Name = "PlaceCode"},
                    new EntityRefFieldDescriptor {Name = "PlaceName"}
                },
                Format = "{{=it.PlaceName}}"
            };

            _updateProductAction = new DeferredAction(TimeSpan.FromMilliseconds(500), () =>
            {
                DispatcherHelper.Invoke(new Action(UpdateProductsBySelectedItems));
            });
        }

        #region .  Methods  .

        public override void InitializeMenus()
        {
            MenuSuffix = GetType().GetFullNameWithoutVersion();

            _cloneCommandMenuItem = new CommandMenuItem
            {
                Caption = "Клонировать",
                Command = CloneCurrentCommand,
                ImageSmall = ImageResources.DCLCloneSimple16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLCloneSimple32.GetBitmapImage(),
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 10
            };

            _cloneListMenuItem = new ListMenuItem
            {
                DisplayMode = DisplayModeType.Content,
                Priority = 11,
                MenuItems = new MenuItemCollection
                {
                    new SubListMenuItem()
                    {
                        Caption = "С мастером",
                        Command = CloneCurrentCommand,
                        CommandParameter = true,
                        ImageSmall = ImageResources.DCLCloneMaster16.GetBitmapImage(),
                        ImageLarge = ImageResources.DCLCloneMaster32.GetBitmapImage(),
                        GlyphAlignment = GlyphAlignmentType.Top,
                        DisplayMode = DisplayModeType.Default,
                        Priority = 1
                    }
                }
            };

            _beginCommandMenuItem = new CommandMenuItem
            {
                Caption = "Начать",
                Command = BeginWorkCommand,
                CommandParameter = true,
                ImageSmall = ImageResources.DCLWorkOpen16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLWorkOpen32.GetBitmapImage(),
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                IsVisible = !IsEnabled,
                Priority = 12
            };
            _endCommandMenuItem = new CommandMenuItem
            {
                Caption = "Закончить",
                Command = EndWorkCommand,
                ImageSmall = ImageResources.DCLWorkClose16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLWorkClose32.GetBitmapImage(),
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                IsVisible = IsEnabled,
                Priority = 13
            };
            _manageWorkCommandMenuItem = new CommandMenuItem
            {
                Caption = "Работы",
                Command = ManageWorkCommand,
                ImageSmall = ImageResources.DCLWorkTimeControl16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLWorkTimeControl32.GetBitmapImage(),
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 14
            };

            base.InitializeMenus();
        }

        protected override void OnSetSource(object source)
        {
            Contract.Requires(source != null);

            var list = source as IEnumerable<IWBPosInput>;
            if (list == null)
                throw new ArgumentException(string.Format("Ожидался IEnumerable<IWBPosInput>, а получили '{0}'.", source == null ? "null" : source.GetType().ToString()));

            var wrappedSource = list.Select(i => new AcceptanceItemInfo(i)
            {
                IsSelected = false,
            }).ToArray();

            var targetSource = new WMSBusinessCollection<AcceptanceItemInfo>(wrappedSource);
            base.OnSetSource(targetSource);

            // родительские элементы могут прийти извне - нужно всегда пересчитывать правильное состояние
            foreach (var parent in Source.Where(i => !i.ParentId.HasValue && Source.Any(j => j.ParentId == i.Id)))
            {
                // помечаем родителем
                parent.HasChildren = true;

                // пересчитаем принимаемое кол-во
                parent.RequiredSKUCount = Source.Where(i => i.ParentId == parent.Id).Sum(i => i.RequiredSKUCount);
            }
        }

        protected override void SubscribeSource()
        {
            base.SubscribeSource();

            var source = Source as WMSBusinessCollection<AcceptanceItemInfo>;
            if (source != null)
                source.ItemPropertyChanged += OnItemPropertyChanged;
        }

        protected override void UnSubscribeSource()
        {
            base.UnSubscribeSource();

            var source = Source as WMSBusinessCollection<AcceptanceItemInfo>;
            if (source != null)
                source.ItemPropertyChanged -= OnItemPropertyChanged;
        }

        protected override ObservableCollection<DataField> GetDataFields()
        {
            var needFields = new[]
            {
                IWBPosInput.SKUNAMEPropertyName,
                IWBPosInput.IWBPosCountPropertyName,
                IWBPosInput.RequiredSKUCountPropertyName,
                IWBPosInput.ProductCountSKUPropertyName,
                IWBPosInput.RemainCountPropertyName,
                IWBPosInput.IWBPosTEPropertyName,
                IWBPosInput.TETypeCodePropertyName,
                IWBPosInput.SKU2TTEQuantityPropertyName,
                IWBPosInput.SKU2TTEQuantityMaxPropertyName,
                IWBPosInput.QLFCodePropertyName,
                IWBPosInput.IWBPosBatchPropertyName,
                IWBPosInput.ArtDescPropertyName,
                IWBPosInput.IWBPosColorPropertyName,
                IWBPosInput.IWBPosTonePropertyName,
                IWBPosInput.IWBPOSLOTPropertyName,
                IWBPosInput.IWBPosSizePropertyName,
                IWBPosInput.IWBPosSerialNumberPropertyName,
                IWBPosInput.IWBPosProductDatePropertyName,
                IWBPosInput.IWBPosExpiryDatePropertyName,
                IWBPosInput.SKU2TTEHeightPropertyName,
                IWBPosInput.IWBPosBlockingPropertyName,
                "VOWNERCODE",
                "VCOUNTRYNAME",
                "VSKULENGTH",
                "VSKUWIDTH",
                "VSKUHEIGHT",
                IWBPosInput.IWBPOSBOXNUMBERPropertyName,
                "VFACTORYNAME",
                IWBPosInput.POSIWBPOSCOUNTPropertyName,
                IWBPosInput.POSPRODUCTCOUNTPropertyName
            };

            var listFields = DataFieldHelper.Instance.GetDataFields(typeof (IWBPosInput), SettingDisplay.List);
            var cardFields = DataFieldHelper.Instance.GetDataFields(typeof (IWBPosInput), SettingDisplay.Detail);

            var resultFields = listFields.Where(i => needFields.Contains(i.Name)).ToList();
            var existsNames = resultFields.Select(i => i.Name).ToList();
            resultFields.AddRange(cardFields.Where(i => !existsNames.Contains(i.Name) && needFields.Contains(i.Name)));

            // доп поля
            var placeField = new DataFieldWithDomain
            {
                Name = "Place",
                Visible = true,
                IsEnabled = true,
                Caption = "Место",
                FieldName = "Place",
                BindingPath = "Place",
                FieldType = typeof (EntityReference),
                Domain = new EntityRefDomain
                {
                    EntityRefDescriptor = PlaceDescriptor
                },
                SourceName = "Place"
            };
            resultFields.Add(placeField);

            var enableEditFields = new[]
            {
                IWBPosInput.RequiredSKUCountPropertyName,
                placeField.Name,
                IWBPosInput.IWBPosTEPropertyName,
                IWBPosInput.TETypeCodePropertyName,
                IWBPosInput.SKU2TTEQuantityPropertyName,
                IWBPosInput.QLFCodePropertyName,
                IWBPosInput.IWBPosBatchPropertyName,
                IWBPosInput.IWBPosColorPropertyName,
                IWBPosInput.IWBPosTonePropertyName,
                IWBPosInput.IWBPOSLOTPropertyName,
                IWBPosInput.IWBPosSizePropertyName,
                IWBPosInput.IWBPosSerialNumberPropertyName,
                IWBPosInput.IWBPOSBOXNUMBERPropertyName,
            };

            foreach (var item in resultFields)
            {
                item.IsEnabled = enableEditFields.Contains(item.Name);
                switch (item.Name)
                {
                    case IWBPosInput.SKUNAMEPropertyName:
                        break;

                    case IWBPosInput.IWBPosCountPropertyName:
                        item.Caption = "По док.";
                        break;

                    case IWBPosInput.RequiredSKUCountPropertyName:
                        item.Caption = "Принимается";
                        break;

                    case IWBPosInput.ProductCountSKUPropertyName:
                        item.Caption = "Принято";
                        break;

                    case "PLACECODE_R":
                    case IWBPosInput.IWBPosTEPropertyName:
                    case IWBPosInput.TETypeCodePropertyName:
                    case IWBPosInput.SKU2TTEQuantityPropertyName:
                    case IWBPosInput.SKU2TTEQuantityMaxPropertyName:
                    case IWBPosInput.QLFCodePropertyName:
                    case IWBPosInput.IWBPosBatchPropertyName:
                    case IWBPosInput.ArtDescPropertyName:
                    case IWBPosInput.IWBPosColorPropertyName:
                    case IWBPosInput.IWBPosTonePropertyName:
                    case IWBPosInput.IWBPOSLOTPropertyName:
                    case IWBPosInput.IWBPosSizePropertyName:
                        break;

                    case IWBPosInput.IWBPosSerialNumberPropertyName:
                        item.Caption = "SN";
                        break;

                    case IWBPosInput.IWBPosProductDatePropertyName:
                        item.Caption = "Произведено";
                        break;

                    case "VCOUNTRYNAME":
                        item.Caption = "Страна";
                        break;

                    case IWBPosInput.IWBPosExpiryDatePropertyName:
                    case IWBPosInput.SKU2TTEHeightPropertyName:
                    case IWBPosInput.IWBPosBlockingPropertyName:
                    case IWBPosInput.ParentIDPropertyName:
                        break;

                    //default:
                    //    item.EnableEdit = false;
                    //    item.IsEnabled = false;
                    //    item.Visible = true;
                    //    break;
                }
            }

            //Формат полей
            if (DisplayFieldsFormat != null)
            {
                foreach (var item in resultFields.Where(p => !string.IsNullOrEmpty(p.Name) && DisplayFieldsFormat.ContainsKey(p.Name)))
                {
                    item.DisplayFormat = DisplayFieldsFormat[item.Name];
                }

                if (Products != null && Products.Fields != null)
                {
                    foreach (var item in Products.Fields.OfType<ClientField>().Where(p => !string.IsNullOrEmpty(p.Name)))
                    {
                        var key = item.Name.ToUpper();
                        if (DisplayFieldsFormat.ContainsKey(key))
                        {
                            var domain = item.Domain as DateTimeDomain;
                            if (domain != null)
                                domain.Format = DisplayFieldsFormat[key];
                        }
                    }
                }
            }

            return new ObservableCollection<DataField>(resultFields);
        }

        protected override void InitializeContextMenu()
        {
            base.InitializeContextMenu();

            ContextMenu.Add(new CommandMenuItem()
            {
                Caption = "Принять",
                Command = AcceptCommand,
                ImageSmall = ImageResources.DCLAccept16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLAccept32.GetBitmapImage(),
                //HotKey = new KeyGesture(Key.F7),
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = -10
            });
            ContextMenu.Add(new CommandMenuItem()
            {
                Caption = "Удалить",
                Command = DeleteCommand,
                ImageSmall = ImageResources.DCLDelete16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLDelete32.GetBitmapImage(),
                //HotKey = new KeyGesture(Key.F7),
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = -8
            });
            ContextMenu.Add(new SeparatorMenuItem() {Priority = -5});
            ContextMenu.Add(_cloneCommandMenuItem);
            ContextMenu.Add(new CommandMenuItem()
            {
                Caption = "С мастером",
                Command = CloneCurrentCommand,
                CommandParameter = true,
                ImageSmall = ImageResources.DCLCloneMaster16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLCloneMaster32.GetBitmapImage(),
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = _cloneCommandMenuItem.Priority + 1
            });
        }

        protected override void CreateMainMenu()
        {
            base.CreateMainMenu();

            var barProcess = Menu.GetOrCreateBarItem("Processes", 2);
            if (!string.IsNullOrEmpty(BatchcodeWorkflowCode))
            {
                barProcess.MenuItems.Add(new CommandMenuItem
                {
                    Caption = "Batch-код",
                    Command = BatchCommand,
                    ImageSmall = ImageResources.DCLBatchProcessParse16.GetBitmapImage(),
                    ImageLarge = ImageResources.DCLBatchProcessParse32.GetBitmapImage(),
                    //HotKey = new KeyGesture(Key.F7),
                    GlyphAlignment = GlyphAlignmentType.Top,
                    DisplayMode = DisplayModeType.Default,
                    Priority = 0
                });
            }
            barProcess.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.ChangeOvxSkuCaption,
                Command = ChangeSkuParamsCommand,
                ImageSmall = ImageResources.DCLChangeOvxSku16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLChangeOvxSku32.GetBitmapImage(),
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 2
            });

            var barCommand = Menu.GetOrCreateBarItem(StringResources.WorkCommands, 1);
            barCommand.MenuItems.Add(_beginCommandMenuItem);
            barCommand.MenuItems.Add(_endCommandMenuItem);
            barCommand.MenuItems.Add(_manageWorkCommandMenuItem);
            barCommand.MenuItems.Add(_cloneCommandMenuItem);
            barCommand.MenuItems.Add(_cloneListMenuItem);
            barCommand.MenuItems.Add(new SeparatorMenuItem {Priority = 2});
        }

        protected override void SourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            base.SourceCollectionChanged(sender, e);
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var oldItem in e.OldItems.Cast<AcceptanceItemInfo>())
                {
                    var parent = GetParentItem(oldItem);
                    if (parent != null)
                        parent.HasChildren = Source.Any(i => i.ParentId == parent.Id);
                    oldItem.RequiredSKUCount = 0;
                }
            }
        }

        protected override bool CanCloseInternal()
        {
            if (!IsAllowAccept)
            {
                if (!IsEnabled)
                    return true;

                var isUserWantsExistWithoutSaveChanges = ViewService.ShowDialog(StringResources.Confirmation,
                    string.Format(StringResources.CloseFromConfirmationFormat, Environment.NewLine),
                    MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);

                // разрешаем закрыть, если пользователь согласился
                return isUserWantsExistWithoutSaveChanges == MessageBoxResult.Yes;
            }

            var errorMessage = new StringBuilder();
            var properties = TypeDescriptor.GetProperties(typeof (IWBPosInput));
            using (var pmConfigMgr = IoC.Instance.Resolve<IPMConfigManager>())
            {
                foreach (var item in SelectedItems)
                {
                    // будем выставлять дефолтное значение, если объект новый
                    if (item.IsNew)
                        DefaultValueSetter.Instance.SetDefaultValues(item);

                    if (IsMigration)
                        continue;

                    var mustPropertyList = pmConfigMgr.GetPMConfigByParamListByArtCode(item.ArtCode, OperationCode,
                        "MUST_SET");
                    foreach (var mustProperty in mustPropertyList)
                    {
                        var p = properties.Find(mustProperty.ObjectName_r, true);
                        if (p != null)
                        {
                            if (item.GetProperty(mustProperty.ObjectName_r) == null)
                            {
                                errorMessage.AppendFormat(
                                    "В строке номер {0} не заполнено обязательное поле '{1}' :{2}.{3}",
                                    Source.IndexOf(item) + 1, p.DisplayName, mustProperty.ObjectName_r,
                                    Environment.NewLine);
                            }
                        }
                        else
                        {
                            errorMessage.AppendFormat(
                                "Ошибка в настройках MUST_SET менеджера товара.{1}Задан неизвестный параметр '{0}'.",
                                mustProperty.ObjectName_r, Environment.NewLine);
                        }
                    }
                }
            }

            if (errorMessage.Length > 0)
            {
                ViewService.ShowDialog("Ошибка", string.Format("Ошибки:{0}{1}", Environment.NewLine, errorMessage),
                    MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                return false;
            }
            return true;
        }

        protected override async Task<IViewModel> WrappModelIntoVM(AcceptanceItemInfo model)
        {
            return await Task.Factory.StartNew(() =>
            {
                if (ObjectViewModel != null)
                {
                    var disposable = ObjectViewModel as IDisposable;
                    if (disposable != null)
                        disposable.Dispose();
                    ObjectViewModel = null;
                }
                ObjectViewModel = (IObjectViewModel<AcceptanceItemInfo>) IoC.Instance.Resolve(typeof (AcceptanceItemViewModel));
                ObjectViewModel.SetSource(model);
                var md = (AcceptanceItemViewModel) ObjectViewModel;
                md.MandantId = MandantId;
                md.DisplayFieldsFormat = DisplayFieldsFormat;
                md.ParentViewModel = this;
                return ObjectViewModel;
            });
        }

        protected override async Task<IViewModel> WrappModelIntoVM(IEnumerable<AcceptanceItemInfo> model)
        {
            return await Task.Factory.StartNew(() =>
            {
                if (ObjectViewModel != null)
                {
                    var disposable = ObjectViewModel as IDisposable;
                    if (disposable != null)
                        disposable.Dispose();
                    ObjectViewModel = null;
                }
                ObjectViewModel =
                    (IObjectViewModel<AcceptanceItemInfo>) IoC.Instance.Resolve(typeof (AcceptanceItemViewModel));
                var pem = ObjectViewModel as IPropertyEditHandler;
                if (pem == null)
                    throw new DeveloperException("Модель не реализует IPropertyEditHandler");
                pem.SetSource(model);
                if (MandantId != null)
                {
                    var md = (AcceptanceItemViewModel) ObjectViewModel;
                    md.MandantId = MandantId;
                }
                return ObjectViewModel;
            });
        }

        protected override AcceptanceItemInfo CreateNewItem()
        {
            return new AcceptanceItemInfo(base.CreateNewItem());
        }

        protected override bool CanNew()
        {
            return IsEnabled && base.CanNew();
        }

        protected override bool CanEdit()
        {
            return IsEnabled && SelectedItems.Count == 1 && !SelectedItems[0].HasChildren;
        }

        protected override bool CanDelete()
        {
            return IsEnabled && base.CanDelete() && !SelectedItems[0].HasChildren;
        }

        private bool CanAccept()
        {
            return SelectedItems.Count > 0 && !SelectedItems.Any(i => i.HasChildren);
        }

        private bool CanBatchCommand()
        {
            return IsEnabled && !string.IsNullOrEmpty(BatchcodeWorkflowCode) && HasSelectedItems();
        }

        private bool CanChangeSkuParams()
        {
            return IsEnabled && !string.IsNullOrEmpty(SkuChangeMpWorkflowCode) && SelectedItems != null &&
                   SelectedItems.Count == 1;
        }

        private void ChangeSkuParams()
        {
            if (!CanChangeSkuParams())
                return;

            if (!ConnectionManager.Instance.AllowRequest())
                return;

            try
            {
                WaitStart();

                var skuid = SelectedItems[0].SKUID;
                var bpContext = new BpContext
                {
                    Items = new object[] {new SKU {SKUID = skuid ?? 0}},
                    DoNotRefresh = false
                };
                bpContext.Set("NeedRefreshSku", true);

                var executionContext = new ExecutionContext(SkuChangeMpWorkflowCode,
                    new Dictionary<string, object>
                    {
                        {BpContext.BpContextArgumentName, bpContext}
                    });
                var engine = IoC.Instance.Resolve<IProcessExecutorEngine>(WorkflowProcessExecutorConstants.Workflow);
                engine.Run(context: executionContext, completedHandler: OnCompleteChangeSkuOvx);
            }
            finally
            {
                WaitStop();
            }
        }

        private void OnCompleteChangeSkuOvx(CompleteContext ctx)
        {
            try
            {
                WaitStart();

                if (ctx.Parameters != null && ctx.Parameters.ContainsKey(BpContext.BpContextArgumentName))
                {
                    var isResultOk = false;
                    var bpcontext = ctx.Parameters[BpContext.BpContextArgumentName] as BpContext;
                    if (bpcontext != null)
                        isResultOk = bpcontext.Get<bool>("IsResultOk");
                    if (isResultOk && bpcontext.Items != null)
                    {
                        var items = bpcontext.Items.OfType<SKU>().ToArray();
                        if (items.Length > 0)
                        {
                            var sku = items[0];
                            foreach (var item in Source.Where(p => p.SKUID == sku.SKUID))
                            {
                                try
                                {
                                    item.SuspendNotifications();
                                    item.SetProperty(IWBPosInput.VSKUHEIGHTPropertyName,
                                        sku.GetProperty(SKU.SKUHEIGHTPropertyName));
                                    item.SetProperty(IWBPosInput.VSKULENGTHPropertyName,
                                        sku.GetProperty(SKU.SKULENGTHPropertyName));
                                    item.SetProperty(IWBPosInput.VSKUWIDTHPropertyName,
                                        sku.GetProperty(SKU.SKUWIDTHPropertyName));
                                }
                                finally
                                {
                                    item.ResumeNotifications();
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                WaitStop();
            }
        }

        protected override void Delete()
        {
            var parents = SelectedItems.Where(x => x.ParentId != null).Select(x => x.ParentId).ToArray().GroupBy(x => x ?? 0);
            base.Delete();
           
            foreach (var parent in parents.Select(p => Source.FirstOrDefault(x => x.Id == p.Key)).Where(parent => parent != null))
            {
                var count =  (decimal)(parent.IWBPosCount - parent.ProductCountSKU);
                parent.RequiredSKUCount = count < 0 ? 0 : count;
            }
        }

        #region products

        private bool CanShowProducts()
        {
            return IsEnabled && SelectedItems.Count > 0;
        }

        private void ShowProducts()
        {
            IsProductsShown = !IsProductsShown;
            UpdateProductsBySelectedItems();
        }

        #endregion

        #region Clone

        private bool CanCloneItem(bool? enableMaster)
        {
            return IsEnabled && SelectedItems.Count == 1 && !SelectedItems[0].ParentId.HasValue;
        }

        private void CloneItem(bool? enableMaster)
        {
            if (enableMaster == true)
                CloneWithMaster();
            else
                CloneItemInternal(SelectedItems[0]);
            RiseCommandsCanExecuteChanged();
        }

        private void CloneWithMaster()
        {
            try
            {
                WaitStart();

                var vm = GetCloneItemWithMasterViewModel();

                var res = ViewService.ShowDialogWindow(vm, true, width: "400");
                if (res == true)
                {
                    var rowCount = (int) vm.Source.Members["RowCount"];
                    var countInRow = (int) vm.Source.Members["CountInRow"];
                    var place = (EntityReference) vm.Source.Members["Place"];
                    var te = (string) vm.Source.Members["TE"];
                    for (int i = 0; i < rowCount; i++)
                    {
                        var clone = CloneItemInternal(SelectedItems[0]);
                        clone.RequiredSKUCount = countInRow;
                        clone.Place = place;
                        clone.IWBPosTE = te;
                    }
                }
            }
            finally
            {
                WaitStop();
            }
        }

        private AcceptanceItemInfo CloneItemInternal(AcceptanceItemInfo selectedItem)
        {
            var clonedItem = (AcceptanceItemInfo) selectedItem.Clone();
            clonedItem.IWBPosTE = null;
            var minId = Source.Min(i => i.Id);
            clonedItem.Id = minId >= 0 ? -1 : minId - 1;
            clonedItem.ParentId = selectedItem.Id;
            clonedItem.ProductCountSKU = 0;

            // если уже есть клоны - проставляем кол-во = 1, иначе - переносим кол-во родителя
            if (selectedItem.HasChildren)
            {
                clonedItem.RequiredSKUCount = 1;
                selectedItem.RequiredSKUCount += clonedItem.RequiredSKUCount;
            }
            else
            {
                clonedItem.RequiredSKUCount = selectedItem.RequiredSKUCount;
            }

            //TODO: заполняем поля клона
            clonedItem.AcceptChanges();
            clonedItem.RemainCount = null;
            clonedItem.Validate();

            Source.Add(clonedItem);
            selectedItem.HasChildren = true;

            return clonedItem;
        }

        private ExpandoObjectViewModelBase GetCloneItemWithMasterViewModel()
        {
            var vm = new ExpandoObjectViewModelBase();
            vm.PanelCaption = "Клонировать";
            vm.Fields = new ObservableCollection<ValueDataField>();
            vm.Fields.Add(new ValueDataField
            {
                Caption = "Строк",
                Name = "RowCount",
                FieldName = "RowCount",
                FieldType = typeof (int)
            });
            vm.Fields.Add(new ValueDataField
            {
                Caption = "Кол-во в строке",
                Name = "CountInRow",
                FieldName = "CountInRow",
                FieldType = typeof (int)
            });

            var placeField = new DataFieldWithDomain
            {
                Caption = "Место",
                Name = "Place",
                FieldName = "Place",
                FieldType = typeof (EntityReference),
                Domain = new EntityRefDomain()
                {
                    EntityRefDescriptor = PlaceDescriptor
                }
            };
            vm.Fields.Add(placeField);
            vm.Fields.Add(new ValueDataField
            {
                Caption = "TE",
                Name = "TE",
                FieldName = "TE",
                FieldType = typeof (string)
            });
            vm.Source.Members["RowCount"] = 1;
            vm.Source.Members["CountInRow"] = 1;
            vm.Source.Members["Place"] = SelectedItems[0].Place;
            vm.Source.Members["TE"] = SelectedItems[0].IWBPosTE;
            return vm;
        }

        #endregion

        #region Work

        private bool CanBeginWork(bool? parameter)
        {
            return true;
        }

        private bool CanEndWork()
        {
            return true;
        }

        private void ManageWork()
        {
            //var factory = IoC.Instance.Resolve<IEntityViewModelFactory>();
            //var vm = factory.Create<NewAcceptanceWorkingViewModel>();
            //vm.FiltersSet()
            //vm.IsReadOnly = false;

            //if (CurrentWorkId != null)
            //    vm.WorkId = CurrentWorkId.Value;

            //var docManager = IoC.Instance.Resolve<IDocumentManagerService>("Float");
            //var doc = docManager.CreateDocument("EntityListView", vm);
            //doc.Show();

            Work work = null;
            if (CurrentWorkId.HasValue)
                using (var mgr = IoC.Instance.Resolve<IBaseManager<Work>>())
                {
                    work = mgr.Get(CurrentWorkId);
                }
            
            var workingVm = new AcceptanceWorkingViewModel(this)
            {
                PanelCaption = typeof (Working).GetDisplayName(),
                ParentViewModelSource = work
            };
            ViewService.ShowDialogWindow(workingVm, true);
        }

        private async void BeginWork(bool? isPushBtn)
        {
            string errMessage = null;

            //Проверяем
            if (!WMSEnvironment.Instance.WorkerId.HasValue)
                errMessage = StringResources.WorkerNotSelect;
            else if (!Source.Any())
                errMessage = "Нет позиций у накладной";
            else if (CurrentIWB.IWB2CargoL == null || CurrentIWB.IWB2CargoL.Count == 0)
                errMessage = "Для накладной не указан входящий груз";

            if (errMessage != null)
            {
                if (!isPushBtn.HasValue || isPushBtn.Value)
                    GetViewService().ShowDialog(StringResources.Error
                        , errMessage
                        , MessageBoxButton.OK
                        , MessageBoxImage.Warning
                        , MessageBoxResult.None);
                return;
            }

            if (!ConnectionManager.Instance.AllowRequest())
                return;

            var workerId = WMSEnvironment.Instance.WorkerId.Value;
            try
            {
                WaitStart();

                var result = await Task.Factory.StartNew(() => GetWork(workerId));
                if (!result)
                    return;

                StartWork(DateTime.Now);
            }
            finally
            {
                WaitStop();
                RiseCommandsCanExecuteChanged();
            }
        }

        private void StartWork(DateTime dt)
        {
            IsEnabled = true;

            _beginCommandMenuItem.IsVisible = !IsEnabled;
            _endCommandMenuItem.IsVisible = IsEnabled;

            _startTime = dt;
            _timer.Start();
        }

        private bool GetWork(decimal workerId)
        {
            var workOperation = BillOperationCode.OP_INPUT_REG.ToString();
            CurrentWorkId = null;

            var workhelper = new WorkHelper();
            Func<DataRow[], string, string> dialogMessageHandler = (rows, workername) =>
            {
                return string.Format(StringResources.YouHaveWorkingsMessageFormat, Environment.NewLine,
                    string.Join(Environment.NewLine,
                        rows.Select(p => string.Format("'{0}' ('{1}').", p["operationname"], p["workid"]))));
            };

            if (
                !workhelper.ClosingWorking(workerId: workerId, filter: null, dialogTitle: StringResources.Confirmation,
                    workername: null, dialogMessageHandler: dialogMessageHandler))
                return false;

            // странная логика (получаем только один груз)
            // TODO: выяснить почему мы игнорируем другие возможные грузы
            var iwb2Cargo = CurrentIWB.IWB2CargoL.First();

            //Создаем работу
            List<Work> workList;
            using (var mgr = IoC.Instance.Resolve<IBaseManager<Work>>())
            {
                var filter = string.Format(SQL_SELECT_WORK, iwb2Cargo.CARGOIWBID, workOperation);
                workList = mgr.GetFiltered(filter, GetModeEnum.Partial).ToList();
            }

            if (workList.Any())
            {
                using (var mgr = IoC.Instance.Resolve<IBaseManager<Working>>())
                {
                    var workId = workList.First().GetKey<decimal>();
                    CurrentWorkId = workId;
                    var working = new Working
                    {
                        WORKID_R = workId,
                        WORKERID_R = workerId,
                        WORKINGFROM = BPH.GetSystemDate()
                    };
                    mgr.Insert(ref working);
                }
            }
            else
            {
                using (var mgrBpProcessManager = IoC.Instance.Resolve<IBPProcessManager>())
                {
                    Work mywork;
                    mgrBpProcessManager.StartWorking("CARGOIWB", iwb2Cargo.CARGOIWBID.ToString(),
                        workOperation, workerId, MandantId, null, null, out mywork);
                    if (mywork != null)
                        CurrentWorkId = mywork.WORKID;
                }
            }

            return true;
        }

        private void EndWork()
        {
            try
            {
                WaitStart();
                CloseWorkAsync();
                IsEnabled = false;
                TimeText = TimeSpan.Zero.ToString(@"mm\:ss");
                _timer.Stop();
            }
            finally
            {
                WaitStop();
                RiseCommandsCanExecuteChanged();
            }
        }

        private async Task CloseWorkAsync()
        {
            List<Working> cargoIWBList;

            using (var mgr = IoC.Instance.Resolve<IBaseManager<Working>>())
            {
                cargoIWBList =
                    mgr.GetFiltered(
                        string.Format("workerid_r = {0} and workingtill is null", WMSEnvironment.Instance.WorkerId.Value),
                        GetModeEnum.Partial).ToList();
            }
            if (cargoIWBList.Any())
            {
                var mgrBpProcessManager = IoC.Instance.Resolve<IBPProcessManager>();
                mgrBpProcessManager.CompleteWorkings(cargoIWBList.Select(i => i.GetKey<Decimal>()), null);
            }
        }

        #endregion

        private void SelectedItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
                return;

            _updateProductAction.Reset();
        }

        private void UpdateProductsBySelectedItems()
        {
            if (!IsProductsShown)
                return;

            var filter = new MLC.Ext.Common.Model.Filter
            {
                Property = "IWBPos",
                Operator = JsFilterOperator.IN,
                Value = SelectedItems.Where(i => i.IWBPosId > 0)
                    .Select(i => new EntityReference(i.IWBPosId, Product.EntityType, new EntityReferenceFieldValue[0]))
                    .ToArray()
            };

            Products.FiltersSet(new[] {filter});
        }

        private void Accept()
        {
            IsAllowAccept = true;
            DoCloseRequest();
        }

        private void Cancel()
        {
            IsAllowAccept = false;
            DoCloseRequest();
        }

        private void OnBatchCommand()
        {
            if (!CanBatchCommand())
                return;

            if (!ConnectionManager.Instance.AllowRequest())
                return;

            Action<CompleteContext> completedHandler = ctx => WaitStop();

            try
            {
                WaitStart();
                var bpContext = new BpContext
                {
                    Items = SelectedItems.ToArray(),
                    DoNotRefresh = true
                };
                bpContext.Set("ShowErrorBatchcodeIsNull", true);
                var executionContext = new ExecutionContext(BatchcodeWorkflowCode,
                    new Dictionary<string, object>
                    {
                        {BpContext.BpContextArgumentName, bpContext}
                    });
                var engine = IoC.Instance.Resolve<IProcessExecutorEngine>(WorkflowProcessExecutorConstants.Workflow);
                engine.Run(context: executionContext, completedHandler: completedHandler);
            }
            catch (Exception)
            {
                WaitStop();
                throw;
            }
        }

        private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            var item = (AcceptanceItemInfo) sender;
            var isClone = item.ParentId.HasValue;

            if (isClone)
            {
                var parentItem = Source.FirstOrDefault(i => i.Id == item.ParentId);
                if (parentItem == null)
                    throw new Exception(
                        string.Format(
                            "Для записи с Id {0} не найдена родительская запись с Id {1}. Нарушена целостность дерева. Обратитесь к разработчикам.",
                            item.Id, item.ParentId));

                var isAnyOtherSkuExists =
                    Source.Where(i => i.ParentId == parentItem.Id).Any(i => i.SKUID != parentItem.SKUID);

                if (!isAnyOtherSkuExists &&
                    propertyChangedEventArgs.PropertyName == IWBPosInput.RequiredSKUCountPropertyName)
                {
                    object prevCount = null;
                    if (!item.GetPreviosPropertyValue(IWBPosInput.RequiredSKUCountPropertyName, out prevCount))
                        throw new Exception(
                            string.Format(
                                "Для записи с Id {0} не удалось получить предыдущее значение количества. Пересчет не возможен. Обратитесь к разработчикам.",
                                item.Id));

                    parentItem.RequiredSKUCount = parentItem.RequiredSKUCount - (decimal) prevCount +
                                                  item.RequiredSKUCount;
                }
                else if (propertyChangedEventArgs.PropertyName == IWBPosInput.SKUIDPropertyName)
                {
                    parentItem.RequiredSKUCount = isAnyOtherSkuExists
                        ? 0
                        : Source.Where(i => i.ParentId == parentItem.Id).Sum(i => i.RequiredSKUCount);
                }
            }
        }

        private AcceptanceItemInfo GetParentItem(AcceptanceItemInfo item)
        {
            if (!item.ParentId.HasValue)
                return null;

            var parentItem = Source.FirstOrDefault(i => i.Id == item.ParentId);
            if (parentItem == null)
                throw new Exception(
                    string.Format(
                        "Для записи с Id {0} не найдена родительская запись с Id {1}. Нарушена целостность дерева. Обратитесь к разработчикам.",
                        item.Id, item.ParentId));
            return parentItem;
        }

        public void CheckIsEnabled()
        {
            if (!WMSEnvironment.Instance.WorkerId.HasValue)
                return;
            using (var mgr = IoC.Instance.Resolve<IBaseManager<Working>>())
            {
                var res =
                    mgr.GetFiltered(
                        string.Format(
                            "WORKERID_R = {0} and WORKINGTILL is null and WORKID_R in (select wmswork.workid from wmswork where wmswork.OPERATIONCODE_R = '{1}')",
                            WMSEnvironment.Instance.WorkerId.Value, BillOperationCode.OP_INPUT_REG.ToString())).ToList();
                if (res.Count() != 1)
                    return;

                var dt = res.First().WORKINGFROM;
                CurrentWorkId = res.First().WORKID_R;
                StartWork(dt ?? DateTime.Now);
            }
        }

        public bool IsAllowEditProperty(string propertyName, object originalSource)
        {
            var item = (AcceptanceItemInfo) originalSource;
            if (item.HasChildren)
                return false;

            return true;
        }

        protected override async Task<bool> Show(AcceptanceItemInfo model)
        {
            var isnew = (model == null || model.IWBPosCount <= 0);
            var vm = await WrappModelIntoVM(model);
            var res = ViewService.ShowDialogWindow(vm, true, true, null, null, false, isnew ? MessageBoxButton.OKCancel : MessageBoxButton.OK) == true;
            return !isnew || res;
        }

        internal void SetFields(EntityLookupViewModel placeEntityLookupViewModel)
        {
            if (placeEntityLookupViewModel != null && placeEntityLookupViewModel.Fields != null)
            {
                var fieldId = placeEntityLookupViewModel.Fields.FirstOrDefault(p => p.Name.ToUpper() == "PLACECODE") as ClientField;
                if (fieldId != null)
                    fieldId.Hidden = true;
            }
        }
        #endregion .  Methods  .

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (_updateProductAction != null)
                {
                    _updateProductAction.Dispose();
                    _updateProductAction = null;
                }
            }
        }
    }
}
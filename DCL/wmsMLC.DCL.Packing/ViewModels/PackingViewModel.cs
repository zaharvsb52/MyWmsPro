//TODO
// * Долго открывается диалог печати
// * 
// *
// *
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using log4net;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.Business.Objects;
using wmsMLC.Business.Objects.Processes;
using wmsMLC.DCL.General.Helpers;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.General.ViewModels.Menu;
using wmsMLC.DCL.Main.Views;
using wmsMLC.DCL.Packing.Views;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.DAL;
using wmsMLC.General.PL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Helpers;
using wmsMLC.General.PL.WPF.Views;
using FilterHelper = wmsMLC.General.BL.Helpers.FilterHelper;
#pragma warning disable 1998

namespace wmsMLC.DCL.Packing.ViewModels
{
    [View(typeof(PackingView))]
    public class PackingViewModel : PanelViewModelBase, IPackingViewModel, IHaveUniqueName
    {
        #region .  Fields & consts  .
        public const string PackingWord = "Упаковка";
        public const string CurrentPlaceCodePropertyName = "CurrentPlaceCode";
        public const string ProductsFilterPropertyName = "ProductsFilter";
        public const string SelectedPackPropertyName = "SelectedPack";
        public const string ActivePackPropertyName = "ActivePack";
        public const string PackedProductsPropertyName = "PackedProducts";
        public const string AvailableTEPropertyName = "AvailableTE";
        public const string PackingProductsPropertyName = "PackingProducts";
        public const string VisiblePackingProductsPropertyName = "VisiblePackingProducts";
        public const string PackedCaptionPropertyName = "PackedCaption";
        public const string HidePropertyName = "Hide";
        public const string LastPackedProductPropertyName = "LastPackedProduct";

        // отбираем только места с признаком место упаковки
        public const string DefaultPlaceFilter = "1 = 1 and pkgCustomParamValue.entityHasTag('PLACE', PLACECODE, 'PACK') > 0";

        // шаблон фильтра для получения товаров на данном месте (получаем только зарезервированные товары, которые не находятся в упаковке)
        public const string ProductsOnPlaceFilterTemplate =
            "owbposid is not null and vPlaceCode='{0}' and productid in (select p.productid from wmsproduct p inner join wmste t on t.tecode = p.tecode_r where t.tecurrentplace='{0}' and t.tepackstatus='TE_PKG_NONE')";

        public const string UniqueName = "D92B4E51-1FF1-4890-A4D4-E96FC5B16236";

        private ILog _log = LogManager.GetLogger(typeof(PackingViewModel));

        private string _currentPlaceCode;
        private string _productsFilter;
        private TE _selectedPack;
        private TE _activePack;
        private ObservableCollection<Product> _packedProducts;
        private ObservableCollection<TE> _availableTE;
        private ObservableCollection<Product> _packingProducts;
        private ObservableCollection<Product> _visiblePackingProducts;
        private string _packedCaption;
        private string _packingProductEditValue;
        private decimal? _skuIdOld;
        private decimal? _storedProductCountSku;
        private bool _hide;
        private Product _lastPackedProduct;
        private string _placeFilter;
        private ObservableCollection<Product> _selectedPackingProducts;
        private CommandMenuItem _fullScreenModeMenuItem;
        private bool _isMaxView;
        private bool _haveChild;

        private static readonly Lazy<string> SKUPatternForBc = new Lazy<string>(() => FilterHelper.GetAttrEntity<SKU>(SKU.SKUIDPropertyName, SKU.SKUNamePropertyName, SKU.ArtCodePropertyName));

        private volatile int _packProcessCount;
        private volatile bool _packStatus;

        private readonly List<Product> _productForRefresh = new List<Product>();
        readonly ConcurrentDictionary<string, Lazy<SKU[]>> _barcodeCache = new ConcurrentDictionary<string, Lazy<SKU[]>>();
        #endregion .  Fields & consts  .

        #region .  сtors  .

        public PackingViewModel()
        {
            PackWorkflowCodes.ClearCachableWorkflow();

            TEKeyPropertyName = TE.TECodePropertyName.ToUpper();
            TEParentPropertyName = TE.TECarrierStreakCodePropertyName.ToUpper();

            PanelCaption = PackingWord;
            PanelCaptionImage = ImageResources.DCLPackingPanel16.GetBitmapImage();
            PlaceFilter = DefaultPlaceFilter;
            Hide = false;

            SelectedPacks = new ObservableCollection<TE>();
            SelectedPackedProducts = new ObservableCollection<Product>();
            SelectedPackedProducts.CollectionChanged += SelectedPackedProductsOnCollectionChanged;
            SelectedPackingProducts = new ObservableCollection<Product>();
            SelectedPackingProducts.CollectionChanged += SelectedPackingProductsOnCollectionChanged;

            VisiblePackingProducts = new ObservableCollection<Product>();

            CreateCommands();
            CreateMenu();
            FillFields();

            FillCurrentPlaceAsync();
        }

        #endregion

        #region .  Properties  .

        public string TEKeyPropertyName { get; set; }

        public string TEParentPropertyName { get; set; }

        public bool HaveChild
        {
            get { return _haveChild; }
            set
            {
                _haveChild = value;
                OnPropertyChanged("HaveChild");
            }
        }

        public ObservableCollection<Product> FilteredProducts { get; set; }

        /// <summary>
        /// Фильтр возможных мест упаковки
        /// </summary>
        public string PlaceFilter
        {
            get { return _placeFilter; }
            set
            {
                if (_placeFilter == value)
                    return;

                _placeFilter = value;
                OnPropertyChanged("PlaceFilter");
            }
        }

        /// <summary>
        /// Фильтр получения торвара, который требуется упаковать
        /// </summary>
        public string ProductsFilter
        {
            get { return _productsFilter; }
            set
            {
                if (_productsFilter == value)
                    return;

                _productsFilter = value;
                OnPropertyChanged(ProductsFilterPropertyName);
                OnProductFilterChanged();
            }
        }

        /// <summary>
        /// Текущее место упаковки
        /// </summary>
        public string CurrentPlaceCode
        {
            get { return _currentPlaceCode; }
            set
            {
                if (_currentPlaceCode == value)
                    return;

                _currentPlaceCode = value;
                OnPropertyChanged(CurrentPlaceCodePropertyName);
                OnCurrentPlaceCodeChanged();
            }
        }

        public ObservableCollection<TE> AvailableTE
        {
            get { return _availableTE; }
            set
            {
                _availableTE = value;
                OnPropertyChanged(AvailableTEPropertyName);
            }
        }

        public TE SelectedPack
        {
            get { return _selectedPack; }
            set
            {
                if (_selectedPack == value)
                    return;

                _selectedPack = value;
                OnPropertyChanged(SelectedPackPropertyName);
                OnSelectedPackChanged();
            }
        }

        public ObservableCollection<TE> SelectedPacks { get; set; }

        public TE ActivePack
        {
            get { return _activePack; }
            set
            {
                if (_activePack == value)
                    return;

                _activePack = value;
                OnPropertyChanged(ActivePackPropertyName);
                OnActivePackChanged();
            }
        }
        public Product LastPackedProduct
        {
            get { return _lastPackedProduct; }
            set
            {
                if (_lastPackedProduct == value)
                    return;

                _lastPackedProduct = value;
                OnPropertyChanged(LastPackedProductPropertyName);
            }
        }

        public ObservableCollection<DataField> AvailableTEFields { get; set; }
        public ObservableCollection<DataField> PackedProductFields { get; set; }
        public ObservableCollection<DataField> PackingProductFields { get; set; }


        public ObservableCollection<Product> PackingProducts
        {
            get { return _packingProducts; }
            set
            {
                _packingProducts = value;
                OnPropertyChanged(PackingProductsPropertyName);
            }
        }

        public ObservableCollection<Product> VisiblePackingProducts
        {
            get { return _visiblePackingProducts; }
            set
            {
                _visiblePackingProducts = value;
                OnPropertyChanged(VisiblePackingProductsPropertyName);
            }
        }

        public ObservableCollection<Product> SelectedPackingProducts
        {
            get { return _selectedPackingProducts; }
            set
            {
                _selectedPackingProducts = value;
                OnPropertyChanged("SelectedPackingProducts");
            }
        }

        public ObservableCollection<Product> PackedProducts
        {
            get { return _packedProducts; }
            set
            {
                _packedProducts = value;
                OnPropertyChanged(PackedProductsPropertyName);
            }
        }

        public ObservableCollection<Product> SelectedPackedProducts { get; set; }

        public string PackedCaption
        {
            get { return _packedCaption; }
            set
            {
                if (_packedCaption == value)
                    return;

                _packedCaption = value;
                OnPropertyChanged(PackedCaptionPropertyName);
            }
        }

        public string PackingProductEditValue
        {
            get { return _packingProductEditValue; }
            set
            {
                _packStatus = false;
                if (_packingProductEditValue == value)
                    return;
                _packingProductEditValue = value;
                OnPropertyChanged("PackingProductEditValue");
                OnPackingProductEditValueChanged();
            }
        }

        public ICommand RefreshCommand { get; private set; }
        public ICommand FullScreenModeCommand { get; private set; }
        public ICommand CreatePackCommand { get; private set; }
        public ICommand OpenPackCommand { get; private set; }
        public ICommand ClosePackCommand { get; private set; }
        public ICommand SetWeightCommand { get; private set; }
        public ICommand DeletePackCommand { get; private set; }
        public ICommand PutInCommand { get; private set; }
        public ICommand PackCommand { get; private set; }
        public ICommand PackAllCommand { get; private set; }
        public ICommand UnpackAllCommand { get; private set; }
        public ICommand ReturnOnSourceTECommand { get; private set; }
        public ICommand PackSourceTECommand { get; private set; }
        public ICommand MoveToOtherTECommand { get; private set; }
        public ICommand MoveAllToOtherTECommand { get; private set; }

        public ICommand SetActivePackCommand { get; private set; }
        public ICommand PrintCommand { get; set; }

        //TODO: разобраться что за свойство
        public bool Hide
        {
            get { return _hide; }
            set
            {
                if (_hide == value)
                    return;

                _hide = value;
                OnPropertyChanged(HidePropertyName);
            }
        }

        public bool PackInProgress
        {
            get { return _packProcessCount > 0; }
        }

        public bool PackStatus
        {
            get { return _packStatus; }
        }

        #endregion

        #region .  Methods  .

        protected override void InitializeSettings()
        {
            base.InitializeSettings();

            IsMenuEnable = true;
            IsCustomizeBarEnabled = true;

            AllowClosePanel = true;
        }

        private void SelectedPackingProductsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Remove)
                RiseCommandsCanExecuteChanged();
        }

        private void SelectedPackedProductsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Remove)
                RiseCommandsCanExecuteChanged();
        }

        private void FillFields()
        {
            PackingProductFields = DataFieldHelper.Instance.GetDataFields(typeof(Product), SettingDisplay.List);
            PackedProductFields = DataFieldHelper.Instance.GetDataFields(typeof(Product), SettingDisplay.List);
            AvailableTEFields = DataFieldHelper.Instance.GetDataFields(typeof(TE), SettingDisplay.List);
        }

        private async void FillCurrentPlaceAsync()
        {
            WaitStart();
            try
            {
                await Task.Factory.StartNew(FillCurrentPlace);
            }
            finally
            {
                WaitStop();
            }
        }

        private void FillCurrentPlace()
        {
            if (string.IsNullOrEmpty(WMSEnvironment.Instance.ClientCode))
                throw new OperationException("Невозможно определить код клиента. Пожалуйста, обратитесь в службу поддержки.");

            // получаем клиента и его CPV
            Client client;
            using (var mgr = IoC.Instance.Resolve<IBaseManager<Client>>())
                client = mgr.Get(WMSEnvironment.Instance.ClientCode);

            if (client == null)
            {
                var message = string.Format("Не найден клиент с кодом {0}", WMSEnvironment.Instance.ClientCode);
                ShowWarning(message);
                return;
            }

            if (client.CustomParamVal == null)
            {
                var message = string.Format("Для клиента с кодом {0} не указаны параметры", WMSEnvironment.Instance.ClientCode);
                ShowWarning(message);
                return;
            }

            var clientPackingPlace = client.CustomParamVal.FirstOrDefault(i => i.CustomParamCode == "ClientPackingPlaceL2");
            if (clientPackingPlace == null || string.IsNullOrEmpty(clientPackingPlace.CPVValue))
            {
                var message = string.Format("Для клиента с кодом {0} не указано место упаковки", WMSEnvironment.Instance.ClientCode);
                ShowWarning(message);
                return;
            }

            var clientPackingPlaceCode = clientPackingPlace.CPVValue;

            // проверяем, что такое место существует и является местом упаковки
            Place packingPlace;
            using (var mgr = IoC.Instance.Resolve<IBaseManager<Place>>())
                packingPlace = mgr.Get(clientPackingPlaceCode);

            if (packingPlace == null)
            {
                var message = string.Format("Не найдено место с кодом {0}", clientPackingPlaceCode);
                ShowWarning(message);
                return;
            }

            if (packingPlace.CustomParamVal == null)
            {
                var message = string.Format("Для места с кодом {0} не указаны параметры", clientPackingPlaceCode);
                ShowWarning(message);
                return;
            }

            var isPackingCpv = packingPlace.CustomParamVal.FirstOrDefault(i => i.CustomParamCode == "PLACETagL1" && i.CPVValue == "PACK");
            if (isPackingCpv == null)
            {
                var message = string.Format("Для места с кодом '{0}' признак места-упаковки не установлен", clientPackingPlaceCode);
                ShowWarning(message);
                return;
            }

            // выставляем место
            CurrentPlaceCode = clientPackingPlaceCode;
        }

        private void OnCurrentPlaceCodeChanged()
        {
            // выставляем новый фильтр для упаковываемых товаров (обновление произойдет автоматически)
            ProductsFilter = string.Format(ProductsOnPlaceFilterTemplate, CurrentPlaceCode);

            //При смене места безусловно обнуляем 'упаковано'
            PackedProducts = null;
            // обновляем доступные короба
            RefreshBoxListAsync();
        }

        private void OnProductFilterChanged()
        {
            //Refresh();
            RefreshPackingProductsAsync();
        }

        private void OnSelectedPackChanged()
        {
            RiseCommandsCanExecuteChanged();
        }

        private void OnActivePackChanged()
        {
            RefreshBoxProductListAsync();
            //RefreshPackedProducts();

            RiseCommandsCanExecuteChanged();
        }

        private SKU[] GetSKULstByBarCode(string barcode)
        {
            // кэшируем получение barcode для быстрых пиков
            return _barcodeCache.GetOrAddSafe(barcode, t =>
            {
                // рассчитываем фильтр
                var filter = string.Format("BARCODEL.BARCODEVALUE='{0}'", barcode);

                // получаем SKU по ШК
                using (var mgr = GetManager<SKU>())
                    return mgr.GetFiltered(filter, SKUPatternForBc.Value).ToArray();
            });
        }

        private void OnPackingProductEditValueChanged()
        {
            // не обрабатываем пустое значение. не ругаемся лишний раз
            if (string.IsNullOrEmpty(PackingProductEditValue))
                return;

            // короб закрыт или не выбран активный
            if (!CanPack())
            {
                PackingProductEditValue = null;
                return;
            }

            // если нечего упаковывать - останавливаем пользователя
            if (VisiblePackingProducts == null || VisiblePackingProducts.Count == 0)
            {
                ShowWarning("В списке на упаковку нет позиций. Попробуйте обновить данные");
                return;
            }

            // ищем SKU по данному ШК
            var skuList = GetSKULstByBarCode(PackingProductEditValue);
            if (skuList.Length == 0)
            {
                var message = string.Format("Не найдено ни одной единицы учета с ШК '{0}'", PackingProductEditValue);
                ShowWarning(message);
                PackingProductEditValue = null;
                return;
            }

            // оставляем только те SKU, по которым есть товар
            var existSkuList = skuList.Where(i => VisiblePackingProducts.Any(j => j.SKUID.Equals(i.SKUID))).ToArray();

            // если много - выходим
            if (existSkuList.Length > 1)
            {
                var items = string.Join(",", existSkuList.Select(i => i.SKUName));
                var message = string.Format("Среди упаковываемого товара найдено более одной единицы учета с ШК '{0}': {1}", PackingProductEditValue, items);
                ShowWarning(message);
                PackingProductEditValue = null;
                return;
            }

            // если один - он нам и нужен
            decimal? targetSkuId = null;
            string targetArt = null;
            if (existSkuList.Length == 1)
            {
                targetSkuId = existSkuList[0].SKUID;
                targetArt = existSkuList[0].ArtCode;
            }
            else
            {
                // если по SKU ничего не нашли - ищем по артикулу
                var productsByArt = VisiblePackingProducts.Where(i => skuList.Any(j => j.ArtCode == i.ArtCode_R)).ToArray();

                // если ничего не нашли и по артикулу - ошибка
                if (productsByArt.Length == 0)
                {
                    var items = string.Join(",", skuList.Select(i => i.ArtCode).Distinct());
                    var message =
                        string.Format(
                            "Среди упаковываемого товара по ШК '{0}' не найдено ни одного с артикулом(ами): {1}",
                            PackingProductEditValue, items);
                    ShowWarning(message);
                    PackingProductEditValue = null;
                    return;
                }

                // если больше одного артикула - ошибка
                if (productsByArt.Select(i => i.ArtCode_R).Distinct().Count() > 1)
                {
                    var items = string.Join(",", productsByArt.Select(i => i.ArtCode_R).Distinct());
                    var message =
                        string.Format(
                            "Среди упаковываемого товара по ШК '{0}' найдено более одного артикула: {1}",
                            PackingProductEditValue, items);
                    ShowWarning(message);
                    PackingProductEditValue = null;
                    return;
                }

                // если по ШК нашли несколько SKU для одного артикула
                var artSkuList = skuList.Where(i => i.ArtCode == productsByArt[0].ArtCode_R).ToArray();
                if (artSkuList.Count() > 1)
                {
                    var message =
                        string.Format(
                            "Для артикула '{0}' найдено более одной единицы учета по ШК '{1}': {2}",
                            productsByArt[0].ArtCode_R, PackingProductEditValue, string.Join(",", artSkuList.Select(i => i.SKUName)));
                    ShowWarning(message);
                    PackingProductEditValue = null;
                    return;
                }
                // выбираем найденный в системе SKU по указанному ШК с найденным артикулом
                targetSkuId = artSkuList[0].SKUID;
                targetArt = productsByArt[0].ArtCode_R;
            }

            // ищем торары

            //оставляем только те, что есть в упаковываемом товаре
            //var existSkuList = skuList.Where(i => VisiblePackingProducts.Any(j => j.SKUID.Equals(i.SKUID))).ToArray();

            //// ничего не осталось
            //if (existSkuList.Length == 0)
            //{
            //    var items = string.Join(",", skuList.Select(i => i.SKUName));
            //    var message = string.Format("Среди упаковываемого товара не найдено ни одного с единицой(ами) учета '{0}'", items);
            //    ShowWarning(message);
            //    PackingProductEditValue = null;
            //    return;
            //}

            //// осталось более 1-ой
            //if (existSkuList.Length > 1)
            //{
            //    var items = string.Join(",", existSkuList.Select(i => i.SKUName));
            //    var message = string.Format("Среди упаковываемого товара найдено более одной единицы учета с ШК '{0}': {1}", PackingProductEditValue, items);
            //    ShowWarning(message);
            //    PackingProductEditValue = null;
            //    return;
            //}

            //var products = VisiblePackingProducts.Where(i => i.SKUID.Equals(existSkuList[0].SKUID)).ToArray();
            //Отбираем товар с SKU артикула
            var products = VisiblePackingProducts.Where(i => i.ArtCode_R == targetArt).ToArray();
            // сюда не должны попасть (мы уже отобрали только нужные SKU)
            if (products.Length == 0)
            {
                var message = string.Format("Разработчику: в первоначальной выборке упаковываемого товара был товар с артикулом {0}, а в финальной исчез", targetArt);
                ShowWarning(message);
                PackingProductEditValue = null;
                return;
            }

            // выделяем найденые строки
            //SelectedPackingProducts = new ObservableCollection<Product>(products);

            // и запускаем процесс упаковки
            Pack(targetSkuId, true, products, false);

            // сбрасываем введенное значение
            PackingProductEditValue = null;
        }

        private void CreateMenu()
        {
            InitializeCustomizationBar();

            var barCommands = Menu.GetOrCreateBarItem(StringResources.Commands, 1, "BarItemCommands");
            barCommands.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.RefreshData,
                Command = RefreshCommand,
                ImageSmall = ImageResources.DCLFilterRefresh16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLFilterRefresh32.GetBitmapImage(),
                HotKey = new KeyGesture(Key.F5),
                //GlyphSize = GlyphSizeType.Large,
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 50
            });

            _fullScreenModeMenuItem = new CommandMenuItem
            {
                Caption = StringResources.ExpandPanel,
                Command = FullScreenModeCommand,
                ImageSmall = ImageResources.DCLExpandPack16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLExpandPack32.GetBitmapImage(),
                HotKey = new KeyGesture(Key.F12),
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 60
            };
            barCommands.MenuItems.Add(_fullScreenModeMenuItem);

            var packingBar = Menu.GetOrCreateBarItem(PackingWord, 2);
            var miCreatePack = new CommandMenuItem
            {
                Caption = StringResources.Create,
                Command = CreatePackCommand,
                ImageSmall = ImageResources.DCLCreatePack16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLCreatePack32.GetBitmapImage(),
                HotKey = new KeyGesture(Key.F7),
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 110
            };
            packingBar.MenuItems.Add(miCreatePack);

            var miOpenPack = new CommandMenuItem
            {
                Caption = StringResources.Open,
                Command = OpenPackCommand,
                ImageSmall = ImageResources.DCLOpenPack16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLOpenPack32.GetBitmapImage(),
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 120
            };
            packingBar.MenuItems.Add(miOpenPack);

            var miClosePack = new CommandMenuItem
            {
                Caption = StringResources.Close,
                Command = ClosePackCommand,
                ImageSmall = ImageResources.DCLClosePack16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLClosePack32.GetBitmapImage(),
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 130
            };
            packingBar.MenuItems.Add(miClosePack);

            var miSetWeight = new CommandMenuItem
            {
                Caption = StringResources.Weight,
                Command = SetWeightCommand,
                ImageSmall = ImageResources.DCLPackWeight16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLPackWeight32.GetBitmapImage(),
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 140
            };
            packingBar.MenuItems.Add(miSetWeight);

            var miDeletePack = new CommandMenuItem
            {
                Caption = StringResources.Delete,
                Command = DeletePackCommand,
                ImageSmall = ImageResources.DCLPackDelete16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLPackDelete32.GetBitmapImage(),
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 150
            };
            packingBar.MenuItems.Add(miDeletePack);

            var miPutIn = new CommandMenuItem
            {
                Caption = StringResources.EmbededTeToTe,
                Command = PutInCommand,
                ImageSmall = ImageResources.DCLPutIn16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLPutIn32.GetBitmapImage(),
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 160
            };
            packingBar.MenuItems.Add(miPutIn);

            var printBar = Menu.GetOrCreateBarItem(StringResources.Printable, 3, "BarItemActionPrint");

            var miPrint = new CommandMenuItem
            {
                IsEnable = true,
                Caption = StringResources.ActionPrint,
                ImageSmall = ImageResources.DCLPrintAndPreview16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLPrintAndPreview32.GetBitmapImage(),
                Command = PrintCommand,
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 10,
                HotKey = new KeyGesture(Key.P, ModifierKeys.Control)
            };
            printBar.MenuItems.Add(miPrint);
        }

        protected override void SetupCustomizeMenu(BarItem bar, ListMenuItem listmenu)
        {
            base.SetupCustomizeMenu(bar, listmenu);

            //if (AppearanceStyleCommand == null)
            //    AppearanceStyleCommand = new DelegateCustomCommand(OnAppearanceStyle, CanAppearanceStyle);

            //// Добавляем функционал подсветки (в начало списка)
            //var minPriority = listmenu.MenuItems.Min(i => i.Priority);
            //listmenu.MenuItems.Add(new CommandMenuItem
            //{
            //    Caption = StringResources.AppearanceStyle,
            //    Command = AppearanceStyleCommand,
            //    ImageSmall = ImageResources.DCLAppearanceStyle16.GetBitmapImage(),
            //    ImageLarge = ImageResources.DCLAppearanceStyle32.GetBitmapImage(),
            //    GlyphAlignment = GlyphAlignmentType.Top,
            //    DisplayMode = DisplayModeType.Default,
            //    Priority = minPriority - 100
            //});

            // убираем функционал настройки самого layout-а
            var item = listmenu.MenuItems.FirstOrDefault(i => i.Caption == StringResources.CustomizeRegion);
            if (item != null)
                listmenu.MenuItems.Remove(item);
        }

        private void CreateCommands()
        {
            CreatePackCommand = new DelegateCustomCommand(CreatePack);
            OpenPackCommand = new DelegateCustomCommand(OpenBox, CanOpenBox);
            ClosePackCommand = new DelegateCustomCommand(CloseBox, CanCloseBox);
            SetWeightCommand = new DelegateCustomCommand(SetWeight, CanSetWeight);
            DeletePackCommand = new DelegateCustomCommand(DeletePack, CanDeletePack);
            PutInCommand = new DelegateCustomCommand(PutIn);

            RefreshCommand = new DelegateCustomCommand(Refresh);
            FullScreenModeCommand = new DelegateCustomCommand(FullScreenModeChange);

            PackCommand = new DelegateCustomCommand(Pack, CanPack);
            PackAllCommand = new DelegateCustomCommand(PackAll, CanPack);
            UnpackAllCommand = new DelegateCustomCommand(UnpackAll, CanUnpackAll);

            MoveToOtherTECommand = new DelegateCustomCommand(MoveTo, CanMove);
            MoveAllToOtherTECommand = new DelegateCustomCommand(MoveAllTo, CanMove);

            PackSourceTECommand = new DelegateCustomCommand(PackSourceTE, CanPackSourceTE);
            ReturnOnSourceTECommand = new DelegateCustomCommand(ReturnOnSourceTE, CanReturnOnSourceTE);

            SetActivePackCommand = new DelegateCustomCommand(SetActivePack);

            PrintCommand = new DelegateCustomCommand(PrintReport, CanPrintReport);

            Commands.AddRange(new[]
            {
                CreatePackCommand, OpenPackCommand, ClosePackCommand, SetWeightCommand, DeletePackCommand, RefreshCommand, PackCommand, 
                PackAllCommand, UnpackAllCommand, 
                MoveToOtherTECommand, MoveAllToOtherTECommand, PrintCommand, FullScreenModeCommand, PackSourceTECommand, ReturnOnSourceTECommand
            });
        }

        private void FullScreenModeChange()
        {
            GetViewService().MakeMaxSize(this, _isMaxView);
            _isMaxView = !_isMaxView;

            _fullScreenModeMenuItem.ImageSmall = _isMaxView ? ImageResources.DCLCollapsePack16.GetBitmapImage() : ImageResources.DCLExpandPack16.GetBitmapImage();
            _fullScreenModeMenuItem.ImageLarge = _isMaxView ? ImageResources.DCLCollapsePack32.GetBitmapImage() : ImageResources.DCLExpandPack32.GetBitmapImage();
            _fullScreenModeMenuItem.Caption = _isMaxView ? StringResources.CollapsePanel : StringResources.ExpandPanel;
        }

        private async void Refresh()
        {
            _packProcessCount = 0;
            _barcodeCache.Clear();

            RefreshBoxListAsync();
            RefreshPackingProductsAsync();
        }

        private async void RefreshPackingProductsAsync()
        {
            // если нет фильтра по товару, то ничего не получаем
            if (string.IsNullOrEmpty(ProductsFilter))
                return;

            WaitStart();
            try
            {
                await Task.Factory.StartNew(() =>
                {
                    // запоминаем выбранные позиции
                    var selectedKeys = SelectedPackingProducts
                        .Select(i => i.ProductId)
                        .Distinct()
                        .ToArray();

                    // получаем данные
                    using (var mgr = GetManager<Product>())
                    {
                        var products = mgr.GetFiltered(ProductsFilter, GetModeEnum.Partial);
                        PackingProducts = new ObservableCollection<Product>(products);
                    }

                    // восстанавливаем выбранные
                    if (PackingProducts != null && PackingProducts.Count > 0 && selectedKeys.Length > 0)
                    {
                        var selected = PackingProducts.Where(i => selectedKeys.Any(j => j.Equals(i.GetKey())));
                        DispatcherHelper.Invoke(new Action(() => SelectedPackingProducts.Clear()));
                        DispatcherHelper.Invoke(new Action(() => SelectedPackingProducts.AddRange(selected)));
                    }
                });
            }
            finally
            {
                WaitStop();
            }
        }

        private async void RefreshSpecifiedPackingProductsAsync(Product[] packingItems)
        {
            WaitStart();
            try
            {
                await Task.Factory.StartNew(() =>
                {
                    var keys = packingItems.Select(i => i.ProductId).Distinct();
                    // получаем данные
                    using (var mgr = IoC.Instance.Resolve<IBaseManager<Product>>())
                    {
                        // получаем обратно только данные, которые передавали
                        var filter = FilterHelper.GetFilterIn(Product.ProductIdPropertyName, keys.Cast<object>());
                        filter += "AND " + ProductsFilter;
                        var products = mgr.GetFiltered(filter, GetModeEnum.Partial).ToArray();

                        DispatcherHelper.Invoke(new Action(() =>
                        {
                            // бежим по товарам, которые хотели упаковывать
                            // если нет в обновленном списке - удаляем из нашего
                            foreach (var planItem in packingItems.Where(i => products.All(j => j.ProductId != i.ProductId)))
                            {
                                var listItem = PackingProducts.FirstOrDefault(i => i.ProductId == planItem.ProductId);
                                // если элемента нет - это означает, что его удалили в другом потоке
                                if (listItem == null)
                                    continue;

                                PackingProducts.Remove(listItem);
                                SelectedPackingProducts.Remove(listItem);
                                VisiblePackingProducts.Remove(listItem);
                            }

                            // появиться в новом списке записи не могут
                            // смотрим то, что обновилось
                            foreach (var planItem in packingItems.Where(i => products.Any(j => j.ProductId == i.ProductId)))
                            {
                                var listItem = PackingProducts.FirstOrDefault(i => i.ProductId == planItem.ProductId);
                                // если элемента нет - это означает, что его удалили в другом потоке
                                if (listItem == null)
                                    continue;

                                var idx = PackingProducts.IndexOf(listItem);
                                if (idx == -1)
                                    continue;

                                var newProduct = products.FirstOrDefault(i => i.ProductId == planItem.ProductId);
                                PackingProducts[idx] = newProduct;

                                // эти же объекты могли быть выбраны - обновим и список выбора
                                var selectedIdx = SelectedPackingProducts.IndexOf(listItem);
                                if (selectedIdx != -1)
                                    SelectedPackingProducts[selectedIdx] = newProduct;

                                // эти же объекты могли быть отображены
                                var visibleIdx = VisiblePackingProducts.IndexOf(listItem);
                                if (visibleIdx != -1)
                                    VisiblePackingProducts[visibleIdx] = newProduct;
                            }
                        }));
                    }
                });
            }
            finally
            {
                WaitStop();
            }
        }

        private async void RefreshBoxListAsync()
        {
            if (CurrentPlaceCode == null)
            {
                Hide = true;
                return;
            }

            WaitStart();
            try
            {
                await Task.Factory.StartNew(() =>
                {
                    // запомнинаем ключи
                    var selectedKey = SelectedPack == null ? null : SelectedPack.GetKey();
                    var activeKey = ActivePack == null ? null : ActivePack.GetKey();

                    using (var mgr = (ITEManager)GetManager<TE>())
                    {
                        var items = mgr.GetPackingTEOnPlace(CurrentPlaceCode);
                        AvailableTE = new ObservableCollection<TE>(items);
                    }

                    if (AvailableTE == null || AvailableTE.Count == 0)
                        return;

                    // восстанавиливаем элементы
                    if (selectedKey != null)
                        SelectedPack = AvailableTE.FirstOrDefault(i => selectedKey.Equals(i.GetKey()));

                    if (activeKey != null)
                        ActivePack = AvailableTE.FirstOrDefault(i => activeKey.Equals(i.GetKey()));
                });
            }
            finally
            {
                WaitStop();
            }
        }

        private async void RefreshOneBoxAsync(TE box)
        {
            if (box == null)
                return;

            var isActive = box == ActivePack;
            var isSelected = box == SelectedPack;
            var idx = AvailableTE.IndexOf(box);

            // такие чудеса тоже бывают
            if (idx == -1)
                return;

            try
            {

                WaitStart();

                await Task.Factory.StartNew(() =>
                {
                    using (var mgr = GetManager<TE>())
                    {
                        if (HaveChild)
                        // Обновляем вложенные ТЕ
                        {
                            var filter = string.Format("{0} in (select wmste.tecode from wmste start with (TECarrierStreakCode = '{1}') connect by prior TECode = TECarrierStreakCode)",
                                        SourceNameHelper.Instance.GetPropertySourceName(typeof(Product), TE.TECodePropertyName),
                                        box.TECode);
                            var items = mgr.GetFiltered(filter, GetModeEnum.Partial).ToList();
                            foreach (var i in items)
                            {
                                var el = AvailableTE.FirstOrDefault(x => x.TECode.Equals(i.TECode));
                                if (el == null) 
                                    continue;
                                AvailableTE[AvailableTE.IndexOf(el)] = i;
                            }
                        }

                        var item = mgr.Get(box.TECode, GetModeEnum.Partial);

                        if (item != null)
                        {
                            DispatcherHelper.Invoke(new Action(() =>
                            {
                                AvailableTE[idx] = item;

                                if (isSelected)
                                    SelectedPack = item;

                                if (isActive)
                                    ActivePack = item;
                            }));
                        }
                    }
                });
            }
            finally
            {
                WaitStop();
            }
        }

        private async void RefreshBoxProductListAsync()
        {
            // если нет активной упаковки, ничего не показываем
            if (ActivePack == null)
            {
                PackedProducts = null;
                return;
            }

            WaitStart();
            try
            {
                DispatcherHelper.Invoke(new Action(() => SelectedPackedProducts.Clear()));

                // а для активной - получаем товары
                using (var mgr = (IProductManager)GetManager<Product>())
                {
                    var items = HaveChild ? mgr.GetByTECode(ActivePack.TECode, true) : mgr.GetByTECode(ActivePack.TECode);
                    PackedProducts = new ObservableCollection<Product>(items);
                }

                LastPackedProduct = PackedProducts.OrderByDescending(p => p.DateUpd).FirstOrDefault();
            }
            finally
            {
                WaitStop();
            }
        }
        /*
                [Obsolete]
                private void RefreshAvailableTE()
                {
                    if (CurrentPlaceCode == null)
                    {
                        Hide = true;
                        return;
                    }

                    // запомнинаем ключи
                    var selectedKey = SelectedPack == null ? null : SelectedPack.GetKey();
                    var activeKey = ActivePack == null ? null : ActivePack.GetKey();

                    using (var mgr = (ITEManager) IoC.Instance.Resolve<IBaseManager<TE>>())
                    {
                        var items = mgr.GetPackingTEOnPlace(CurrentPlaceCode);
                        AvailableTE = new ObservableCollection<TE>(items);
                    }

                    if (AvailableTE == null || AvailableTE.Count == 0)
                        return;

                    // восстанавиливаем элементы
                    if (selectedKey != null)
                        SelectedPack = AvailableTE.FirstOrDefault(i => selectedKey.Equals(i.GetKey()));

                    if (activeKey != null)
                        ActivePack = AvailableTE.FirstOrDefault(i => activeKey.Equals(i.GetKey()));
                }

                [Obsolete]
                private void RefreshAllProducts()
                {
                    using (var mgr = IoC.Instance.Resolve<IBaseManager<Product>>())
                        FilteredProducts =
                            new ObservableCollection<Product>(mgr.GetFiltered(ProductsFilter, GetModeEnum.Partial));

                    RefreshPackingProducts();
                    RefreshPackedProducts();
                }

                [Obsolete]
                private void RefreshPackingProducts()
                {
                    // запоминаем выбранные позиции
                    var selectedKeys = SelectedPackingProducts
                        .Select(i => i.ProductId)
                        .Distinct()
                        .ToArray();

                    DispatcherHelper.Invoke(new Action(() => SelectedPackingProducts.Clear()));
                    if (FilteredProducts == null || FilteredProducts.Count == 0)
                    {
                        PackingProducts = null;
                        VisiblePackingProducts.Clear();
                        return;
                    }

                    // получаем список ТЕ-шек, которых у нас нет на месте
                    var nonPackedTECodes =
                        FilteredProducts.Select(i => i.TECode).Distinct().Where(teCode => AvailableTE.All(j => j.TECode != teCode));

                    // проверяем по БД действительно ли они не являются упаковками
                    var filters = wmsMLC.General.BL.Helpers.FilterHelper.GetArrayFilterIn("TECODE", nonPackedTECodes,
                        string.Format(" AND (TECURRENTPLACE <> '{0}' OR NVL(TEPACKSTATUS,' ') != 'TE_PKG_NONE')",
                            CurrentPlaceCode));
                    var packedOnNotThisPlaceTE = new List<TE>();
                    using (var mgr = IoC.Instance.Resolve<IBaseManager<TE>>())
                    {
                        foreach (var f in filters)
                        {
                            packedOnNotThisPlaceTE.AddRange(mgr.GetFiltered(f));
                        }
                    }

                    // получаем из общего списка только те продукты, которых нет в упаковках (текущее место и другие)
                    var items =
                        FilteredProducts.Where(
                            i =>
                                AvailableTE.All(j => j.TECode != i.TECode) &&
                                packedOnNotThisPlaceTE.All(j => j.TECode != i.TECode));
                    PackingProducts = new ObservableCollection<Product>(items);

                    // восстанавливаем выбранные
                    if (PackingProducts != null)
                    {
                        var selected = PackingProducts.Where(i => selectedKeys.Any(j => j.Equals(i.GetKey())));
                        DispatcherHelper.Invoke(new Action(() => SelectedPackingProducts.Clear()));
                        DispatcherHelper.Invoke(new Action(() => SelectedPackingProducts.AddRange(selected)));
                    }
                }

                [Obsolete]
                private void RefreshPackedProducts()
                {
                    DispatcherHelper.Invoke(new Action(() => SelectedPackedProducts.Clear()));
                    // если нет активной упаковки, ничего не показываем
                    if (ActivePack == null)
                    {
                        PackedProducts = null;
                        return;
                    }

                    // а для активной - получаем товары
                    using (var mgr = IoC.Instance.Resolve<IBaseManager<Product>>())
                    {
                        var items = ((IProductManager)mgr).GetByTECode(ActivePack.TECode, GetModeEnum.Partial);
                        PackedProducts = new ObservableCollection<Product>(items);
                    }
                    LastPackedProduct = PackedProducts.OrderByDescending(p => p.GetProperty("DATEUPD")).FirstOrDefault();
                }
        */
        #region .  Commands  .

        public bool CanMove()
        {
            return ActivePack != null &&
                   ActivePack.TEPackStatus != TEPackStatus.TE_PKG_COMPLETED &&
                   SelectedPack != ActivePack &&
                   SelectedPackedProducts != null &&
                   SelectedPackedProducts.Count > 0 &&
                   !HaveChild &&
                   SelectedPackedProducts.All(t => t.StatusCode_R.Equals(ProductStates.PRODUCT_BUSY.ToString()));
        }

        public async void MoveTo()
        {
            if (!SelectedPackedProducts.All(t => t.StatusCode_R.Equals(ProductStates.PRODUCT_BUSY.ToString())))
            {
                ShowWarning("Все перемещаемые товары должны быть в статусе " + ProductStates.PRODUCT_BUSY);
                return;
            }

            var preferPack = SelectedPack;

            if (SelectedPack == ActivePack)
                preferPack = null;

            if (SelectedPack != null && SelectedPack.TEPackStatus == TEPackStatus.TE_PKG_COMPLETED)
                preferPack = null;

            try
            {
                WaitStart();

                // запускаем упаковку
                await Task.Factory.StartNew(() =>
                {
                    try
                    {
                        var packer = new Packer();
                        packer.Move(AvailableTE.ToArray(), preferPack == null ? null : preferPack.TECode, SelectedPackedProducts.ToArray(), CurrentPlaceCode);
                    }
                    catch (Exception ex)
                    {
                        _log.Error("Ошибка при упаковке корба целиком", ex);
                        var message = ExceptionHelper.ExceptionToString(ex);
                        ShowWarning(message);
                    }
                });
            }
            finally
            {
                WaitStop();
            }

            // обновляем все
            DispatcherHelper.Invoke(new Action(() =>
            {
                // обновляем короб
                RefreshOneBoxAsync(ActivePack);

                // обновляем короб
                RefreshOneBoxAsync(SelectedPack);

                // обновляем список товаров в коробе
                RefreshBoxProductListAsync();
            }));
        }

        private void MoveAllTo()
        {
            SelectedPackedProducts.Clear();
            SelectedPackedProducts.AddRange(PackedProducts);
            MoveTo();
        }

        public bool CanPack()
        {
            return (ActivePack != null &&
                   ActivePack.TEPackStatus != TEPackStatus.TE_PKG_COMPLETED) &&
                   (!string.IsNullOrEmpty(PackingProductEditValue) || (SelectedPackingProducts != null &&
                   SelectedPackingProducts.Count > 0));
        }

        public void Pack(bool isAllPack)
        {
            Pack(null, false, SelectedPackingProducts.ToArray(), isAllPack);
        }

        public void Pack()
        {
            Pack(null, false, SelectedPackingProducts.ToArray(), false);
        }

        private async void Pack(decimal? startSkuId, bool isByBarcode, Product[] products, bool isAllPack)
        {
            if (ActivePack == null)
            {
                ShowWarning("Не выбран активный короб");
                return;
            }

            if (products == null || products.Length == 0)
            {
                ShowWarning("Не определены упаковываемые товары");
                return;
            }

            // накапливаем список товаров, который нужно будет обновлять
            _productForRefresh.AddRange(products);
            var isUnpack = false;
            var startSkuIdInternal = startSkuId;

            try
            {
                WaitStart();

                // увеличиваем счетчик входов в упаковку
                _packProcessCount++;

                // запускаем упаковку
                await Task.Factory.StartNew(() =>
                {
                    try
                    {
                        if (!startSkuIdInternal.HasValue && products.Length == 1)
                            startSkuIdInternal = products[0].SKUID;
                        if (_skuIdOld != startSkuIdInternal)
                            _storedProductCountSku = null;

                        _skuIdOld = startSkuIdInternal;
                        var packer = new Packer();
                        packer.Pack(products.ToArray(), ActivePack.TECode, startSkuId, isByBarcode, false, ref isUnpack, isAllPack, ref _storedProductCountSku);
                        _packStatus = true;
                    }
                    catch (Exception ex)
                    {
                        _log.Error("Ошибка при упаковке", ex);
                        var message = ex.Message;
                        ShowWarning(message);
                    }
                });
            }
            finally
            {
                WaitStop();
            }

            // если еще кто-то упаковывается
            _packProcessCount--;
            if (_packProcessCount > 0)
                return;

            if (isUnpack)
            {
                DispatcherHelper.Invoke(new Action(RefreshPackingProductsAsync));
                return;
            }

            DispatcherHelper.Invoke(new Action(() =>
            {
                // обновляем короб
                RefreshOneBoxAsync(ActivePack);

                // обновляем сприсок товаров на упаковку
                //RefreshPackingProductsAsync();

                // выбираем уникальные записи из списка заявленых на обновлени и обновляем
                var refreshingItems = _productForRefresh.DistinctBy(i => i.ProductId).ToArray();
                // очищаем список для следующих подходов
                _productForRefresh.Clear();
                // обновляем
                RefreshSpecifiedPackingProductsAsync(refreshingItems);

                // обновляем список товаров в коробе
                RefreshBoxProductListAsync();
            }));
        }

        private bool CanPackSourceTE()
        {
            return SelectedPackingProducts != null &&
                   SelectedPackingProducts.Count > 0;
        }

        private async void PackSourceTE()
        {
            if (SelectedPackingProducts == null || SelectedPackingProducts.Count == 0)
            {
                var message = "Не выбран ни один товар";
                ShowWarning(message);
                return;
            }

            var teCodeList = SelectedPackingProducts.Select(i => i.TECode).Distinct().ToArray();

            try
            {
                WaitStart();

                // запускаем упаковку
                await Task.Factory.StartNew(() =>
                {
                    try
                    {
                        var act = new Packer();
                        act.PackSourceTE(teCodeList, CurrentPlaceCode);
                    }
                    catch (Exception ex)
                    {
                        _log.Error("Ошибка при упаковке короба целиком", ex);
                        var message = ExceptionHelper.ExceptionToString(ex);
                        ShowWarning(message);
                    }
                });
            }
            finally
            {
                WaitStop();
            }

            // обновляем все
            DispatcherHelper.Invoke(new Action(() =>
            {
                RefreshBoxListAsync();
                RefreshPackingProductsAsync();

                // выставляем активный короб
                var selected = AvailableTE.Where(i => teCodeList.Any(j => j == i.TECode)).ToArray();
                if (selected.Length > 0)
                {
                    SelectedPacks = new ObservableCollection<TE>(selected);
                    SelectedPack = ActivePack = SelectedPacks[0];
                }
            }));
        }

        public bool CanReturnOnSourceTE()
        {
            return ActivePack != null &&
                   ActivePack.TEPackStatus != TEPackStatus.TE_PKG_COMPLETED &&
                   SelectedPackedProducts != null &&
                   SelectedPackedProducts.Count > 0;
        }

        public async void ReturnOnSourceTE()
        {
            if (!CanReturnOnSourceTE())
                return;

            if (string.IsNullOrEmpty(CurrentPlaceCode))
                throw new OperationException("Не указано текущее место");

            try
            {
                WaitStart();

                await Task.Factory.StartNew(() =>
                {
                    try
                    {
                        using (var mgr = GetSpecManager<IBPProcessManager>())
                            mgr.ReturnOnSourceTE(SelectedPackedProducts.Select(i => i.ProductId.Value), CurrentPlaceCode);
                    }
                    catch (Exception ex)
                    {
                        var message = string.Format("Не удалось переместить товар на исходную ТЕ!{0}{1}", Environment.NewLine, ExceptionHelper.GetErrorMessage(ex));
                        _log.Error(message, ex);
                        ShowWarning(message);
                    }
                });
            }
            finally
            {
                WaitStop();
            }

            // обновляем результаты
            DispatcherHelper.BeginInvoke(new Action(() =>
            {
                RefreshOneBoxAsync(SelectedPack);
                RefreshPackingProductsAsync();
                RefreshBoxProductListAsync();
                RiseCommandsCanExecuteChanged();
            }));
        }

        private void PackAll()
        {
            SelectedPackingProducts.Clear();
            SelectedPackingProducts.AddRange(VisiblePackingProducts);
            Pack(true);
        }

        private bool CanUnpackAll()
        {
            return false;
            //Задача http://mp-ts-nwms/issue/wmsMLC-6304 в suspend'е
            //return ActivePack != null &&
            //       SelectedPack == ActivePack &&
            //       SelectedPackedProducts != null &&
            //       SelectedPackedProducts.Count > 0;
        }
        private async void UnpackAll()
        {
            if (!CanUnpackAll())
                return;

            try
            {
                WaitStart();

                await Task.Factory.StartNew(() =>
                {
                    var packer = new Packer();
                    packer.UnpackAll(SelectedPack.TECode, CurrentPlaceCode);
                });

                DispatcherHelper.BeginInvoke(
                    new Action(() =>
                    {
                        Refresh();
                        RiseCommandsCanExecuteChanged();
                    }));
            }
            finally
            {
                WaitStop();
            }
        }

        private bool CanSetWeight()
        {
            return false;
        }

        private void SetWeight()
        {
            throw new NotImplementedException();
        }

        private bool CanCloseBox()
        {
            if (SelectedPack == null)
                return false;

            return SelectedPack.TEPackStatus == TEPackStatus.TE_PKG_ACTIVATED;
        }

        private async void CloseBox()
        {
            if (SelectedPack == null)
            {
                ShowWarning("Короб не выбран");
                return;
            }

            if (SelectedPack.TEPackStatus != TEPackStatus.TE_PKG_ACTIVATED)
            {
                ShowWarning("Ошибочный статус упаковки " + SelectedPack.TECode);
                return;
            }

            try
            {
                WaitStart();

                await Task.Factory.StartNew(() =>
                {
                    try
                    {
                        var act = new Packer();
                        act.ClosePack(SelectedPack);
                    }
                    catch (Exception ex)
                    {
                        _log.Error("Ошибка при закрытии короба", ex);
                        var message = ExceptionHelper.ExceptionToString(ex);
                        ShowWarning(message);
                    }
                });
            }
            finally
            {
                WaitStop();
            }

            DispatcherHelper.BeginInvoke(
                new Action(() =>
                {
                    RefreshOneBoxAsync(SelectedPack);
                    RiseCommandsCanExecuteChanged();
                }));
        }

        private bool CanOpenBox()
        {
            if (SelectedPack == null)
                return false;

            return SelectedPack.TEPackStatus == TEPackStatus.TE_PKG_COMPLETED;
        }

        private async void OpenBox()
        {
            try
            {
                WaitStart();

                await Task.Factory.StartNew(() =>
                {
                    var packer = new Packer();
                    packer.OpenPack(SelectedPack.TECode, CurrentPlaceCode, "OP_PACKING_OPEN");
                });

                DispatcherHelper.BeginInvoke(
                    new Action(() =>
                    {
                        RefreshOneBoxAsync(SelectedPack);
                        RiseCommandsCanExecuteChanged();
                    }));
            }
            catch (Exception ex)
            {
                _log.Error("Ошибка при открытии короба", ex);
                var message = ExceptionHelper.GetFirstMeaningException(ex).Message;
                ShowWarning(message);
            }
            finally
            {
                WaitStop();
            }
        }

        private void CreatePack()
        {
            WaitStart();
            using (var mgr = IoC.Instance.Resolve<IBPProcessManager>())
            {
                var context = new BpContext();
                context.Set("CURRENTPLACE", CurrentPlaceCode);
                mgr.Parameters.Add(BpContext.BpContextArgumentName, context);
                mgr.Run(code: "CREATEBOX", completedHandler: ctx => CreatePackProcessComplete(context));
            }
        }

        private void CreatePackProcessComplete(BpContext context)
        {
            DispatcherHelper.BeginInvoke(
                new Action(() =>
                {
                    var box = context.Get<TE>("BOX");
                    if (box != null)
                    {
                        AvailableTE.Insert(0, box);
                        ActivePack = SelectedPack = box;
                    }
                }));
            WaitStop();
        }

        private bool CanDeletePack()
        {
            if (SelectedPack == null)
                return false;

            return true;
        }

        private void DeletePack()
        {
            WaitStart();
            using (var mgr = IoC.Instance.Resolve<IBPProcessManager>())
            {
                var context = new BpContext();
                mgr.Parameters.Add(BpContext.BpContextArgumentName, context);
                context.Items = SelectedPacks.ToArray();
                mgr.Run(code: "DELETEPACK", completedHandler: ctx => DeletePackProcessComplete(context));
            }
        }

        private void DeletePackProcessComplete(BpContext context)
        {
            DispatcherHelper.Invoke(
                new Action(() =>
                {
                    RefreshBoxListAsync();
                    RiseCommandsCanExecuteChanged();
                }));
            WaitStop();
        }

        private void SetActivePack()
        {
            ActivePack = SelectedPack;
        }

        public async void PutIn()
        {
            WaitStart();

            try
            {
                string truckCode = null;
                using (var mgr = GetManager<Client>())
                {
                    var client = mgr.Get(WMSEnvironment.Instance.ClientCode);
                    if (client == null || string.IsNullOrEmpty(client.TruckCode_R))
                    {
                        ShowWarning("Для текущего клиента не определен погрузчик!");
                        return;
                    }
                    truckCode = client.TruckCode_R;
                }

                decimal? workerID;
                using (var mgr = GetManager<Worker>())
                {
                    var workerFilter = string.Format("workerid in (select workerid_r from wmsworking where USERCODE_R = '{0}')", WMSEnvironment.Instance.AuthenticatedUser.GetSignature());
                    var workers = mgr.GetFiltered(workerFilter, FilterHelper.GetAttrEntity<Worker>(new[] {Worker.WorkerIDPropertyName})).ToList();
                    if (!workers.Any())
                    {
                        ShowWarning("Для текущего клиента не привязаны работники!");
                        return;
                    }
                    if (workers.Count > 1)
                    {
                        var key = GetWorker(workerFilter);
                        if (key == null)
                            return;

                        workerID = key;
                    }
                    else
                        workerID = (decimal) workers[0].GetKey();
                }

                // запускаем "Вложить"
                await Task.Factory.StartNew(() =>
                {
                    try
                    {
                        var packer = new Packer();
                        packer.PutIn(CurrentPlaceCode, truckCode, (decimal) workerID);
                    }
                    catch (Exception ex)
                    {
                        _log.Error("Ошибка при выполнении 'Вложить'", ex);
                        var message = ExceptionHelper.ExceptionToString(ex);
                        ShowWarning(message);
                    }
                });
            }
            finally
            {
                WaitStop();
            }

            //Обновляем короба
            DispatcherHelper.BeginInvoke(new Action(() =>
            {
                RefreshBoxListAsync();
                RiseCommandsCanExecuteChanged();
            }));
        }

        private static decimal? GetWorker(string filter)
        {
            var destType = typeof(IListViewModel<Worker>);
            var model = (IObjectListViewModel)IoC.Instance.Resolve(destType, null);
            model.Mode = ObjectListMode.LookUpList3Points;
            model.AllowAddNew = true;
            model.InitializeMenus();

            var modelCapt = model as PanelViewModelBase;
            if (modelCapt != null)
            {
                modelCapt.PanelCaption = "Выберите работника";
                modelCapt.IsActive = true;
            }

            if (!string.IsNullOrEmpty(filter))
                model.ApplyFilter(filter);

            var window = new CustomLookUpOptPopupContent { DataContext = model };

            if (window.Owner == null && Application.Current.MainWindow.IsActive)
                window.Owner = Application.Current.MainWindow;

            if (window.ShowDialog() != true)
                return null;

            var worker = model.SelectedItem as Worker;

            return worker != null ? (decimal?)worker.GetKey<Decimal>() : null;
        }


        private bool CanPrintReport()
        {
            return SelectedPack != null;
        }

        private void PrintReport()
        {
            // Wait не делаем - т.к. внутри будет открыт диалог
            try
            {
                if (!ConnectionManager.Instance.AllowRequest())
                    return;

                if (!CanPrintReport())
                    return;

                var ovm = new PrintViewModel(new object[] { SelectedPack }, typeof(Business.Objects.Packing));

                GetViewService().ShowDialogWindow(ovm, true, true, "40%", "50%");
            }
            catch (Exception ex)
            {
                if (!ExceptionHandler(ex, ExceptionResources.EpsCreateTaskError))
                    throw;
            }
        }

        #endregion

        #endregion

        #region .  IModelHandler  .

        public object GetSource()
        {
            throw new NotImplementedException();
        }

        public void SetSource(object source)
        {
            throw new NotImplementedException();
        }

        public void RefreshData()
        {
            throw new NotImplementedException();
        }

        void IModelHandler.RefreshDataAsync()
        {
            throw new System.NotImplementedException();
        }

        public void RefreshView()
        {
            throw new NotImplementedException();
        }

        public event EventHandler SourceUpdateStarted;
        public event EventHandler SourceUpdateCompleted;
        public event EventHandler RefreshViewEvent;
        public bool IsReadEnable { get; private set; }
        public bool IsEditEnable { get; private set; }
        public bool IsNewEnable { get; private set; }
        public bool IsDelEnable { get; private set; }
        public object ParentViewModelSource { get; set; }
        public SettingDisplay DisplaySetting { get; set; }

        #endregion

        #region .  IHaveUniqueName  .

        public string GetUniqueName()
        {
            return UniqueName;
        }

        #endregion

        #region .  Helpers  .
        private static IBaseManager<T> GetManager<T>(IUnitOfWork uow = null) where T : WMSBusinessObject
        {
            var mgr = IoC.Instance.Resolve<IBaseManager<T>>();
            if (uow != null)
                mgr.SetUnitOfWork(uow);
            return mgr;
        }

        private static T GetSpecManager<T>(IUnitOfWork uow = null)
            where T : ITrueBaseManager
        {
            var mgr = IoC.Instance.Resolve<T>();
            if (uow != null)
                mgr.SetUnitOfWork(uow);
            return mgr;
        }

        private static void ShowWarning(string message)
        {
            GetViewService().ShowDialog("Предупреждение", message, MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
        }
        private static void ShowLogicalError(string message)
        {
            GetViewService().ShowDialog("Ошибка", message, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
        }

        #endregion
    }
}

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.General.ViewModels.Menu;
using wmsMLC.DCL.Main.ViewModels;
using wmsMLC.DCL.Resources;
using wmsMLC.DCL.Topology.ViewModels.Visitors;
using wmsMLC.DCL.Topology.Views;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Helpers;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Services;
using wmsMLC.General.PL.WPF.ViewModels;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Topology.ViewModels
{
    [View(typeof(TopologyView))]
    public class TopologyViewModel : PanelViewModelBase, ITopologyViewModel
    {
        #region .  Fields  .
        private readonly Material _selectedObjectMaterial = new DiffuseMaterial(Brushes.Red);
        private Material _lastObjectMaterial;
        private object _selectedObject;
        private string _description;

        private GridLinesVisual3D _grid;
        private LightSetup _light;
        private Warehouse _selectedWarehouse;
        private ObservableCollection<Warehouse> _warehouses;
        private ObservableCollection<Visual3D> _objects;
        private bool _isFilterVisible;
        private string _placeInformation;
        private bool _showCoordinates;
        private bool _showCameraInfo;
        private bool _showCameraTarget;
        private bool _showFieldOfView;
        private bool _showFrameRate;
        private bool _showTriangleCountInfo;
        private bool _showViewCube;
        private Point3D _currentPosition;
        private bool _isTTShow;
        private bool _isHideCompletedTT;
        private bool _isShowFree;
        private bool _isShowABCD;

        #endregion

        public TopologyViewModel()
        {
            PanelCaption = "Визуализация топологии";
            AllowClosePanel = true;
            ShowCoordinates = true;
            ShowCameraInfo = false;
            ShowCameraTarget = false;
            ShowFieldOfView = false;
            ShowFrameRate = false;
            ShowTriangleCountInfo = false;
            ShowViewCube = true;

            Objects = new ObservableCollection<Visual3D>();
            var whMgr = IoC.Instance.Resolve<IBaseManager<Warehouse>>();
            Warehouses = new ObservableCollection<Warehouse>(whMgr.GetAll());

            RefreshCommand = new DelegateCustomCommand(Refresh);
            ExportCommand = new DelegateCustomCommand(ExportData);
            FilterCommand = new DelegateCustomCommand(ShowFilter);

            ShowCoordinatesCommand = new DelegateCustomCommand(OnShowCoordinates);
            ShowCameraInfoCommand = new DelegateCustomCommand(OnShowCameraInfo);
            ShowCameraTargetCommand = new DelegateCustomCommand(OnShowCameraTarget);
            ShowFieldOfViewCommand = new DelegateCustomCommand(OnShowFieldOfView);
            ShowFrameRateCommand = new DelegateCustomCommand(OnShowFrameRate);
            ShowTriangleCountInfoCommand = new DelegateCustomCommand(OnShowTriangleCountInfo);
            ShowViewCubeCommand = new DelegateCustomCommand(OnShowViewCube);

            FindByArtCommand = new DelegateCustomCommand<object>(FindByArt);
            ShowFreeCommand = new DelegateCustomCommand<object>(ShowFree);
            ShowABCDCommand = new DelegateCustomCommand<object>(ShowABCD);
            ShowTTCommand = new DelegateCustomCommand<object>(ShowTT);
            HideCompletedTTCommand = new DelegateCustomCommand<object>(HideCompletedTT);
            AddMenu();

            Filters = new FilterViewModel();
            Filters.ApplyFilterCommand = RefreshCommand;

            var whField = new DataField();
            whField.FieldName = "VWAREHOUSENAME";
            whField.Caption = "Склад";
            whField.SourceName = "VWAREHOUSENAME";
            whField.FieldType = typeof(string);
            Filters.Fields.Add(whField);

            var areaCodeField = new DataField();
            areaCodeField.FieldName = "VAREACODE";
            areaCodeField.Caption = "Область";
            areaCodeField.SourceName = "VAREACODE";
            areaCodeField.FieldType = typeof(string);
            Filters.Fields.Add(areaCodeField);

            var segmentCodeField = new DataField();
            segmentCodeField.FieldName = "SEGMENTCODE_R_NUMBER";
            segmentCodeField.Caption = "Сектор";
            segmentCodeField.SourceName = "SEGMENTCODE_R_NUMBER";
            segmentCodeField.FieldType = typeof(string);
            Filters.Fields.Add(segmentCodeField);
        }

        #region .  Methods  .

        protected override void InitializeSettings()
        {
            //Используем глобальные настройки вида панели инструментов
            //MenuSuffix = GetType().GetFullNameWithoutVersion();

            base.InitializeSettings();
            IsMenuEnable = true;
            IsCustomizeBarEnabled = true;
        }

        private void OnShowCoordinates()
        {
            ShowCoordinates = !ShowCoordinates;
        }

        private void OnShowCameraInfo()
        {
            ShowCameraInfo = !ShowCameraInfo;
        }

        private void OnShowCameraTarget()
        {
            ShowCameraTarget = !ShowCameraTarget;
        }

        private void OnShowFieldOfView()
        {
            ShowFieldOfView = !ShowFieldOfView;
        }

        private void OnShowFrameRate()
        {
            ShowFrameRate = !ShowFrameRate;
        }

        private void OnShowTriangleCountInfo()
        {
            ShowTriangleCountInfo = !ShowTriangleCountInfo;
        }

        private void OnShowViewCube()
        {
            ShowViewCube = !ShowViewCube;
        }

        private void FindByArt(object parameter)
        {
            var visitor = new FindArtSelectorVisitor((string)parameter);
            foreach (var obj in Objects)
                AcceptVisitor(obj, visitor);
        }

        private void ShowABCD(object parameter)
        {
            var visitor = new ABCDSelectorVisitor((bool)parameter);
            foreach (var obj in Objects)
                AcceptVisitor(obj, visitor);
        }

        private void ShowTT(object parameter)
        {
            var par = (bool)parameter;
            if (!par)
            {
                var ttModels = Objects.OfType<TransportTaskDirectionModel>().ToArray();
                foreach (var model in ttModels)
                {
                    model.ToDefaultFill();
                    Objects.Remove(model);
                }
            }
            else
            {
                var ttMgr = IoC.Instance.Resolve<IBaseManager<TransportTask>>();
                var allTasks = ttMgr.GetAll();

                foreach (var transportTask in allTasks)
                {
                    var model = new TransportTaskDirectionModel(transportTask, FindPlaceAccessor);
                    if (model.IsValid)
                        Objects.Add(model);
                }
            }
        }

        private void HideCompletedTT(object parameter)
        {
            var ttModels = Objects.OfType<TransportTaskDirectionModel>().ToArray();
            var par = (bool)parameter;
            foreach (var model in ttModels)
            {
                if (model.Source.StatusCode == TTaskStates.TTASK_COMPLETED)
                    model.Visible = !par;
            }
        }

        private List<PlaceModel> FindPlaceAccessor(string code)
        {
            var visitor = new FindPlaceVisitor(code);
            foreach (var obj in Objects)
                AcceptVisitor(obj, visitor);
            return visitor.FoundModels;
        }

        private void ShowFree(object parameter)
        {
            var visitor = new PlaceStatesSelectorVisitor((bool)parameter);
            foreach (var obj in Objects)
                AcceptVisitor(obj, visitor);
        }

        private void AcceptVisitor(Visual3D obj, IVisitor visitor)
        {
            var visitorAcceptor = obj as IVisitorAcceptor;
            if (visitorAcceptor != null)
                visitorAcceptor.AcceptVisitor(visitor);

            var childHandler = obj as ModelVisual3D;
            if (childHandler != null)
                foreach (var o in childHandler.Children)
                    AcceptVisitor(o, visitor);
        }

        public void Refresh()
        {
            WaitStart();
            try
            {
                Objects.Clear();
                FillSystemObjects();
                FillObjects();
                CurrentPosition = new Point3D { X = -20, Y = -40, Z = 45 };
            }
            finally
            {
                WaitStop();
            }
        }

        private void ExportData()
        {

        }

        private void ShowFilter()
        {
            IsFilterVisible = !IsFilterVisible;
        }

        private void FillSystemObjects()
        {
            _light = null;
            _light = new DefaultLights();
            Objects.Add(_light);

            _grid = null;
            _grid = new GridLinesVisual3D();
            _grid.Width = 10;
            _grid.Length = 10;
            _grid.MinorDistance = 10;
            _grid.Thickness = 0.1;
            Objects.Add(_grid);

            IsShowFree = false;
            IsShowABCD = false;
            IsTTShow = false;
        }

        private void FillObjects()
        {
            // получаем места из БД
            var places = GetPlaces().ToList();
            if (!places.Any())
                return;

            var segments = GetSegments(places).ToList();
            var areas = GetAreas(segments).ToList();
            var warehouses = GetWarehouses(areas).ToList();
            var warehouseOffset = 0.0;

            foreach (var warehouse in warehouses)
            {
                var warehouseAreas = areas.Where(a => a.Warehouse == warehouse.WarehouseCode).ToList();
                var warehouseSegments = segments.Where(s => warehouseAreas.Any(a => a.AreaCode == s.AreaCode_R)).ToList();
                var warehousePlaces = places.Where(p => warehouseSegments.Any(s => s.SegmentCode == p.SegmentCode)).ToList();
                var warehouseModel = new WarehouseModel(warehouse, warehouseAreas, warehouseSegments, warehousePlaces);
                
                warehouseModel.Transform = new TranslateTransform3D(0, warehouseOffset, 0);
                Objects.Add(warehouseModel);

                warehouseOffset += warehouseModel.Width + 20;
            }

            // рассчитываем размеры сетки
            var maxX = Objects.OfType<BoxVisual3D>().Max(i => i.Center.X);
            var minX = Objects.OfType<BoxVisual3D>().Min(i => i.Center.X);
            var maxY = Objects.OfType<BoxVisual3D>().Max(i => i.Center.Y);
            var minY = Objects.OfType<BoxVisual3D>().Min(i => i.Center.Y);
            _grid.Center = new Point3D((maxX + minX) / 2.0, (maxY + minY) / 2.0, 0);
            _grid.Length = 300;
            _grid.Width = 300;
        }

        private IEnumerable<Place> GetPlaces()
        {
            Filters.RiseFixFilterExpression();
            var mgr = IoC.Instance.Resolve<IBaseManager<Place>>();
            return mgr.GetFiltered(string.Format("(placewidth < 1000000) and (placeposx is not null) and (placeposy is not null) and {0}", Filters.GetSqlExpression()));
        }

        private IEnumerable<Segment> GetSegments(IEnumerable<Place> places)
        {
            var mgr = IoC.Instance.Resolve<IBaseManager<Segment>>();
            return mgr.GetFiltered(string.Format("(segmentposx is not null) and (segmentposy is not null) and {0}", FilterHelper.GetFilterIn(Segment.SegmentCodePropertyName, places.Select(p => p.SegmentCode).Distinct())));
        }

        private IEnumerable<Area> GetAreas(IEnumerable<Segment> segments)
        {
            var mgr = IoC.Instance.Resolve<IBaseManager<Area>>();
            return mgr.GetFiltered(FilterHelper.GetFilterIn(Area.AreaCodePropertyName, segments.Select(s => s.AreaCode_R).Distinct()));
        }

        private IEnumerable<Warehouse> GetWarehouses(IEnumerable<Area> areas)
        {
            var mgr = IoC.Instance.Resolve<IBaseManager<Warehouse>>();
            return mgr.GetFiltered(FilterHelper.GetFilterIn(Warehouse.WarehouseCodePropertyName, areas.Select(p => p.Warehouse).Distinct()));
        }

        private void AddMenu()
        {
            InitializeCustomizationBar();
            var bar = Menu.GetOrCreateBarItem(StringResources.Commands, 1);

            bar.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.Filter,
                Command = FilterCommand,
                ImageSmall = ImageResources.DCLFilter16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLFilter32.GetBitmapImage(),
                HotKey = new KeyGesture(Key.F3),
                //GlyphSize = GlyphSizeType.Large,
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 10
            });

            bar.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.ExportData,
                Command = ExportCommand,
                ImageSmall = ImageResources.DCLFilterRefresh16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLFilterRefresh32.GetBitmapImage(),
                //GlyphSize = GlyphSizeType.Large,
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 10
            });

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
                Priority = 20
            });

            // меню вида
            var listMenuItem = new ListMenuItem
                {
                    Caption = "Вид",
                    ImageSmall = ImageResources.DCLDefault16.GetBitmapImage(),
                    ImageLarge = ImageResources.DCLDefault32.GetBitmapImage(),
                    //GlyphSize = GlyphSizeType.Large,
                    GlyphAlignment = GlyphAlignmentType.Top,
                    DisplayMode = DisplayModeType.Default,
                    Priority = 30
                };
            bar.MenuItems.Add(listMenuItem);
            listMenuItem.MenuItems.Add(new CheckCommandMenuItem
                {
                    Caption = "Координаты",
                    Command = ShowCoordinatesCommand,
                    ImageSmall = ImageResources.DCLDefault16.GetBitmapImage(),
                    ImageLarge = ImageResources.DCLDefault32.GetBitmapImage(),
                    HotKey = new KeyGesture(Key.F6),
                    //GlyphSize = GlyphSizeType.Large,
                    GlyphAlignment = GlyphAlignmentType.Top,
                    DisplayMode = DisplayModeType.Default,
                    Priority = 10
                });
            listMenuItem.MenuItems.Add(new CheckCommandMenuItem
            {
                Caption = "Информация камеры",
                Command = ShowCameraInfoCommand,
                ImageSmall = ImageResources.DCLDefault16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLDefault32.GetBitmapImage(),
                HotKey = new KeyGesture(Key.F7),
                //GlyphSize = GlyphSizeType.Large,
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 20
            });
            listMenuItem.MenuItems.Add(new CheckCommandMenuItem
            {
                Caption = "Цель",
                Command = ShowCameraTargetCommand,
                ImageSmall = ImageResources.DCLDefault16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLDefault32.GetBitmapImage(),
                HotKey = new KeyGesture(Key.F8),
                //GlyphSize = GlyphSizeType.Large,
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 30
            });
            listMenuItem.MenuItems.Add(new CheckCommandMenuItem
            {
                Caption = "Поле вида",
                Command = ShowFieldOfViewCommand,
                ImageSmall = ImageResources.DCLDefault16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLDefault32.GetBitmapImage(),
                HotKey = new KeyGesture(Key.F9),
                //GlyphSize = GlyphSizeType.Large,
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 40
            });
            listMenuItem.MenuItems.Add(new CheckCommandMenuItem
            {
                Caption = "FPS",
                Command = ShowFrameRateCommand,
                ImageSmall = ImageResources.DCLDefault16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLDefault32.GetBitmapImage(),
                HotKey = new KeyGesture(Key.F10),
                //GlyphSize = GlyphSizeType.Large,
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 50
            });
            listMenuItem.MenuItems.Add(new CheckCommandMenuItem
            {
                Caption = "Количество полигонов",
                Command = ShowTriangleCountInfoCommand,
                ImageSmall = ImageResources.DCLDefault16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLDefault32.GetBitmapImage(),
                HotKey = new KeyGesture(Key.F11),
                //GlyphSize = GlyphSizeType.Large,
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 60
            });
            listMenuItem.MenuItems.Add(new CheckCommandMenuItem
            {
                Caption = "Кубический вид",
                Command = ShowViewCubeCommand,
                ImageSmall = ImageResources.DCLDefault16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLDefault32.GetBitmapImage(),
                HotKey = new KeyGesture(Key.F12),
                //GlyphSize = GlyphSizeType.Large,
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 70
            });
        }

        private string GetInformation(ISourceModel obj)
        {
            if (obj == null || obj.Source == null)
                return "Информация недоступна";

            var sb = new StringBuilder(obj.Source.ToString());
            sb.AppendLine();
            var properties = TypeDescriptor.GetProperties(obj.Source);
            foreach (PropertyDescriptor property in properties)
            {
                var nonNullableType = property.PropertyType.GetNonNullableType();
                if (nonNullableType.IsValueType || nonNullableType == typeof(string))
                {
                    sb.AppendFormat("{0}: {1}", property.DisplayName, property.GetValue(obj.Source));
                    sb.AppendLine();
                }
            }
            return sb.ToString();
        }

        public void ShowCardForSelectedObject()
        {
            var placeModel = SelectedObject as ISourceModel;
            if (placeModel == null || placeModel.Source == null)
                return;

            var vs = IoC.Instance.Resolve<IViewService>();
            var vm = WrappModelIntoVM(placeModel.Source);
            if (vm != null)
                vs.Show(vm);
        }

        protected virtual IViewModel WrappModelIntoVM(object model)
        {
            var vmType = typeof (IObjectViewModel<>).MakeGenericType(model.GetType());
            var ovm = IoC.Instance.Resolve(vmType) as IObjectViewModel;
            if (ovm != null)
                ovm.SetSource(model);
            return ovm;
        }

        private void OnIsTTShowChanged()
        {
            IsHideCompletedTT = false;
        }
        private void OnIsHideCompletedTTChanged()
        {
            
        }

        #endregion

        #region .  Properties  .
        public ICommand RefreshCommand { get; private set; }
        public ICommand ExportCommand { get; private set; }
        public ICommand FilterCommand { get; private set; }

        public ICommand ShowFreeCommand { get; private set; }
        public ICommand ShowABCDCommand { get; private set; }
        public ICommand ShowTTCommand { get; private set; }
        public ICommand HideCompletedTTCommand { get; private set; }
        public ICommand FindByArtCommand { get; private set; }

        public ICommand ShowCoordinatesCommand { get; private set; }
        public ICommand ShowCameraInfoCommand { get; private set; }
        public ICommand ShowCameraTargetCommand { get; private set; }
        public ICommand ShowFieldOfViewCommand { get; private set; }
        public ICommand ShowFrameRateCommand { get; private set; }
        public ICommand ShowTriangleCountInfoCommand { get; private set; }
        public ICommand ShowViewCubeCommand { get; private set; }

        public ObservableCollection<Visual3D> Objects
        {
            get { return _objects; }
            set
            {
                _objects = value;
                OnPropertyChanged("Objects");
            }
        }

        public ObservableCollection<Warehouse> Warehouses
        {
            get { return _warehouses; }
            set
            {
                _warehouses = value;
                OnPropertyChanged("Warehouses");
            }
        }

        public Warehouse SelectedWarehouse
        {
            get { return _selectedWarehouse; }
            set
            {
                _selectedWarehouse = value;
                OnPropertyChanged("SelectedWarehouse");
            }
        }

        public object SelectedObject
        {
            get { return _selectedObject; }
            set
            {
                OnSelectedObjectChanging(value);
                _selectedObject = value;
                OnPropertyChanged("SelectedObject");
                OnSelectedObjectChanged();
            }
        }

        private void OnSelectedObjectChanging(object newObj)
        {
            var box = SelectedObject as BoxVisual3D;
            if (box != null)
                box.Material = _lastObjectMaterial;

            var selectable = SelectedObject as ISelectable;
            if (selectable != null)
                selectable.ClearSelected();
        }

        private void OnSelectedObjectChanged()
        {
            var box = _selectedObject as BoxVisual3D;
            if (box != null)
            {
                _lastObjectMaterial = box.Material;
                box.Material = _selectedObjectMaterial;
                Description = box.ToString();
            }

            PlaceInformation = GetInformation(SelectedObject as ISourceModel);

            var selectable = SelectedObject as ISelectable;
            if (selectable != null)
                selectable.SetSelected();
        }

        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                OnPropertyChanged("Description");
            }
        }

        public bool IsFilterVisible
        {
            get { return _isFilterVisible; }
            set
            {
                if (_isFilterVisible == value)
                    return;

                _isFilterVisible = value;
                OnPropertyChanged("IsFilterVisible");
            }
        }

        public string PlaceInformation
        {
            get { return _placeInformation; }
            set
            {
                _placeInformation = value;
                OnPropertyChanged("PlaceInformation");
            }
        }

        public IFilterViewModel Filters { get; protected set; }

        public bool ShowCoordinates
        {
            get { return _showCoordinates; }
            set
            {
                _showCoordinates = value;
                OnPropertyChanged("ShowCoordinates");
            }
        }

        public bool ShowCameraInfo
        {
            get { return _showCameraInfo; }
            set
            {
                _showCameraInfo = value;
                OnPropertyChanged("ShowCameraInfo");
            }
        }

        public bool ShowCameraTarget
        {
            get { return _showCameraTarget; }
            set
            {
                _showCameraTarget = value;
                OnPropertyChanged("ShowCameraTarget");
            }
        }

        public bool ShowFieldOfView
        {
            get { return _showFieldOfView; }
            set
            {
                _showFieldOfView = value;
                OnPropertyChanged("ShowFieldOfView");
            }
        }

        public bool ShowFrameRate
        {
            get { return _showFrameRate; }
            set
            {
                _showFrameRate = value;
                OnPropertyChanged("ShowFrameRate");
            }
        }

        public bool ShowTriangleCountInfo
        {
            get { return _showTriangleCountInfo; }
            set
            {
                _showTriangleCountInfo = value;
                OnPropertyChanged("ShowTriangleCountInfo");
            }
        }

        public bool ShowViewCube
        {
            get { return _showViewCube; }
            set
            {
                _showViewCube = value;
                OnPropertyChanged("ShowViewCube");
            }
        }

        public Point3D CurrentPosition
        {
            get
            {
                return this._currentPosition;
            }
            set
            {
                this._currentPosition = value;
                OnPropertyChanged("CurrentPosition");
            }
        }

        public bool IsTTShow
        {
            get { return _isTTShow; }
            set
            {
                if (_isTTShow == value)
                    return;

                _isTTShow = value;
                OnPropertyChanged("IsTTShow");
                OnIsTTShowChanged();
            }
        }

        public bool IsHideCompletedTT
        {
            get { return _isHideCompletedTT; }
            set
            {
                if (_isHideCompletedTT == value)
                    return;

                _isHideCompletedTT = value;
                OnPropertyChanged("IsHideCompletedTT");
                OnIsHideCompletedTTChanged();
            }
        }

        public bool IsShowFree
        {
            get { return _isShowFree; }
            set
            {
                _isShowFree = value;
                OnPropertyChanged("IsShowFree");
            }
        }

        public bool IsShowABCD
        {
            get { return _isShowABCD; }
            set
            {
                _isShowABCD = value;
                OnPropertyChanged("IsShowABCD");
            }
        }

        #endregion
    }

    public interface ISelectable
    {
        void SetSelected();
        void ClearSelected();
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Grid;
using log4net;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.Helpers;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.General.ViewModels.Menu;
using wmsMLC.DCL.General.Views;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Helpers;
using wmsMLC.General.PL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Helpers;
using wmsMLC.General.PL.WPF.Services;
using Application = System.Windows.Application;
using BarItem = wmsMLC.DCL.General.ViewModels.Menu.BarItem;

namespace wmsMLC.DCL.Main.Views
{
    // ReSharper disable RedundantExtendsListEntry
    public partial class GCListView : CustomSubListControl, ISubView //INotifyPropertyChanged, ISettingsNameHandler, 
    // ReSharper restore RedundantExtendsListEntry
    {
        #region .  Const  .

        private const string Entity2GcTypeName = "ENTITY2GC";

        #endregion

        #region .  Variables  .

        public static readonly ILog _log = LogManager.GetLogger(typeof(GCListView));
        public static readonly DependencyProperty SourcePropertyInternal = DependencyProperty.Register("Source", typeof(WMSBusinessCollection<Entity2GC>), typeof(GCListView), new PropertyMetadata(OnSourcePropertyInternalChanged));

        private CommandMenuItem _selectMenuItem;
        private CommandMenuItem _cpvMenuItem;
        private readonly LookupInfo _lookupInfo;
        private ObservableCollection<object> _childSource;

        #endregion

        #region .  ctor  .
        public GCListView()
        {
            InitializeComponent();
            Menu = new MenuViewModel(GetType().GetFullNameWithoutVersion()) {NotUseGlobalLayoutSettings = true};
            SelectCommand = new DelegateCustomCommand(OnSelect, CanCud);
            DeleteCommand = new DelegateCustomCommand(OnDelete, CanDelete);
            CpvCommand = new DelegateCustomCommand(OnCPV, CanCPV);
            InitializeBaseCommandsMenu();
            ChildSource = new ObservableCollection<object>();
           
            // выставляем себя в качестве источника
            LayoutRoot.DataContext = this;
            gridControl.MaxHeight = CalcMaxHeight();

            gridControl.RestoredLayoutFromXml += GridControlOnRestoredLayoutFromXml;
        }

        private void GridControlOnRestoredLayoutFromXml(object sender, EventArgs eventArgs)
        {
            gridControl.SelectionMode = MultiSelectMode.Cell;
        }

        public GCListView(string fieldName, string lookupCode = "")
            : this()
        {
            if (string.IsNullOrEmpty(lookupCode))
                throw new DeveloperException("LookupCode is null");

            _lookupInfo = LookupHelper.GetLookupInfo(lookupCode);
            _itemType = _lookupInfo.ItemType;

            if (_itemType == null)
                throw new DeveloperException("Type of itemType is null.");

            SubListParentFieldName = fieldName;
            FillFields();

            DataContextChanged += OnDataContextChanged;
        }
        #endregion
        
        #region .  Commands  .
        
        public ICommand SelectCommand { get; private set; }
        public ICommand CpvCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }

        private bool CanCud()
        {
            var canCud = !IsReadOnly
                && ParentViewModelSource != null && _itemType != null
                && (!ShouldUpdateSeparately || !ParentViewModelSource.IsNew);

            if (!canCud)
                return false;

            if (!ShouldUpdateSeparately)
                return true;

            return !ParentViewModelSource.IsDirty;
        }

        private void OnSelect()
        {
            if (!ConnectionManager.Instance.AllowRequest())
                return;

            if (!CanCud())
                return;

            using (var window = CreateWindow())
            {
                if (window.ShowDialog() != true || IsReadOnly)
                    return;

                var model = window.DataContext as IObjectListViewModel;
                if (model == null || model.SelectedItem == null)
                    return;

                OnSourceCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, model.SelectedItem));
                
                Revalidate();
            }
        }
       
        public bool CanDelete()
        {
            return CanCud() && CurrentItem != null && SelectedItems != null;
        }

        public void OnDelete()
        {
            if (!ConnectionManager.Instance.AllowRequest())
                return;

            if (!CanDelete())
                return;

            try
            {
                if (GetViewService().ShowDialog(StringResources.Confirmation
                    , StringResources.ConfirmationDeleteObject
                    , MessageBoxButton.YesNo
                    , MessageBoxImage.Question
                    , MessageBoxResult.Yes) != MessageBoxResult.Yes)
                    return;

                var items = SelectedItems.Cast<EditableBusinessObject>().ToList();

                foreach (var item in items)
                {
                    ChildSource.Remove(item);
                    
                    var key = ((WMSBusinessObject) item).GetKey().ToString();
                    if (!string.IsNullOrEmpty(key))
                    {
                        var exit = false;
                        while (!exit)
                        {
                            var e2gc = Source.FirstOrDefault(x => string.Equals(key, x.ENTITY2GCKEY));
                            if (e2gc != null)
                                Source.Remove(e2gc);
                            else exit = true;
                        }
                    }
                }
                Revalidate();

            }
            catch (Exception ex)
            {
                throw new OperationException(ExceptionResources.ItemCantDelete, ex);
            }
        }


        public bool CanCPV()
        {
            var check = SelectedItems != null && (CanCud() && CurrentItem != null && SelectedItems.Count == 1);
            if (!check)
                return false;

            var item = CurrentItem as WMSBusinessObject;
            if (item == null) 
                return false;

            var key = item.GetKey().ToString();

            if (string.IsNullOrEmpty(key)) 
                return false;

            var e2Gc = Source.FirstOrDefault(x => string.Equals(key, x.ENTITY2GCKEY));
            if (e2Gc == null) 
                return false;

            return e2Gc.GetKey() != null;
        }

        public void OnCPV()
        {
            if (!CanCPV())
                return;

            var vs = IoC.Instance.Resolve<IViewService>();

            var item = CurrentItem as WMSBusinessObject;
            if (item == null)
                return;

            var vm = new CustomParamValueTreeViewModel<Entity2GCCpv>
            {
                CpEntity = Entity2GcTypeName,
                CpSource = _itemType.Name.ToUpper(),
                ShouldUpdateSeparately = true,
                Mode = ObjectListMode.ObjectList,
                AutoExpandAllNodes = true
            };

            var cpv = (CustomParamValue)Activator.CreateInstance(typeof(Entity2GCCpv));
            vm.KeyPropertyName = cpv.GetPrimaryKeyPropertyName();
            vm.ParentIdPropertyName = cpv.ChangePropertyName(CustomParamValue.CPVParentPropertyName);

            if (ParentViewModelSource != null)
            {
                vm.CpTarget = GetParentViewModelSourceName();
            }

            var el = Source.FirstOrDefault(x => string.Equals(item.GetKey().ToString(), x.ENTITY2GCKEY));
            if (el != null)
                vm.CpKey = el.GetKey().ToString();

            vm.RefreshData();
            var result = vs.ShowDialogWindow(viewModel: vm, isRestoredLayout: true, isNotNeededClosingOnOkResult: false, noButtons: true, noActionOnCancelKey: false, height: "50%", width: "65%");
            if (!result.HasValue) return;

            UpdateSource(el);
        }

        private string GetParentViewModelSourceName()
        {
            return ParentViewModelSource == null ? null : SourceNameHelper.Instance.GetSourceName(ParentViewModelSource.GetType()).ToUpper();
        }

        private void UpdateSource(Entity2GC obj)
        {
            if (Source == null)
                return;

            try
            {
                ParentViewModelSource.SuspendNotifications();
                ParentViewModelSource.SuspendValidating();
                Source.SuspendNotifications();
                var parentViewModelSourceName = GetParentViewModelSourceName();
                
                using (var mgr = IoC.Instance.Resolve<IBaseManager<Entity2GC>>())
                {
                    // Перевычитываем из БД весь Source
                    if (obj == null)
                    {
                        var keys = Source.Select(item => (object)item.ENTITY2GCID).ToList();
                        if (keys.Count <= 0) return;

                        var array = mgr.GetFiltered(FilterHelper.GetFilterIn(Entity2GC.ENTITY2GCIDPropertyName, keys)).ToArray();
                        foreach (var itParent in array.Cast<IHasParent>().Where(itParent => itParent != null))
                        {
                            itParent.SourceTypeName = _itemType.Name.ToUpper();
                            itParent.TargetTypeName = parentViewModelSourceName;
                        }

                        Source.Clear();
                        Source.AddRange(array);
                        ParentViewModelSource.AcceptChanges();
                    }
                    else
                    {
                        var isDirty = ParentViewModelSource.IsDirty;

                        var index = Source.IndexOf(obj);
                        Source.RemoveAt(index);

                        var it = mgr.Get(obj.GetKey());
                        var itParent = (IHasParent) it;
                        if (itParent != null)
                        {
                            itParent.SourceTypeName = _itemType.Name.ToUpper();
                            itParent.TargetTypeName = GetParentViewModelSourceName();
                        }
                        Source.Insert(index, it);

                        if (!isDirty)
                            ParentViewModelSource.AcceptChanges();
                    }
                }
            }
            finally
            {
                Source.ResumeNotifications();
                ParentViewModelSource.ResumeValidating();
                ParentViewModelSource.ResumeNotifications();
                ParentViewModelSource.Validate();
            }
        }

        #endregion .  Commands  .

        #region .  Properties  .

        public WMSBusinessCollection<Entity2GC> Source
        {
            get { return (WMSBusinessCollection<Entity2GC>)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public ObservableCollection<object> ChildSource
        {
            get { return _childSource; }
            set
            {
                if (value == _childSource)
                    return;
                _childSource = value;
                OnPropertyChanged("ChildSource");
            }
        }

        public DependencyProperty SourceProperty
        {
            get { return SourcePropertyInternal; }
        }
        #endregion . Properties .

        private CustomLookUpOptPopupContent CreateWindow()
        {
            var destType = typeof(IListViewModel<>).MakeGenericType(_itemType);
            var model = (IObjectListViewModel)IoC.Instance.Resolve(destType, null);
            model.Mode = ObjectListMode.LookUpList3Points;
            model.ParentViewModelSource = ParentViewModelSource;
            model.AllowAddNew = true;
            model.InitializeMenus();

            var modelCapt = model as PanelViewModelBase;
            if (modelCapt != null)
            {
                modelCapt.SetPanelCaptionPrefix(DataContext.GetType());
                modelCapt.IsActive = true;
            }

            if (_lookupInfo != null)
            {
                string str;
                Dictionary<string, Dictionary<string, string>> varFilter;
                LookupHelper.InitializeVarFilter(_lookupInfo.Filter, out str, out varFilter);

                string filter = null;
                if (ChildSource != null && ChildSource.Count > 0)
                    filter = (FilterHelper.GetFilterIn(_itemType, ChildSource.Cast<IKeyHandler>())).Replace(" IN ", " NOT IN ");

                if (!string.IsNullOrEmpty(str))
                    filter = string.IsNullOrEmpty(filter) ? str : str + " AND " + filter;

                if (varFilter != null && varFilter.Count > 0)
                {
                    var filterParent = new StringBuilder();
                    foreach (var add in varFilter.Select(key => UpdateFilter(key.Key, ParentViewModelSource, varFilter)).Where(add => !string.IsNullOrEmpty(add)))
                    {
                        filterParent.Append(filterParent.Length > 0 ? " AND " + add : add);
                    }

                    if (filterParent.Length > 0)
                        filter = string.IsNullOrEmpty(filter) ? filterParent.ToString() : filter + " AND " + filterParent;
                }

                model.ApplyFilter(filter);
            }

            var result = new CustomLookUpOptPopupContent
            {
                DataContext = model
            };

            if (result.Owner == null && Application.Current.MainWindow.IsActive)
                result.Owner = Application.Current.MainWindow;
            return result;
        }

        private string UpdateFilter(string key, object data, Dictionary<string, Dictionary<string, string>> varFilter)
        {
            if (varFilter == null || data == null || string.IsNullOrEmpty(key))
                return null;

            var keyinternal = LookupHelper.GetStringValue(key);
            if (varFilter.ContainsKey(keyinternal))
            {
                string filter = null;
                var propvalue = string.Empty;
                if (data is ExpandoObject)
                {
                    var dict = data as IDictionary<string, object>;
                    if (dict.ContainsKey(key))
                        propvalue = dict[key].To<string>();
                    if (string.IsNullOrEmpty(propvalue))
                        propvalue = LookupHelper.FilterValueNull;
                }
                else
                {
                    var prdesc = TypeDescriptor.GetProperties(data);
                    var prop = prdesc.Find(keyinternal, true);
                    if (prop == null)
                        return null;
                    propvalue = LookupHelper.GetStringValue(prop.GetValue(data).To(LookupHelper.FilterValueNull));
                }

                if (string.IsNullOrEmpty(propvalue))
                    return null;

                if (varFilter[keyinternal].ContainsKey(propvalue))
                    filter = string.Format(varFilter[keyinternal][propvalue], propvalue);
                else if (varFilter[keyinternal].ContainsKey(LookupHelper.FilterValueAny))
                    filter = string.Format(varFilter[keyinternal][LookupHelper.FilterValueAny], propvalue);
                return filter;
            }
            return null;
        }

        public void FillFields()
        {
            var fields = DataFieldHelper.Instance.GetDataFields(_itemType, SettingDisplay.LookUp);

            foreach (var field in fields)
            {
                field.AllowAddNewValue = true;
            }

            Fields = new ObservableCollection<DataField>(fields);
        }
        
        private void InitializeBaseCommandsMenu()
        {
            var bar = new BarItem
            {
                Name = "BarItemCommands",
                Caption = StringResources.Commands,
                GlyphSize = GlyphSizeType.Small
            };
            Menu.Bars.Add(bar);

            _selectMenuItem = new CommandMenuItem
            {
                Caption = StringResources.Select,
                Command = SelectCommand,
                ImageSmall = ImageResources.DCLAddNew16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLAddNew32.GetBitmapImage(),
                GlyphSize = GlyphSizeType.Small,
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                IsVisible = true,
                Priority = 1
            };
            bar.MenuItems.Add(_selectMenuItem);

            bar.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.Delete,
                Command = DeleteCommand,
                ImageSmall = ImageResources.DCLDelete16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLDelete32.GetBitmapImage(),
                //HotKey = new KeyGesture(Key.F9),
                GlyphSize = GlyphSizeType.Small,
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                IsVisible = true,
                Priority = 2
            });

            _cpvMenuItem = new CommandMenuItem
            {
                Caption = "Настройка параметров",
                Command = CpvCommand,
                ImageSmall = ImageResources.DCLGridEditBegin16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLGridEditBegin32.GetBitmapImage(),
                GlyphSize = GlyphSizeType.Small,
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                IsVisible = true,
                Priority = 3
            };
            bar.MenuItems.Add(_cpvMenuItem);
        }

        public void OnSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add || e.NewItems == null) 
                return;

            if (Source == null)
                Source = new WMSBusinessCollection<Entity2GC>();

            foreach (var item in e.NewItems)
            {
                if (item == null) 
                    continue;

                if (Source == null) 
                    continue;
                var newItem = new Entity2GC
                {
                    ENTITY2GCKEY = ((WMSBusinessObject)item).GetKey().ToString()
                };

                Source.Add(newItem);
                ChildSource.Add((WMSBusinessObject)item);
            }
        }

        public static void OnSourcePropertyInternalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((GCListView)d).OnSourcePropertyInternalChanged(e.NewValue);
        }

        public void OnSourcePropertyInternalChanged(object newvalue)
        {
            if (newvalue == null)
                CurrentItem = null;

            if (Source == null) return;

            UpdateSource(null);
            ChildSource.Clear();
            if (Source == null || Source.Count == 0) 
                return;

            var keys = Source.Select(item => (object) item.ENTITY2GCKEY).ToList();
            var pk = WMSBusinessObject.GetPrimaryKeyPropertyName(_itemType);
            var pkSourceName = SourceNameHelper.Instance.GetPropertySourceName(_itemType, pk);

            using (var mgr = GetManager(_itemType))
            {
                var items = mgr.GetFiltered(FilterHelper.GetFilterIn(pkSourceName, keys));
                ChildSource.AddRange(items);
            }
        }

        public override void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            base.OnDataContextChanged(sender, e);

            _selectMenuItem.IsVisible = ParentViewModelSource != null;
            OnPropertyChanged("Menu");
        }

        #region . IDisposable .
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // events
                PropertyChanged -= OnPropertyChanged;
                DataContextChanged -= OnDataContextChanged;
                UnSubscribeSource();

                //controls
                if (gridControl != null)
                    gridControl.Dispose();

                // прибиваем ссылки на внешние сущности
                ParentViewModelSource = null;
                CurrentItem = null;
            }

            base.Dispose(disposing);
        }
        #endregion . IDisposable .

        // событие отображения редактора ячейки

        private void GridControl_OnSelectionChanged(object sender, GridSelectionChangedEventArgs e)
        {
            SelectedItems = gridControl.SelectedItems;
            RaiseCanExecuteChanged();
        }

        private void BarItem_OnItemClick(object sender, ItemClickEventArgs e)
        {
            var cont = e.Item.Content.ToString();

            if (string.IsNullOrEmpty(cont))
                return;

            if (!cont.Equals(StringResources.Copy)) 
                return;

            var oldMode = gridControl.ClipboardCopyMode;
            gridControl.ClipboardCopyMode = ClipboardCopyMode.ExcludeHeader;
            SendKeys.SendWait("^C");
            gridControl.ClipboardCopyMode = oldMode;
        }
    }
}
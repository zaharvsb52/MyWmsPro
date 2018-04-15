using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Grid.TreeList;
using wmsMLC.Business.General;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.Helpers;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.General.ViewModels.Menu;
using wmsMLC.DCL.General.Views;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.DAL;
using wmsMLC.General.PL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.Properties;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Helpers;
using wmsMLC.General.PL.WPF.Services;
using wmsMLC.General.PL.WPF.Views;
using BarItem = wmsMLC.DCL.General.ViewModels.Menu.BarItem;
using IoC = wmsMLC.General.IoC;

namespace wmsMLC.DCL.Main.Views
{
    public partial class CustomParamValueSubTreeView : CustomSubControl, INotifyPropertyChanged, ISettingsNameHandler, ISubView
    {
        private WMSBusinessObject _parentViewModelSource;
        private readonly Type _itemType;
        private IList _selectedItems;

        public CustomParamValueSubTreeView()
        {
            ShowTotalRow = false;
            ShowNodeImage = false;
            KeyPropertyName = new CustomParamValue().GetPrimaryKeyPropertyName();
            ParentIdPropertyName = CustomParamValue.CPVParentPropertyName.ToUpper();
            //DefaultSortingField = CustomParamValue.CustomParamCodePropertyName.ToUpper();

            InitializeComponent();
            AutoExpandAllNodes = true;
            Menu = new MenuViewModel(GetType().GetFullNameWithoutVersion()) {NotUseGlobalLayoutSettings = true};
            NewCommand = new DelegateCustomCommand(OnNew, CanNew);
            EditCommand = new DelegateCustomCommand(OnEdit, CanEdit);
            DeleteCommand = new DelegateCustomCommand(OnDelete, CanDelete);
            InitializeBaseCommandsMenu();

            LayoutRoot.DataContext = this;

            customTreeListControl.MaxHeight = CalcMaxHeight();

            customTreeListControl.RestoredLayoutFromXml += CustomTreeListControlOnRestoredLayoutFromXml;
            PropertyChanged += OnPropertyChanged;
        }

        private void CustomTreeListControlOnRestoredLayoutFromXml(object sender, EventArgs eventArgs)
        {
            customTreeListControl.SelectionMode=MultiSelectMode.Cell;
            customTreeListControl.View.NavigationStyle = GridViewNavigationStyle.Cell;
        }

        public CustomParamValueSubTreeView(Type itemsType, Type itemType = null)
            : this()
        {
            if (itemsType == null)
                throw new ArgumentNullException("itemsType");


            if (itemType == null)
            {
                if (itemsType.IsGenericType)
                    _itemType = itemsType.GetGenericArguments().FirstOrDefault();
            }
            else
            {
                _itemType = itemType;
            }

            if (_itemType == null)
                throw new DeveloperException("Type of itemsType is not generic.");

            if (!typeof(CustomParamValue).IsAssignableFrom(_itemType))
                throw new DeveloperException("Type {0} is not assignable from CustomParamValue", _itemType);

            //KeyPropertyName = CustomParamValue.ChangePropertyNameByType(new CustomParamValue().GetPrimaryKeyPropertyName(), _itemType);
            //ParentIdPropertyName = CustomParamValue.ChangePropertyNameByType(CustomParamValue.CPVParentPropertyName, _itemType);

            var cpv = (CustomParamValue) Activator.CreateInstance(_itemType);
            KeyPropertyName = cpv.GetPrimaryKeyPropertyName();
            ParentIdPropertyName = cpv.ChangePropertyName(CustomParamValue.CPVParentPropertyName);

            OnItemTypeChanged();

            DataContextChanged += OnDataContextChanged;
        }

        #region . Properties .
        public string KeyPropertyName { get; protected set; }
        public string ParentIdPropertyName { get; protected set; }
        public bool ShowNodeImage { get; protected set; }
        public bool ShowTotalRow { get; protected set; }
        public string DefaultSortingField { get; protected set; }
        public bool TotalRowItemFilteredSymbolIsVisible { get; protected set; }
        public string TotalRowItemAdditionalInfo { get; protected set; }
        public ObservableCollection<object> SelectedItems { get; set; }
        public bool ShowColumnHeaders { get; protected set; }

        public IList Source
        {
            get { return (IList)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }
        private static readonly DependencyProperty SourcePropertyInternal = DependencyProperty.Register("Source", typeof(IList), typeof(CustomParamValueSubTreeView), new PropertyMetadata(OnSourcePropertyInternalChanged));

        private static void OnSourcePropertyInternalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CustomParamValueSubTreeView)d).OnSourcePropertyInternalChanged(e.NewValue);
        }

        private void OnSourcePropertyInternalChanged(object newvalue)
        {
            if (newvalue == null)
                CurrentItem = null;
        }

        private bool _isReadOnly;
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set
            {
                if (_isReadOnly == value) return;
                _isReadOnly = value;
                OnPropertyChanged("IsReadOnly");
            }
        }

        private ObservableCollection<DataField> _fields;
        public ObservableCollection<DataField> Fields
        {
            get { return _fields; }
            set
            {
                if (value == _fields) return;
                _fields = value;
                OnPropertyChanged("Fields");
            }
        }

        private EditableBusinessObject _currentItem;
        public EditableBusinessObject CurrentItem
        {
            get { return _currentItem; }
            set
            {
                if (_currentItem == value) return;
                _currentItem = value;
                OnPropertyChanged("CurrentItem");
            }
        }

        public MenuViewModel Menu { get; set; }
        public IModelHandler ParentViewModel { get; set; }

        /// <summary>
        /// Можно ли этот объект сохранять отдельно от основного.
        /// </summary>
        public bool ShouldUpdateSeparately { get; set; }

        private bool _autoExpandAllNodes;
        public bool AutoExpandAllNodes
        {
            get { return _autoExpandAllNodes; }
            set
            {
                if (_autoExpandAllNodes == value)
                    return;
                _autoExpandAllNodes = value;
                OnPropertyChanged("AutoExpandAllNodes");
            }
        }
        #endregion . Properties .

        #region .  Commands  .
        public ICommand NewCommand { get; private set; }
        public ICommand EditCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }

        private bool CanNew()
        {
            return CanCud();
        }

        private void OnNew()
        {
            if (!ConnectionManager.Instance.AllowRequest())
                return;

            if (!CanNew())
                return;

            try
            {
                OnEdit(null);
            }
            catch (Exception ex)
            {
                throw new OperationException(ExceptionResources.ItemCantInsert, ex);
            }
        }

        private bool CanEdit()
        {
            return CanCud() && CurrentItem != null;
        }

        private void OnEdit()
        {
            if (!ConnectionManager.Instance.AllowRequest())
                return;

            if (!CanEdit())
                return;

            try
            {
                OnEdit(CurrentItem);
            }
            catch (Exception ex)
            {
                throw new OperationException(ExceptionResources.ItemCantEdit, ex);
            }
        }

        private bool CanDelete()
        {
            return CanCud() && CurrentItem != null && ((CustomParamValue)CurrentItem).CPVParent == null && (_selectedItems != null && _selectedItems.Count == 1);
        }

        private void OnDelete()
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

                var selectedItem = (CustomParamValue)CurrentItem;
                var deletes = CpvHelper.GetChildsCpvByParentCpv<CustomParamValue>(Source.Cast<CustomParamValue>(), selectedItem, true);

                if (ShouldUpdateSeparately)
                {
                    var uowFactory = IoC.Instance.Resolve<IUnitOfWorkFactory>();
                    using (var uow = uowFactory.Create(false))
                    {
                        try
                        {
                            uow.BeginChanges();
                            var mng = GetManager(_itemType);
                            mng.SetUnitOfWork(uow);

                            foreach (var p in deletes)
                                mng.Delete(p);
                            mng.Delete(CurrentItem);

                            uow.CommitChanges();
                        }
                        catch
                        {
                            uow.RollbackChanges();
                            throw;
                        }
                    }

                    if (ParentViewModel != null)
                        ParentViewModel.RefreshData();
                }
                else
                {
                    foreach (var p in deletes)
                        Source.Remove(p);
                    Source.Remove(CurrentItem);
                }
            }
            catch (Exception ex)
            {
                throw new OperationException(ExceptionResources.ItemCantDelete, ex);
            }
        }
        #endregion .  Commands  .


        //private void ShowValidationError(string message)
        //{
        //    GetViewService().ShowDialog(StringResources.Error
        //        , string.Format("{0}{1}{2}", typeof(CustomParam).GetDisplayName() + ":", Environment.NewLine, string.Format(DCL.Resources.StringResources.CpCountValidationFormat, message))
        //        , MessageBoxButton.OK
        //        , MessageBoxImage.Error
        //        , MessageBoxResult.Yes);
        //}

        private void OnEdit(EditableBusinessObject selectedItem)
        {
            var modelType = typeof(CustomParamValueTreeViewModel<>).MakeGenericType(_itemType);
            var model = (ICustomParamValueTreeViewModel)IoC.Instance.Resolve(modelType, null);
            model.Mode = ObjectListMode.LookUpList3Points;
            model.AutoExpandAllNodes = true;

            var cpv = (CustomParamValue)Activator.CreateInstance(_itemType);
            model.KeyPropertyName = cpv.GetPrimaryKeyPropertyName();
            model.ParentIdPropertyName = cpv.ChangePropertyName(CustomParamValue.CPVParentPropertyName);

            model.CpEntity = SourceNameHelper.Instance.GetSourceName(_parentViewModelSource.GetType()).ToUpper();
            model.CpKey = _parentViewModelSource.GetKey().To<string>();
            model.ShouldUpdateSeparately = ShouldUpdateSeparately;
            model.SelectedItem = selectedItem;
            model.MandantCode = (string)(_parentViewModelSource.ContainsProperty("VMANDANTCODE")
                ? _parentViewModelSource.GetProperty("VMANDANTCODE")
                : null);

            if (_parentViewModelSource != null)
                model.ParentViewModelSource = _parentViewModelSource;

            if (model.CustomFilters != null)
                model.ApplyFilter(null);

            var issourceupdated = false;
            EventHandler onSourceUpdatedHandler = (s, e) =>
            {
                issourceupdated = true;
            };

            try
            {
                model.SourceUpdated += onSourceUpdatedHandler;
                GetViewService()
                    .ShowDialogWindow(viewModel: model, isRestoredLayout: true, isNotNeededClosingOnOkResult: false,
                        noButtons: true, noActionOnCancelKey: false, height: "50%", width: "65%");
                if (issourceupdated)
                {
                    if (ShouldUpdateSeparately)
                    {
                        if (ParentViewModel != null)
                            ParentViewModel.RefreshData();
                    }
                    else
                    {
                        //TODO: Надо доделывать для этого режима
                        Source = (IList)model.GetSource();
                    }
                }
            }
            finally
            {
                model.SourceUpdated -= onSourceUpdatedHandler;
            }
        }

        private void OnSelectedItemChanged(object sender, SelectedItemChangedEventArgs e)
        {
            CurrentItem = e.NewItem as EditableBusinessObject;
        }
       
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext != null)
                _parentViewModelSource = DataContext as WMSBusinessObject;

            if (_parentViewModelSource != null)
            {
                //HACK: Используется для VarFilter и многого другого
                RaiseCanExecuteChanged();
                SubscribeSource();
            }
        }

        private void SubscribeSource()
        {
            UnSubscribeSource();

            var npch = _parentViewModelSource as INotifyPropertyChanged;
            if (npch != null)
            {
                //HACK: VarFilter
                npch.PropertyChanged += OnPropertyChanged;
            }
        }

        private void UnSubscribeSource()
        {
            var npch = _parentViewModelSource as INotifyPropertyChanged;
            if (npch != null)
            {
                //HACK: VarFilter
                npch.PropertyChanged -= OnPropertyChanged;
            }
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

            bar.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.New,
                Command = NewCommand,
                ImageSmall = ImageResources.DCLAddNew16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLAddNew32.GetBitmapImage(),
                GlyphSize = GlyphSizeType.Small,
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 1
            });

            bar.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.Edit,
                Command = EditCommand,
                ImageSmall = ImageResources.DCLEdit16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLEdit32.GetBitmapImage(),
                GlyphSize = GlyphSizeType.Small,
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 2
            });

            bar.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.Delete,
                Command = DeleteCommand,
                ImageSmall = ImageResources.DCLDelete16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLDelete32.GetBitmapImage(),
                GlyphSize = GlyphSizeType.Small,
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 3
            });
        }

        private IBaseManager GetManager(Type type)
        {
            var mto = IoC.Instance.Resolve<IManagerForObject>();
            var mgrType = mto.GetManagerByTypeName(type.Name);
            return (IBaseManager)IoC.Instance.Resolve(mgrType, null);
        }

        private bool CanCud()
        {
            var canCud = !IsReadOnly
                && _parentViewModelSource != null && _itemType != null
                && (!ShouldUpdateSeparately || !_parentViewModelSource.IsNew);

            if (!canCud)
                return false;

            if (!ShouldUpdateSeparately)
                return true;

            return !_parentViewModelSource.IsDirty;
        }

        private void OnItemTypeChanged()
        {
            var result = DataFieldHelper.Instance.GetDataFields(_itemType, SettingDisplay.LookUp);
            foreach (var field in result)
                field.AllowAddNewValue = true;
            Fields = result;
        }

        private void RaiseCanExecuteChanged(ICommand command)
        {
            var dc = command as ICustomCommand;
            if (dc == null) 
                return;
            dc.RaiseCanExecuteChanged();
        }

        private static IViewService GetViewService()
        {
            return IoC.Instance.Resolve<IViewService>();
        }

        private void RaiseCanExecuteChanged()
        {
            foreach (var p in GetType().GetProperties().Where(p => p.PropertyType == typeof(ICommand)))
            {
                RaiseCanExecuteChanged((ICommand)p.GetGetMethod().Invoke(this, null));
            }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            RaiseCanExecuteChanged();
        }

        #region .  INotifyPropertyChanged  .
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion .  INotifyPropertyChanged  .

        public string GetSuffix()
        {
            return _itemType == null ? null : _itemType.FullName;
        }

        #region . IDisposable .
        protected override void Dispose(bool disposing)
        {
            // events
            UnSubscribeSource();
            DataContextChanged -= OnDataContextChanged;
            base.Dispose(disposing);
        }
        #endregion

        private void NodeDoubleClick(object sender, RowDoubleClickEventArgs rowDoubleClickEventArgs)
        {
            if (customTreeListControl.View.FocusedNode == null || !customTreeListControl.View.FocusedNode.HasChildren)
                return;
            customTreeListControl.View.FocusedNode.IsExpanded = !customTreeListControl.View.FocusedNode.IsExpanded;
        }

        public DependencyProperty SourceProperty
        {
            get { return SourcePropertyInternal; }
        }

        private void CustomTreeListControl_OnSelectionChanged(object sender, TreeListSelectionChangedEventArgs e)
        {
            _selectedItems = customTreeListControl.SelectedItems;
            RaiseCanExecuteChanged();
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using DevExpress.Utils;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Grid;
using log4net;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.Helpers;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.General.ViewModels.Menu;
using wmsMLC.DCL.General.Views;
using wmsMLC.DCL.Main.Views.Controls;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Helpers;
using Application = System.Windows.Application;
using BarItem = wmsMLC.DCL.General.ViewModels.Menu.BarItem;
using Clipboard = System.Windows.Clipboard;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace wmsMLC.DCL.Main.Views
{
    public partial class SubListView : ISubView
    {
        #region .  Fields  .
        public static readonly ILog Log = LogManager.GetLogger(typeof(SubListView));
        public static readonly DependencyProperty SourcePropertyInternal = DependencyProperty.Register("Source", typeof(IList), typeof(SubListView), new PropertyMetadata(OnSourcePropertyInternalChanged));
     
        private bool _isGridEditMode;

        private CommandMenuItem _gridEditMenuItem;
        private CommandMenuItem _newMenuItem;
        private CommandMenuItem _editMenuItem;

        public event EventHandler SelectionChanged;
        #endregion .  Fields  .

        #region .  ctor  .
        public SubListView()
        {
            InitializeComponent();

            Menu = new MenuViewModel(GetType().GetFullNameWithoutVersion()) {NotUseGlobalLayoutSettings = true};
            NewCommand = new DelegateCustomCommand(OnNew, CanNew);
            SelectCommand = new DelegateCustomCommand(OnSelect, CanNew);
            EditCommand = new DelegateCustomCommand(OnEdit, CanEdit);
            DeleteCommand = new DelegateCustomCommand(OnDelete, CanDelete);

            GridEditCommand = new DelegateCustomCommand(OnGridEdit, CanGridEdit);

            InitializeBaseCommandsMenu();

            // выставляем себя в качестве источника
            LayoutRoot.DataContext = this;
            gridControl.MaxHeight = CalcMaxHeight();

            gridControl.RestoredLayoutFromXml += GridControlOnRestoredLayoutFromXml;
        }

        private void GridControlOnRestoredLayoutFromXml(object sender, EventArgs eventArgs)
        {
            gridControl.SelectionMode = MultiSelectMode.Cell;
        }

        public SubListView(Type itemsType, Type itemType, string fieldName)
            : this()
        {
            if (itemsType == null)
                throw new ArgumentNullException("itemsType");

            _itemsType = itemsType;

            if (itemType == null)
            {
                if (_itemsType.IsGenericType)
                    _itemType = _itemsType.GetGenericArguments().FirstOrDefault();
            }
            else
                _itemType = itemType;

            if (_itemType == null)
                throw new DeveloperException("Type of itemsType is not generic.");

            SubListParentFieldName = fieldName;
            FillFields();

            DataContextChanged += OnDataContextChanged;
        }
        #endregion .  ctor  .

        #region .  Properties  .

        public IList Source
        {
            get { return (IList)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public DependencyProperty SourceProperty
        {
            get { return SourcePropertyInternal; }
        }

        public bool IsGridEditMode
        {
            get { return _isGridEditMode; }
            set
            {
                _isGridEditMode = value;
                OnPropertyChanged("IsGridEditMode");
            }
        }

        #endregion . Properties .

        #region .  Commands  .
        public ICommand NewCommand { get; private set; }
        public ICommand SelectCommand { get; private set; }
        public ICommand EditCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }

        public ICommand GridEditCommand { get; private set; }

        private bool CanNew()
        {
            if (!CanCud())
                return false;

            //HACK: VarFilter
            if (ParentViewModelSource.GetType() == typeof(EpsTask) && typeof(EpsConfig).IsAssignableFrom(_itemType))
                return !string.IsNullOrEmpty(((EpsTask)ParentViewModelSource).TaskType);

            return true;
        }

        private void OnNew()
        {
            if (!ConnectionManager.Instance.AllowRequest())
                return;

            if (!CanNew())
                return;

            try
            {
                var newItem = Activator.CreateInstance(_itemType);
                //var parentdatatypename = ParentViewModelSource.GetType().Name;
                var parentdatatypename = SourceNameHelper.Instance.GetSourceName(ParentViewModelSource.GetType());

                //Получаем детализацию по манданту принимаемой позиции или непривязанные к мандантам
                var mId = TypeDescriptor.GetProperties(ParentViewModel).Cast<PropertyDescriptor>().FirstOrDefault(i => i.Name.EqIgnoreCase("MANDANTID"));
                if (mId != null)
                {
                    var mandantId = mId.GetValue(ParentViewModel);
                    var fields = DataFieldHelper.Instance.GetDataFields(_itemType, SettingDisplay.Detail);

                    var fieldQlf = fields.FirstOrDefault(i => i.Name.EqIgnoreCase("QLFDETAILCODE_R"));
                    if (fieldQlf != null)
                    {
                        var lookupInfo = LookupHelper.GetLookupInfo(fieldQlf.LookupCode);
                        //TODO: при передаче в [...] этот фильтр зачем то TOUPPER делается, если указать WMSQLFDETAIL.QLFDETAILCODE то TOUPPER обходится и передается корректный фильтр. Передать Оле на доанализ и доделку
                        lookupInfo.Filter = String.Format(" WMSQLFDETAIL.QLFDETAILCODE in (select QLFDETAILCODE_R from wmsqlfdetail2mandant where PARTNERID_R = {0}) or WMSQLFDETAIL.QLFDETAILCODE not in (select QLFDETAILCODE_R from wmsqlfdetail2mandant)", mandantId);
                    }
                }

                var properties = IsLookupExist(newItem, parentdatatypename);

                var property = properties.Select(p => new { Name = p.Name.ToUpper(), Property = p })
                    .FirstOrDefault(p => p.Name.Contains("MASTER") || p.Name.Contains("PARENT"));
                SetValueByLookup(newItem, property == null ? (properties.Length > 0 ? properties[0] : null) : property.Property, ((IKeyHandler)ParentViewModelSource).GetKey());

                var parentProperty =
                    TypeDescriptor.GetProperties(ParentViewModelSource).Cast<PropertyDescriptor>().FirstOrDefault(i => i.Name.EqIgnoreCase("MANDANTID"));
                if (parentProperty != null)
                {
                    var mandants = IsLookupExist(newItem, "MANDANT");
                    foreach (var m in mandants)
                        SetValueByLookup(newItem, m, parentProperty.GetValue(ParentViewModelSource));
                }


                //Hack: Для позиций груза приходной накладной. Проставляем типы по умолчанию для факта и документа
                var pos = newItem as CargoIWBPos;
                if (pos != null && SubListParentFieldName != null)
                {
                    pos.CargoIwbPosType = SubListParentFieldName == "CARGOIWBPOSLCLIENT" ? "CLIENT" : "FACT";
                }

                //var propertysys = IsLookupExist(newItem, SourceNameHelper.GetSourceName(typeof (SysObject))); 
                //SetValueByLookup(newItem, propertysys, parentdatatypename);

                //HACK: Заполнение EpsConfig2Entity используется для VarFilter'а. Можно сделать через лукап от SysObj.
                if (typeof(EpsConfig).IsAssignableFrom(_itemType))
                {
                    if (string.IsNullOrEmpty(((EpsConfig)newItem).EpsConfig2Entity))
                        ((EpsConfig)newItem).EpsConfig2Entity = SourceNameHelper.Instance.GetSourceName(ParentViewModelSource.GetType()).ToUpper();

                    var keyHandler = ParentViewModelSource as IKeyHandler;
                    if (keyHandler != null && string.IsNullOrEmpty(((EpsConfig) newItem).EpsConfigKey))
                        ((EpsConfig) newItem).EpsConfigKey = keyHandler.GetKey<string>();
                }

                //HACK: Заполнение Report2ReportEntity
                if (typeof(Report2Report).IsAssignableFrom(_itemType) && !string.IsNullOrEmpty(SubListParentFieldName))
                {
                    switch (SubListParentFieldName)
                    {
                        case Report.ChildReportsPropertyName:
                            ((Report2Report)newItem).ReportCode = null;
                            break;
                        case Report.ParentsReportsPropertyName:
                            ((Report2Report) newItem).ReportCode = ((Report2Report) newItem).R2Rparent;
                            ((Report2Report)newItem).R2Rparent = null;
                            break;
                    }
                }

                if (newItem is EditableBusinessObject)
                    ((EditableBusinessObject) newItem).AcceptChanges(true);

                if (ViewObject(ref newItem, true) == true)
                {
                    if (ShouldUpdateSeparately)
                    {
                        //HACK: Добавление связок Working -> W2E2Working
                        var working = newItem as Working;
                        var parentvm = ParentViewModel as CustomObjectListViewModelBase<InputPlPos>;
                        if (parentvm != null && working != null)
                        {
                            var plpos = parentvm.Source.FirstOrDefault();
                            if (plpos != null)
                            {
                                Work2Entity w2E;
                                using (var mgrW2E = IoC.Instance.Resolve<IBaseManager<Work2Entity>>())
                                {
                                    w2E = mgrW2E.GetFiltered(String.Format(" workid_r = {0} and work2entityentity = 'PL' and work2entitykey = {1}", working.WORKID_R, plpos.PlIdR)).FirstOrDefault();
                                }

                                if (w2E != null)
                                {
                                    var ww2 = new W2E2Working() { WorkingId = working.WORKINGID, Work2EntityId = w2E.Work2EntityId };
                                    using (var mgrWw2 = IoC.Instance.Resolve<IBaseManager<W2E2Working>>())
                                    {
                                        mgrWw2.Insert(ref ww2);
                                    }
                                }
                            }

                        }
                        ParentRefresh();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new OperationException(ExceptionResources.ItemCantInsert, ex);
            }
        }

        private void OnSelect()
        {
            if (!ConnectionManager.Instance.AllowRequest())
                return;

            if (!CanNew())
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

            var result = new CustomLookUpOptPopupContent
            {
                DataContext = model
            };

            if (result.Owner == null && Application.Current.MainWindow.IsActive)
                result.Owner = Application.Current.MainWindow;
            return result;
        }
        
        private bool CanEdit()
        {
            return CanCud() && CurrentItem != null && (SelectedItems != null && SelectedItems.Count == 1);
        }

        private bool CanGridEdit()
        {
            if (IsGridEditMode)
                return true;
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
                gridControl.ShowLoadingPanel = true;
                var source = Source;

                //INFO: если изменяем новый объект, то необходимо запомнить текущее состояние
                var isNew = CurrentItem as IIsNew;
                if (isNew != null && isNew.IsNew)
                {
                    var editable = CurrentItem as IEditable;
                    if (editable != null)
                    {
                        editable.AcceptChanges(true);
                    }
                }

                object curItem = CurrentItem;
                if (ViewObject(ref curItem, false) == true)
                {
                    if (ShouldUpdateSeparately)
                    {
                        ParentRefresh();
                    }
                    else
                    {
                        Source = source;
                        Revalidate();
                    }
                }
            }
            catch (Exception ex)
            {
                //ExceptionHandler(ex, ExceptionResources.ItemCantEdit);
                throw new OperationException(ExceptionResources.ItemCantEdit, ex);
            }
            finally
            {
                gridControl.ShowLoadingPanel = false;
            }
        }

        private bool CanDelete()
        {
            return CanCud() && CurrentItem != null && SelectedItems != null;
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

                var items = SelectedItems.Cast<EditableBusinessObject>().ToList();

                if (ShouldUpdateSeparately)
                {
                    using (var mng = GetManager(_itemType))
                    foreach (var item in items)
                        {
                            mng.Delete(item);
                            if (ParentViewModel == null)
                                Source.Remove(item);
                        }
                    ParentRefresh();
                }
                else
                {
                    foreach (var item in items)
                        Source.Remove(item);
                    Revalidate();
                }
            }
            catch (Exception ex)
            {
                throw new OperationException(ExceptionResources.ItemCantDelete, ex);
            }
        }
        
        private void OnGridEdit()
        {
            gridControl.View.Focus();
            if (IsGridEditMode && ShouldUpdateSeparately)
            {
                var dirtyList = Source.Cast<WMSBusinessObject>().Where(i => i.IsDirty).ToArray();
                if (dirtyList.Any())
                {
                    var res = GetViewService()
                        .ShowDialog(StringResources.Confirmation, StringResources.ConfirmationUnsavedData, MessageBoxButton.YesNoCancel, MessageBoxImage.Question,
                            MessageBoxResult.Yes);
                    if (res == MessageBoxResult.Cancel)
                        return;

                    if (res == MessageBoxResult.Yes)
                    {
                        gridControl.View.CommitEditing();

                        using (var mgr = GetManager(_itemType))
                            foreach (var bo in dirtyList)
                                mgr.Update(bo);
                        ParentRefresh();
                    }
                    else
                        foreach (var bo in dirtyList)
                            bo.RejectChanges();
                }
            }

            IsGridEditMode = !IsGridEditMode;
            FillFields();
            GetViewService().RestoreLayout(this);
            if (_gridEditMenuItem == null)
                return;

            _gridEditMenuItem.ImageSmall = IsGridEditMode ? ImageResources.DCLGridEditEnd16.GetBitmapImage() : ImageResources.DCLGridEditBegin16.GetBitmapImage();
            _gridEditMenuItem.ImageLarge = IsGridEditMode ? ImageResources.DCLGridEditEnd32.GetBitmapImage() : ImageResources.DCLGridEditBegin32.GetBitmapImage();
        }

        #endregion .  Commands  .

        private bool CanCud()
        {
            var canCud = !IsReadOnly && !IsGridEditMode
                && ParentViewModelSource != null && _itemType != null
                && (!ShouldUpdateSeparately || !ParentViewModelSource.IsNew);

            if (!canCud)
                return false;

            if (!ShouldUpdateSeparately)
                return true;

            return !ParentViewModelSource.IsDirty;
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

            _newMenuItem = new CommandMenuItem
            {
                Caption = StringResources.New,
                Command = NewCommand,
                ImageSmall = ImageResources.DCLAddNew16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLAddNew32.GetBitmapImage(),
                GlyphSize = GlyphSizeType.Small,
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                IsVisible = true,
                Priority = 1
            };
            bar.MenuItems.Add(_newMenuItem);

            _editMenuItem = new CommandMenuItem
            {
                Caption = StringResources.Edit,
                Command = EditCommand,
                //HotKey = new KeyGesture(Key.F6),
                ImageSmall = ImageResources.DCLEdit16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLEdit32.GetBitmapImage(),
                GlyphSize = GlyphSizeType.Small,
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                IsVisible = true,
                Priority = 2
            };
            bar.MenuItems.Add(_editMenuItem);

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
                Priority = 3
            });

            _gridEditMenuItem = new CommandMenuItem
            {
                Caption = StringResources.MenuCaptionEditTable,
                Command = GridEditCommand,
                ImageSmall = ImageResources.DCLGridEditBegin16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLGridEditBegin32.GetBitmapImage(),
                //HotKey = new KeyGesture(Key.F9),
                GlyphSize = GlyphSizeType.Small,
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                //Is Visible = ShouldUpdateSeparately
                Priority = 4
            };
            bar.MenuItems.Add(_gridEditMenuItem);
        }

        private bool? ViewObject(ref object source, bool isadd)
        {
            const SettingDisplay displaySetting = SettingDisplay.Detail;

            IObjectViewModel editModel;
            if (CustomEditModel == null)
            {
                var destType = typeof (IObjectViewModel<>).MakeGenericType(_itemType);
                editModel = (IObjectViewModel) IoC.Instance.Resolve(destType, null);
            }
            else
            {
                editModel = CustomEditModel();
            }

            // выставляем режим модели
            editModel.Mode = ShouldUpdateSeparately ? ObjectViewModelMode.SubObject : ObjectViewModelMode.MemoryObject;

            editModel.ParentViewModelSource = ParentViewModelSource;

            var obj = source as WMSBusinessObject;
            if (obj != null && obj.HasPrimaryKey() && editModel.Mode != ObjectViewModelMode.MemoryObject && !isadd)
            {
                // Получаем весь объект
                using (var mgr = GetManager(obj.GetType()))
                {
                    if (mgr != null)
                        obj = mgr.Get(obj.GetKey()) as WMSBusinessObject;
                }
            }

            editModel.SetSource(obj);
            editModel.DisplaySetting = displaySetting;

            try
            {
                if (editModel.Mode != ObjectViewModelMode.Object)
                {
                    editModel.IsAdd = isadd;
                    if (isadd)
                    {
                        editModel.IsVisibleMenuSaveAndContinue = true;
                        editModel.SourceBase = obj == null ? null : (WMSBusinessObject)obj.Clone();
                        editModel.CollectionChanged -= OnSourceCollectionChanged;
                        editModel.CollectionChanged += OnSourceCollectionChanged;
                    }
                    else
                    {
                        editModel.IsVisibleMenuSaveAndContinue = false;
                    }
                    editModel.NeedRefresh += OnNeedRefresh;
                }

                var modelCapt = editModel as PanelViewModelBase;
                if (modelCapt != null)
                {
                    modelCapt.SetPanelCaptionPrefix(DataContext.GetType());
                }

                gridControl.ShowLoadingPanel = false;
                var result = GetViewService().ShowDialogWindow(editModel, true, true, "50%", "50%", true);
                if (result == true)
                {
                    return true;
                }

                source = editModel.GetSource();
                return editModel.IsNeedRefresh;
            }
            finally
            {
                editModel.CollectionChanged -= OnSourceCollectionChanged;
                editModel.NeedRefresh -= OnNeedRefresh;
            }
        }

        private void FillFields()
        {
            var fields = DataFieldHelper.Instance.GetDataFields(_itemType, SettingDisplay.LookUp);

            foreach (var field in fields)
            {
                field.AllowAddNewValue = true;
                if (IsGridEditMode)
                    field.IsEnabled = field.EnableEdit;
            }

            // INFO: делаем это для сохранения имен полей (нужно для настроек грида) и заполнения полей с look up
            // INFO: отключили возможность править поля со списками
            //FillLookUpFields(fields);

            Fields = new ObservableCollection<DataField>(fields);
        }

        public void OnSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
            {
                if (Source == null)
                    Source = Activator.CreateInstance(_itemsType) as IList;
                if (Source != null)
                {
                    foreach (var item in e.NewItems)
                    {
                        if (item != null)
                            Source.Add(item);
                    }
                }
            }
        }

        public static void OnSourcePropertyInternalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SubListView)d).OnSourcePropertyInternalChanged(e.NewValue);
        }

        public void OnSourcePropertyInternalChanged(object newvalue)
        {
            if (newvalue == null)
                CurrentItem = null;
        }

        public override void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            base.OnDataContextChanged(sender, e);
            
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
        private void TableView_ShowingEditor(object sender, ShowingEditorEventArgs e)
        {
            var valueEditController = ParentViewModel as IValueEditController;
            if (valueEditController != null)
                e.Cancel = !valueEditController.EnableEdit(e.Row, e.Column.FieldName);
        }

        private void BarItem_OnItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.Item.Content == null)
                return;

            var cont = e.Item.Content.ToString();
            if (string.IsNullOrEmpty(cont))
                return;

            if (cont.Equals(StringResources.Copy))
            {
                var oldMode = gridControl.ClipboardCopyMode;
                try
                {
                    gridControl.ByCheckCopyMode = false;
                    gridControl.ClipboardCopyMode = ClipboardCopyMode.ExcludeHeader;
                    //SendKeys.SendWait("^C");
                    gridControl.CopyToClipboard();
                    return;
                }
                finally
                {
                    gridControl.ClipboardCopyMode = oldMode;
                    gridControl.ByCheckCopyMode = true;
                }
            }

            var tableView = gridControl.View as TableView;
            if (tableView == null)
                return;

            var cells = tableView.GetSelectedCells().ToList();
            if (cells.Count == 0)
                return;
            if (cells.FirstOrDefault(c => c.Column.AllowEditing == DefaultBoolean.True) == null)
                return;

            if (cont.Equals(StringResources.Delete))
            {
                ChangeValue(cells, null);
                return;
            }

            if (cont.Equals(StringResources.Paste))
                ChangeValue(cells, Clipboard.GetText());
        }

        private void ChangeValue(ICollection<GridCell> cells, string value)
        {
            if (string.IsNullOrEmpty(value) || !(value.Contains("\r\t") || value.Contains("\n")))
            {
                foreach (var cell in cells)
                {
                    try
                    {
                        var row = cell.RowHandle;
                        var columnName = cell.Column.FieldName;
                        if ((cell.Column.AllowEditing == DefaultBoolean.True)
                            && cell.Column.ActualEditSettings.GetType() != typeof(CustomGridSimpleLookupEditSettings))
                            gridControl.SetCellValue(row, columnName, value);
                    }
                    catch(Exception ex)
                    {
                        Log.Debug(ex);
                    }
                }
                return;
            }

            var rows = value.TrimEnd('\n').Split('\n');
            var minIndexRow = cells.Min(c => c.RowHandle);
            var maxIndexRow = cells.Count == 1 ? minIndexRow + rows.Length - 1 : cells.Max(c => c.RowHandle);

            for (var i = minIndexRow; i <= maxIndexRow; i++)
            {
                var k = (i - minIndexRow) % rows.Length;
                var values = rows[k].Trim('\r').Split('\t');

                var minIndexColumn = cells.Min(c => c.Column.VisibleIndex);
                var maxIndexColumn = cells.Count == 1 ? minIndexColumn + values.Length - 1 : cells.Max(c => c.Column.VisibleIndex);

                for (var j = minIndexColumn; j <= maxIndexColumn; j++)
                {
                    if ((gridControl.Columns.First(c => c.VisibleIndex == j && c.Visible).AllowEditing != DefaultBoolean.True) ||
                         (gridControl.Columns.First(c => c.VisibleIndex == j && c.Visible).ActualEditSettings.GetType() == typeof(CustomGridSimpleLookupEditSettings)))
                        continue;
                    var h = (j - minIndexColumn) % values.Length;
                    try
                    {
                        gridControl.SetCellValue(i, gridControl.Columns.First(c => c.VisibleIndex == j && c.Visible).FieldName, values[h]);
                    }
                    catch(Exception ex)
                    {
                        Log.Debug(ex);
                    }
                }
            }
        }

        private void GridControl_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                BarItem_OnItemClick(null, new ItemClickEventArgs(new BarButtonItem() { Content = StringResources.Paste }, null));
                e.Handled = true;
                return;
            }

            if (e.Key != Key.Delete || (Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
                return;

            BarItem_OnItemClick(null, new ItemClickEventArgs(new BarButtonItem() { Content = StringResources.Delete }, null));
            e.Handled = true;
        }

        private void GridControl_OnSelectionChanged(object sender, GridSelectionChangedEventArgs e)
        {
            SelectedItems = gridControl.SelectedItems;

            var handler = SelectionChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);

            RaiseCanExecuteChanged();
        }
    }
}
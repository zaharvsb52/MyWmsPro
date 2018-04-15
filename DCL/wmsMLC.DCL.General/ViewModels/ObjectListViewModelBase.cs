using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.Helpers;
using wmsMLC.DCL.General.ViewModels.Menu;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Helpers;
using wmsMLC.General.DAL;
using wmsMLC.General.PL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Helpers;
using wmsMLC.General.PL.WPF.Services;
using wmsMLC.General.PL.WPF.ViewModels;

namespace wmsMLC.DCL.General.ViewModels
{
    public class ObjectListViewModelBase<TModel> : ListViewModelBase<TModel>, IObjectListViewModel, IHelpHandler, IExportData
    {
        #region .  Fields  .
        private bool _allowAddNew;
        private bool _allowEditing;
        private bool _allowMultipleSelect;
        private bool _showColumnHeaders;
        private bool _showDetail;
        private bool _addingNewItem;

        private CommandMenuItem _miNew;
        private CommandMenuItem _miDelete;
        private CommandMenuItem _miEdit;
        private CommandMenuItem _miShowInNewWindow;

        public event EventHandler ShouldChangeSelectedItem;
        #endregion .  Fields  .

        public ObjectListViewModelBase()
        {
            NewCommand = new DelegateCustomCommand(New, CanNew);
            DeleteCommand = new DelegateCustomCommand(Delete, CanDelete);
            ShowInNewWindowCommand = new DelegateCustomCommand(ShowInNewWindow);
            QuickLinkMenuCommand = new DelegateCustomCommand<QuickLinkCommandParameter>(OnQuickLinkMenu, OnCanQuickLinkMenu);
            PivotCommand = new DelegateCustomCommand(ShowPivot);
            ExportCommand = new DelegateCustomCommand(ShowExport);

            SelectCommand = new DelegateCustomCommand(OnSelect, CanSelect);

            Commands.AddRange(new[] { NewCommand, DeleteCommand, QuickLinkMenuCommand, SelectCommand });
        }

        #region .  Properties  .
        public ICommand NewCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICommand ShowInNewWindowCommand { get; private set; }
        public ICommand QuickLinkMenuCommand { get; set; }
        public ICommand PivotCommand { get; set; }
        public ICommand ExportCommand { get; set; }


        public ObjectListMode Mode { get; set; }

        // в режиме LookUp
        public ICommand SelectCommand { get; private set; }

        /// <summary>
        /// Разрешение на добавление новых записей.
        /// </summary>
        public bool AllowAddNew
        {
            get { return _allowAddNew; }
            set
            {
                if (_allowAddNew == value)
                    return;
                _allowAddNew = value;
                OnPropertyChanged("AllowAddNew");
            }
        }

        public bool AllowEditing
        {
            get { return _allowEditing; }
            set
            {
                if (_allowEditing == value)
                    return;

                _allowEditing = value;
                OnPropertyChanged("AllowEditing");
            }
        }

        public bool AllowMultipleSelect
        {
            get { return _allowMultipleSelect; }
            set
            {
                if (_allowMultipleSelect == value)
                    return;

                _allowMultipleSelect = value;
                OnPropertyChanged("AllowMultipleSelect");
            }
        }

        /// <summary>
        /// Признак необходимости отображать заголовки в списках.
        /// </summary>
        public bool ShowColumnHeaders
        {
            get { return _showColumnHeaders; }
            set
            {
                _showColumnHeaders = value;
                OnPropertyChanged("ShowColumnHeaders");
            }
        }

        /// <summary>
        /// Признак необходимости отображать detail.
        /// </summary>
        public bool ShowDetail
        {
            get { return _showDetail; }
            set
            {
                _showDetail = value;
                OnPropertyChanged("ShowDetail");
            }
        }

        private object _selectedItem;
        public object SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (_selectedItem == value)
                    return;
                _selectedItem = value;
                OnPropertyChanged("SelectedItem");
                OnShouldChangeSelectedItem();
            }
        }

        public object EditValue { get; set; }
        public string ValueMember { get; set; }
        public IFilterViewModel CustomFilters { get { return Filters; } }
        #endregion .  Properties  .

        #region .  Methods  .

        #region Menu
        protected override void InitializeSettings()
        {
            base.InitializeSettings();

            ShowColumnHeaders = true;
            ShowDetail = false;
        }

        protected override void CreateMainMenu()
        {
            base.CreateMainMenu();

            if (!IsMenuEnable)
                return;
            _miNew = new CommandMenuItem
            {
                Caption = StringResources.New,
                Command = NewCommand,
                ImageSmall = ImageResources.DCLAddNew16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLAddNew32.GetBitmapImage(),
                HotKey = new KeyGesture(Key.F7),
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 110
            };

            _miDelete = new CommandMenuItem
            {
                Caption = StringResources.Delete,
                Command = DeleteCommand,
                ImageSmall = ImageResources.DCLDelete16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLDelete32.GetBitmapImage(),
                HotKey = new KeyGesture(Key.F9),
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 120
            };

            _miEdit = new CommandMenuItem
            {
                Caption = StringResources.Edit,
                Command = EditCommand,
                HotKey = EditKey,
                ImageSmall = ImageResources.DCLEdit16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLEdit32.GetBitmapImage(),
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 111
            };

            var barCommands = Menu.GetOrCreateBarItem(StringResources.Commands, 1);
            barCommands.MenuItems.Add(new SeparatorMenuItem());
            barCommands.MenuItems.AddRange(new[] { _miNew, _miDelete, _miEdit });

            var barCustomize = Menu.GetOrCreateBarItem(StringResources.CustomizationBarMenu, 30);
            _miShowInNewWindow = new CommandMenuItem
            {
                Caption = StringResources.OpenInNewWindow,
                Command = ShowInNewWindowCommand,
                ImageSmall = ImageResources.DCLDuplicateTab16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLDuplicateTab32.GetBitmapImage(),
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default
            };
            barCustomize.MenuItems.Add(_miShowInNewWindow);

            barCustomize.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.OlapData,
                Command = PivotCommand,
                ImageSmall = ImageResources.DCLOlapData16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLOlapData32.GetBitmapImage(),
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 80
            });

            barCustomize.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.ExportData,
                Command = ExportCommand,
                ImageSmall = ImageResources.DCLExportData16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLExportData32.GetBitmapImage(),
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 90
            });

            if (Mode == ObjectListMode.LookUpList || Mode == ObjectListMode.LookUpList3Points)
            {
                if (Menu != null && Menu.Bars != null)
                {
                    foreach (var bar in Menu.Bars.Where(p => p != null && p.Caption.EqIgnoreCase(StringResources.CustomizationBarMenu)).ToArray())
                    {
                        foreach (var menu in bar.MenuItems.Where(p => p.Caption.EqIgnoreCase(StringResources.Swap)).OfType<ListMenuItem>().ToArray())
                        {
                            foreach (var submenu in menu.MenuItems.Where(p => p.Caption.EqIgnoreCase(StringResources.Download)).ToArray())
                            {
                                menu.MenuItems.Remove(submenu);
                            }
                        }

                        foreach (var menu in bar.MenuItems.Where(p => p.Caption.EqIgnoreCase(StringResources.OpenInNewWindow)).ToArray())
                        {
                            bar.MenuItems.Remove(menu);
                        }
                    }
                }
                if (_history != null)
                    _history.IsVisible = false;
            }

            CreateQuickLinkBar();
        }

        protected override void CreateContextMenu()
        {
            base.CreateContextMenu();

            if (!IsContextMenuEnable)
                return;
            if (ContextMenu == null)
                ContextMenu = new MenuItemCollection { ParentName = "MenuItemCRUDContextMenu" };

            // CRUD items
            var commands = new[] { _miDelete.Clone(), _miEdit.Clone(), _history.Clone(), _miShowInNewWindow.Clone() };
            foreach (var mi in commands)
            {
                if (mi != null)
                {
                    mi.GlyphSize = GlyphSizeType.Small;
                    ContextMenu.Add(mi);
                }
            }
            ContextMenu.Add(new SeparatorMenuItem());

            // Print item
            if (Print != null)
            {
                var miPrint = Print.Clone();
                miPrint.GlyphSize = GlyphSizeType.Small;
                ContextMenu.Add(miPrint);
                ContextMenu.Add(new SeparatorMenuItem());
            }

            var entityQuickItems = GetEntityLinkMenuItems();
            foreach (var mi in entityQuickItems)
            {
                var cmi = (mi as CommonMenuItemBase);
                if (cmi != null)
                    cmi.GlyphSize = GlyphSizeType.Small;
                ContextMenu.Add(mi);
            }
            ContextMenu.Add(new SeparatorMenuItem());
        }

        private ObservableCollection<MenuItemBase> GetEntityLinkMenuItems()
        {
            var el = new ObservableCollection<MenuItemBase>();

            using (var mgrLink = (IEntityLinkManager)IoC.Instance.Resolve<IBaseManager<EntityLink>>())
            {
                var links = mgrLink.GetByEntityType(typeof(TModel));
                if (links != null)
                {
                    var commands = links.Select(i => new CommandMenuItem
                    {
                        Caption = i.EntityLinkName,
                        Command = QuickLinkMenuCommand,
                        CommandParameter = new QuickLinkCommandParameter
                        {
                            EntityName = i.EntityLinkTo,
                            Filter = i.EntityLinkFilter,
                            Action = i.EntityLinkType
                        },
                    });

                    el.AddRange(commands);
                }
            }
            return el;
        }

        protected virtual void CreateQuickLinkBar()
        {
            if (Mode != ObjectListMode.ObjectList)
                return;

            try
            {
                var entityQuickItems = GetEntityLinkMenuItems();
                if (entityQuickItems != null && entityQuickItems.Count > 0)
                {
                    var quickLink = new SubListMenuItem
                    {
                        Name = "SubListMenuItemQuickLink",
                        IsDynamicBarItem = true,
                        Caption = StringResources.QuickLink,
                        ImageSmall = ImageResources.DCLQuickLink16.GetBitmapImage(),
                        ImageLarge = ImageResources.DCLQuickLink32.GetBitmapImage(),
                        GlyphAlignment = GlyphAlignmentType.Top,
                        DisplayMode = DisplayModeType.Default,
                        Priority = 1000,
                        IsEnable = true
                    };
                    var quickBar = new BarItem
                    {
                        Caption = StringResources.QuickLink,
                        Priority = 1000,
                        Name = "BarItemQuickLink",
                        IsDynamicBarItem = true
                    };

                    foreach (var mi in entityQuickItems)
                    {
                        var cmi = (mi as CommonMenuItemBase);
                        if (cmi != null)
                            cmi.GlyphSize = GlyphSizeType.Small;
                        quickLink.MenuItems.Add(mi);
                    }

                    quickLink.MenuItems.Add(new SeparatorMenuItem());

                    quickBar.MenuItems.Add(quickLink);
                    Menu.Bars.Add(quickBar);
                }
            }
            catch (Exception ex)
            {
                if (!ExceptionHandler(ex, ExceptionResources.ContextMenuError))
                    throw;
            }
        }
        #endregion

        private bool OnCanQuickLinkMenu(QuickLinkCommandParameter parameter)
        {
            return HasSelectedItems() && !IsInFiltering &&
                parameter != null && !string.IsNullOrEmpty(parameter.EntityName);
        }

        protected virtual async void OnQuickLinkMenu(QuickLinkCommandParameter parameter)
        {
            if (!ConnectionManager.Instance.AllowRequest())
                return;
            if (!OnCanQuickLinkMenu(parameter))
                return;

            try
            {
                WaitStart();

                using (var objectManager = IoC.Instance.Resolve<ISysObjectManager>())
                {
                    var entitytype = objectManager.GetTypeByName(parameter.EntityName);
                    string filterExpression = null;
                    switch (parameter.Action)
                    {
                        case "HISTORY":
                            var ovm = IoC.Instance.Resolve(typeof(HistoryListViewModelBase<>).MakeGenericType(entitytype));
                            var lvm = ovm as IListViewModel;
                            if (lvm != null)
                            {
                                if (!string.IsNullOrEmpty(parameter.Filter))
                                    filterExpression = GetFilterQuickLink(parameter.Filter);
                                await ShowQuickLink(lvm, filterExpression);
                            }
                            break;
                        case "ARCHIVE":
                            throw new DeveloperException("No method - ViewArchive");
                        default:
                            var command = parameter.EntityName + ModuleBase.ViewServiceRegisterSuffixTreeShow;
                            IViewModel vm;
                            var viewservice = GetViewService();
                            if (!viewservice.TryResolveViewModel(command, out vm))
                            {
                                command = parameter.EntityName + ModuleBase.ViewServiceRegisterSuffixListShow;
                                viewservice.TryResolveViewModel(command, out vm);
                            }

                            if (vm == null)
                                throw new DeveloperException(DeveloperExceptionResources.CantCreateViewmodelForCommand, command);

                            if (parameter.Action == "WORK")
                            {
                                if (!string.IsNullOrEmpty(parameter.Filter))
                                    filterExpression = GetFilterQuickLink(parameter.Filter);
                            }
                            await ShowQuickLink(vm, filterExpression);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                if (!ExceptionHandler(ex, StringResources.Error))
                    throw;
            }
            finally
            {
                WaitStop();
            }
        }

        protected virtual async Task ShowQuickLink(IViewModel vm, string filterExpression)
        {
            GetViewService().Show(vm, new ShowContext { DockingType = DockType.Document, ShowInNewWindow = true });
            await TaskEx.Run(() =>
            {
                DispatcherHelper.Invoke(new Action(() =>
                {
                    var modelfilter = vm as IListViewModel;
                    if (modelfilter != null)
                        modelfilter.ApplyFilter(filterExpression);
                }));
            });
        }

        private string GetFilterQuickLink(string filter)
        {
            if (string.IsNullOrEmpty(filter)) return string.Empty;
            var newFilter = filter;
            var pattern = new Regex(@"\[(?<val>.*?)\]", RegexOptions.Compiled | RegexOptions.Singleline);
            foreach (Match m in pattern.Matches(newFilter))
            {
                var str = m.Groups["val"].Value;
                var pattern2 = new Regex(@"\$(?<val>.*?)(\s|'|$)", RegexOptions.Compiled | RegexOptions.Singleline);
                var strNew = string.Empty;
                foreach (var m2 in pattern2.Matches(str).Cast<Match>())
                {
                    var propName = m2.Groups["val"].Value;
                    if (string.IsNullOrEmpty(propName))
                        continue;

                    var isQuote = str.Contains(string.Format("'${0}'", propName));

                    var values = new List<string>();
                    var sb = str.Contains('=') ? new StringBuilder(str.Substring(0, str.IndexOf('=')) + " IN (") : new StringBuilder("(");
                        

                    foreach (var strValue in SelectedItems.Cast<WMSBusinessObject>().Select(item => item.GetProperty(propName)).
                        Select(i => CheckValue(i, isQuote)).Where(strValue => values.FirstOrDefault(strValue.Equals) == null))
                    {
                        var typeParam = TypeDescriptor.GetProperties(SelectedItems.FirstOrDefault()).Find(propName, true).PropertyType;
                        //TODO: сделать правильную обработку null полей
                        if (strValue.Contains("NULL") &&  values.Contains("'NULL'"))
                            continue;
                        var strParam = strValue == "NULL" && typeParam.IsAssignableFrom(typeof(string)) ? "'NULL'" : strValue;
                        values.Add(strParam);
                        if (values.Count == 1)
                            sb.Append(strParam);
                        else
                            sb.Append(",").Append(strParam);
                    }
                    
                    strNew = strNew + sb.Append(")");
                }
                newFilter = newFilter.Replace(string.Format("[{0}]", str), strNew);
            }
            return newFilter;
        }

        private string CheckValue(object value, bool isQuote = false)
        {
            if (value == null)
                return "NULL";
            if (isQuote || value is string)
                return string.Format("'{0}'", (value));
            if (value is Guid)
                return string.Format("{1}{0}{2}", (value), "{", "}");
            return string.Format("{0}", value);
        }

        protected override async void Edit()
        {
            base.Edit();
            if (!ConnectionManager.Instance.AllowRequest())
                return;

            if (!CanEdit())
                return;

            try
            {
                if (SelectedItems.Count == 0)
                    return;

                WaitStart();

                if (SelectedItems.Count == 1)
                {
                    await Show(SelectedItems[0], false);
                }
                else
                {
                    await Show(SelectedItems, false);
                }
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

        protected virtual bool CanDelete()
        {
            return !IsInFiltering && HasSelectedItems() && IsDelEnable;
        }

        protected virtual void Delete()
        {
            try
            {
                WaitStart();

                if (!ConnectionManager.Instance.AllowRequest())
                    return;

                if (!DeleteConfirmation()) return;

                using (var mgr = GetManager())
                    mgr.Delete(SelectedItems);
            }
            catch (Exception ex)
            {
                if (!ExceptionHandler(ex, ExceptionResources.ItemsCantDelete))
                    throw;
            }
            finally
            {
                WaitStop();
            }
        }

        protected virtual bool DeleteConfirmation()
        {
            var vs = GetViewService();
            var dr = vs.ShowDialog(StringResources.Confirmation
                , string.Format(StringResources.ConfirmationDeleteRecords, SelectedItems.Count)
                , MessageBoxButton.YesNo //MessageBoxButton.YesNoCancel
                , MessageBoxImage.Question
                , MessageBoxResult.Yes);

            return dr == MessageBoxResult.Yes;
        }

        protected virtual bool CanNew()
        {
            switch (Mode)
            {
                case ObjectListMode.LookUpList:
                case ObjectListMode.LookUpList3Points:
                    return !IsInFiltering && AllowAddNew;
                default:
                    return !IsInFiltering && IsNewEnable;
            }
        }

        protected virtual void New()
        {
            _addingNewItem = false;

            try
            {
                WaitStart();

                if (!ConnectionManager.Instance.AllowRequest())
                    return;

                SelectedItems.Clear();

                using (var mgr = GetManager())
                {
                    var newItem = mgr.New();
                    if (Mode == ObjectListMode.ObjectList)
                    {
                        _addingNewItem = true;
                        Show(newItem);
                    }
                    else
                    {
                        var vm = WrappModelIntoVM(newItem);
                        var sublistViewModel = vm as IObjectViewModel;
                        if (sublistViewModel != null && sublistViewModel.Mode != ObjectViewModelMode.Object)
                        {
                            var sourcebase = newItem as WMSBusinessObject;
                            sublistViewModel.SourceBase = sourcebase == null
                                ? null
                                : (WMSBusinessObject)sourcebase.Clone();
                            sublistViewModel.IsVisibleMenuSaveAndContinue = true;
                        }

                        var dialogresult = GetViewService()
                            .ShowDialogWindow(vm, true, isNotNeededClosingOnOkResult: true, height: "50%", width: "50%",
                                noButtons: true);

                        //Проверяем было ли сохранение
                        var vmNewItem = (TModel)((IModelHandler)vm).GetSource();
                        if (dialogresult == true || !ReferenceEquals(newItem, vmNewItem))
                        {
                            var obj = vmNewItem as IKeyHandler;
                            if (obj != null)
                            {
                                _addingNewItem = true;
                                ValueMember = null;
                                EditValue = obj.GetKey();
                            }
                        }
                    }
                }
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

        protected override void OnSorceAddedItem(TModel item)
        {
            base.OnSorceAddedItem(item);

            //Фокус на новую строку
            if (_addingNewItem && item != null)
            {
                _addingNewItem = false;
                SelectedItems.Add(item);
                SaveSelection();
            }
        }

        protected override void RestoreSelection()
        {
            base.RestoreSelection();

            //Возвращаем фокус на строку редактирования.
            if (!HasSelectedItems())
                return;

            if (Mode == ObjectListMode.LookUpList || Mode == ObjectListMode.LookUpList3Points)
            {
                var obj = SelectedItems[0] as IKeyHandler;
                if (obj != null)
                {
                    ValueMember = null;
                    EditValue = obj.GetKey();
                    //Изменяем выбранную строку
                    SelectedItem = obj;
                    return;
                }
            }
            SelectedItem = SelectedItems[0];
        }

        private bool CanSelect()
        {
            return !IsInFiltering && HasSelectedItems() && IsReadEnable;
        }

        private void OnSelect()
        {
            if (!CanSelect())
                return;

            SelectedItem = SelectedItems.First();
        }

        protected override ObservableCollection<DataField> GetFields(Type type, SettingDisplay settings)
        {
            switch (Mode)
            {
                case ObjectListMode.LookUpList:
                    return base.GetFields(typeof(TModel), SettingDisplay.LookUp);
                default:
                    return base.GetFields(type, settings);
            }
        }

        protected virtual async void Show(TModel model)
        {
            await Show(model, false);
        }

        protected virtual async Task Show(TModel model, bool allowNewWindow)
        {
            var vs = GetViewService();
            if (Mode == ObjectListMode.ObjectList)
            {
                var vm = WrappModelIntoVM(model);
                vs.Show(vm, new ShowContext { DockingType = DockType.Document, ShowInNewWindow = allowNewWindow });

                var modelHandler = vm as IModelHandler;
                if (modelHandler != null)
                    modelHandler.RefreshDataAsync();
            }
            else
            {
                var vm = await GetViewModelAsync(model);
                var sublistViewModel = vm as IObjectViewModel;
                if (sublistViewModel != null && sublistViewModel.Mode != ObjectViewModelMode.Object)
                {
                    sublistViewModel.IsVisibleMenuSaveAndContinue = false;
                }

                vs.ShowDialogWindow(vm, isRestoredLayout: true, isNotNeededClosingOnOkResult: true, height: "50%",
                    width: "50%", noButtons: true);
            }
        }

        private bool IsNew(TModel model)
        {
            var isn = model as IIsNew;
            return isn != null && isn.IsNew;
        }

        protected virtual async Task Show(IEnumerable<TModel> model, bool allowNewWindow)
        {
            IViewModel vm = await GetViewModelAsync(model);
            GetViewService().Show(vm, new ShowContext { DockingType = DockType.Document, ShowInNewWindow = allowNewWindow });
        }

        protected async Task<IViewModel> GetViewModelAsync(TModel model)
        {
            return await Task.Factory.StartNew(() =>
            {
                // если новая - показываем как есть
                IViewModel vm;
                if (IsNew(model))
                    vm = WrappModelIntoVM(model);
                else
                {
                    // пытаемся получить по ключу
                    var kh = model as IKeyHandler;
                    if (kh != null)
                    {
                        using (var mgr = GetManager())
                        {
                            var obj = mgr.Get(kh.GetKey());
                            if (obj == null)
                                throw new DALCustomException(string.Format("Запись с кодом '{0}' была удалена или перенесена", kh.GetKey()));
                            vm = WrappModelIntoVM(obj);
                        }
                    }
                    else
                        vm = WrappModelIntoVM(model);
                }

                return vm;
            });
        }

        protected async Task<IViewModel> GetViewModelAsync(IEnumerable<TModel> model)
        {
            return await Task.Factory.StartNew(() =>
            {
                IViewModel vm;
                List<TModel> itemsList = new List<TModel>();
                using (var mgr = GetManager())
                {
                    var filterList = FilterHelper.GetArrayFilterIn(typeof(TModel), model.Cast<object>());
                    foreach (var filter in filterList)
                    {
                        var res = mgr.GetFiltered(filter, GetModeEnum.Partial);
                        if (res.Any())
                            itemsList.AddRange(res);
                    }
                }
                if (model.Count() != itemsList.Count())
                {
                    var diff =
                            model.Where(
                                p =>
                                    itemsList.Cast<IKeyHandler>()
                                        .FirstOrDefault(i => i.GetKey().Equals(((IKeyHandler)p).GetKey())) == null)
                                .ToList();
                    if (diff.Any())
                        throw new OperationException("Объекты были удалены: {0}",
                            string.Join(",", diff.Cast<IKeyHandler>().Select(i => i.GetKey())));
                }
                vm = WrappModelIntoVM(itemsList.ToArray());
                return vm;
            });
        }

        protected virtual async void EditInNewWindow()
        {
            if (!ConnectionManager.Instance.AllowRequest())
                return;

            if (!CanEdit())
                return;

            try
            {
                WaitStart();

                if (SelectedItems.Count == 0)
                    return;

                if (SelectedItems.Count == 1)
                {
                    await Show(SelectedItems[0], true);
                }
                else
                {
                    await Show(SelectedItems, true);
                }
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

        protected virtual IViewModel WrappModelIntoVM(TModel model)
        {
            var ovm = IoC.Instance.Resolve<IObjectViewModel<TModel>>();
            ovm.SetSource(model);
            return ovm;
        }

        protected virtual IViewModel WrappModelIntoVM(IEnumerable<TModel> model)
        {
            var ovm = IoC.Instance.Resolve<IObjectViewModel<TModel>>();
            var pem = ovm as IPropertyEditHandler;
            if (pem == null)
                throw new DeveloperException("Модель не реализует IPropertyEditHandler");
            // включаем режим множественного редактирования
            pem.SetSource(model);
            return ovm;
        }

        private void ShowInNewWindow()
        {
            if (!ConnectionManager.Instance.AllowRequest())
                return;
            var vs = GetViewService();
            var vm = IoC.Instance.Resolve(GetType(), null) as ObjectListViewModelBase<TModel>;
            vs.Show(vm, new ShowContext { DockingType = DockType.Document, ShowInNewWindow = true });
        }

        private void ShowPivot()
        {
            if (!ConnectionManager.Instance.AllowRequest())
                return;
            var vs = GetViewService();
            var vm = IoC.Instance.Resolve<PivotViewModel<TModel>>();
            vm.Source = Source;
            //vs.ShowDialogWindow(viewModel: vm, isRestoredLayout: false, width: "95%", height: "90%");
            vs.ShowDialogWindow(viewModel: vm, isRestoredLayout: false);
        }

        private void ShowExport()
        {
            if (!ConnectionManager.Instance.AllowRequest())
                return;
            var vs = GetViewService();
            var vm = IoC.Instance.Resolve<ExportViewModel<TModel>>();
            vm.Source = Source;
            RiseSourceExport();
            vm.StreamExport = StreamExport;
            vs.ShowDialogWindow(vm, false);
        }

        protected virtual void OnShouldChangeSelectedItem()
        {
            var handler = ShouldChangeSelectedItem;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
        #endregion .  Methods  .

        #region . IHelpHandler .
        string IHelpHandler.GetHelpLink()
        {
            return "List";
        }

        string IHelpHandler.GetHelpEntity()
        {
            return typeof(TModel).Name;
        }
        #endregion

        #region . IExportData .

        public event EventHandler SourceExport;

        public void RiseSourceExport()
        {
            var h = SourceExport;
            if (h != null)
                h(this, EventArgs.Empty);
        }

        public Stream StreamExport { get; set; }

        #endregion
    }

    public class QuickLinkCommandParameter
    {
        public string ParentPropertyName { get; set; }
        public string EntityName { get; set; }
        public string PropertyName { get; set; }
        public bool IsCollection { get; set; }
        public string Filter { get; set; }
        public string Action { get; set; }
    }
}
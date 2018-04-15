using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
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
using System.Xml.Linq;
using DevExpress.Mvvm;
using MLC.Ext.Wpf.ViewModels;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.DCL.General.ViewModels.Ext;
using WebClient.Common.Client.Protocol.DataTransferObjects.Query;
using IoC = wmsMLC.General.IoC;
using LoadOptions = System.Xml.Linq.LoadOptions;

namespace wmsMLC.DCL.General.ViewModels
{
    /// <summary>
    /// Базовый класс для всех ViewModel, которым необходимо предоставить функционал над списками (List, Tree, ...)
    /// </summary>
    /// <typeparam name="TModel">Тип объекта списка</typeparam>
    public class ListViewModelBase<TModel> : BusinessProcessViewModelBase<ObservableCollection<TModel>, TModel>,
        IHaveUniqueName, IListViewModel<TModel>, ISettingsNameHandler, ISelectable
    {
        #region .  Fields && Consts  .
        public const string GetHistoryMethodName = "GetHistory";
        public const string IsInFilteringPropertyName = "IsInFiltering";
        public const string SelectedItemsPropertyName = "SelectedItems";

        protected static Regex AndOrRegex = new Regex("AND|OR");
        protected static Regex FilterMaxRowCountRegex = new Regex(StringResources.FilterMaxRowCount);

        private log4net.ILog _log = log4net.LogManager.GetLogger(typeof(ListViewModelBase<TModel>));
        private ObservableCollection<DataField> _fields;
        private ObservableCollection<TModel> _selectedItems;
        protected bool Isusedfilter;
        private string _totalRowItemAdditionalInfo;
        private bool _totalRowItemFilteredSymbolIsVisible;
        private bool _isFilterVisible;
        private bool _isInFiltering;
        private IBaseManager<TModel> _managerForChangesMonitoring;
        protected CommandMenuItem _history;
        private CommandMenuItem _filter;
        private CommandMenuItem _applyFilter;
        private bool _isApplyFilter;
        private object[] _lastSelectedKeys;
        private IFilterViewModel<TModel> _filters;
        private bool _isCloseDoubleClick;
        private bool _isNeedRefresh;
        private bool _isNeedClearCache;
        private bool _isRefreshAlways;
        private string _suffix;
        private string _filterString;

        #endregion

        public ListViewModelBase()
        {
            // ReSharper disable DoNotCallOverridableMethodsInConstructor
            IsMainMenuEnable = true;
            IsRefreshAlways = false;
            IsNeedClearCache = true;
            IsLoadUnloadDataEnable = true;
            EditKey = new KeyGesture(Key.F6);

            InitilizeManagerEventsMonitoring();

            PanelCaption = GetPanelCaption();
            PanelCaptionImage = GetPanelImage();
            // ReSharper restore DoNotCallOverridableMethodsInConstructor

            SelectedItems = new ObservableCollection<TModel>();

            FilterCommand = new DelegateCustomCommand(Filter, CanFilter);
            RefreshCommand = new DelegateCustomCommand(OnRefreshData, CanRefreshData);
            ShowHistoryCommand = new DelegateCustomCommand(ShowHistory, CanShowHistory);
            GridFilterChangedCommand = new DelegateCustomCommand(() => TotalRowItemAdditionalInfo = null);
            DownloadXmlCommand = new DelegateCustomCommand(DownloadXml, CanDownloadXml);
            UnloadXmlCommand = new DelegateCustomCommand(UnloadXml, CanUnloadXml);
            EditCommand = new DelegateCustomCommand(Edit, CanEdit);

            _isApplyFilter = false;
            Filters.ToDefault();
            Commands.AddRange(new[]
            {
                FilterCommand, RefreshCommand, SaveLayoutCommand, SaveDBLayoutCommand, SaveDBLayoutUpVersionCommand, ClearLayoutCommand, 
                ShowHistoryCommand, PrintCommand, UnloadXmlCommand, EditCommand
            });

            FillRigths();
            //CreateMainMenu();
        }

        #region .  Properties  .

        protected KeyGesture EditKey { get; set; }

        public IFilterViewModel<TModel> Filters
        {
            get { return _filters ?? (_filters = CreateFilterViewModel()); }
        }

        public ICommand FilterCommand { get; private set; }
        public ICommand RefreshCommand { get; private set; }
        public ICommand AppearanceStyleCommand { get; private set; }
        public ICommand ShowHistoryCommand { get; private set; }
        public ICommand GridFilterChangedCommand { get; private set; }
        public ICommand DownloadXmlCommand { get; set; }
        public ICommand UnloadXmlCommand { get; set; }

        public ICommand EditCommand { get; private set; }

        public bool IsHistoryEnable { get; protected set; }

        public bool IsLoadUnloadDataEnable { get; set; }

        public bool IsMainMenuEnable { get; set; }

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

        public bool IsCloseDoubleClick
        {
            get { return _isCloseDoubleClick; }
            set
            {
                if (_isCloseDoubleClick == value)
                    return;

                _isCloseDoubleClick = value;
                OnPropertyChanged("IsCloseDoubleClick");
            }
        }

        public ObservableCollection<TModel> SelectedItems
        {
            get { return _selectedItems; }
            set
            {
                if (_selectedItems == value)
                    return;

                if (_selectedItems != null)
                    _selectedItems.CollectionChanged -= SelectedItemsCollectionChanged;

                _selectedItems = value;

                if (_selectedItems != null)
                    _selectedItems.CollectionChanged += SelectedItemsCollectionChanged;

                OnPropertyChanged(SelectedItemsPropertyName);
            }
        }

        public ObservableCollection<DataField> Fields
        {
            get { return _fields ?? (_fields = GetFields(typeof(TModel), SettingDisplay.List)); }
        }

        public virtual void InitializeMenus()
        {
            InitializeCustomizationBar();
            CreateMainMenu();
            if (IsMainMenuEnable)
                CreateProcessMenu();

            //Внимание! Метод должен быть последним
            CreateContextMenu();
        }

        public bool TotalRowItemFilteredSymbolIsVisible
        {
            get { return _totalRowItemFilteredSymbolIsVisible; }
            set
            {
                if (_totalRowItemFilteredSymbolIsVisible == value) return;
                _totalRowItemFilteredSymbolIsVisible = value;
                OnPropertyChanged("TotalRowItemFilteredSymbolIsVisible");
            }
        }

        public string TotalRowItemAdditionalInfo
        {
            get { return _totalRowItemAdditionalInfo; }
            set
            {
                if (_totalRowItemAdditionalInfo == value) return;
                _totalRowItemAdditionalInfo = value;
                OnPropertyChanged("TotalRowItemAdditionalInfo");
            }
        }

        // Признак, что данные были измененны, но update данных не происходил
        public bool IsNeedRefresh
        {
            get { return _isNeedRefresh; }
            set
            {
                if (_isNeedRefresh == value) return;
                _isNeedRefresh = value;
                OnPropertyChanged("IsNeedRefresh");
            }
        }

        // Всегда обновлять, не учитывая IsNeedRefresh
        public bool IsRefreshAlways
        {
            get { return _isRefreshAlways; }
            set
            {
                if (_isRefreshAlways == value) return;
                _isRefreshAlways = value;
                OnPropertyChanged("IsRefreshAlways");
            }
        }

        // перед получением данных нужно ли их очищать
        public bool IsNeedClearCache
        {
            get { return _isNeedClearCache; }
            set
            {
                if (_isNeedClearCache == value) return;
                _isNeedClearCache = value;
                OnPropertyChanged("IsNeedClearCache");
            }
        }

        /// <summary>
        /// Признак того, что в данный момент осуществляется отбор данных
        /// </summary>
        protected bool IsInFiltering
        {
            get { return _isInFiltering; }
            set
            {
                if (_isInFiltering == value)
                    return;

                _isInFiltering = value;
                OnPropertyChanged(IsInFilteringPropertyName);
                // обновляем комманды, т.к. на многие должны быть заведены на это св-во
                RiseCommandsCanExecuteChanged();
            }
        }

        // фильтр применяемый к таблице
        public string FilterString
        {
            get { return _filterString; }
            set
            {
                if (_filterString == value) return;
                _filterString = value;
                OnPropertyChanged("FilterString");
            }
        }

        protected virtual string ViewServiceRegisterSuffix
        {
            get { return ModuleBase.ViewServiceRegisterSuffixListShow; }
        }

        public bool IsSelectedFirstItem { get; set; }
        #endregion .  Properties  .

        #region .  Methods  .

        #region .  Command methods  .
        //[Obsolete]
        //protected virtual ObservableCollection<DataField> GetDataFields()
        //{
        //    return DataFieldHelper.Instance.GetDataFields(typeof(TModel), SettingDisplay.List);
        //}

        private bool CanFilter()
        {
            return !IsInFiltering && IsReadEnable;
        }

        private void Filter()
        {
            IsFilterVisible = !IsFilterVisible;
        }

        protected virtual bool CanRefreshData()
        {
            return !IsInFiltering && IsReadEnable;
        }

        public override void RefreshData()
        {
            OnRefreshData();
        }

        protected virtual void OnRefreshData()
        {
            if (!ConnectionManager.Instance.AllowRequest())
                return;

            if (!CanRefreshData())
                return;

            SaveSelection();

            ApplyFilter();
        }

        private bool CanAppearanceStyle()
        {
            return IsCustomizeEnabled;
        }

        private void OnAppearanceStyle()
        {
            if (!CanAppearanceStyle())
                return;
            IsCustomization = false; //TODO: Пока так. Не сбрасывается флажок после закрытия формы подсветки
            IsCustomization = true;
        }

        protected virtual bool CanUnloadXml()
        {
            return !IsInFiltering && HasSelectedItems() && IsUnloadXmlEnabled;
        }

        protected virtual XmlDocument GetXmlDocument(WMSBusinessObject item)
        {
            return XmlDocumentConverter.ConvertFrom(item);
        }

        private void UnloadXml()
        {
            if (!CanUnloadXml())
                return;

            var exMess = string.Empty;
            var dlg = new FolderBrowserDialog
                {
                    Description = StringResources.SelectCompilePath,
                    ShowNewFolderButton = true,
                };
            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            string fileName = null;

            try
            {
                WaitStart();
                var path = dlg.SelectedPath;
                var filterExpression = FilterHelper.GetFilterIn(typeof(TModel), SelectedItems.Cast<IKeyHandler>());

                // Получение объекта с вложенными, ибо в Grid попадает сокращенный
                var items = GetManager().GetFiltered(filterExpression).ToArray();
                fileName = Path.Combine(path, string.Format("{0}.{1}.xml", GetSecurityType().Name, BPH.GetSystemDate().ToString("yyyy-MM-dd-hh-mm-ss")));
                var xDocument = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
                xDocument.Add(new XElement("Root"));

                foreach (var item in items.Cast<WMSBusinessObject>())
                {
                    var xmlItem = GetXmlDocument(item);
                    var xElement = XElement.Parse(xmlItem.InnerXml);
                    var xElementRoot = xDocument.Element("Root");
                    if (xElementRoot != null)
                        xElementRoot.Add(xElement);
                }
                xDocument.Save(fileName);
            }
            catch (Exception ex)
            {
                exMess = exMess + Environment.NewLine + string.Format(StringResources.UnloadError, fileName, ex.Message);
            }
            finally
            {
                WaitStop();
            }

            if (string.IsNullOrEmpty(exMess))
                GetViewService().ShowDialog(StringResources.Information,
                                            StringResources.UnloadSuccess,
                                            MessageBoxButton.OK,
                                            MessageBoxImage.Information,
                                            MessageBoxResult.Yes);
            else
                GetViewService().ShowDialog(StringResources.Error,
                                            exMess,
                                            MessageBoxButton.OK,
                                            MessageBoxImage.Error,
                                            MessageBoxResult.Yes);
        }

        protected virtual bool CanDownloadXml()
        {
            return !IsInFiltering && IsDownloadXmlEnabled;
        }

        protected virtual object DownloadXml(XmlDocument xmlDoc, IUnitOfWork uow, ref bool? isInsert)
        {
            using (var mgr = GetManager())
            {
                mgr.SetUnitOfWork(uow);
                var entity = XmlDocumentConverter.ConvertTo(GetSecurityType(), xmlDoc);

                //Не делаем валидацию при добавлении. Даем возможность загружать вложенные сущности

                var key = ((WMSBusinessObject)entity).GetKey();
                if (key == null || (mgr.Get(key) == null && !(typeof(decimal)).IsInstanceOfType(key)))
                {
                    mgr.Insert(ref entity);
                    return entity;
                }

                // Update - только если ключ не число
                if ((mgr.Get(key) != null && !(typeof(decimal)).IsInstanceOfType(key)))
                {
                    var valid = entity as wmsMLC.General.BL.Validation.IValidatable;
                    if (valid != null)
                    {
                        valid.Validate();
                        if (valid.Validator.HasCriticalError())
                            throw new DeveloperException(valid.Validator.Errors.ToString().Replace("\r\n", ""));
                    }

                    //Сохраняем вложенные сущности
                    var customXmlSerializable = entity as ICustomXmlSerializable;
                    if (customXmlSerializable != null)
                        customXmlSerializable.OverrideIgnore = false;
                    mgr.Update(entity);
                    return entity;
                }

                // Иначе insert, без указания ключа
                if (isInsert == null)
                {
                    var res = GetViewService().ShowDialog(StringResources.Download,
                        StringResources.InsertAnyway, MessageBoxButton.YesNo,
                        MessageBoxImage.Question, MessageBoxResult.Yes);
                    isInsert = res == MessageBoxResult.Yes;
                }
                if (isInsert == false)
                    return false;

                // Ключа не должно быть
                ((WMSBusinessObject)entity).SetProperty(((WMSBusinessObject)entity).GetPrimaryKeyPropertyName(), null);
                mgr.Insert(ref entity);
                return entity;
            }
        }

        private void DownloadXml()
        {
            if (!CanDownloadXml())
                return;

            var sb = new StringBuilder();
            var dlg = new OpenFileDialog()
            {
                Filter = string.Format("xml files ({0}.*.xml)|{0}.*.xml|All files (*.*)|*.*", GetSecurityType().Name),
                Multiselect = true
            };

            bool? isInsert = null;

            try
            {
                WaitStart();
                if (dlg.ShowDialog() != DialogResult.OK) return;

                foreach (var fileName in dlg.FileNames)
                {
                    //var uowFactory = IoC.Instance.Resolve<IUnitOfWorkFactory>();
                    //using (var uow = uowFactory.Create(false))
                    using (var uow = UnitOfWorkHelper.GetUnit())
                    {
                        uow.TimeOut = 600000; //10 минут
                        uow.BeginChanges();

                        try
                        {
                            var xDoc = XDocument.Load(fileName, LoadOptions.None);
                            if (xDoc.Root != null)
                            {
                                var rootCategory = xDoc.Root.Elements();
                                foreach (var item in rootCategory)
                                {
                                    var xmlDoc = new XmlDocument();
                                    xmlDoc.LoadXml(item.ToString());
                                    var ar = new[] { "TRANSACT", "USERINS", "USERUPD", "DATEINS", "DATEUPD" };
                                    foreach (var s in ar)
                                        RemoveChildInXML(ref xmlDoc, s);
                                    DownloadXml(xmlDoc, uow, ref isInsert);

                                }
                            }

                            uow.CommitChanges();
                        }
                        catch (Exception ex)
                        {
                            uow.RollbackChanges();
                            var msg = string.Format(StringResources.DownloadError, fileName, ExceptionHelper.GetErrorMessage(ex, false));
                            sb.AppendLine(msg);
                        }
                    }

                    if (sb.Length != 0)
                        break;
                }
            }
            finally
            {
                WaitStop();
            }

            var errorMsgStr = sb.ToString();

            if (isInsert == false)
                return;

            if (string.IsNullOrEmpty(errorMsgStr))
                GetViewService().ShowDialog(StringResources.Information,
                    StringResources.DownloadSuccess,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information,
                    MessageBoxResult.Yes);
            else
                GetViewService().ShowDialog(StringResources.Error,
                    errorMsgStr,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.Yes);
        }

        private void RemoveChildInXML(ref XmlDocument xmlDoc, string name)
        {
            if (xmlDoc.DocumentElement != null)
            {
                var cn = xmlDoc.DocumentElement.ChildNodes;
                foreach (var n in from XmlNode n in cn where n.Name.EqIgnoreCase(name) select n)
                    xmlDoc.DocumentElement.RemoveChild(n);
            }
        }

        protected override TModel[] GetPrintedItems()
        {
            return SelectedItems == null ? null : SelectedItems.ToArray();
        }

        protected override bool CanPrintReport()
        {
            return HasSelectedItems();
        }

        protected virtual bool CanShowHistory()
        {
            return HasSelectedItems() && IsHistoryEnable;
        }

        private async void ShowHistory()
        {
            try
            {
                WaitStart();

                var vs = GetViewService();
                var entity = typeof(TModel).Name.ToUpper();

                await vs.GetWebEntityMappingAsync(entity);
                string webentity;
                if (!vs.TryGetWmsWebObject(entity, out webentity))
                    throw new DeveloperException(ExceptionResources.WmsWebMappingNotFoundFormat, entity);

                //Вызываем старый код
                if (webentity == "NONE")
                {
                    if (!ConnectionManager.Instance.AllowRequest())
                        return;

                    var ovm = IoC.Instance.Resolve<HistoryListViewModelBase<TModel>>();
                    // Устанавливаем Default значения перед FilterExpression, иначе они не применяются
                    ovm.Filters.ToDefault();
                    ovm.Filters.FilterExpression = GetFilterForHistory();
                    ovm.ApplyFilter();

                    vs.Show(ovm, new ShowContext { DockingType = DockType.Document, ShowInNewWindow = true });
                    return;
                }

                foreach (var item in SelectedItems)
                {
                    var kh = item as IKeyHandler;
                    if (kh != null)
                    {
                        var pkname = kh.GetPrimaryKeyPropertyName();
                        string webpkname;
                        if (!vs.TryGetWmsWebObject(pkname, out webpkname))
                        {
                            var wmspkname = entity + ".PK";
                            if (!vs.TryGetWmsWebObject(wmspkname, out webpkname))
                                throw new DeveloperException(ExceptionResources.WmsWebMappingNotFoundFormat, pkname);
                        }

                        var pkvalue = kh.GetKey();
                        var factory = IoC.Instance.Resolve<IEntityViewModelFactory>();
                        var entityType = string.Format("{0}History", webentity);
                        var vm = factory.CreateEntityList<EntityJournalHistoryViewModel>(entityType);

                        var title = string.Format("{0}: {1} ({2})", StringResources.History, PanelCaption, pkvalue);
                        var rootFilter = new MLC.Ext.Common.Model.Filter
                        {
                            Property = webpkname,
                            Operator = JsFilterOperator.EQ,
                            Value = pkvalue
                        };
                        vm.FiltersSet(new[] { rootFilter });
                        vs.Show(viewModel: vm, id: title, title: title);
                    }
                    break;
                }

                //Старый код
                //if (!ConnectionManager.Instance.AllowRequest())
                //    return;

                //var ovm = IoC.Instance.Resolve<HistoryListViewModelBase<TModel>>();
                //// Устанавливаем Default значения перед FilterExpression, иначе они не применяются
                //ovm.Filters.ToDefault();
                //ovm.Filters.FilterExpression = GetFilterForHistory();
                //ovm.ApplyFilter();

                //vs.Show(ovm, new ShowContext { DockingType = DockType.Document, ShowInNewWindow = true });
                //Старый код
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
        #endregion

        protected override void InitializeSettings()
        {
            base.InitializeSettings();

            AllowClosePanel = true;
        }

        /// <summary>
        /// Заполнение парв на операции с данной моделью
        /// </summary>
        private void FillRigths()
        {
            // проверяем есть ли права
            IsHistoryEnable = Check(WMSBusinessObjectManager<WMSBusinessObject, int>.GetHistoryRightName);

            // если права есть, то проверяем, а поддерживает ли manager работу с историей
            //            if (IsHistoryEnable)
            //            {
            //                var mgrType = IoC.Instance.ResolveType<IBaseManager<TModel>>();
            //                IsHistoryEnable = typeof(IHistoryManager<TModel>).IsAssignableFrom(mgrType);
            //            }
        }

        protected virtual IFilterViewModel<TModel> CreateFilterViewModel()
        {
            var res = new FilterViewModel<TModel> { ApplyFilterCommand = RefreshCommand };
            res.IsRowCountEnabled = IsChangeRowCount;

            // получаем поля для фильтра
            var filterFields = GetFields(typeof(TModel), SettingDisplay.Filter);
            foreach (var field in filterFields)
            {
                // определяем специфичные поля (коллекции, объекты)
                var isCollection = typeof(IEnumerable).IsAssignableFrom(field.FieldType) && field.FieldType != typeof(string);
                var isObject = typeof(WMSBusinessObject).IsAssignableFrom(field.FieldType);
                if (isCollection || isObject)
                {
                    var itemType = field.FieldType;
                    if (isCollection)
                    {
                        var itemCollectionType = field.FieldType.GetGenericTypeFormInheritanceNode(typeof(BusinessObjectCollection<>));
                        if (itemCollectionType == null)
                            continue;
                        itemType = itemCollectionType.GetGenericArguments()[0];
                    }

                    var listFiled = GetFields(itemType, SettingDisplay.List);
                    if (listFiled != null)
                        foreach (var dataField in listFiled)
                        {
                            var innerField = (DataField)dataField.Clone();
                            innerField.Caption = string.Format("{0}.{1}", field.Caption, dataField.Caption);
                            innerField.FieldName = string.Format("{0}.{1}", field.FieldName, dataField.FieldName);
                            innerField.SourceName = string.Format("{0}.{1}", field.SourceName, dataField.SourceName);
                            res.Fields.Add(innerField);
                        }
                }
                else
                {
                    res.Fields.Add(field);
                }
            }
            return res;
        }

        /// <summary>
        /// Получение полей для отображения. Может быть перекрыт для изменения коллекций по-умолчанию
        /// </summary>
        /// <returns>Список полей для отображения в фильтре</returns>
        protected virtual ObservableCollection<DataField> GetFields(Type type, SettingDisplay settings)
        {
            return DataFieldHelper.Instance.GetDataFields(type, settings);
        }

        protected virtual bool HasSelectedItems()
        {
            if (SelectedItems == null)
                return false;

            return SelectedItems.Count > 0;
        }

        protected void SaveSelection()
        {
            // запоминаем ключи
            _lastSelectedKeys = SelectedItems.OfType<IKeyHandler>()
                                                .Select(i => i.GetKey())
                                                .ToArray();
        }

        protected virtual void RestoreSelection()
        {
            SelectedItems.Clear();
            if (Source != null && _lastSelectedKeys != null && _lastSelectedKeys.Length > 0)
            {
                var newItems = Source.OfType<IKeyHandler>()
                    .Where(i => _lastSelectedKeys.Any(j => j != null && j.Equals(i.GetKey())))
                    .Cast<TModel>()
                    .ToArray();
                foreach (var newItem in newItems)
                    SelectedItems.Add(newItem);
            }
            RiseCommandsCanExecuteChanged();
        }

        protected override void OnSourceChanging(ObservableCollection<TModel> newValue)
        {
            base.OnSourceChanging(newValue);
            if (newValue != null)
                SaveSelection();
        }

        protected override void OnSourceChanged()
        {
            RestoreSelection();
            UpdateStatusRow();

            base.OnSourceChanged();
        }

        protected override void SourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            base.SourceCollectionChanged(sender, e);

            RestoreSelection();
            UpdateStatusRow();
        }

        private void UpdateStatusRow()
        {
            if (Source == null)
            {
                TotalRowItemFilteredSymbolIsVisible = false;
                TotalRowItemAdditionalInfo = null;
            }
            else
            {
                TotalRowItemFilteredSymbolIsVisible = (Filters != null && Source.Count == Filters.MaxRowCount);
            }
        }

        protected virtual void CreateMainMenu()
        {
            if (!IsMenuEnable || !IsMainMenuEnable)
                return;

            var bar = Menu.GetOrCreateBarItem(StringResources.Commands, 1, "BarItemCommands");

            _filter = new CommandMenuItem
                {
                    Caption = StringResources.Filter,
                    Command = FilterCommand,
                    ImageSmall = ImageResources.DCLFilter16.GetBitmapImage(),
                    ImageLarge = ImageResources.DCLFilter32.GetBitmapImage(),
                    HotKey = new KeyGesture(Key.F3),
                    GlyphAlignment = GlyphAlignmentType.Top,
                    DisplayMode = DisplayModeType.Default,
                    Priority = 10
                };
            bar.MenuItems.Add(_filter);

            _applyFilter = new CommandMenuItem
            {
                Caption = StringResources.Filter,
                Command = FilterCommand,
                ImageSmall = ImageResources.DCLFilterApply16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLFilterApply32.GetBitmapImage(),
                HotKey = new KeyGesture(Key.F3),
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                IsVisible = false,
                Priority = 10
            };
            bar.MenuItems.Add(_applyFilter);

            bar.MenuItems.Add(new CommandMenuItem
                {
                    Caption = StringResources.RefreshData,
                    Command = RefreshCommand,
                    ImageSmall = ImageResources.DCLFilterRefresh16.GetBitmapImage(),
                    ImageLarge = ImageResources.DCLFilterRefresh32.GetBitmapImage(),
                    HotKey = new KeyGesture(Key.F5),
                    GlyphAlignment = GlyphAlignmentType.Top,
                    DisplayMode = DisplayModeType.Default,
                    Priority = 50
                });

            if (IsHistoryEnable)
            {
                _history = new CommandMenuItem
                    {
                        Caption = StringResources.History,
                        Command = ShowHistoryCommand,
                        HotKey = new KeyGesture(Key.F12),
                        ImageSmall = ImageResources.DCLHistory16.GetBitmapImage(),
                        ImageLarge = ImageResources.DCLHistory32.GetBitmapImage(),
                        DisplayMode = DisplayModeType.Default,
                        GlyphAlignment = GlyphAlignmentType.Top,
                        Keyword = "History",
                        // в самый конец
                        Priority = 1000
                    };
                bar.MenuItems.Add(_history);
            }

            var barTech = Menu.GetOrCreateBarItem(StringResources.CustomizationBarMenu, 30, "BarItemCustomization");

            if (!IsLoadUnloadDataEnable)
                return;

            var listmenu = new ListMenuItem
            {
                Name = "ListMenuItemSwap",
                Caption = StringResources.Swap,
                ImageSmall = ImageResources.DCLSwap16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLSwap32.GetBitmapImage(),
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default
            };

            listmenu.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.Download,
                Command = DownloadXmlCommand,
                ImageSmall = ImageResources.DCLDownload16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLDownload32.GetBitmapImage(),
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 10
            });

            listmenu.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.Unload,
                Command = UnloadXmlCommand,
                ImageSmall = ImageResources.DCLUnload16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLUnload32.GetBitmapImage(),
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 20
            });

            barTech.MenuItems.Add(listmenu);
        }

        protected virtual void CreateContextMenu()
        {
            InitializeContextMenu();
        }

        protected override void SetupCustomizeMenu(BarItem bar, ListMenuItem listmenu)
        {
            base.SetupCustomizeMenu(bar, listmenu);

            if (AppearanceStyleCommand == null)
                AppearanceStyleCommand = new DelegateCustomCommand(OnAppearanceStyle, CanAppearanceStyle);

            // Добавляем функционал подсветки (в начало списка)
            var minPriority = listmenu.MenuItems.Min(i => i.Priority);
            listmenu.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.AppearanceStyle,
                Command = AppearanceStyleCommand,
                ImageSmall = ImageResources.DCLAppearanceStyle16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLAppearanceStyle32.GetBitmapImage(),
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = minPriority - 100
            });

            // убираем функционал настройки самого layout-а
            var item = listmenu.MenuItems.FirstOrDefault(i => i.Caption == StringResources.CustomizeRegion);
            if (item != null)
                listmenu.MenuItems.Remove(item);
        }

        protected override object[] GetItemsParameter()
        {
            return SelectedItems.Cast<object>().ToArray();
        }

        protected virtual void SelectedItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Reset)
                RiseCommandsCanExecuteChanged();
        }

        protected virtual IBaseManager<TModel> GetManager()
        {
            return IoC.Instance.Resolve<IBaseManager<TModel>>();
        }

        protected virtual void InitilizeManagerEventsMonitoring()
        {
            _managerForChangesMonitoring = GetManager();
            _managerForChangesMonitoring.AllowMonitorChangesInOtherInsances = true;

            UnSubscribeManagerEventsMonitoring();

            _managerForChangesMonitoring.Changed += ManagerChanged;
            _managerForChangesMonitoring.Disposed += ManagerDisposed;
        }

        protected virtual void UnSubscribeManagerEventsMonitoring()
        {
            if (_managerForChangesMonitoring == null)
                return;
            _managerForChangesMonitoring.Changed -= ManagerChanged;
            _managerForChangesMonitoring.Disposed -= ManagerDisposed;
        }

        private void ManagerDisposed(object sender, EventArgs eventArgs)
        {
            var mgr = (IBaseManager<TModel>)sender;
            // отписываемся
            mgr.Disposed -= ManagerDisposed;
            mgr.Changed -= ManagerChanged;
        }

        protected virtual TManager GetManager<TManager>()
            where TManager : class//, IBaseManager<TModel>
        {
            return (TManager)GetManager();
        }

        protected virtual void ManagerChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            IsNeedRefresh = true;

            DispatcherHelper.Invoke(new Action(() =>
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Reset:
                        if (IsRefreshAlways)
                            RefreshData();
                        break;

                    case NotifyCollectionChangedAction.Add:
                        InsertOrUpdateSource(e.NewItems);
                        IsNeedRefresh = false;
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        RemoveItemsFromSource(e.OldItems);
                        IsNeedRefresh = false;
                        break;
                }
            }));
        }

        private void RemoveItemsFromSource(IList deletedItems)
        {
            if (Source == null || deletedItems == null || deletedItems.Count == 0)
                return;

            try
            {
                WaitStart();

                foreach (var list in deletedItems)
                {
                    var items = list as IList;
                    if (items == null || items.Count == 0)
                        continue;

                    foreach (var it in items)
                    {
                        var kh = it as IKeyHandler;
                        if (kh == null)
                            continue;

                        var key = kh.GetKey();
                        if (key == null)
                            continue;

                        var sourceItem = Source.FirstOrDefault(i => i is IKeyHandler && key.Equals(((IKeyHandler)i).GetKey()));
                        if (sourceItem != null)
                            Source.Remove(sourceItem);
                    }
                }
            }
            catch (Exception ex)
            {
                _log.WarnFormat("Не удалось удалить объект(ы) из журнала по событию обновления. {0}", ex.Message);
                _log.Debug(ex);
            }
            finally
            {
                WaitStop();
            }
        }

        private async void InsertOrUpdateSource(IList newItems)
        {
            if (Source == null || newItems == null)
                return;

            try
            {
                OnSourceUpdateStarted();
                WaitStart();
                SaveSelection();
                foreach (var list in newItems)
                {
                    var items = list as IList;
                    if (items == null || items.Count == 0)
                        continue;

                    var keyItems = items.OfType<IKeyHandler>().ToList();

                    // проверим могут ли новые элементы по текущему фильтру быть в журнале
                    var dbItems = await GetFilteredItems(items);
                    foreach (var it in dbItems)
                    {
                        var kh = it as IKeyHandler;
                        if (kh == null)
                            continue;

                        var key = kh.GetKey();
                        if (key == null)
                            continue;

                        var sourceItem = Source.FirstOrDefault(i => i is IKeyHandler && key.Equals(((IKeyHandler)i).GetKey()));
                        if (sourceItem != null)
                        {
                            var index = Source.IndexOf(sourceItem);
                            Source.Remove(sourceItem);
                            Source.Insert(index, it);
                        }
                        else
                        {
                            Source.Add(it);
                            OnSorceAddedItem(it);
                        }
                        var item = keyItems.FirstOrDefault(i => key.Equals((i.GetKey())));
                        if (item != null)
                            keyItems.Remove(item);
                    }

                    // убираем устаревшие
                    foreach (var kh in keyItems)
                    {
                        var key = kh.GetKey();
                        if (key == null)
                            continue;

                        var sourceItem = Source.FirstOrDefault(i => i is IKeyHandler && key.Equals(((IKeyHandler)i).GetKey()));
                        if (sourceItem != null)
                        {
                            Source.Remove(sourceItem);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.WarnFormat("Не удалось добавить/обновить объект(ы) в журнал по событию обновления. {0}", ex.Message);
                _log.Debug(ex);
            }
            finally
            {
                OnSourceUpdateCompleted();
                RestoreSelection();
                WaitStop();
            }
        }

        protected virtual void OnSorceAddedItem(TModel item)
        {
        }

        private async Task<IEnumerable<TModel>> GetFilteredItems(IList items)
        {
            var list = items.Cast<object>().ToArray();
            return await TaskEx.Run(() =>
            {
                //Если ничего не пришло - выходим
                if (list.Length == 0)
                    return new TModel[0];

                // проверим могут ли новые элементы по текущему фильтру быть в журнале
                var mainFilter = Filters.GetSqlExpression();
                const string filterformat = "{0} AND {1}";
                var filterextlength = string.Format(filterformat, mainFilter, null).Length;

                //TODO: Реализовать возможность добавление доп. фильтров не только в конец, но и перед основным фильтром
                var filtersList = FilterHelper.GetArrayFilterIn(typeof(TModel), list,
                    new string(' ', filterextlength));

                var mgr = GetManager();
                //Оптимизация обновления, критерий - минимум обращений к серверу
                var filter = filtersList.Length > 1
                    ? mainFilter
                    : string.Format(filterformat, mainFilter, filtersList[0].TrimEnd());
                return mgr.GetFiltered(filter, GetModeEnum.Partial).ToArray();
            });
        }

        public virtual void ApplyFilter()
        {
            // фиксируем введенные значения
            Filters.RiseFixFilterExpression();

            var sqlFilter = Filters.GetSqlExpression();

            // запускаем фильтрацию
            if (string.IsNullOrEmpty(FilterString))
                ApplySqlFilter(sqlFilter);
            else
            {
                var listFilter = Filters.GetSqlExpression(FilterChangeNameToDB(FilterString));
                var resultFilter = string.IsNullOrEmpty(sqlFilter)
                    ? listFilter
                    : string.Format("{0} AND ({1})", sqlFilter, listFilter);

                ApplySqlFilter(resultFilter);
            }
        }

        public void ApplyFilter(string filterExpression)
        {
            Filters.FilterExpression = filterExpression;
            ApplyFilter();
        }

        public async void ApplySqlFilter(string sqlFilter)
        {
            var startTime = DateTime.Now;
            try
            {
                IsInFiltering = true;
                WaitStart();

                // делаем запрос данных

                var data = await GetFilteredDataAsync(sqlFilter);
                if (Source == null)
                    Source = data == null ? new ObservableRangeCollection<TModel>() : new ObservableRangeCollection<TModel>(data);
                else
                    ((ObservableRangeCollection<TModel>)Source).ReplaceRange(data);

                IsNeedRefresh = false;

                if (_filter == null && _applyFilter == null)
                    return;

                _filter.Hint = _applyFilter.Hint = string.Format(Filters.GetExpression());

                if (string.IsNullOrEmpty(Filters.GetExpression()))
                {
                    if (_isApplyFilter && Menu != null)
                        ChangeImageFilter();
                    return;
                }

                if (AndOrRegex.Matches(Filters.GetExpression().ToUpper()).Count == 0)
                {
                    if (FilterMaxRowCountRegex.Matches(Filters.GetExpression()).Count != 1)
                    {
                        _isApplyFilter = false;
                        ChangeImageFilter();
                    }
                    else
                        if (_isApplyFilter)
                            ChangeImageFilter();
                    return;
                }
                if (_isApplyFilter || Menu == null)
                    return;

                ChangeImageFilter();
            }
            catch (Exception ex)
            {
                if (!ExceptionHandler(ex, ExceptionResources.ItemCantFilter))
                    throw;
            }
            finally
            {
                WaitStop();
                IsInFiltering = false;

                _log.DebugFormat("Receive journal of {0} in {1}", typeof(TModel).Name, DateTime.Now - startTime);
            }
        }

        public void ChangeImageFilter(bool isApplyFilter)
        {
            _isApplyFilter = isApplyFilter;
            ChangeImageFilter();
        }

        private void ChangeImageFilter()
        {
            var bar = Menu.Bars.FirstOrDefault(p => p.Caption == StringResources.Commands);
            if (bar == null)
                return;

            var cmd = bar.MenuItems.FirstOrDefault(i => i.Caption == StringResources.Filter);
            if (cmd == null)
                return;

            var command = cmd as CommandMenuItem;
            if (command == null)
                return;

            if (_isApplyFilter)
            {
                _filter.IsVisible = true;
                _applyFilter.IsVisible = false;
            }
            else
            {
                _filter.IsVisible = false;
                _applyFilter.IsVisible = true;
            }

            _isApplyFilter = !_isApplyFilter;
        }

        protected async virtual Task<IEnumerable<TModel>> GetFilteredDataAsync(string sqlFilter)
        {
            var now = DateTime.Now;
            return await Task.Factory.StartNew(() =>
            {
                //System.Threading.Thread.Sleep(10000);
                using (var manager = GetManager())
                {
                    // перед получением очищаем данные - нам не нужен кэш при явном запросе журнала
                    if (IsNeedClearCache)
                        manager.ClearCache();
                    // получаем данные
                    var result = manager.GetFiltered(sqlFilter, GetModeEnum.Partial);
                    TotalRowItemAdditionalInfo =
                        string.Format(StringResources.ListViewModelBaseTotalRowItemAdditionalInfo,
                            (DateTime.Now - now).TotalSeconds, manager.LastQueryExecutionTime);
                    return result;
                }
            });
        }

        // Замена  FieldName на SourceName для фильтра грида
        public string FilterChangeNameToDB(string filter)
        {
            if (string.IsNullOrEmpty(filter))
                return null;

            var newFilter = filter;
            foreach (var f in Fields.Where(i => filter.Contains(i.FieldName)))
                newFilter = newFilter.Replace(string.Format("[{0}]", f.FieldName), string.Format("[{0}]", f.SourceName));
            return newFilter;
        }

        protected override Type GetSecurityType()
        {
            return typeof(TModel);
        }

        protected override Type GetEntityForProcesses()
        {
            return typeof(TModel);
        }

        /// <summary>
        /// Уникальное имя ViewModel, которое необходимо для однозначного определения viewmodel среди активных
        /// </summary>
        /// <returns>уникальное имя viewmodel</returns>
        public virtual string GetUniqueName()
        {
            return GetType().FullName;
        }

        private string GetFilterExpressionFormat(Type type, bool usesql)
        {
            var leftParenthesis = usesql ? string.Empty : "[";
            var rightParenthesis = usesql ? string.Empty : "]";
            return string.Format("{0}{1}0{2}{3} = {4}{1}1{2}{4}", leftParenthesis, "{", "}", rightParenthesis,
                type == typeof(string) || type == typeof(Guid) ? "'" : string.Empty);
        }

        protected virtual string GetPanelCaption()
        {
            var attributes = TypeDescriptor.GetAttributes(typeof(TModel));
            var att = attributes[typeof(ListViewCaptionAttribute)] as ListViewCaptionAttribute;
            return att == null
                               ? string.Format(StringResources.ObjectListViewModelBasePanelCaptionFormatDefault, typeof(TModel).Name)
                               : att.Caption;
        }

        protected virtual ImageSource GetPanelImage()
        {
            return ImageResources.DCLJournal16.GetBitmapImage();//  ResourceHelper.GetImageForAction("List");
        }

        protected virtual string GetFilterForHistory()
        {
            string res = null;
            string sourceName = null;
            foreach (var item in SelectedItems)
            {
                var kh = item as IKeyHandler;
                if (kh == null)
                    throw new DeveloperException("Историю можно получить только от элементов, для которых определен IKeyHandler");

                // имя св-ва можно получить только один раз
                if (string.IsNullOrEmpty(sourceName))
                    sourceName = SourceNameHelper.Instance.GetPropertySourceName(typeof(TModel), kh.GetPrimaryKeyPropertyName());
                var key = kh.GetKey();

                if (key is Guid)
                {
                    var f2 = string.Format("[{0}] = '{1}'", sourceName, SerializationHelper.GetCorrectStringValue(key));
                    res += string.IsNullOrEmpty(res) ? f2 : " OR " + f2;
                }
                else
                {
                    var format = (key is string ? "[{0}] = '{1}'" : "[{0}] = {1}");
                    var f2 = string.Format(format, sourceName, key);
                    res += string.IsNullOrEmpty(res) ? f2 : " OR " + f2;
                }
            }
            return res;
        }

        protected override void Dispose(bool disposing)
        {
            if (_selectedItems != null)
            {
                _selectedItems.CollectionChanged -= SelectedItemsCollectionChanged;
                _selectedItems = null;
            }
            UnSubscribeManagerEventsMonitoring();
            _managerForChangesMonitoring = null;
            _history = null;
            _filter = null;
            _applyFilter = null;

            base.Dispose(disposing);
        }

        protected virtual bool CanEdit()
        {
            return !IsInFiltering && HasSelectedItems() && IsReadEnable;
        }

        protected virtual void Edit()
        {
            if (IsCloseDoubleClick)
                DispatcherHelper.Invoke(new Action(() => GetViewService().Close(this, true)));
        }

        #endregion

        public void SetSuffix(string suffix)
        {
            _suffix = suffix;
        }

        public string GetSuffix()
        {
            return _suffix ?? string.Empty;
        }

        #region .  ISelectable  .
        public object[] GetSelectedKeys()
        {
            if (SelectedItems == null)
                return null;
            return SelectedItems.Cast<IKeyHandler>().Select(i => i.GetKey()).ToArray();
        }
        #endregion
    }
}

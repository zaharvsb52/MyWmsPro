using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using wmsMLC.Business.General;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.Helpers;
using wmsMLC.DCL.General.ViewModels.Menu;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.DAL;
using wmsMLC.General.PL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Commands;

namespace wmsMLC.DCL.General.ViewModels
{
    public class CustomParamValueTreeViewModel<T> : ObjectTreeViewModelBase<T>, ICustomParamValueTreeViewModel where T : CustomParamValue
    {
        private string _cpvValuePropertyName;
        private List<CustomParam> _cps;
        private bool _isUpdating;
        private int _cpvid;
        private bool _hasAddCpv;
        private string _errorMessage;

        public CustomParamValueTreeViewModel()
        {
            SaveCommand = new DelegateCustomCommand(OnSave, CanSave);
            Commands.AddRange(new[] { SaveCommand });

            CpSource = string.Empty;
            CpTarget = string.Empty;
        }

        #region . Properties .

        public event EventHandler SourceUpdated;
        public event EventHandler NeedChangeFocusRow;
        public string CpEntity { get; set; }
        public string CpKey { get; set; }
        public string CpTarget { get; set; }
        public string CpSource { get; set; }
        public bool ShouldUpdateSeparately { get; set; }
        public string MandantCode { get; set; }

        private IObjectViewModel _customParamValueObjectViewModel;
        public IObjectViewModel CustomParamValueObjectViewModel
        {
            get { return _customParamValueObjectViewModel; }
            set
            {
                if (_customParamValueObjectViewModel == value)
                    return;
                _customParamValueObjectViewModel = value;
                OnPropertyChanged("CustomParamValueObjectViewModel");
            }
        }
        #endregion . Properties .

        #region . Commands .
        public ICommand SaveCommand { get; private set; }

        private bool CanSave()
        {
            return ShouldUpdateSeparately && HasChanges(GetChanges(Source));
        }

        private void OnSave()
        {
            if (!CanSave())
                return;

            OnSaveInternal(true);
            OnNeedChangeFocusRow();
        }

        protected override void OnRefreshData()
        {
            if (!_isUpdating && (_hasAddCpv || HasChanges(GetChanges(Source))) && GetViewService().ShowDialog(StringResources.Confirmation
                    , StringResources.ConfirmationRefreshDate
                    , MessageBoxButton.YesNo
                    , MessageBoxImage.Question
                    , MessageBoxResult.No) != MessageBoxResult.Yes)
            {
                return;
            }

            OnRefreshDataInternal(HasSelectedItems() ? SelectedItems[0] : null);
        }

        protected override bool CanNew()
        {
            Func<bool> handler = () =>
            {
                var selectedItem = SelectedItems[0];
                if (selectedItem == null || selectedItem.Cp == null)
                    return false;

                var parentcp = selectedItem.Cp;
                if (parentcp == null || parentcp.CustomParamCount <= 1 || parentcp.IsReadOnly)
                    return false;

                // ищем количество в пределах родителя
                var items = Source.Where(p => p.CustomParamCode == parentcp.GetKey<string>() && p.CPVParent == selectedItem.CPVParent).ToArray();
                return parentcp.CustomParamCount > items.Length;
            };
            return !_isUpdating && !WaitIndicatorVisible && HasSelectedItems() && handler();
        }

        protected override void New()
        {
            if (!CanNew())
                return;

            try
            {
                WaitStart();

                if (!ConnectionManager.Instance.AllowRequest())
                    return;

                _isUpdating = true;
                var selectedItem = SelectedItems[0];

                var currentCp = selectedItem.Cp;
                if (currentCp == null)
                    return;
                var newItems = new List<CustomParamValue>();
                var newCpv = CreateItem(currentCp, selectedItem.CPVParent);
                newItems.Add(newCpv);
                CollectItems(newCpv, _cps.ToArray(), newItems);
                foreach (var item in newItems)
                {
                    item.AcceptChanges();
                    Source.Add((T)item);
                }
                _hasAddCpv = true;
                SelectedItem = newCpv;
            }
            catch (Exception ex)
            {
                if (!ExceptionHandler(ex, ExceptionResources.ItemCantCreate))
                    throw;
            }
            finally
            {
                WaitStop();
                _isUpdating = false;
                if (_hasAddCpv)
                    OnNeedChangeFocusRow();
            }
        }

        protected override bool CanDelete()
        {
            return !_isUpdating && !WaitIndicatorVisible && HasSelectedItems() && Source != null && Source.Any();
        }

        protected override void Delete()
        {
            if (!CanDelete())
                return;

            try
            {
                WaitStart();

                if (!ConnectionManager.Instance.AllowRequest())
                    return;

                var selectedItem = SelectedItems[0];
                var deletes = CpvHelper.GetChildsCpvByParentCpv<T>(Source, selectedItem, true);

                var message = StringResources.ConfirmationDeleteCpv;
                var messageFormat = StringResources.CpvChanges;
                if (deletes.Length > 1)
                {
                    deletes = deletes.OrderByDescending(p => p.CPVParent).ToArray();
                    message = StringResources.ConfirmationDeleteCpvs;
                    messageFormat = StringResources.CpvsChange;
                }
                if (HasChanges(GetChanges(deletes)))
                    message = string.Format(messageFormat, message);
                if (!DeleteConfirmation(message))
                    return;

                if (ShouldUpdateSeparately)
                {
                    var dbdeletes = deletes.Where(p => p.CPVID >= 0).ToArray();
                    if (dbdeletes.Any())
                    {
                        using (var mng = GetManager())
                        {
                            mng.Delete(dbdeletes);
                        }
                    }
                }

                foreach (var d in deletes)
                {
                    Source.Remove(d);
                }

                OnSourceUpdated();
                if(HasSelectedItems())
                    SelectedItem = SelectedItems[0];
            }
            catch (Exception ex)
            {
                if (!ExceptionHandler(ex, ExceptionResources.ItemsCantDelete))
                    throw;
            }
            finally
            {
                WaitStop();
                OnNeedChangeFocusRow();
            }
        }

        #endregion . Commands .

        #region . Methods .

        protected override void ManagerChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //Ничего не делаем
        }

        protected override bool HasSelectedItems()
        {
            var result = base.HasSelectedItems();
            if (!result)
                return false;
            return SelectedItems[0] != null;
        }

        protected override void CreateMainMenu()
        {
            base.CreateMainMenu();

            if (_history != null)
                _history.IsVisible = false;

            if (Menu != null && Menu.Bars != null)
            {
                foreach (var bar in Menu.Bars.Where(p => p != null && p.Caption.EqIgnoreCase(StringResources.Commands)).ToArray())
                {
                    foreach (var menu in bar.MenuItems.Where(p => p.Caption.EqIgnoreCase(StringResources.Filter) || p.Caption.EqIgnoreCase(StringResources.Edit)).ToArray())
                    {
                        bar.MenuItems.Remove(menu);
                    }
                }

                foreach (
                    var bar in
                        Menu.Bars.Where(p => p.Caption.EqIgnoreCase(StringResources.CustomizationBarMenu)).ToArray())
                {
                    bar.MenuItems.Clear();
                    Menu.Bars.Remove(bar);
                }

                var barcommands = Menu.GetOrCreateBarItem(StringResources.Commands, 1);
                barcommands.MenuItems.Add(new SeparatorMenuItem { Priority = 999 });

                barcommands.MenuItems.Add(new CommandMenuItem
                {
                    Caption = StringResources.Save,
                    Command = SaveCommand,
                    HotKey = new KeyGesture(Key.F6),
                    ImageSmall = ImageResources.DCLSave16.GetBitmapImage(),
                    ImageLarge = ImageResources.DCLSave32.GetBitmapImage(),
                    GlyphAlignment = GlyphAlignmentType.Top,
                    DisplayMode = DisplayModeType.Default,
                    Priority = 1000
                });
            }
        }

        public override void CreateProcessMenu()
        {
            //Нет печати и БП
        }

        protected override void CreateContextMenu()
        {
            base.CreateContextMenu();
            if (ContextMenu != null)
            {
                foreach (var menu in ContextMenu.Where(p => p.Caption.EqIgnoreCase(StringResources.Edit) || p.Caption.EqIgnoreCase(StringResources.OpenInNewWindow)).ToArray())
                {
                    ContextMenu.Remove(menu);
                }

                if (Menu != null && Menu.Bars != null)
                {
                    var barCommands = Menu.Bars.FirstOrDefault(p => p.Caption.EqIgnoreCase(StringResources.Commands));
                    if (barCommands != null)
                    {
                        var newCommandMenuItem = barCommands.MenuItems.FirstOrDefault(p => p.Caption.EqIgnoreCase(StringResources.New)) as CommandMenuItem;
                        if (newCommandMenuItem != null)
                        {
                            newCommandMenuItem = newCommandMenuItem.Clone();
                            newCommandMenuItem.GlyphSize = GlyphSizeType.Small;
                            ContextMenu.Insert(0, newCommandMenuItem);
                        }
                    }
                }
            }
        }

        protected override void SelectedItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            base.SelectedItemsCollectionChanged(sender, e);
            //RiseCommandsCanExecuteChanged();
            if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Reset)
                return;

            OnSelectedItemsCollectionChanged();
        }

        private void OnSelectedItemsCollectionChanged()
        {
            if (_isUpdating)
                return;

            T selectedItem = null;
            if (HasSelectedItems())
                selectedItem = SelectedItems[0];

            if (selectedItem == null)
                selectedItem = GetManager().New();

            if (CustomParamValueObjectViewModel == null)
            {
                var modelType = typeof(CustomParamValueObjectViewModel<>).MakeGenericType(typeof(T));
                var model = (IObjectViewModel) IoC.Instance.Resolve(modelType, null);
                model.Mode = ObjectViewModelMode.MemoryObject;
                model.SetSource(selectedItem);

                var vm = this as IModelHandler;
                if (vm != null)
                    model.ParentViewModelSource = vm.ParentViewModelSource;
                CustomParamValueObjectViewModel = model;
                CustomParamValueObjectViewModel.NeedRefresh += OnNeedRefreshCpv;
            }
            else
            {
                if (!ReferenceEquals(CustomParamValueObjectViewModel.GetSource(), selectedItem))
                {
                    //очищаем существующие биндинги. нас очень беспокоят оставшиеся конверторы у биндингов
                    CustomParamValueObjectViewModel.SetSource(GetManager().New());
                    CustomParamValueObjectViewModel.SetSource(selectedItem);
                }
            }
        }

        private void OnNeedRefreshCpv(object sender, EventArgs eventArgs)
        {
            RiseCommandsCanExecuteChanged();
        }

        protected override ObservableCollection<DataField> GetFields(Type type, SettingDisplay settings)
        {
            var result = base.GetFields(type, settings);
            var field = result.SingleOrDefault(p => p.FieldName == GetCpvValuePropertyName());
            if (field != null)
            {
                var formattedfield = (DataField)field.Clone();
                formattedfield.BindingPath =
                    formattedfield.FieldName =
                        formattedfield.SourceName =
                            formattedfield.Name = CustomParamValue.FormattedValuePropertyName;
                formattedfield.IsEnabled = false;
                result.Add(formattedfield);

                var isRredOnlyfield = (DataField)field.Clone();
                isRredOnlyfield.Caption =
                    isRredOnlyfield.BindingPath =
                        isRredOnlyfield.FieldName =
                            isRredOnlyfield.SourceName =
                                isRredOnlyfield.Name = CustomParamValue.IsReadOnlyPropertyName;
                isRredOnlyfield.IsEnabled = false;
                result.Add(isRredOnlyfield);

                field.Caption = "Value";
            }

            return result;
        }

        protected async override Task<IEnumerable<T>> GetFilteredDataAsync(string sqlFilter)
        {
            try
            {
                _isUpdating = true;
                var now = DateTime.Now;
                double lastQueryExecutionTime = 0;
                var result = await Task.Factory.StartNew(() => GetEditModelSource(out lastQueryExecutionTime));
                TotalRowItemAdditionalInfo = string.Format(StringResources.ListViewModelBaseTotalRowItemAdditionalInfo,
                    (DateTime.Now - now).TotalSeconds, lastQueryExecutionTime);
                return result;
            }
            finally
            {
                _isUpdating = false;
            }
        }

        private CustomParam[] ValidateParentCustomParam(ICollection<CustomParam> cps)
        {
            if (cps == null)
                throw new ArgumentNullException("cps");

            var result = new List<CustomParam>();
            foreach (var cp in cps.Where(cp => !string.IsNullOrEmpty(cp.CustomParamParent)))
            {
                if (GetParentCustomParam(cps, cp) == null)
                    result.Add(cp);
            }

            return result.ToArray();
        }

        private CustomParam GetParentCustomParam(IEnumerable<CustomParam> cps, CustomParam cp)
        {
            if (cps == null)
                throw new ArgumentNullException("cps");

            if (cp == null)
                throw new ArgumentNullException("cp");

            if (string.IsNullOrEmpty(cp.CustomParamParent))
                return cp;
            return
                cps.SingleOrDefault(
                    p => p.GetKey<string>() == cp.CustomParamParent);
        }

        private CustomParam GetRootCustomParam(IEnumerable<CustomParam> cps, CustomParam cp)
        {
            if (cps == null)
                throw new ArgumentNullException("cps");

            if (cp == null)
                throw new ArgumentNullException("cp");

            if (string.IsNullOrEmpty(cp.CustomParamParent))
                return cp;

            return cps.Where(p => p.GetKey<string>() == cp.CustomParamParent).Select(p => GetRootCustomParam(cps, p)).FirstOrDefault();
        }

        private List<T> GetEditModelSource(out double lastQueryExecutionTime)
        {
            lastQueryExecutionTime = 0;
            if (_cps != null)
                _cps.Clear();
            _cps = null;

            if (string.IsNullOrEmpty(CpEntity))
                throw new DeveloperException("Property CpEntity is undefined.");
            if (string.IsNullOrEmpty(CpKey))
                throw new DeveloperException("Property CpKey is undefined.");

            //Получаем список cpv
            var mto = IoC.Instance.Resolve<IManagerForObject>();
            var modeltype = typeof(T);
            var mgrType = mto.GetManagerByTypeName(modeltype.Name);
            if (mgrType == null)
                throw new DeveloperException(string.Format("Unknown source type '{0}'.", modeltype.Name));

            List<CustomParamValue> items;
            using (var mgr = IoC.Instance.Resolve(mgrType, null) as IBaseManager)
            {
                if (mgr == null)
                    throw new DeveloperException(string.Format("Can't resolve IBaseManager by '{0}'.", mgrType.Name));
                var cpvtype = typeof(CustomParamValue);
                items = mgr.GetFiltered(string.Format("{0} = '{1}' AND {2} = '{3}'",
                    SourceNameHelper.Instance.GetPropertySourceName(cpvtype, CustomParamValue.CPV2EntityPropertyName),
                    CpEntity,
                    SourceNameHelper.Instance.GetPropertySourceName(cpvtype, CustomParamValue.CPVKeyPropertyName),
                    CpKey), GetModeEnum.Partial)
                        .OfType<CustomParamValue>().ToList();
                lastQueryExecutionTime += mgr.LastQueryExecutionTime;
            }

            using (var mgr = IoC.Instance.Resolve<IBaseManager<CustomParam>>())
            {
                //Получаем список параметров для данного item.CustomParamCode с учетом манданта
                _cps = ((ICustomParamManager)mgr).GetCPByInstance(CpEntity, CpKey, null, CpSource, CpTarget).ToList();
                lastQueryExecutionTime += mgr.LastQueryExecutionTime;
            }

            //проверяем наличие родительских параметров в CP
            var badcps = ValidateParentCustomParam(_cps);
            if (badcps.Length > 0)
            {
                CustomParam[] cpall;
                using (var mgr = IoC.Instance.Resolve<IBaseManager<CustomParam>>())
                {
                    //Получаем список параметров для данного item.CustomParamCode без учета манданта
                    cpall =
                        mgr.GetFiltered(string.Format("{0} = '{1}'",
                            SourceNameHelper.Instance.GetPropertySourceName(typeof(CustomParam), CustomParam.CustomParam2EntityPropertyName), CpEntity), GetModeEnum.Partial)
                            .ToArray();
                    lastQueryExecutionTime += mgr.LastQueryExecutionTime;
                }
                foreach (var bcp in badcps)
                {
                    var parent = GetParentCustomParam(cpall, bcp);
                    if (parent == null)
                        throw new DeveloperException("Can't find parent for '{0}'.", bcp);

                    if (_cps.Any(p => p.GetKey<string>() == parent.GetKey<string>()))
                        continue;

                    parent.IsReadOnly = true;
                    _cps.Add(parent);
                }
            }

            if (_cps == null || !_cps.Any())
            {
                _errorMessage = string.IsNullOrEmpty(MandantCode)
                    ? "Проверьте привязку параметров к манданту."
                    : string.Format("Проверьте привязку параметров к манданту '{0}'.", MandantCode);
            }

            foreach (var item in items)
            {
                item.Cp = _cps.FirstOrDefault(i => i.GetKey<string>() == item.CustomParamCode);
                item.FormattedValue = CpvHelper.GetFormattedValue(item);
            }

            // получим корневые элементы cp - они обязаны быть
            var rootElements = _cps.Where(i => string.IsNullOrEmpty(i.CustomParamParent)).ToArray();
            if (rootElements.Length == 0 && string.IsNullOrEmpty(_errorMessage))
                throw new DeveloperException("Отсутствуют корневые элементы параметров!");

            // добавим корневые cpv, которые отсутствуют
            foreach (var root in rootElements)
            {
                var exist = items.FirstOrDefault(i => i.CustomParamCode.EqIgnoreCase(root.GetKey<string>()));
                if (exist == null)
                {
                    items.Add(CreateItem(root, null));
                }
            }

            var rootCpvs = items.Where(i => i.CPVParent == null).ToArray();
            // проходим по всем корневым cpv и добираем отсутствущие элементы
            foreach (var cpv in rootCpvs)
            {
                CollectItems(cpv, _cps.ToArray(), items);
            }

            //cpv, которые не должны были быть показаны, но показываем
            var badcpvs = items.Where(cpv => cpv.Cp == null).ToArray();
            if (badcpvs.Any())
            {
                using (var mgr = IoC.Instance.Resolve<IBaseManager<CustomParam>>())
                {
                    foreach (var cpv in badcpvs)
                    {
                        var cp = mgr.Get(cpv.CustomParamCode);
                        lastQueryExecutionTime += mgr.LastQueryExecutionTime;
                        if (cp == null)
                        {
                            items.Remove(cpv);
                        }
                        else
                        {
                            cp.IsReadOnly = true;
                            cpv.Cp = cp;
                            if (string.IsNullOrEmpty(cpv.CPVValue))
                                cpv.CPVValue = cp.CustomParamDefault;
                            cpv.FormattedValue = CpvHelper.GetFormattedValue(cpv);
                        }
                    }
                }
            }

            var result = new List<T>();
            items.ForEach(p =>
            {
                p.AcceptChanges();
                result.Add((T)p);
            });
            return result;
        }

        private CustomParamValue CreateItem(CustomParam cp, decimal? parentCpvId)
        {
            var cpv = (CustomParamValue)Activator.CreateInstance(typeof(T));
            cpv.CPVID = --_cpvid;
            cpv.CustomParamCode = cp.GetKey<string>();
            cpv.CPV2Entity = CpEntity;
            cpv.CPVKey = CpKey;
            cpv.CPVValue = cp.CustomParamDefault;
            cpv.VCustomParamParent = cp.CustomParamParent;
            cpv.Cp = cp;
            cpv.FormattedValue = CpvHelper.GetFormattedValue(cpv);
            cpv.CPVParent = parentCpvId;
            cpv.VCustomParamCount = cp.CustomParamCount;
            cpv.VCustomParamDesc = cp.CustomParamDesc;
            return cpv;
        }

        private void CollectItems(CustomParamValue cpv, CustomParam[] cps, List<CustomParamValue> items)
        {
            var childs = cps.Where(i => i.CustomParamParent.EqIgnoreCase(cpv.CustomParamCode)).ToArray();
            if (childs.Length > 0)
            {
                foreach (var cp in childs)
                {
                    // есть ли уже этот элемент в коллекции
                    var existChild =
                        items.FirstOrDefault(
                            i => i.CPVParent == cpv.CPVID && i.CustomParamCode == cp.GetKey<string>());
                    if (existChild == null)
                    {
                        existChild = CreateItem(cp, cpv.CPVID);
                        items.Add(existChild);
                    }
                    existChild.Cp = cp;
                    // пусть соберет своих деток
                    CollectItems(existChild, cps, items);
                }
            }
        }

        private bool Validation(ICollection<T> cpvs)
        {
            if (cpvs == null || cpvs.Count == 0)
                return false;

            var mustsetcpvs = cpvs.Where(cpv => cpv.Cp.CustomParamMustSet).ToArray();
            // INFO: мы уже собрали все необходимые CustomParamMustSet, проверяем на наличие значения
            //INFO: не собрали, проход по верхнему уровню cpv, без учета mustset.
            var badcpv = mustsetcpvs.Where(cpv => !cpv.IsReadOnly && !cpv.Cp.CustomparamInputdisable && string.IsNullOrEmpty(cpv.CPVValue)).ToArray();
            if (badcpv.Any())
            {
                var message = string.Format("Не заполнено значение у параметров:{0}{1}", 
                    Environment.NewLine, 
                    string.Join(Environment.NewLine, badcpv.Select(p => string.Format("'{0}' ('{1}', ид. '{2}')", p.Cp.CustomParamName, p.CustomParamCode, p.GetKey()))));
                var vs = GetViewService();
                vs.ShowDialog(StringResources.Error
                    , message
                    , MessageBoxButton.OK
                    , MessageBoxImage.Error
                    , MessageBoxResult.Yes);
                return false;
            }

            return true;
        }

        internal class SpecialComparer : IComparer<decimal?>
        {
            public int Compare(decimal? x, decimal? y)
            {
                if (x == null && y == null)
                    return 0;
                if (x == null)
                    return 1;
                if (y == null)
                    return -1;
                if (x.Value == y.Value)
                    return 0;
                return x.Value > y.Value ? 1 : -1;
            }
        }

        private bool OnSaveInternal(bool canSave)
        {
            if (!canSave)
                return true;

            if (!ConnectionManager.Instance.AllowRequest())
                return false;

            try
            {
                _isUpdating = true;
                WaitStart();

                var changes = GetChanges(Source, true);

                var relatedChanges = CollectRelatedParams(Source, changes);

                if (!Validation(relatedChanges))
                    return false;

                var selectedItem = SelectedItems[0];

                if (ShouldUpdateSeparately)
                {
                    var uowFactory = IoC.Instance.Resolve<IUnitOfWorkFactory>();
                    using (var uow = uowFactory.Create(false))
                    {
                        try
                        {
                            uow.BeginChanges();
                            var mng = GetManager();
                            mng.SetUnitOfWork(uow);

                            var sources = Source.OrderByDescending(cpv => cpv.CPVParent, new SpecialComparer());
                            foreach (var cpv in sources)
                            {
                                var key = cpv.GetKey<decimal>();
                                if (key < 0)
                                {
                                    var item = cpv;

                                    if (!string.IsNullOrEmpty(cpv.CPVValue) || //Если были проставлены значения по умолчанию - сохраняем
                                        cpv.Cp.CUSTOMPARAMSAVEMODE || HasChanges(GetChanges(CpvHelper.GetChildsCpvByParentCpv<T>(sources, cpv, false), true)))
                                    {
                                        mng.Insert(ref item);

                                        item.Cp = cpv.Cp;
                                        item.FormattedValue = CpvHelper.GetFormattedValue(item);
                                        SourceUpdate(item, cpv);
                                        var childs = sources.Where(i => i.CPVParent == key);
                                        foreach (var child in childs)
                                            child.CPVParent = item.CPVID;
                                    }
                                    else
                                    {
                                        cpv.AcceptChanges();
                                    }
                                }
                                else
                                {
                                    //Удаляем параметры, у которых не установлен CUSTOMPARAMSAVEMODE и CPVValue is null и нет деток
                                    if (string.IsNullOrEmpty(cpv.CPVValue) && !cpv.Cp.CUSTOMPARAMSAVEMODE &&
                                        CpvHelper.GetChildsCpvByParentCpv<T>(sources, cpv, false).Length == 0)
                                    {
                                        mng.Delete(cpv);
                                        cpv.AcceptChanges();
                                    }
                                    else if (cpv.IsDirty)
                                    {
                                        mng.Update(cpv);
                                    }
                                }
                            }

                            uow.CommitChanges();
                        }
                        catch
                        {
                            uow.RollbackChanges();
                            throw;
                        }
                    }
                }

                _hasAddCpv = false;
                if (selectedItem != null)
                    SelectedItem = selectedItem;
                OnSourceUpdated();
            }
            catch (Exception ex)
            {
                if (!ExceptionHandler(ex, ExceptionResources.ItemCantSave))
                    throw;
                return false;
            }
            finally
            {
                _isUpdating = false;
                WaitStop();
            }

            return true;
        }

        private void SourceUpdate(T newcpv, T oldcpv)
        {
            var index = Source.IndexOf(oldcpv);
            Source.Remove(oldcpv);
            if (index >= 0)
                Source.Insert(index, newcpv);
            else
                Source.Add(newcpv);
        }

        protected override bool CanCloseInternal()
        {
            if (!ShouldUpdateSeparately || !HasChanges(GetChanges(Source)))
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
                        return OnSaveInternal(CanSave());
                    case MessageBoxResult.No:
                        return true;
                    default:
                        throw new DeveloperException(DeveloperExceptionResources.UnknownDialogResult);
                }
            }
            finally
            {
                WaitStop();
            }
        }

        private string GetCpvValuePropertyName()
        {
            if (string.IsNullOrEmpty(_cpvValuePropertyName))
                _cpvValuePropertyName = CpvHelper.GetPropertyName(typeof(T), CustomParamValue.CPVValuePropertyName);
            return _cpvValuePropertyName;
        }

        private T[] GetChanges(IEnumerable<T> source, bool savemode = false)
        {
            if (source == null)
                return new T[0];
            if (savemode)
                return source.Where(p => !string.IsNullOrEmpty(p.CPVValue) || //Если были проставлены значения по умолчанию - сохраняем
                    p.IsNew || p.IsDirty || p.Cp.CUSTOMPARAMSAVEMODE).ToArray();
            return source.Where(p => p.IsNew || p.IsDirty).ToArray();
        }

        private T[] CollectRelatedParams(IEnumerable<T> source, IEnumerable<T> dirties)
        {
            var result = new List<T>();
            if (source == null)
                return dirties == null? new T[0] : dirties.ToArray();
            if (dirties == null)
                return new T[0];
            var dirtiesArray = dirties as T[] ?? dirties.ToArray();
            result.AddRange(dirtiesArray);
            var sourceArray = source as T[] ?? source.ToArray();
            foreach (var dirty in dirtiesArray)
            {
                // соберем на нижнем уровне
                var lowerLevelItems = sourceArray.Where(i => i.CPVParent == dirty.CPVID && i.Cp.CustomParamMustSet).ToArray();
                if (lowerLevelItems.Any())
                    result.AddRange(lowerLevelItems);
                // соберем на одном уровне
                var sameLevelItems = sourceArray.Where(i => i.CPVID != dirty.CPVID && i.CPVParent == dirty.CPVParent && i.Cp.CustomParamMustSet).ToArray();
                if (sameLevelItems.Any())
                    result.AddRange(sameLevelItems);
                // соберем на верхнем уровне
                var upperLevelItems = sourceArray.Where(i => i.CPVID == dirty.CPVParent).ToArray();
                if (upperLevelItems.Any())
                {
                    // идем вверх по дереву
                    var others = CollectRelatedParams(sourceArray, upperLevelItems);
                    if (others.Any())
                    {
                        foreach (var other in others)
                            if(result.FirstOrDefault(i => i.CPVID == other.CPVID) == null)
                                result.Add(other);
                    }
                }
            }
            return result.Distinct(new CpvComparer<T>()).ToArray();
        }

        private void OnRefreshDataInternal(object selectedItem)
        {
            _hasAddCpv = false;
            _cpvid = 0;
            SelectedItem = selectedItem;
            Source = null;
            base.OnRefreshData();
        }

        private bool DeleteConfirmation(string message)
        {
            var vs = GetViewService();
            var dr = vs.ShowDialog(StringResources.Confirmation
                , message
                , MessageBoxButton.YesNo
                , MessageBoxImage.Question
                , MessageBoxResult.Yes);

            return dr == MessageBoxResult.Yes;
        }

        private bool HasChanges(IEnumerable<CustomParamValue> changes)
        {
            return changes != null && changes.Any();
        }

        private void OnSourceUpdated()
        {
            var handler = SourceUpdated;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void OnNeedChangeFocusRow()
        {
            var handler = NeedChangeFocusRow;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        void ICustomParamValueTreeViewModel.ShowErrorMessage()
        {
            if (!string.IsNullOrEmpty(_errorMessage))
            {
                GetViewService().ShowDialog(StringResources.Information, _errorMessage, MessageBoxButton.OK,
                    MessageBoxImage.Warning, MessageBoxResult.OK);
                _errorMessage = null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing && CustomParamValueObjectViewModel != null)
            {
                CustomParamValueObjectViewModel.NeedRefresh -= OnNeedRefreshCpv;
                CustomParamValueObjectViewModel = null;
            }
        }
        #endregion . Methods .

        private class CpvComparer<TCpv> : IEqualityComparer<TCpv> where TCpv : CustomParamValue
        {
            public bool Equals(TCpv x, TCpv y)
            {
                 //Check whether the compared objects reference the same data.
                if (ReferenceEquals(x, y)) 
                    return true;

                //Check whether any of the compared objects is null.
                if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                    return false;

                return Equals(x.GetKey(), y.GetKey());
            }

            public int GetHashCode(TCpv obj)
            {
                return obj == null ? 0 : obj.GetKey().GetHashCode();
            }
        }
    }

    public interface ICustomParamValueTreeViewModel : IObjectListViewModel, IObjectTreeViewModel
    {
        string CpEntity { get; set; }

        string CpKey { get; set; }

        bool ShouldUpdateSeparately { get; set; }

        string MandantCode { get; set; }

        event EventHandler SourceUpdated;
        event EventHandler NeedChangeFocusRow;

        void ShowErrorMessage();
    }
}

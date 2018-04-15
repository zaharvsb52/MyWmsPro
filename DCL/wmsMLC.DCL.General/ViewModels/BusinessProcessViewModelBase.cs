using System;
using System.Linq.Dynamic;
using System.Collections.Generic;
using System.ComponentModel;
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
using wmsMLC.DCL.General.ViewModels.Menu;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Helpers;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.ViewModels;

namespace wmsMLC.DCL.General.ViewModels
{
    /// <summary>
    /// Класс, аггрегирующий логику работы с бизнес-процессами.
    /// </summary>
    public abstract class BusinessProcessViewModelBase<TModel, T> : EditViewModelBase<TModel>
    {
        #region .  Fields&Consts  .

        private const bool EmptyTriggerExpressionAllowExecuteAnyItem = true;

        private BarItem _processBar;
        private ListMenuItem _bpMenuListItem;
        private Report[] _reports;
        protected CommandMenuItem Print;
        private ILog _log = LogManager.GetLogger(typeof (ViewModelBase));
        private DateTime _bpStartTime;
        private string _bpCode;
        private bool _isbusy;

        #endregion

        protected BusinessProcessViewModelBase()
        {
            PrintCommand = new DelegateCustomCommand(PrintReport, CanPrintReport);
            ExecuteBPCommand =
                new DelegateCustomCommand<BusinessProcessCommandParameter>(OnBPExecute);

            //CreateProcessMenu();
        }

        #region .  Properties  .

        public ICommand ExecuteBPCommand { get; private set; }
        public ICommand LoadBPMenuItemsCommand { get; private set; }
        public ICommand PrintCommand { get; set; }

        /// <summary>
        /// Флаг, запрещающий использование процессов
        /// </summary>
        protected bool DenyBusinessProcessTrigger { get; set; }

        public Report[] Reports
        {
            get { return _reports; }
            set
            {
                if (_reports == value)
                    return;

                _reports = value;
            }
        }

        #endregion

        #region .  Methods  .

        protected override void InitializeSettings()
        {
            base.InitializeSettings();

            DenyBusinessProcessTrigger = false;
        }

        protected virtual T[] GetPrintedItems()
        {
            return null;
        }

        protected virtual bool CanPrintReport()
        {
            return false;
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

                var items = GetPrintedItems();
                if (items == null)
                    return;

                var ovm = new PrintViewModel(items.Cast<object>().ToArray());

                GetViewService().ShowDialogWindow(ovm, true, true);
            }
            catch (Exception ex)
            {
                if (!ExceptionHandler(ex, ExceptionResources.EpsCreateTaskError))
                    throw;
            }
        }

        private async void OnBPExecute(BusinessProcessCommandParameter parameter)
        {
            if (!ConnectionManager.Instance.AllowRequest())
                return;

            var items = GetItemsParameter().ToArray();

            try
            {
                WaitStart();
                _isbusy = true;
                // синхронно проверяем возможность запуска данного процесса по триггеру
                var res = await CanUseTriggerForItems((BPTrigger) parameter.Trigger, items);

                if (!res)
                {
                    var vs = GetViewService();
                    vs.ShowDialog("Внимание", "Невозможно запустить процесс. Не выполняется условие запуска.",
                        MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                    WaitStopInternal();
                    return;
                }

                var managerInstance = IoC.Instance.Resolve<IBPProcessManager>();
                await Task.Factory.StartNew(() =>
                {
                    // TODO: собираем параметры для процесса
                    // HACK: сейчас параметры заданы статически

                    //managerInstance.Parameters.Add("Items", items);
                    // настраиваем контекст
                    // TODO: необходимо стандартизировать и вынести параметры в одно место
                    var context = new BpContext {Items = items, Name = parameter.Name};
                    context.Set("DialogVisible", true); // признак того, что надо показывать диалоги
                    managerInstance.Parameters.Add(BpContext.BpContextArgumentName, context);

                    _bpStartTime = DateTime.Now;
                    _bpCode = parameter.BusinessProcessCode.ToString();
                    try
                    {
                        _log.DebugFormat("Start process: {0}", _bpCode);
                        managerInstance.Run(code: _bpCode, completedHandler: OnBpProcessEnd);
                    }
                    finally
                    {
                        _log.DebugFormat("Stop process: {0} in {1}", _bpCode, DateTime.Now - _bpStartTime);
                    }
                });
            }
            catch (Exception)
            {
                WaitStopInternal();
                throw;
            }
        }

        private void WaitStopInternal()
        {
            if (_isbusy)
            {
                _isbusy = false;
                WaitStop();
            }
        }

        protected virtual void OnBpProcessEnd(CompleteContext context)
        {
            WaitStopInternal();
            _log.DebugFormat("End process: {0} in {1}", _bpCode, DateTime.Now - _bpStartTime);
        }

        public virtual void CreateProcessMenu()
        {
            if (!IsMenuEnable)
                return;

            _processBar = Menu.GetOrCreateBarItem(StringResources.BusinessProcesses, 20, "BarItemBusinessProcesses");
            try
            {
                CreateBPMenu();
                CreatePrintMenu();
            }
            catch (Exception ex)
            {
                if (!ExceptionHandler(ex, "Ошибка при создании меню"))
                    throw;
            }
        }

        private void CreateBPMenu()
        {
            try
            {
                _bpMenuListItem = new ListMenuItem
                {
                    IsDynamicBarItem = true,
                    Name = "ListMenuItemBp",
                    Caption = StringResources.Wait,
                    ImageSmall = ImageResources.DCLBusinessProcess16.GetBitmapImage(),
                    ImageLarge = ImageResources.DCLBusinessProcess32.GetBitmapImage(),
                    HotKey = new KeyGesture(Key.F12),
                    GlyphAlignment = GlyphAlignmentType.Top,
                    DisplayMode = DisplayModeType.Default,
                    Priority = 100,
                    IsEnable = false
                };
                var items = GetBPMenuItems();
                if (items == null)
                    return;
                _bpMenuListItem.PropertyChanged += BPMenuListItem_PropertyChanged;
                _bpMenuListItem.MenuItems.AddRange(items);
                _processBar.MenuItems.Add(_bpMenuListItem);
                OnPropertyChanged(GetType().ExtractPropertyName(() => Menu));
            }
            catch (Exception)
            {
                _bpMenuListItem.Caption = StringResources.ErrorButton;
                throw;
            }
            finally
            {
                if (_bpMenuListItem.Caption != StringResources.ErrorButton)
                    _bpMenuListItem.Caption = StringResources.BusinessProcess;
                _bpMenuListItem.IsEnable = _bpMenuListItem.MenuItems.Count > 0;
            }
        }

        private async void BPMenuListItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != ListMenuItem.IsEnableItemsPropertyName)
                return;

            if (!_bpMenuListItem.IsEnableItems)
            {
                foreach (var mi in _bpMenuListItem.MenuItems.OfType<CommandMenuItem>())
                    mi.IsEnable = false;
            }
            else
            {
                var processingItems = GetItemsParameter();
                //INFO: мы проверяем количество записей в CanUseTriggerForItems
                //if (processingItems == null || processingItems.Length == 0)
                //    return;

                foreach (var mi in _bpMenuListItem.MenuItems.OfType<CommandMenuItem>().ToArray())
                {
                    var p = mi.CommandParameter as BusinessProcessCommandParameter;
                    if (p == null)
                        continue;

                    mi.IsEnable = await CanUseTriggerForItems((BPTrigger) p.Trigger, processingItems);
                }
            }
        }

        protected void CreatePrintMenu()
        {
            var barTech = Menu.GetOrCreateBarItem(StringResources.Printable, 40, "BarItemActionPrint");
            Print = new CommandMenuItem
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
            barTech.MenuItems.Add(Print);
        }

        /// <summary>
        /// Метод определяет возможно ли запустить данный триггер для выделенных элементов. Если хотя бы для одного такой возможности нет, то меню блокируется для всех
        /// </summary>
        /// <param name="trigger">запускаемый триггер</param>
        /// <param name="items">проверочная коллекция элементов</param>
        /// <returns>истина - в случае возможности запуска</returns>
        private async Task<bool> CanUseTriggerForItems(BPTrigger trigger, object[] items)
        {
            if (WMSEnvironment.Instance.IsConnected == false)
                return false;

            //INFO: признак того, что можно запускать без выбранной или новой записи
            //TODO: сделанть enum на действие
            if (trigger.TriggerAction.EqIgnoreCase("FORCE"))
                return true;

            if (items == null || items.Length == 0)
                return false;

            // если новый объект, то запрещаем
            var source = Source as IIsNew;
            if (source != null && source.IsNew)
                return false;

            // если коолекция и стоит признак OnlyOne
            if (trigger.TriggerOnlyByOneItem && items.Length > 1)
                return false;

            // если есть фильтр по сущности
            if (!string.IsNullOrEmpty(trigger.TriggerEntityFilter))
            {
                try
                {
                    var res = items.Cast<T>().Where(trigger.TriggerEntityFilter);
                    if (items.Count() != res.Count())
                        return false;
                }
                catch (Exception ex)
                {
                    var message = string.Format(
                        "Не удалось применить клиентскую проверку триггера процесса. Процесс '{0}'. Выражение '{1}'.",
                        trigger.ProcessCode, trigger.TriggerEntityFilter);
                    _log.Debug(message, ex);
                    return false;
                }
            }

            // выражения нет - фильтровать ничего не нужно
            if (string.IsNullOrEmpty(trigger.TriggerExcpression))
                return EmptyTriggerExpressionAllowExecuteAnyItem;

            // формируем фильтр
            var filter = GetBPCheckFilter(trigger.TriggerExcpression, items);

            // получаем Manager-a
            var objectType = items[0].GetType();
            var interfaceType = typeof (IBaseManager<>).MakeGenericType(objectType);

            return await CheckTriggerAsync(items, interfaceType, objectType, filter);
        }

        private static async Task<bool> CheckTriggerAsync(object[] items, Type interfaceType, Type objectType,
            string filter)
        {
            return await Task<bool>.Factory.StartNew(() =>
            {
                using (var managerInstance = IoC.Instance.Resolve(interfaceType) as IBaseManager)
                {
                    if (managerInstance == null)
                        throw new DeveloperException("Не найден Manager для сущности '{0}'.", objectType.Name);
                    var checkedItems = managerInstance.GetFiltered(filter, GetModeEnum.Partial).ToArray();

                    //TODO: возможно нужно как-то сильнее проверять
                    return checkedItems.Length == items.Length;
                }
            });
        }


        /// <summary>
        /// Формирует фильтр для проверки возможности запуска данного процесса для выбранных элементов
        /// </summary>
        /// <param name="triggerExpression"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        private string GetBPCheckFilter(string triggerExpression, object[] items)
        {
            var filters = new List<string>();

            //берем фильтр триггера
            var filter = triggerExpression;
            if (!string.IsNullOrEmpty(filter))
                filters.Add(string.Format("({0})", filter));

            // накладываем ограничение по выбранным элементам
            filter = FilterHelper.GetFilterIn(typeof (T), items);
            if (!string.IsNullOrEmpty(filter))
                filters.Add(string.Format("({0})", filter));
            // формируем итоговый
            return string.Join(" AND ", filters.ToArray());
        }

        protected abstract object[] GetItemsParameter();

        /// <summary>
        /// Получаем список элементов меню бизнес-процессов с триггерами, доступными для сущности
        /// </summary>
        protected virtual IEnumerable<MenuItemBase> GetBPMenuItems()
        {
            // вычитываем триггеры
            var entityForBP = GetEntityForProcesses();
            BPTrigger[] triggers;
            using (var triggerManager = IoC.Instance.Resolve<IBPTriggerManager>())
                triggers = triggerManager.GetByEntity(entityForBP, BPTriggerMode.MANUAL).ToArray();

            if (triggers.Length == 0)
                return new MenuItemBase[0];

            var res = new List<MenuItemBase>();
            // вычитывем процессы
            using (var bpManager = IoC.Instance.Resolve<IBaseManager<BPProcess>>())
            {
                foreach (var trigger in triggers)
                {
                    var bp = bpManager.Get(trigger.ProcessCode);
                    if (bp == null)
                        throw new DeveloperException("Для триггера не найден процесс с кодом {0}.",
                            string.IsNullOrEmpty(trigger.ProcessCode) ? "NULL" : trigger.ProcessCode);

                    if (bp.Disable)
                        continue;

                    if (!string.IsNullOrEmpty(trigger.ButtonCode))
                    {
                        try
                        {
                            // пытаемся создать кнопку с указанными координатами, если не получается, то добавляем стнадартным образом
                            CreateBtnMenu(trigger.ButtonCode, trigger, bp.GetKey());
                            continue;
                        }
                        catch (Exception ex)
                        {
                            if (!ExceptionHandler(ex, "Ошибка при создании меню процессов."))
                                throw;
                        }
                    }

                    var miProcess = new CommandMenuItem
                    {
                        IsDynamicBarItem = true,
                        Caption = bp.Name,
                        ImageSmall = ImageResources.DCLBusinessProcess16.GetBitmapImage(),
                        ImageLarge = ImageResources.DCLBusinessProcess32.GetBitmapImage(),
                        GlyphAlignment = GlyphAlignmentType.Top,
                        DisplayMode = DisplayModeType.Default,
                        Command = ExecuteBPCommand,
                        IsEnable = false,
                    };
                    miProcess.CommandParameter = new BusinessProcessCommandParameter(bp.GetKey(), trigger, miProcess);
                    res.Add(miProcess);
                }
                return res;
            }
        }

        private void CreateBtnMenu(string code, BPTrigger trigger, object bpCode, MenuItemBase miChild = null)
        {
            if (string.IsNullOrEmpty(code))
                throw new ArgumentNullException("code");

            // получаем кнопку
            UIButton btn;
            using (var btnMgr = IoC.Instance.Resolve<IBaseManager<UIButton>>())
                btn = btnMgr.Get(code);

            if (btn == null)
                throw new DeveloperException("Can't find button with code " + code);

            // Eсли кнопка будет добавляться в бар и имеет имеет ребенка и родителя, то - ListMenuItem, если только ребенка, то SubListMenuItem, а если в бар не добавляется, то Command
            // Вот такая "прозрачная" логика, будь она не ладна!
            var res = miChild != null
                ? !string.IsNullOrEmpty(btn.Parent)
                    ? new ListMenuItem()
                    : new SubListMenuItem()
                : new CommandMenuItem {Command = ExecuteBPCommand};

            // если можем добавит вложенный элемент (вне зависимости от того, что перед нами), то добавляем
            var miList = res as ListMenuItem;
            if (miList != null)
                miList.MenuItems.Add(miChild);

            //заполняем параметры кнопок
            res.Name = code;
            res.IsDynamicBarItem = true;
            res.IsEnable = true;
            res.Caption = btn.Caption;
            res.Hint = btn.Hint;
            res.GlyphAlignment = GlyphAlignmentType.Top;
            res.DisplayMode = DisplayModeType.Default;
            res.Priority = (int) btn.Order;
            res.CommandParameter = new BusinessProcessCommandParameter(bpCode, trigger, res);

            if (!string.IsNullOrEmpty(btn.Image))
            {
                res.ImageSmall =
                    ResourceHelper.GetImageByName("wmsMLC.DCL.Resources", "ImageResources",
                        string.Format("{0}16", btn.Image)) ?? ImageResources.DCLDefault16.GetBitmapImage();
                res.ImageLarge =
                    ResourceHelper.GetImageByName("wmsMLC.DCL.Resources", "ImageResources",
                        string.Format("{0}32", btn.Image)) ?? ImageResources.DCLDefault32.GetBitmapImage();
            }
            else
            {
                res.ImageSmall = ImageResources.DCLDefault16.GetBitmapImage();
                res.ImageLarge = ImageResources.DCLDefault32.GetBitmapImage();
            }

            if (!string.IsNullOrEmpty(btn.HotKey))
            {
                var kgc = new KeyGestureConverter();
                var hotKey = kgc.ConvertFromString(btn.HotKey) as KeyGesture;
                if (hotKey != null)
                    res.HotKey = hotKey;
            }

            // получаем панель. нет - используем стандартную
            var bar = string.IsNullOrEmpty(btn.Panel) ? _processBar : Menu.GetOrCreateBarItem(btn.Panel);

            if (string.IsNullOrEmpty(btn.Parent))
            {
                // если базовой не нашли, добавлем в список
                bar.MenuItems.Add(res);
            }
            else
            {
                MenuItemBase baseItem = null;
                // ищем такую кнопку
                foreach (var menuItem in bar.MenuItems)
                {
                    baseItem = GetMenuItem(menuItem, btn.Parent);
                    if (baseItem == null)
                        continue;

                    var listItem = baseItem as ListMenuItem;
                    if (listItem == null)
                        throw new DeveloperException(
                            "Ошибка настройки кнопок меню процессов. Невозможно добавить кнопку с кодом {0} в кнопку с кодом {1} - не найден список",
                            code, btn.Parent);

                    listItem.MenuItems.Add(res);
                    break;
                }

                if (baseItem == null)
                    CreateBtnMenu(btn.Parent, null, null, res);
            }
        }

        private MenuItemBase GetMenuItem(MenuItemBase item, string name)
        {
            if (item.Name == name)
                return item;

            var listItem = item as ListMenuItem;
            if (listItem == null)
                return null;

            foreach (var mi in listItem.MenuItems)
            {
                var res = GetMenuItem(mi, name);
                if (res != null)
                    return res;
            }

            return null;
        }

        protected virtual Type GetEntityForProcesses()
        {
            return typeof (TModel);
        }

        protected override void Dispose(bool disposing)
        {
            _processBar = null;
            _bpMenuListItem = null;
            _reports = null;
            Print = null;

            base.Dispose(disposing);
        }

        #endregion
    }

    public class BusinessProcessCommandParameter
    {
        public BusinessProcessCommandParameter(object businessProcessCode, object trigger, CommandMenuItem menuItem)
        {
            BusinessProcessCode = businessProcessCode;
            Trigger = trigger;
            MenuItem = menuItem;
            Name = MenuItem.Caption;
        }

        public object BusinessProcessCode { get; private set; }
        public object Trigger { get; private set; }
        public CommandMenuItem MenuItem { get; private set; }
        public string Name { get; private set; }
    }
}
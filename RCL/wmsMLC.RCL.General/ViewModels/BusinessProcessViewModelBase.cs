using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Linq;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.Business.Objects.Processes;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.RCL.General.ViewModels.Menu;

namespace wmsMLC.RCL.General.ViewModels
{
    /// <summary>
    /// Класс, аггрегирующий логику работы с бизнес-процессами
    /// </summary>
    public abstract class BusinessProcessViewModelBase<TModel, T> : EditViewModelBase<TModel>
    {
        //private ListMenuItem _bpMenuListItem;

        protected BusinessProcessViewModelBase()
        {
            DenyBusinessProcessTrigger = false;

            BusinessProcessCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand<BusinessProcessCommandParameter>(OnBPExecute, CanBPExecute);
            LoadBPMenuItemsCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand<BusinessProcessCommandParameter>(OnLoadBPMenuItems, CanLoadBPMenuItems);
            EmptyCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand<BusinessProcessCommandParameter>(OnDoNothing, CanDoNothing);

            CreateMenu();
        }

        #region .  Properties  .
        public ICommand BusinessProcessCommand { get; private set; }
        public ICommand LoadBPMenuItemsCommand { get; private set; }
        public ICommand EmptyCommand { get; private set; }

        /// <summary>
        /// Флаг, запрещающий использование процессов
        /// </summary>
        protected bool DenyBusinessProcessTrigger { get; set; }

        //        public WMSBusinessCollection<BPTrigger> Triggers
        //        {
        //            get { return _triggers; }
        //            set
        //            {
        //                if (_triggers != null)
        //                {
        //                    var clnChanged = _triggers as INotifyCollectionChanged;
        //                    if (clnChanged != null)
        //                        clnChanged.CollectionChanged -= TriggersCollectionChanged;
        //                }
        //
        //                _triggers = value;
        //
        //                if (_triggers != null)
        //                {
        //                    var clnChanged = _triggers as INotifyCollectionChanged;
        //                    if (clnChanged != null)
        //                        clnChanged.CollectionChanged += TriggersCollectionChanged;
        //                }
        //            }
        //        }
        #endregion

        #region .  Methods  .
        private bool CanLoadBPMenuItems(BusinessProcessCommandParameter arg)
        {
            return true;
        }

        private void OnLoadBPMenuItems(BusinessProcessCommandParameter obj)
        {
            
        }

        private bool CanUseTriggerForItems(BPTrigger trigger, object[] items)
        {
            // выражения нет - фильтровать ничего не нужно
            if (string.IsNullOrEmpty(trigger.TriggerExcpression))
                return true;

            // формируем фильтр
            var filters = new List<string>();
            var filter = trigger.TriggerExcpression;
            if (!string.IsNullOrEmpty(filter))
                filters.Add(string.Format("({0})", filter));
            filter = GetFilter(items);
            if (!string.IsNullOrEmpty(filter))
                filters.Add(string.Format("({0})", filter));
            filter = string.Join(" AND ", filters.ToArray());

            //var filter = string.Format("({0}) AND ({1})", trigger.TriggerExcpression, GetFilter(items));
            // получаем Manager-a
            var objectType = items[0].GetType();
            var interfaceType = typeof(IBaseManager<>).MakeGenericType(objectType);
            var managerInstance = IoC.Instance.Resolve(interfaceType) as IBaseManager;
            if (managerInstance == null)
                throw new DeveloperException("Не найден Manager для сущности '{0}'.", objectType.Name);
            var checkedItems = managerInstance.GetFiltered(filter, GetModeEnum.Partial).ToArray();
            return checkedItems.Length == items.Length;
        }
        
        private bool CanDoNothing(BusinessProcessCommandParameter arg)
        {
            return true;
        }

        private void OnDoNothing(BusinessProcessCommandParameter obj)
        {
        }
        
        private bool CanBPExecute(BusinessProcessCommandParameter parameter)
        {
            // получаем элементы, над которыми будем выполнять процессы
            var processingItems = GetItemsParameter();
            if (processingItems == null)
                return false;
            if (processingItems.Length == 0)
                return false;
            return CanUseTriggerForItems((BPTrigger)parameter.Trigger, processingItems);
        }

        private void OnBPExecute(BusinessProcessCommandParameter parameter)
        {
            //var items = CheckTriggerExpression(parameter.TriggerId);
            var items = GetItemsParameter().Cast<T>().ToArray();
            var managerInstance = IoC.Instance.Resolve<IBPProcessManager>();
            // TODO: собираем параметры для процесса
            // HACK: сейчас параметры заданы статически
            managerInstance.Parameters.Add("Items", items);
            //managerInstance.Parameters.Add("Comment", "Автоматическая блокировка");
            WaitStart();
            managerInstance.Run(parameter.BusinessProcessCode.ToString(), ()=> WaitStop());
        }

        /// <summary>
        /// Получение типа сущности для которого будет отбираться процессы
        /// </summary>
        /// <returns></returns>
        protected virtual Type GetEntityForBPCheck()
        {
            return typeof(TModel);
        }

        /// <summary>
        /// Создаем кнопку меню для отображения процессов. Классы, для которых процессы не нужны - могут перекрыть этот метод и ничего не делать
        /// </summary>
        protected async virtual void CreateMenu()
        {
        }
        
        /// <summary>
        /// Заполняем меню бизнес-процессов триггерами, доступными для сущности
        /// </summary>
        protected virtual ObservableCollection<MenuItemBase> GetBPMenuItems()
        {
            return new ObservableCollection<MenuItemBase>();
        }

        private string GetFilter(IEnumerable<object> items)
        {
            var filterSql = string.Empty;
            string sourceName = null;
            foreach (var item in items)
            {
                var kh = item as IKeyHandler;
                if (kh == null)
                    throw new DeveloperException("Фильтр можно получить только от элементов, для которых определен IKeyHandler");

                // имя св-ва можно получать только один раз
                if (string.IsNullOrEmpty(sourceName))
                    sourceName = SourceNameHelper.Instance.GetPropertySourceName(typeof(TModel), kh.GetPrimaryKeyPropertyName());

                var key = kh.GetKey();
                if (key == null) //на нет и суда нет
                    continue;
                var formatSql = (key is string ? "({0} = '{1}')" : "({0} = {1})");
                var f1 = string.Format(formatSql, sourceName, key);
                filterSql += (string.IsNullOrEmpty(filterSql) ? f1 : " OR " + f1);
            }
            return filterSql;
        }

        protected abstract object[] GetItemsParameter();
        #endregion
    }

    public class BusinessProcessCommandParameter
    {
        public BusinessProcessCommandParameter(object businessProcessCode, object triggerId, object trigger)
        {
            BusinessProcessCode = businessProcessCode;
            TriggerId = triggerId;
            Trigger = trigger;
        }

        public object BusinessProcessCode { get; private set; }
        public object TriggerId { get; private set; }
        public object Trigger { get; private set; }
    }
}

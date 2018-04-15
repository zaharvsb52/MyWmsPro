using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels.Menu;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL;
using wmsMLC.General.PL.Model;

namespace wmsMLC.DCL.General.ViewModels
{
    /// <summary>
    /// ViewModel для отображения истории сущности.
    /// </summary>
    /// <typeparam name="TModel">Тип сущности, по которой отображается история</typeparam>
    public class HistoryListViewModelBase<TModel> : ListViewModelBase<HistoryWrapper<TModel>>
    {
        #region .  Fields  .
        // ReSharper disable once StaticFieldInGenericType
        private static readonly Lazy<ObservableCollection<DataField>> _historyFieldsCache = new Lazy<ObservableCollection<DataField>>(CreateHistoryFields);
        #endregion

        public HistoryListViewModelBase()
        {
            PanelCaption = string.Format("{0}: {1}", StringResources.History, PanelCaption);

            // прячем меню истории
            if (_history != null)
                _history.IsVisible = false;

            IsHistoryEnable = false;
        }

        protected override void CreateMainMenu()
        {
            base.CreateMainMenu();
            //TODO: Это что?
            if (Menu != null && Menu.Bars != null)
            {
                foreach (var bar in Menu.Bars.Where(p => p != null && p.Caption.EqIgnoreCase(StringResources.CustomizationBarMenu)).ToArray())
                {
                    foreach (var menu in bar.MenuItems.Where(p => p != null && p.Caption.EqIgnoreCase(StringResources.Swap)).OfType<ListMenuItem>().ToArray())
                    {
                        foreach (var submenu in menu.MenuItems.Where(p => p != null && p.Caption.EqIgnoreCase(StringResources.Download)).ToArray())
                        {
                            menu.MenuItems.Remove(submenu);
                        }
                    }
                }
            }
        }

        #region .  Methods  .
        protected override ObservableCollection<DataField> GetFields(Type type, SettingDisplay settings)
        {
            // получаем список полей как для детализации
            var items = base.GetFields(type, SettingDisplay.Detail);

            // убираем завязки на Lookup-ы (пусть отображаются Id-шники)
            foreach (var item in items)
            {
                item.LookupCode = null;
            }

            // во все настройки добавляем новые поля
            foreach (var f in _historyFieldsCache.Value)
                items.Add(f);
            return items;
        }

        protected override async Task<IEnumerable<HistoryWrapper<TModel>>> GetFilteredDataAsync(string sqlFilter)
        {
            return await Task.Factory.StartNew(() =>
            {
                using (var mgr = GetHistoryManager())
                    return mgr.GetHistory(sqlFilter);
            });
        }

        protected override void InitilizeManagerEventsMonitoring()
        {
            // он нам не нужен - ничего не делаем
        }

        protected override Type GetSecurityType()
        {
            // всер разрешения берем от базовой сущности
            return typeof(TModel);
        }

        public override void CreateProcessMenu() { }

        protected override bool CanDownloadXml()
        {
            return false;
        }

        private static ObservableCollection<DataField> CreateHistoryFields()
        {
            var res = new ObservableCollection<DataField>();

            var dfDateFrom = new DataField();
            dfDateFrom.Name = HistoryWrapper<TModel>.HDATEFROMPropertyName;
            dfDateFrom.Caption = "Действительна c";
            dfDateFrom.Description = "Дата начала периода актуальности";
            dfDateFrom.FieldName = dfDateFrom.Name;
            dfDateFrom.SourceName = dfDateFrom.Name;
            dfDateFrom.BindingPath = dfDateFrom.Name;
            dfDateFrom.FieldType = typeof(DateTime);
            dfDateFrom.DisplayFormat = "dd.MM.yyyy HH:mm:ss";
            res.Add(dfDateFrom);

            var dfDateTill = new DataField();
            dfDateTill.Name = HistoryWrapper<TModel>.HDATETILLPropertyName;
            dfDateTill.Caption = "Действительна по";
            dfDateTill.Description = "Дата окончания периода актуальности";
            dfDateTill.FieldName = dfDateTill.Name;
            dfDateTill.SourceName = dfDateTill.Name;
            dfDateTill.BindingPath = dfDateTill.Name;
            dfDateTill.FieldType = typeof(DateTime?);
            dfDateTill.DisplayFormat = "dd.MM.yyyy HH:mm:ss";
            res.Add(dfDateTill);

            var dfId = new DataField();
            dfId.Name = HistoryWrapper<TModel>.HISTORYIDPropertyName;
            dfId.Caption = "ИД истории";
            dfId.Description = "Уникальный идентификатор исторической записи";
            dfId.FieldName = dfId.Name;
            dfId.SourceName = dfId.Name;
            dfId.BindingPath = dfId.Name;
            dfId.FieldType = typeof(decimal);
            res.Add(dfId);

            var dfArchGuidId = new DataField();
            dfArchGuidId.Name = HistoryWrapper<TModel>.ARCHINSTGUID_RPropertyName;
            dfArchGuidId.Caption = "Архив";
            dfArchGuidId.Description = "Уникальный идентификатор записи Архива";
            dfArchGuidId.FieldName = dfArchGuidId.Name;
            dfArchGuidId.SourceName = dfArchGuidId.Name;
            dfArchGuidId.BindingPath = dfArchGuidId.Name;
            dfArchGuidId.FieldType = typeof(Guid);
            res.Add(dfArchGuidId);

            return res;
        }

        private static IHistoryManager<TModel> GetHistoryManager()
        {
            return (IHistoryManager<TModel>)IoC.Instance.Resolve<IBaseManager<TModel>>();
        } 
        #endregion
    }
}
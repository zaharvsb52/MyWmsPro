using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web;
using System.Windows;
using DevExpress.Data.Filtering;
using DevExpress.Mvvm;
using MLC.Ext.Wpf.Helpers;
using MLC.Ext.Wpf.ViewModels;
using wmsMLC.General.PL.WPF.Services;

namespace wmsMLC.DCL.Main.Services.Commands
{
    public class BrowseEntitiesCommand : IViewCommand
    {
        private readonly IDocumentManagerService _documentManagerService;
        private readonly IEntityViewModelFactory _entityViewModelFactory;

        public BrowseEntitiesCommand(IDocumentManagerService documentManagerService, IEntityViewModelFactory entityViewModelFactory)
        {
            Contract.Requires(documentManagerService != null);
            Contract.Requires(entityViewModelFactory != null);

            _documentManagerService = documentManagerService;
            _entityViewModelFactory = entityViewModelFactory;
        }

        public void Execute(string command)
        {
            var parameters = HttpUtility.ParseQueryString(command);
            if (!parameters.HasKeys())
                throw new ArgumentException(string.Format("Ошибка в формате browseEntities команды '{0}'.", command));

            const string entityTypeKey = "entityType";
            const string idKey = "id";
            const string criteriaKey = "criteria";
            const string titleKey = "title";
            
            Func<string, string> getparvaluehandler = key =>
            {
                return parameters.AllKeys.Any(p => p == key) ? parameters[key] : null;
            };

            var entityType = getparvaluehandler(entityTypeKey);
            //Проверяем наличие обязательного параметра entityType
            if (string.IsNullOrEmpty(entityType))
                throw new ArgumentException(string.Format("Ошибка в формате команды '{0}'. Значение параметра '{1}' не определено.", command, entityTypeKey));

            var id = getparvaluehandler(idKey);
            var criteria = getparvaluehandler(criteriaKey);
            var title = getparvaluehandler(titleKey);

            Invoke(new Action(() =>
            {
                var finddoc = FindDocumentById(_documentManagerService, id);
                if (finddoc != null)
                {
                    finddoc.Show();
                    return;
                }

                var vm = _entityViewModelFactory.CreateEntityList<EntityJournalViewModel>(entityType);
                vm.IsAllowAutoLoadStoreOnInit = false;
                if (!string.IsNullOrEmpty(criteria))
                {
                    var co = CriteriaOperator.Parse(criteria);
                    var filters = FilterCriteriaConverterHelper.ConvertToFilters(co).ToList();
                    vm.FiltersSet(filters);
                }

                var doc = _documentManagerService.CreateDocument("EntityListView", vm);
                if (!string.IsNullOrEmpty(id))
                    doc.Id = id;

                if (!string.IsNullOrEmpty(title))
                    doc.Title = title;

                doc.Show();
            }));
        }

        public static void Invoke(Delegate action)
        {
            if (Application.Current == null || Application.Current.Dispatcher.CheckAccess())
            {
                action.DynamicInvoke(null);
            }
            else
            {
                Application.Current.Dispatcher.Invoke(action);
            }
        }

        public static IDocument FindDocumentById(IDocumentManagerService docManager, object id)
        {
            if (docManager != null && id != null)
                return docManager.FindDocumentById(id);
            return null;
        }
    }
}
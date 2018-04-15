//TODO: продумать и вынести стратегии поиска имененований
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Microsoft.Practices.Unity;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.General.PL.WPF.ViewModels;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Main
{
    internal class CaliburnBootstrapper : BootstrapperBase
    {
        private readonly IUnityContainer _unityContainer;

        /// <summary>
        /// Стартуем (без Application)
        /// </summary>
        public CaliburnBootstrapper(IUnityContainer unityContainer)
            : base(false)
        {
            _unityContainer = unityContainer;
        }

        protected override void Configure()
        {
            base.Configure();

            ViewLocator.LocateTypeForModelType += LocateTypeForModelType;
            ViewModelLocator.LocateForView += LocateForView;

            _unityContainer.RegisterType<IEventAggregator, EventAggregator>(new ContainerControlledLifetimeManager());
        }

        #region .  IoC  .
        protected override object GetInstance(Type service, string key)
        {
            return _unityContainer.Resolve(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return new[] { _unityContainer.Resolve(service) };
        }

        protected override void BuildUp(object instance)
        {
            _unityContainer.BuildUp(instance);
        }
        #endregion

        private static object LocateForView(object o)
        {
            var viewModelTypeName = o.GetType().FullName;
            return viewModelTypeName.Replace("View", "ViewModel");
        }

        private static Type LocateTypeForModelType(Type modelType, DependencyObject arg2, object context)
        {
            var attribute = modelType
                .GetCustomAttributes(typeof(ViewAttribute), false)
                .Cast<ViewAttribute>()
                .FirstOrDefault(i => (i.Context == null && context == null) ||
                                        (i.Context != null && i.Context.Equals(context)) ||
                                        (context != null && context.Equals(i.Context)));
            if (attribute != null)
                return attribute.ViewType;

            if (modelType == typeof(MenuViewModel))
                return typeof(MenuView);

            if (modelType == typeof(FilterViewModel))
                return typeof(FilterView);

            if (modelType.IsGenericType && modelType.GetGenericTypeDefinition() == typeof(FilterViewModel))
                return typeof(FilterView);

            if (modelType.IsGenericType && modelType.GetGenericTypeDefinition() == typeof(MultipleSelectionViewModel<>))
                return typeof(MultipleSelectionView);

            if (modelType == typeof(AuthenticationViewModel))
                return typeof(AuthenticationView);

            if (modelType.IsGenericType && modelType.GetGenericTypeDefinition() == typeof(ListViewModelBase<>))
                return typeof(ObjectListView);

            if (modelType.IsGenericType && modelType.GetGenericTypeDefinition() == typeof(ObjectListViewModelBase<>))
                return typeof(ObjectListView);

            if (modelType.IsGenericType && modelType.GetGenericTypeDefinition() == typeof(CustomObjectListViewModelBase<>))
                return typeof(CustomObjectListView);

            if (modelType.IsGenericType && modelType.GetGenericTypeDefinition() == typeof(ObjectTreeViewModelBase<>))
                return typeof(ObjectTreeView);

            if (modelType.IsGenericType && modelType.GetGenericTypeDefinition() == typeof(HistoryListViewModelBase<>))
                return typeof(ObjectListView);

            if (modelType.IsGenericType && modelType.GetGenericTypeDefinition() == typeof(ObjectViewModelBase<>))
                return typeof(ObjectView);

            if (modelType.IsGenericType && modelType.GetGenericTypeDefinition() == typeof(CustomObjectViewModelBase<>))
                return typeof(ObjectView);

            if (modelType == typeof(PrintViewModel))
                return typeof(PrintView);

            if (modelType == typeof(PropertyViewModel))
                return typeof(PropertyView);
            
            if (modelType.IsGenericType && modelType.GetGenericTypeDefinition() == typeof(PivotViewModel<>))
                return typeof(PreviewPivotGridView);

            if (modelType.IsGenericType && modelType.GetGenericTypeDefinition() == typeof(ExportViewModel<>))
                return typeof(PreviewGridControlView);

            if (modelType == typeof(SystemMessageViewModel))
                return typeof(SystemMessageView);

            if (typeof(ExpandoObjectViewModelBase).IsAssignableFrom(modelType) || modelType == typeof(CustomParamValueViewModel))
                return typeof(CustomObjectView);

            //Дизайнер DCL
            if (modelType == typeof(DialogLayoutViewModel))
                return typeof(DialogLayoutView);

            //Дизайнер RCL
            if (modelType == typeof(RclDialogLayoutViewModel))
                return typeof(RclDesignerView);

            if (modelType.IsGenericType && modelType.GetGenericTypeDefinition() == typeof(CustomParamValueTreeViewModel<>))
                return typeof(CustomParamValueTreeView);

            if (modelType.IsGenericType && modelType.GetGenericTypeDefinition() == typeof(CustomParamValueObjectViewModel<>))
                return typeof(ObjectView);

            var fullName = modelType.FullName.Replace("Model", string.Empty);
            var viewType = modelType.Assembly.GetType(fullName);
            return viewType;
        }
    }
}
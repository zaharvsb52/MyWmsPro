//TODO: продумать и вынести стратегии поиска имененований

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Microsoft.Practices.Unity;
using wmsMLC.General.PL.WPF.Components.ViewModels;
using wmsMLC.General.PL.WPF.Views;
using wmsMLC.RCL.Main.ViewModels;
using wmsMLC.RCL.Main.Views;

namespace wmsMLC.RCL.Main
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

        private static Type LocateTypeForModelType(Type modelType, DependencyObject arg, object context)
        {
            var attribute = modelType
               .GetCustomAttributes(typeof(ViewAttribute), false)
               .Cast<ViewAttribute>()
               .FirstOrDefault(i => (i.Context == null && context == null) ||
                                       (i.Context != null && i.Context.Equals(context)) ||
                                       (context != null && context.Equals(i.Context)));
            if (attribute != null)
                return attribute.ViewType;

            //if (modelType == typeof(MenuViewModel))
            //    return typeof(MenuView);

            //if (modelType.IsGenericType && modelType.GetGenericTypeDefinition() == typeof(FilterViewModel<>))
            //    return typeof(FilterView);

            //if (modelType.IsGenericType && modelType.GetGenericTypeDefinition() == typeof(MultipleSelectionViewModel<>))
            //    return typeof(MultipleSelectionView);

            //if (modelType.IsGenericType && modelType.GetGenericTypeDefinition() == typeof(TreeFilterContentViewModel<>))
            //    return typeof(TreeFilterContentView);

            //if (modelType.IsGenericType && modelType.GetGenericTypeDefinition() == typeof(CustomFilterContentViewModel<>))
            //    return typeof(CustomFilterContentView);

            if (modelType == typeof(AuthenticationViewModel))
                return typeof(AuthenticationView);

            if (modelType == typeof(SystemMessageViewModel))
                return typeof(SystemMessageView);

            if (typeof(DialogSourceViewModel).IsAssignableFrom(modelType))
                return typeof(DialogSourceView);

            var fullName = modelType.FullName.Replace("Model", string.Empty);
            var viewType = modelType.Assembly.GetType(fullName);
            return viewType;
        }
    }
}
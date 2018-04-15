using System;
using System.Windows;
using Microsoft.Practices.Unity;
using wmsMLC.Activities.Business.Designer;
using wmsMLC.Business;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.Content.Factory;
using wmsMLC.DCL.Content.ViewModels;
using wmsMLC.DCL.Content.ViewModels.ArtMassInput;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Helpers;
using wmsMLC.DCL.Main.ViewModels;
using wmsMLC.DCL.Packing.ViewModels;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Factory;
using wmsMLC.General.PL.WPF.Services;

namespace wmsMLC.DCL.Content
{
    public sealed class ContentModule : ModuleBase
    {
        private readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(ContentModule));

        public ContentModule(IUnityContainer container) : base(container) { }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
            _log.Debug("Start configure content");

            // регистрируем Design-части Activity
            ActivityLibraryMetadata.RegisterAll();

            InitProcessAttributeStrategies();

            InitValidateStrategies();

            RegisterFactory();

            InitImageSelectorStrategies();

            IoC.Instance.Register(typeof(IDashboardDesignerViewModel), typeof(DashboardDesignerViewModel));

            _log.Debug("End configure content");
        }

        public override void Run()
        {
            base.Run();

            var viewService = Container.Resolve<IViewService>();
            var guaranteeExistViewModel = typeof(BPWorkflowListViewModel);
            foreach (var item in BLHelper.Registered)
            {
                // пытаемся найти ViewModel в стандартном namespace
                var assumeListViewModelName = string.Format("{0}.{1}ListViewModel", guaranteeExistViewModel.Namespace, item.ObjectType.Name);
                var assumeTreeViewModelName = string.Format("{0}.{1}TreeViewModel", guaranteeExistViewModel.Namespace, item.ObjectType.Name);
                var assumeObjectViewModelName = string.Format("{0}.{1}ViewModel", guaranteeExistViewModel.Namespace, item.ObjectType.Name);

                // если нет - берем базовый тип
                var listViewModelType = Type.GetType(assumeListViewModelName) ?? typeof(ObjectListViewModelBase<>).MakeGenericType(item.ObjectType);

                // для TreeView если нет - не берем базовый тип
                //var treeViewModelType = Type.GetType(assumeTreeViewModelName) ?? typeof(ObjectTreeViewModelBase<>).MakeGenericType(item.ObjectType);
                var treeViewModelType = Type.GetType(assumeTreeViewModelName);

                // если нет - берем базовый тип
                var objectViewModelType = Type.GetType(assumeObjectViewModelName) ?? typeof(ObjectViewModelBase<>).MakeGenericType(item.ObjectType);

                // регистриуем во viewservice
                viewService.Register(item.ObjectType.Name + ViewServiceRegisterSuffixListShow, listViewModelType);
                if (treeViewModelType != null)
                    viewService.Register(item.ObjectType.Name + ViewServiceRegisterSuffixTreeShow, treeViewModelType);

                // регистрируем в IoC
                var interfaceType = typeof (IListViewModel<>).MakeGenericType(item.ObjectType);
                IoC.Instance.Register(interfaceType, listViewModelType);

                var interfaceObjectType = typeof(IObjectViewModel<>).MakeGenericType(item.ObjectType);
                IoC.Instance.Register(interfaceObjectType, objectViewModelType);
            }

            viewService.Register("ARTMASSINPUT", typeof(ArtMassInputViewModel));
            viewService.Register(StringResources.Packing, typeof(IPackingViewModel));
            //viewService.Register("OPENWCL", typeof(OpenWclViewCommand));
            viewService.Register(StringResources.Chat, typeof(IChatViewModel));
            viewService.Register(MainViewModel.ShowMainMenuAction, typeof(MainMenuTreeViewModel));

            //Включаем кеширование wf 
            BPWorkflowManager.SetObjectCachable(BatchcodeWorkflowCodes.ExecuteWorkflowCode);
            foreach (var wf in BatchcodeWorkflowCodes.WorkflowCodes)
            {
                BPWorkflowManager.SetObjectCachable(wf);
            }

            LoadApplicationResource("/wmsMLC.DCL.Content;Component/Themes/Generic.xaml");

            SplashScreenHelper.SetState(StringResources.LoadCustomization);
            MainViewModel.LoadCustomization();
            MainViewModel.ShowTree();
            MainViewModel.LoadDxAssemblies();
        }

        /// <summary>
        /// Метод добавляющий стратегии формтирования аттрибутов сущности
        /// </summary>
        private static void InitProcessAttributeStrategies()
        {
            ProcessAttributeStrategiesHelper.Initialize();
        }

        public static void InitValidateStrategies()
        {
            ValidateStrategiesHelper.Initialize();
        }

        private static void InitImageSelectorStrategies()
        {
            ImageSelector.AddGetImageStrategy(typeof(UserGroup), o => ImageResources.DCLDefault16.GetBitmapImage());
            ImageSelector.AddGetImageStrategy(typeof(CustomParam), o => ImageResources.DCLDefault16.GetBitmapImage());
            ImageSelector.AddGetImageStrategy(typeof(MotionAreaGroupTr), o => ImageResources.DCLDefault16.GetBitmapImage());
            ImageSelector.AddGetImageStrategy(typeof(IPackingViewModel), o => ImageResources.DCLPackingPanel16.GetBitmapImage());
            ImageSelector.AddGetImageStrategy(typeof(IChatViewModel), o => ImageResources.DCLDefault16.GetBitmapImage());
            ImageSelector.AddGetImageStrategy(typeof(ObjectTreeMenu), o =>
                {
                    var item = o as ObjectTreeMenu;
                    if (item == null || string.IsNullOrEmpty(item.ObjectTreePictureSmall)) return null;
                    return ResourceHelper.GetImageByName("wmsMLC.DCL.Resources", "ImageResources",
                                                                   item.ObjectTreePictureSmall);
                });
        }

        private void RegisterFactory()
        {
            IoC.Instance.Register(typeof(IObjectListFactory), typeof(ObjectListFactory));
        }
    }
}
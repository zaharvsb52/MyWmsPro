using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using System.Xml;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;
using log4net;
using Microsoft.Practices.Unity;
using MLC.Ext.Wpf.Views;
using wmsMLC.Business;
using wmsMLC.DCL.Main.Helpers;
using wmsMLC.DCL.Main.Properties;
using wmsMLC.DCL.Main.Services;
using wmsMLC.DCL.Main.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.DCL.Main.Views.Controls;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Services;
using wmsMLC.General.PL.WPF.Views;
using ResourceHelper = wmsMLC.DCL.Resources.ResourceHelper;

namespace wmsMLC.DCL.Main
{
    public sealed class Module : ModuleBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Module));
        private const string DefaultTheme = "Office2013";

        public Module(IUnityContainer container) : base(container) { }

        public override void Run()
        {
            base.Run();

            // Настраиваем ImageSelector
            InitImageSelectorStrategies();

            // Настраиваем механизм сохранения настроек
            InitSaveRestoreStrategies();

            // Важно: стили нужно настраивать после отображения начальных форм. Иначе меню объектов не подхватит стиль
            InitDevexpress();

            LoadApplicationResource("/wmsMLC.DCL.Main;Component/Themes/Generic.xaml");

            // прогрузим инициализаторы DevExp
            //Убрал из-за ошибок при открытии Аналитики DevExpress 15.2.9
            //Не удалось привести тип объекта "DevExpress.Xpf.Bars.PopupMenuInfo" к типу "DevExpress.Xpf.Bars.PopupInfo`1[DevExpress.Xpf.Bars.PopupControlContainer]".
            //ImproveUIPerformance();

            //Подпишем кого надо на события
            BLHelper.SubscribeEvents();

            ConfigureNewDal();

            //Загрузим начальные кэши
            //BLHelper.FillInitialCaches();
        }

        private void ConfigureNewDal()
        {
            ViewLocator.Default = new ViewLocator(new[]
            {
                typeof (EntityListView).Assembly // MLC.Ext.Wpf
            });
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
            Log.Debug("Start configure Main");

            //Настраиваем Business Layer (Пока тут)
            BLHelper.InitBL(new IoCConfigurationContext { ExternalContainer = Container }, DALType.Service);

            //Регистрируем клиента для сервиса
            var clienttypecode = Settings.Default.ClientType.To(ClientTypeCode.None);
            if (clienttypecode == ClientTypeCode.None)
                throw new DeveloperException("ClientType '{0}' is undefined.", clienttypecode);
            BLHelper.RegisterServiceClient(Settings.Default.SessionId, clienttypecode, Settings.Default.SDCL_Endpoint);

            // подписываемся на обработку ошибок
            ExceptionPolicy.Instance.ExceptionOccure += OnExceptionOccure;

            // регистрируем Shell
            Container.RegisterType<Shell>(new ContainerControlledLifetimeManager()).RegisterType<IShell, Shell>();

            // отрабатываем PRISM
            //var shell = (DependencyObject)Container.Resolve<IShell>();
            //RegionManager.SetRegionManager(shell, Container.Resolve<IRegionManager>());

            Log.Debug("50% configure Main");

            // регистрируем сервисы
            Container.RegisterType<IViewService, ViewService>(new ContainerControlledLifetimeManager());

            // регистрируем помощника по SQL (когда появися MS SQL нужно будет подменить)
            Container.RegisterType<ISqlExpressionHelper, SqlExpressionHelper>(new ContainerControlledLifetimeManager());

            // регистрируем главную форму
            Container.RegisterInstance(typeof(MainView), Container.Resolve<MainView>(), new ContainerControlledLifetimeManager());

            // стартуем Caliburn
            var caliburnBootstrapper = new CaliburnBootstrapper(Container);
            caliburnBootstrapper.Start();

            // регистрируем главный регион
            //var regionManager = Container.Resolve<IRegionManager>();
            //regionManager.AddToRegion(RegionNames.MainRegion, Container.Resolve<MainView>());

            IoC.Instance.Register<IAuthenticationViewModel, AuthenticationViewModel>();

            Log.Debug("End configure Main");
        }

        //private void ImproveUIPerformance()
        //{
        //    RunTypeInitializers(Assembly.GetAssembly(typeof(LayoutHelper))); //Core
        //    RunTypeInitializers(Assembly.GetAssembly(typeof(TextEdit))); //Editors
        //    RunTypeInitializers(Assembly.GetAssembly(typeof(DockLayoutManager))); //Docking
        //    RunTypeInitializers(Assembly.GetAssembly(typeof(GridControl))); //Grid
        //    RunTypeInitializers(Assembly.GetAssembly(typeof(BarManager))); //BarManager
        //}

        //private static void RunTypeInitializers(Assembly a)
        //{
        //    Type[] types = a.GetExportedTypes();
        //    for (int i = 0; i < types.Length; i++)
        //    {
        //        RuntimeHelpers.RunClassConstructor(types[i].TypeHandle);
        //    }
        //}

        private static void InitImageSelectorStrategies()
        {
            ImageSelector.AddGetImageStrategy(typeof(TreeItem), o =>
                {
                    var item = o as TreeItem;
                    if (item == null || string.IsNullOrEmpty(item.ImageName)) 
                        return null;
                    return ResourceHelper.GetImageByName("wmsMLC.DCL.Resources", "ImageResources", item.ImageName);
                });
        }

        //Кеш содержимого файла настроек
        private static readonly Dictionary<string, string> LayoutInfo = new Dictionary<string, string>(); 

        private static void InitSaveRestoreStrategies()
        {
            // учим как сохранять/восстанавливать Grid
            SaveRestoreLayoutHelper.RegisterSaveRestoreStrategy(typeof(GridDataControlBase), context =>
            {
                if (!context.ValidateAction())
                    return;
                var controlGrid = context.ProcessingObject as CustomGridControl;
                var controlTree = context.ProcessingObject as CustomTreeListControl;
                if (controlGrid == null && controlTree == null)
                    return;

                switch (context.ActionType)
                {
                    case SaveRestoreActionType.Save:
                        foreach (var fileConfig in context.FileNames)
                        {
                            var vers = fileConfig.Version == null ? new Version(0, 0, 0, 0) : new Version(fileConfig.Version.Major, fileConfig.Version.Minor, fileConfig.Version.Build, fileConfig.Version.Revision + 1);
                            fileConfig.Version = vers;
                        }
                        PrepareSaveFile(context);
                        foreach (var fileNames in context.FileNames)
                        {
                            fileNames.SetCurrentFileName();
                        }

                        if (context.FileNames.Length > 0)
                        {
                            var fl = new string[context.FileNames.Length];

                            for (var i = 0; i < fl.Length; i++)
                            {
                                fl[i] = context.FileNames[i].GetFullFileNameClient();
                                if (LayoutInfo.ContainsKey(fl[i]))
                                    LayoutInfo.Remove(fl[i]);
                            }

                            if (controlGrid != null)
                                controlGrid.SaveLayoutToXml(fl);
                            else
                                controlTree.SaveLayoutToXml(fl);
                        }
                        break;
                    case SaveRestoreActionType.Restore:
                        for (var i = 0; i < context.FileNames.Length; i++)
                        {
                            var fileNames = context.FileNames[i];
                            var filenameClient = fileNames.CurrentFullFileName;

                            if (string.IsNullOrEmpty(filenameClient) || !File.Exists(filenameClient))
                            {
                                filenameClient = string.Empty;
                                if (context.FilesDb != null)
                                {
                                    byte[] filedb;
                                    var filenameBd = fileNames.GetFileNameBd();
                                    if (context.FilesDb.ContainsKey(filenameBd) &&
                                        (filedb = context.FilesDb[filenameBd].Text) != null)
                                    {
                                        fileNames.Version = context.FilesDb[filenameBd].Version;
                                        PrepareSaveFile(context.FileNames[i]);
                                        fileNames.SetCurrentFileName();
                                        filenameClient = fileNames.CurrentFullFileName;
                                        File.WriteAllBytes(filenameClient, filedb);
                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(filenameClient))
                            {
                                if (LayoutInfo.ContainsKey(filenameClient))
                                {
                                    var layout = LayoutInfo[filenameClient];
                                    if (controlGrid != null)
                                        controlGrid.RestoreLayoutFromString(layout, i);
                                    else
                                        controlTree.RestoreLayoutFromString(layout, i);
                                }
                                else
                                {
                                    if (File.Exists(filenameClient))
                                    {
                                        try
                                        {
                                            using (var fileStream = new FileStream(filenameClient, FileMode.Open))
                                            {
                                                using (var wr = new StreamReader(fileStream))
                                                {
                                                    var layout = wr.ReadToEnd();
                                                    if (controlGrid != null)
                                                        controlGrid.RestoreLayoutFromString(layout, i);
                                                    else
                                                        controlTree.RestoreLayoutFromString(layout, i);
                                                    LayoutInfo[filenameClient] = layout;
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            File.Delete(filenameClient);
                                            Log.Error(
                                                new OperationException(
                                                    string.Format(ExceptionResources.RestoreLayoutBadFile,
                                                        filenameClient, Environment.NewLine), ex));
                                        }
                                    }
                                }
                            }
                        }

                        break;
                    case SaveRestoreActionType.Clear:
                        Clear(context);
                        break;
                    default:
                        throw new DeveloperException("Unknown enum value.");
                }
            });


            // учим как сохранять/восстанавливать CustomDataLayoutControl
            SaveRestoreLayoutHelper.RegisterSaveRestoreStrategy(typeof(CustomDataLayoutControl), context =>
            {
                if (!context.ValidateAction())
                    return;
                var control = context.ProcessingObject as CustomDataLayoutControl;
                if (control == null)
                    throw new DeveloperException("Strategy is not valid.");

                switch (context.ActionType)
                {
                    case SaveRestoreActionType.Save:
                        foreach (var fileConfig in context.FileNames)
                        {
                            var vers = fileConfig.Version == null
                                ? new Version(0, 0, 0, 0)
                                : new Version(fileConfig.Version.Major, fileConfig.Version.Minor,
                                    fileConfig.Version.Build, fileConfig.Version.Revision + 1);
                            fileConfig.Version = vers;
                        }
                        PrepareSaveFile(context);

                        foreach (var fileconfig in context.FileNames)
                        {
                            fileconfig.SetCurrentFileName();

                            var filenameClient = fileconfig.GetFullFileNameClient();
                            if (LayoutInfo.ContainsKey(filenameClient))
                                LayoutInfo.Remove(filenameClient);
                            using (var fileStream = new FileStream(filenameClient, FileMode.OpenOrCreate))
                            {
                                using (var wr = XmlWriter.Create(fileStream))
                                {
                                    control.WriteToXML(wr);
                                }
                                fileStream.Flush();
                            }
                        }
                        break;
                    case SaveRestoreActionType.Restore:
                        foreach (var fileConfig in context.FileNames)
                        {
                            string filenameClient = fileConfig.CurrentFullFileName;

                            if (string.IsNullOrEmpty(filenameClient) || !File.Exists(filenameClient))
                            {
                                filenameClient = string.Empty;
                                if (context.FilesDb != null)
                                {
                                    byte[] filedb;
                                    var filenameBd = fileConfig.GetFileNameBd();
                                    if (context.FilesDb.ContainsKey(filenameBd) &&
                                        (filedb = context.FilesDb[filenameBd].Text) != null)
                                    {
                                        fileConfig.Version = context.FilesDb[filenameBd].Version;
                                        PrepareSaveFile(fileConfig);
                                        fileConfig.SetCurrentFileName();
                                        filenameClient = fileConfig.CurrentFullFileName;
                                        File.WriteAllBytes(filenameClient, filedb);
                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(filenameClient))
                            {
                                if (LayoutInfo.ContainsKey(filenameClient))
                                {
                                    control.RestoreLayout(LayoutInfo[filenameClient]);
                                }
                                else
                                {
                                    if (File.Exists(filenameClient))
                                    {
                                        var needDelete = false;
                                        using (var fileStream = new FileStream(filenameClient, FileMode.Open))
                                        {
                                            using (var wr = new StreamReader(fileStream))
                                            {
                                                try
                                                {
                                                    var layout = wr.ReadToEnd();
                                                    control.RestoreLayout(layout);
                                                    LayoutInfo[filenameClient] = layout;
                                                }
                                                catch (Exception ex)
                                                {
                                                    needDelete = true;
                                                    Log.Error(
                                                        new OperationException(
                                                            string.Format(ExceptionResources.RestoreLayoutBadFile,
                                                                filenameClient, Environment.NewLine), ex));
                                                }
                                            }
                                        }
                                        if (needDelete)
                                            File.Delete(filenameClient);
                                    }
                                }
                            }
                        }
                        break;
                    case SaveRestoreActionType.Clear:
                        Clear(context);
                        break;
                    default:
                        throw new DeveloperException("Unknown enum value.");
                }
            });

            // учим как сохранять/восстанавливать CustomLookUpEdit
            SaveRestoreLayoutHelper.RegisterSaveRestoreStrategy(typeof(CustomLookUpEdit), context =>
            {
                if (!context.ValidateAction())
                    return;
                var control = context.ProcessingObject as CustomLookUpEdit;
                if (control == null)
                    throw new DeveloperException("Strategy is not valid.");

                switch (context.ActionType)
                {
                    case SaveRestoreActionType.Save:
                        if (control.PopupContentGridControl != null)
                        {
                            foreach (var fileConfig in context.FileNames)
                            {
                                var vers = fileConfig.Version == null
                                    ? new Version(0, 0, 0, 0)
                                    : new Version(fileConfig.Version.Major, fileConfig.Version.Minor,
                                        fileConfig.Version.Build, fileConfig.Version.Revision + 1);
                                fileConfig.Version = vers;
                            }
                            PrepareSaveFile(context);
                            foreach (var fileConfig in context.FileNames)
                            {
                                fileConfig.SetCurrentFileName();
                                control.PopupContentGridControl.SaveLayoutToXml(fileConfig.CurrentFullFileName);

                            }
                        }
                        break;
                    case SaveRestoreActionType.Restore:
                        foreach (var fileConfig in context.FileNames)
                        {
                            string filenameClient = fileConfig.CurrentFullFileName;

                            if (string.IsNullOrEmpty(filenameClient) || !File.Exists(filenameClient))
                            {
                                filenameClient = string.Empty;
                                if (context.FilesDb != null)
                                {
                                    byte[] filedb;
                                    var filenameBd = fileConfig.GetFileNameBd();
                                    if (context.FilesDb.ContainsKey(filenameBd) &&
                                        (filedb = context.FilesDb[filenameBd].Text) != null)
                                    {
                                        fileConfig.Version = context.FilesDb[filenameBd].Version;
                                        PrepareSaveFile(fileConfig);
                                        fileConfig.SetCurrentFileName();
                                        filenameClient = fileConfig.CurrentFullFileName;
                                        File.WriteAllBytes(filenameClient, filedb);
                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(filenameClient) && File.Exists(filenameClient))
                            {
                                try
                                {
                                    if (control.PopupContentGridControl != null)
                                        control.PopupContentGridControl.RestoreLayoutFromXml(filenameClient);
                                }
                                catch (Exception ex)
                                {
                                    File.Delete(filenameClient);
                                    Log.Error(new OperationException(string.Format(ExceptionResources.RestoreLayoutBadFile, filenameClient, Environment.NewLine), ex));
                                }
                            }
                        }
                        break;
                    case SaveRestoreActionType.Clear:
                       Clear(context);
                        break;
                    default:
                        throw new DeveloperException("Unknown enum value.");
                }
            });

            // учим как сохранять/восстанавливать CustomBarManager
            SaveRestoreLayoutHelper.RegisterSaveRestoreStrategy(typeof(CustomBarManager), context =>
            {
                if (!context.ValidateAction())
                    return;
                var control = context.ProcessingObject as CustomBarManager;
                if (control == null)
                    throw new DeveloperException("Strategy is not valid.");

                switch (context.ActionType)
                {
                    case SaveRestoreActionType.Save:
                        foreach (var fileConfig in context.FileNames)
                        {
                            var vers = fileConfig.Version == null ? new Version(0, 0, 0, 0) : new Version(fileConfig.Version.Major, fileConfig.Version.Minor, fileConfig.Version.Build, fileConfig.Version.Revision + 1);
                            fileConfig.Version = vers;
                        }
                        PrepareSaveFile(context);
                        var count = context.FileNames.Length - 1;
                        for (var i = 0; i <= count; i++)
                        {
                            var fileConfig = context.FileNames[i];
                            fileConfig.SetCurrentFileName();
                            var filenameClient = fileConfig.CurrentFullFileName;
                            control.SaveLayoutToXml(filenameClient, i == count && count > 0); //Если файл один, то нет глобальных настроек
                            if (LayoutInfo.ContainsKey(filenameClient))
                                LayoutInfo.Remove(filenameClient);
                        }
                        break;

                    case SaveRestoreActionType.Restore:
                        foreach (var fileConfig in context.FileNames)
                        {
                            var filenameClient = fileConfig.CurrentFullFileName;

                            if (string.IsNullOrEmpty(filenameClient) || !File.Exists(filenameClient))
                            {
                                filenameClient = string.Empty;
                                if (context.FilesDb != null)
                                {
                                    byte[] filedb;
                                    var filenameBd = fileConfig.GetFileNameBd();
                                    if (context.FilesDb.ContainsKey(filenameBd) &&
                                        (filedb = context.FilesDb[filenameBd].Text) != null)
                                    {
                                        fileConfig.Version = context.FilesDb[filenameBd].Version;
                                        PrepareSaveFile(fileConfig);
                                        fileConfig.SetCurrentFileName();
                                        filenameClient = fileConfig.CurrentFullFileName;
                                        File.WriteAllBytes(filenameClient, filedb);
                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(filenameClient))
                            {
                                if (LayoutInfo.ContainsKey(filenameClient))
                                {
                                    control.RestoreLayoutFromString(LayoutInfo[filenameClient]);
                                }
                                else
                                {
                                    if (File.Exists(filenameClient))
                                    {
                                        try
                                        {
                                            //control.RestoreLayoutFromXml(filenameClient);
                                            using (var fileStream = new FileStream(filenameClient, FileMode.Open))
                                            {
                                                using (var wr = new StreamReader(fileStream))
                                                {
                                                    var layout = wr.ReadToEnd();
                                                    control.RestoreLayoutFromString(layout);
                                                    LayoutInfo[filenameClient] = layout;
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            File.Delete(filenameClient);
                                            Log.Error(
                                                new OperationException(
                                                    string.Format(ExceptionResources.RestoreLayoutBadFile,
                                                        filenameClient,
                                                        Environment.NewLine), ex));
                                        }
                                    }
                                }
                            }
                        }
                        break;

                    case SaveRestoreActionType.Clear:
                        Clear(context);
                        break;
                    default:
                        throw new DeveloperException("Unknown enum value.");
                }
            });

            // учим как сохранять/восстанавливать Window
            SaveRestoreLayoutHelper.RegisterSaveRestoreStrategy(typeof (Window), context =>
            {
                 if (!context.ValidateAction())
                    return;
                 var control = context.ProcessingObject as Window;
                 if (control == null)
                     throw new DeveloperException("Strategy is not valid.");

                switch (context.ActionType)
                {
                    case SaveRestoreActionType.Save:
                        foreach (var fileConfig in context.FileNames)
                        {
                            var vers = fileConfig.Version == null ? new Version(0, 0, 0, 0) : new Version(fileConfig.Version.Major, fileConfig.Version.Minor, fileConfig.Version.Build, fileConfig.Version.Revision + 1);
                            fileConfig.Version = vers;
                        }
                        for (var i = 0; i < context.FileNames.Length; i++)
                        {
                            if ((i == 0 && (context.FormComponents & FormComponents.FormSize) == FormComponents.FormSize) ||
                                (i == 1 && (context.FormComponents & FormComponents.FormPosition) == FormComponents.FormPosition))
                            {
                                PrepareSaveFile(context.FileNames[i]);
                            }
                        }
                        foreach (var fileNames in context.FileNames)
                        {
                            fileNames.SetCurrentFileName();
                        }

                        if (context.FileNames.Length > 0)
                        {
                            var saveRestoreLayoutWindowHelper = new SaveRestoreLayoutWindowHelper();
                            if ((context.FormComponents & FormComponents.FormSize) == FormComponents.FormSize)
                            {
                                var file = context.FileNames[0].GetFullFileNameClient();
                                saveRestoreLayoutWindowHelper.SaveToXmlWindowSize(file, control);
                            }
                            if ((context.FormComponents & FormComponents.FormPosition) == FormComponents.FormPosition && context.FileNames.Length >= 1)
                            {
                                var file = context.FileNames[1].GetFullFileNameClient();
                                saveRestoreLayoutWindowHelper.SaveToXmlWindowPosition(file, control);
                            }
                        }
                        break;
                    case SaveRestoreActionType.Restore:
                        for (var i = 0; i < context.FileNames.Length; i++)
                        {
                            if ((i == 0 && (context.FormComponents & FormComponents.FormSize) == FormComponents.FormSize) ||
                                (i == 1 && (context.FormComponents & FormComponents.FormPosition) == FormComponents.FormPosition))
                            {
                                var fileNames = context.FileNames[i];
                                var filenameClient = fileNames.CurrentFullFileName;

                                if (string.IsNullOrEmpty(filenameClient) || !File.Exists(filenameClient))
                                {
                                    filenameClient = string.Empty;
                                    if (context.FilesDb != null)
                                    {
                                        byte[] filedb;
                                        var filenameBd = fileNames.GetFileNameBd();
                                        if (context.FilesDb.ContainsKey(filenameBd) &&
                                            (filedb = context.FilesDb[filenameBd].Text) != null)
                                        {
                                            fileNames.Version = context.FilesDb[filenameBd].Version;
                                            PrepareSaveFile(context.FileNames[i]);
                                            fileNames.SetCurrentFileName();
                                            filenameClient = fileNames.CurrentFullFileName;
                                            File.WriteAllBytes(filenameClient, filedb);
                                        }
                                    }
                                }

                                if (!string.IsNullOrEmpty(filenameClient) && File.Exists(filenameClient))
                                {
                                    var saveRestoreLayoutWindowHelper = new SaveRestoreLayoutWindowHelper();
                                    try
                                    {
                                        if (i == 0)
                                            saveRestoreLayoutWindowHelper.LoadFromXmlWindowSize(filenameClient, control);
                                        else if (i == 1)
                                            saveRestoreLayoutWindowHelper.LoadFromXmlWindowPosition(filenameClient,
                                                control);
                                    }
                                    catch (Exception ex)
                                    {
                                        File.Delete(filenameClient);
                                        Log.Error(
                                            new OperationException(
                                                string.Format(ExceptionResources.RestoreLayoutBadFile,
                                                    filenameClient, Environment.NewLine), ex));
                                    }
                                }
                            }
                        }
                        break;
                    case SaveRestoreActionType.Clear:
                         for (var i = 0; i < context.FileNames.Length; i++)
                        {
                            if ((i == 0 && (context.FormComponents & FormComponents.FormSize) == FormComponents.FormSize) ||
                                (i == 1 && (context.FormComponents & FormComponents.FormPosition) == FormComponents.FormPosition))
                            {
                                Clear(context.FileNames[i], context.FilesDb);
                            }
                         }
                        break;
                    default:
                        throw new DeveloperException("Unknown enum value.");
                }
            });
        }

        private static void Clear(FileAppearanceSettingsClient fileNames, IDictionary<string, FileAppearanceSettingsDb> filesDb)
        {
            //Сначала удаляем текущий файл
            if (!string.IsNullOrEmpty(fileNames.CurrentFullFileName))
            {
                var filenameClient = fileNames.CurrentFullFileName;
                if (LayoutInfo.ContainsKey(filenameClient))
                    LayoutInfo.Remove(filenameClient);
                var fi = new FileInfo(filenameClient);
                if (fi.Exists)
                    fi.Delete();
            }

            //Затем, если настройки есть в БД, восстанавливаем их
            if (filesDb != null)
            {
                byte[] filedb;
                var filenameBd = fileNames.GetFileNameBd();
                if (filesDb.ContainsKey(filenameBd) && (filedb = filesDb[filenameBd].Text) != null)
                {
                    fileNames.Version = filesDb[filenameBd].Version;
                    fileNames.SetCurrentFileName();
                    File.WriteAllBytes(fileNames.CurrentFullFileName, filedb);
                }
            }
        }

        private static void Clear(SaveRestoreContext context)
        {
            foreach (var fileNames in context.FileNames)
            {
                Clear(fileNames, context.FilesDb);
            }
        }

        private static void PrepareSaveFile(FileAppearanceSettingsClient fileConfig)
        {
            var dir = new DirectoryInfo(fileConfig.Root);
            if (!dir.Exists)
            {
                try
                {
                    dir.Create();
                }
                catch (Exception ex)
                {
                    throw new DeveloperException("Не удалось создать папку для сохранения настроек.", ex);
                }
            }

            if (!string.IsNullOrEmpty(fileConfig.CurrentFullFileName))
            {
                var fi = new FileInfo(fileConfig.CurrentFullFileName);
                if (fi.Exists)
                    fi.Delete();
            }

            if (!string.IsNullOrEmpty(fileConfig.GetFullFileNameClient()))
            {
                var fi = new FileInfo(fileConfig.GetFullFileNameClient());
                if (fi.Exists)
                    fi.Delete();
            }
        }


        private static void PrepareSaveFile(SaveRestoreContext context)
        {
            if (context.FileNames == null)
                return;

            foreach (var fileConfig in context.FileNames)
            {
                PrepareSaveFile(fileConfig);
            }
        }

        private static void InitDevexpress()
        {
            //DevExpress.Xpf.Core.ThemeManager.ApplicationThemeName = "DXStyle";
            //if (Theme.Themes.All(i => i.Name != "GrayOne"))
            //{
            //    var theme = new Theme("GrayOne", "/DevExpress.Xpf.Themes.GrayOne.v12.2;component/Themes/Generic.xaml")
            //        {
            //            FullName = "Custom wmsMLCThemeGrayOne based theme",
            //            AssemblyName = "DevExpress.Xpf.Themes.GrayOne.v12.2"
            //        };
            //    Theme.RegisterTheme(theme);
            //}
            if (Settings.Default.CallUpgrade)
            {
                Settings.Default.Upgrade();
                Settings.Default.CallUpgrade = false;
            }
            ThemeManager.ApplicationThemeName = Settings.Default.Theme ?? DefaultTheme;

            DataControlBase.AllowInfiniteGridSize = true;
        }

        private static void OnExceptionOccure(object sender, ExceptionEventArgs e)
        {
            if (e.PolicyName == BlExceptionHandler.BusinessLogicPolicyName)
                return; //В лог записали. Должно сработать OperationException

            // для процессов такие ошибки нужно пробрасывать до пользователя
            if (e.PolicyName == "BP" && e.Exception is PassThroughException)
            {
                e.Handled = true;
                e.NeedThrow = true;
                return;
            }

            try
            {
                // обрабатываем (последний рубеж)
                Log.DebugFormat("Start showing exception message");

                //INFO: пробуем сделать все правильно и поживем вот так
                // если все ок, то закоментированный код удалить
                if (Application.Current.Dispatcher.CheckAccess())
                {
                    Log.DebugFormat("Main window is not loaded. Show new error window");
                    ShowError(e.Exception);
                }
                else
                {
                    Log.DebugFormat("Main window is loaded. Trying to show throw dispatcher");
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)(() =>
                    {
                        try
                        {
                            ShowError(e.Exception);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex);
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                Log.FatalFormat("!!! === EXCEPTION WAS NOT SHOWN === !!!");
                Log.Debug(ex);
            }

            e.NeedThrow = false;
            e.Handled = true;
        }

        private static void ShowError(Exception ex)
        {
            ErrorBox.ShowError("Exception: ", ex);
        }
    }
}
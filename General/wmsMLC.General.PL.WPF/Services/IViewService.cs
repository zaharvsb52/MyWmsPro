using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using wmsMLC.General.PL.WPF.ViewModels;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.General.PL.WPF.Services
{
    public interface IViewService
    {
        void Register(string command, Type viewModelOrCommandType);

        void Show(string command);
        void Show(string command, ShowContext context);
        IViewModel ResolveViewModel(string command);
        bool TryResolveViewModel(string command, out IViewModel viewModel);
        IViewCommand ResolveViewCommand(string command);
        bool TryResolveViewCommand(string command, out IViewCommand viewCommand);

        void Show(IViewModel viewModel);
        void Show(IViewModel viewModel, ShowContext context);
        void Show(IViewModel viewModel, ShowContext context, ref IViewModel currentViewModel);
        void Show(object viewModel, object id, object title);
        IView GetView(IViewModel viewModel);
        void AddViewToCache(Type modelType, IView view);

        void Show(IView view);
        void Show(IView view, ShowContext context);

        void Show(FrameworkElement content);
        void Show(FrameworkElement content, ShowContext context);

        bool? SaveLayout(IViewModel viewModel, string title);
        bool SaveLayout(FrameworkElement element, FormComponents formComponents);
        bool? SaveDBLayout(IViewModel viewModel, string title, bool upCriticalVersion = false);
        bool SaveDBLayout(FrameworkElement element, FormComponents formComponents, bool upCriticalVersion = false);
        void AllRestoreLayout();
        void RestoreLayout(FrameworkElement obj);
        bool? ClearLayout(IViewModel viewModel, string title, Type[] actionProcessingObjectTypes = null, Type[] doNotActionProcessingObjectTypes = null);
        bool ClearLayout(FrameworkElement obj, FormComponents formComponents, Type[] actionProcessingObjectTypes = null, Type[] doNotActionProcessingObjectTypes = null);

        MessageBoxResult ShowDialog(string title, string message,
                                    MessageBoxButton buttons,
                                    MessageBoxImage image,
                                    MessageBoxResult defaultResult,
                                    double? fontSize = null);

        bool? ShowDialogWindow(IViewModel viewModel, bool isRestoredLayout, bool isNotNeededClosingOnOkResult = false,
            string width = null, string height = null, bool noButtons = false,
            MessageBoxButton buttons = MessageBoxButton.OKCancel, bool noActionOnCancelKey = false, IntPtr? intPtrOwner = null);

        bool Close(object viewOrViewModel, bool isDialog = false);
        bool CloseAll();
        bool CloseAll(bool absolutely);

        void MakeMaxSize(IViewModel viewModel, bool isMax = true);

        void ShowError(string message, Exception ex, string[] attachments, Dictionary<string, string> additionalParams,
            bool isBtnSandMailEnable = true, string mandantCode = null, bool isMandantListVisible = true);
        void ShowError(string message, Exception ex, bool isBtnSandMailEnable = true);

        /// <summary>
        /// Получим активное в данный момент окно.
        /// </summary>
        Window GetActiveWindow();

        Task GetWebEntityMappingAsync(string wmsentity);
        bool TryGetWmsWebObject(string wmsobject, out string wmswebobject);
    }

    public class ShowContext
    {
        public static DockType DefaultDockType = DockType.Document;

        public ShowContext()
        {
            DockingType = DefaultDockType;
            InDialog = false;
            ShowInNewWindow = false;
            LoadLayoutSettings = true;
        }

        public DockType DockingType { get; set; }
        public bool InDialog { get; set; }
        public bool ShowInNewWindow { get; set; }
        public bool LoadLayoutSettings { get; set; }
    }

    [Flags]
    public enum FormComponents
    {
        None = 0,
        Menu = 1,
        Components      = 0x10,
        FormSize        = 0x100,
        FormPosition    = 0x1000,
        MenuAndComponents = Menu | Components,
        All = MenuAndComponents | FormSize | FormPosition
    }
}
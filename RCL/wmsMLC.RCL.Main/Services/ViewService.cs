using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Caliburn.Micro;
using Microsoft.Practices.Unity;
using wmsMLC.General;
using wmsMLC.General.PL.WPF.Components.Controls.Rcl;
using wmsMLC.General.PL.WPF.Components.ViewModels;
using wmsMLC.General.PL.WPF.Events;
using wmsMLC.General.PL.WPF.Services;
using wmsMLC.General.PL.WPF.ViewModels;
using wmsMLC.General.PL.WPF.Views;
using wmsMLC.RCL.Main.Views;

namespace wmsMLC.RCL.Main.Services
{
    public class ViewService : IViewService, IHandle<CloseRequestEvent>, IHandle<ErrorEvent>
    {
        #region .  Constants & Fields  .
        private readonly Dictionary<Type, UIElement> _views = new Dictionary<Type, UIElement>();
        #endregion  .  Constants & Fields  .

        public ViewService(IUnityContainer container)
        {
            var eventAggregator = container.Resolve<IEventAggregator>();
            eventAggregator.Subscribe(this);
        }

        #region . IViewService  .
        public MessageBoxResult ShowDialog(string title, string message, MessageBoxButton buttons, MessageBoxImage image, MessageBoxResult defaultResult, double? fontSize = null)
        {
            if (Application.Current.Dispatcher.CheckAccess())
                return CustomMessageBox.Show(message, title, buttons, image, defaultResult, fontSize ?? 14);
            return (MessageBoxResult)Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                new Func<MessageBoxResult>(() => CustomMessageBox.Show(message, title, buttons, image, defaultResult, fontSize ?? 14)));

            //if (Application.Current.Dispatcher.CheckAccess())
            //    return DXMessageBox.Show(message, title, buttons, image, defaultResult);
            //return (MessageBoxResult)Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
            //    new Func<MessageBoxResult>(() => DXMessageBox.Show(message, title, buttons, image, defaultResult)));
        }

        public bool? ShowDialogWindow(IViewModel viewModel, bool isRestoredLayout, bool isNotNeededClosingOnOkResult = false, string width = null, string height = null, bool noButtons = false, MessageBoxButton buttons = MessageBoxButton.OKCancel, bool noActionOnCancelKey = false, IntPtr? intPtrOwner = null)
        {
            var view = GetBindViewByViewModel(viewModel);

            // если view не является window, то его не нужно оборачивать
            var dialogWindow = view as Window;
            if (dialogWindow == null)
            {
                //Если калибрун не сбиндил тайтлы, попробуем выставить сами
                var panel = view as IPanelView;
                if (panel != null)
                    if (string.IsNullOrEmpty(panel.PanelCaption))
                        panel.PanelCaption = ((RclPanelViewModelBase)viewModel).PanelCaption ?? string.Empty;
                var newWindow = CreateDialogWindow(view);
                dialogWindow = newWindow;
            }

            // присваиваем родителя
            if (dialogWindow.Owner == null && Application.Current.MainWindow.IsActive)
                dialogWindow.Owner = Application.Current.MainWindow;

            //Закрываем окно на меню клик
            var menuHandler = viewModel as IRclMenuHandler;
            if (menuHandler != null)
            {
                menuHandler.MenuAction = parameter =>
                {
                    if (parameter == null)
                        throw new DeveloperException("Parameter of MenuAction is null.");

                    var dialogresult = parameter.Value.To(Key.None) != Key.Escape;

                    if (dialogWindow is CustomDXDialog)
                    {
                        ((CustomDXDialog)dialogWindow).Close(dialogresult);
                    }
                    else
                    {
                        dialogWindow.DialogResult = dialogresult;
                        dialogWindow.Close();
                    }
                };
            }

            var dr = dialogWindow.ShowDialog();
            return dialogWindow is CustomDXDialog ? ((CustomDXDialog)dialogWindow).TrueResult : dr;
        }

        private static CustomDXDialog CreateDialogWindow(UIElement view)
        {
            var newWindow = new CustomDXDialog
                {
#if DEBUG
                    ShowTitle = true,
                    //ShowIcon = false,
                    ShowInTaskbar = true,
                    ResizeMode = ResizeMode.CanResize,
                    Content = view,
                    WindowState = WindowState.Normal,
#else
                    ShowTitle = false,
                    //ShowIcon = false,
                    ShowInTaskbar = false,
                    ResizeMode = ResizeMode.NoResize,
                    Content = view,
                    WindowState = WindowState.Maximized,
                    Topmost = true
#endif
                };
            if (view is IPanelView)
            {
                newWindow.ShowTitle = true;
                var title = ((IPanelView)view).PanelCaption;
                if (!string.IsNullOrEmpty(title))
                    newWindow.Title = title;
            }
            return newWindow;
        }

        private UIElement GetBindViewByViewModel(IViewModel viewModel)
        {
            if (viewModel == null)
                throw new ArgumentNullException("viewModel");

            UIElement view;
            var key = viewModel.GetType();
            if (_views.ContainsKey(key))
            {
                view = _views[key];
                _views.Remove(key);
            }
            else
            {
                view = ViewLocator.LocateForModel(viewModel, null, null);
            }
            if (view == null)
                throw new DeveloperException(string.Format("Can't find view for view model {0}", viewModel));

            ViewModelBinder.Bind(viewModel, view, null);

            return view;
        }

        void IViewService.Register(string command, Type viewModelOrCommandType)
        {
            throw new NotImplementedException();
        }

        void IViewService.Show(string command)
        {
            throw new NotImplementedException();
        }

        void IViewService.Show(string command, ShowContext context)
        {
            throw new NotImplementedException();
        }

        IViewModel IViewService.ResolveViewModel(string command)
        {
            throw new NotImplementedException();
        }

        bool IViewService.TryResolveViewModel(string command, out IViewModel viewModel)
        {
            throw new NotImplementedException();
        }

        IViewCommand IViewService.ResolveViewCommand(string command)
        {
            throw new NotImplementedException();
        }

        bool IViewService.TryResolveViewCommand(string command, out IViewCommand viewCommand)
        {
            throw new NotImplementedException();
        }

        void IViewService.Show(IViewModel viewModel)
        {
            throw new NotImplementedException();
        }

        void IViewService.Show(IViewModel viewModel, ShowContext context)
        {
            throw new NotImplementedException();
        }

        void IViewService.Show(IViewModel viewModel, ShowContext context, ref IViewModel currentViewModel)
        {
            throw new NotImplementedException();
        }

        void IViewService.Show(object viewModel, object id, object title)
        {
            throw new NotImplementedException();
        }

        public IView GetView(IViewModel viewModel)
        {
            return GetBindViewByViewModel(viewModel) as IView;
        }

        public void AddViewToCache(Type modelType, IView view)
        {
            var element = view as UIElement;
            if (element != null)
                _views[modelType] = element;
        }

        public void Show(IView view)
        {
            throw new NotImplementedException();
        }

        public void Show(IView view, ShowContext context)
        {
            throw new NotImplementedException();
        }

        public void Show(FrameworkElement content)
        {
            throw new NotImplementedException();
        }

        public void Show(FrameworkElement content, ShowContext context)
        {
            throw new NotImplementedException();
        }

        public bool? SaveLayout(IViewModel viewModel, string title)
        {
            throw new NotImplementedException();
        }

        public bool SaveLayout(FrameworkElement element, FormComponents formComponents)
        {
            throw new NotImplementedException();
        }

        public bool? SaveDBLayout(IViewModel viewModel, string title, bool upCriticalVersion = false)
        {
            throw new NotImplementedException();
        }

        public bool SaveDBLayout(FrameworkElement element, FormComponents formComponents, bool upCriticalVersion = false)
        {
            throw new NotImplementedException();
        }

        public void AllRestoreLayout()
        {
            throw new NotImplementedException();
        }

        public void RestoreLayout(FrameworkElement obj)
        {
            throw new NotImplementedException();
        }

        public bool? ClearLayout(IViewModel viewModel, string title, Type[] actionProcessingObjectTypes = null, Type[] doNotActionProcessingObjectTypes = null)
        {
            throw new NotImplementedException();
        }

        public bool ClearLayout(FrameworkElement obj, FormComponents formComponents, Type[] actionProcessingObjectTypes = null, Type[] doNotActionProcessingObjectTypes = null)
        {
            throw new NotImplementedException();
        }

        public bool Close(object viewOrViewModel, bool isDialog = false)
        {
            throw new NotImplementedException();
        }

        public bool CloseAll()
        {
            throw new NotImplementedException();
        }

        public bool CloseAll(bool absolutely)
        {
            throw new NotImplementedException();
        }

        public void MakeMaxSize(IViewModel viewModel, bool isMax = true)
        {
            throw new NotImplementedException();
        }

        public void ShowError(string message, Exception ex, string[] attachments,
            Dictionary<string, string> additionalParams,
            bool isBtnSandMailEnable = true, string mandantCode = null, bool isMandantListVisible = true)
        {
            ErrorBox.ShowError(message, ex, attachments, additionalParams, isBtnSandMailEnable, mandantCode);
        }

        public void ShowError(string message, Exception ex, bool isBtnSandMailEnable = true)
        {
            ErrorBox.ShowError(message, ex, isBtnSandMailEnable);
        }

        public Window GetActiveWindow()
        {
            return Application.Current.Windows.OfType<Window>().LastOrDefault(window => window.IsActive);
        }

        bool IViewService.TryGetWmsWebObject(string wmsobject, out string wmswebobject)
        {
            throw new NotImplementedException();
        }

        Task IViewService.GetWebEntityMappingAsync(string wmsentity)
        {
            throw new NotImplementedException();
        }
        #endregion . IViewService  .

        #region .  IHandle<>  .
        void IHandle<CloseRequestEvent>.Handle(CloseRequestEvent message)
        {
            throw new NotImplementedException();
        }

        public void Handle(ErrorEvent message)
        {
            throw new NotImplementedException();
        }
        #endregion .  IHandle<>  .
    }
}
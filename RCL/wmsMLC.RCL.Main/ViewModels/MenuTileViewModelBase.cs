using System;
using System.Threading.Tasks;
using System.Windows.Input;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Components.ViewModels;
using wmsMLC.General.PL.WPF.Helpers;

namespace wmsMLC.RCL.Main.ViewModels
{
    public abstract class MenuTileViewModelBase : RclViewModel
    {
        protected MenuTileViewModelBase()
        {
            MenuClickCommand = new DelegateCustomCommand<string>(this, OnMenuClickInternal, CanMenuClick);
        }

        #region .  Properties  .

        protected virtual TimeSpan MaxContentChangeInterval
        {
            get { return TimeSpan.FromDays(2); }
        }

        protected abstract Action DefaultCompletedActionHandler { get; }

        #endregion .  Properties  .

        #region . Commands .
        public ICommand MenuClickCommand { get; set; }

        protected virtual bool CanMenuClick(string parameter)
        {
            return !WaitIndicatorVisible;
        }

        private void OnMenuClickInternal(string parameter)
        {
            if (!CanMenuClick(parameter))
                return;

            OnMenuClick(parameter);
        }

        protected abstract void OnMenuClick(string parameter);

        #endregion . Commands .

        #region .  Methods  .
        #region .  Create Menu  .

        public abstract void InitializeMenu();

        protected virtual void DefaultCompletedHandler(CompleteContext context)
        {
            WaitIndicatorVisible = false;
            var handler = DefaultCompletedActionHandler;
            if (handler != null)
                DispatcherHelper.Invoke(handler);
        }

        #endregion .  Create Menu  .

        protected virtual Task RunBpAsync(string bpprocess, BpContext bpContext, Action<CompleteContext> completedHandler)
        {
            var managerInstance = IoC.Instance.Resolve<IBPProcessManager>();
            return Task.Factory.StartNew(() =>
            {
                System.Threading.Thread.Sleep(200);

                if (bpContext == null)
                    bpContext = new BpContext();

                //Больше не добавляем Да/нет
                //bpContext.Properties.Add("ConfirmKeyHelp", string.Format("{0}" + Resources.StringResources.ConfirmKeyHelp, Environment.NewLine));

                managerInstance.Parameters.Add(BpContext.BpContextArgumentName, bpContext);

                DispatcherHelper.Invoke(new Action(delegate
                {
                    try
                    {
                        managerInstance.Run(code: bpprocess, completedHandler: completedHandler);
                    }
                    catch (Exception ex)
                    {
                        if (completedHandler != null)
                            completedHandler(new CompleteContext(ex));
                        throw;
                    }
                }));
            });
        }

        protected virtual bool ExceptionHandler(Exception ex, string message)
        {
            if (ex == null)
                return true;

            var result = new OperationException(message, ex);
            return ExceptionPolicy.Instance.HandleException(result, "PL");
        }

        #endregion .  Methods  .
    }
}

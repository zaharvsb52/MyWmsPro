using System;
using System.Security.Authentication;
using System.Threading.Tasks;
using System.Windows;
using log4net;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Services;
using wmsMLC.General.PL.WPF.ViewModels;

namespace wmsMLC.DCL.Main.ViewModels
{
    public class AuthenticationViewModel : ViewModelBase, ICloseRequest, IAuthenticationViewModel
    {
        #region Fields & Consts

        private readonly ILog _log = LogManager.GetLogger(typeof (AuthenticationViewModel));

        private const int DefaultMaxAuthAttempts = 5;

        private int _authAttempts;
        private string _login;
        private string _password;
        private string _errorText;
        private bool _resultDialog;

        private readonly IAuthenticationProvider _authenticationProvider;
        private readonly IViewService _viewService;
        private bool _inWait;

        # endregion

        #region .  Constructor  .
        public AuthenticationViewModel(IAuthenticationProvider authenticationProvider, IViewService viewService)
        {
            _authenticationProvider = authenticationProvider;
            _viewService = viewService;

            _authAttempts = 0;
            ResultDialog = false;
            AuthenticateCommand = new DelegateCustomCommand(Authenticate, CanAuthenticate);
        }

        private bool CanAuthenticate()
        {
            return !InWait && !string.IsNullOrEmpty(Login);
        }
        private void RiseCommandsCanExecuteChanged()
        {
            AuthenticateCommand.RaiseCanExecuteChanged();
        }
        #endregion

        #region .  Properties  .

        public string Login
        {
            get { return _login; }
            set
            {
                if (_login == value)
                    return;

                _login = value;
                OnPropertyChanged("Login");
                RiseCommandsCanExecuteChanged();
            }
        }

        public bool ResultDialog
        {
            get { return _resultDialog; }
            set
            {
                if (_resultDialog == value)
                    return;

                _resultDialog = value;
                OnPropertyChanged("ResultDialog");
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                if (_password == value)
                    return;

                _password = value;
                OnPropertyChanged("Password");
            }
        }

        public string ErrorText
        {
            get { return _errorText; }
            set
            {
                _errorText = value;
                OnPropertyChanged("ErrorText");
            }
        }

        public bool InWait
        {
            get { return _inWait; }
            set
            {
                if (_inWait == value)
                    return;

                _inWait = value;
                OnPropertyChanged("InWait");
                RiseCommandsCanExecuteChanged();
            }
        }

        public ICustomCommand AuthenticateCommand { get; private set; }

        public event EventHandler CloseRequest;

        #endregion

        #region .  Methods  .
        private async void Authenticate()
        {
            try
            {
                InWait = true;

                var res = await AuthenticateAsync();
                if (res)
                {
                    ResultDialog = true;
                    DoCloseRequest();
                }
                _log.InfoFormat("User '{0}' successfully authenticated.", WMSEnvironment.Instance.AuthenticatedUser.GetSignature());
            }
            catch (AuthenticationException authException)
            {
                _log.WarnFormat("User '{0}' is not authenticated.", Login);
                ErrorText = authException.Message;
            }
            catch (Exception ex)
            {
                _log.Error("Unknown auth error.", ex);
                ErrorText = wmsMLC.General.Resources.ExceptionResources.Undefined;
            }
            finally
            {
                InWait = false;

                _authAttempts++;

                if (_authAttempts >= DefaultMaxAuthAttempts)
                {
                    DoCloseRequest();

                    _viewService.ShowDialog(StringResources.AuthenticationError
                        , string.Format(StringResources.ApplicationWillBeShutdown, Environment.NewLine)
                        , MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                }
            }
        }

        private async Task<bool> AuthenticateAsync()
        {
            return await Task.Factory.StartNew(() =>
                {
                    _authenticationProvider.Authenticate(Login, Password);
                    return _authenticationProvider.AuthenticatedUser != null;
                });
        }

        private void DoCloseRequest()
        {
            var h = CloseRequest;
            if (h != null)
                h(this, EventArgs.Empty);
        }
        #endregion
    }
}

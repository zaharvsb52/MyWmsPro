using System;
using System.Security.Authentication;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using log4net;
using wmsMLC.General;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Components.ViewModels;
using wmsMLC.General.PL.WPF.Services;
using wmsMLC.RCL.Resources;

namespace wmsMLC.RCL.Main.ViewModels
{
    public class AuthenticationViewModel : RclViewModel, ICloseRequest, IAuthenticationViewModel
    {
        #region Fields & Consts

        public const int DefaultMaxAuthAttempts = 5;

        private readonly ILog _log = LogManager.GetLogger(typeof(AuthenticationViewModel));
        private int _authAttempts;
        private string _login;
        private string _password;
        private string _errorText;
        private bool _resultDialog;
        private ICryptoKeyProvider _cryptoKeyProvider;

        private readonly IAuthenticationProvider _authenticationProvider;
        private readonly IViewService _viewService;

        #endregion  Fields & Consts

        #region .  Constructor  .

        public AuthenticationViewModel(IAuthenticationProvider authenticationProvider, IViewService viewService)
        {
            _authenticationProvider = authenticationProvider;
            _viewService = viewService;

            _authAttempts = 0;
            ResultDialog = false;
            OnCommand = new DelegateCustomCommand(Authenticate, CanAuthenticate);
        }

        #endregion .  Constructor  .

        #region .  Properties  .

        public string Login
        {
            get { return _login; }
            set
            {
                string login;
                _login = ParseBarcode(value, 0, out login) ? login : value;
                OnPropertyChanged("Login");
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
                string password;
                _password = ParseBarcode(value, 1, out password) ? password : value;
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

        public ICommand OnCommand { get; private set; }

        public event EventHandler CloseRequest;
        public event EventHandler DataError;

        #endregion .  Properties  .

        #region .  Methods  .

        private bool CanAuthenticate()
        {
            return !WaitIndicatorVisible;
        }

        private async void Authenticate()
        {
            if (!CanAuthenticate())
                return;

            try
            {
                WaitIndicatorVisible = true;

                var res = await AuthenticateAsync();
                if (res)
                {
                    ResultDialog = true;
                    DoCloseRequest();
                }
            }
            catch (AuthenticationException authException)
            {
                _log.WarnFormat("User '{0}' is not authenticated.", Login);
                ErrorText = authException.Message;
                //Очистка пароля при ошибке идентификации пользователя. Не работает если конфиг RCL настроен на SDCL_Endpoint
                Password = null;
                var handler = DataError;
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                _log.Error("Unknown auth error.", ex);
                ErrorText = wmsMLC.General.Resources.ExceptionResources.Undefined;
            }
            finally
            {
                WaitIndicatorVisible = false;

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

        private bool ParseBarcode(string code, int index, out string value)
        {
            value = null;
            if (string.IsNullOrEmpty(code))
                return false;

            if (_cryptoKeyProvider == null)
                _cryptoKeyProvider = IoC.Instance.Resolve<ICryptoKeyProvider>();

            var descr = _cryptoKeyProvider.GetKey(index);
            var txt = CryptoHelper.Decrypt(code, descr);
            if (!string.IsNullOrEmpty(txt))
            {
                value = txt;
                return true;
            }
            return false;
        }

        #endregion .  Methods  .
    }
}
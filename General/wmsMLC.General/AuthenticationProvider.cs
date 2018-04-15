using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Security;
using System.Security.Authentication;

namespace wmsMLC.General
{
    public sealed class AuthenticationProvider : IAuthenticationProvider
    {
        #region .  Fields&Consts  .

        private class AuthenticatedUserClass : IUserInfo
        {
            private readonly ICredentials _credential;
            private readonly string _userId;

            internal AuthenticatedUserClass(string userId, ICredentials credentials)
            {
                _userId = userId;
                _credential = credentials;
            }

            public string GetSignature()
            {
                return _userId;
            }

            public ICredentials GetCredentials()
            {
                return _credential;
            }
        }

        private readonly IAuthService _authService;
        private IUserInfo _authenticatedUser;

        public IUserInfo AuthenticatedUser
        {
            get { return _authenticatedUser; }
            private set
            {
                OnChanging(value);
                _authenticatedUser = value;
                OnChanged();
            }
        }

        public bool InAuthentication { get; private set; }

        public AuthenticationProvider(IAuthService authService)
        {
            Contract.Requires(authService != null);
            _authService = authService;
        }

        #endregion

        #region .  Methods  .
        public event EventHandler<AuthenticatedUserChangingEventArgs> AuthenticatedUserChanging;
        public event EventHandler AuthenticatedUserChanged;

        public void LogOff()
        {
            AuthenticatedUser = null;
            _authService.LogOff();
        }

        public void Authenticate(string login, string password)
        {
            try
            {
                InAuthentication = true;

                string userCode;
                if (!_authService.Authenticate(login, password, out userCode))
                    throw new AuthenticationException("Неверные имя пользователя или пароль.");

                // упаковываем пароль
                var secureStr = new SecureString();
                password.ToCharArray().ToList().ForEach(secureStr.AppendChar);
                var credential = new NetworkCredential(login, secureStr);
                AuthenticatedUser = new AuthenticatedUserClass(userCode, credential);
            }
            finally
            {
                InAuthentication = false;
            }
        }

        private void OnChanging(IUserInfo newValue)
        {
            var h = AuthenticatedUserChanging;
            if (h != null)
                h(this, new AuthenticatedUserChangingEventArgs(newValue));
        }
        private void OnChanged()
        {
            var h = AuthenticatedUserChanged;
            if (h != null)
                h(this, EventArgs.Empty);
        }
        #endregion
    }

    public class AuthenticatedUserChangingEventArgs : EventArgs
    {
        public AuthenticatedUserChangingEventArgs(IUserInfo newValue)
        {
            NewValue = newValue;
        }

        public IUserInfo NewValue { get; private set; }
    }
}
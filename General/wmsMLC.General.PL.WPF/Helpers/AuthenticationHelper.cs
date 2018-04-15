using System;
using wmsMLC.General.PL.WPF.Services;
using wmsMLC.General.PL.WPF.ViewModels;

namespace wmsMLC.General.PL.WPF.Helpers
{
    /// <summary>
    /// Используем этот класс везде, где нужна аутентификация
    /// </summary>
    public static class AuthenticationHelper
    {
        #region .  Settings  .
        public static bool AskCredentials = true;
        #endregion

        private static readonly Lazy<IAuthenticationProvider> AuthenticationProvider = new Lazy<IAuthenticationProvider>(() => IoC.Instance.Resolve<IAuthenticationProvider>());

        public static bool IsAuthenticated
        {
            get { return AuthenticationProvider.Value.AuthenticatedUser != null; }
        }

        public static bool Authenticate(IntPtr? intPtrOwner = null)
        {
            if (AskCredentials)
            {
                var authVm = IoC.Instance.Resolve<IAuthenticationViewModel>();
                authVm.Login = GetCurrentUserLogin(AuthenticationProvider.Value);

                var viewService = IoC.Instance.Resolve<IViewService>();
                viewService.ShowDialogWindow((IViewModel) authVm, true, intPtrOwner: intPtrOwner);
                return IsAuthenticated;
            }
            return false;
        }

        public static bool Authenticate(string login, string password)
        {
            AuthenticationProvider.Value.Authenticate(login, password);
            return IsAuthenticated;
        }

        public static void LogOff()
        {
            AuthenticationProvider.Value.LogOff();
        }

        private static string GetCurrentUserLogin(IAuthenticationProvider provider)
        {
            // если пользоваетль еще не зарегистрировался - подствляем имя из системы
            if (!IsAuthenticated)
                return Environment.UserName;

            if (provider == null) throw new ArgumentNullException("provider");

            if (provider.AuthenticatedUser == null)
                return null;

            // получаем текущего пользователя. если он из домена - убираем домен
            var signature = provider.AuthenticatedUser.GetSignature();
            var idx = signature.IndexOf("\\", StringComparison.Ordinal);
            if (idx > 0)
                signature = signature.Substring(idx + 1);

            return signature;
        }
    }
}
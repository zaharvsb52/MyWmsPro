using System;
using System.Net;

namespace wmsMLC.General
{
    /// <summary> Сервис аутентификации </summary>
    public interface IAuthenticationProvider
    {
        /// <summary> Аутентификация с использованием введенных данных </summary>
        void Authenticate(string login, string password);

        void LogOff();

        /// <summary> Информация о зарегистрированном пользователе </summary>
        IUserInfo AuthenticatedUser { get; }

        bool InAuthentication { get; }

        event EventHandler<AuthenticatedUserChangingEventArgs> AuthenticatedUserChanging;
        event EventHandler AuthenticatedUserChanged;
    }

    public interface IUserInfo
    {
        string GetSignature();
        ICredentials GetCredentials();
    }
}

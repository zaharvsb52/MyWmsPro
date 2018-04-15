using System;

namespace wmsMLC.General.Types
{
    /// <summary>
    /// Событие или ошибка
    /// </summary>
    /// <param name="ex">исключительная ситуация</param>
    /// <param name="errorType">тип ошибки</param>
    /// <param name="errorCode">код ошибки</param>
    /// <param name="message">параметры кода ошибки</param>
    /// <param name="businessProcess">название бизнеспроцесса</param>
    /// <param name="linkById">привязка по Id</param>
    /// <param name="hostName">имя хоста</param>
    /// <param name="userName">имя пользователя</param>
    public delegate void EventErrorHandler(Exception ex, ErrorType errorType, string errorCode, string[] message, string businessProcess, string linkById, string hostName = "", string userName = "");
}

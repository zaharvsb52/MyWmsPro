using System;
using System.Linq;
using log4net;
#pragma warning disable 1587
/// --------------------------------------------------------------------------------------
/// <Copyright File="wmsBaseObject.cs" Company="ЗАО Логистическая компания МОЛКОМ">
///   Copyright (c) ЗАО Логистическая компания МОЛКОМ. All rights reserved.
/// </Copyright>
/// <Author>Vladimir G. Nosov</Author>
/// <Date>24.10.2012 9:25:42</Date>
/// <Summary>Базовый объект для любой сущности системы
/// Содержит стандартные необходимые параметры</Summary>
/// --------------------------------------------------------------------------------------
#pragma warning restore 1587
using wmsMLC.General.Services;

namespace wmsMLC.General.Types
{
    /// <summary>
    /// Базовый объект для любой сущности системы
    /// Содержит стандартные необходимые параметры
    /// </summary>
    public class BaseObject
    {
        protected ILog Log = LogManager.GetLogger(typeof(BaseObject));

        /// <summary>
        /// Событие об ошибки
        /// </summary>
        public EventErrorHandler OnEventErrorHandler;

        /// <summary>
        /// Событие новой телеграммы для отправки
        /// </summary>
        //public TelegramEventHandler OnTelegramEventHandler;

        /// <summary>
        /// Событие на которое подписывается сущность системы
        /// </summary>
        public string FeedBack;

        /// <summary>
        /// Связь с какой-либо сущностью системы по ее коду
        /// </summary>
        public string LinkById;

        /// <summary>
        /// логин пользователя в системе [Login]
        /// </summary>
        public string UserName;

        /// <summary>
        /// Имя пользовательского хоста [Host] (DNS имя, IP допустимо) (в том числе и терминальное оборудование)
        /// </summary>
        public string HostName { get; set; }

        #region .  Process Messages  .
        protected void ProcessInfo(string errorCode,
                           string[] message = null,
                           string businessProcess = "")
        {
            ProcessMessage(null, ErrorType.INFO, errorCode, message ?? new[] { "" }, businessProcess, LinkById);
        }

        protected void ProcessException(Exception ex,
                                        string errorCode = "EUnknown",
                                        string[] message = null,
                                        string businessProcess = "")
        {
            ProcessMessage(ex, ErrorType.ERROR, errorCode, message ?? new[] { string.Empty }, businessProcess, LinkById);
        }

        protected void ProcessMessage(Exception ex,
            ErrorType errorType,
            string errorCode,
            string[] message,
            string businessProcess,
            string linkById,
            string hostName = "",
            string userName = "")
        {
            switch (errorType)
            {
                case ErrorType.ERROR:
                    Log.Error(ex.Message);
                    if (Log.IsDebugEnabled)
                    {
                        Log.DebugFormat("Fire error with code {0} in process {1}. Parameters {2}", errorCode, businessProcess, string.Join(",", message));
                        Log.Debug(ex);
                    }
                    break;

                case ErrorType.INFO:
                    if (Log.IsInfoEnabled)
                    {
                        var ms = string.Join(",", message);
                        Log.Info(ms);

                        if (Log.IsDebugEnabled)
                            Log.DebugFormat("Fire info message with code {0} in process {1}. Parameters {2}", errorCode, businessProcess, ms);
                    }
                    break;

                default:
                    if (Log.IsDebugEnabled)
                        Log.DebugFormat("Fire unknown message with code {0} in process {1}. Parameters {2}", errorCode, businessProcess, string.Join(",", message));
                    break;
            }

            if (OnEventErrorHandler != null)
                OnEventErrorHandler(ex, errorType, errorCode, message, businessProcess, linkById, hostName, userName);
        }
        #endregion

    }
}

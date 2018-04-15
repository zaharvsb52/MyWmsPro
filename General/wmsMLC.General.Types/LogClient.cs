#pragma warning disable 1587
/// --------------------------------------------------------------------------------------
/// <Copyright File="wmsLogClient.cs" Company="ЗАО Логистическая компания МОЛКОМ">
///   Copyright (c) ЗАО Логистическая компания МОЛКОМ. All rights reserved.
/// </Copyright>
/// <Author>Vladimir G. Nosov</Author>
/// <Date>27.08.2012 10:27:51</Date>
/// <Summary>клиент для службы логирования
/// Методы, заканчивающиеся на _WF, предназначены для wmsWorkFlowEngine 
/// Исключительные ситуации контролируются wmsWorkFlowEngine
/// </Summary>
/// --------------------------------------------------------------------------------------
#pragma warning restore 1587

using System;
using System.Diagnostics;
using log4net;
using wmsMLC.General.Services;
using wmsMLC.General.Types;

namespace wmsMLC.General.wmsLogClient
{
    /// <summary>
    /// клиент для службы логирования
    /// </summary>
    public class LogClient
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(LogClient));
        /// <summary>
        /// Синглтон объекта
        /// </summary>
        private static LogClient _instance;
        public static LogClient Logger
        {
            get { return _instance ?? (_instance = new LogClient()); }
        }

        /// <summary>
        /// уровень отладки
        /// </summary>
        private DebugLevel _debugLevel = DebugLevel.Level1;

        private string _clientName;

        private string _appPath = "";

        /// <summary>
        /// Созадние канала привязки через netTcpBinding
        ///  и регистрация в службе логирования
        /// </summary>
        /// <param name="serviceEndPoint"> строковое имя конечной точки подключения </param>
        /// <param name="appPath"></param>
        /// <param name="clientName"> имя регистрируемого клиента в логе </param>
        public void Initialize(string serviceEndPoint, string appPath, string clientName)
        {
            _clientName = clientName;
            _appPath = appPath;
        }

        /// <summary>
        /// Установка уровня отладки.
        /// </summary>
        /// <param name="level">уровень отладки</param>
        /// <returns>возращает true в случае успеха</returns>
        public bool SetDebugLevel(DebugLevel level)
        {
            _debugLevel = level;
            return true;
        }

        /// <summary>
        /// Событие логирования сообщения или ошибки
        /// </summary>
        /// <param name="ex">исключительная ситуация</param>
        /// <param name="errorType">тип ошибки</param>
        /// <param name="errorCode">код ошибки или сообщения</param>
        /// <param name="message">параметры сообщения</param>
        /// <param name="businessProcess">имя бизнес процесса</param>
        /// <param name="linkById">Связь с какой-либо сущностью системы по ее коду</param>
        public void OnEventErrorHandler(Exception ex, ErrorType errorType, string errorCode, string[] message, string businessProcess, string linkById)
        {
            Log(ex, errorType, errorCode, message, businessProcess, linkById, Environment.MachineName, Environment.UserName);
        }

        /// <summary>
        /// Событие логирования сообщения или ошибки
        /// </summary>
        /// <param name="ex">исключительная ситуация</param>
        /// <param name="errorType">тип ошибки</param>
        /// <param name="errorCode">код ошибки или сообщения</param>
        /// <param name="message">параметры сообщения</param>
        /// <param name="businessProcess">имя бизнес процесса</param>
        /// <param name="linkById">Связь с какой-либо сущностью системы по ее коду</param>
        /// <param name="hostName"></param>
        /// <param name="userName"></param>
        public void OnEventErrorHandler(Exception ex, ErrorType errorType, string errorCode, string[] message, string businessProcess, string linkById, string hostName, string userName)
        {
            string tmpHostName = (hostName != null) ? ((hostName.Length > 0) ? hostName : Environment.MachineName) : Environment.MachineName;
            string tmpUserName = (userName != null) ? ((userName.Length > 0) ? userName : Environment.UserName) : Environment.UserName;
            Log(ex, errorType, errorCode, message, businessProcess, linkById, tmpHostName, tmpUserName);
        }

        private void Log(Exception ex, ErrorType errorType, string errorCode, string[] message, string businessProcess, string linkById, string hostName, string userName)
        {
            try
            {
                LogMessage msg = ex != null
                                     ? Types.LogMessage.BuildMessage(ex, errorType, errorCode, message,
                                                                     businessProcess, linkById, hostName,
                                                                     userName)
                                     : Types.LogMessage.BuildMessage(new StackTrace(2, true), errorType,
                                                                     errorCode, message, businessProcess,
                                                                     linkById, hostName, userName);
                _log.Debug(msg.FormatMessage("#"));
            }
            catch (Exception e)
            {
                ErrorOutput(e);
            }
        }

        /// <summary>
        /// Вывод ошибки в консоль и в файл с именем сервиса
        /// </summary>
        /// <param name="e"></param>
        private void ErrorOutput(Exception e)
        {
            Console.WriteLine(e.Message);
            try
            {
                if (_log.IsErrorEnabled)
                    _log.ErrorFormat("Exception: {0}", e.Message);

                if (_log.IsDebugEnabled)
                    _log.Debug(e);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}

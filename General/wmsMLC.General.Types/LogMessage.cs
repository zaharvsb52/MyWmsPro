#pragma warning disable 1587
/// --------------------------------------------------------------------------------------
/// <Copyright File="wmsLogMessage.cs" Company="ЗАО Логистическая компания МОЛКОМ">
///   Copyright (c) ЗАО Логистическая компания МОЛКОМ. All rights reserved.
/// </Copyright>
/// <Author>Vladimir G. Nosov</Author>
/// <Date>27.07.2012 10:55:56</Date>
/// <Summary>Тип передаваемого сообщения в лог файл
/// Методы, заканчивающиеся на _WF, предназначены для wmsWorkFlowEngine 
/// Исключительные ситуации контролируются wmsWorkFlowEngine
/// </Summary>
/// --------------------------------------------------------------------------------------
#pragma warning restore 1587

using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using wmsMLC.General.Services;

namespace wmsMLC.General.Types
{
    /// <summary>
    /// Тип передаваемого сообщения в лог файл
    /// </summary>
    public class LogMessage
    {
        public LogMessage()
        {
            LogMessageLevel = LogMessageLevel.Level5;
            try
            {
                EventDate = DateTime.Now;
            }
            catch (Exception ex)
            {
                throw new Exception("EventDate", ex);
            }
        }

        public LogMessage(string message)
        {
            LogMessageLevel = LogMessageLevel.Level5;
            try
            {
                EventDate = DateTime.Now;
                EventMessage = message;
            }
            catch (Exception ex)
            {
                throw new Exception("EventDate", ex);
            }
        }

        /// <summary>
        /// Связь с какой-либо сущностью системы по ее коду
        /// </summary>
        public string LinkById;

        /// <summary>
        /// Дата и время события
        /// </summary>
        public DateTime EventDate;

        /// <summary>
        /// Имя модуля, который прислал сообщение
        /// </summary>
        public string Module;

        /// <summary>
        /// Версия модуля, который прислал сообщение
        /// </summary>
        public string ModuleVersion;

        /// <summary>
        /// Имя пользовательского хоста [Host] (DNS имя, IP допустимо) (в том числе и терминальное оборудование)
        /// </summary>
        public string HostName;

        /// <summary>
        /// Тип сообщения, определяющий значимость сообщения
        /// ERROR|INFO
        /// </summary>
        public ErrorType ErrorType;

        /// <summary>
        /// Объект кода в модуле, который прислал сообщение
        /// </summary>
        public string CodeObjectName;

        /// <summary>
        /// логин пользователя в системе [Login]
        /// </summary>
        public string UserName;

        /// <summary>
        /// В результате какого процесса (название) появилась данная запись
        /// </summary>
        public string Bp;

        /// <summary>
        /// Уникальный номер, генерируемый при очередном запуске бизнес-процесса
        /// </summary>
        public string BpUniqueNum;

        /// <summary>
        /// Код ошибки
        /// </summary>
        public string ErrorCode;

        /// <summary>
        /// Список параметров кода ошибки
        /// </summary>
        public string[] ErrorCodeArgs;

        /// <summary>
        /// Сообщение события или ошибки
        /// </summary>
        public string EventMessage;

        /// <summary>
        /// Объект исключения
        /// </summary>
        public Exception ExceptionClass;

        /// <summary>
        /// Стек исключения
        /// </summary>
        public string StackTrace;

        /// <summary>
        /// Статус события
        /// </summary>
        public string EventStatus;

        /// <summary>
        /// Имя ПК или сервера, где работает модуль
        /// </summary>
        public string SysHostName;

        /// <summary>
        /// имя учетной записи, под которой запущены сервис, инициировавший запись
        /// </summary>
        public string SysUserName;

        /// <summary>
        /// Дата в формате YYYY.MM.DD
        /// </summary>
        public string DateString
        {
            get
            {
                return string.Format("{0,4:0000}.{1,2:00}.{2,2:00}", EventDate.Year, EventDate.Month, EventDate.Day);
            }
        }

        /// <summary>
        /// Время в формате HH:MM:SS.SSS
        /// </summary>
        public string TimeString
        {
            get
            {
                return string.Format("{0,2:00}:{1,2:00}:{2,2:00}.{3,3:000}", EventDate.Hour, EventDate.Minute, EventDate.Second, EventDate.Millisecond);
            }
        }

        public string OutMessage
        {
            get
            {
                return (((ErrorType == ErrorType.ERROR) || (ErrorType == ErrorType.LocalERROR)) ? string.Format("{0}:{1}", EventMessage, StackTrace) : EventMessage);
            }
        }

        /// <summary>
        /// Сборка лог-сообщения
        /// </summary>
        /// <param name="stackTrace">трассировка стека программы</param>
        /// <param name="errorType">тип сообщения</param>
        /// <param name="errorCode">код сообщения или ошибки</param>
        /// <param name="errorCodeArgs">аргументы сообщения</param>
        /// <param name="businessProcess">имя бизнес процесса</param>
        /// <param name="linkById"></param>
        /// <param name="hostName"></param>
        /// <param name="userName"></param>
        /// <returns>заполненное логируемое сообщение</returns>
        public static LogMessage BuildMessage(StackTrace stackTrace, ErrorType errorType, string errorCode, string[] errorCodeArgs, string businessProcess, string linkById, string hostName, string userName)
        {
            LogMessage msg = Build(stackTrace, errorType, errorCode, errorCodeArgs, businessProcess, linkById);
            msg.UserName = userName;
            msg.HostName = hostName;
            return msg;
        }

        /// <summary>
        /// Сборка лог-сообщения
        /// </summary>
        /// <param name="stackTrace">трассировка стека программы</param>
        /// <param name="errorType">тип сообщения</param>
        /// <param name="errorCode">код сообщения или ошибки</param>
        /// <param name="errorCodeArgs">аргументы сообщения</param>
        /// <param name="businessProcess">имя бизнес процесса</param>
        /// <param name="linkById"></param>
        /// <returns>заполненное логируемое сообщение</returns>
        public static LogMessage BuildMessage(StackTrace stackTrace, ErrorType errorType, string errorCode, string[] errorCodeArgs, string businessProcess, string linkById)
        {
            LogMessage msg = Build(stackTrace, errorType, errorCode, errorCodeArgs, businessProcess, linkById);
            return msg;
        }

        private static LogMessage Build(StackTrace stackTrace, ErrorType errorType, string errorCode,
                                        string[] errorCodeArgs, string businessProcess, string linkById)
        {
            var declaringType = stackTrace.GetFrame(0).GetMethod().DeclaringType;

            var assembly = System.Reflection.Assembly.GetEntryAssembly();
            // На случай, если assembly==null
            string myModule = "UnknownModule";
            string myModuleVersion = "Unknown";
            if (assembly != null)
            {
                myModule = assembly.GetName().Name;
                myModuleVersion = assembly.GetName().Version.ToString();
            }
            var msg = new LogMessage
                {
                    //имя сервиса или ПО, откуда пришел запрос на добавление данных в лог
                    Module = myModule,
                    //версия приложения/сервиса
                    ModuleVersion = myModuleVersion,
                    //Имя ПК пользователя
                    HostName = Environment.MachineName,
                    //Имя системного ПК или сервера, где работает модуль
                    SysHostName = Environment.MachineName,
                    //Имя пользователя
                    UserName = Environment.UserName,
                    //Имя системного пользователя
                    SysUserName = Environment.UserName,
                    //тип, определяющий значимость сообщения и его направленность
                    //- «INFO» для обычных сообщений
                    //- «ERROR» для сообщений об ошибках
                    ErrorType = errorType,
                    //детализация, относящаяся к полю "сервис", например, имя функции или алгоритма                    
                    CodeObjectName = string.Format("{0}.{1}", (declaringType != null) ? declaringType.Name : string.Empty, stackTrace.GetFrame(0).GetMethod().Name),
                    //код ошибки
                    ErrorCode = errorCode,
                    //аргументы когда ошибок
                    ErrorCodeArgs = errorCodeArgs,
                    //сообщение или значение ошибки
                    EventMessage = errorCode,
                    //трассировка стека
                    //StackTrace = stackTrace.GetFrame(0).ToString(),
                    StackTrace = stackTrace.ToString(),
                    //статус записи
                    EventStatus = "ES_NEW",
                    //имя бизнес процесса
                    Bp = businessProcess,
                    //уникальный номер БП
                    BpUniqueNum = Thread.CurrentThread.ManagedThreadId.ToString(CultureInfo.InvariantCulture),
                    //Связь с какой-либо сущностью системы по ее коду
                    LinkById = linkById
                };
            return msg;
        }

        /// <summary>
        /// Сборка лог-сообщения
        /// </summary>
        /// <param name="ex">Exception, которое пишется в лог</param> А.С.Ш.
        /// <param name="errorType">тип сообщения</param>
        /// <param name="errorCode">код сообщения или ошибки</param>
        /// <param name="errorCodeArgs">аргументы сообщения</param>
        /// <param name="businessProcess">имя бизнес процесса</param>
        /// <param name="linkById"></param>
        /// <param name="hostName"></param>
        /// <param name="userName"></param>
        /// <returns>заполненное логируемое сообщение</returns>
        public static LogMessage BuildMessage(Exception ex, ErrorType errorType, string errorCode, string[] errorCodeArgs, string businessProcess, string linkById, string hostName, string userName)
        {
            LogMessage msg = BuildEx(ex, errorType, errorCode, errorCodeArgs, businessProcess, linkById);
            msg.UserName = userName;
            msg.HostName = hostName;
            return msg;
        }

        /// <summary>
        /// Сборка лог-сообщения
        /// </summary>
        /// <param name="ex">Exception, которое пишется в лог</param> А.С.Ш.
        /// <param name="errorType">тип сообщения</param>
        /// <param name="errorCode">код сообщения или ошибки</param>
        /// <param name="errorCodeArgs">аргументы сообщения</param>
        /// <param name="businessProcess">имя бизнес процесса</param>
        /// <param name="linkById"></param>
        /// <returns>заполненное логируемое сообщение</returns>
        public static LogMessage BuildMessage(Exception ex, ErrorType errorType, string errorCode, string[] errorCodeArgs, string businessProcess, string linkById)
        {
            LogMessage msg = BuildEx(ex, errorType, errorCode, errorCodeArgs, businessProcess, linkById);
            return msg;
        }

        private static LogMessage BuildEx(Exception ex, ErrorType errorType, string errorCode, string[] errorCodeArgs, string businessProcess, string linkById)
        {
            var assembly = System.Reflection.Assembly.GetEntryAssembly();
            // На случай, если assembly==null
            string myModule = "UnknownModule";
            string myModuleVersion = "Unknown";
            if (assembly != null)
            {
                myModule = assembly.GetName().Name;
                myModuleVersion = assembly.GetName().Version.ToString();
            }
            var msg = new LogMessage
                {
                    //имя сервиса или ПО, откуда пришел запрос на добавление данных в лог
                    Module = myModule,
                    //версия приложения/сервиса
                    ModuleVersion = myModuleVersion,
                    //Имя ПК пользователя
                    HostName = Environment.MachineName,
                    //Имя системного ПК или сервера, где работает модуль
                    SysHostName = Environment.MachineName,
                    //Имя пользователя
                    UserName = Environment.UserName,
                    //Имя системного пользователя
                    SysUserName = Environment.UserName,
                    //тип, определяющий значимость сообщения и его направленность
                    //- «INFO» для обычных сообщений
                    //- «ERROR» для сообщений об ошибках
                    ErrorType = errorType,
                    //детализация, относящаяся к полю "сервис", например, имя функции или алгоритма
                    CodeObjectName = string.Format("{0}.{1}", ex.Source, ex.TargetSite),
                    //код ошибки
                    ErrorCode = errorCode,
                    //аргументы когда ошибок
                    ErrorCodeArgs = errorCodeArgs,
                    //сообщение или значение ошибки
                    EventMessage = errorCode,
                    //трассировка стека
                    //StackTrace = stackTrace.GetFrame(0).ToString(),
                    StackTrace = string.Format("Type: {0} Message: {1} \r\n {2} StackTrace: {3}", ex.GetType(), ex.Message, (ex.InnerException != null) ? ex.InnerException.Message : "", ex.StackTrace),
                    //статус записи
                    EventStatus = "ES_NEW",
                    //имя бизнес процесса
                    Bp = businessProcess,
                    //уникальный номер БП
                    BpUniqueNum = Thread.CurrentThread.ManagedThreadId.ToString(CultureInfo.InvariantCulture),
                    //Связь с какой-либо сущностью системы по ее коду
                    LinkById = linkById
                };
            return msg;
        }

        /// <summary>
        /// Сборка лог-сообщения
        /// </summary>       
        /// <param name="errorType">тип сообщения</param>
        /// <param name="errorCode">код сообщения или ошибки</param>
        /// <param name="errorCodeArgs">аргументы сообщения</param>
        /// <param name="businessProcess">имя бизнес процесса</param>
        /// <param name="linkById"></param>        
        public void BuildMessage_WF(object errorType, object errorCode, object[] errorCodeArgs, object businessProcess, object linkById)
        {
            var stackTrace = new StackTrace(1, true);
            //имя сервиса или ПО, откуда пришел запрос на добавление данных в лог
            Module = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
            //версия приложения/сервиса
            ModuleVersion = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
            //Имя ПК пользователя
            HostName = Environment.MachineName;
            //Имя системного ПК или сервера, где работает модуль
            SysHostName = Environment.MachineName;
            //Имя пользователя
            UserName = Environment.UserName;
            //Имя системного пользователя
            SysUserName = Environment.UserName;
            //тип, определяющий значимость сообщения и его направленность
            //- «INFO» для обычных сообщений
            //- «ERROR» для сообщений об ошибках
            ErrorType = (ErrorType)Enum.Parse(typeof(ErrorType), errorType.ToString());
            //детализация, относящаяся к полю "сервис", например, имя функции или алгоритма
            var declaringType = stackTrace.GetFrame(0).GetMethod().DeclaringType;
            if (declaringType != null)
                CodeObjectName = string.Format("{0}.{1}", declaringType.Name, stackTrace.GetFrame(0).GetMethod().Name);
            //код ошибки
            ErrorCode = errorCode.ToString();
            //аргументы когда ошибок
            ErrorCodeArgs = Array.ConvertAll(errorCodeArgs, p => (p ?? String.Empty).ToString());
            //сообщение или значение ошибки
            EventMessage = errorCode.ToString();
            //трассировка стека
            //StackTrace = stackTrace.GetFrame(0).ToString(),
            StackTrace = stackTrace.ToString();
            //статус записи
            EventStatus = "ES_NEW";
            //имя бизнес процесса
            Bp = businessProcess.ToString();
            //уникальный номер БП
            BpUniqueNum = Thread.CurrentThread.ManagedThreadId.ToString(CultureInfo.InvariantCulture);
            //Связь с какой-либо сущностью системы по ее коду
            LinkById = linkById.ToString();
        }

        /// <summary>
        /// формирование сообщения с разделителем
        /// </summary>
        /// <param name="splitter">символ разделителя</param>
        /// <returns>сформированное сообщение</returns>
        public string FormatMessage(string splitter)
        {
            //return string.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}{0}{9}{0}{10}{0}{11}{0}{12}{0}{13}{0}{14}{0}",
            //           splitter, DateString, TimeString, Module, ModuleVersion, SysHostName, HostName, SysUserName, UserName, ErrorType, CodeObjectName, Bp, BpUniqueNum, LinkById, OutMessage);
            return string.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}",
                       splitter, ErrorType, CodeObjectName, Bp, BpUniqueNum, LinkById, OutMessage);
        }

        public LogMessageLevel LogMessageLevel { get; set; }
    }
}

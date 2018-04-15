#pragma warning disable 1587
/// --------------------------------------------------------------------------------------
/// <Copyright File="wmsTelegram.cs" Company="ЗАО Логистическая компания МОЛКОМ">
///   Copyright (c) ЗАО Логистическая компания МОЛКОМ. All rights reserved.
/// </Copyright>
/// <Author>Vladimir G. Nosov</Author>
/// <Date>07.11.2012 16:40:23</Date>
/// <Summary>Телеграмма для передачи и обмена данными</Summary>
/// --------------------------------------------------------------------------------------
#pragma warning restore 1587
using System;
using System.Xml.Serialization;
using ProtoBuf;

namespace wmsMLC.General.Services
{
    /// <summary>
    /// Перечисление типа телеграммы - комманда или ...
    /// </summary>
    public enum TelegramBodyType
    {
        Cmd = 0, Wms = 1
    }

    /// <summary>
    /// Перечисление типов телеграмм - запрос и ответ
    /// </summary>
    public enum TelegramType
    {
        /// <summary>
        /// Запрос.
        /// </summary>
        Query,

        /// <summary>
        /// Запрос, которому не нужен ответ.
        /// </summary>
        QueryNoAnswer,

        /// <summary>
        /// Ответ.
        /// </summary>
        Answer,
    }

    /// <summary>
    /// Системные 
    /// </summary>
    public enum TelegramSysCommand
    {
        /// <summary>
        /// Перезагрузить конфигурацию
        /// </summary>
        ReloadConfig,
        /// <summary>
        /// Подтвердить получение
        /// </summary>
        Ack,
        /// <summary>
        /// Приостановить работу
        /// </summary>
        Pause,
        /// <summary>
        /// Возобновить работу
        /// </summary>
        Resume,
        /// <summary>
        /// Остановить сервис
        /// </summary>
        Stop,
        /// <summary>
        /// Немедленно остановить сервис
        /// </summary>
        Kill,
        /// <summary>
        /// Подключить новый сервис
        /// </summary>
        Connect,
        /// <summary>
        /// Системное сообщение
        /// </summary>
        Message
    }

    /// <summary>
    /// Идентификатор телеграммы
    /// </summary>
    [Serializable]
    [ProtoContract]
    public struct ToId
    {
        /// <summary>
        /// ID сервиса (вкомпилен)
        /// </summary>
        [XmlElement("SID")]
        [ProtoMember(1)]
        public string ServiceId;

        /// <summary>
        /// ID сессии (выдаётся каждый раз при подключении)
        /// </summary>
        [XmlElement("Session")]
        [ProtoMember(2)]
        public int SessionId;

        /// <summary>
        /// Клиентская сессия.
        /// </summary>
        [XmlElement("ClientSessionId")]
        [ProtoMember(3)]
        public decimal? ClientSessionId;

        override public string ToString()
        {
            return string.Format("{0}:{1}", ServiceId, SessionId);
        }

        public string Key
        {
            get
            {
                return string.Format("{0}_{1}", ServiceId, SessionId);
            }
        }
    }

    /// <summary>
    /// Телеграмма для передачи и обмена данными
    /// </summary>
    [XmlRoot("Telegram")]
    [Serializable]
    [ProtoContract]
    public class Telegram
    {
        /// <summary>
        /// Дата и время создания телеграммы
        /// </summary>
        [XmlElement("Date")]
        [ProtoMember(1)]
        public DateTime Date;

        /// <summary>
        /// Идентификатор транзакции (если мы захотим объединить обработку нескольких телеграмм)
        /// </summary>
        [XmlElement("TN")]
        [ProtoMember(2)]
        public Guid TransactionNumber;

        /// <summary>
        /// Получатель телеграммы
        /// </summary>
        [XmlElement("TID")]
        [ProtoMember(3)]
        public ToId ToId;

        /// <summary>
        /// Отправитель телеграммы
        /// </summary>
        [XmlElement("FID")]
        [ProtoMember(4)]
        public ToId FromId;

        /// <summary>
        /// Тип телеграммы
        /// </summary>
        [XmlElement("Type")]
        [ProtoMember(5)]
        public TelegramType Type;

        /// <summary>
        /// Подпись пользователя
        /// NOTE: подумать хватит ли нам одного string, или лучше вынести в структуру
        /// </summary>
        [XmlElement("UserInfo")]
        [ProtoMember(6)]
        public string UserInfo;

        [XmlElement("UnitOfWork")]
        [ProtoMember(7)]
        public Guid UnitOfWork;

        /// <summary>
        /// Содержимое телеграммы - корень древовидной структуры
        /// </summary>
        [XmlElement("Content")]
        [ProtoMember(8)]
        public Node Content;

        [XmlElement("TimeOut")]
        [ProtoMember(9)]
        public int? TimeOut;

        /// <summary>
        /// Время выполнения последнего запроса.
        /// </summary>
        [ProtoMember(10)]
        public double LastQueryExecutionTime;

        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public Telegram()
        {
            Type = TelegramType.Query;
            Date = DateTime.Now;
            Content = new Node();
        }

        public Telegram(TelegramType type = TelegramType.Query)
        {
            Type = type;
            Date = DateTime.Now;
            Content = new Node();
        }

        /// <summary>
        /// Форматирует ID
        /// </summary>
        /// <param name="serviceId"></param>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public static string Key(string serviceId, string sessionId)
        {
            return string.Format("{0}_{1}", serviceId, sessionId);
        }

        public static string Key(string serviceId, int sessionId)
        {
            return string.Format("{0}_{1}", serviceId, sessionId);
        }
    }
}

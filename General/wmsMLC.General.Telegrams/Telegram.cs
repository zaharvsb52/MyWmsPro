using System;
using System.Xml.Serialization;
using ProtoBuf;
using wmsMLC.General.Types;

namespace wmsMLC.General.Telegrams
{
    /// <summary>
    /// Телеграмма для передачи и обмена данными
    /// </summary>
    [XmlRoot("Telegram")]
    [Serializable]
    [ProtoContract]
    public class Telegram
    {
        public const TelegramType DefaultTelegramType = TelegramType.Query;
        static Int32 _serial;

        public Telegram()
        {
            Content = new Node();
            Type = DefaultTelegramType;
        }
        
        #region .  Properties  .

        /// <summary>
        /// Дата и время создания телеграммы
        /// </summary>
        [XmlElement("Date")]
        [ProtoMember(1)]
        public DateTime Date { get; set; }

        /// <summary>
        /// Идентификатор транзакции (если мы захотим объединить обработку нескольких телеграмм)
        /// </summary>
        [XmlElement("TN")]
        [ProtoMember(2)]
        public string TransactionNumber { get; set; }

        /// <summary>
        /// Получатель телеграммы
        /// </summary>
        [XmlElement("TID")]
        [ProtoMember(3)]
        public ToId ToId { get; set; }

        /// <summary>
        /// Отправитель телеграммы
        /// </summary>
        [XmlElement("FID")]
        [ProtoMember(4)]
        public ToId FromId { get; set; }

        /// <summary>
        /// Тип телеграммы
        /// </summary>
        [XmlElement("Type")]
        [ProtoMember(5)]
        public TelegramType Type { get; set; }

        /// <summary>
        /// Содержимое телеграммы - корень древовидной структуры
        /// </summary>
        [XmlElement("Content")]
        [ProtoMember(6)]
        public Node Content { get; set; }

        public string FromKey
        {
            get { return FromId.Key; }
        }

        public string ToKey
        {
            get { return ToId.Key; }
        }

        #endregion

        #region .  Helpers  .
        
        /// <summary>
        /// Новый серийный номер
        /// </summary>
        /// <returns>серийный номер</returns>
        public static int NewSn
        {
            get
            {
                _serial++;
                if (_serial >= Int32.MaxValue - 2) { _serial = 0; }
                return _serial;
            }
        }

        /// <summary>
        /// Последний серийный номер
        /// </summary>
        public static int LastSN
        {
            get { return _serial; }
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

        #endregion
    }
}

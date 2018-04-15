using System;
using System.Xml.Serialization;
using ProtoBuf;

namespace wmsMLC.General.Types
{
    [XmlRoot("SocketMessageTemplate")]
    [Serializable]
    [ProtoContract]
    public class SocketMessageTemplate
    {
        /// <summary>
        /// Действия выполняемые на сервере, за исключением типа Answer - ответ.
        /// </summary>
        [XmlElement("Action")]
        [ProtoMember(1)]
        public int Action { get; set; }
        
        /// <summary>
        /// Возвращаемое значение / параметры.
        /// </summary>
        [XmlElement("Result")]
        [ProtoMember(2)]
        public byte[] Result { get; set; }

        [XmlElement("ServiceName")]
        [ProtoMember(3)]
        public string ServiceName { get; set; }

        public enum ActionType
        {
            None = 0,
            Answer = 1,
            Connect = 2,
            SendTelegram = 3,
            Test = 4,
        }
    }
}

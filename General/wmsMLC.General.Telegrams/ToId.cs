using System;
using System.Xml.Serialization;
using ProtoBuf;

namespace wmsMLC.General.Telegrams
{
    /// <summary> Идентификатор телеграммы </summary>
    [Serializable]
    [ProtoContract]
    public struct ToId
    {
        /// <summary> ID сервиса (вкомпилен) </summary>
        [XmlElement("SID")]
        [ProtoMember(1)]
        public string ServiceId { get; set; }

        /// <summary> ID сессии (выдаётся каждый раз при подключении) </summary>
        [XmlElement("Session")]
        [ProtoMember(2)]
        public int SessionId { get; set; }

        /// <summary> номер телеграммы </summary>
        [XmlElement("SN")]
        [ProtoMember(3)]
        public int SerialNumber { get; set; }

        override public string ToString()
        {
            return string.Format("{0}:{1}:{2}", ServiceId, SessionId, SerialNumber);
        }

        public string Key
        {
            get
            {
                return string.Format("{0}_{1}", ServiceId, SessionId);
            }
        }
    }
}
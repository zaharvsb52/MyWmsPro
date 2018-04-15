using System.Xml.Serialization;

namespace wmsMLC.General.Services
{
    public class SdclConnectInfo
    {
        [XmlElement("Code")]
        public string Code;
        [XmlElement("Endpoint")]
        public string Endpoint;
    }

    public interface ISdclConnectInfoProvider
    {
        SdclConnectInfo GetSdclConnectInfo(string clientCode, SdclConnectInfo lastSdclConnectInfo);
    }
}

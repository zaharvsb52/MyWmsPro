using System.Xml.Serialization;

namespace wmsMLC.Business.Objects
{
    [XmlRoot("TEventParam")]
    public class TEventParam : WMSBusinessObject
    {
        public const string ParamNamePropertyName = "PARAMNAME";
        public const string ParamValuePropertyName = "PARAMVALUE";

        public TEventParam() {}
        public TEventParam(string paramName, object paramValue)
        {
            SetProperty(ParamNamePropertyName, paramName);
            SetProperty(ParamValuePropertyName, paramValue);
        }
    }
}
using System.IO;
using System.Xml.Serialization;
using wmsMLC.APS.wmsSI;
using wmsMLC.APS.wmsSI.Wrappers;

namespace wmsMLC.Tests.Processes
{
    public class YExternalTrafficLoadTest : SiBaseTest
    {
        public override void DoTest()
        {
            var si = new IntegrationService();
            var item = new YExternalTrafficWrapper();
            var path = @"Data\YExternalTrafficWrapper.xml";
            var serialiser = new XmlSerializer(typeof(YExternalTrafficWrapper));
            using (TextReader reader = new StreamReader(path))
            {
                item = (YExternalTrafficWrapper)serialiser.Deserialize(reader);
                reader.Close();
            }
            
            si.YExternalTrafficLoad(item); 
        }
    }
}

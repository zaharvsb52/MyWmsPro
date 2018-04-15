using System.IO;
using System.Xml.Serialization;
using NUnit.Framework;
using wmsMLC.APS.wmsSI;
using wmsMLC.APS.wmsSI.Wrappers;

namespace wmsMLC.Tests.Processes
{
    [TestFixture]
    public class PartnerLoadTest : SiBaseTest
    {
        [Test]
        public override void DoTest()
        {
            //double el = 0.5;
            //var roundel1 = Math.Round(el, 2, MidpointRounding.AwayFromZero);
            //var roundel2 = Math.Round(el, 2, MidpointRounding.ToEven);

            var si = new IntegrationService();
            var item = new PartnerWrapper();
            var path = @"Data\PartnerWrapper.xml";
            var serialiser = new XmlSerializer(typeof(PartnerWrapper));
            using (TextReader reader = new StreamReader(path))
            {
                item = (PartnerWrapper)serialiser.Deserialize(reader);
                reader.Close();
            }
            
            var result = si.PartnerLoad(item);
        }
    }
}

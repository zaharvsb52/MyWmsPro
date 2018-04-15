using System.IO;
using System.Xml.Serialization;
using NUnit.Framework;
using wmsMLC.APS.wmsSI;
using wmsMLC.APS.wmsSI.Wrappers;

namespace wmsMLC.Tests.Processes
{
    [TestFixture]
    public class LabelWrapperTest : SiBaseTest
    {
        [Test]
        public override void DoTest()
        {
            var si = new IntegrationService();
            LabelWrapper[] item;
            var path = @"Data\LabelWrapper.xml";
            var serialiser = new XmlSerializer(typeof(LabelWrapper[]));
            using (TextReader reader = new StreamReader(path))
            {
                item = (LabelWrapper[]) serialiser.Deserialize(reader);
                reader.Close();
            }

            var results = si.LabelsLoad(item);
        }
    }
}

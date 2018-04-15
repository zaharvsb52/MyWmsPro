using System.IO;
using System.Xml.Serialization;
using NUnit.Framework;
using wmsMLC.APS.wmsSI;
using wmsMLC.APS.wmsSI.Wrappers;

namespace wmsMLC.Tests.Processes
{
    public class ShipmentLoadTest : SiBaseTest
    {
        [Test]
        public override void DoTest()
        {
            var si = new IntegrationService();
            SalesInvoiceWrapper item;
            var path = @"Data\SalesInvoiceWrapper.xml";
            //var path = @"Data\SalesInvoiceWrapper_OWBBOXRESERVE.xml";
            //var path = @"Data\SalesInvoiceWrapper_SpeedTest.xml";
            //var path = @"Data\SalesInvoiceWrapper_UPDATEEXISTCPV.xml";
            var serialiser = new XmlSerializer(typeof(SalesInvoiceWrapper));
            using (TextReader reader = new StreamReader(path))
            {
                item = (SalesInvoiceWrapper)serialiser.Deserialize(reader);
                reader.Close();
            }

            //var startTime = DateTime.Now;
            var result = si.ShipmentLoad(item);
            //var endresult = string.Format("Загружено за {0}", DateTime.Now - startTime);
        }
    }
}

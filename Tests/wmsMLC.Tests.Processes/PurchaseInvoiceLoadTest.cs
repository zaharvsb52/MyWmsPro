using System.IO;
using System.Xml.Serialization;
using NUnit.Framework;
using wmsMLC.APS.wmsSI;
using wmsMLC.APS.wmsSI.Wrappers;

namespace wmsMLC.Tests.Processes
{
    [TestFixture]
    public class PurchaseInvoiceLoadTest : SiBaseTest
    {
        [Test]
        public override void DoTest()
        {
            var si = new IntegrationService();
            PurchaseInvoiceWrapper item;
            var path = @"Data\PurchaseInvoiceWrapper.xml";
            var serialiser = new XmlSerializer(typeof(PurchaseInvoiceWrapper));
            using (TextReader reader = new StreamReader(path))
            {
                item = (PurchaseInvoiceWrapper)serialiser.Deserialize(reader);
                reader.Close();
            }
            
            var results = si.ReceiptLoad(item);
        }
    }
}

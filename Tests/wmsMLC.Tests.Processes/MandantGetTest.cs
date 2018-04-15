using NUnit.Framework;
using wmsMLC.APS.wmsSI;

namespace wmsMLC.Tests.Processes
{
    [TestFixture]
    public class MandantGetTest : SiBaseTest
    {
        public override void DoTest()
        {
            var si = new IntegrationService();
            var result = si.MandantGet();
        }
    }
}

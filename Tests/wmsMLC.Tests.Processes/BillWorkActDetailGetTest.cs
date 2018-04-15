using NUnit.Framework;
using wmsMLC.APS.wmsSI;

namespace wmsMLC.Tests.Processes
{
    [TestFixture]
    public class BillWorkActDetailGetTest : SiBaseTest
    {
        public override void DoTest()
        {
            var si = new IntegrationService();
            var workAct = si.BillWorkActDetailGet("");
        }
    }
}

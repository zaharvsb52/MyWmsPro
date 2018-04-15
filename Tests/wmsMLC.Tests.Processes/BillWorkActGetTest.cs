using System;
using NUnit.Framework;
using wmsMLC.APS.wmsSI;
using wmsMLC.General;

namespace wmsMLC.Tests.Processes
{
    [TestFixture]
    public class BillWorkActGetTest : SiBaseTest
    {
        public override void DoTest()
        {
            var si = new IntegrationService();
            var workAct1 = si.BillWorkActGet("");

            try
            {
                var workAct2 = si.BillWorkActGet("");
                throw new DeveloperException("dateFrom > dateTill");
            }
            catch(Exception ex)
            {

            }
        }
    }
}

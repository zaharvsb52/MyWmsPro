using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class BillBiller2MandantTest : BaseWMSObjectTest<BillBiller2Mandant>
    {
        private readonly BillBillerTest _billBillerTest = new BillBillerTest();
        private readonly MandantTest _mandantTest = new MandantTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _billBillerTest, _mandantTest };
        }

        protected override void FillRequiredFields(BillBiller2Mandant obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().BILLER2MANDANTID = TestDecimal;
            obj.AsDynamic().BILLBILLER2MANDANTPARTNERID = _mandantTest.CreateNew().GetKey();
            obj.AsDynamic().BILLBILLER2MANDANTBILLERCODE = _billBillerTest.CreateNew().GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(BILLER2MANDANTID = '{0}')", TestDecimal);
        }
    }
}
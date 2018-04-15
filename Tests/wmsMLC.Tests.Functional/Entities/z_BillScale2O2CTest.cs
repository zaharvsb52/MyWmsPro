using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class BillScale2O2CTest : BaseWMSObjectTest<BillScale2O2C>
    {
        private readonly BillScaleTest _billScaleTest = new BillScaleTest();
        private readonly BillOperation2ContractTest _billOperation2ContractTest = new BillOperation2ContractTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _billScaleTest, _billOperation2ContractTest };
        }

        protected override void FillRequiredFields(BillScale2O2C obj)
        {
            base.FillRequiredFields(obj);

            var scale = _billScaleTest.CreateNew();
            var o2c = _billOperation2ContractTest.CreateNew();

            obj.AsDynamic().SCALE2O2CID = TestDecimal;
            obj.AsDynamic().BILLSCALE2O2CSCALECODE = scale.GetKey();
            obj.AsDynamic().BILLSCALE2O2COPERATION2CONTRACTID = o2c.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(SCALE2O2CID = '{0}')", TestDecimal);
        }
    }
}
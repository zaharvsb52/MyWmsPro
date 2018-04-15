using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class Res2SupplyChainTest : BaseWMSObjectTest<Res2SupplyChain>
    {
        private readonly ResTest _resTest = new ResTest();
        private readonly SupplyChainTest _supplyChainTest = new SupplyChainTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _resTest, _supplyChainTest };
        }


        protected override void FillRequiredFields(Res2SupplyChain obj)
        {
            base.FillRequiredFields(obj);

            var res = _resTest.CreateNew();
            _supplyChainTest.TestString = TestString + "9";
            _supplyChainTest.TestDecimal = TestDecimal + 9;
            var sc = _supplyChainTest.CreateNew();

            obj.AsDynamic().RES2SUPPLYCHAINID = TestDecimal;
            obj.AsDynamic().RES2SUPPLYCHAINRESID = res.GetKey();
            obj.AsDynamic().RES2SUPPLYCHAINSUPPLYCHAINID = sc.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(RES2SUPPLYCHAINID = '{0}')", TestDecimal);
        }
    }
}
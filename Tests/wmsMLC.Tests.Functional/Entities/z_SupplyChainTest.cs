using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class SupplyChainTest : BaseWMSObjectTest<SupplyChain>
    {
        private readonly MSCTest _mscTest = new MSCTest();
        private readonly SupplyAreaTest _supplyAreaTest = new SupplyAreaTest();
        private readonly BillOperationTest _billOperationTest = new BillOperationTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _mscTest, _supplyAreaTest, _billOperationTest };
        }


        protected override void FillRequiredFields(SupplyChain obj)
        {
            base.FillRequiredFields(obj);

            var msc = _mscTest.CreateNew();
            var sa = _supplyAreaTest.CreateNew();
            var oper = _billOperationTest.CreateNew();

            obj.AsDynamic().SUPPLYCHAINID = TestDecimal;
            obj.AsDynamic().MSCCODE_R = msc.GetKey();
            obj.AsDynamic().SUPPLYCHAINSOURCESUPPLYAREA = sa.GetKey();
            obj.AsDynamic().SUPPLYCHAINTARGETSUPPLYAREA = sa.GetKey();
            obj.AsDynamic().OPERATIONCODE_R = oper.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(SUPPLYCHAINID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(SupplyChain obj)
        {
            obj.AsDynamic().SUPPLYCHAINREQUIREDOPERATION = TestString;
        }

        protected override void CheckSimpleChange(SupplyChain source, SupplyChain dest)
        {
            string sourceName = source.AsDynamic().SUPPLYCHAINREQUIREDOPERATION;
            string destName = dest.AsDynamic().SUPPLYCHAINREQUIREDOPERATION;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}
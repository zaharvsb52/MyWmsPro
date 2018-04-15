using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class BillOperationCauseTest : BaseWMSObjectTest<BillOperationCause>
    {
        private readonly BillOperation2ContractTest _billOperation2ContractTest = new BillOperation2ContractTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _billOperation2ContractTest };
        }

        protected override void FillRequiredFields(BillOperationCause obj)
        {
            base.FillRequiredFields(obj);

            var o2c = _billOperation2ContractTest.CreateNew();

            obj.AsDynamic().OPERATIONCAUSEID = TestDecimal;
            obj.AsDynamic().OPERATION2CONTRACTID_R = o2c.GetKey();
            obj.AsDynamic().OPERATIONCAUSENAME = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(OPERATIONCAUSEID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(BillOperationCause obj)
        {
            obj.AsDynamic().OPERATIONCAUSEDESC = TestString;
        }

        protected override void CheckSimpleChange(BillOperationCause source, BillOperationCause dest)
        {
            string sourceName = source.AsDynamic().OPERATIONCAUSEDESC;
            string destName = dest.AsDynamic().OPERATIONCAUSEDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}
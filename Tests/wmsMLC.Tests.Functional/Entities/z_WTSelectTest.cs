using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class WTSelectTest : BaseWMSObjectTest<WTSelect>
    {
        private readonly MandantTest _mandantTest = new MandantTest();
        private readonly BillOperationTest _billOperationTest = new BillOperationTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _mandantTest, _billOperationTest };
        }

        protected override void FillRequiredFields(WTSelect obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().WTSELECTID = TestDecimal;
            obj.AsDynamic().PARTNERID_R = _mandantTest.CreateNew().GetKey();
            obj.AsDynamic().OPERATIONCODE_R = _billOperationTest.CreateNew().GetKey();
            obj.AsDynamic().WTSELECTDOC = true;
            obj.AsDynamic().WTSELECTVALUE = true;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(WTSELECTID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(WTSelect obj)
        {
            obj.AsDynamic().WTSELECTVALUETYPE = TestString;
        }

        protected override void CheckSimpleChange(WTSelect source, WTSelect dest)
        {
            string sourceName = source.AsDynamic().WTSELECTVALUETYPE;
            string destName = dest.AsDynamic().WTSELECTVALUETYPE;
            sourceName.ShouldBeEquivalentTo(destName);
        }

    }
}
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class BillOperationTest : BaseWMSObjectTest<BillOperation>
    {
        private readonly BillOperationClassTest _billOperationClassTest = new BillOperationClassTest();

        protected override void FillRequiredFields(BillOperation obj)
        {
            base.FillRequiredFields(obj);

            var boc = _billOperationClassTest.CreateNew();
            // Проверить
            obj.AsDynamic().OPERATIONCODE = TestString;
            obj.AsDynamic().OPERATIONNAME = TestString;
            obj.AsDynamic().OPERATIONCLASSCODE_R = boc.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(OPERATIONCODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(BillOperation obj)
        {
            obj.AsDynamic().OPERATIONDESC = TestString;
        }

        protected override void CheckSimpleChange(BillOperation source, BillOperation dest)
        {
            string sourceName = source.AsDynamic().OPERATIONDESC;
            string destName = dest.AsDynamic().OPERATIONDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _billOperationClassTest };
        }
    }
}
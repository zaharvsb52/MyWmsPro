using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class BillOperationClassTest : BaseWMSObjectTest<BillOperationClass>
    {
        protected override void FillRequiredFields(BillOperationClass obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().OPERATIONCLASSCODE = TestString;
            obj.AsDynamic().OPERATIONCLASSNAME = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(OPERATIONCLASSCODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(BillOperationClass obj)
        {
            obj.AsDynamic().OPERATIONCLASSDESC = TestString;
        }

        protected override void CheckSimpleChange(BillOperationClass source, BillOperationClass dest)
        {
            string sourceName = source.AsDynamic().OPERATIONCLASSDESC;
            string destName = dest.AsDynamic().OPERATIONCLASSDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}
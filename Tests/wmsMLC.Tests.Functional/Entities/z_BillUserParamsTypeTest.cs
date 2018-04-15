using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class BillUserParamsTypeTest : BaseWMSObjectTest<BillUserParamsType>
    {
        protected override void FillRequiredFields(BillUserParamsType obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().USERPARAMSTYPECODE = TestString;
            obj.AsDynamic().USERPARAMSTYPENAME = TestString;
            obj.AsDynamic().USERPARAMSTYPERANGETYPE = TestString;
            obj.AsDynamic().USERPARAMSTYPERANGEDATATYPE = TestDecimal;
            obj.AsDynamic().USERPARAMSTYPEVALUEDATATYPE = TestDecimal;
            obj.AsDynamic().USERPARAMSTYPEUSINGOPTION = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(USERPARAMSTYPECODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(BillUserParamsType obj)
        {
            obj.AsDynamic().USERPARAMSTYPEDESC = TestString;
        }

        protected override void CheckSimpleChange(BillUserParamsType source, BillUserParamsType dest)
        {
            string sourceName = source.AsDynamic().USERPARAMSTYPEDESC;
            string destName = dest.AsDynamic().USERPARAMSTYPEDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}
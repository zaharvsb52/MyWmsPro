using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class BillUserParamsTest : BaseWMSObjectTest<BillUserParams>
    {
        private readonly BillUserParamsTypeTest _billUserParamsTypeTest = new BillUserParamsTypeTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _billUserParamsTypeTest };
        }

        protected override void FillRequiredFields(BillUserParams obj)
        {
            base.FillRequiredFields(obj);

            var type = _billUserParamsTypeTest.CreateNew();

            obj.AsDynamic().USERPARAMSCODE = TestString;
            obj.AsDynamic().USERPARAMSNAME = TestString;
            obj.AsDynamic().USERPARAMSTYPECODE_R = type.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(USERPARAMSTYPECODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(BillUserParams obj)
        {
            obj.AsDynamic().USERPARAMSDESC = TestString;
        }

        protected override void CheckSimpleChange(BillUserParams source, BillUserParams dest)
        {
            string sourceName = source.AsDynamic().USERPARAMSDESC;
            string destName = dest.AsDynamic().USERPARAMSDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}
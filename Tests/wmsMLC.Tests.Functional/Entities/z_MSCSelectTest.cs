using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class MSCSelectTest : BaseWMSObjectTest<MSCSelect>
    {
        private readonly MSCTest _mscTest = new MSCTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _mscTest };
        }

        protected override void FillRequiredFields(MSCSelect obj)
        {
            base.FillRequiredFields(obj);

            var msc = _mscTest.CreateNew();

            obj.AsDynamic().MSCSELECTID = TestDecimal;
            obj.AsDynamic().MSCCODE_R = msc.GetKey();
            obj.AsDynamic().PRIORITY = TestDecimal;
            obj.AsDynamic().MSCSELECTTECOMPLETEMIN = TestDecimal;
            obj.AsDynamic().MSCSELECTTECOMPLETEOWBGROUP = false;
            obj.AsDynamic().MSCSELECTISBASE = false;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(MSCSELECTID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(MSCSelect obj)
        {
            obj.AsDynamic().MSCSELECTTECOMPLETEMAX = TestDecimal;
        }

        protected override void CheckSimpleChange(MSCSelect source, MSCSelect dest)
        {
            decimal sourceName = source.AsDynamic().MSCSELECTTECOMPLETEMAX;
            decimal destName = dest.AsDynamic().MSCSELECTTECOMPLETEMAX;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}
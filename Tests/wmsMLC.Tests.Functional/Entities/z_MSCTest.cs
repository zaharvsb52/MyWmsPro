using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class MSCTest : BaseWMSObjectTest<MSC>
    {
        private readonly MMTest _mmTest = new MMTest();
        private readonly MSCTypeTest _mscTypeTest = new MSCTypeTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _mmTest, _mscTypeTest };
        }
        
        protected override void FillRequiredFields(MSC obj)
        {
            base.FillRequiredFields(obj);

            var mm = _mmTest.CreateNew();
            var msct = _mscTypeTest.CreateNew();

            obj.AsDynamic().MSCCODE = TestString;
            obj.AsDynamic().MSCNAME = TestString;
            obj.AsDynamic().MSCSTRATEGY = TestString;
            obj.AsDynamic().MSCTARGETSUPPLYAREA = TestString;
            obj.AsDynamic().MMCODE_R = mm.GetKey();
            obj.AsDynamic().MSCOPERATIONORDER = TestDecimal;
            obj.AsDynamic().MSCTYPECODE_R = msct.GetKey();
            
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(MSCCODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(MSC obj)
        {
            obj.AsDynamic().MSCDESC = TestString;
        }

        protected override void CheckSimpleChange(MSC source, MSC dest)
        {
            string sourceName = source.AsDynamic().MSCDESC;
            string destName = dest.AsDynamic().MSCDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class PattTWhereSectionTest : BaseWMSObjectTest<PattTWhereSection>
    {
        private readonly PattTDataSourceTest _pattTDataSourceTest = new PattTDataSourceTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _pattTDataSourceTest };
        }

        protected override void FillRequiredFields(PattTWhereSection obj)
        {
            base.FillRequiredFields(obj);

            var ds = _pattTDataSourceTest.CreateNew();

            obj.AsDynamic().TEMPLATEWHERESECTIONID = TestDecimal;
            obj.AsDynamic().TEMPLATEDATASOURCECODE_R = ds.GetKey();
            obj.AsDynamic().TEMPLATEWHERESECTIONNAME = TestString;
            obj.AsDynamic().TEMPLATEWHERESECTIONCODE = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(TEMPLATEWHERESECTIONID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(PattTWhereSection obj)
        {
            obj.AsDynamic().TEMPLATEWHERESECTIONDESC = TestString;
        }

        protected override void CheckSimpleChange(PattTWhereSection source, PattTWhereSection dest)
        {
            string sourceName = source.AsDynamic().TEMPLATEWHERESECTIONDESC;
            string destName = dest.AsDynamic().TEMPLATEWHERESECTIONDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}
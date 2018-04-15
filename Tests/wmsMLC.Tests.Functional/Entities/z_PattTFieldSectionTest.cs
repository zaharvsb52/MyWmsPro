using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class PattTFieldSectionTest : BaseWMSObjectTest<PattTFieldSection>
    {
        private readonly PattTDataSourceTest _pattTDataSourceTest = new PattTDataSourceTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _pattTDataSourceTest };
        }

        protected override void FillRequiredFields(PattTFieldSection obj)
        {
            base.FillRequiredFields(obj);

            var ds = _pattTDataSourceTest.CreateNew();

            obj.AsDynamic().TEMPLATEFIELDSECTIONID = TestDecimal;
            obj.AsDynamic().TEMPLATEDATASOURCECODE_R = ds.GetKey();
            obj.AsDynamic().TEMPLATEFIELDSECTIONNAME = TestString;
            obj.AsDynamic().TEMPLATEFIELDSECTIONCODE = TestString;
            obj.AsDynamic().TEMPLATEFIELDSECTIONRESULT = false;
            obj.AsDynamic().TEMPLATEFIELDSECTIONDETERM = false;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(TEMPLATEFIELDSECTIONID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(PattTFieldSection obj)
        {
            obj.AsDynamic().TEMPLATEFIELDSECTIONDESC = TestString;
        }

        protected override void CheckSimpleChange(PattTFieldSection source, PattTFieldSection dest)
        {
            string sourceName = source.AsDynamic().TEMPLATEFIELDSECTIONDESC;
            string destName = dest.AsDynamic().TEMPLATEFIELDSECTIONDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}
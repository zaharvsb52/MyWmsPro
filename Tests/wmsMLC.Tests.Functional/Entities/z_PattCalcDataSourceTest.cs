using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class PattCalcDataSourceTest : BaseWMSObjectTest<PattCalcDataSource>
    {
        private readonly PattTDataSourceTest _pattTDataSourceTest = new PattTDataSourceTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _pattTDataSourceTest };
        }

        protected override void FillRequiredFields(PattCalcDataSource obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().CALCDATASOURCECODE = TestString;
            obj.AsDynamic().CALCDATASOURCENAME = TestString;
            obj.AsDynamic().TEMPLATEDATASOURCECODE_R = _pattTDataSourceTest.CreateNew().GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(CALCDATASOURCECODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(PattCalcDataSource obj)
        {
            obj.AsDynamic().CALCDATASOURCEDESC = TestString;
        }

        protected override void CheckSimpleChange(PattCalcDataSource source, PattCalcDataSource dest)
        {
            string sourceName = source.AsDynamic().CALCDATASOURCEDESC;
            string destName = dest.AsDynamic().CALCDATASOURCEDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}
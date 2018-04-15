using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class PattTDataSourceTest : BaseWMSObjectTest<PattTDataSource>
    {
        private readonly SysEnumTest _sysEnumTest = new SysEnumTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _sysEnumTest };
        }

        protected override void FillRequaredFields(PattTDataSource obj)
        {
            base.FillRequaredFields(obj);

            var se = _sysEnumTest.CreateNew();

            obj.AsDynamic().TEMPLATEDATASOURCECODE = TestString;
            obj.AsDynamic().TEMPLATEDATASOURCENAME = TestString;
            obj.AsDynamic().TEMPLATEDATASOURCETYPE = se.GetKey();
            //obj.AsDynamic().TEMPLATEDATASOURCEBODY = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(TEMPLATEDATASOURCECODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(PattTDataSource obj)
        {
            obj.AsDynamic().TEMPLATEDATASOURCEDESC = TestString;
        }

        protected override void CheckSimpleChange(PattTDataSource source, PattTDataSource dest)
        {
            string sourceName = source.AsDynamic().TEMPLATEDATASOURCEDESC;
            string destName = dest.AsDynamic().TEMPLATEDATASOURCEDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}
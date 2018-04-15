using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class MgRouteSelectTest : BaseWMSObjectTest<MgRouteSelect>
    {
        private readonly MgRouteTest _mgRouteTest = new MgRouteTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _mgRouteTest };
        }

        protected override void FillRequiredFields(MgRouteSelect obj)
        {
            base.FillRequiredFields(obj);

            var mgRoute = _mgRouteTest.CreateNew();

            obj.AsDynamic().MGROUTESELECTID = TestDecimal;
            obj.AsDynamic().PRIORITY = TestDecimal;
            obj.AsDynamic().MGROUTEID_R = mgRoute.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(MGROUTESELECTID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(MgRouteSelect obj)
        {
            obj.AsDynamic().MGROUTESELECTREGION = TestString;
        }

        protected override void CheckSimpleChange(MgRouteSelect source, MgRouteSelect dest)
        {
            string sourceName = source.AsDynamic().MGROUTESELECTREGION;
            string destName = dest.AsDynamic().MGROUTESELECTREGION;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}
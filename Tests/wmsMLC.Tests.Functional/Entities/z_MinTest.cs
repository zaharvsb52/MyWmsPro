using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class MinTest : BaseWMSObjectTest<Min>
    {
        private readonly MandantTest _mandantTest = new MandantTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _mandantTest };
        }

        protected override void FillRequiredFields(Min obj)
        {
            base.FillRequiredFields(obj);

            var mandant = _mandantTest.CreateNew();

            obj.AsDynamic().MINID = TestDecimal;
            obj.AsDynamic().MINNAME = TestString;
            obj.AsDynamic().PARTNERID_R = mandant.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(MINID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(Min obj)
        {
            obj.AsDynamic().MINDESC = TestString;
        }

        protected override void CheckSimpleChange(Min source, Min dest)
        {
            string sourceName = source.AsDynamic().MINDESC;
            string destName = dest.AsDynamic().MINDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}
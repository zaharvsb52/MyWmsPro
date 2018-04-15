using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class TransitTest : BaseWMSObjectTest<Transit>
    {
        private readonly MandantTest _mandantTest = new MandantTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] {_mandantTest};
        }

        protected override void FillRequiredFields(Transit obj)
        {
            base.FillRequiredFields(obj);

            //var mandant = _mandantTest.CreateNew();

            obj.AsDynamic().TRANSITID = TestDecimal;
            obj.AsDynamic().TRANSITNAME = TestString;
            obj.AsDynamic().MANDANTID = 1;
            obj.AsDynamic().TRANSIT2ENTITY = "WAREHOUSE";
            obj.AsDynamic().TRANSITV2GUI = false;
            obj.AsDynamic().TRANSITHOSTREF = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(TRANSITID = {0})", TestDecimal);
        }

        protected override void MakeSimpleChange(Transit obj)
        {
            obj.AsDynamic().TRANSITDESC = TestString;
        }

        protected override void CheckSimpleChange(Transit source, Transit dest)
        {
            string sourceName = source.AsDynamic().TRANSITDESC;
            string destName = dest.AsDynamic().TRANSITDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class FactoryTest : BaseWMSObjectTest<Factory>
    {
        public FactoryTest()
        {
            TestString = "ABC";
        }

        private readonly MandantTest _mandantTest = new MandantTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _mandantTest };
        }


        protected override void FillRequiredFields(Factory obj)
        {
            base.FillRequiredFields(obj);

            var m = _mandantTest.CreateNew();

            obj.AsDynamic().FACTORYID = TestDecimal;
            obj.AsDynamic().FACTORYCODE = TestString;
            obj.AsDynamic().FACTORYNAME = TestString;
            obj.AsDynamic().FACTORYHOSTREF = TestString;
            obj.AsDynamic().FACTORYBATCHCODE = TestString;
            obj.AsDynamic().PARTNERID_R = m.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(FACTORYID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(Factory obj)
        {
            obj.AsDynamic().FACTORYDESC = TestString;
        }

        protected override void CheckSimpleChange(Factory source, Factory dest)
        {
            string sourceName = source.AsDynamic().FACTORYDESC;
            string destName = dest.AsDynamic().FACTORYDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}
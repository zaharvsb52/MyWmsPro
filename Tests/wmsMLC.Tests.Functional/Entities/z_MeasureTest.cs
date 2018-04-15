using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class MeasureTest : BaseWMSObjectTest<Measure>
    {
        private readonly MeasureTypeTest _measureTypeTest = new MeasureTypeTest();

        protected override void FillRequiredFields(Measure obj)
        {
            base.FillRequiredFields(obj);

            var mt = _measureTypeTest.CreateNew();

            obj.AsDynamic().MEASURECODE = TestString;
            obj.AsDynamic().MEASUREFACTOR = TestDouble;
            obj.AsDynamic().MEASURETYPECODE_R = mt.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(MEASURECODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(Measure obj)
        {
            obj.AsDynamic().MEASURENAME = TestString;
        }

        protected override void CheckSimpleChange(Measure source, Measure dest)
        {
            string sourceName = source.AsDynamic().MEASURENAME;
            string destName = dest.AsDynamic().MEASURENAME;
            sourceName.ShouldBeEquivalentTo(destName);
        }

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _measureTypeTest };
        }
    }
}
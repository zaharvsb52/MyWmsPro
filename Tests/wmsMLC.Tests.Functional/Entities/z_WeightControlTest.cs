using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class WeightControlTest : BaseWMSObjectTest<WeightControl>
    {
        private readonly MeasureTest _measureTest = new MeasureTest();
        private readonly TETypeTest _teTypeTest = new TETypeTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _measureTest, _teTypeTest };
        }


        protected override void FillRequiredFields(WeightControl obj)
        {
            base.FillRequiredFields(obj);

            var measure = _measureTest.CreateNew();
            var tetype = _teTypeTest.CreateNew();

            obj.AsDynamic().WEIGHTCONTROLID = TestDecimal;
            obj.AsDynamic().MANDANTID = 1;
            obj.AsDynamic().PRIORITY = TestDecimal;
            obj.AsDynamic().WEIGHTCONTROLDEV = TestDouble;
            obj.AsDynamic().MEASURECODE_R = measure.GetKey();
            obj.AsDynamic().TETYPECODE_R = tetype.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(WEIGHTCONTROLID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(WeightControl obj)
        {
            obj.AsDynamic().WEIGHTCONTROLARTGROUP = TestString;
        }

        protected override void CheckSimpleChange(WeightControl source, WeightControl dest)
        {
            string sourceName = source.AsDynamic().WEIGHTCONTROLARTGROUP;
            string destName = dest.AsDynamic().WEIGHTCONTROLARTGROUP;
            sourceName.ShouldBeEquivalentTo(destName);
        }

    }
}
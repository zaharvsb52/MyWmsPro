using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class TransitDataTest : BaseWMSObjectTest<TransitData>
    {
        private readonly TransitTest _transitTest = new TransitTest();
        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _transitTest };
        }

        protected override void FillRequiredFields(TransitData obj)
        {
            base.FillRequiredFields(obj);

            var transit = _transitTest.CreateNew();

            obj.AsDynamic().TRANSITDATAID = TestDecimal;
            obj.AsDynamic().TRANSITID_R = transit.GetKey();
            obj.AsDynamic().TRANSITDATAKEY = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(TRANSITDATAID = {0})", TestDecimal);
        }

        protected override void MakeSimpleChange(TransitData obj)
        {
            obj.AsDynamic().TRANSITDATAVALUE = TestString;
        }

        protected override void CheckSimpleChange(TransitData source, TransitData dest)
        {
            string sourceName = source.AsDynamic().TRANSITDATAVALUE;
            string destName = dest.AsDynamic().TRANSITDATAVALUE;
            sourceName.ShouldBeEquivalentTo(destName);
        }

    }
}
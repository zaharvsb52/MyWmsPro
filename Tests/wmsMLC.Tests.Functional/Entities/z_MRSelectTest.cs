using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class MRSelectTest : BaseWMSObjectTest<MRSelect>
    {
        private readonly MRTest _mrTest = new MRTest();
        //private readonly MandantTest _mandantTest = new MandantTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _mrTest, /*, _mandantTest*/ };
        }

        protected override void FillRequiredFields(MRSelect obj)
        {
            base.FillRequiredFields(obj);

            var mr = _mrTest.CreateNew();
            //var mandant = _mandantTest.CreateNew();

            obj.AsDynamic().MRSELECTID = TestDecimal;
            obj.AsDynamic().MRCODE_R = mr.GetKey();
            obj.AsDynamic().PRIORITY = TestDecimal;
            obj.AsDynamic().MANDANTID = 1;// mandant.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(MRSELECTID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(MRSelect obj)
        {
            obj.AsDynamic().MRSELECTCALCSKU = true;
        }

        protected override void CheckSimpleChange(MRSelect source, MRSelect dest)
        {
            bool sourceName = source.AsDynamic().MRSELECTCALCSKU;
            bool destName = dest.AsDynamic().MRSELECTCALCSKU;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}